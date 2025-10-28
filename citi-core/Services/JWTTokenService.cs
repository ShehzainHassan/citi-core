using citi_core.Data;
using citi_core.Interfaces;
using citi_core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace citi_core.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IJWTRepository _jwtRepository;

        public JwtTokenService(IConfiguration configuration, IJWTRepository jwtRepository)
        {
            _configuration = configuration;
            _jwtRepository = jwtRepository;
        }

        public string GenerateAccessToken(User user)
        {
            var key = _configuration["Jwt:SecretKey"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "CitiBank.API",
                audience: "CitiBank.Mobile",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task SaveRefreshTokenAsync(Guid userId, string token, string ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress
            };

            await _jwtRepository.AddRefreshTokenAsync(refreshToken);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string token)
        {
            return await _jwtRepository.GetUserByRefreshTokenAsync(token);
        }

        public async Task RevokeRefreshTokenAsync(string token, string ipAddress, string? replacementToken = null)
        {
            var refreshToken = await _jwtRepository.GetRefreshTokenAsync(token);
            if (refreshToken == null) return;

            await _jwtRepository.RevokeRefreshTokenAsync(refreshToken, ipAddress, replacementToken);
        }
    }
}
