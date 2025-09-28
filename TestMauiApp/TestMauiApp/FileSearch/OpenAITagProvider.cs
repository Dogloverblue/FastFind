// OpenAiTagProvider.cs
using OpenAI.Files;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class OpenAiTagProvider : ITagProvider, IDisposable
{
    private readonly SemaphoreSlim _throttle;
    private readonly int _maxConcurrency;
    static List<string> imageFileTypes = new List<string> { "png", "jpg", "jpeg", "webp", "gif" };
    static string key = "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";

    public OpenAiTagProvider(int maxConcurrency = 4)
    {
        _maxConcurrency = Math.Max(1, maxConcurrency);
        _throttle = new SemaphoreSlim(_maxConcurrency);
    }

    public void Dispose() => _throttle.Dispose();

    public async Task<IReadOnlyList<string>> GetTagsAsync(string filePath, CancellationToken ct)
    {
        await _throttle.WaitAsync(ct);
        try
        {
            string csv = await CallOpenAiAsync(filePath, ct);
            return NormalizeTags(csv);
        }
        finally { _throttle.Release(); }
    }

    private static async Task<string> CallOpenAiAsync(string filePath, CancellationToken ct)
    {
        string inputFile = filePath;
        string fileExtension = Path.GetExtension(inputFile).ToLower().Substring(1);
        var messageItems = new List<ResponseContentPart>();
        OpenAIFileClient files = new(key);
        if (fileExtension.Equals("pdf"))
        {
            Console.WriteLine("found: pdf, for " + inputFile);
            OpenAIFile file = files.UploadFile(inputFile, FileUploadPurpose.UserData);
            messageItems.Add(ResponseContentPart.CreateInputTextPart("Create a set of 5-15 tags for this pdf, to make it easily reconizable to a file searching algorithum. Seperate each tag by commas. Tags can have spaces in them. Include tags related to the contents of the file. "+
                "File Name: "+filePath));
            messageItems.Add(ResponseContentPart.CreateInputFilePart(file.Id));
        }
        else if (imageFileTypes.Contains(fileExtension))
        {
            Console.WriteLine("found: image, for " + inputFile);
            OpenAIFile file = files.UploadFile(inputFile, "vision");
            messageItems.Add(ResponseContentPart.CreateInputTextPart("Create a set of 5-15 tags for this image, to make it easily reconizable to a file searching algorithum. Seperate each tag by commas. Tags can have spaces in them. Include tags related to the contents of things in the image. "+
                "File Name: " + filePath));
            messageItems.Add(ResponseContentPart.CreateInputImagePart(file.Id));
        }
        else
        {
            Console.WriteLine("found: other, for " + inputFile);
            var fileName = Path.GetFileName(inputFile);
            messageItems.Add(ResponseContentPart.CreateInputTextPart(
                "Create a set of 5-15 tags for this file, to make it easily reconizable to a file searching algorithum. Seperate each tag by commas. Tags can have spaces in them. Based on the file name and extension, infer probably what it is used for, and create appropiate tags"
                + "File Path And Name: " + Path.GetFullPath(fileName)));
        }

        OpenAIResponseClient client = new(model: "gpt-4.1-mini", apiKey: key);

        OpenAIResponse response = (OpenAIResponse)client.CreateResponse([
            ResponseItem.CreateUserMessageItem(
        messageItems
    ),
]);
        await Task.Yield();
        return response.GetOutputText();
    }

    private static IReadOnlyList<string> NormalizeTags(string csv) =>
        (csv ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(t => t.Trim().ToLowerInvariant())
        .Where(t => t.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
}
