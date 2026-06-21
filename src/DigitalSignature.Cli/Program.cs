namespace DigitalSignature.Cli;

internal sealed class Program
{
    private static async Task<int> Main(string[] args)
    {
        var app = App.Build();
        return await app.RunAsync(args);
    }
}
