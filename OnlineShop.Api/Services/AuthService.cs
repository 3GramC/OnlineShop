using OnlineShop.Api.Data;
using OnlineShop.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Domain.Entities;
using OnlineShop.Api.Domain.Enums;
namespace OnlineShop.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        public AuthService(AppDbContext db) => _db = db;

        public async Task RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
        {
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Customer
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        public async Task<User> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
            return user;
        }
    }
}
