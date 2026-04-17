namespace Guardrail.Features.VerifyVsaCompliance;

public record VerifyVsaRequest(string TargetFilePath, string ProviderName);

public class VerifyVsaComplianceHandler(IEnumerable<ILLMProvider> providers, RepositoryScanner scanner)
{
    public async Task HandleAsync(VerifyVsaRequest request)
    {
        var provider = providers.FirstOrDefault(p => p.ProviderName == request.ProviderName)
            ?? throw new ArgumentException("Provider not found");

        var rules = await File.ReadAllTextAsync("Core/rules/vsa-playbook.md");

        if (Directory.Exists(request.TargetFilePath) &&
            Directory.GetDirectories(request.TargetFilePath, "Features").Any())
        {
            var slices = scanner.FindSlices(request.TargetFilePath);

            foreach (var slicePath in slices)
            {
                await AnalyzeSlice(slicePath, provider, rules);
            }
        }
        else
        {
            await AnalyzeSlice(request.TargetFilePath, provider, rules);
        }
    }

    private async Task AnalyzeSlice(string path, ILLMProvider provider, string rules)
    {
        string codeContext = "";
        if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                codeContext += $"\n// File: {Path.GetFileName(file)}\n" + await File.ReadAllTextAsync(file);
            }
        }
        else
        {
            codeContext = await File.ReadAllTextAsync(path);
        }

        Console.WriteLine($"\n🔍 Analyzing Slice/File: {Path.GetFileName(path)}...");

        var response = await provider.AnalyzeCodeAsync(new AnalysisRequest(codeContext, rules, path));
        RenderResult(response);
    }

    private void RenderResult(AnalysisResponse response)
    {
        var color = response.IsCompliant ? ConsoleColor.Green : ConsoleColor.Red;
        var status = response.IsCompliant ? "[PASS]" : "[FAIL]";

        Console.ForegroundColor = color;
        Console.WriteLine($"{status} Architectural Conformity");
        Console.ResetColor();

        Console.WriteLine($"\nFeedback:\n{response.Feedback}");

        if (!string.IsNullOrEmpty(response.SuggestedRefactoring))
        {
            Console.WriteLine("\n💡 Suggested Refactoring:");
            Console.WriteLine(response.SuggestedRefactoring);
        }
    }
}
