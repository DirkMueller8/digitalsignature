using DigitalSignature.Core.Abstractions;
using DigitalSignature.Cli.Commands;
using DigitalSignature.Cli.Presentation;
using DigitalSignature.Infrastructure.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalSignature.Cli.DependencyInjection;

public static class ServiceRegistry
{
    public static IServiceCollection AddDigitalSignatureServices(this IServiceCollection services)
    {
        services.AddSingleton<IConsoleWriter, ConsoleWriter>();

        services.AddSingleton<IKeyGenerator, RsaKeyGenerator>();
        services.AddSingleton<IDocumentSigner, RsaDocumentSigner>();
        services.AddSingleton<ISignatureVerifier, RsaSignatureVerifier>();

        services.AddSingleton<ICommandHandler, GenerateKeysCommandHandler>();
        services.AddSingleton<ICommandHandler, SignCommandHandler>();
        services.AddSingleton<ICommandHandler, VerifyCommandHandler>();
        services.AddSingleton<ICommandHandler, DemoCommandHandler>();
        services.AddSingleton<ICommandHandler, HelpCommandHandler>();

        return services;
    }
}
