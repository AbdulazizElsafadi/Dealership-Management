using Dealership_Management.Data;
using Dealership_Management.DTOs;
using Dealership_Management.Models;
using Dealership_Management.Security;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Dealership_Management.Services
{
    public class AuthService : IAuthService
    {
        private readonly DealershipDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(DealershipDbContext context, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for email {Email}.", loginDto.Email);
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user with email {Email} not found.", loginDto.Email);
                return null;
            }
            try
            {
                // This will throw if the password is not valid for bcrypt
                bool isValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
                if (!isValid)
                {
                    _logger.LogWarning("Login failed: invalid password for email {Email}.", loginDto.Email);
                    return null;
                }
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                _logger.LogError(ex, "BCrypt salt parse error for email {Email}.", loginDto.Email);
                return null;
            }
            _logger.LogInformation("Login successful for email {Email}.", loginDto.Email);
            return CreateAuthResponse(user);
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Registration attempt for email {Email}.", registerDto.Email);
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                _logger.LogWarning("Registration failed: email {Email} already exists.", registerDto.Email);
                return null;
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = Role.Customer // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Registration successful for email {Email}.", registerDto.Email);
            return CreateAuthResponse(user);
        }

        public async Task<IEnumerable<CustomerListItemDto>> ListAllCustomersAsync()
        {
            _logger.LogInformation("Listing all customers.");
            var customers = await _context.Users
                .Where(u => u.Role == Role.Customer)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            _logger.LogInformation("Retrieved {Count} customers.", customers.Count);
            return customers.Select(u => new CustomerListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<User?> RegisterUserPendingAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Registration attempt for email {Email} (pending).", registerDto.Email);
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                _logger.LogWarning("Registration failed: email {Email} already exists.", registerDto.Email);
                return null;
            }
            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = Role.Customer // Default role
                // Optionally, add a flag for IsActive = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task ActivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                // Optionally, set IsActive = true if you have such a field
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetUserForLoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for email {Email} (OTP flow).", loginDto.Email);
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user with email {Email} not found.", loginDto.Email);
                return null;
            }
            try
            {
                bool isValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
                if (!isValid)
                {
                    _logger.LogWarning("Login failed: invalid password for email {Email}.", loginDto.Email);
                    return null;
                }
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                _logger.LogError(ex, "BCrypt salt parse error for email {Email}.", loginDto.Email);
                return null;
            }
            return user;
        }

        public async Task<string> GenerateJwtAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return string.Empty;
            return _tokenService.CreateToken(user);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public AuthResponseDto CreateAuthResponse(User user)
        {
            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            var token = _tokenService.CreateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = userResponse
            };
        }
    }
}