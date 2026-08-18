using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides an interface for interacting with the <see cref="VirtualFS"/> by mirroring the methods of <see cref="System.IO"/>.
/// </summary>
/// <remarks>
/// The methods in this class are not documented individually; their purpose is the same as those in <see cref="System.IO"/>.
/// </remarks>
public static class Forward
{   
    // File operations

    public static bool FileExists(string absolutePath) =>
        VirtualFS.IsExistFile(absolutePath);

    public static void WriteAllText(string absolutePath, string contents) =>
        VirtualFS.WriteTextFile(absolutePath, contents);

    public static string ReadAllText(string absolutePath) =>
        VirtualFS.ReadTextFile(absolutePath);

    public static void WriteAllBytes(string absolutePath, byte[] bytes) =>
        VirtualFS.WriteBinaryFile(absolutePath, bytes);

    public static byte[] ReadAllBytes(string absolutePath) =>
        VirtualFS.ReadBinaryFile(absolutePath);

    public static void Delete(string absolutePath) =>
        VirtualFS.DeleteFileNoThrow(absolutePath); // Delete in System.IO does not throw an exception if the file does not exist, so we use DeleteFileNoThrow to mimic that behavior.


    // Directory operations

    public static bool DirectoryExists(string absoluteDirPath) =>
        VirtualFS.QueryEntries(absoluteDirPath, "*", recursive: true, EntryKind.Both).Any();

    // CreateDirectory is not implemented because the virtual file system does not support creating directories directly. Instead, directories are created implicitly when files are written to them.

    
    // Get files operations
    public static string[] GetFiles(string absoluteDir) =>
        VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Files).ToArray();

    public static string[] GetFiles(string absoluteDir, string searchPattern) =>
        VirtualFS.QueryEntries(absoluteDir, searchPattern, recursive: false, EntryKind.Files).ToArray();

    public static string[] GetFiles(string absoluteDir, string searchPattern, SearchOption searchOption) =>
        VirtualFS.QueryEntries(absoluteDir, searchPattern, searchOption == SearchOption.AllDirectories, EntryKind.Files).ToArray();

    
    // Get directories operations

    public static string[] GetDirectories(string absoluteDir) =>
        VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Directories).ToArray();

    public static string[] GetDirectories(string absoluteDir, string searchPattern) =>
        VirtualFS.QueryEntries(absoluteDir, searchPattern, recursive: false, EntryKind.Directories).ToArray();

    public static string[] GetDirectories(string absoluteDir, string searchPattern, SearchOption searchOption) =>
        VirtualFS.QueryEntries(absoluteDir, searchPattern, searchOption == SearchOption.AllDirectories, EntryKind.Directories).ToArray();


    // Get file system entries operations

    public static string[] GetFileSystemEntries(string absoluteDir) =>
        VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Both).ToArray();

    public static string[] GetFileSystemEntries(string absoluteDir, string searchPattern) =>
        VirtualFS.QueryEntries(absoluteDir, searchPattern, recursive: false, EntryKind.Both).ToArray();

    
    // Enumerate operations

    public static IEnumerable<string> EnumerateFiles(string absoluteDir) =>
        VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Files);

    public static IEnumerable<string> EnumerateFiles(string absoluteDir, string searchPattern) =>
        VirtualFS.QueryEntries(absoluteDir, searchPattern, recursive: false, EntryKind.Files);

    public static IEnumerable<string> EnumerateFiles(string absoluteDir, string searchPattern, SearchOption searchOption) =>
        VirtualFS.QueryEntries(absoluteDir, searchPattern, searchOption == SearchOption.AllDirectories, EntryKind.Files);

    public static IEnumerable<string> EnumerateDirectories(string absoluteDir) =>
        VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Directories);

    public static IEnumerable<string> EnumerateFileSystemEntries(string absoluteDir) =>
        VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Both);
}