using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using TestFinancialService.API.Models;

namespace TestFinancialService.API.Services;
public class WebSocketsService
{
    private readonly ConcurrentDictionary<Guid, (WebSocket socket, string ticker)> _webSockets = new();
    private readonly ILogger _logger;
    public WebSocketsService(TickersService tickersService, ILogger<WebSocketsService> logger)
    {
        tickersService.TickerPriceChanged += TickersService_TickerPriceChanged;
        _logger = logger;
    }

    private async void TickersService_TickerPriceChanged(string ticker, double price)
    {
        var collection = _webSockets.Values.ToList()
            .Where(x => string.Equals(x.ticker, ticker, StringComparison.InvariantCultureIgnoreCase) && x.socket.State == WebSocketState.Open)
            .Select(x => x.socket);

        var info = new TickerInfo
        {
            Price = price,
            TickerName = ticker,
        };
        var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, info);
        var array = ms.ToArray();
        await Parallel.ForEachAsync(collection, async (ws, ct) =>
        {
            try
            {
                await ws.SendAsync(array, WebSocketMessageType.Text, true, ct);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Failed to send to websocket");
                RemoveWebSocket(ws);
            }
        });
    }

    public void AddWebSocket(WebSocket webSocket, string ticker)
    {
        if (_webSockets.TryAdd(Guid.NewGuid(), (webSocket, ticker)))
        {
            _logger.LogDebug("New websocket added");
        }
        else
        {
            _logger.LogDebug("failed to add new websocket");
        }
    }

    public void RemoveWebSocket(WebSocket webSocket)
    {
        var key = _webSockets.FirstOrDefault(x => x.Value.socket == webSocket).Key;
       _webSockets.Remove(key, out _);
    }
}