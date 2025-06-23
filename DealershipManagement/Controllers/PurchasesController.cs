using Dealership_Management.DTOs;
using Dealership_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using Dealership_Management.Security;
using Dealership_Management.Models;

namespace Dealership_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<PurchasesController> _logger;
        private readonly IVehicleService _vehicleService;
        private readonly IOtpService _otpService;

        public PurchasesController(IPurchaseService purchaseService, ILogger<PurchasesController> logger, IVehicleService vehicleService, IOtpService otpService)
        {
            _purchaseService = purchaseService;
            _logger = logger;
            _vehicleService = vehicleService;
            _otpService = otpService;
        }

        /// <summary>
        /// Request a purchase (Customer only)
        /// </summary>
        /// <remarks>Role: Customer</remarks>
        [HttpPost("request")]
        [Authorize(Roles = "Customer")]
        [SwaggerOperation(Summary = "Customer")]
        public async Task<ActionResult<PurchaseHistoryItemDto>> RequestPurchase([FromBody] PurchaseRequestDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("User ID claim missing in purchase request.");
                return Problem(title: "User ID claim missing.", statusCode: StatusCodes.Status401Unauthorized, type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
            }
            var userId = int.Parse(userIdClaim);
            _logger.LogInformation("User {UserId} is requesting a purchase.", userId);
            // OTP validation
            var otpValid = await _otpService.ValidateOtpAsync(userId, dto.OtpCode, OtpPurpose.Purchase);
            if (!otpValid)
            {
                _logger.LogWarning("Invalid OTP for user {UserId} in purchase request.", userId);
                return Problem(title: "Invalid or expired OTP code.", statusCode: StatusCodes.Status400BadRequest, type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");
            }
            try
            {
                // Get the vehicle to determine the current price
                var vehicle = await _vehicleService.GetVehicleByIdAsync(dto.VehicleId);
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found for purchase request.", dto.VehicleId);
                    return Problem(title: $"Vehicle with ID {dto.VehicleId} not found.", statusCode: StatusCodes.Status404NotFound, type: "https://tools.ietf.org/html/rfc9110#section-15.5.5");
                }
                var result = await _purchaseService.RequestPurchaseAsync(userId, dto);
                _logger.LogInformation("Purchase request successful for user {UserId}.", userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in purchase request for user {UserId}.", userId);
                return Problem(title: ex.Message, statusCode: StatusCodes.Status400BadRequest, type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing purchase request for user {UserId}.", userId);
                return Problem(title: "Internal server error.", statusCode: StatusCodes.Status500InternalServerError, type: "https://tools.ietf.org/html/rfc9110#section-15.6.1");
            }
        }

        /// <summary>
        /// Get purchase history (Customer only)
        /// </summary>
        /// <remarks>Role: Customer</remarks>
        [HttpGet("history")]
        [Authorize(Roles = "Customer")]
        [SwaggerOperation(Summary = "Customer")]
        public async Task<ActionResult<IEnumerable<PurchaseHistoryItemDto>>> GetPurchaseHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("User ID claim missing in purchase history request.");
                return Problem(title: "User ID claim missing.", statusCode: StatusCodes.Status401Unauthorized, type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
            }
            var userId = int.Parse(userIdClaim);
            _logger.LogInformation("User {UserId} is requesting purchase history.", userId);
            try
            {
                var result = await _purchaseService.GetCustomerPurchaseHistoryAsync(userId);
                _logger.LogInformation("Purchase history retrieved for user {UserId}.", userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase history for user {UserId}.", userId);
                return Problem(title: "Internal server error.", statusCode: StatusCodes.Status500InternalServerError, type: "https://tools.ietf.org/html/rfc9110#section-15.6.1");
            }
        }

        /// <summary>
        /// Get all purchases (Admin only)
        /// </summary>
        /// <remarks>Role: Admin</remarks>
        [SwaggerOperation(Summary = "Admin")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<AdminPurchaseListItemDto>>> GetAllPurchases()
        {
            _logger.LogInformation("Admin is requesting all purchases.");
            try
            {
                var result = await _purchaseService.GetAllPurchasesForAdminAsync();
                _logger.LogInformation("All purchases retrieved for admin.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all purchases for admin.");
                return Problem(title: "Internal server error.", statusCode: StatusCodes.Status500InternalServerError, type: "https://tools.ietf.org/html/rfc9110#section-15.6.1");
            }
        }

        /// <summary>
        /// Get purchase detail (Admin only)
        /// </summary>
        /// <remarks>Role: Admin</remarks>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Admin")]
        public async Task<ActionResult<AdminPurchaseDetailDto>> GetPurchaseDetail(int id)
        {
            _logger.LogInformation("Admin is requesting purchase detail for purchase {PurchaseId}.", id);
            try
            {
                var result = await _purchaseService.GetPurchaseDetailForAdminAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Purchase detail not found for purchase {PurchaseId}.", id);
                    return Problem(title: $"Purchase detail not found for purchase {id}.", statusCode: StatusCodes.Status404NotFound, type: "https://tools.ietf.org/html/rfc9110#section-15.5.5");
                }
                _logger.LogInformation("Purchase detail retrieved for purchase {PurchaseId}.", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase detail for purchase {PurchaseId}.", id);
                return Problem(title: "Internal server error.", statusCode: StatusCodes.Status500InternalServerError, type: "https://tools.ietf.org/html/rfc9110#section-15.6.1");
            }
        }

        /// <summary>
        /// Complete a purchase (Admin only)
        /// </summary>
        /// <remarks>Role: Admin</remarks>
        [HttpPut("complete/{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Admin")]
        public async Task<IActionResult> CompletePurchase(int id)
        {
            var adminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (adminIdStr == null)
            {
                _logger.LogWarning("Admin ID claim missing in complete purchase request.");
                return Problem(title: "Admin ID claim missing.", statusCode: StatusCodes.Status401Unauthorized, type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
            }
            var adminId = int.Parse(adminIdStr);
            _logger.LogInformation("Admin {AdminId} is attempting to complete purchase {PurchaseId}.", adminId, id);
            try
            {
                var (found, completed) = await _purchaseService.CompletePurchaseAsync(id, adminId);
                if (!found)
                {
                    _logger.LogWarning("Purchase {PurchaseId} not found for completion.", id);
                    return Problem(title: $"Purchase {id} not found for completion.", statusCode: StatusCodes.Status404NotFound, type: "https://tools.ietf.org/html/rfc9110#section-15.5.5");
                }
                if (!completed)
                {
                    _logger.LogWarning("Purchase {PurchaseId} is not pending and cannot be completed.", id);
                    return Problem(title: "Purchase not pending.", statusCode: StatusCodes.Status400BadRequest, type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");
                }
                _logger.LogInformation("Purchase {PurchaseId} completed by admin {AdminId}.", id, adminId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing purchase {PurchaseId} by admin {AdminId}.", id, adminId);
                return Problem(title: "Internal server error.", statusCode: StatusCodes.Status500InternalServerError, type: "https://tools.ietf.org/html/rfc9110#section-15.6.1");
            }
        }

        /// <summary>
        /// Request OTP for purchase (Customer only)
        /// </summary>
        /// <remarks>Role: Customer</remarks>
        [HttpPost("request-otp")]
        [Authorize(Roles = "Customer")]
        [SwaggerOperation(Summary = "Customer")]
        public async Task<IActionResult> RequestPurchaseOtp([FromBody] int vehicleId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("User ID claim missing in OTP request.");
                return Problem(title: "User ID claim missing.", statusCode: StatusCodes.Status401Unauthorized, type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
            }
            var userId = int.Parse(userIdClaim);
            // Generate OTP for purchase
            var otp = await _otpService.GenerateOtpAsync(userId, OtpPurpose.Purchase);
            // Log OTP to console (already done in OtpService, but log here for clarity)
            Console.WriteLine($"[OTP] UserId: {userId}, Purpose: Purchase, Code: {otp.Code}");
            return NoContent();
        }
    }
}