using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMauiApp.FileSearch
{

  
    internal class FileTypeAliases
    {
        public readonly static Dictionary<string, HashSet<string>> FileTypeAliasesMap =
    new(StringComparer.OrdinalIgnoreCase)
{
    // ----- Documents / books -----
    { "pdf",   new HashSet<string>{ "document","file","text","write","paper","report","manual","brochure","scan","ebook" } },
    { "txt",   new HashSet<string>{ "document","file","text","write","notes","plain text","readme" } },
    { "doc",   new HashSet<string>{ "word","document","doc","paper","letter","resume","cv","contract","essay","report" } },
    { "docx",  new HashSet<string>{ "word","document","doc","paper","letter","resume","cv","contract","essay","report" } },
    { "rtf",   new HashSet<string>{ "rich text","document","wordpad","formatted text" } },
    { "odt",   new HashSet<string>{ "opendocument","document","writer","libreoffice" } },
    { "md",    new HashSet<string>{ "markdown","readme","notes","wiki","documentation" } },
    { "epub",  new HashSet<string>{ "ebook","book","reader","publication" } },
    { "mobi",  new HashSet<string>{ "ebook","kindle","book" } },
    { "azw",   new HashSet<string>{ "ebook","kindle","book" } },
    { "azw3",  new HashSet<string>{ "ebook","kindle","book" } },
    { "djvu",  new HashSet<string>{ "ebook","scan","book" } },

    // ----- Presentations -----
    { "ppt",   new HashSet<string>{ "presentation","slides","deck","talk","pitch" } },
    { "pptx",  new HashSet<string>{ "presentation","slides","deck","talk","pitch" } },
    { "odp",   new HashSet<string>{ "presentation","slides","deck","impress","libreoffice" } },

    // ----- Spreadsheets / data -----
    { "xls",   new HashSet<string>{ "spreadsheet","excel","sheet","table","budget","ledger" } },
    { "xlsx",  new HashSet<string>{ "spreadsheet","excel","sheet","table","budget","ledger" } },
    { "ods",   new HashSet<string>{ "spreadsheet","sheet","calc","libreoffice" } },
    { "csv",   new HashSet<string>{ "data","dataset","spreadsheet","table","export","comma separated" } },
    { "tsv",   new HashSet<string>{ "data","dataset","spreadsheet","table","export","tab separated" } },
    { "sqlite",new HashSet<string>{ "database","db","sqlite","data" } },
    { "db",    new HashSet<string>{ "database","db","data" } },
    { "mdb",   new HashSet<string>{ "database","access","db" } },
    { "accdb", new HashSet<string>{ "database","access","db" } },
    { "sql",   new HashSet<string>{ "database","sql","dump","query","schema" } },
    { "parquet", new HashSet<string>{ "data","dataset","analytics","columnar" } },

    // ----- Images / graphics -----
    { "png",   new HashSet<string>{ "image","picture","photo","photograph","art","screenshot","graphic" } },
    { "jpg",   new HashSet<string>{ "image","picture","photo","photograph","art","jpeg","snapshot" } },
    { "jpeg",  new HashSet<string>{ "image","picture","photo","photograph","art","snapshot" } },
    { "webp",  new HashSet<string>{ "image","picture","photo","photograph","art","web image" } },
    { "gif",   new HashSet<string>{ "image","picture","photo","photograph","art","animation" } },
    { "bmp",   new HashSet<string>{ "image","bitmap","picture","photo","scan" } },
    { "tif",   new HashSet<string>{ "image","tiff","scan","picture","photo","photograph" } },
    { "tiff",  new HashSet<string>{ "image","scan","picture","photo","photograph" } },
    { "heic",  new HashSet<string>{ "image","iphone photo","picture","photo" } },
    { "heif",  new HashSet<string>{ "image","iphone photo","picture","photo" } },
    { "svg",   new HashSet<string>{ "vector","icon","logo","graphic" } },
    { "ai",    new HashSet<string>{ "illustrator","vector","logo","design","graphic" } },
    { "eps",   new HashSet<string>{ "vector","logo","print","graphic" } },
    { "psd",   new HashSet<string>{ "photoshop","layers","design","graphic","image" } },
    { "ico",   new HashSet<string>{ "icon","favicon","app icon" } },
    { "cr2",   new HashSet<string>{ "raw","camera raw","canon","photo" } },
    { "nef",   new HashSet<string>{ "raw","camera raw","nikon","photo" } },
    { "arw",   new HashSet<string>{ "raw","camera raw","sony","photo" } },
    { "raw",   new HashSet<string>{ "raw","camera raw","photo" } },
    { "indd",  new HashSet<string>{ "indesign","layout","magazine","brochure","design" } },
    { "sketch",new HashSet<string>{ "design","ui","vector","mac app" } },
    { "fig",   new HashSet<string>{ "figma","design","ui","prototype" } },

    // ----- Audio -----
    { "mp3",   new HashSet<string>{ "audio","music","song","recording","voice","podcast","audiobook" } },
    { "wav",   new HashSet<string>{ "audio","recording","sound","voice","music" } },
    { "flac",  new HashSet<string>{ "audio","lossless","music","song" } },
    { "m4a",   new HashSet<string>{ "audio","music","itunes","song" } },
    { "aac",   new HashSet<string>{ "audio","music","song" } },
    { "ogg",   new HashSet<string>{ "audio","music","song","vorbis" } },
    { "wma",   new HashSet<string>{ "audio","music","song" } },
    { "aiff",  new HashSet<string>{ "audio","recording","sound","aif","music" } },
    { "aif",   new HashSet<string>{ "audio","recording","sound","music" } },
    { "opus",  new HashSet<string>{ "audio","voice","call","recording" } },
    { "mid",   new HashSet<string>{ "midi","music","instrument","sequence" } },
    { "midi",  new HashSet<string>{ "midi","music","instrument","sequence" } },

    // ----- Video -----
    { "mp4",   new HashSet<string>{ "video","clip","movie","film","recording","screen recording","lecture" } },
    { "mkv",   new HashSet<string>{ "video","clip","movie","film" } },
    { "mov",   new HashSet<string>{ "video","clip","movie","film","apple","quicktime" } },
    { "avi",   new HashSet<string>{ "video","clip","movie","film" } },
    { "wmv",   new HashSet<string>{ "video","clip","movie","film" } },
    { "webm",  new HashSet<string>{ "video","clip","movie","film","web video" } },
    { "m4v",   new HashSet<string>{ "video","clip","movie","film" } },
    { "flv",   new HashSet<string>{ "video","clip","movie","film" } },
    { "3gp",   new HashSet<string>{ "video","phone video","clip" } },
    { "srt",   new HashSet<string>{ "subtitles","captions","closed captions" } },
    { "vtt",   new HashSet<string>{ "subtitles","captions","webvtt" } },

    // ----- Archives / packages / images -----
    { "zip",   new HashSet<string>{ "archive","compressed","zip file","package","bundle" } },
    { "rar",   new HashSet<string>{ "archive","compressed","package","bundle" } },
    { "7z",    new HashSet<string>{ "archive","compressed","package","bundle" } },
    { "tar",   new HashSet<string>{ "archive","compressed","package","bundle" } },
    { "gz",    new HashSet<string>{ "gzip","compressed","archive" } },
    { "bz2",   new HashSet<string>{ "bzip2","compressed","archive" } },
    { "xz",    new HashSet<string>{ "compressed","archive" } },
    { "tgz",   new HashSet<string>{ "archive","compressed","tar.gz" } },
    { "iso",   new HashSet<string>{ "disk image","installer","os image","iso" } },
    { "img",   new HashSet<string>{ "disk image","image file" } },

    // ----- Executables / installers / scripts -----
    { "exe",   new HashSet<string>{ "app","program","application","executable","installer","setup" } },
    { "msi",   new HashSet<string>{ "installer","setup","windows installer" } },
    { "bat",   new HashSet<string>{ "batch","script","cmd","windows script" } },
    { "cmd",   new HashSet<string>{ "batch","script","cmd","windows script" } },
    { "ps1",   new HashSet<string>{ "powershell","script","windows script" } },
    { "sh",    new HashSet<string>{ "shell","script","bash" } },
    { "apk",   new HashSet<string>{ "android app","apk","installer" } },
    { "ipa",   new HashSet<string>{ "ios app","iphone app","installer" } },
    { "dmg",   new HashSet<string>{ "mac installer","disk image","app" } },
    { "pkg",   new HashSet<string>{ "mac installer","package","installer" } },
    { "appimage", new HashSet<string>{ "linux app","portable app","installer" } },

    // ----- Web / code / config -----
    { "html",  new HashSet<string>{ "webpage","site","html","index" } },
    { "htm",   new HashSet<string>{ "webpage","site","html","index" } },
    { "css",   new HashSet<string>{ "stylesheet","style","css" } },
    { "js",    new HashSet<string>{ "javascript","script","frontend" } },
    { "ts",    new HashSet<string>{ "typescript","script","frontend" } },
    { "jsx",   new HashSet<string>{ "react","javascript","component" } },
    { "tsx",   new HashSet<string>{ "react","typescript","component" } },
    { "json",  new HashSet<string>{ "json","config","settings","manifest","data" } },
    { "yaml",  new HashSet<string>{ "yaml","yml","config","settings" } },
    { "yml",   new HashSet<string>{ "yaml","config","settings" } },
    { "xml",   new HashSet<string>{ "xml","config","settings","feed" } },
    { "ini",   new HashSet<string>{ "config","settings","ini" } },
    { "conf",  new HashSet<string>{ "config","settings" } },
    { "log",   new HashSet<string>{ "log","logs","diagnostics","trace" } },
    { "ipynb", new HashSet<string>{ "notebook","jupyter","analysis","data science" } },
    { "py",    new HashSet<string>{ "python","script","code" } },
    { "java",  new HashSet<string>{ "java","source","code" } },
    { "cs",    new HashSet<string>{ "c#","csharp","source","code" } },
    { "cpp",   new HashSet<string>{ "c++","source","code" } },
    { "c",     new HashSet<string>{ "c","source","code" } },
    { "h",     new HashSet<string>{ "header","c header","c++ header" } },
    { "hpp",   new HashSet<string>{ "header","c++ header" } },
    { "kt",    new HashSet<string>{ "kotlin","source","code" } },
    { "swift", new HashSet<string>{ "swift","source","code" } },
    { "go",    new HashSet<string>{ "golang","source","code" } },
    { "rs",    new HashSet<string>{ "rust","source","code" } },
    { "php",   new HashSet<string>{ "php","script","server" } },
    { "rb",    new HashSet<string>{ "ruby","script","source" } },
    { "psd1",  new HashSet<string>{ "powershell","module","manifest" } },

    // ----- System / misc -----
    { "bak",   new HashSet<string>{ "backup","old","previous version" } },
    { "tmp",   new HashSet<string>{ "temp","temporary","cache" } },
    { "torrent", new HashSet<string>{ "torrent","magnet","download" } },
};


        internal static IEnumerable<string> GetTagAliases(string inputFileExtension)
        {
            if (!FileTypeAliasesMap.ContainsKey(inputFileExtension))
            {
                Console.WriteLine("does not contain file type:" + inputFileExtension);
                return ImmutableList.Create(inputFileExtension);
            }
            var fileAliases = FileTypeAliasesMap[inputFileExtension];
            return ImmutableList.Create(inputFileExtension).Concat(fileAliases);

        }
    }


}
