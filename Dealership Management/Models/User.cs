using System.ComponentModel.DataAnnotations;

namespace Dealership_Management.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public Role Role { get; set; } = Role.Customer;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
    }
}