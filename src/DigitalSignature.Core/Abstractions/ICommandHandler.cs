namespace DigitalSignature.Core.Abstractions;

public interface ICommandHandler
{
    string CommandName { get; }
    string Description { get; }
    Task ExecuteAsync(string[] args);
}
