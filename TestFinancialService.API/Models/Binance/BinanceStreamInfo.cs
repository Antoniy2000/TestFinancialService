using System.Text.Json.Serialization;

namespace TestFinancialService.API.Models.Binance;
public class BinanceStreamInfo
{
    [JsonPropertyName("e")]
    public string EventType { get; set; }

    [JsonPropertyName("s")]
    public string TickerName { get; set; }

    [JsonPropertyName("p")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double Price { get; set; }

    [JsonPropertyName("q")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double Quantity { get; set; }

    [JsonPropertyName("f")]
    public long FirstTradeId { get; set; }

    [JsonPropertyName("l")]
    public long LastTradeId { get; set; }

    [JsonPropertyName("m")]
    public bool MarketMaker { get; set; }
}
