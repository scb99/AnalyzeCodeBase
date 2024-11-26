namespace AnalyzeCodeBase.Interfaces;

public interface IResultReporter
{
    void ReportResults(int interfaceCount, int classCount, int methodCount);
}