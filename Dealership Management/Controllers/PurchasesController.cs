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

        public PurchasesController(IPurchaseService purchaseService, ILogger<PurchasesController> logger, IVehicleService vehicleService)
        {
            _purchaseService = purchaseService;
            _logger = logger;
            _vehicleService = vehicleService;
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
                return Unauthorized("User ID claim missing.");
            }
            var userId = int.Parse(userIdClaim);
            _logger.LogInformation("User {UserId} is requesting a purchase.", userId);
            try
            {
                // Get the vehicle to determine the current price
                var vehicle = await _vehicleService.GetVehicleByIdAsync(dto.VehicleId);
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found for purchase request.", dto.VehicleId);
                    return NotFound($"Vehicle with ID {dto.VehicleId} not found.");
                }
                var result = await _purchaseService.RequestPurchaseAsync(userId, dto);
                _logger.LogInformation("Purchase request successful for user {UserId}.", userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in purchase request for user {UserId}.", userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing purchase request for user {UserId}.", userId);
                return StatusCode(500, "Internal server error.");
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
                return Unauthorized("User ID claim missing.");
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
                return StatusCode(500, "Internal server error.");
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
                return StatusCode(500, "Internal server error.");
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
                    return NotFound();
                }
                _logger.LogInformation("Purchase detail retrieved for purchase {PurchaseId}.", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase detail for purchase {PurchaseId}.", id);
                return StatusCode(500, "Internal server error.");
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
                return Unauthorized("Admin ID claim missing.");
            }
            var adminId = int.Parse(adminIdStr);
            _logger.LogInformation("Admin {AdminId} is attempting to complete purchase {PurchaseId}.", adminId, id);
            try
            {
                var (found, completed) = await _purchaseService.CompletePurchaseAsync(id, adminId);
                if (!found)
                {
                    _logger.LogWarning("Purchase {PurchaseId} not found for completion.", id);
                    return NotFound();
                }
                if (!completed)
                {
                    _logger.LogWarning("Purchase {PurchaseId} is not pending and cannot be completed.", id);
                    return BadRequest("Purchase not pending.");
                }
                _logger.LogInformation("Purchase {PurchaseId} completed by admin {AdminId}.", id, adminId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing purchase {PurchaseId} by admin {AdminId}.", id, adminId);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}