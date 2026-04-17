using Guardrail.Features.VerifyVsaCompliance;
using Guardrail.Infrastructure.Providers;
using Guardrail.Infrastructure.Providers.Gemini;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<ILLMProvider, GeminiProvider>();
services.AddTransient<VerifyVsaComplianceHandler>();

var serviceProvider = services.BuildServiceProvider();

var handler = serviceProvider.GetRequiredService<VerifyVsaComplianceHandler>();

await handler.HandleAsync(new VerifyVsaRequest("Features/Orders/CreateOrderHandler.cs", "Gemini"));