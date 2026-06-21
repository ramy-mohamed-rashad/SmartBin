using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp.PixelFormats;
using SmartBin.Models;
using SmartBin.ModelViews;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SmartBin.Services
{
    public interface ITransactionService
    {
        public Task<TransactionResultMV> AddTransactionAsync(TransactionInsertMV transaction, string token);
        public Task<List<TransactionResultMV>> GetAllTransaction(int BinId, AppUser User, int PageNumber, int PageSize);
    }

    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private static Lazy<InferenceSession> _onnxSession;

        public TransactionService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            if (_onnxSession == null)
            {
                string modelPath = Path.Combine(_environment.ContentRootPath, "AI_Models", "smart_bin_best_model.onnx");
                _onnxSession = new Lazy<InferenceSession>(() => new InferenceSession(modelPath));
            }
        }

        public async Task<TransactionResultMV> AddTransactionAsync(TransactionInsertMV transaction, string token)
        {
            var bin = await _context.Bins.FirstOrDefaultAsync(b => b.IdentificationToken == token);
            if (bin == null)
            {
                throw new Exception("Bin not found with the provided IdentificationToken.");
            }

            var newTransaction = new Transaction
            {
                BinId = bin.BinId,
                Timestamp = DateTime.UtcNow,
                //Weight = transaction.Weight
            };

            // Variable to hold the name so we don't get a NullReferenceException at the end
            string finalMaterialName = string.Empty;

            // IF WE HAVE AN IMAGE, USE THE AI
            if (transaction.Image != null)
            {
                // 1. Save the image to wwwroot/images/transactions
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "transactions");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(transaction.Image.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await transaction.Image.CopyToAsync(fileStream);
                }

                newTransaction.ImageUrl = $"/images/transactions/{uniqueFileName}";

                // 2. Open a read stream and send it to the AI Model
                using (var aiStream = transaction.Image.OpenReadStream())
                {
                    var aiResult = await AnalyzeImageAsync(aiStream);

                    // Weight Override Logic
                    //int glassMinimumWeightGrams = 100;
                    //bool isPredictedGlass = aiResult.MaterialName.Equals("Glass", StringComparison.OrdinalIgnoreCase);
                    //bool isPredictedPlastic = aiResult.MaterialName.Equals("Plastic", StringComparison.OrdinalIgnoreCase);
                    //Console.WriteLine(isPredictedGlass);
                    //if (isPredictedGlass && transaction.Weight < glassMinimumWeightGrams)
                    //{
                    //    // Override the AI prediction
                    //    aiResult.MaterialName = "Plastic";
                    //    aiResult.Confidence = 80;
                    //}
                    //if(isPredictedPlastic && transaction.Weight > glassMinimumWeightGrams)
                    //{
                    //    aiResult.MaterialName = "Glass";
                    //    aiResult.Confidence = 80;
                    //}

                    // 3. Find the material based on the AI's prediction
                    Material material = await _context.Materials.FirstOrDefaultAsync(m => m.Name == aiResult.MaterialName);
                    if (material == null)
                    {
                        throw new Exception($"AI predicted '{aiResult.MaterialName}', but it doesn't exist in the database.");
                    }

                    // 4. Update the transaction with the AI data
                    newTransaction.MaterialId = material.MaterialId;
                    newTransaction.AiConfidencePercentage = aiResult.Confidence;
                    finalMaterialName = material.Name; // Save name for the return model
                }
            }
            // IF NO IMAGE, RELY ON THE MANUAL MATERIAL NAME
            else
            {
                Material material = await _context.Materials.FirstOrDefaultAsync(m => m.Name == transaction.MaterialName);
                if (material == null)
                {
                    throw new Exception("Material not found with the provided MaterialName.");
                }

                newTransaction.AiConfidencePercentage = 0;
                newTransaction.MaterialId = material.MaterialId;
                finalMaterialName = material.Name; // Save name for the return model
            }

            // 5. Save everything to the database exactly once
            _context.Transactions.Add(newTransaction);
            await _context.SaveChangesAsync();

            // 6. Return the mapped view model
            var transactionResult = new TransactionResultMV
            {
                BinId = newTransaction.BinId,
                Timestamp = newTransaction.Timestamp, // Reuse the exact same timestamp 
                AiConfidencePercentage = newTransaction.AiConfidencePercentage,
                ImageUrl = newTransaction.ImageUrl,
                MaterialName = finalMaterialName, // Safely use the string variable here
                TransactionId = newTransaction.TransactionId
            };

            return transactionResult;
        }

        public async Task<(string MaterialName, float Confidence)> AnalyzeImageAsync(Stream imageStream)
        {
            var session = _onnxSession.Value;

            // NEW: Dynamically grab the exact input name the ONNX model expects!
            string inputName = session.InputMetadata.Keys.First();

            // Pre-process the image
            Tensor<float> imageTensor = PreprocessImage(imageStream);

            // Use the dynamic inputName here instead of hardcoding it
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, imageTensor) };
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            var output = results.First().AsEnumerable<float>().ToArray();

            int maxIndex = Array.IndexOf(output, output.Max());
            float confidence = output[maxIndex] * 100f;

            string materialName = GetMaterialNameFromIndex(maxIndex);

            return (materialName, confidence);
        }

        public async Task<List<TransactionResultMV>> GetAllTransaction(int BinId, AppUser User, int PageNumber, int PageSize)
        {
            var transactions = await _context.Transactions
                .Where(t => t.BinId == BinId && t.Bin.UserId == User.Id)
                .Include(t => t.Material)
                .OrderByDescending(t => t.Timestamp)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(t => new TransactionResultMV
                {
                    TransactionId = t.TransactionId,
                    Timestamp = t.Timestamp,
                    AiConfidencePercentage = t.AiConfidencePercentage,
                    ImageUrl = t.ImageUrl,
                    MaterialName = t.Material.Name
                })
                .ToListAsync();

            return transactions;
        }

        // --- HELPER METHODS YOU WILL NEED TO IMPLEMENT ---

        private Tensor<float> PreprocessImage(Stream imageStream)
        {
            // 1. Load the image and force it into RGB format
            using var image = Image.Load<Rgb24>(imageStream);

            // 2. Resize to 224x224 to match MobileNetV2 exactly
            image.Mutate(x => x.Resize(224, 224));

            // 3. Create a Tensor. Keras defaults to NHWC layout: [BatchSize, Height, Width, Channels]
            var tensor = new DenseTensor<float>(new[] { 1, 224, 224, 3 });

            // 4. Loop through every pixel
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image[x, y];

                    // FIX: Remove the math! 
                    // Just pass the raw float values (0.0 to 255.0) because your 
                    // Python model already has 'preprocess_input' built into its layers.
                    tensor[0, y, x, 0] = pixel.R; // Red channel
                    tensor[0, y, x, 1] = pixel.G; // Green channel
                    tensor[0, y, x, 2] = pixel.B; // Blue channel
                }
            }

            return tensor;
        }

        private string GetMaterialNameFromIndex(int index)
        {
            // UPDATED: Now matches the 5 folders from your MobileNetV3 dataset!
            string[] classNames = new[]
            {
                "glass",     // Index 0
                "metal",     // Index 1
                "organic",   // Index 2
                "paper",     // Index 3
                "plastic"    // Index 4
            };

            if (index >= 0 && index < classNames.Length)
            {
                return classNames[index];
            }

            throw new Exception($"AI returned an unknown index: {index}");
        }
    }
}