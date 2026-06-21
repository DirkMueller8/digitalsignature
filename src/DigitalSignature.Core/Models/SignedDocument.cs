namespace DigitalSignature.Core.Models;

public sealed record SignedDocument(
    string DocumentPath,
    byte[] DocumentHash,
    byte[] Signature,
    string Algorithm);
