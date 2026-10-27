using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_kleshchenok.Models;

namespace wt_lab7_7postgres_efcore_kleshchenok.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Rental> Rentals => Set<Rental>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Car>(e =>
        {
            e.Property(c => c.Category).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Transmission).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.FuelType).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.DailyPrice).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Rental>(e =>
        {
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.TotalPrice).HasPrecision(10, 2);

            e.Property(r => r.StartDate).HasColumnType("date");
            e.Property(r => r.EndDate).HasColumnType("date");

            e.HasOne<Car>().WithMany().HasForeignKey(r => r.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Customer>().WithMany().HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });

        // 2. Начальные данные (Seed Data)
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Car>().HasData(
            new Car { Id = 1, Brand = "Hyundai", Model = "Solaris", Year = 2023, Category = CarCategory.Economy, Transmission = Transmission.Automatic, FuelType = FuelType.Petrol, Seats = 5, DailyPrice = 60, IsAvailable = true, Description = "Экономичный городской автомобиль", CreatedAt = created },
            new Car { Id = 2, Brand = "Kia", Model = "Sportage", Year = 2022, Category = CarCategory.Suv, Transmission = Transmission.Automatic, FuelType = FuelType.Diesel, Seats = 5, DailyPrice = 90, IsAvailable = false, Description = "Просторный кроссовер", CreatedAt = created },
            new Car { Id = 3, Brand = "BMW", Model = "5 Series", Year = 2024, Category = CarCategory.Business, Transmission = Transmission.Automatic, FuelType = FuelType.Petrol, Seats = 5, DailyPrice = 180, IsAvailable = true, Description = "Бизнес-седан премиум-класса", CreatedAt = created },
            new Car { Id = 4, Brand = "Tesla", Model = "Model 3", Year = 2024, Category = CarCategory.Comfort, Transmission = Transmission.Automatic, FuelType = FuelType.Electric, Seats = 5, DailyPrice = 150, IsAvailable = true, Description = "Электромобиль, запас хода 500 км", CreatedAt = created },
            new Car { Id = 5, Brand = "Volkswagen", Model = "Multivan", Year = 2021, Category = CarCategory.Minivan, Transmission = Transmission.Automatic, FuelType = FuelType.Diesel, Seats = 7, DailyPrice = 120, IsAvailable = true, Description = "Семейный минивэн на 7 мест", CreatedAt = created },
            new Car { Id = 6, Brand = "Toyota", Model = "Camry", Year = 2023, Category = CarCategory.Comfort, Transmission = Transmission.Automatic, FuelType = FuelType.Hybrid, Seats = 5, DailyPrice = 100, IsAvailable = true, Description = "Надёжный гибридный седан", CreatedAt = created }
        );
    }
}