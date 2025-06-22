using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dealership_Management.Models
{
    public enum PurchaseStatus
    {
        Pending = 0,
        Completed = 1,
        Rejected = 2
    }

    public class Purchase
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAtPurchase { get; set; }

        public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

        public int? ProcessedByAdminId { get; set; }
        public User? ProcessedByAdmin { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
    }
}