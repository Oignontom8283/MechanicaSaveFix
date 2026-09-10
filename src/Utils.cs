using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Game.Saving;
using Game.Utilities;
using UnityEngine;

public static class Utils
{   
    #region Other Utilities

    /// <summary>
    /// Provides access to the current game context.
    /// </summary>
    public static class GameContext
    {
        public static string savesFolderPath => Singleton<SaveManager>.Instance.pGameSavesFolderPath;
    }

    private static string[] RemoveEmptyEntries(string[] source)
    {
        int count = 0;
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i].Length > 0) count++;
        }

        string[] result = new string[count];
        int idx = 0;
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i].Length > 0)
            {
                result[idx] = source[i];
                idx++;
            }
        }
        return result;
    }

    #endregion

    #region Path Utilities

    /// <summary>
    /// Reproduces Path.GetRelativePath(string, string) from .NET Core,
    /// compatible with .NET Framework 1.0.
    /// </summary>
    public static string GetRelativePath(string relativeTo, string path)
    {
        if (relativeTo == null)
            throw new ArgumentNullException("relativeTo");
        if (path == null)
            throw new ArgumentNullException("path");
        if (relativeTo.Length == 0)
            throw new ArgumentException("The value cannot be empty.", "relativeTo");
        if (path.Length == 0)
            throw new ArgumentException("The value cannot be empty.", "path");

        string fullRelativeTo = Path.GetFullPath(relativeTo);
        string fullPath = Path.GetFullPath(path);

        string rootRelativeTo = Path.GetPathRoot(fullRelativeTo);
        string rootPath = Path.GetPathRoot(fullPath);

        if (string.Compare(rootRelativeTo, rootPath, true, CultureInfo.InvariantCulture) != 0)
        {
            return fullPath;
        }

        char[] separators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        string[] splitRelativeTo = fullRelativeTo.Split(separators);
        string[] splitPath = fullPath.Split(separators);

        splitRelativeTo = RemoveEmptyEntries(splitRelativeTo);
        splitPath = RemoveEmptyEntries(splitPath);

        int commonLength = 0;
        int minLength = Math.Min(splitRelativeTo.Length, splitPath.Length);

        while (commonLength < minLength &&
               string.Compare(splitRelativeTo[commonLength], splitPath[commonLength], true, CultureInfo.InvariantCulture) == 0)
        {
            commonLength++;
        }

        StringBuilder sb = new StringBuilder();

        for (int i = commonLength; i < splitRelativeTo.Length; i++)
        {
            if (sb.Length > 0) sb.Append(Path.DirectorySeparatorChar);
            sb.Append("..");
        }

        for (int i = commonLength; i < splitPath.Length; i++)
        {
            if (sb.Length > 0) sb.Append(Path.DirectorySeparatorChar);
            sb.Append(splitPath[i]);
        }

        if (sb.Length == 0)
            return ".";

        return sb.ToString();
    }

    /// <summary>
    /// Checks if a target path (file or directory) is located inside a parent directory.
    /// </summary>
    /// <param name="parentPath">The root directory path.</param>
    /// <param name="targetPath">The path (file or folder) to check.</param>
    /// <returns><c>true</c> if <paramref name="targetPath"/> is inside <paramref name="parentPath"/>; otherwise, <c>false</c>.</returns>
    public static bool IsSubPathOf(string parentPath, string targetPath)
    {
        string relativePath = GetRelativePath(parentPath, targetPath);

        return
            !relativePath.StartsWith("..") && // Ensures path doesn't traverse up out of the parent folder
            !Path.IsPathRooted(relativePath); // Handles edge cases on different drives/roots (e.g., C:\ vs D:\)
    }
    
    /// <summary>
    /// Sanitizes a file path by replacing backslashes with forward slashes and removing any leading slashes.
    /// </summary>
    /// <param name="path">The file path to sanitize.</param>
    /// <returns>The sanitized file path.</returns>
    /// <remarks>
    /// <b>Necessary for zip file compatibility!</b>
    /// </remarks>
    public static string SanitizePath(string path) => Utils.TrimStart(path.Replace('\\', '/'), '/');

    
    /// <summary>
    /// Specifies the type of a file system path, indicating whether it is a file or a directory.
    /// </summary>
    public enum PathType
    {
        File,
        Directory
    }

    /// <summary>
    /// Determines whether the specified path is a file or a directory.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>A <see cref="PathType"/> value indicating whether the path is a file or a directory.</returns>
    /// <exception cref="ArgumentException">Thrown when the path is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the path does not exist.</exception>
    public static PathType GetPathType(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        if (File.Exists(path) || Directory.Exists(path))
        {
            FileAttributes attr = File.GetAttributes(path);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                return PathType.Directory;
            }
            else
            {
                return PathType.File;
            }
        }
        else
        {
            throw new FileNotFoundException($"The specified path does not exist: {path}");
        }
    }

    /// <summary>
    /// Sanitizes a file or directory name by replacing invalid characters with a specified replacement character.
    /// </summary>
    /// <param name="name">The name to sanitize.</param>
    /// <param name="replacement">The character to replace invalid characters with.</param>
    /// <param name="defaultName">The default name to return if the input is null or empty.</param>
    /// <param name="spaceReplacement">If true, spaces will be replaced with the replacement character.</param>
    /// <returns>The sanitized name.</returns>
    public static string SanitizeName(string name, char replacement = '_', string defaultName = "unnamed", bool spaceReplacement = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return defaultName;

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalidChar, replacement);
        }

        if (spaceReplacement)
        {
            name = name.Replace(' ', replacement);
        }

        // Windows does not allow file names to end with a space or a period, so we trim those characters from the end of the name.
        name = name.Trim('.', ' ');

        return string.IsNullOrWhiteSpace(name) ? defaultName : name;
    }

    #endregion

    #region String Utilities

    /// <summary>
    /// Returns a name that doesn't appear in <paramref name="existingNames"/>, derived from
    /// <paramref name="desiredName"/>. If the desired name (after stripping any existing "_N"
    /// suffix) is already free, it's returned as-is. Otherwise, "_1", "_2", etc. are appended
    /// until a free name is found.
    /// </summary>
    /// <param name="desiredName">The name to start from.</param>
    /// <param name="existingNames">The names already taken.</param>
    /// <returns>A name not present in <paramref name="existingNames"/>.</returns>
    public static string MakeUniqueName(string desiredName, IEnumerable<string> existingNames)
    {
        var taken = new HashSet<string>(existingNames, StringComparer.Ordinal);

        // Strip an existing "_N" suffix (N can be negative or zero) to get the base name.
        string baseName = Regex.Replace(desiredName, @"_-?\d+$", "");

        if (!taken.Contains(baseName))
        {
            return baseName;
        }

        int suffix = 1;
        string candidate;
        do
        {
            candidate = $"{baseName}_{suffix}";
            suffix++;
        }
        while (taken.Contains(candidate));

        return candidate;
    }

    /// <summary>
    /// Converts a wildcard pattern (using '*' and '?') into a regular expression for matching file paths.
    /// </summary>
    /// <param name="pattern">The wildcard pattern to convert.</param>
    /// <returns>The equivalent regular expression.</returns>
    public static Regex WildcardToRegex(string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) pattern = "*";

        var sb = new StringBuilder();
        sb.Append('^');

        foreach (char c in pattern)
        {
            switch (c)
            {
                case '*':
                    sb.Append(".*");
                    break;
                case '?':
                    sb.Append('.');
                    break;
                default:
                    sb.Append(Regex.Escape(c.ToString()));
                    break;
            }
        }

        sb.Append('$');
        return new Regex(sb.ToString(), RegexOptions.Compiled);
    }

    /// <summary>
    /// Converts a string to a byte array using UTF-8 encoding.
    /// </summary>
    /// <param name="text">The UTF-8 string to convert.</param>
    /// <returns>The resulting byte array.</returns>
    public static byte[] TextToBytes(string text) => Encoding.UTF8.GetBytes(text);

    /// <summary>
    /// Converts a byte array to a string using UTF-8 encoding.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The resulting UTF-8 string.</returns>
    public static string BytesToText(byte[] bytes) => Encoding.UTF8.GetString(bytes);

    /// <summary>
    /// Removes all leading occurrences of a specified set of characters
    /// from the string.
    /// </summary>
    public static string TrimStart(string str, params char[] trimChars)
    {
        if (str == null)
            throw new ArgumentNullException("str");

        if (str.Length == 0)
            return str;

        int startIndex = 0;

        // Native behavior: if trimChars is null or empty, trim whitespace
        if (trimChars == null || trimChars.Length == 0)
        {
            while (startIndex < str.Length && char.IsWhiteSpace(str[startIndex]))
            {
                startIndex++;
            }
        }
        else
        {
            // Search for specified characters
            while (startIndex < str.Length)
            {
                char c = str[startIndex];
                bool match = false;

                // Classic for loop to avoid Array.IndexOf boxing in .NET 1.0
                for (int i = 0; i < trimChars.Length; i++)
                {
                    if (trimChars[i] == c)
                    {
                        match = true;
                        break;
                    }
                }

                if (!match)
                {
                    // As soon as we find a character that is not in trimChars, we stop
                    break;
                }

                startIndex++;
            }
        }

        // If nothing was removed, return the original string to avoid an allocation
        if (startIndex == 0)
            return str;

        // If the entire string was removed
        if (startIndex == str.Length)
            return string.Empty;

        return str.Substring(startIndex);
    }

    #endregion

    #region File Utilities

    /// <summary>
    /// Verifies if a file exists at the specified absolute path and is not empty.
    /// </summary>
    /// <param name="absolutePath">The absolute path to the file.</param>
    /// <returns><c>true</c> if the file exists and is not empty; otherwise, <c>false</c>.</returns>
    public static bool VerifyFileValid(string absolutePath)
    {
        var info = new FileInfo(absolutePath);

        return info.Exists && info.Length > 0;
    }

    #endregion

    #region JSON/Unity obj Utilities
    
    /// <summary>
    /// Tries to deserialize a JSON string into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
    /// <returns>The deserialized object, or the default value if deserialization fails.</returns>
    public static T FromJsonOrThrow<T>(string json)
    {
        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON to type {typeof(T).FullName}.", ex);
        }
    }

    /// <summary>
    /// Tries to deserialize a JSON string into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize (must be a class).</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized object, or null</returns>
    public static T FromJsonOrNull<T>(string json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            MechanicaSaveFix.Log.LogWarning($"Failed to deserialize JSON to type {typeof(T).FullName}: input string is null or empty.");
            return null;
        }

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception ex)
        {
            MechanicaSaveFix.Log.LogWarning($"Failed to deserialize JSON to type {typeof(T).FullName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Serializes an object to a JSON string. Throws an exception if serialization fails.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The JSON string representing the object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when serialization fails.</exception>
    public static string ToJsonOrThrow<T>(T obj)
    {
        try
        {
            return JsonUtility.ToJson(obj);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to serialize object of type {typeof(T).FullName} to JSON.", ex);
        }
    }

    /// <summary>
    /// Serializes an object to a JSON string. Returns null if serialization fails.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The JSON string representing the object, or null if serialization fails.</returns>
    public static string ToJsonOrNull<T>(T obj)
    {
        if (obj == null)
        {
            MechanicaSaveFix.Log.LogWarning($"Failed to serialize object of type {typeof(T).FullName} to JSON: object is null.");
            return null;
        }

        try
        {
            return JsonUtility.ToJson(obj);
        }
        catch (Exception ex)
        {
            MechanicaSaveFix.Log.LogWarning($"Failed to serialize object of type {typeof(T).FullName} to JSON: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Archive Utilities
    
    /// <summary>
    /// Specifies values that indicate whether a compression operation emphasizes speed or compression size.
    /// </summary>
    /// <remarks>
    /// Use a cast to convert between <see cref="SaveCompressionLevel"/> and <see cref="CompressionLevel"/>.
    /// <para><b>Example:</b></para>
    /// <code>
    /// SaveCompressionLevel level = SaveCompressionLevel.Optimal;
    /// CompressionLevel systemLevel = (CompressionLevel)level;
    /// </code>
    /// </remarks>
    public enum SaveCompressionLevel
    {
        Optimal = CompressionLevel.Optimal,
        Fast = CompressionLevel.Fastest,
        None = CompressionLevel.NoCompression
    }

    /// <summary>
    /// Reads a single file from a zip archive on disk and returns its content as a byte array.
    /// </summary>
    /// <param name="zipPath">The path to the zip archive file.</param>
    /// <param name="entryName">The name of the file entry to read.</param>
    /// <returns> The content of the file as a byte array, or <c>null</c> if the file is not found.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified zip file is not found.</exception>
    public static byte[] ReadSingleByteFileFromZip(string zipPath, string entryName)
    {
        if (!File.Exists(zipPath) && !VerifyFileValid(zipPath))
        {
            throw new FileNotFoundException($"${nameof(ReadSingleByteFileFromZip)}: Archive file not found: {zipPath}");
        }

        string sanitizedEntryName = SanitizePath(entryName);

        using (var zipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            ZipArchiveEntry entry = archive.GetEntry(sanitizedEntryName);
            if (entry == null) return null;
            
            using (var entryStream = entry.Open())
            using (var ms = new MemoryStream())
            {
                entryStream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }

    /// <summary>
    /// Reads a single text file from a zip archive on disk and returns its content as a string.
    /// </summary>
    /// <param name="zipPath">The path to the zip archive file.</param>
    /// <param name="entryName">The name of the file entry to read.</param>
    /// <returns>The content of the file as a string, or <c>null</c> if the file is not found.</returns>
    public static string ReadSingleTextFileFromZip(string zipPath, string entryName)
    {
        byte[] entryBytes = ReadSingleByteFileFromZip(zipPath, entryName);

        if (entryBytes == null)
        {
            return null;
        }
        else
        {
            return BytesToText(entryBytes);
        }
    }

    /// <summary>
    /// Adds or replaces a single binary file inside a zip archive on disk. If an entry with
    /// the same name already exists, it is removed first, then re-added with the new content.
    /// </summary>
    /// <param name="zipPath">The path to the zip archive file. Created if it doesn't exist.</param>
    /// <param name="entryName">The name of the file entry inside the archive.</param>
    /// <param name="content">The binary content to write.</param>
    /// <remarks>This method is not recommended for writing multiple files.</remarks>
    public static void WriteSingleFileToZip(string zipPath, string entryName, byte[] content)
    {
        if (File.Exists(zipPath) && !VerifyFileValid(zipPath))
        {
            throw new InvalidOperationException($"Archive file is invalid or empty: {zipPath}");
        }

        string sanitizedEntryName = SanitizePath(entryName);

        using (var zipStream = new FileStream(zipPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Update))
        {
            // ZipArchive has no "overwrite" option: remove the old entry first, if present.
            ZipArchiveEntry existingEntry = archive.GetEntry(sanitizedEntryName);
            existingEntry?.Delete();

            ZipArchiveEntry newEntry = archive.CreateEntry(sanitizedEntryName, (CompressionLevel)MechanicaSaveFix.saveCompressionLevel.Value);

            using (var entryStream = newEntry.Open())
            {
                entryStream.Write(content, 0, content.Length);
            }
        }
    }

    /// <summary>
    /// Adds or replaces a single text file inside a zip archive on disk. If an entry with
    /// the same name already exists, it is removed first, then re-added with the new content.
    /// </summary>
    /// <param name="zipPath">The path to the zip archive file. Created if it doesn't exist.</param>
    /// <param name="entryName">The name of the file entry inside the archive.</param>
    /// <param name="content">The text content to write.</param>
    /// <remarks>This method is not recommended for writing multiple files.</remarks>
    public static void WriteSingleFileToZip(string zipPath, string entryName, string content)
    {
        WriteSingleFileToZip(zipPath, entryName, TextToBytes(content));
    }

    /// <summary>
    /// Creates a new zip archive at the given path, populated with the provided entries.
    /// Overwrites the file if it already exists.
    /// </summary>
    /// <param name="zipPath">The path where the archive will be created.</param>
    /// <param name="entries">The entry name/content pairs to write into the archive.</param>
    public static void CreateArchiveWithDefaults(string zipPath, IReadOnlyDictionary<string, byte[]> entries)
    {
        using (var zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            foreach (var kvp in entries)
            {
                string sanitizedEntryName = SanitizePath(kvp.Key);
                ZipArchiveEntry entry = archive.CreateEntry(sanitizedEntryName, (CompressionLevel)MechanicaSaveFix.saveCompressionLevel.Value);

                using (var entryStream = entry.Open())
                {
                    entryStream.Write(kvp.Value, 0, kvp.Value.Length);
                }
            }
        }
    }

    /// <summary>
    /// Same as <see cref="CreateArchiveWithDefaults(string, IReadOnlyDictionary{string, byte[]})"/>,
    /// but for text entries, encoded as UTF-8.
    /// </summary>
    /// <param name="zipPath">The path where the archive will be created.</param>
    /// <param name="entries">The entry name/content pairs to write into the archive.</param>
    public static void CreateArchiveWithDefaults(string zipPath, IReadOnlyDictionary<string, string> entries)
    {
        var byteEntries = new Dictionary<string, byte[]>();
        foreach (var kvp in entries)
        {
            byteEntries[kvp.Key] = TextToBytes(kvp.Value);
        }

        CreateArchiveWithDefaults(zipPath, byteEntries);
    }

    #endregion
}
