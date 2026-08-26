using Microsoft.Extensions.Logging;
using Puchitto.Server.Management;
using Puchitto.Server.Sample;

var server = PuchittoServer.CreateBuilder()
    .UseRules<SampleGameServerRules>()
    .Listen("http://localhost:8080/")
    .ConfigureLogging(opts =>
    {
        opts.AddConsole();
    })
    .Build();

await server.Host();