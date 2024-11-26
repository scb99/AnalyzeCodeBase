using AnalyzeCodeBase.Interfaces;

namespace AnalyzeCodeBase;

public class ResultReporter : IResultReporter
{
    public void ReportResults(int interfaceCount, int classCount, int methodCount)
    {
        Console.WriteLine();
        Console.WriteLine($"Found {interfaceCount} interface{(interfaceCount == 1 ? "" : "s")} with no references.");
        Console.WriteLine($"Found {classCount} class{(classCount == 1 ? "" : "es")} with no references.");
        Console.WriteLine($"Found {methodCount} method{(methodCount == 1 ? "" : "s")} with no references.");
        Console.WriteLine();
    }
}