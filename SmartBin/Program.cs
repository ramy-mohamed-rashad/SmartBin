using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartBin.Models;
using SmartBin.ModelViews;
using SmartBin.Services;
using System.Text;

namespace SmartBin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer(); //  Moved to the correct place

            // Database Context
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

            // Application Services
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<IBinSectionService, BinSectionService>();
            builder.Services.AddScoped<IBinService, BinService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IBinRequestService, BinRequestService>();
            builder.Services.AddScoped<ICameraService, CameraService>();
            builder.Services.AddScoped<ICameraRequestService, CameraRequestService>();

            // Identity Setup
            builder.Services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<AppDbContext>();

            // Authentication & JWT Setup
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                };
            });

            // CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });

            // 2. Build the application
            var app = builder.Build();

            // ── Apply pending migrations automatically ────────────────────────
            try
            {
                using var migrScope = app.Services.CreateScope();
                var db = migrScope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Startup] Migration skipped (DB unreachable?): {ex.Message}");
            }

            // ── Seed Admin role + admin user ──────────────────────────────────
            try
            {
                using var scope = app.Services.CreateScope();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                if (!await roleManager.RoleExistsAsync("Admin"))
                    await roleManager.CreateAsync(new IdentityRole<int>("Admin"));

                if (!await roleManager.RoleExistsAsync("User"))
                    await roleManager.CreateAsync(new IdentityRole<int>("User"));

                const string adminEmail = "admin@gmail.com";
                const string adminPassword = "Aa12345678@";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new AppUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "System Admin",
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Startup] Seeding skipped (DB unreachable?): {ex.Message}");
            }

            // ─────────────────────────────────────────────────────────────────

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "SmartBin API v1");
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // 4. Run the application
           await app.RunAsync();
        }
    }
}