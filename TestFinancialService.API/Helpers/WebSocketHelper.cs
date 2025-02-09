using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace TestFinancialService.API.Helpers;
public static class WebSocketHelper
{
    public static async Task<string> ReadAsStringAsync(this ClientWebSocket webSocket, CancellationToken ct)
    {
        var buffer = new byte[1024];
        WebSocketReceiveResult result;
        var responseText = new StringBuilder();
        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            var messageChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
            responseText.Append(messageChunk);
        } while (!result.EndOfMessage && !ct.IsCancellationRequested);

        return responseText.ToString();
    }

    public static async Task<T?> ReadAsJsonAsync<T>(this ClientWebSocket webSocket, JsonSerializerOptions options = null, CancellationToken ct = default)
    {
        var str = await ReadAsStringAsync(webSocket, ct);
        var res = JsonSerializer.Deserialize<T>(str, options);
        return res;
    }
}
