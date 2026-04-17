namespace Guardrail.Core.Features.VerifyVsaCompliance;

public class VerifyVsaComplianceHandler
{
    private readonly IEnumerable<ILLMProvider> _providers;
    private readonly string _playbook;

    public VerifyVsaComplianceHandler(IEnumerable<ILLMProvider> providers)
    {
        _providers = providers;
        _playbook = File.ReadAllText("rules/vsa-playbook.md");
    }

    public async Task ExecuteAsync(string targetPath, string providerName)
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderName == providerName)
            ?? throw new Exception("Provider not found");

        var code = await File.ReadAllTextAsync(targetPath);
        var result = await provider.AnalyzeCodeAsync(new AnalysisRequest(code, _playbook, targetPath));

        Console.WriteLine($"--- RESULT: ---");
        Console.WriteLine(result.IsCompliant ? "✅ COMPLIANT" : "❌ NON-COMPLIANT");
        Console.WriteLine(result.Feedback);
    }
}