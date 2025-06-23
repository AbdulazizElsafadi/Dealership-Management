using Microsoft.EntityFrameworkCore;
using Dealership_Management.Models;

namespace Dealership_Management.Data
{
    public class DealershipDbContext : DbContext
    {
        public DealershipDbContext(DbContextOptions<DealershipDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Vehicle configuration
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            // Purchase configuration
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(p => p.User)
                    .WithMany(u => u.Purchases)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Vehicle)
                    .WithMany(v => v.Purchases)
                    .HasForeignKey(p => p.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.ProcessedByAdmin)
                    .WithMany()
                    .HasForeignKey(p => p.ProcessedByAdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OtpCode configuration
            modelBuilder.Entity<OtpCode>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(o => o.User)
                    .WithMany(u => u.OtpCodes)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Admin User",
                    Email = "admin@dealership.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Replace with a real hash
                    Role = Role.Admin,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new User
                {
                    Id = 2,
                    FullName = "Customer User",
                    Email = "customer@dealership.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("customer123"), // Replace with a real hash
                    Role = Role.Customer,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Seed Vehicles
            modelBuilder.Entity<Vehicle>().HasData(
                new Vehicle { Id = 1, Make = "Toyota", Model = "Camry", Year = 2022, Price = 25000.00m, Color = "Silver", Mileage = 15000, Description = "Well-maintained sedan with low mileage", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 2, Make = "Honda", Model = "CR-V", Year = 2021, Price = 28000.00m, Color = "Blue", Mileage = 22000, Description = "Reliable SUV with great fuel economy", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 3, Make = "Ford", Model = "Mustang", Year = 2023, Price = 35000.00m, Color = "Red", Mileage = 5000, Description = "Sporty coupe, low mileage", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 4, Make = "Chevrolet", Model = "Malibu", Year = 2020, Price = 18000.00m, Color = "White", Mileage = 30000, Description = "Reliable sedan, one owner", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 5, Make = "Tesla", Model = "Model 3", Year = 2022, Price = 42000.00m, Color = "Black", Mileage = 12000, Description = "Electric, autopilot included", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 6, Make = "BMW", Model = "X5", Year = 2019, Price = 39000.00m, Color = "Gray", Mileage = 35000, Description = "Luxury SUV, well maintained", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 7, Make = "Audi", Model = "A4", Year = 2021, Price = 32000.00m, Color = "Blue", Mileage = 18000, Description = "Premium sedan, great condition", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 8, Make = "Hyundai", Model = "Elantra", Year = 2020, Price = 16000.00m, Color = "Silver", Mileage = 25000, Description = "Fuel efficient, compact sedan", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 9, Make = "Kia", Model = "Sorento", Year = 2018, Price = 21000.00m, Color = "White", Mileage = 40000, Description = "Spacious SUV, family friendly", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 10, Make = "Mercedes-Benz", Model = "C-Class", Year = 2022, Price = 45000.00m, Color = "Black", Mileage = 9000, Description = "Luxury sedan, almost new", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) }
            );
        }
    }
}