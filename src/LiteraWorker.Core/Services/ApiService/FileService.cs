namespace LiteraWorker.Core.Services.ApiService;

using System.IO.Compression;
using System.Net.Http.Headers;
using LiteraWorker.Core.Helpers;

public sealed class FileService(IHttpClientFactory httpClientFactory)
{
    public async ValueTask<Result<EmptyRecord>> UploadFilesAsync(IEnumerable<string> filePaths, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();

        // We need to keep references to the content objects so they aren't garbage collected/disposed early
        var fileContents = new List<HttpContent>();

        foreach (var filePath in filePaths)
        {
            var file = new FileInfo(filePath);

            var fileContent = new StreamContent(file.OpenRead());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            content.Add(fileContent, "formFiles", file.Name);
            fileContents.Add(fileContent); // Keep reference alive
        }

        using var client = httpClientFactory.CreateClient("LiteraClient");

        try
        {
            var result = await client.PostAsync("api/file/upload", content, ct);

            if (!result.IsSuccessStatusCode)
            {
                return Result<EmptyRecord>.Failure(result.ReasonPhrase ?? "An error occured", (int)result.StatusCode);
            }

            return Result<EmptyRecord>.Success(EmptyRecord.Empty);
        }
        finally
        {
            // Explicitly dispose contents after the request to ensure streams close cleanly
            foreach (var fc in fileContents)
            {
                fc.Dispose();
            }
        }
    }

    public async ValueTask<List<string>> DownloadFilesAsync(
        IEnumerable<string> filenames,
        CancellationToken ct = default)
    {
        using var client = httpClientFactory.CreateClient("LiteraClient");
        var query = string.Join("&", filenames.Select(f =>
            $"filename={Uri.EscapeDataString(f)}"));

        using var response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/file?{query}"),
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        response.EnsureSuccessStatusCode();

        var outputPath = Path.Combine(
            AppContext.BaseDirectory,
            "data",
            "downloaded-files");

        Directory.CreateDirectory(outputPath);

        await using var zipStream = await response.Content.ReadAsStreamAsync(ct);

        return ExtractZipFromStream(zipStream, outputPath);
    }

    // SAFE ZIP EXTRACTION
    private static List<string> ExtractZipFromStream(Stream zipStream, string outputFolder)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        var extractedFiles = new List<string>();

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
                continue; // directory

            var destinationPath = Path.GetFullPath(
                Path.Combine(outputFolder, entry.FullName));

            // Prevent Zip Slip
            if (!destinationPath.StartsWith(
                    Path.GetFullPath(outputFolder),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Invalid ZIP entry path.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            using var entryStream = entry.Open();
            using var fileStream = File.Create(destinationPath);
            entryStream.CopyTo(fileStream);

            extractedFiles.Add(destinationPath);
        }

        return extractedFiles;
    }
}
