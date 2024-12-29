using CurrencyInator.Core.Data.Models;

namespace CurrencyInator.Core.Models;

public class CurrencyRateResult
{
    public required string Currency { get; set; }
    public required DateOnly Date { get; set; }
    public required decimal BuyRate { get; set; }
    public required decimal SellRate { get; set; }

    public static CurrencyRateResult From(CurrencyRate model) => new()
    {
        Currency = model.Currency,
        Date = model.Date,
        BuyRate = model.BuyRate,
        SellRate = model.SellRate,
    };
}
