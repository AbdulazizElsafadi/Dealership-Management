using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dealership_Management.Migrations
{
    /// <inheritdoc />
    public partial class SeedTenVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Color", "CreatedAt", "Description", "IsAvailable", "Make", "Mileage", "Model", "Price", "Year" },
                values: new object[,]
                {
                    { 3, "Red", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sporty coupe, low mileage", true, "Ford", 5000, "Mustang", 35000.00m, 2023 },
                    { 4, "White", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reliable sedan, one owner", true, "Chevrolet", 30000, "Malibu", 18000.00m, 2020 },
                    { 5, "Black", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Electric, autopilot included", true, "Tesla", 12000, "Model 3", 42000.00m, 2022 },
                    { 6, "Gray", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Luxury SUV, well maintained", true, "BMW", 35000, "X5", 39000.00m, 2019 },
                    { 7, "Blue", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Premium sedan, great condition", true, "Audi", 18000, "A4", 32000.00m, 2021 },
                    { 8, "Silver", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fuel efficient, compact sedan", true, "Hyundai", 25000, "Elantra", 16000.00m, 2020 },
                    { 9, "White", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Spacious SUV, family friendly", true, "Kia", 40000, "Sorento", 21000.00m, 2018 },
                    { 10, "Black", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Luxury sedan, almost new", true, "Mercedes-Benz", 9000, "C-Class", 45000.00m, 2022 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
