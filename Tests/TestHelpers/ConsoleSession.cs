namespace Tests.TestHelpers;

// Omdirigerer Console.In/Console.Out, mens en test kører, så CLI-viewsene kan få
// scriptet input, og deres output kan undersøges. Når inputlinjerne løber tør,
// returnerer Console.ReadLine() null - ligesom når en rigtig bruger afslutter input-strømmen.
public sealed class ConsoleSession : IDisposable
{
    private readonly TextReader originalIn;
    private readonly TextWriter originalOut;
    private readonly StringWriter output = new();

    public ConsoleSession(params string[] inputLines)
    {
        originalIn = Console.In;
        originalOut = Console.Out;

        string input = inputLines.Length == 0
            ? ""
            : string.Join(Environment.NewLine, inputLines) + Environment.NewLine;

        Console.SetIn(new StringReader(input));
        Console.SetOut(output);
    }

    public string Output => output.ToString();

    public void Dispose()
    {
        Console.SetIn(originalIn);
        Console.SetOut(originalOut);
    }
}
