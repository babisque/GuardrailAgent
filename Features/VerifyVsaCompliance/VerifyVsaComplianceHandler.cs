namespace Guardrail.Features.VerifyVsaCompliance;

public record VerifyVsaRequest(string TargetFilePath, string ProviderName);

public class VerifyVsaComplianceHandler
{
    private readonly IEnumerable<ILLMProvider> _provider;

    public VerifyVsaComplianceHandler(IEnumerable<ILLMProvider> providers)
    {
        _provider = providers;
    }

    public async Task HandleAsync(VerifyVsaRequest request)
    {
        var provider = _provider.FirstOrDefault(p => p.ProviderName == request.ProviderName)
            ?? throw new ArgumentException("Provider not found");

        var code = await File.ReadAllTextAsync(request.TargetFilePath);

        var rules = await File.ReadAllTextAsync("rules/vsa-playbook.md");

        Console.WriteLine($"\n🔍 Analyzing: {Path.GetFileName(request.TargetFilePath)}...");

        var response = await provider.AnalyzeCodeAsync(new AnalysisRequest(code, rules, request.TargetFilePath));

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
