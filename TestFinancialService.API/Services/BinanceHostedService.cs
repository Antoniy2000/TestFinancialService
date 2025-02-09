using Microsoft.Extensions.Options;
using System.Net.WebSockets;
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
    private readonly ILogger _logger;
    public BinanceHostedService(IOptions<BinanceOptions> options, TickersService tickersService, ILogger<BinanceHostedService> logger)
    {
        _options = options.Value;
        _tickersService = tickersService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Start executing");
        if (!_options.Enabled)
            return;

        foreach (var tickerName in _options.Tickers)
        {
            ListenTickerUpdatesAsync(tickerName, cancellationToken);
        }
        _logger.LogDebug("Start executed");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stop executed");
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
                    _logger.LogError(ex, $"Connection to {name} failed");
                }

                if (webSocket.State == WebSocketState.Open)
                {
                    _logger.LogDebug($"Connected to {name} updates server");
                }
            }

            BinanceStreamInfo response = null;
            try
            {
                response = await webSocket.ReadAsJsonAsync<BinanceStreamInfo>(ct: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to receive response from {name}");
            }

            if (response != null && !string.IsNullOrEmpty(response.TickerName))
            {
                _tickersService.SetPrice(response.TickerName, response.Price);
            }
        }
    }

    private async Task ConnectWebSocketAsync(ClientWebSocket webSocket, string name, CancellationToken ct)
    {
        var url = new Uri(Path.Combine(_options.Endpoint, "ws", name));
        _logger.LogInformation($"Connecting to {url}");

        await webSocket.ConnectAsync(url, ct);
        var message = new BinanceRequest
        {
            Method = "SUBSCRIBE",
            Params = [$"{name}@aggTrade"],
            Id = 1
        };
        var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, message, _jsonOptions, ct);
        await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Text, true, ct);

        var res = await webSocket.ReadAsStringAsync(ct: ct);
        _logger.LogDebug($"Received response from {url}: {res}");
    }
}
