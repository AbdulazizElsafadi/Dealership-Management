using Dealership_Management.Data;
using Dealership_Management.DTOs;
using Dealership_Management.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dealership_Management.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly DealershipDbContext _context;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(DealershipDbContext context, ILogger<VehicleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _context.Vehicles
                .OrderBy(v => v.Make)
                .ThenBy(v => v.Model)
                .ToListAsync();

            return vehicles.Select(MapToResponseDto);
        }

        public async Task<VehicleResponseDto?> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            return vehicle != null ? MapToResponseDto(vehicle) : null;
        }

        public async Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto)
        {
            _logger.LogInformation("Creating vehicle: {Make} {Model} {Year}.", createVehicleDto.Make, createVehicleDto.Model, createVehicleDto.Year);
            var vehicle = new Vehicle
            {
                Make = createVehicleDto.Make,
                Model = createVehicleDto.Model,
                Year = createVehicleDto.Year,
                Price = createVehicleDto.Price,
                Color = createVehicleDto.Color,
                Mileage = createVehicleDto.Mileage,
                Description = createVehicleDto.Description,
                IsAvailable = true,
                CreatedAt = System.DateTime.UtcNow
            };
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Vehicle created with ID {Id}.", vehicle.Id);
            return MapToResponseDto(vehicle);
        }

        public async Task<VehicleResponseDto?> UpdateVehicleAsync(int id, UpdateVehicleDto updateVehicleDto)
        {
            _logger.LogInformation("Updating vehicle with ID {Id}.", id);
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle with ID {Id} not found for update.", id);
                return null;
            }

            if (updateVehicleDto.Make != null)
                vehicle.Make = updateVehicleDto.Make;
            if (updateVehicleDto.Model != null)
                vehicle.Model = updateVehicleDto.Model;
            if (updateVehicleDto.Year.HasValue)
                vehicle.Year = updateVehicleDto.Year.Value;
            if (updateVehicleDto.Price.HasValue)
                vehicle.Price = updateVehicleDto.Price.Value;
            if (updateVehicleDto.Color != null)
                vehicle.Color = updateVehicleDto.Color;
            if (updateVehicleDto.Mileage.HasValue)
                vehicle.Mileage = updateVehicleDto.Mileage.Value;
            if (updateVehicleDto.Description != null)
                vehicle.Description = updateVehicleDto.Description;
            if (updateVehicleDto.IsAvailable.HasValue)
                vehicle.IsAvailable = updateVehicleDto.IsAvailable.Value;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Vehicle with ID {Id} updated.", id);
            return MapToResponseDto(vehicle);
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            _logger.LogInformation("Deleting vehicle with ID {Id}.", id);
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle with ID {Id} not found for deletion.", id);
                return false;
            }
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Vehicle with ID {Id} deleted.", id);
            return true;
        }

        public async Task<IEnumerable<VehicleResponseDto>> GetAvailableVehiclesAsync()
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.IsAvailable)
                .OrderBy(v => v.Make)
                .ThenBy(v => v.Model)
                .ToListAsync();

            return vehicles.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<VehicleResponseDto>> SearchVehiclesAsync(string? make, string? model, int? minYear, int? maxYear, decimal? minPrice, decimal? maxPrice)
        {
            _logger.LogInformation("Searching vehicles with filters: make={Make}, model={Model}, minYear={MinYear}, maxYear={MaxYear}, minPrice={MinPrice}, maxPrice={MaxPrice}", make, model, minYear, maxYear, minPrice, maxPrice);
            var query = _context.Vehicles.Where(v => v.IsAvailable).AsQueryable();

            if (!string.IsNullOrWhiteSpace(make))
                query = query.Where(v => v.Make.ToLower().Contains(make.ToLower()));

            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(v => v.Model.ToLower().Contains(model.ToLower()));

            if (minYear.HasValue)
                query = query.Where(v => v.Year >= minYear.Value);

            if (maxYear.HasValue)
                query = query.Where(v => v.Year <= maxYear.Value);

            if (minPrice.HasValue)
                query = query.Where(v => v.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(v => v.Price <= maxPrice.Value);

            var vehicles = await query
                .OrderBy(v => v.Make)
                .ThenBy(v => v.Model)
                .ToListAsync();
            _logger.LogInformation("Found {Count} vehicles.", vehicles.Count);
            return vehicles.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<VehicleListItemDto>> SearchVehiclesListAsync(string? make, string? model, int? minYear, int? maxYear, decimal? minPrice, decimal? maxPrice)
        {
            _logger.LogInformation("Searching vehicle list with filters: make={Make}, model={Model}, minYear={MinYear}, maxYear={MaxYear}, minPrice={MinPrice}, maxPrice={MaxPrice}", make, model, minYear, maxYear, minPrice, maxPrice);
            var query = _context.Vehicles.Where(v => v.IsAvailable).AsQueryable();

            if (!string.IsNullOrWhiteSpace(make))
                query = query.Where(v => v.Make.ToLower().Contains(make.ToLower()));

            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(v => v.Model.ToLower().Contains(model.ToLower()));

            if (minYear.HasValue)
                query = query.Where(v => v.Year >= minYear.Value);

            if (maxYear.HasValue)
                query = query.Where(v => v.Year <= maxYear.Value);

            if (minPrice.HasValue)
                query = query.Where(v => v.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(v => v.Price <= maxPrice.Value);

            var vehicles = await query
                .OrderBy(v => v.Make)
                .ThenBy(v => v.Model)
                .ToListAsync();
            _logger.LogInformation("Found {Count} vehicles for list.", vehicles.Count);
            return vehicles.Select(v => new VehicleListItemDto
            {
                Id = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                Price = v.Price,
                Color = v.Color
            });
        }

        private static VehicleResponseDto MapToResponseDto(Vehicle vehicle)
        {
            return new VehicleResponseDto
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Price = vehicle.Price,
                Color = vehicle.Color,
                Mileage = vehicle.Mileage,
                Description = vehicle.Description,
                IsAvailable = vehicle.IsAvailable,
                CreatedAt = vehicle.CreatedAt
            };
        }
    }
}