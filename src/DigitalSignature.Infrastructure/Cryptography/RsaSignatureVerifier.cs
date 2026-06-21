using System.Security.Cryptography;
using DigitalSignature.Core.Abstractions;
using DigitalSignature.Core.Models;

namespace DigitalSignature.Infrastructure.Cryptography;

public sealed class RsaSignatureVerifier : ISignatureVerifier
{
    public VerificationResult Verify(string documentPath, byte[] signature, string publicKeyPem)
    {
        var documentBytes = File.ReadAllBytes(documentPath);
        var hash = SHA256.HashData(documentBytes);
        var hashHex = Convert.ToHexString(hash).ToLowerInvariant();

        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);

        var isValid = rsa.VerifyData(
            documentBytes,
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return isValid
            ? new VerificationResult(true, "Signature is valid. Authenticity and integrity confirmed.", hashHex)
            : new VerificationResult(false, "Signature verification FAILED. Document may have been tampered with or the key is wrong.", hashHex);
    }
}
