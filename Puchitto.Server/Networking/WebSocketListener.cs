using System.Net;
using Microsoft.Extensions.Logging;
using Puchitto.Server.Clients;

namespace Puchitto.Server.Networking;

/// <summary>
/// The listener class that starts up the HTTP server.
/// </summary>
public class WebSocketListener
{
    /// <summary>
    /// Called when a client connects.
    /// </summary>
    public Func<ClientConnection, Task>? OnClientConnected { get; set; }
    
    /// <summary>
    /// The HTTP listener.
    /// </summary>
    private readonly HttpListener _httpListener;
    
    /// <summary>
    /// The WebSocket listener logger.
    /// </summary>
    private readonly ILogger<WebSocketListener> _logger;
    
    /// <summary>
    /// Constructs a new web socket listener with the given list of prefixes.
    /// </summary>
    /// <param name="prefixes">
    /// The list of prefixes.
    /// </param>
    /// <param name="logger">
    /// The supplied logger.
    /// </param>
    public WebSocketListener(
        string[] prefixes,
        ILogger<WebSocketListener> logger)
    {
        _httpListener = new HttpListener();
        _logger = logger;
        
        foreach (var prefix in prefixes)
        {
            _httpListener.Prefixes.Add(prefix);
        }
    }

    /// <summary>
    /// Listens for connections.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    public async Task Listen(CancellationToken token = default)
    {
        _logger.LogInformation("Started listening.");
        _httpListener.Start();

        while (!token.IsCancellationRequested)
        {
            var context = await _httpListener.GetContextAsync();
            _logger.LogInformation("Accepted new connection from {Info}.", context.Request.RemoteEndPoint);
            
            if (context.Request.IsWebSocketRequest)
            {
                await HandleIncomingWebSocketClient(context);
            }
            else
            {
                await ReturnInvalidResponse(context);
            }
        }
    }
    
    /// <summary>
    /// Handles an incoming WebSocket client.
    /// </summary>
    /// <param name="context">The HTTP listener context.</param>
    private async Task HandleIncomingWebSocketClient(HttpListenerContext context)
    {
        var webSocketContext = await context.AcceptWebSocketAsync("puchitto");
        var webSocket = webSocketContext.WebSocket;
        _logger.LogInformation("Accepted new WebSocket client.");

        if (OnClientConnected is not null)
        {
            await OnClientConnected.Invoke(new ClientConnection(webSocket));
        }
    }

    /// <summary>
    /// Returns an invalid response if the incoming connection isn't a WebSocket connection.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    private async Task ReturnInvalidResponse(HttpListenerContext context)
    {
        const string errorMessage = "I am but a puchitto server.";
        
        await using (var sw = new StreamWriter(context.Response.OutputStream))
        {
            await sw.WriteLineAsync(errorMessage);
            context.Response.StatusCode = 419;
        }

        context.Response.Close();
    }
}