using DigitalSignature.Cli.DependencyInjection;
using DigitalSignature.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalSignature.Cli;

internal sealed class App
{
    private readonly IReadOnlyDictionary<string, ICommandHandler> _handlers;
    private readonly IConsoleWriter _writer;

    private App(IReadOnlyDictionary<string, ICommandHandler> handlers, IConsoleWriter writer)
    {
        _handlers = handlers;
        _writer   = writer;
    }

    public static App Build()
    {
        var services = new ServiceCollection();
        services.AddDigitalSignatureServices();

        var provider = services.BuildServiceProvider();
        var handlers = provider
            .GetServices<ICommandHandler>()
            .ToDictionary(h => h.CommandName, h => h);

        return new App(handlers, provider.GetRequiredService<IConsoleWriter>());
    }

    public async Task<int> RunAsync(string[] args)
    {
        var command    = args.Length > 0 ? args[0].ToLowerInvariant() : "help";
        var remaining  = args.Length > 1 ? args[1..] : [];

        if (!_handlers.TryGetValue(command, out var handler))
        {
            _writer.WriteError($"Unknown command: '{command}'");
            _writer.WriteInfo("Run 'digitalsignature help' to see available commands.");
            return 1;
        }

        try
        {
            await handler.ExecuteAsync(remaining);
            return 0;
        }
        catch (Exception ex)
        {
            _writer.WriteError($"Unexpected error: {ex.Message}");
            return 1;
        }
    }
}
