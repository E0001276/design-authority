using DesignAuthority.Infrastructure.FileStorage;
using Microsoft.Extensions.Options;

namespace DesignAuthority.Infrastructure.IntegrationTests.FileStorage;

public sealed class LocalFileStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "design-authority-tests", Guid.NewGuid().ToString("N"));
    private readonly LocalFileStorage storage;

    public LocalFileStorageTests()
    {
        storage = new LocalFileStorage(Options.Create(new FileStorageOptions { RootPath = root }));
    }

    [Fact]
    public async Task Staged_minute_uses_a_relative_reference_and_can_be_promoted_and_read()
    {
        var content = new byte[] { 1, 2, 3, 4 };
        await using var source = new MemoryStream(content);

        var staged = await storage.StageAsync("minutes\\review.docx", source, content.Length, CancellationToken.None);

        Assert.Equal("review.docx", staged.OriginalFileName);
        Assert.False(Path.IsPathRooted(staged.RelativePath));
        Assert.EndsWith(".docx", staged.StorageFileName, StringComparison.OrdinalIgnoreCase);

        await storage.PromoteAsync(staged, CancellationToken.None);
        await using (var stored = await storage.OpenReadAsync(staged.RelativePath, CancellationToken.None))
        using (var actual = new MemoryStream())
        {
            await stored.CopyToAsync(actual);
            Assert.Equal(content, actual.ToArray());
        }

        await storage.DeleteAsync(staged.RelativePath, CancellationToken.None);
        Assert.False(File.Exists(Path.Combine(root, staged.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task Stage_rejects_wrong_extensions_oversize_and_length_mismatches()
    {
        await using var content = new MemoryStream([1, 2, 3]);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.StageAsync("review.pdf", content, 3, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            storage.StageAsync("review.docx", content, 5 * 1024 * 1024 + 1, CancellationToken.None));
        await Assert.ThrowsAsync<IOException>(() =>
            storage.StageAsync("review.docx", content, 4, CancellationToken.None));
    }

    [Fact]
    public async Task File_references_cannot_escape_the_configured_root()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.OpenReadAsync("..\\outside.docx", CancellationToken.None));
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
