using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

public sealed class StoredFileEntryCache : IDisposable
{
    private readonly string _cacheFilePath;
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private Dictionary<string, StoredFileEntry> _map = new Dictionary<string, StoredFileEntry>(StringComparer.OrdinalIgnoreCase);

    // JSON options: compact & stable
    private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public StoredFileEntryCache(string cacheFilePath)
    {
        _cacheFilePath = cacheFilePath;
        LoadFromDisk();
    }

    public void Dispose() => _lock?.Dispose();

    // ---- Public API ----

    /// <summary> Try get a stored entry by hash. </summary>
    public bool TryGet(string sha256Hex, out StoredFileEntry? entry)
    {
        _lock.EnterReadLock();
        try { return _map.TryGetValue(sha256Hex, out entry); }
        finally { _lock.ExitReadLock(); }
    }

    /// <summary> Put/update a stored entry for the given hash and persist. </summary>
    public void Put(string sha256Hex, StoredFileEntry entry, bool saveNow = true)
    {
        _lock.EnterWriteLock();
        try
        {
            _map[sha256Hex] = entry;
            if (saveNow) SaveToDisk_NoLock();
        }
        finally { _lock.ExitWriteLock(); }
    }

    /// <summary>
    /// Get from cache or create via factory and store.
    /// </summary>
    public StoredFileEntry GetOrCreate(string sha256Hex, Func<StoredFileEntry> factory, bool saveNow = true)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_map.TryGetValue(sha256Hex, out var existing))
                return existing;

            var created = factory();
            _lock.EnterWriteLock();
            try
            {
                _map[sha256Hex] = created;
                if (saveNow) SaveToDisk_NoLock();
            }
            finally { _lock.ExitWriteLock(); }
            return created;
        }
        finally { _lock.ExitUpgradeableReadLock(); }
    }

    /// <summary>
    /// Compute SHA-256 of a file as hex (streamed).
    /// </summary>
    public static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken ct = default)
    {
        using var sha = SHA256.Create();
        const int BufferSize = 1024 * 1024; // 1 MB
        byte[] buffer = new byte[BufferSize];

        await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);
        int bytesRead;
        while ((bytesRead = await fs.ReadAsync(buffer.AsMemory(0, BufferSize), ct)) > 0)
        {
            sha.TransformBlock(buffer, 0, bytesRead, null, 0);
            if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
        }
        sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        return Convert.ToHexString(sha.Hash!).ToLowerInvariant();
    }

    /// <summary>
    /// Convenience: compute hash for file, then get-or-create entry.
    /// If created, you typically pass defaults from current file state.
    /// Optionally updates filePath on a cached entry if it moved.
    /// </summary>
    public async Task<StoredFileEntry> GetOrCreateForFileAsync(
        string filePath,
        Func<StoredFileEntry> createIfMissing,
        bool updatePathIfDifferent = true,
        CancellationToken ct = default)
    {
        var hash = await ComputeFileHashAsync(filePath, ct);

        // Fast path with upgradeable lock
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_map.TryGetValue(hash, out var existing))
            {
                if (updatePathIfDifferent && !string.Equals(existing.filePath, filePath, StringComparison.OrdinalIgnoreCase))
                {
                    // Re-store with updated path (tags/types preserved)
                    var updated = new StoredFileEntry(existing.fileName, existing.tags, existing.filetypes, filePath);
                    _lock.EnterWriteLock();
                    try
                    {
                        _map[hash] = updated;
                        SaveToDisk_NoLock();
                    }
                    finally { _lock.ExitWriteLock(); }
                    return updated;
                }
                return existing;
            }

            var created = createIfMissing();
            _lock.EnterWriteLock();
            try
            {
                _map[hash] = created;
                SaveToDisk_NoLock();
            }
            finally { _lock.ExitWriteLock(); }
            return created;
        }
        finally { _lock.ExitUpgradeableReadLock(); }
    }

    // ---- Persistence ----

    private void LoadFromDisk()
    {
        _lock.EnterWriteLock();
        try
        {
            if (!File.Exists(_cacheFilePath))
            {
                _map = new Dictionary<string, StoredFileEntry>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            string json = File.ReadAllText(_cacheFilePath, Encoding.UTF8);
            var dict = JsonSerializer.Deserialize<Dictionary<string, StoredFileEntry>>(json, JsonOpts)
                       ?? new Dictionary<string, StoredFileEntry>(StringComparer.OrdinalIgnoreCase);

            // Ensure comparer (re-wrap if needed)
            _map = new Dictionary<string, StoredFileEntry>(dict, StringComparer.OrdinalIgnoreCase);
        }
        finally { _lock.ExitWriteLock(); }
    }

    private void SaveToDisk_NoLock()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_cacheFilePath)!);

        string tmp = _cacheFilePath + ".tmp";
        string json = JsonSerializer.Serialize(_map, JsonOpts);
        File.WriteAllText(tmp, json, Encoding.UTF8);

        if (File.Exists(_cacheFilePath))
        {
            // Atomic replace with backup (best effort)
            string bak = _cacheFilePath + ".bak";
            try { File.Replace(tmp, _cacheFilePath, bak, ignoreMetadataErrors: true); }
            catch
            {
                // Fallback if Replace fails on some FS
                File.Delete(_cacheFilePath);
                File.Move(tmp, _cacheFilePath);
            }
        }
        else
        {
            File.Move(tmp, _cacheFilePath);
        }
    }
}
