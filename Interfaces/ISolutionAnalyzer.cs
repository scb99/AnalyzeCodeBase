using Microsoft.CodeAnalysis;

namespace AnalyzeCodeBase.Interfaces;

public interface ISolutionAnalyzer
{
    Task<(int interfaceCount, int classCount, int methodCount)> AnalyzeSolutionAsync(Solution solution);
}