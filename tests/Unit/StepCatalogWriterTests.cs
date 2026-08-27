using Gherkin.Generator.Utils;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for <see cref="StepCatalogWriter"/>.
/// </summary>
[TestFixture]
public class StepCatalogWriterTests
{
    private string _directory = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "GherkinStepCatalogTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Test]
    public void WriteIfChanged_NewFile_WritesContent()
    {
        // Given: No existing catalog file
        const string fileName = "Test.StepCatalog.md";

        // When: Writing content
        StepCatalogWriter.WriteIfChanged(_directory, fileName, "# Step Catalog");

        // Then: The file exists with the expected content
        var path = Path.Combine(_directory, fileName);
        Assert.That(File.Exists(path), Is.True);
        Assert.That(File.ReadAllText(path), Is.EqualTo("# Step Catalog"));
    }

    [Test]
    public void WriteIfChanged_ChangedContent_OverwritesFile()
    {
        // Given: An existing catalog file with old content
        const string fileName = "Test.StepCatalog.md";
        var path = Path.Combine(_directory, fileName);
        File.WriteAllText(path, "old content");

        // When: Writing new content
        StepCatalogWriter.WriteIfChanged(_directory, fileName, "new content");

        // Then: The file is updated
        Assert.That(File.ReadAllText(path), Is.EqualTo("new content"));
    }

    [Test]
    public void WriteIfChanged_UnchangedContent_DoesNotRewriteFile()
    {
        // Given: An existing catalog file matching the new content
        const string fileName = "Test.StepCatalog.md";
        var path = Path.Combine(_directory, fileName);
        File.WriteAllText(path, "same content");
        var originalWriteTime = File.GetLastWriteTimeUtc(path);

        // When: Writing identical content
        Thread.Sleep(50);
        StepCatalogWriter.WriteIfChanged(_directory, fileName, "same content");

        // Then: The file's write time is unchanged (no rewrite occurred)
        Assert.That(File.GetLastWriteTimeUtc(path), Is.EqualTo(originalWriteTime));
    }

    [Test]
    public void WriteIfChanged_LeavesNoTempFilesBehind()
    {
        // Given: A fresh directory
        const string fileName = "Test.StepCatalog.md";

        // When: Writing content
        StepCatalogWriter.WriteIfChanged(_directory, fileName, "content");

        // Then: Only the final catalog file remains, no leftover .tmp files
        var entries = Directory.GetFiles(_directory);
        Assert.That(entries, Has.Length.EqualTo(1));
        Assert.That(entries[0], Does.EndWith(fileName));
    }

    [Test]
    public void WriteIfChanged_UnwritableDirectory_DoesNotThrow()
    {
        // Given: A directory that does not exist and cannot be created implicitly
        var badDirectory = Path.Combine(_directory, "missing", "nested", "path");

        // When/Then: Writing does not throw, even though the write will fail
        Assert.DoesNotThrow(() => StepCatalogWriter.WriteIfChanged(badDirectory, "Test.StepCatalog.md", "content"));
    }
}
