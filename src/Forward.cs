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

    public static bool FileExists(string absolutePath)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(FileExists)}: {absolutePath}");
        return VirtualFS.IsExistFile(absolutePath);
    }

    public static void WriteAllText(string absolutePath, string contents)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(WriteAllText)}: {absolutePath}");
        VirtualFS.WriteTextFile(absolutePath, contents);
    }

    public static string ReadAllText(string absolutePath)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(ReadAllText)}: {absolutePath}");
        return VirtualFS.ReadTextFile(absolutePath);
    }

    public static void WriteAllBytes(string absolutePath, byte[] bytes)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(WriteAllBytes)}: {absolutePath}");
        VirtualFS.WriteBinaryFile(absolutePath, bytes);
    }

    public static byte[] ReadAllBytes(string absolutePath)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(ReadAllBytes)}: {absolutePath}");
        return VirtualFS.ReadBinaryFile(absolutePath);
    }

    public static void Delete(string absolutePath)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(Delete)}: {absolutePath}");
        VirtualFS.DeleteFile(absolutePath);
    }


    // Directory operations

    public static bool DirectoryExists(string absoluteDirPath)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(DirectoryExists)}: {absoluteDirPath}");
        return VirtualFS.IsExistDirectory(absoluteDirPath);
    }

    public static void DeleteDirectory(string absolutePath, bool recursive)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(DeleteDirectory)}: {absolutePath}, recursive: {recursive}");
        VirtualFS.DeleteDirectory(absolutePath, recursive);
    }
        
    
    // Get files operations
    public static string[] GetFiles(string absoluteDir)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(GetFiles)}: {absoluteDir}");
        return VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Files).ToArray();
    }

    public static string[] GetFiles(string absoluteDir, string searchPattern)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(GetFiles)}: {absoluteDir}, searchPattern: {searchPattern}");
        return VirtualFS.QueryEntries(absoluteDir, searchPattern, recursive: false, EntryKind.Files).ToArray();
    }

    public static string[] GetFiles(string absoluteDir, string searchPattern, SearchOption searchOption)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(GetFiles)}: {absoluteDir}, searchPattern: {searchPattern}, searchOption: {searchOption}");
        return VirtualFS.QueryEntries(absoluteDir, searchPattern, searchOption == SearchOption.AllDirectories, EntryKind.Files).ToArray();
    }

    
    // Get directories operations

    public static string[] GetDirectories(string absoluteDir)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(GetDirectories)}: {absoluteDir}");
        return VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Directories).ToArray();
    }

    public static string[] GetDirectories(string absoluteDir, string searchPattern)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(GetDirectories)}: {absoluteDir}, searchPattern: {searchPattern}");
        return VirtualFS.QueryEntries(absoluteDir, searchPattern, recursive: false, EntryKind.Directories).ToArray();
    }

    public static string[] GetDirectories(string absoluteDir, string searchPattern, SearchOption searchOption)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(GetDirectories)}: {absoluteDir}, searchPattern: {searchPattern}, searchOption: {searchOption}");
        return VirtualFS.QueryEntries(absoluteDir, searchPattern, searchOption == SearchOption.AllDirectories, EntryKind.Directories).ToArray();
    }


    // Get file system entries operations

    public static string[] GetFileSystemEntries(string absoluteDir)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(GetFileSystemEntries)}: {absoluteDir}");
        return VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Both).ToArray();
    }

    public static string[] GetFileSystemEntries(string absoluteDir, string searchPattern)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(GetFileSystemEntries)}: {absoluteDir}, searchPattern: {searchPattern}");
        return VirtualFS.QueryEntries(absoluteDir, searchPattern, recursive: false, EntryKind.Both).ToArray();
    }

    
    // Enumerate operations

    public static IEnumerable<string> EnumerateFiles(string absoluteDir)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(EnumerateFiles)}: {absoluteDir}");
        return VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Files);
    }

    public static IEnumerable<string> EnumerateFiles(string absoluteDir, string searchPattern)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(EnumerateFiles)}: {absoluteDir}, searchPattern: {searchPattern}");
        return VirtualFS.QueryEntries(absoluteDir, searchPattern, recursive: false, EntryKind.Files);
    }

    public static IEnumerable<string> EnumerateFiles(string absoluteDir, string searchPattern, SearchOption searchOption)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(EnumerateFiles)}: {absoluteDir}, searchPattern: {searchPattern}, searchOption: {searchOption}");
        return VirtualFS.QueryEntries(absoluteDir, searchPattern, searchOption == SearchOption.AllDirectories, EntryKind.Files);
    }

    public static IEnumerable<string> EnumerateDirectories(string absoluteDir)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(EnumerateDirectories)}: {absoluteDir}");
        return VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Directories);
    }

    public static IEnumerable<string> EnumerateFileSystemEntries(string absoluteDir)
    {
        MechanicaSaveFix.Log.LogDebug($"{nameof(EnumerateFileSystemEntries)}: {absoluteDir}");
        return VirtualFS.QueryEntries(absoluteDir, "*", recursive: false, EntryKind.Both);
    }
}