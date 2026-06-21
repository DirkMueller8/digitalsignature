using DigitalSignature.Core.Abstractions;

namespace DigitalSignature.Cli.Commands;

public sealed class SignCommandHandler(
    IDocumentSigner signer,
    IConsoleWriter writer) : ICommandHandler
{
    public string CommandName => "sign";
    public string Description => "Sign a document using a private RSA key";

    public Task ExecuteAsync(string[] args)
    {
        if (args.Length < 1)
        {
            writer.WriteError("Usage: digitalsignature sign <document> [<private-key.pem>]");
            return Task.CompletedTask;
        }

        var documentPath  = args[0];
        var privateKeyPath = args.Length > 1 ? args[1] : "private-key.pem";

        if (!File.Exists(documentPath))
        {
            writer.WriteError($"Document not found: {documentPath}");
            return Task.CompletedTask;
        }

        if (!File.Exists(privateKeyPath))
        {
            writer.WriteError($"Private key not found: {privateKeyPath}");
            writer.WriteInfo("Run 'digitalsignature generate' first to create a key pair.");
            return Task.CompletedTask;
        }

        writer.WriteHeader("DOCUMENT SIGNING");
        writer.WriteKeyValue("Document:", documentPath);
        writer.WriteKeyValue("Private key:", privateKeyPath);
        writer.WriteKeyValue("Algorithm:", "SHA-256 with RSA");
        writer.WriteLine();

        var privateKeyPem = File.ReadAllText(privateKeyPath);

        writer.WriteInfo("Computing SHA-256 hash of document...");
        writer.WriteInfo("Signing hash with private key...");

        var result     = signer.Sign(documentPath, privateKeyPem);
        var sigPath    = documentPath + ".sig";
        var hashHex    = Convert.ToHexString(result.DocumentHash).ToLowerInvariant();
        var sigBase64  = Convert.ToBase64String(result.Signature);

        File.WriteAllBytes(sigPath, result.Signature);

        writer.WriteLine();
        writer.WriteKeyValue("Document hash (SHA-256):", hashHex);
        writer.WriteKeyValue("Signature size:", $"{result.Signature.Length} bytes");
        writer.WriteLine();
        writer.WriteLine("Signature (Base64):");
        writer.WriteSeparator();
        foreach (var chunk in ChunkBase64(sigBase64, 64))
            writer.WriteCode(chunk);
        writer.WriteSeparator();
        writer.WriteLine();
        writer.WriteSuccess($"Signature saved → {sigPath}");
        writer.WriteInfo("Send the document and signature file to the recipient.");
        writer.WriteInfo("The recipient uses your public key to verify both.");

        return Task.CompletedTask;
    }

    private static IEnumerable<string> ChunkBase64(string value, int chunkSize)
    {
        for (var i = 0; i < value.Length; i += chunkSize)
            yield return value.Substring(i, Math.Min(chunkSize, value.Length - i));
    }
}
