using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TestFinancialService.API.Helpers;
using TestFinancialService.API.Models.Binance;
using TestFinancialService.API.Options;

namespace TestFinancialService.API.Services;
public class BinanceHostedService : IHostedService
{
    private readonly BinanceOptions _options;
    private readonly TickersService _tickersService;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    public BinanceHostedService(IOptions<BinanceOptions> options, TickersService tickersService)
    {
        _options = options.Value;
        _tickersService = tickersService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var tickerName in _options.Tickers)
        {
            ListenTickerUpdatesAsync(tickerName, cancellationToken);
        }
        return;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task ListenTickerUpdatesAsync(string name, CancellationToken ct)
    {
        using var webSocket = new ClientWebSocket();
        while (!ct.IsCancellationRequested)
        {
            while (webSocket.State != WebSocketState.Open && !ct.IsCancellationRequested)
            {
                try
                {
                    await ConnectWebSocketAsync(webSocket, name, ct);
                }
                catch (Exception ex)
                {

                }
            }

            try
            {
                var response = await webSocket.ReadAsJsonAsync<BinanceStreamInfo>(ct: ct);
                if (response != null && !string.IsNullOrEmpty(response.TickerName))
                {
                    _tickersService.SetPrice(response.TickerName, response.Price);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    private async Task ConnectWebSocketAsync(ClientWebSocket webSocket, string name, CancellationToken ct)
    {
        await webSocket.ConnectAsync(new Uri(Path.Combine(_options.Endpoint, "ws", name)), ct);
        var message = new BinanceRequest
        {
            Method = "SUBSCRIBE",
            Params = [$"{name}@aggTrade"],
            Id = 1
        };
        var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, message, _jsonOptions, ct);
        await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Text, true, ct);
        _ = await webSocket.ReadAsJsonAsync<BinanceResponse>(ct: ct);
    }
}
