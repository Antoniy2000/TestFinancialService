namespace TestFinancialService.API.Options;
public record BinanceOptions
{
    public bool Enabled { get; set; } = true;
    public string Endpoint { get; set; }
    public List<string> Tickers { get; set; }
}
