using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NHSOds.MCP.Services;
using NHSOds.MCP.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Route all logging to stderr so it doesn't corrupt the stdio MCP stream.
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Register the ODS typed HTTP client.
builder.Services
    .AddHttpClient<OdsApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://directory.spineservices.nhs.uk/ORD/2-0-0/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(15);
    });

// Register the MCP server with stdio transport and our tools.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<NHSOdsTools>();

await builder.Build().RunAsync();
