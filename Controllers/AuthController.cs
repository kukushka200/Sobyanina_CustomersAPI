using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomersAPI.Data;
using CustomersAPI.Models;
using CustomersAPI.Models.Auth;
using CustomersAPI.Services;

namespace CustomersAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;

        public AuthController(AppDbContext context, IJwtTokenService jwtTokenService, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var existingUser = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                //  Email должен быть уникальным - возвращаем ошибку
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }
      // Создаем нового пользователя с хешированным паролем
            var user = new AppUser
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Address = request.Address,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Регистрация успешна" });
        }
     // Вход пользователя в систему
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Неверный email или пароль" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Пользователь заблокирован" });
            }

            // Генерируем JWT-токен и используем срок действия из настроек JwtSettings
            var token = _jwtTokenService.GenerateToken(user);
            var expiration = DateTime.UtcNow.AddDays(_jwtSettings.ExpirationInDays);

            return Ok(new AuthResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token,
                Expiration = expiration
            });
        }
    }
}