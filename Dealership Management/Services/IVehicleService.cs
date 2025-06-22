using Dealership_Management.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dealership_Management.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync();
        Task<VehicleResponseDto?> GetVehicleByIdAsync(int id);
        Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto);
        Task<VehicleResponseDto?> UpdateVehicleAsync(int id, UpdateVehicleDto updateVehicleDto);
        Task<bool> DeleteVehicleAsync(int id);
        Task<IEnumerable<VehicleResponseDto>> GetAvailableVehiclesAsync();
        Task<IEnumerable<VehicleResponseDto>> SearchVehiclesAsync(string? make, string? model, int? minYear, int? maxYear, decimal? minPrice, decimal? maxPrice);
        Task<IEnumerable<VehicleListItemDto>> SearchVehiclesListAsync(string? make, string? model, int? minYear, int? maxYear, decimal? minPrice, decimal? maxPrice);
    }
}