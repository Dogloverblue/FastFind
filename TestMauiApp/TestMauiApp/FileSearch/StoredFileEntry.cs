using System.Collections.Generic;
using System.Text.Json.Serialization;

public sealed class StoredFileEntry
{
    [JsonPropertyName("fileName")] public string fileName { get; }
    [JsonPropertyName("tags")] public IEnumerable<string> tags { get; }
    [JsonPropertyName("filetypes")] public IEnumerable<string> filetypes { get; }
    [JsonPropertyName("filePath")] public string filePath { get; }

    [JsonConstructor]
    public StoredFileEntry(string fileName, IEnumerable<string> tags, IEnumerable<string> filetypes, string filePath)
    {
        this.fileName = fileName;
        this.tags = tags ?? System.Array.Empty<string>();
        this.filetypes = filetypes ?? System.Array.Empty<string>();
        this.filePath = filePath;
    }

    public StoredFileEntry WithTags(IEnumerable<string> newTags) =>
        new StoredFileEntry(fileName, newTags, filetypes, filePath);
}