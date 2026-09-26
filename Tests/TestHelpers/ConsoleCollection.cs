using Xunit;

namespace Tests.TestHelpers;

// Console er global tilstand for hele processen. xUnit kører testklasser parallelt som
// standard, så hver testklasse, der omdirigerer konsollen, hører til denne collection, som
// kører for sig selv - ellers kunne to tests overskrive hinandens Console.In/Out.
[CollectionDefinition(Name, DisableParallelization = true)]
public class ConsoleCollection
{
    public const string Name = "Console";
}
