using System.ComponentModel.DataAnnotations;

namespace wt_lab7_7postgres_efcore_kleshchenok.Models;

public enum CarCategory
{
    [Display(Name = "Эконом")] Economy,
    [Display(Name = "Комфорт")] Comfort,
    [Display(Name = "Бизнес")] Business,
    [Display(Name = "Внедорожник")] Suv,
    [Display(Name = "Минивэн")] Minivan
}

public enum Transmission
{
    [Display(Name = "Механика")] Manual,
    [Display(Name = "Автомат")] Automatic
}

public enum FuelType
{
    [Display(Name = "Бензин")] Petrol,
    [Display(Name = "Дизель")] Diesel,
    [Display(Name = "Электро")] Electric,
    [Display(Name = "Гибрид")] Hybrid
}

public enum RentalStatus
{
    [Display(Name = "Активна")] Active,
    [Display(Name = "Завершена")] Completed
}
