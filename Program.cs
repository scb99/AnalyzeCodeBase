using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

namespace AnalyzeCodeBase;

class Program
{
    static async Task Main()
    {
        int classCount, methodCount, interfaceCount;
       
        MSBuildWorkspace workspace = CreateWorkSpace();

        Solution solution = await OpenSolutionAsync(workspace);

        (interfaceCount, classCount, methodCount) = await AnalyzeSolutionAsync(solution);

        ReportResults(interfaceCount, classCount, methodCount);

        EndProgram();
    }

    private static void EndProgram()
    {
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void ReportResults(int interfaceCount, int classCount, int methodCount)
    {
        Console.WriteLine();
        Console.WriteLine($"Found {interfaceCount} interfaces with zero references.");
        Console.WriteLine($"Found {classCount} classes with zero references.");
        Console.WriteLine($"Found {methodCount} methods with zero references.");
        Console.WriteLine();
    }

    private static async Task<Tuple<int, int, int>> AnalyzeSolutionAsync(Solution solution)
    {
        int classCount = 0;
        int interfaceCount = 0;
        int methodCount = 0;

        foreach (var project in solution.Projects)
        {
            Console.WriteLine($"Compiling project {project.Name}...");
            var compilation = await project.GetCompilationAsync();
            if (compilation == null)
            {
                continue;
            }

            Console.WriteLine($"Getting syntax trees for project {project.Name}...");
            var syntaxTrees = compilation.SyntaxTrees;

            // Output names of methods with zero references
            Console.WriteLine($"Starting to analyze interfaces for project {project.Name}...");
            interfaceCount = await FindInterfaceReferences(solution, interfaceCount, compilation, syntaxTrees);
            Console.WriteLine();

            // Output names of classes with zero references
            Console.WriteLine($"Starting to analyze classes for project {project.Name}...");
            classCount = await FindClassReferences(solution, classCount, compilation, syntaxTrees);
            Console.WriteLine();

            // Output names of methods with zero references
            Console.WriteLine($"Starting to analyze methods for project {project.Name}...");
            methodCount = await FindMethodReferences(solution, methodCount, compilation, syntaxTrees);
            Console.WriteLine();
        }

        return Tuple.Create(interfaceCount, classCount, methodCount);
    }

    private static async Task<Solution> OpenSolutionAsync(MSBuildWorkspace workspace)
    {
        Console.WriteLine("Open solution...");
        var solution = await workspace.OpenSolutionAsync("C:\\Users\\bruel\\Sync\\STPC Web App\\DBExplorerBlazorSLN3\\DBExplorerBlazor3\\DBExplorerBlazor3.sln");
        Console.WriteLine("Solution opened.");
        Console.WriteLine();
        return solution;
    }

    private static MSBuildWorkspace CreateWorkSpace()
    {
        Console.WriteLine("Create MSBuild workspace...");
        var workspace = MSBuildWorkspace.Create();
        return workspace;
    }

    private static async Task<int> FindClassReferences(Solution solution, int classCount, Compilation? compilation, IEnumerable<SyntaxTree> syntaxTrees)
    {
        var classes = syntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

        foreach (var @class in classes)
        {
            if (@class.Identifier.Text.Contains("Extensions") || (@class.Identifier.Text.Contains("Tests")) || (@class.Identifier.Text.Contains("Page")) ||
               (@class.Identifier.Text.Contains("Imports")) || (@class.Identifier.Text.Contains("Program")))
            {
                continue;
            }

            var model = compilation?.GetSemanticModel(@class.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(@class);
            if (symbol == null)
            {
                continue;
            }

            var references = await SymbolFinder.FindReferencesAsync(symbol, solution);
            var count = references.Aggregate(0, (total, reference) => total + reference.Locations.Count());
            if (count == 0)
            {
                Console.WriteLine($"Class '{@class.Identifier}' has zero references.");
                classCount++;
            }
        }

        return classCount;
    }

    private static async Task<int> FindInterfaceReferences(Solution solution, int interfaceCount, Compilation? compilation, IEnumerable<SyntaxTree> syntaxTrees)
    {
        var interfaces = syntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>());

        foreach (var @interface in interfaces)
        {
            var model = compilation?.GetSemanticModel(@interface.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(@interface);
            if (symbol == null)
            {
                continue;
            }

            var references = await SymbolFinder.FindReferencesAsync(symbol, solution);
            var count = references.Aggregate(0, (total, reference) => total + reference.Locations.Count());
            if (count == 0)
            {
                Console.WriteLine($"Interface '{@interface.Identifier}' has zero references.");
                interfaceCount++;
            }
        }

        return interfaceCount;
    }

    private static async Task<int> FindMethodReferences(Solution solution, int methodCount, Compilation? compilation, IEnumerable<SyntaxTree> syntaxTrees)
    {
        var methods = syntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>());

        foreach (var method in methods)
        {
            if (method.Identifier.Text.Contains("OnInitializedAsync") || (method.Identifier.Text.Contains("OnInitialized")) || (method.Identifier.Text.Contains("Dispose")  ||
               (method.Identifier.Text.Contains("OnParametersSet2Async")) || (method.Identifier.Text.Contains("OnAfterRender")) || (method.Identifier.Text.Contains("OnAfterRenderAsync")) ||
               (method.Identifier.Text.Contains("Equals")) || (method.Identifier.Text.Contains("Main")) || (method.Identifier.Text.Contains("BuildRenderTree")) ||
               (method.Identifier.Text.Contains("GetHashCode")) || (method.Identifier.Text.Contains("ConfigureServices")) || (method.Identifier.Text.Contains("Configure")) ||
               (method.Identifier.Text.Contains("Compare")) || (method.Identifier.Text.Contains('_')) || (method.Identifier.Text.Contains("ExecuteAsync")) ||
               (method.Identifier.Text.Contains("GetByIDAsync")) || (method.Identifier.Text.Contains("OnGet"))))
            {
                continue;
            }
            
            var model = compilation?.GetSemanticModel(method.SyntaxTree);
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
                Console.WriteLine($"Method '{method.Identifier}' in class '{className}' has zero references.");
                methodCount++;
            }
        }

        return methodCount;
    }
}