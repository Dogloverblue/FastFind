using TestMauiApp.FileSearch;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Queries.Function;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileSearch
{
    public static class SearchIndex
    {
        private static readonly LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private static readonly string IndexFolder = AppPaths.IndexDir;

        private static readonly FSDirectory DirectoryHandle = FSDirectory.Open(IndexFolder);
        private static readonly Analyzer Analyzer = new StandardAnalyzer(AppLuceneVersion);

        // One IndexWriter per process is the standard pattern (thread-safe).
        private static readonly Lazy<IndexWriter> WriterLazy = new Lazy<IndexWriter>(() =>
        {
            var cfg = new IndexWriterConfig(AppLuceneVersion, Analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };
            return new IndexWriter(DirectoryHandle, cfg);
        });

        private static IndexWriter Writer => WriterLazy.Value;

        public sealed class BestMatchResult
        {
            public string Path { get; init; } = "";
            public string Name { get; init; } = "";
            public string FileType { get; init; } = "";
            public string[] Tags { get; init; } = Array.Empty<string>();
            public float Score { get; init; }
        }

        public static void RemoveByPath(string path)
        {
            var normalizedPath = Path.GetFullPath(path);
            Writer.DeleteDocuments(new Term("path", normalizedPath));
            Writer.Flush(triggerMerge: false, applyAllDeletes: true);
        }

        // Delete all files under a directory (recursive) by path prefix
        public static void RemoveByDirectory(string directoryPath)
        {
            var dir = NormalizeDir(directoryPath);
            // PrefixQuery works on the single-term StringField because it enumerates matching terms by prefix.
            var q = new PrefixQuery(new Term("path", dir));
            Writer.DeleteDocuments(q);
            Writer.Flush(triggerMerge: false, applyAllDeletes: true);
        }

        public static List<string> GetAllIndexedPaths()
        {
            using var reader = DirectoryReader.Open(Writer, applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);
            var hits = searcher.Search(new MatchAllDocsQuery(), reader.NumDocs); // NumDocs = live docs
            var paths = new List<string>(reader.NumDocs);

            foreach (var sd in hits.ScoreDocs)
            {
                var doc = searcher.Doc(sd.Doc);
                var p = doc.Get("path");
                if (!string.IsNullOrEmpty(p)) paths.Add(p);
            }
            return paths;
        }

        public static void RemovePathsNotIn(ISet<string> keepPaths)
        {
            using var reader = DirectoryReader.Open(Writer, applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);
            var hits = searcher.Search(new MatchAllDocsQuery(), reader.NumDocs);

            var toDelete = new List<Term>();
            foreach (var sd in hits.ScoreDocs)
            {
                var doc = searcher.Doc(sd.Doc);
                var p = doc.Get("path");
                if (!string.IsNullOrEmpty(p) && !keepPaths.Contains(p))
                    toDelete.Add(new Term("path", p));
            }

            if (toDelete.Count > 0)
            {
                Writer.DeleteDocuments(toDelete.ToArray());
                Writer.Flush(triggerMerge: false, applyAllDeletes: true);
            }
        }

        // Normalize to a full path with a trailing separator for prefix queries
        private static string NormalizeDir(string path)
        {
            var full = Path.GetFullPath(path);
            // ensure trailing directory separator for precise prefix matches
            if (!full.EndsWith(Path.DirectorySeparatorChar))
                full += Path.DirectorySeparatorChar;
            return full;
        }

        // ---- Function 1: Add/Update a file in the index ----
        /// <summary>
        /// Indexes (or upserts) a file. "path" acts as the unique key.
        /// fileType is typically an extension like ".pdf" or "pdf".
        /// </summary>
        public static void UpsertEntry(StoredFileEntry entry)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));
            if (string.IsNullOrWhiteSpace(entry.filePath)) throw new ArgumentException("filePath is required");

            // Normalize
            var normalizedPath = Path.GetFullPath(entry.filePath);
            var normalizedName = string.IsNullOrWhiteSpace(entry.fileName)
                ? Path.GetFileName(normalizedPath)
                : entry.fileName;

            string[] normTags = entry.tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToLowerInvariant())
                .Distinct()
                .ToArray();

            string[] normTypes = entry.filetypes
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t =>
                {
                    var s = t.Trim().ToLowerInvariant();
                    return s.StartsWith(".") ? s.Substring(1) : s; // ".pdf" -> "pdf"
                })
                .Distinct()
                .ToArray();

            var doc = new Document
    {
        // Unique key (untokenized, stored)
        new StringField("path", normalizedPath, Field.Store.YES),

        // Searchable, stored
        new TextField("name", normalizedName, Field.Store.YES),

        // Tags: analyzed for search; stored for display
        new TextField("tags", string.Join(" ", normTags), Field.Store.YES),

        // Types (aliases): analyzed for fuzzy/keyword search; stored for display
        new TextField("type", string.Join(" ", normTypes), Field.Store.YES),
    };

            // Also index exact, multi-valued keywords for filtering/facets later (not stored)
            foreach (var t in normTypes)
                doc.Add(new StringField("type_kw", t, Field.Store.NO));

            foreach (var tg in normTags)
                doc.Add(new StringField("tag_kw", tg, Field.Store.NO));

            // Upsert by unique key (path)
            Writer.UpdateDocument(new Term("path", normalizedPath), doc);
            Writer.Flush(triggerMerge: false, applyAllDeletes: true);
        }

        /// <summary>
        /// Searches across name, tags, and type using fuzzy matching
        /// and returns the top N hits (ranked).
        /// </summary>
        public static List<BestMatchResult> SearchBestMatches(string rawQuery, int count = 5)
        {
            if (string.IsNullOrWhiteSpace(rawQuery)) return new List<BestMatchResult>();
            if (count <= 0) count = 1;

            using var reader = DirectoryReader.Open(Writer, applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);

            var fields = new[] { "tags", "name", "type" };
            var boosts = new Dictionary<string, float>
            {
                ["tags"] = 3.0f,
                ["name"] = 2.0f,
                ["type"] = 1.2f
            };
            var parser = new MultiFieldQueryParser(AppLuceneVersion, fields, Analyzer, boosts);

            string Fuzzyize(string q) =>
                string.Join(" ",
                    q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(t => t.Length >= 3 ? $"{t}~1" : t)
                );

            Query mainQuery;
            try { mainQuery = parser.Parse(Fuzzyize(rawQuery)); }
            catch (ParseException) { mainQuery = new MatchAllDocsQuery(); }

            var bq = new BooleanQuery
    {
        { mainQuery, Occur.SHOULD }
    };

            // Prefix boost for last token (helps partial input)
            var last = rawQuery.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (!string.IsNullOrWhiteSpace(last) && last.Length >= 2)
            {
                bq.Add(new PrefixQuery(new Term("name", last.ToLowerInvariant())), Occur.SHOULD);
                bq.Add(new PrefixQuery(new Term("tags", last.ToLowerInvariant())), Occur.SHOULD);
            }

            // >>> Fallback "catch-all" so we always fill up to N results <<<
            // Tiny boost so real matches always outrank these.
            var fallback = new MatchAllDocsQuery();
            fallback.Boost = 0.0001f; // super low priority
            bq.Add(fallback, Occur.SHOULD);

            var top = searcher.Search(bq, count);
            var results = new List<BestMatchResult>(top.ScoreDocs.Length);

            foreach (var sd in top.ScoreDocs)
            {
                var doc = searcher.Doc(sd.Doc);
                results.Add(new BestMatchResult
                {
                    Path = doc.Get("path") ?? "",
                    Name = doc.Get("name") ?? "",
                    FileType = doc.Get("type") ?? "",
                    Tags = (doc.Get("tags") ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                    Score = sd.Score
                });
            }
            return results;
        }

        // ---- Helper(s) ----
        private static string NormalizeFileType(string fileType)
        {
            if (string.IsNullOrWhiteSpace(fileType)) return "";
            var t = fileType.Trim().ToLowerInvariant();
            if (t.StartsWith(".")) t = t.Substring(1);
            return t;
        }

        /// <summary>
        /// Optional: call on shutdown to close the writer cleanly.
        /// </summary>
        public static void Dispose()
        {
            if (WriterLazy.IsValueCreated)
            {
                Writer.Flush(triggerMerge: true, applyAllDeletes: true);
                Writer.Commit();
                Writer.Dispose();
            }
            DirectoryHandle?.Dispose();
            Analyzer?.Dispose();
        }
    }
}