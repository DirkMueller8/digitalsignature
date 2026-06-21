using DigitalSignature.Core.Abstractions;

namespace DigitalSignature.Cli.Commands;

public sealed class VerifyCommandHandler(
    ISignatureVerifier verifier,
    IConsoleWriter writer) : ICommandHandler
{
    public string CommandName => "verify";
    public string Description => "Verify a document's signature using a public RSA key";

    public Task ExecuteAsync(string[] args)
    {
        if (args.Length < 2)
        {
            writer.WriteError("Usage: digitalsignature verify <document> <signature.sig> [<public-key.pem>]");
            return Task.CompletedTask;
        }

        var documentPath  = args[0];
        var signaturePath = args[1];
        var publicKeyPath = args.Length > 2 ? args[2] : "public-key.pem";

        if (!File.Exists(documentPath))
        {
            writer.WriteError($"Document not found: {documentPath}");
            return Task.CompletedTask;
        }

        if (!File.Exists(signaturePath))
        {
            writer.WriteError($"Signature file not found: {signaturePath}");
            return Task.CompletedTask;
        }

        if (!File.Exists(publicKeyPath))
        {
            writer.WriteError($"Public key not found: {publicKeyPath}");
            return Task.CompletedTask;
        }

        writer.WriteHeader("SIGNATURE VERIFICATION");
        writer.WriteKeyValue("Document:", documentPath);
        writer.WriteKeyValue("Signature:", signaturePath);
        writer.WriteKeyValue("Public key:", publicKeyPath);
        writer.WriteKeyValue("Algorithm:", "SHA-256 with RSA");
        writer.WriteLine();

        var signature    = File.ReadAllBytes(signaturePath);
        var publicKeyPem = File.ReadAllText(publicKeyPath);

        writer.WriteInfo("Recomputing document hash (SHA-256)...");
        writer.WriteInfo("Verifying signature against hash using public key...");
        writer.WriteLine();

        var result = verifier.Verify(documentPath, signature, publicKeyPem);

        writer.WriteKeyValue("Document hash (SHA-256):", result.DocumentHash);
        writer.WriteLine();

        if (result.IsValid)
        {
            writer.WriteSuccess("AUTHENTICITY CONFIRMED — Document was signed by the private key holder.");
            writer.WriteSuccess("INTEGRITY CONFIRMED   — Document has not been altered since signing.");
        }
        else
        {
            writer.WriteError("AUTHENTICITY / INTEGRITY VIOLATION!");
            writer.WriteError(result.Message);
        }

        return Task.CompletedTask;
    }
}
