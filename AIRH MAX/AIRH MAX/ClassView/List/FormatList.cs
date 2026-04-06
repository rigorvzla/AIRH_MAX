namespace AIRH_MAX.ClassView.List
{
    public class FormatList
    {
        public static List<string> compressExtensions = new List<string>
        {
           ".7z",".zip",".rar",".iso",".tar",".gz",".bz2",".xz",".wim",".arj",".cab",".lzk",".cpio",".deb", ".rmp",".nsis",".chm",".z",".dmg",".hfs",".msi"
        };

        public static List<string> videoExtensions = new List<string>
        {
            ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".mpeg", ".3gp", ".ogg", ".rm", ".rmvb", ".xvid",
            ".asf", ".ts", ".vob", ".mpg", ".m2ts", ".divx", ".xvid", ".dat", ".f4v", ".h264", ".hevc", ".vp9"
        };

        public static List<string> audioExtensions = new List<string>
        {
                ".mp3", ".wav", ".flac", ".ogg", ".aac", ".wma", ".m4a", ".opus", ".ac3", ".mid", ".midi",
                ".amr", ".aiff", ".ape", ".alac", ".wv", ".pcm", ".dts", ".ra", ".mp2", ".mka"
        };

        public static List<string> imageExtensions = new List<string>
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".ico", ".svg", ".webp",
            ".jif", ".jfif", ".jfi", ".jp2", ".jpx", ".j2k", ".j2c", ".fpx", ".pcd", ".raw",
            ".tif", ".tga", ".exif", ".dng", ".cr2", ".nef", ".arw", ".orf", ".rw2", ".pef",
            ".sr2", ".raf", ".x3f", ".erf", ".svgz", ".eps", ".ai", ".cur"
        };

        public static List<string> documentExtensions = new List<string>
        {
            ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".pdf", ".txt", ".rtf", ".odt",
            ".ods", ".odp", ".csv", ".xml", ".html", ".htm", ".md", ".pages", ".numbers", ".key",
            ".ps", ".eps", ".tex", ".dot", ".dotx", ".wpd", ".xps", ".log", ".eml", ".msg",
            ".mobi", ".epub", ".azw", ".azw3", ".fb2", ".djvu", ".chm", ".pdb"
        };

        public static List<List<string>> Todo = new List<List<string>>
        {
            videoExtensions,
            audioExtensions,
            imageExtensions,
            documentExtensions,
            compressExtensions
        };
    }
}
