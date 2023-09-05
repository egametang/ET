// Better Streaming Assets, Piotr Gwiazdowski <gwiazdorrr+github at gmail.com>, 2017

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using Better;
using Better.StreamingAssets;
using Better.StreamingAssets.ZipArchive;

#if UNITY_EDITOR
using BetterStreamingAssetsImp = BetterStreamingAssets.EditorImpl;
#elif UNITY_ANDROID
using BetterStreamingAssetsImp = BetterStreamingAssets.ApkImpl;
#else
using BetterStreamingAssetsImp = BetterStreamingAssets.LooseFilesImpl;
#endif

public static partial class BetterStreamingAssets
{
    internal struct ReadInfo
    {
        public string readPath;
        public long size;
        public long offset;
        public uint crc32;
    }

    public static string Root
    {
        get { return BetterStreamingAssetsImp.s_root; }
    }

    public static void Initialize()
    {
        BetterStreamingAssetsImp.Initialize(Application.dataPath, Application.streamingAssetsPath);
    }

    /// <summary>
    /// Android only: raised when there's a Streaming Asset that is compressed. If there is no handler
    /// or it returns false, a warning will be logged.
    /// </summary>
    public static event Func<string, bool> CompressedStreamingAssetFound;

#if UNITY_EDITOR
    public static void InitializeWithExternalApk(string apkPath)
    {
        BetterStreamingAssetsImp.ApkMode = true;
        BetterStreamingAssetsImp.Initialize(apkPath, "jar:file://" + apkPath + "!/assets/");
    }

    public static void InitializeWithExternalDirectories(string dataPath, string streamingAssetsPath)
    {
        BetterStreamingAssetsImp.ApkMode = false;
        BetterStreamingAssetsImp.Initialize(dataPath, streamingAssetsPath);
    }
#endif

    public static bool FileExists(string path)
    {
        ReadInfo info;
        return BetterStreamingAssetsImp.TryGetInfo(path, out info);
    }

    public static bool DirectoryExists(string path)
    {
        return BetterStreamingAssetsImp.DirectoryExists(path);
    }

    public static AssetBundleCreateRequest LoadAssetBundleAsync(string path, uint crc = 0)
    {
        var info = GetInfoOrThrow(path);
        return AssetBundle.LoadFromFileAsync(info.readPath, crc, (ulong)info.offset);
    }

    public static AssetBundle LoadAssetBundle(string path, uint crc = 0)
    {
        var info = GetInfoOrThrow(path);
        return AssetBundle.LoadFromFile(info.readPath, crc, (ulong)info.offset);
    }

    public static System.IO.Stream OpenRead(string path)
    {
        if ( path == null )
            throw new ArgumentNullException("path");
        if ( path.Length == 0 )
            throw new ArgumentException("Empty path", "path");

        return BetterStreamingAssetsImp.OpenRead(path);
    }

    public static System.IO.StreamReader OpenText(string path)
    {
        Stream str = OpenRead(path);
        try
        {
            return new StreamReader(str);
        }
        catch (System.Exception)
        {
            if (str != null)
                str.Dispose();
            throw;
        }
    }

    public static string ReadAllText(string path)
    {
        using ( var sr = OpenText(path) )
        {
            return sr.ReadToEnd();
        }
    }

    public static string[] ReadAllLines(string path)
    {
        string line;
        var lines = new List<string>();

        using ( var sr = OpenText(path) )
        {
            while ( ( line = sr.ReadLine() ) != null )
            {
                lines.Add(line);
            }
        }

        return lines.ToArray();
    }

    public static byte[] ReadAllBytes(string path)
    {
        if ( path == null )
            throw new ArgumentNullException("path");
        if ( path.Length == 0 )
            throw new ArgumentException("Empty path", "path");

        return BetterStreamingAssetsImp.ReadAllBytes(path);
    }

    public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return BetterStreamingAssetsImp.GetFiles(path, searchPattern, searchOption);
    }

    public static string[] GetFiles(string path)
    {
        return GetFiles(path, null);
    }

    public static string[] GetFiles(string path, string searchPattern)
    {
        return GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
    }

    private static ReadInfo GetInfoOrThrow(string path)
    {
        ReadInfo result;
        if ( !BetterStreamingAssetsImp.TryGetInfo(path, out result) )
            ThrowFileNotFound(path);
        return result;
    }

    private static void ThrowFileNotFound(string path)
    {
        throw new FileNotFoundException("File not found", path);
    }

    static partial void AndroidIsCompressedFileStreamingAsset(string path, ref bool result);

#if UNITY_EDITOR
    internal static class EditorImpl
    {
        public static bool ApkMode = false;

        public static string s_root
        {
            get { return ApkMode ? ApkImpl.s_root : LooseFilesImpl.s_root; }
        }

        internal static void Initialize(string dataPath, string streamingAssetsPath)
        {
            if ( ApkMode )
            {
                ApkImpl.Initialize(dataPath, streamingAssetsPath);
            }
            else
            {
                LooseFilesImpl.Initialize(dataPath, streamingAssetsPath);
            }
        }

        internal static bool TryGetInfo(string path, out ReadInfo info)
        {
            if ( ApkMode )
                return ApkImpl.TryGetInfo(path, out info);
            else
                return LooseFilesImpl.TryGetInfo(path, out info);
        }

        internal static bool DirectoryExists(string path)
        {
            if ( ApkMode )
                return ApkImpl.DirectoryExists(path);
            else
                return LooseFilesImpl.DirectoryExists(path);
        }

        internal static Stream OpenRead(string path)
        {
            if ( ApkMode )
                return ApkImpl.OpenRead(path);
            else
                return LooseFilesImpl.OpenRead(path);
        }

        internal static byte[] ReadAllBytes(string path)
        {
            if ( ApkMode )
                return ApkImpl.ReadAllBytes(path);
            else
                return LooseFilesImpl.ReadAllBytes(path);
        }

        internal static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if ( ApkMode )
                return ApkImpl.GetFiles(path, searchPattern, searchOption);
            else
                return LooseFilesImpl.GetFiles(path, searchPattern, searchOption);
        }
    }
#endif

#if UNITY_EDITOR || !UNITY_ANDROID
    internal static class LooseFilesImpl
    {
        public static string s_root;
        private static string[] s_emptyArray = new string[0];

        public static void Initialize(string dataPath, string streamingAssetsPath)
        {
            s_root = Path.GetFullPath(streamingAssetsPath).Replace('\\', '/').TrimEnd('/');
        }

        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if (!Directory.Exists(s_root))
                return s_emptyArray;

            // this will throw if something is fishy
            path = PathUtil.NormalizeRelativePath(path, forceTrailingSlash : true);

            Debug.Assert(s_root.Last() != '\\' && s_root.Last() != '/' && path.StartsWith("/"));

            var files = Directory.GetFiles(s_root + path, searchPattern ?? "*", searchOption);

            for ( int i = 0; i < files.Length; ++i )
            {
                Debug.Assert(files[i].StartsWith(s_root));
                files[i] = files[i].Substring(s_root.Length + 1).Replace('\\', '/');
            }

#if UNITY_EDITOR
            // purge meta files
            {
                int j = 0;
                for ( int i = 0; i < files.Length; ++i )
                {
                    if ( !files[i].EndsWith(".meta") )
                    {
                        files[j++] = files[i];
                    }
                }
                Array.Resize(ref files, j);
            }

#endif
            return files;
        }

        public static bool TryGetInfo(string path, out ReadInfo info)
        {
            path = PathUtil.NormalizeRelativePath(path);

            info = new ReadInfo();

            var fullPath = s_root + path;
            if ( !File.Exists(fullPath) )
                return false;

            info.readPath = fullPath;
            return true;
        }

        public static bool DirectoryExists(string path)
        {
            var normalized = PathUtil.NormalizeRelativePath(path);
            return Directory.Exists(s_root + normalized);
        }

        public static byte[] ReadAllBytes(string path)
        {
            ReadInfo info;

            if ( !TryGetInfo(path, out info) )
                ThrowFileNotFound(path);

            return File.ReadAllBytes(info.readPath);
        }

        public static System.IO.Stream OpenRead(string path)
        {
            ReadInfo info;
            if ( !TryGetInfo(path, out info) )
                ThrowFileNotFound(path);

            Stream fileStream = File.OpenRead(info.readPath);
            try
            {
                return new SubReadOnlyStream(fileStream, leaveOpen: false);
            }
            catch ( System.Exception )
            {
                fileStream.Dispose();
                throw;
            }
        }
    }
#endif

#if UNITY_EDITOR || UNITY_ANDROID
    internal static class ApkImpl
    {
        private static string[] s_paths;
        private static PartInfo[] s_streamingAssets;
        public static string s_root;

        private struct PartInfo
        {
            public long size;
            public long offset;
            public uint crc32;
        }

        public static void Initialize(string dataPath, string streamingAssetsPath)
        {
            s_root = dataPath;

            List<string> paths = new List<string>();
            List<PartInfo> parts = new List<PartInfo>();

            GetStreamingAssetsInfoFromJar(s_root, paths, parts);

            if (paths.Count == 0 && !Application.isEditor && Path.GetFileName(dataPath) != "base.apk")
            {
                // maybe split?
                var newDataPath = Path.GetDirectoryName(dataPath) + "/base.apk";
                if (File.Exists(newDataPath))
                {
                    s_root = newDataPath;
                    GetStreamingAssetsInfoFromJar(newDataPath, paths, parts);
                }
            }

            s_paths = paths.ToArray();
            s_streamingAssets = parts.ToArray();
        }

        public static bool TryGetInfo(string path, out ReadInfo info)
        {
            path = PathUtil.NormalizeRelativePath(path);
            info = new ReadInfo();

            var index = Array.BinarySearch(s_paths, path, StringComparer.OrdinalIgnoreCase);
            if ( index < 0 )
                return false;

            var dataInfo = s_streamingAssets[index];
            info.crc32 = dataInfo.crc32;
            info.offset = dataInfo.offset;
            info.size = dataInfo.size;
            info.readPath = s_root;
            return true;
        }

        public static bool DirectoryExists(string path)
        {
            var normalized = PathUtil.NormalizeRelativePath(path, forceTrailingSlash : true);
            var dirIndex = GetDirectoryIndex(normalized);
            return dirIndex >= 0 && dirIndex < s_paths.Length;
        }

        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if ( path == null )
                throw new ArgumentNullException("path");

            var actualDirPath = PathUtil.NormalizeRelativePath(path, forceTrailingSlash : true);

            // find first file there
            var index = GetDirectoryIndex(actualDirPath);
            if ( index < 0 )
                throw new IOException();
            if ( index == s_paths.Length )
                throw new DirectoryNotFoundException();

            Predicate<string> filter;
            if ( string.IsNullOrEmpty(searchPattern) || searchPattern == "*" )
            {
                filter = null;
            }
            else if ( searchPattern.IndexOf('*') >= 0 || searchPattern.IndexOf('?') >= 0 )
            {
                var regex = PathUtil.WildcardToRegex(searchPattern);
                filter = (x) => regex.IsMatch(x);
            }
            else
            {
                filter = (x) => string.Compare(x, searchPattern, true) == 0;
            }

            List<string> results = new List<string>();

            for ( int i = index; i < s_paths.Length; ++i )
            {
                var filePath = s_paths[i];

                if ( !filePath.StartsWith(actualDirPath) )
                    break;

                string fileName;

                var dirSeparatorIndex = filePath.LastIndexOf('/', filePath.Length - 1, filePath.Length - actualDirPath.Length);
                if ( dirSeparatorIndex >= 0 )
                {
                    if ( searchOption == SearchOption.TopDirectoryOnly )
                        continue;

                    fileName = filePath.Substring(dirSeparatorIndex + 1);
                }
                else
                {
                    fileName = filePath.Substring(actualDirPath.Length);
                }

                // now do a match
                if ( filter == null || filter(fileName) )
                {
                    Debug.Assert(filePath[0] == '/');
                    results.Add(filePath.Substring(1));
                }
            }

            return results.ToArray();
        }

        public static byte[] ReadAllBytes(string path)
        {
            ReadInfo info;
            if ( !TryGetInfo(path, out info) )
                ThrowFileNotFound(path);

            byte[] buffer;
            using ( var fileStream = File.OpenRead(info.readPath) )
            {
                if ( info.offset != 0 )
                {
                    if ( fileStream.Seek(info.offset, SeekOrigin.Begin) != info.offset )
                        throw new IOException();
                }

                if ( info.size > (long)int.MaxValue )
                    throw new IOException();

                int count = (int)info.size;
                int offset = 0;

                buffer = new byte[count];
                while ( count > 0 )
                {
                    int num = fileStream.Read(buffer, offset, count);
                    if ( num == 0 )
                        throw new EndOfStreamException();
                    offset += num;
                    count -= num;
                }
            }

            return buffer;
        }

        public static System.IO.Stream OpenRead(string path)
        {
            ReadInfo info;
            if ( !TryGetInfo(path, out info) )
                ThrowFileNotFound(path);

            Stream fileStream = File.OpenRead(info.readPath);
            try
            {
                return new SubReadOnlyStream(fileStream, info.offset, info.size, leaveOpen : false);
            }
            catch ( System.Exception )
            {
                fileStream.Dispose();
                throw;
            }
        }

        private static int GetDirectoryIndex(string path)
        {
            Debug.Assert(s_paths != null);

            // find first file there
            var index = Array.BinarySearch(s_paths, path, StringComparer.OrdinalIgnoreCase);
            if ( index >= 0 )
                return ~index;

            // if the end, no such directory exists
            index = ~index;
            if ( index == s_paths.Length )
                return index;

            for ( int i = index; i < s_paths.Length && s_paths[i].StartsWith(path); ++i )
            {
                // because otherwise there would be a match
                Debug.Assert(s_paths[i].Length > path.Length);

                if ( path[path.Length - 1] == '/' )
                    return i;

                if ( s_paths[i][path.Length] == '/' )
                    return i;
            }

            return s_paths.Length;
        }

        private static void GetStreamingAssetsInfoFromJar(string apkPath, List<string> paths, List<PartInfo> parts)
        {
            using ( var stream = File.OpenRead(apkPath) )
            using ( var reader = new BinaryReader(stream) )
            {
                if ( !stream.CanRead )
                    throw new ArgumentException();
                if ( !stream.CanSeek )
                    throw new ArgumentException();

                long expectedNumberOfEntries;
                long centralDirectoryStart;
                ZipArchiveUtils.ReadEndOfCentralDirectory(stream, reader, out expectedNumberOfEntries, out centralDirectoryStart);

                try
                {
                    stream.Seek(centralDirectoryStart, SeekOrigin.Begin);

                    long numberOfEntries = 0;

                    ZipCentralDirectoryFileHeader header;

                    const int prefixLength = 7;
                    const string prefix = "assets/";
                    const string assetsPrefix = "assets/bin/";
                    Debug.Assert(prefixLength == prefix.Length);

                    while ( ZipCentralDirectoryFileHeader.TryReadBlock(reader, out header) )
                    {
                        if ( header.CompressedSize != header.UncompressedSize )
                        {
#if UNITY_ASSERTIONS
                            var fileName = Encoding.UTF8.GetString(header.Filename);
                            if (fileName.StartsWith(prefix) && !fileName.StartsWith(assetsPrefix))
                            {
                                bool isStreamingAsset = true;
                                AndroidIsCompressedFileStreamingAsset(fileName, ref isStreamingAsset);
                                if (!isStreamingAsset)
                                {
                                    // partial method ignored it
                                }
                                else if (CompressedStreamingAssetFound?.Invoke(fileName) == true)
                                {
                                    // handler ignored it
                                }
                                else 
                                { 
                                    Debug.LogAssertionFormat($"BetterStreamingAssets: file {fileName} is where Streaming Assets are put, but is compressed. " +
                                        $"If this is a App Bundle build, see README.md for a possible workaround. " +
                                        $"If this file is not a Streaming Asset (has been on purpose by hand or by another plug-in), handle " +
                                        $"{nameof(CompressedStreamingAssetFound)} event or implement " +
                                        $"{nameof(AndroidIsCompressedFileStreamingAsset)} partial method to prevent " +
                                        $"this message from appearing again. ");
                                }
                            }
#endif
                            // we only want uncompressed files
                        }
                        else
                        {
                            var fileName = Encoding.UTF8.GetString(header.Filename);
                            
                            if (fileName.EndsWith("/"))
                            {
                                // there's some strangeness when it comes to OBB: directories are listed as files
                                // simply ignoring them should be enough
                                Debug.Assert(header.UncompressedSize == 0);
                            }
                            else if ( fileName.StartsWith(prefix) )
                            {
                                // ignore normal assets...
                                if ( fileName.StartsWith(assetsPrefix) )
                                {
                                    // Note: if you put bin directory in your StreamingAssets you will get false negative here
                                }
                                else
                                {
                                    var relativePath = fileName.Substring(prefixLength - 1);
                                    var entry = new PartInfo()
                                    {
                                        crc32 = header.Crc32,
                                        offset = header.RelativeOffsetOfLocalHeader, // this offset will need fixing later on
                                        size = header.UncompressedSize
                                    };

                                    var index = paths.BinarySearch(relativePath, StringComparer.OrdinalIgnoreCase);
                                    if ( index >= 0 )
                                        throw new System.InvalidOperationException("Paths duplicate! " + fileName);

                                    paths.Insert(~index, relativePath);
                                    parts.Insert(~index, entry);
                                }
                            }
                        }

                        numberOfEntries++;
                    }

                    if ( numberOfEntries != expectedNumberOfEntries )
                        throw new ZipArchiveException("Number of entries does not match");

                }
                catch ( EndOfStreamException ex )
                {
                    throw new ZipArchiveException("CentralDirectoryInvalid", ex);
                }

                // now fix offsets
                for ( int i = 0; i < parts.Count; ++i )
                {
                    var entry = parts[i];
                    stream.Seek(entry.offset, SeekOrigin.Begin);

                    if ( !ZipLocalFileHeader.TrySkipBlock(reader) )
                        throw new ZipArchiveException("Local file header corrupt");

                    entry.offset = stream.Position;

                    parts[i] = entry;
                }
            }
        }
    }
#endif
}

