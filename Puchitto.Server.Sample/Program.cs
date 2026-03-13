using Microsoft.Extensions.Logging;
using Puchitto.Server.Management;
using Puchitto.Server.Sample;
using Puchitto.Server.Scripting;

var config = new PuchittoServerConfig
{
    Prefixes = ["http://localhost:8080/"],
    LoggingBuilder = (opts) =>
    {
        opts.AddConsole();
    }
};

var server = new PuchittoServer<SampleGameServerRules>(config);
await server.Host();