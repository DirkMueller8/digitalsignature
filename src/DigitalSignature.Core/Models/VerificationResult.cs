namespace DigitalSignature.Core.Models;

public sealed record VerificationResult(
    bool IsValid,
    string Message,
    string DocumentHash);
