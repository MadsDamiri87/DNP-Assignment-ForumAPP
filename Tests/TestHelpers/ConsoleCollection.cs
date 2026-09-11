using Xunit;

namespace Tests.TestHelpers;

// Console is global state for the whole process. xUnit runs test classes in parallel by
// default, so every test class that redirects the console joins this collection, which
// runs on its own - otherwise two tests could overwrite each other's Console.In/Out.
[CollectionDefinition(Name, DisableParallelization = true)]
public class ConsoleCollection
{
    public const string Name = "Console";
}
