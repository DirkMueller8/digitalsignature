using System.Security.Cryptography;
using DigitalSignature.Core.Abstractions;

namespace DigitalSignature.Cli.Commands;

public sealed class DemoCommandHandler(
    IKeyGenerator keyGenerator,
    IDocumentSigner signer,
    ISignatureVerifier verifier,
    IConsoleWriter writer) : ICommandHandler
{
    private const string OutputDir = "demo-output";

    public string CommandName => "demo";
    public string Description => "Run a full end-to-end digital signature demonstration";

    public async Task ExecuteAsync(string[] args)
    {
        Directory.CreateDirectory(OutputDir);

        WriteIntroduction();
        await Task.Delay(400);

        var documentPath         = await StepCreateDocument();
        await Task.Delay(400);

        var (privateKeyPem, publicKeyPem) = await StepGenerateKeys();
        await Task.Delay(400);

        var signed = await StepSignDocument(documentPath, privateKeyPem);
        await Task.Delay(400);

        await StepVerifySignature(documentPath, signed.Signature, publicKeyPem, expectSuccess: true);
        await Task.Delay(400);

        var tamperedPath = await StepTamperDocument(documentPath, signed.DocumentHash);
        await Task.Delay(400);

        await StepVerifySignature(tamperedPath, signed.Signature, publicKeyPem, expectSuccess: false);
        await Task.Delay(200);

        WriteSummary(signed.DocumentHash, signed.Signature);
    }

    // ──────────────────────────────────────────────────────────────────────────

    private void WriteIntroduction()
    {
        writer.WriteHeader("DIGITAL SIGNATURE: AUTHENTICITY & INTEGRITY IN PRACTICE");
        writer.WriteLine();
        writer.WriteWarning("KEY INSIGHT: Digital signatures are NOT encryption.");
        writer.WriteSeparator();
        writer.WriteKeyValue("Encryption  →", "Confidentiality  (controls who can READ a message)");
        writer.WriteKeyValue("Signatures  →", "Authenticity     (proves WHO sent it)");
        writer.WriteKeyValue("            →", "Integrity        (proves it was NOT tampered with)");
        writer.WriteSeparator();
    }

    private async Task<string> StepCreateDocument()
    {
        writer.WriteStep(1, "Create Sample Document (NDA)");

        var documentPath = Path.Combine(OutputDir, "nda.txt");
        var content = BuildNdaContent();

        await File.WriteAllTextAsync(documentPath, content);

        writer.WriteKeyValue("File:", documentPath);
        writer.WriteLine();
        writer.WriteSeparator();
        foreach (var line in content.Split('\n'))
            writer.WriteCode(line.TrimEnd());
        writer.WriteSeparator();

        return documentPath;
    }

    private async Task<(string PrivateKeyPem, string PublicKeyPem)> StepGenerateKeys()
    {
        writer.WriteStep(2, "Generate RSA 3072-bit Key Pair (Sender: Alice)");

        writer.WriteKeyValue("Algorithm:", "RSA (Rivest-Shamir-Adleman)");
        writer.WriteKeyValue("Key size:", "3072 bits");
        writer.WriteLine();

        var keyPair = keyGenerator.Generate(3072);

        var privatePath = Path.Combine(OutputDir, "private-key.pem");
        var publicPath  = Path.Combine(OutputDir, "public-key.pem");

        await File.WriteAllTextAsync(privatePath, keyPair.PrivateKeyPem);
        await File.WriteAllTextAsync(publicPath,  keyPair.PublicKeyPem);

        writer.WriteSuccess($"Private key saved → {privatePath}");
        writer.WriteWarning("Alice keeps the private key SECRET — never shared!");
        writer.WriteSuccess($"Public key saved  → {publicPath}");
        writer.WriteInfo("Alice shares the public key with recipients like Bob.");
        writer.WriteLine();

        writer.WriteLine("Public key (PEM, first two lines shown):");
        writer.WriteSeparator();
        var pemLines = keyPair.PublicKeyPem.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        writer.WriteCode(pemLines[0]);
        writer.WriteCode(pemLines[1][..Math.Min(50, pemLines[1].Length)] + "...");
        writer.WriteCode(pemLines[^1]);
        writer.WriteSeparator();

        return (keyPair.PrivateKeyPem, keyPair.PublicKeyPem);
    }

    private async Task<Core.Models.SignedDocument> StepSignDocument(string documentPath, string privateKeyPem)
    {
        writer.WriteStep(3, "Alice Signs the Document");

        writer.WriteInfo("Computing SHA-256 hash of nda.txt...");
        writer.WriteInfo("Signing hash with Alice's private key (RSA-PKCS1)...");
        writer.WriteLine();

        var signed = signer.Sign(documentPath, privateKeyPem);
        var sigPath = Path.Combine(OutputDir, "nda.sig");

        await File.WriteAllBytesAsync(sigPath, signed.Signature);

        var hashHex   = Convert.ToHexString(signed.DocumentHash).ToLowerInvariant();
        var sigBase64 = Convert.ToBase64String(signed.Signature);

        writer.WriteKeyValue("Operation:", "σ = RSA-Sign( private-key, SHA-256( nda.txt ) )");
        writer.WriteLine();
        writer.WriteKeyValue("H(nda.txt):", hashHex);
        writer.WriteLine();
        writer.WriteLine("Signature σ (Base64, first 64 chars):");
        writer.WriteSeparator();
        writer.WriteCode(sigBase64[..Math.Min(64, sigBase64.Length)] + "...");
        writer.WriteSeparator();
        writer.WriteLine();
        writer.WriteSuccess($"Signature saved → {sigPath}");
        writer.WriteInfo("Alice sends nda.txt + nda.sig to Bob.");

        return signed;
    }

    private async Task StepVerifySignature(
        string documentPath,
        byte[] signature,
        string publicKeyPem,
        bool expectSuccess)
    {
        var stepTitle = expectSuccess
            ? "Bob Verifies the Signature"
            : "Bob Verifies the Tampered Document";
        var stepNumber = expectSuccess ? 4 : 6;

        writer.WriteStep(stepNumber, stepTitle);

        writer.WriteInfo("Bob recomputes H'(document) using the received file...");
        writer.WriteInfo("Bob verifies: RSA-Verify( public-key, H'(document), σ )");
        writer.WriteLine();

        var result = verifier.Verify(documentPath, signature, publicKeyPem);

        writer.WriteKeyValue("H'(document):", result.DocumentHash);
        writer.WriteLine();

        if (result.IsValid)
        {
            writer.WriteSuccess("AUTHENTICITY CONFIRMED — Document was signed by Alice's private key.");
            writer.WriteSuccess("INTEGRITY CONFIRMED   — Document has not been altered since signing.");
        }
        else
        {
            writer.WriteError("INTEGRITY VIOLATION! The signature verification FAILED.");
            writer.WriteError("The document was modified after signing.");
            writer.WriteInfo("Signatures cannot be forged without the private key.");
        }

        await Task.CompletedTask;
    }

    private async Task<string> StepTamperDocument(string originalPath, byte[] originalHash)
    {
        writer.WriteStep(5, "Attack Simulation — Eve Tampers with the Document");

        var originalContent = await File.ReadAllTextAsync(originalPath);
        var tamperedContent = originalContent.Replace(
            "an amount agreed upon by both parties",
            "$0 (zero dollars — forged by Eve)");

        var tamperedPath = Path.Combine(OutputDir, "nda-tampered.txt");
        await File.WriteAllTextAsync(tamperedPath, tamperedContent);

        var tamperedBytes = await File.ReadAllBytesAsync(tamperedPath);
        var tamperedHash  = Convert.ToHexString(SHA256.HashData(tamperedBytes)).ToLowerInvariant();
        var originalHex   = Convert.ToHexString(originalHash).ToLowerInvariant();

        writer.WriteWarning("Eve modifies the payment clause in nda.txt...");
        writer.WriteSuccess($"Tampered document saved → {tamperedPath}");
        writer.WriteLine();
        writer.WriteKeyValue("Original hash:", originalHex);
        writer.WriteKeyValue("Tampered hash:", tamperedHash);
        writer.WriteWarning("The hashes differ! Any change, however small, produces a completely different hash.");
        writer.WriteInfo("Eve cannot update the signature without Alice's private key.");

        return tamperedPath;
    }

    private void WriteSummary(byte[] hash, byte[] signature)
    {
        writer.WriteLine();
        writer.WriteBoxTop();
        writer.WriteBoxLine("  SUMMARY");
        writer.WriteBoxLine(new string('─', 64));
        writer.WriteBoxLine($"  Algorithm:    SHA-256withRSA (3072-bit key)");
        writer.WriteBoxLine($"  Hash size:    {hash.Length * 8} bits  ({hash.Length} bytes)");
        writer.WriteBoxLine($"  Sig size:     {signature.Length * 8} bits ({signature.Length} bytes)");
        writer.WriteBoxLine($"  Guarantees:   Authenticity ✓ | Integrity ✓ | Confidentiality ✗");
        writer.WriteBoxLine($"  Legal basis:  Cryptographic core of eIDAS qualified signatures");
        writer.WriteBoxLine(new string('─', 64));
        writer.WriteBoxLine("  Files created in demo-output/:");
        writer.WriteBoxLine("    nda.txt           — original document");
        writer.WriteBoxLine("    nda-tampered.txt  — Eve's forged document");
        writer.WriteBoxLine("    private-key.pem   — Alice's private key (never share!)");
        writer.WriteBoxLine("    public-key.pem    — Alice's public key (shareable)");
        writer.WriteBoxLine("    nda.sig           — cryptographic signature");
        writer.WriteBoxLine(new string('─', 64));
        writer.WriteBoxLine("  Next steps:");
        writer.WriteBoxLine("    digitalsignature generate              — generate a key pair");
        writer.WriteBoxLine("    digitalsignature sign <file>           — sign any document");
        writer.WriteBoxLine("    digitalsignature verify <f> <sig>      — verify a signature");
        writer.WriteBoxBottom();
    }

    private static string BuildNdaContent() =>
        """
        NON-DISCLOSURE AGREEMENT
        ========================

        Parties:  Alice Corp. ("Disclosing Party")
                  Bob Industries ("Receiving Party")

        Date:     2026-06-21

        1. CONFIDENTIALITY
           The Receiving Party agrees to keep confidential all proprietary
           information disclosed by the Disclosing Party.

        2. COMPENSATION
           In exchange for access to said information, the Receiving Party
           shall pay an amount agreed upon by both parties.

        3. TERM
           This agreement shall remain in force for a period of two (2) years
           from the date of signing.

        Signed: Alice Corp.
        """;
}
