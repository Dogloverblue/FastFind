using TestMauiApp.FileSearch;
using FileSearch;
using OpenAI.Files;
using OpenAI.Responses;
using System.Collections.Immutable;
using System.Threading.Tasks;


namespace TestMauiApp.FileSearch;
class Program
{


    public static StoredFileEntryCache cache = new StoredFileEntryCache(AppPaths.EntryCacheFile);
    public static OpenAiTagProvider tagger = new OpenAiTagProvider(maxConcurrency: 4);

    public static TestMauiApp.FileSearch.FolderIndexer indexer = new TestMauiApp.FileSearch.FolderIndexer(cache, tagger)
{
    MaxConcurrency = 4
};

static void Main()
    {
        AppPaths.setup();

        indexer.AddDisallowedExtensions(".mca", ".dat", ".bin", ".nbt");

        indexer.addPathToIndex(@"C:\Users\Lowen\OneDrive\Desktop\myFiles\rats");
        indexer.addPathToIndex(@"C:\Users\Lowen\OneDrive\Desktop\myFiles\others");
        //indexer.addPathToIndex(@"C:\Users\Lowen\OneDrive\Desktop");
        Console.WriteLine("Indexing...,");

        
       indexer.updateIndex().GetAwaiter().GetResult();

        Console.WriteLine("Hello from Main!");
        Console.WriteLine("Lucene quick test ready. Type a query (Enter to quit).");
        while (true)
        {
            Console.Write("\nQuery> ");
            var q = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(q)) break;

            if (q.StartsWith("removeFolder:"))
            {
                indexer.removePathFromIndex(q.Replace("removeFolder:", ""));
                continue;
            }
            if (q.StartsWith("addFolder:"))
            {
                indexer.addPathToIndex(q.Replace("addFolder:", ""));
                indexer.updateIndex().GetAwaiter().GetResult();
                continue;
            }

            var hits = SearchIndex.SearchBestMatches(q, count: 3);
            if (hits.Count == 0)
            { 
                Console.WriteLine("No results.");
                continue;
            }

            int i = 1;
            foreach (var h in hits)
            {
                Console.WriteLine($"{i++}. score={h.Score:0.00}  name={h.Name}  type={h.FileType}");
                Console.WriteLine($"    path={h.Path}");
                Console.WriteLine($"    tags=[{string.Join(", ", h.Tags)}]");
            }
        }

        //inputFilesFromTestDirectory().GetAwaiter().GetResult();
        SearchIndex.Dispose();
    }





    //async static Task inputFilesFromTestDirectory()
    //{

    //    string[] files = Directory.GetFiles("testfiles");

    //    foreach (string inputfile in files)
    //    {
    //        var fileName = Path.GetFileName(inputfile);
    //        var fileTypeAndAliases = FileTypeAliases.GetTagAliases(Path.GetExtension(inputfile).Substring(1));
    //        var filePath = Path.GetFullPath(inputfile);
    //        var entry = await cache.GetOrCreateForFileAsync(
    //        filePath: filePath,
    //        createIfMissing: () => new StoredFileEntry(
    //            fileName: fileName,
    //            filetypes: fileTypeAndAliases,
    //            filePath: filePath
    //        ),
    //        updatePathIfDifferent: true
    //    );
    //        SearchIndex.UpsertEntry(entry);
    //    }
    //}






}