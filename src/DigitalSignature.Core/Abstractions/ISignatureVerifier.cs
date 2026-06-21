using DigitalSignature.Core.Models;

namespace DigitalSignature.Core.Abstractions;

public interface ISignatureVerifier
{
    VerificationResult Verify(string documentPath, byte[] signature, string publicKeyPem);
}
