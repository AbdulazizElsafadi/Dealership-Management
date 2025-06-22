using Dealership_Management.Models;
using System.Threading.Tasks;

namespace Dealership_Management.Security
{
    public interface IOtpService
    {
        Task<OtpCode> GenerateOtpAsync(int userId, OtpPurpose purpose);
        Task<bool> ValidateOtpAsync(int userId, string code, OtpPurpose purpose);
    }
}