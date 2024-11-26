using AnalyzeCodeBase.Interfaces;
using Microsoft.CodeAnalysis.MSBuild;

namespace AnalyzeCodeBase;

public class WorkspaceFactory : IWorkspaceFactory
{
    public MSBuildWorkspace CreateWorkspace()
    {
        Console.WriteLine("Create MSBuild workspace...");
        return MSBuildWorkspace.Create();
    }
}