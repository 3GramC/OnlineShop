using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Domain.Entities;

namespace OnlineShop.Api.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(AppDbContext db, string adminEmail,  string password, CancellationToken ct)
        {
            var adminExists = await db.Users.AnyAsync(u => u.Email == adminEmail, ct);
            if (adminExists)
            {
                return;
            }
            var admin = new User
            {
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = Domain.Enums.UserRole.Admin,
            };
            db.Users.Add(admin);
            await db.SaveChangesAsync(ct);
        }
    }
}
