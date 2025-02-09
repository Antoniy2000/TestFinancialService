namespace TestFinancialService.API.Options;
public record BinanceOptions
{
    public string Endpoint { get; set; }
    public List<string> Tickers { get; set; }
}
