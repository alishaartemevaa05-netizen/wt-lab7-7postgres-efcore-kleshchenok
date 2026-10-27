using System.ComponentModel.DataAnnotations;

namespace wt_lab7_7postgres_efcore_kleshchenok.Models;

/// <summary>DTO для создания аренды: клиент передаёт только своё решение, цену и статус считает сервер.</summary>
public class RentalCreateRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите автомобиль")]
    public int CarId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Укажите клиента")]
    public int CustomerId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
