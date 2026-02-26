using System.Net.WebSockets;

namespace Puchitto.Server.Clients;

public class ClientConnection
{
    private readonly WebSocket _socket;
    
    private readonly byte[] _readBuffer = new byte[1024 * 10];

    public ClientConnection(WebSocket socket)
    {
        _socket = socket;
    }

    public async Task PumpMessages(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
        {
            var buffer = new ArraySegment<byte>(_readBuffer);
            var result = await _socket.ReceiveAsync(buffer, stoppingToken);
            if (result.MessageType != WebSocketMessageType.Close)
            {
                continue;
            }
            
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, stoppingToken);
        }
    }

    /// <summary>
    /// Sends a given buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="stoppingToken">The cancellation token.</param>
    public async Task SendBuffer(ArraySegment<byte> buffer, CancellationToken stoppingToken = default)
    {
        while (!stoppingToken.IsCancellationRequested && _socket.State != WebSocketState.Open)
        {
            await Task.Yield();
        }
        
        await _socket.SendAsync(
            buffer,
            WebSocketMessageType.Binary,
            WebSocketMessageFlags.EndOfMessage,
            stoppingToken
        );
    }
}
