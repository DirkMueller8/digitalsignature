using DigitalSignature.Core.Models;

namespace DigitalSignature.Core.Abstractions;

public interface IKeyGenerator
{
    KeyPair Generate(int keySizeInBits = 3072);
}
