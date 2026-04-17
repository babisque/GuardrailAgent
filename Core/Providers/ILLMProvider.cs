public record AnalysisRequest(string CodeContext, string Rules, string FileName);
public record AnalysisResponse(bool IsCompliant, string Feedback, string SuggestedRefactoring);

public interface ILLMProvider
{
    string ProviderName { get; }

    Task<AnalysisResponse> AnalyzeCodeAsync(AnalysisRequest request, CancellationToken cancellationToken = default);
}