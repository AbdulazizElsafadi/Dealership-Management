using Microsoft.AspNetCore.Mvc;
using Dealership_Management.DTOs;
using Dealership_Management.Services;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using Dealership_Management.Security;
using Dealership_Management.Models;

namespace Dealership_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehiclesController> _logger;
        private readonly IOtpService _otpService;

        public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger, IOtpService otpService)
        {
            _vehicleService = vehicleService;
            _logger = logger;
            _otpService = otpService;
        }

        /// <summary>
        /// Get all available vehicles with optional filters
        /// </summary>
        [SwaggerOperation(Summary = "Customer, Admin")]
        [HttpGet]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<IEnumerable<VehicleListItemDto>>> GetVehicles(
            [FromQuery] string? make,
            [FromQuery] string? model,
            [FromQuery] int? minYear,
            [FromQuery] int? maxYear,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            _logger.LogInformation("Getting vehicles with filters: make={Make}, model={Model}, minYear={MinYear}, maxYear={MaxYear}, minPrice={MinPrice}, maxPrice={MaxPrice}", make, model, minYear, maxYear, minPrice, maxPrice);
            try
            {
                var vehicles = await _vehicleService.SearchVehiclesListAsync(make, model, minYear, maxYear, minPrice, maxPrice);
                _logger.LogInformation("Retrieved {Count} vehicles.", vehicles.Count());
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles.");
                return Problem(title: "Internal server error.", statusCode: StatusCodes.Status500InternalServerError, type: "https://tools.ietf.org/html/rfc9110#section-15.6.1");
            }
        }

        /// <summary>
        /// Get a specific vehicle by ID
        /// </summary>
        [SwaggerOperation(Summary = "Customer, Admin")]
        [HttpGet("{id}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<VehicleResponseDto>> GetVehicle(int id)
        {
            _logger.LogInformation("Getting vehicle with ID {Id}.", id);
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {Id} not found.", id);
                    return Problem(title: $"Vehicle with ID {id} not found.", statusCode: StatusCodes.Status404NotFound, type: "https://tools.ietf.org/html/rfc9110#section-15.5.5");
                }
                _logger.LogInformation("Vehicle with ID {Id} retrieved.", id);
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle with ID {Id}.", id);
                return Problem(title: "Internal server error.", statusCode: StatusCodes.Status500InternalServerError, type: "https://tools.ietf.org/html/rfc9110#section-15.6.1");
            }
        }

        /// <summary>
        /// Create a new vehicle
        /// </summary>
        [SwaggerOperation(Summary = "Admin")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VehicleResponseDto>> CreateVehicle(CreateVehicleDto createVehicleDto)
        {
            var vehicle = await _vehicleService.CreateVehicleAsync(createVehicleDto);
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
        }

        /// <summary>
        /// Update an existing vehicle
        /// </summary>
        [SwaggerOperation(Summary = "Admin")]
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateVehicle(int id, UpdateVehicleDto updateVehicleDto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("User ID claim missing in update vehicle request.");
                return Problem(title: "User ID claim missing.", statusCode: StatusCodes.Status401Unauthorized, type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
            }
            var userId = int.Parse(userIdClaim);
            // OTP validation
            var otpValid = await _otpService.ValidateOtpAsync(userId, updateVehicleDto.OtpCode, OtpPurpose.UpdateVehicle);
            if (!otpValid)
            {
                _logger.LogWarning("Invalid OTP for user {UserId} in update vehicle request.", userId);
                return Problem(title: "Invalid or expired OTP code.", statusCode: StatusCodes.Status400BadRequest, type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");
            }
            var vehicle = await _vehicleService.UpdateVehicleAsync(id, updateVehicleDto);
            if (vehicle == null)
                return Problem(title: $"Vehicle with ID {id} not found.", statusCode: StatusCodes.Status404NotFound, type: "https://tools.ietf.org/html/rfc9110#section-15.5.5");

            return Ok(vehicle);
        }

        /// <summary>
        /// Delete a vehicle
        /// </summary>
        [SwaggerOperation(Summary = "Admin")]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var result = await _vehicleService.DeleteVehicleAsync(id);
            if (!result)
                return Problem(title: $"Vehicle with ID {id} not found.", statusCode: StatusCodes.Status404NotFound, type: "https://tools.ietf.org/html/rfc9110#section-15.5.5");

            return NoContent();
        }

        /// <summary>
        /// Request OTP for updating a vehicle (Admin only)
        /// </summary>
        /// <remarks>Role: Admin</remarks>
        [HttpPost("request-update-otp")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Admin")]
        public async Task<IActionResult> RequestUpdateVehicleOtp()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("User ID claim missing in OTP request.");
                return Unauthorized("User ID claim missing.");
            }
            var userId = int.Parse(userIdClaim);
            // Generate OTP for update vehicle
            var otp = await _otpService.GenerateOtpAsync(userId, OtpPurpose.UpdateVehicle);
            // Log OTP to console (already done in OtpService, but log here for clarity)
            Console.WriteLine($"[OTP] UserId: {userId}, Purpose: UpdateVehicle, Code: {otp.Code}");
            return NoContent();
        }
    }
}