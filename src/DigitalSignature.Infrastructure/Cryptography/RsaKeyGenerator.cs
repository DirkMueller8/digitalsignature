using System.Security.Cryptography;
using DigitalSignature.Core.Abstractions;
using DigitalSignature.Core.Models;

namespace DigitalSignature.Infrastructure.Cryptography;

public sealed class RsaKeyGenerator : IKeyGenerator
{
    public KeyPair Generate(int keySizeInBits = 3072)
    {
        using var rsa = RSA.Create(keySizeInBits);
        return new KeyPair(
            PrivateKeyPem: rsa.ExportRSAPrivateKeyPem(),
            PublicKeyPem: rsa.ExportRSAPublicKeyPem());
    }
}
