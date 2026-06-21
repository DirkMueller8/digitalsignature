using DigitalSignature.Core.Abstractions;

namespace DigitalSignature.Cli.Commands;

public sealed class GenerateKeysCommandHandler(
    IKeyGenerator keyGenerator,
    IConsoleWriter writer) : ICommandHandler
{
    public string CommandName => "generate";
    public string Description => "Generate an RSA 3072-bit key pair and save to PEM files";

    public Task ExecuteAsync(string[] args)
    {
        var outputDir = args.Length > 0 ? args[0] : ".";
        var keySize = ParseKeySize(args);

        writer.WriteHeader("RSA KEY PAIR GENERATION");
        writer.WriteKeyValue("Algorithm:", "RSA");
        writer.WriteKeyValue("Key size:", $"{keySize} bits");
        writer.WriteKeyValue("Output directory:", outputDir);
        writer.WriteLine();

        Directory.CreateDirectory(outputDir);

        writer.WriteInfo("Generating key pair...");
        var keyPair = keyGenerator.Generate(keySize);

        var privatePath = Path.Combine(outputDir, "private-key.pem");
        var publicPath  = Path.Combine(outputDir, "public-key.pem");

        File.WriteAllText(privatePath, keyPair.PrivateKeyPem);
        File.WriteAllText(publicPath,  keyPair.PublicKeyPem);

        writer.WriteSuccess($"Private key saved → {privatePath}");
        writer.WriteWarning("Keep the private key secret — never share it!");
        writer.WriteSuccess($"Public key saved  → {publicPath}");
        writer.WriteInfo("The public key can be shared freely with recipients.");
        writer.WriteLine();

        writer.WriteLine("Public key (PEM):");
        writer.WriteSeparator();
        foreach (var line in keyPair.PublicKeyPem.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            writer.WriteCode(line.Trim());
        writer.WriteSeparator();

        return Task.CompletedTask;
    }

    private static int ParseKeySize(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i] is "--bits" or "-b" && int.TryParse(args[i + 1], out var size))
                return size;
        return 3072;
    }
}
