using Dealership_Management.Models;

namespace Dealership_Management.Security
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}