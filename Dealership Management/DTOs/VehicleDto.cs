using System.ComponentModel.DataAnnotations;

namespace Dealership_Management.DTOs
{
    public class CreateVehicleDto
    {
        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Range(1900, 2030)]
        public int Year { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        [Range(0, int.MaxValue)]
        public int? Mileage { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateVehicleDto
    {
        [StringLength(50)]
        public string? Make { get; set; }

        [StringLength(50)]
        public string? Model { get; set; }

        [Range(1900, 2030)]
        public int? Year { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        [Range(0, int.MaxValue)]
        public int? Mileage { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsAvailable { get; set; }
    }

    public class VehicleResponseDto
    {
        public int Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string? Color { get; set; }
        public int? Mileage { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class VehicleListItemDto
    {
        public int Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string? Color { get; set; }
    }
}