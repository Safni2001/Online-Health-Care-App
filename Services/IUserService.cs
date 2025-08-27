using HealthCareApp.Models;

namespace HealthCareApp.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<bool> CreateUserAsync(User user, string userType);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> IsUserActiveAsync(int userId);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeactivateUserAsync(int userId);
    }
}
