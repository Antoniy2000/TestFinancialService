using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.WebSockets;
using TestFinancialService.API.Services;

namespace TestFinancialService.API.Controllers;

[Route("ws")]
public class FinancialInstrumentsWebSocketController(WebSocketsService webSocketsService) : ControllerBase
{
    [HttpGet("{pair}")]
    public async Task Get(string pair, CancellationToken ct)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            webSocketsService.AddWebSocket(ws, pair);
            await HandleWebSocketConnectionAsync(ws, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }

    private async Task HandleWebSocketConnectionAsync(WebSocket webSocket, CancellationToken ct)
    {
        var buffer = new byte[64];
        try
        {
            while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                }
            }
        }
        finally
        {
            if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            }
            webSocketsService.RemoveWebSocket(webSocket);
            webSocket.Dispose();
        }
    }
}
