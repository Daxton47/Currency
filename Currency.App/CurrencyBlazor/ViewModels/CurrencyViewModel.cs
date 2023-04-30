using System.ComponentModel.DataAnnotations;

namespace CurrencyBlazor.ViewModels;

public class CurrencyViewModel
{
    [Display(Name = "Currency Code")]
    [Required]
    [MaxLength(25, ErrorMessage = "Currency Code cannot exceed 25 characters.")]
    public string CurrencyCode { get; set; } = default!;

    [Required]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Rate must be above 0")]
    public double Rate { get; set; }
}
