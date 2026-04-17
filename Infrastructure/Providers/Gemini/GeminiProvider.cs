using Google.GenAI;
using Google.GenAI.Types;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Guardrail.Infrastructure.Providers.Gemini;

public class GeminiProvider : ILLMProvider
{
    private readonly Client _client;
    public string ProviderName => "Gemini";

    public GeminiProvider()
    {
        var apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                     ?? throw new InvalidOperationException("GEMINI_API_KEY not found.");

        _client = new Client(apiKey: apiKey);
    }

    public async Task<AnalysisResponse> AnalyzeCodeAsync(AnalysisRequest request, CancellationToken ct = default)
    {
        var response = await _client.Models.GenerateContentAsync(
            model: "gemini-3-flash-preview",
            contents:
            [
                new Content
                {
                    Role = "user",
                    Parts = [ new Part { Text = $"{request.Rules}\n\nCode for analyzes:\n{request.CodeContext}" } ]
                }
            ],
            config: new GenerateContentConfig { ResponseMimeType = "application/json" },
            cancellationToken: ct
        );

        var resultText = response.Text ?? throw new Exception("Gemini returned an empty response.");

        return JsonSerializer.Deserialize<AnalysisResponse>(resultText,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}