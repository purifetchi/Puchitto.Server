using System.Net.WebSockets;

namespace Puchitto.Server.Clients;

public class ClientConnection
{
    /// <summary>
    /// Called when we have an incoming message.
    /// </summary>
    public Func<ArraySegment<byte>, Task>? OnIncomingMessage { get; set; }    
    
    /// <summary>
    /// The socket this client connection is using.
    /// </summary>
    private readonly WebSocket _socket;
    
    /// <summary>
    /// The read buffer.
    /// </summary>
    private readonly byte[] _readBuffer = new byte[1024 * 10];

    /// <summary>
    /// Constructs a new client connection.
    /// </summary>
    /// <param name="socket">The websocket.</param>
    public ClientConnection(WebSocket socket)
    {
        _socket = socket;
    }

    /// <summary>
    /// Pumps incoming messages.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    public async Task PumpMessages(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            var read = 0;
            do
            {
                result = await _socket.ReceiveAsync(
                    new ArraySegment<byte>(_readBuffer, read, _readBuffer.Length - read),
                    stoppingToken
                );
                read += result.Count;
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, stoppingToken);
                break;
            }

            if (result.MessageType != WebSocketMessageType.Binary)
            {
                continue;
            }
            
            var segment = new ArraySegment<byte>(_readBuffer, 0, read);
            if (OnIncomingMessage != null)
            {
                await OnIncomingMessage(segment);
            }
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
