using Microsoft.CodeAnalysis.MSBuild;

namespace AnalyzeCodeBase.Interfaces;

public interface IWorkspaceFactory
{
    MSBuildWorkspace CreateWorkspace();
}