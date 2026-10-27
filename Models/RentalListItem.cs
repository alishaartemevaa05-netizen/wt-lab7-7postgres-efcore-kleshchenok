namespace wt_lab7_7postgres_efcore_kleshchenok.Models;
/// <summary>DTO для списка аренд: вместо идентификаторов — название автомобиля и ФИО клиента.</summary>
public class RentalListItem
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public string CarName { get; set; } = "";
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public RentalStatus Status { get; set; }
}
