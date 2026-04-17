using Guardrail.Features.VerifyVsaCompliance;
using Guardrail.Infrastructure.Providers;
using Guardrail.Infrastructure.Providers.Gemini;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

var services = new ServiceCollection();
services.AddSingleton<ILLMProvider, GeminiProvider>();
services.AddTransient<VerifyVsaComplianceHandler>();
var serviceProvider = services.BuildServiceProvider();

var rootCommand = new RootCommand("Guardrail Agent: The VSA sheriff for your codebase.");

var pathOption = new Option<string>(
    aliases: ["--path", "-p"],
    description: "Caminho do diretório ou arquivo para analisar.")
{ IsRequired = true };

var analyzeCommand = new Command("analyze", "Analyzes the architectural compliance of a specified path.")
{
    pathOption
};

analyzeCommand.SetHandler(async (string path) =>
{
    var handler = serviceProvider.GetRequiredService<VerifyVsaComplianceHandler>();
    await handler.HandleAsync(new VerifyVsaRequest(path, "Gemini"));
}, pathOption);

rootCommand.AddCommand(analyzeCommand);
return await rootCommand.InvokeAsync(args);