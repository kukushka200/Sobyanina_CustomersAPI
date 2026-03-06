using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using CustomersAPI.Models;

namespace CustomersAPI.Services
{
    /// <summary>
    /// Интерфейс для сервиса генерации JWT-токенов
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Генерирует JWT-токен для пользователя
        /// </summary>
        /// <param name="user">Пользователь, для которого генерируется токен</param>
        /// <returns>JWT-токен в виде строки</returns>
        string GenerateToken(AppUser user);
    }

    /// <summary>
    /// Сервис для генерации JWT-токенов
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
        }

        /// <inheritdoc />
        public string GenerateToken(AppUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Создаем claims (утверждения) для токена
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject - идентификатор пользователя
                new Claim(JwtRegisteredClaimNames.Email, user.Email), // Email пользователя
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Уникальный идентификатор токена
                new Claim("name", user.Name ?? ""), // Имя пользователя 
                new Claim("userId", user.Id.ToString()) // UserId 
            };

            // Создаем ключ для подписи токена
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            
            // Создаем учетные данные для подписи
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Создаем токен
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer, // Издатель токена
                audience: _jwtSettings.Audience, // Аудитория токена
                claims: claims, // Утверждения
                expires: DateTime.UtcNow.AddDays(_jwtSettings.ExpirationInDays), // Время истечения
                signingCredentials: credentials // Подпись
            );

            // Возвращаем токен в виде строки
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}