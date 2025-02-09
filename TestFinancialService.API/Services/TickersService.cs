using TestFinancialService.API.Models;

namespace TestFinancialService.API.Services;
public class TickersService(ILogger<TickersService> logger)
{
    private readonly List<TickerInfo> _tickers = [];

    public delegate void TickerPriceChangedEventArgs(string ticker, double price);
    public event TickerPriceChangedEventArgs TickerPriceChanged;

    public List<string> Available => _tickers.Select(x => x.TickerName).ToList();

    public double? GetPrice(string name)
    {
        return _tickers.FirstOrDefault(x => string.Equals(x.TickerName, name, StringComparison.InvariantCultureIgnoreCase))?.Price;
    }

    public void SetPrice(string name, double price)
    {
        logger.LogDebug($"Set price: {name} - {price}");
        var ticker = _tickers.FirstOrDefault(x => x.TickerName == name);
        if (ticker != null)
        {
            ticker.Price = price;
        }
        else
        {
            _tickers.Add(new()
            {
                TickerName = name,
                Price = price
            });
        }
        // here we are notificating about changes
        TickerPriceChanged?.Invoke(name, price);
    }
}