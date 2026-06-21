namespace DigitalSignature.Core.Abstractions;

public interface IConsoleWriter
{
    void WriteHeader(string title);
    void WriteSubHeader(string title);
    void WriteSuccess(string text);
    void WriteError(string text);
    void WriteWarning(string text);
    void WriteInfo(string text);
    void WriteStep(int number, string description);
    void WriteKeyValue(string key, string value);
    void WriteCode(string text);
    void WriteSeparator();
    void WriteLine(string text = "");
    void WriteBoxLine(string text);
    void WriteBoxTop();
    void WriteBoxBottom();
}
