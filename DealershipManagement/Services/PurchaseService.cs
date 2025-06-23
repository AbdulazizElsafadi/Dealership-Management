using Dealership_Management.Data;
using Dealership_Management.DTOs;
using Dealership_Management.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dealership_Management.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly DealershipDbContext _context;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(DealershipDbContext context, ILogger<PurchaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PurchaseResponseDto?> CreatePurchaseAsync(CreatePurchaseDto purchaseDto)
        {
            _logger.LogInformation("Creating purchase for user {UserId} and vehicle {VehicleId}.", purchaseDto.UserId, purchaseDto.VehicleId);
            var vehicle = await _context.Vehicles.FindAsync(purchaseDto.VehicleId);
            if (vehicle == null || !vehicle.IsAvailable)
            {
                return null; // Vehicle not found or not available
            }

            var purchase = new Purchase
            {
                UserId = purchaseDto.UserId,
                VehicleId = purchaseDto.VehicleId,
                PriceAtPurchase = purchaseDto.PriceAtPurchase,
                PurchaseDate = DateTime.UtcNow
            };

            vehicle.IsAvailable = false; // Mark vehicle as sold

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Purchase created for user {UserId} and vehicle {VehicleId}.", purchaseDto.UserId, purchaseDto.VehicleId);
            return await GetPurchaseByIdAsync(purchase.Id);
        }

        public async Task<PurchaseResponseDto?> GetPurchaseByIdAsync(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.User)
                .Include(p => p.Vehicle)
                .FirstOrDefaultAsync(p => p.Id == id);

            return purchase == null ? null : MapToResponseDto(purchase);
        }

        public async Task<IEnumerable<PurchaseResponseDto>> GetPurchasesByUserIdAsync(int userId)
        {
            var purchases = await _context.Purchases
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.Vehicle)
                .ToListAsync();

            return purchases.Select(MapToResponseDto);
        }

        public async Task<PurchaseHistoryItemDto> RequestPurchaseAsync(int userId, PurchaseRequestDto dto)
        {
            _logger.LogInformation("User {UserId} is requesting purchase for vehicle {VehicleId}.", userId, dto.VehicleId);
            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null || !vehicle.IsAvailable)
                throw new InvalidOperationException("Vehicle not available");

            // Prevent duplicate pending purchase requests for the same vehicle by the same user
            bool alreadyRequested = await _context.Purchases.AnyAsync(p => p.UserId == userId && p.VehicleId == dto.VehicleId && p.Status == PurchaseStatus.Pending);
            if (alreadyRequested)
                throw new InvalidOperationException("You have already requested a purchase for this vehicle and it is still pending.");

            var purchase = new Purchase
            {
                UserId = userId,
                VehicleId = dto.VehicleId,
                PriceAtPurchase = vehicle.Price,
                PurchaseDate = DateTime.UtcNow,
                Status = PurchaseStatus.Pending
            };
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Purchase request created for user {UserId} and vehicle {VehicleId}.", userId, dto.VehicleId);
            return MapToHistoryDto(purchase, vehicle);
        }

        public async Task<IEnumerable<PurchaseHistoryItemDto>> GetCustomerPurchaseHistoryAsync(int userId)
        {
            _logger.LogInformation("Getting purchase history for user {UserId}.", userId);
            var purchases = await _context.Purchases
                .Where(p => p.UserId == userId)
                .Include(p => p.Vehicle)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
            _logger.LogInformation("Retrieved {Count} purchases for user {UserId}.", purchases.Count(), userId);
            return purchases.Select(p => MapToHistoryDto(p, p.Vehicle));
        }

        public async Task<IEnumerable<AdminPurchaseListItemDto>> GetAllPurchasesForAdminAsync()
        {
            _logger.LogInformation("Admin is requesting all purchases.");
            var purchases = await _context.Purchases
                .Include(p => p.User)
                .Include(p => p.Vehicle)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
            _logger.LogInformation("Admin retrieved {Count} purchases.", purchases.Count());
            return purchases.Select(p => new AdminPurchaseListItemDto
            {
                Id = p.Id,
                UserId = p.UserId,
                CustomerName = p.User.FullName,
                VehicleId = p.VehicleId,
                VehicleMake = p.Vehicle.Make,
                VehicleModel = p.Vehicle.Model,
                PriceAtPurchase = p.PriceAtPurchase,
                Status = p.Status.ToString(),
                PurchaseDate = p.PurchaseDate
            });
        }

        public async Task<AdminPurchaseDetailDto?> GetPurchaseDetailForAdminAsync(int id)
        {
            _logger.LogInformation("Admin is requesting purchase detail for purchase {PurchaseId}.", id);
            var p = await _context.Purchases
                .Include(pu => pu.User)
                .Include(pu => pu.Vehicle)
                .Include(pu => pu.ProcessedByAdmin)
                .FirstOrDefaultAsync(pu => pu.Id == id);
            if (p == null)
            {
                _logger.LogWarning("Purchase detail not found for purchase {PurchaseId}.", id);
                return null;
            }
            _logger.LogInformation("Admin retrieved purchase detail for purchase {PurchaseId}.", id);
            return new AdminPurchaseDetailDto
            {
                Id = p.Id,
                UserId = p.UserId,
                CustomerName = p.User.FullName,
                VehicleId = p.VehicleId,
                VehicleMake = p.Vehicle.Make,
                VehicleModel = p.Vehicle.Model,
                VehicleYear = p.Vehicle.Year,
                PriceAtPurchase = p.PriceAtPurchase,
                Status = p.Status.ToString(),
                PurchaseDate = p.PurchaseDate,
                ProcessedByAdminId = p.ProcessedByAdminId,
                ProcessedByAdminName = p.ProcessedByAdmin?.FullName
            };
        }

        public async Task<(bool found, bool completed)> CompletePurchaseAsync(int purchaseId, int adminId)
        {
            _logger.LogInformation("Admin {AdminId} is attempting to complete purchase {PurchaseId}.", adminId, purchaseId);
            var purchase = await _context.Purchases.Include(p => p.Vehicle).FirstOrDefaultAsync(p => p.Id == purchaseId);
            if (purchase == null)
            {
                _logger.LogWarning("Purchase {PurchaseId} not found for completion.", purchaseId);
                return (false, false);
            }
            if (purchase.Status != PurchaseStatus.Pending)
            {
                _logger.LogWarning("Purchase {PurchaseId} is not pending and cannot be completed.", purchaseId);
                return (true, false);
            }
            purchase.Status = PurchaseStatus.Completed;
            purchase.ProcessedByAdminId = adminId;
            if (purchase.Vehicle != null)
                purchase.Vehicle.IsAvailable = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Purchase {PurchaseId} completed by admin {AdminId}.", purchaseId, adminId);
            return (true, true);
        }

        private PurchaseResponseDto MapToResponseDto(Purchase purchase)
        {
            return new PurchaseResponseDto
            {
                Id = purchase.Id,
                UserId = purchase.UserId,
                VehicleId = purchase.VehicleId,
                PurchaseDate = purchase.PurchaseDate,
                PriceAtPurchase = purchase.PriceAtPurchase,
                User = purchase.User == null ? null : new UserResponseDto
                {
                    Id = purchase.User.Id,
                    FullName = purchase.User.FullName,
                    Email = purchase.User.Email,
                    Role = purchase.User.Role,
                    CreatedAt = purchase.User.CreatedAt
                },
                Vehicle = purchase.Vehicle == null ? null : new VehicleResponseDto
                {
                    Id = purchase.Vehicle.Id,
                    Make = purchase.Vehicle.Make,
                    Model = purchase.Vehicle.Model,
                    Year = purchase.Vehicle.Year,
                    Price = purchase.Vehicle.Price,
                    Color = purchase.Vehicle.Color,
                    Mileage = purchase.Vehicle.Mileage,
                    Description = purchase.Vehicle.Description,
                    IsAvailable = purchase.Vehicle.IsAvailable,
                    CreatedAt = purchase.Vehicle.CreatedAt
                }
            };
        }

        private PurchaseHistoryItemDto MapToHistoryDto(Purchase p, Vehicle v)
        {
            return new PurchaseHistoryItemDto
            {
                Id = p.Id,
                VehicleId = v.Id,
                VehicleMake = v.Make,
                VehicleModel = v.Model,
                VehicleYear = v.Year,
                PriceAtPurchase = p.PriceAtPurchase,
                Status = p.Status.ToString(),
                PurchaseDate = p.PurchaseDate
            };
        }
    }
}