using HealthCareApp.Data;
using HealthCareApp.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace HealthCareApp.Services
{
    public class UserService : IUserService
    {
        private readonly HealthCareDbContext _context;

        public UserService(HealthCareDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user != null)
            {
                // Check if password is already hashed (starts with $2a$ or $2b$)
                if (user.Password.StartsWith("$2a$") || user.Password.StartsWith("$2b$"))
                {
                    // Password is already hashed, verify with BCrypt
                    if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                    {
                        return user;
                    }
                }
                else
                {
                    // Password is plain text, check direct match (for existing data)
                    if (user.Password == password)
                    {
                        // Hash the password for future use
                        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                        await _context.SaveChangesAsync();
                        return user;
                    }
                }
            }

            return null;
        }

        public async Task<bool> CreateUserAsync(User user, string userType)
        {
            try
            {
                // Hash password
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.UserType = userType;
                user.CreatedDate = DateTime.Now;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create corresponding profile based on user type
                switch (userType)
                {
                    case "Admin":
                        var admin = new Admin
                        {
                            UserID = user.UserID,
                            Name = user.Username
                        };
                        _context.Admins.Add(admin);
                        break;

                    case "Doctor":
                        var doctor = new Doctor
                        {
                            UserID = user.UserID,
                            Name = user.Username
                        };
                        _context.Doctors.Add(doctor);
                        break;

                    case "Patient":
                        var patient = new Patient
                        {
                            UserID = user.UserID,
                            Name = user.Username
                        };
                        _context.Patients.Add(patient);
                        break;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Admin)
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.UserID == userId);
        }

        public async Task<bool> IsUserActiveAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);
            return user?.IsActive ?? false;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsActive = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
