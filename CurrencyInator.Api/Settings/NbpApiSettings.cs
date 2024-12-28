using System.ComponentModel.DataAnnotations;

namespace CurrencyInator.Api.Settings;

public class NbpApiSettings
{
    [Required]
    public required string Url { get; set; }
}
