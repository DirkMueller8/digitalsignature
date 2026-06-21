using DigitalSignature.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalSignature.Cli.Commands;

public sealed class HelpCommandHandler(
    IServiceProvider serviceProvider,
    IConsoleWriter writer) : ICommandHandler
{
    public string CommandName => "help";
    public string Description => "Display usage information for all commands";

    public Task ExecuteAsync(string[] args)
    {
        writer.WriteHeader("DIGITAL SIGNATURE CLI — HELP");
        writer.WriteKeyValue("Purpose:", "Simulate RSA-based digital signature workflows");
        writer.WriteKeyValue("Article:", "Electronic Signatures: Authenticity & Integrity in Practice");
        writer.WriteLine();

        writer.WriteLine("Available commands:");
        writer.WriteSeparator();

        var handlers = serviceProvider
            .GetServices<ICommandHandler>()
            .OrderBy(h => h.CommandName);

        foreach (var handler in handlers)
            writer.WriteKeyValue($"  {handler.CommandName}", handler.Description);

        writer.WriteSeparator();
        writer.WriteLine();
        writer.WriteLine("Usage examples:");
        writer.WriteCode("  digitalsignature demo");
        writer.WriteCode("  digitalsignature generate ./keys");
        writer.WriteCode("  digitalsignature sign contract.pdf ./keys/private-key.pem");
        writer.WriteCode("  digitalsignature verify contract.pdf contract.pdf.sig ./keys/public-key.pem");
        writer.WriteLine();
        writer.WriteInfo("Run 'demo' for a fully-guided walkthrough of the signing workflow.");

        return Task.CompletedTask;
    }
}
