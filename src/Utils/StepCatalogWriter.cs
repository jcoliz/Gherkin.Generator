using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Gherkin.Generator.Utils;

/// <summary>
/// Writes the generated step catalog to disk at test-assembly load time.
/// </summary>
/// <remarks>
/// Called from a generated module initializer. All failures are swallowed and reported via
/// <see cref="Trace"/> so that a catalog write problem never fails or crashes an otherwise
/// valid test run.
/// </remarks>
public static class StepCatalogWriter
{
    /// <summary>
    /// Writes <paramref name="content"/> to <paramref name="fileName"/> in <paramref name="directory"/>,
    /// skipping the write if the file already contains identical content.
    /// </summary>
    /// <param name="directory">Directory to write the catalog file into (typically beside the test assembly).</param>
    /// <param name="fileName">Deterministic file name for the catalog, e.g. "MyTests.StepCatalog.md".</param>
    /// <param name="content">Rendered catalog content.</param>
    public static void WriteIfChanged(string directory, string fileName, string content)
    {
        try
        {
            var path = Path.Combine(directory, fileName);

            if (File.Exists(path) && File.ReadAllText(path, Encoding.UTF8) == content)
            {
                // Content unchanged; avoid unnecessary writes and timestamp churn.
                return;
            }

            var tempPath = Path.Combine(directory, $"{fileName}.{Guid.NewGuid():N}.tmp");
            File.WriteAllText(tempPath, content, Encoding.UTF8);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(tempPath, path);
        }
        catch (Exception ex)
        {
            // A catalog write failure must never fail or crash an otherwise valid test run.
            Trace.TraceWarning($"Gherkin.Generator: failed to write step catalog '{fileName}' to '{directory}': {ex.Message}");
        }
    }
}
