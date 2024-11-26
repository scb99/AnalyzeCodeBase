using AnalyzeCodeBase.Interfaces;

namespace AnalyzeCodeBase;

public class ProgramTerminator(ICrossCuttingConditionalCodeService execute) : IProgramTerminator
{
    private ICrossCuttingConditionalCodeService Execute { get; set; } = execute;

    public void EndProgram()
    {
        Console.WriteLine("Press any key to exit...");
        if (Execute.ConditionalCode())
        {
            Console.ReadKey();
        }
    }
}