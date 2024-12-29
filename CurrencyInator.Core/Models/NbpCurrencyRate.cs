namespace CurrencyInator.Core.Models;

public class NbpCurrencyRate
{
    public required string Table { get; set; }
    public required string Currency { get; set; }
    public required string Code { get; set; }
    public required List<NbpRate> Rates { get; set; }
}

public class NbpRate
{
    public required string No { get; set; }
    public required string EffectiveDate { get; set; }
    public required decimal Bid { get; set; }
    public required decimal Ask { get; set; }
}


