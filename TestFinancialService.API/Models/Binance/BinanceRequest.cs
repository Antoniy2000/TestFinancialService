namespace TestFinancialService.API.Models.Binance;
public class BinanceRequest
{
    public long? Id { get; set; }
    public string Method { get; set; }
    public List<string> Params { get; set; }
}
