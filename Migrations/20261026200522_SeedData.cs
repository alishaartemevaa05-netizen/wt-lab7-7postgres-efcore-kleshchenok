using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wwtlab77postgresefcorekleshchenok.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "Brand", "Category", "CreatedAt", "DailyPrice", "Description", "FuelType", "ImageUrl", "IsAvailable", "Model", "Seats", "Transmission", "Year" },
                values: new object[,]
                {
                    { 1, "Hyundai", "Economy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 60m, "Экономичный городской автомобиль", "Petrol", null, true, "Solaris", 5, "Automatic", 2023 },
                    { 2, "Kia", "Suv", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 90m, "Просторный кроссовер", "Diesel", null, false, "Sportage", 5, "Automatic", 2022 },
                    { 3, "BMW", "Business", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 180m, "Бизнес-седан премиум-класса", "Petrol", null, true, "5 Series", 5, "Automatic", 2024 },
                    { 4, "Tesla", "Comfort", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 150m, "Электромобиль, запас хода 500 км", "Electric", null, true, "Model 3", 5, "Automatic", 2024 },
                    { 5, "Volkswagen", "Minivan", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 120m, "Семейный минивэн на 7 мест", "Diesel", null, true, "Multivan", 7, "Automatic", 2021 },
                    { 6, "Toyota", "Comfort", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100m, "Надёжный гибридный седан", "Hybrid", null, true, "Camry", 5, "Automatic", 2023 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
