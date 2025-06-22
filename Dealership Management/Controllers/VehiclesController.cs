using Microsoft.AspNetCore.Mvc;
using Dealership_Management.DTOs;
using Dealership_Management.Services;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;

namespace Dealership_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
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
                return StatusCode(500, "Internal server error.");
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
                    return NotFound();
                }
                _logger.LogInformation("Vehicle with ID {Id} retrieved.", id);
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle with ID {Id}.", id);
                return StatusCode(500, "Internal server error.");
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
            var vehicle = await _vehicleService.UpdateVehicleAsync(id, updateVehicleDto);
            if (vehicle == null)
                return NotFound();

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
                return NotFound();

            return NoContent();
        }
    }
}