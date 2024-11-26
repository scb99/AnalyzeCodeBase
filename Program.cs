using AnalyzeCodeBase.Interfaces;
using AnalyzeCodeBase.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Diagnostics;

namespace AnalyzeCodeBase;

public class Program(IWorkspaceFactory workspaceFactory, ISolutionAnalyzer solutionAnalyzer, IResultReporter resultReporter, IProgramTerminator programTerminator)
{
    private readonly IWorkspaceFactory _workspaceFactory = workspaceFactory;
    private readonly ISolutionAnalyzer _solutionAnalyzer = solutionAnalyzer;
    private readonly IResultReporter _resultReporter = resultReporter;
    private readonly IProgramTerminator _programTerminator = programTerminator;

    public static async Task Main()
    {
        var program = new Program(new WorkspaceFactory(), new SolutionAnalyzer(), new ResultReporter(), new ProgramTerminator(new ConditionalCodeService()));
        await program.RunAsync();
    }

    public async Task RunAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        MSBuildWorkspace workspace = _workspaceFactory.CreateWorkspace();
        Solution solution = await OpenSolutionAsync(workspace);

        var (interfaceCount, classCount, methodCount) = await _solutionAnalyzer.AnalyzeSolutionAsync(solution);

        _resultReporter.ReportResults(interfaceCount, classCount, methodCount);

        stopwatch.Stop();
        Console.WriteLine($"Program execution time: {stopwatch.Elapsed}");

        _programTerminator.EndProgram();
    }

    private static async Task<Solution> OpenSolutionAsync(MSBuildWorkspace workspace)
    {
        Console.WriteLine("Open solution...");

        // Note: The path to the solution file is hard-coded here for simplicity.
        // And the paths are different on different machines.
        string? filePath = "C:\\Users\\bruel\\Sync\\STPC Web App\\DBExplorerBlazorSLN3\\DBExplorerBlazor3\\DBExplorerBlazor3.sln";
        if (File.Exists(filePath))
        {
            var solution1 = await workspace.OpenSolutionAsync(filePath);
            Console.WriteLine("Solution opened.");
            return solution1;
        }

        filePath = "C:\\Users\\bruell\\Sync\\STPC Web App\\DBExplorerBlazorSLN3\\DBExplorerBlazor3\\DBExplorerBlazor3.sln";
        var solution = await workspace.OpenSolutionAsync(filePath);
        Console.WriteLine("Solution opened.");
        return solution;
    }
}