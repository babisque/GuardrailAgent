namespace Guardrail.Core.Features.VerifyVsaCompliance;

public class RepositoryScanner
{
    public IEnumerable<string> FindSlices(string rootPath)
    {
        var featuresPath = Path.Combine(rootPath, "Features");
        if (!Directory.Exists(featuresPath)) return Enumerable.Empty<string>();

        return Directory.GetDirectories(featuresPath, "*", SearchOption.TopDirectoryOnly);
    }
}
