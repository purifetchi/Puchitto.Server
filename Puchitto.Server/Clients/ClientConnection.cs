using System.Net.WebSockets;

namespace Puchitto.Server.Clients;

public class ClientConnection
{
    /// <summary>
    /// Called when we have an incoming message.
    /// </summary>
    public Func<ArraySegment<byte>, Task>? OnIncomingMessage { get; set; } 
    
    /// <summary>
    /// Called when the connection is closed.
    /// </summary>
    public Func<Task>? OnConnectionClosed { get; set; } 
    
    /// <summary>
    /// The socket this client connection is using.
    /// </summary>
    private readonly WebSocket _socket;

    /// <summary>
    /// The socket abort cancellation token source.
    /// </summary>
    private readonly CancellationTokenSource _abortCts = new();
    
    /// <summary>
    /// The read buffer.
    /// </summary>
    private readonly byte[] _readBuffer = new byte[1024 * 10];
    
    /// <summary>
    /// The send lock.
    /// </summary>
    private readonly SemaphoreSlim _sendLock = new(1, 1);

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
    public async Task PumpMessages()
    {
        try
        {
            var token = _abortCts.Token;
            while (!token.IsCancellationRequested && _socket.State == WebSocketState.Open)
            {
                var (result, read) = await ReadOneMessage(token);

                if (result == null ||
                    token.IsCancellationRequested ||
                    _socket.State != WebSocketState.Open)
                {
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
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
        catch (WebSocketException)
        {
            // Swallow, the client disconnected ungracefully. Can happen.
        }
        catch (OperationCanceledException)
        {
            // Swallow, cancelled from within Close()
        }
        finally
        {
            if (OnConnectionClosed != null)
            {
                await OnConnectionClosed();
            }    
        }
    }

    /// <summary>
    /// Reads one incoming message from the client.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>The message and the amount of data read.</returns>
    private async Task<(WebSocketReceiveResult? result, int read)> ReadOneMessage(CancellationToken stoppingToken)
    {
        WebSocketReceiveResult result;
        var read = 0;
        do
        {
            if (read >= _readBuffer.Length)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "", stoppingToken);
                return (null, 0);
            }
            
            result = await _socket.ReceiveAsync(
                new ArraySegment<byte>(_readBuffer, read, _readBuffer.Length - read),
                stoppingToken
            );
            read += result.Count;
        } while (!result.EndOfMessage);
        
        return (result, read);
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
            if (_socket.State is WebSocketState.Closed 
                or WebSocketState.CloseReceived
                or WebSocketState.CloseSent
                or WebSocketState.Aborted)
            {
                return;
            }
            
            await Task.Yield();
        }
     
        await _sendLock.WaitAsync(stoppingToken);

        try
        {
            await _socket.SendAsync(
                buffer,
                WebSocketMessageType.Binary,
                WebSocketMessageFlags.EndOfMessage,
                stoppingToken
            );
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public async Task Close()
    {
        if (_socket.State != WebSocketState.Open)
        {
            await _abortCts.CancelAsync();
            return;
        }
        
        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        await _abortCts.CancelAsync();
    }
}
