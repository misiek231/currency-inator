using System.ComponentModel.DataAnnotations;

namespace CurrencyInator.Core.Data.Models;

public class DbSettings
{
    [Required]
    public required string ConnectionString { get; set; }

    [Required]
    public required string DatabaseName { get; set; }
}
