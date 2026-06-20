using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartBin.Models;
using SmartBin.ModelViews;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartBin.Services
{
    public interface IAuthService
    {
        Task<AuthResponseMV> RegisterAsync(RegisterMV model);
        Task<AuthResponseMV> LoginAsync(LoginMV model);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseMV> RegisterAsync(RegisterMV model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                throw new Exception("User already exists!");

            AppUser user = new AppUser
            {
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            return await LoginAsync(new LoginMV { Email = model.Email, Password = model.Password });
        }

        public async Task<AuthResponseMV> LoginAsync(LoginMV model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                throw new Exception("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.Contains("Admin") ? "Admin" : "User";

            var token = await GenerateJwtToken(user, roles);

            return new AuthResponseMV
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id.ToString(),
                Role = primaryRole
            };
        }

        private async Task<string> GenerateJwtToken(AppUser user, IList<string> roles)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims so [Authorize(Roles="Admin")] works
            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(7),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}