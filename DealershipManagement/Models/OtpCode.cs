using System.ComponentModel.DataAnnotations;

namespace Dealership_Management.Models
{
    public enum OtpPurpose
    {
        Register,
        Login,
        Purchase,
        UpdateVehicle
    }

    public class OtpCode
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public OtpPurpose Purpose { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; } = null!;
    }
}