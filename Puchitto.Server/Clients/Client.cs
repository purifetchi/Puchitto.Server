using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace Puchitto.Server.Clients;

public class Client
{
    /// <summary>
    /// The ID of this client.
    /// </summary>
    public Guid Id { get; }
    
    /// <summary>
    /// The connection for this client.
    /// </summary>
    public ClientConnection Connection { get; }
    
    public Client(
        Guid id,
        ClientConnection connection)
    {
        Console.WriteLine($"Creating new client...");
        Id = id;
        Connection = connection;
    }

    public async Task SendData(string data)
    {
        
    }

    public async Task Disconnect()
    {
        
    }
}