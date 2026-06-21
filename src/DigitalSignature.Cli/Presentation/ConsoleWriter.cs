using DigitalSignature.Core.Abstractions;

namespace DigitalSignature.Cli.Presentation;

public sealed class ConsoleWriter : IConsoleWriter
{
    private const int BoxWidth = 66;

    // C# 14 field keyword: compiler-generated backing field with guarded setter
    public int IndentLevel
    {
        get => field;
        set => field = int.Clamp(value, 0, 8);
    } = 0;

    public void WriteHeader(string title)
    {
        Console.WriteLine();
        WriteBoxTop();
        WriteBoxLine($"  {title}");
        WriteBoxBottom();
        Console.WriteLine();
    }

    public void WriteSubHeader(string title)
    {
        Console.WriteLine();
        var line = new string('─', BoxWidth);
        WriteColored($"┌{line}┐", ConsoleColor.DarkCyan);
        WriteColored($"│  {title.PadRight(BoxWidth - 2)}│", ConsoleColor.DarkCyan);
        WriteColored($"└{line}┘", ConsoleColor.DarkCyan);
    }

    public void WriteSuccess(string text)
    {
        WriteColored($"  ✓ {text}", ConsoleColor.Green);
    }

    public void WriteError(string text)
    {
        WriteColored($"  ✗ {text}", ConsoleColor.Red);
    }

    public void WriteWarning(string text)
    {
        WriteColored($"  ! {text}", ConsoleColor.Yellow);
    }

    public void WriteInfo(string text)
    {
        WriteColored($"  ℹ {text}", ConsoleColor.Cyan);
    }

    public void WriteStep(int number, string description)
    {
        Console.WriteLine();
        WriteSubHeader($"STEP {number}: {description}");
    }

    public void WriteKeyValue(string key, string value)
    {
        Console.Write("  ");
        WriteColored(key.PadRight(22), ConsoleColor.Gray, newLine: false);
        WriteColored(value, ConsoleColor.White);
    }

    public void WriteCode(string text)
    {
        WriteColored($"  {text}", ConsoleColor.DarkYellow);
    }

    public void WriteSeparator()
    {
        WriteColored("  " + new string('─', BoxWidth - 2), ConsoleColor.DarkGray);
    }

    public void WriteLine(string text = "")
    {
        Console.WriteLine(text);
    }

    public void WriteBoxTop()
    {
        WriteColored($"╔{new string('═', BoxWidth)}╗", ConsoleColor.DarkCyan);
    }

    public void WriteBoxBottom()
    {
        WriteColored($"╚{new string('═', BoxWidth)}╝", ConsoleColor.DarkCyan);
    }

    public void WriteBoxLine(string text)
    {
        WriteColored($"║{text.PadRight(BoxWidth)}║", ConsoleColor.DarkCyan);
    }

    private static void WriteColored(string text, ConsoleColor color, bool newLine = true)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        if (newLine)
            Console.WriteLine(text);
        else
            Console.Write(text);
        Console.ForegroundColor = previous;
    }
}
