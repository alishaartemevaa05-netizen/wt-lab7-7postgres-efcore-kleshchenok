using System.ComponentModel.DataAnnotations;

namespace wt_lab7_7postgres_efcore_kleshchenok.Models;

public class Rental
{
    public int Id { get; set; }

    [Display(Name = "Автомобиль")]
    public int CarId { get; set; }

    [Display(Name = "Клиент")]
    public int CustomerId { get; set; }

    [Display(Name = "Дата начала")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Display(Name = "Дата окончания")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    [Display(Name = "Стоимость")]
    public decimal TotalPrice { get; set; }

    [Display(Name = "Статус")]
    public RentalStatus Status { get; set; } = RentalStatus.Active;
}
