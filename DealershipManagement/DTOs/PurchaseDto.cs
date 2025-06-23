using System;
using System.ComponentModel.DataAnnotations;

namespace Dealership_Management.DTOs
{
    public class CreatePurchaseDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public decimal PriceAtPurchase { get; set; }
    }

    public class PurchaseResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PriceAtPurchase { get; set; }

        public UserResponseDto? User { get; set; }
        public VehicleResponseDto? Vehicle { get; set; }
    }

    public class PurchaseRequestDto
    {
        public int VehicleId { get; set; }
        [Required]
        public string OtpCode { get; set; } = string.Empty;
    }

    public class PurchaseHistoryItemDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public int VehicleYear { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
    }

    public class AdminPurchaseListItemDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public decimal PriceAtPurchase { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
    }

    public class AdminPurchaseDetailDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public int VehicleYear { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public int? ProcessedByAdminId { get; set; }
        public string? ProcessedByAdminName { get; set; }
    }
}