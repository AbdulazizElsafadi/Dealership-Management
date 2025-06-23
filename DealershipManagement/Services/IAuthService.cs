using Dealership_Management.DTOs;
using System.Threading.Tasks;
using Dealership_Management.Models;

namespace Dealership_Management.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<IEnumerable<CustomerListItemDto>> ListAllCustomersAsync();
        Task<User?> RegisterUserPendingAsync(RegisterDto registerDto);
        Task ActivateUserAsync(int userId);
        Task<User?> GetUserForLoginAsync(LoginDto loginDto);
        Task<string> GenerateJwtAsync(int userId);
        Task<User?> GetUserByIdAsync(int userId);
        AuthResponseDto CreateAuthResponse(User user);
    }
}