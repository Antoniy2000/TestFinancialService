using TestFinancialService.API.Models;

namespace TestFinancialService.API.Services;
public class TickersService
{
    private readonly List<TickerInfo> _tickers = [];

    public List<string> Available => _tickers.Select(x => x.TickerName).ToList();

    public double? GetPrice(string name)
    {
        return _tickers.FirstOrDefault(x => x.TickerName == name)?.Price;
    }

    public void SetPrice(string name, double price)
    {
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
    }
}
