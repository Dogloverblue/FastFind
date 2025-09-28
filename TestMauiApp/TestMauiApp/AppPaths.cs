using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMauiApp.FileSearch
{
    static class AppPaths
    {
        private const string App = "FastFind";

        public static string LocalBase =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), App);

        public static string CacheDir => EnsureDir(Path.Combine(LocalBase, "cache"));
        public static string IndexDir => EnsureDir(Path.Combine(LocalBase, "search_index"));

        public static string EntryCacheFile => Path.Combine(CacheDir, "entry_cache.json");
        public static string SettingsFile => Path.Combine(LocalBase, "settings.json");

        private static string EnsureDir(string dir)
        {
            Directory.CreateDirectory(dir);
            return dir;
        }
    }
}