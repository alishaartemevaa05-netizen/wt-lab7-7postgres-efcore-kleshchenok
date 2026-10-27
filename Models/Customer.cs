using System.ComponentModel.DataAnnotations;

namespace wt_lab7_7postgres_efcore_kleshchenok.Models;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите ФИО")]
    [StringLength(100)]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите телефон")]
    [Phone(ErrorMessage = "Некорректный телефон")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Укажите номер ВУ")]
    [StringLength(20)]
    [Display(Name = "Водительское удостоверение")]
    public string DriverLicense { get; set; } = string.Empty;
}
