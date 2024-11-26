using AnalyzeCodeBase.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace AnalyzeCodeBase;

public class SolutionAnalyzer : ISolutionAnalyzer
{
    public async Task<(int interfaceCount, int classCount, int methodCount)> AnalyzeSolutionAsync(Solution solution)
    {
        int classCount = 0;
        int interfaceCount = 0;
        int methodCount = 0;

        foreach (var project in solution.Projects)
        {
            Console.WriteLine();
            Console.WriteLine($"Compiling project {project.Name}...");
            var compilation = await project.GetCompilationAsync();
            if (compilation == null)
            {
                continue;
            }

            Console.WriteLine($"Getting syntax trees for project {project.Name}...");
            var syntaxTrees = compilation.SyntaxTrees;

            Console.WriteLine($"Starting to analyze interfaces for project {project.Name}...");
            interfaceCount += await FindInterfaceReferences(solution, compilation, syntaxTrees);

            Console.WriteLine($"Starting to analyze classes for project {project.Name}...");
            classCount += await FindClassReferences(solution, compilation, syntaxTrees);

            Console.WriteLine($"Starting to analyze methods for project {project.Name}...");
            methodCount += await FindMethodReferences(solution, compilation, syntaxTrees);
        }

        return (interfaceCount, classCount, methodCount);
    }

    private static async Task<int> FindClassReferences(Solution solution, Compilation compilation, IEnumerable<SyntaxTree> syntaxTrees)
    {
        int classCount = 0;
        var classes = syntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

        foreach (var @class in classes)
        {
            if (@class.Identifier.Text.Contains("Extensions") ||
                @class.Identifier.Text.Contains("Tests") ||
                @class.Identifier.Text.Contains("Page") ||
                @class.Identifier.Text.Contains("Imports") ||
                @class.Identifier.Text.Contains("Program"))
            {
                continue;
            }

            var model = compilation.GetSemanticModel(@class.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(@class);
            if (symbol == null)
            {
                continue;
            }

            var references = await SymbolFinder.FindReferencesAsync(symbol, solution);
            var count = references.Aggregate(0, (total, reference) => total + reference.Locations.Count());
            if (count == 0)
            {
                var filePath = @class.SyntaxTree.FilePath;
                Console.WriteLine($"Class '{@class.Identifier}' in file '{filePath}' has no references.");
                classCount++;
            }
        }

        return classCount;
    }

    private static async Task<int> FindInterfaceReferences(Solution solution, Compilation compilation, IEnumerable<SyntaxTree> syntaxTrees)
    {
        int interfaceCount = 0;
        var interfaces = syntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>());

        foreach (var @interface in interfaces)
        {
            var model = compilation.GetSemanticModel(@interface.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(@interface);
            if (symbol == null)
            {
                continue;
            }

            var references = await SymbolFinder.FindReferencesAsync(symbol, solution);
            var count = references.Aggregate(0, (total, reference) => total + reference.Locations.Count());
            if (count == 0)
            {
                var filePath = @interface.SyntaxTree.FilePath;
                Console.WriteLine($"Interface '{@interface.Identifier}' in file '{filePath}' has no references.");
                interfaceCount++;
            }
        }

        return interfaceCount;
    }

    private static async Task<int> FindMethodReferences(Solution solution, Compilation compilation, IEnumerable<SyntaxTree> syntaxTrees)
    {
        int methodCount = 0;
        var methods = syntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>());

        foreach (var method in methods)
        {
            if (method.Identifier.Text.Contains("OnInitialized") ||
                method.Identifier.Text.Contains("OnInitializedAsync") ||
                method.Identifier.Text.Contains("OnAfterRender") ||
                method.Identifier.Text.Contains("OnAfterRenderAsync") ||
                method.Identifier.Text.Contains("Dispose") ||
                method.Identifier.Text.Contains("Equals") ||
                method.Identifier.Text.Contains("GetHashCode") ||
                method.Identifier.Text.Contains("Main") ||
                method.Identifier.Text.Contains("BuildRenderTree") ||
                method.Identifier.Text.Contains("Configure") ||
                method.Identifier.Text.Contains("ConfigureServices") ||
                method.Identifier.Text.Contains("Compare") ||
                method.Identifier.Text.Contains('_') ||
                method.Identifier.Text.Contains("ExecuteAsync") ||
                method.Identifier.Text.Contains("GetByIDAsync") ||
                method.Identifier.Text.Contains("OnGet"))
            {
                continue;
            }

            var model = compilation.GetSemanticModel(method.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(method);
            if (symbol == null)
            {
                continue;
            }

            var references = await SymbolFinder.FindReferencesAsync(symbol, solution);
            var count = references.Aggregate(0, (total, reference) => total + reference.Locations.Count());
            if (count == 0)
            {
                // Find the class that the method is in
                var classDeclaration = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                var className = classDeclaration?.Identifier.Text ?? "Unknown";
                var filePath = @method.SyntaxTree.FilePath;
                Console.WriteLine($"Method '{method.Identifier}' in class '{className}' in file '{filePath}' has no references.");
                methodCount++;
            }
        }

        return methodCount;
    }
}