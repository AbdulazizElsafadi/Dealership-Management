using Dealership_Management.DTOs;
using Dealership_Management.Services;
using Dealership_Management.Security;
using Dealership_Management.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;

namespace Dealership_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IOtpService _otpService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IOtpService otpService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _otpService = otpService;
            _logger = logger;
        }

        [SwaggerOperation(Summary = "Anonymous")]
        [HttpPost("register/request-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterRequestOtp([FromBody] RegisterDto registerDto)
        {
            _logger.LogInformation("User registration OTP request for email {Email}.", registerDto.Email);
            var user = await _authService.RegisterUserPendingAsync(registerDto);
            if (user == null)
            {
                _logger.LogWarning("Registration failed: email {Email} already exists.", registerDto.Email);
                return Problem(
                    title: "Email already exists.",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                );
            }
            await _otpService.GenerateOtpAsync(user.Id, OtpPurpose.Register);
            return Ok(new { userId = user.Id, message = "OTP sent. Please verify." });
        }

        [SwaggerOperation(Summary = "Anonymous")]
        [HttpPost("register/verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterVerifyOtp([FromBody] OtpVerifyDto dto)
        {
            var valid = await _otpService.ValidateOtpAsync(dto.UserId, dto.Code, OtpPurpose.Register);
            if (!valid) return Problem(title: "Invalid or expired OTP.", statusCode: StatusCodes.Status400BadRequest, type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");
            await _authService.ActivateUserAsync(dto.UserId);
            var user = await _authService.GetUserByIdAsync(dto.UserId);
            if (user == null) return Problem(title: "User not found.", statusCode: StatusCodes.Status404NotFound, type: "https://tools.ietf.org/html/rfc9110#section-15.5.5");
            var response = _authService.CreateAuthResponse(user);
            return Ok(response);
        }

        [SwaggerOperation(Summary = "Anonymous")]
        [HttpPost("login/request-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginRequestOtp([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("User login OTP request for email {Email}.", loginDto.Email);
            var user = await _authService.GetUserForLoginAsync(loginDto);
            if (user == null)
            {
                _logger.LogWarning("Login failed for email {Email}.", loginDto.Email);
                return Problem(title: "Invalid credentials.", statusCode: StatusCodes.Status401Unauthorized, type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
            }
            await _otpService.GenerateOtpAsync(user.Id, OtpPurpose.Login);
            return Ok(new { userId = user.Id, message = "OTP sent. Please verify." });
        }

        [SwaggerOperation(Summary = "Anonymous")]
        [HttpPost("login/verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginVerifyOtp([FromBody] OtpVerifyDto dto)
        {
            var valid = await _otpService.ValidateOtpAsync(dto.UserId, dto.Code, OtpPurpose.Login);
            if (!valid) return Problem(title: "Invalid or expired OTP.", statusCode: StatusCodes.Status400BadRequest, type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");
            var token = await _authService.GenerateJwtAsync(dto.UserId);
            return Ok(new { token });
        }

        [SwaggerOperation(Summary = "Admin")]
        [HttpGet("customers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerListItemDto>>> ListAllCustomers()
        {
            _logger.LogInformation("Admin is requesting list of all customers.");
            try
            {
                var result = await _authService.ListAllCustomersAsync();
                _logger.LogInformation("List of all customers retrieved for admin.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving list of all customers for admin.");
                return Problem(title: "Internal server error.", statusCode: StatusCodes.Status500InternalServerError, type: "https://tools.ietf.org/html/rfc9110#section-15.6.1");
            }
        }
    }
}