// FolderIndexer.cs
using FileSearch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestMauiApp.FileSearch
{
    class FolderIndexer
    {
        private readonly HashSet<string> _includeDirs = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _includeFiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _disallowedExtensions = new(StringComparer.OrdinalIgnoreCase);
        private readonly StoredFileEntryCache _cache;
        private readonly ITagProvider _tagger;

        public int MaxConcurrency { get; set; } = Math.Max(2, Environment.ProcessorCount / 2);

        public FolderIndexer(StoredFileEntryCache cache, ITagProvider tagger)
        {
            _cache = cache;
            _tagger = tagger;
        }

        private static readonly HashSet<string> _excludedDirNames =
    new(StringComparer.OrdinalIgnoreCase) { "node_modules" };

        public void AddAllowedExtensions(params string[] exts)
        {
            foreach (var e in exts ?? Array.Empty<string>())
                _allowedExtensions.Add(NormExt(e));
        }

        public void AddDisallowedExtensions(params string[] exts)
        {
            foreach (var e in exts ?? Array.Empty<string>())
                _disallowedExtensions.Add(NormExt(e));
        }

        private static IEnumerable<string> EnumerateFilesSkippingExcluded(string root)
        {
            // If the root itself is excluded, bail
            var rootName = Path.GetFileName(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar));
            if (_excludedDirNames.Contains(rootName))
                yield break;

            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                // Enqueue subdirectories (skipping excluded names and reparse points)
                IEnumerable<string> subdirs = Array.Empty<string>();
                try { subdirs = Directory.EnumerateDirectories(current); }
                catch { /* access denied etc. */ }

                foreach (var d in subdirs)
                {
                    var name = Path.GetFileName(d);
                    if (_excludedDirNames.Contains(name)) continue;

                    try
                    {
                        var di = new DirectoryInfo(d);
                        if ((di.Attributes & FileAttributes.ReparsePoint) != 0) continue; // avoid symlink/junction loops
                    }
                    catch { /* best effort */ }

                    stack.Push(d);
                }

                // Yield files in this directory
                IEnumerable<string> files = Array.Empty<string>();
                try { files = Directory.EnumerateFiles(current); }
                catch { /* access denied etc. */ }

                foreach (var f in files)
                    yield return f;
            }
        }

        public void addPathToIndex(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            if (Directory.Exists(path)) _includeDirs.Add(NormalizeDir(path));
            else if (File.Exists(path)) _includeFiles.Add(Path.GetFullPath(path));
            else if (LooksLikeDirectory(path)) _includeDirs.Add(NormalizeDir(path));
            else _includeFiles.Add(Path.GetFullPath(path));
        }

        public void removePathFromIndex(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            if (Directory.Exists(path) || LooksLikeDirectory(path))
            {
                var dir = NormalizeDir(path);
                _includeDirs.Remove(dir);
                SearchIndex.RemoveByDirectory(dir);
            }
            else
            {
                var p = Path.GetFullPath(path);
                _includeFiles.Remove(p);
                SearchIndex.RemoveByPath(p);
            }
        }

        /// Scan included dirs/files, index in parallel, then purge anything outside the alive set.
        public async Task updateIndex(CancellationToken ct = default)
        {
            // Build worklist
            var work = new List<string>();
            foreach (var dir in _includeDirs.ToArray())
            {
                if (!Directory.Exists(dir)) continue;
                IEnumerable<string> files;
                try { files = EnumerateFilesSkippingExcluded(dir); }
                catch { continue; }
                foreach (var f in files)
                    if (ShouldIndex(f)) work.Add(Path.GetFullPath(f));
            }
            foreach (var f in _includeFiles.ToArray())
                if (File.Exists(f) && ShouldIndex(f))
                    work.Add(Path.GetFullPath(f));

            var alive = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
            using var throttler = new SemaphoreSlim(MaxConcurrency);
            var tasks = new List<Task>(work.Count);

            foreach (var path in work)
            {
                await throttler.WaitAsync(ct);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await IndexOneAsync(path, ct);
                        alive[path] = 1;
                    }
                    catch
                    {
                        // TODO: log; don't fail entire run
                    }
                    finally { throttler.Release(); }
                }, ct));
            }

            await Task.WhenAll(tasks);

            // Purge anything no longer present
            SearchIndex.RemovePathsNotIn(alive.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase));
        }

        // ---- internals ----

        private async Task IndexOneAsync(string filePath, CancellationToken ct)
        {
            var entry = await _cache.GetOrCreateForFileAsync(
                filePath: filePath,
                createIfMissing: () =>
                {
                    var name = Path.GetFileName(filePath);
                    var ext = NormExt(Path.GetExtension(filePath));
                    var types = FileTypeAliases.GetTagAliases(ext); // your existing alias provider
                    return new StoredFileEntry(name, Array.Empty<string>(), types, filePath);
                },
                updatePathIfDifferent: true,
                ct: ct
            );

            // Only tag if needed (empty tags) and we're a type we care about
            var ext = NormExt(Path.GetExtension(filePath));
            bool shouldTag = !entry.tags.Any();

            if (shouldTag)
            {
                var tags = await _tagger.GetTagsAsync(filePath, ct);
                if (tags.Count > 0)
                {
                    entry = entry.WithTags(tags);
                    // Update cache so we never call OpenAI again for these bytes
                    var hash = await StoredFileEntryCache.ComputeFileHashAsync(filePath, ct);
                    _cache.Put(hash, entry);
                }
            }

            SearchIndex.UpsertEntry(entry);
        }

        private static string NormalizeDir(string path)
        {
            var full = Path.GetFullPath(path);
            if (!full.EndsWith(Path.DirectorySeparatorChar))
                full += Path.DirectorySeparatorChar;
            return full;
        }

        private static bool LooksLikeDirectory(string path) =>
            path.EndsWith(Path.DirectorySeparatorChar.ToString()) || string.IsNullOrEmpty(Path.GetExtension(path));

        private bool ShouldIndex(string filePath)
        {
            try
            {
                var fi = new FileInfo(filePath);
                var fileExtension = NormExt(fi.Extension);
                if ((fi.Attributes & FileAttributes.Hidden) != 0) return false;
                if ((fi.Attributes & FileAttributes.System) != 0) return false;
                if (_disallowedExtensions.Contains(fileExtension)) return false;
                if (_allowedExtensions.Count == 0) return true;
                return _allowedExtensions.Contains(fileExtension);
            }
            catch { return false; }
        }

        private static string NormExt(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext)) return "";
            var s = ext.Trim().ToLowerInvariant();
            return s.StartsWith(".") ? s[1..] : s;
        }
    }
}
