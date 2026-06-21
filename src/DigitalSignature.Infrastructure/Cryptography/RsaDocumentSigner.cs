using System.Security.Cryptography;
using DigitalSignature.Core.Abstractions;
using DigitalSignature.Core.Models;

namespace DigitalSignature.Infrastructure.Cryptography;

public sealed class RsaDocumentSigner : IDocumentSigner
{
    public SignedDocument Sign(string documentPath, string privateKeyPem)
    {
        var documentBytes = File.ReadAllBytes(documentPath);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var hash = SHA256.HashData(documentBytes);
        var signature = rsa.SignData(documentBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return new SignedDocument(
            DocumentPath: documentPath,
            DocumentHash: hash,
            Signature: signature,
            Algorithm: "SHA256withRSA");
    }
}
