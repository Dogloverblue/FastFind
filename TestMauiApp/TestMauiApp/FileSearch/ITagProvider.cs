using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITagProvider
{
    Task<IReadOnlyList<string>> GetTagsAsync(string filePath, CancellationToken ct);
}