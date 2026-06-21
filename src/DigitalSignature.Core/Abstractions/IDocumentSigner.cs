using DigitalSignature.Core.Models;

namespace DigitalSignature.Core.Abstractions;

public interface IDocumentSigner
{
    SignedDocument Sign(string documentPath, string privateKeyPem);
}
