namespace Tests.TestHelpers;

// Redirects Console.In/Console.Out for the duration of a test, so the CLI views can be
// fed scripted input and their output inspected. When the input lines run out,
// Console.ReadLine() returns null - exactly like when a real user ends the input stream.
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
