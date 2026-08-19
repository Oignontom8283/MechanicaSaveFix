using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Steamworks;
using System.IO.Compression;

public enum Mode { Idle, Capturing, Playback,  }
public enum EntryKind { Files, Directories, Both }


public static class VirtualFS
{
    /// <summary>
    /// A dictionary that maps file paths to their corresponding content in the virtual file system.
    /// </summary>
    /// <remarks>
    /// File paths must be <b>relative to the root</b> save directory <b>AND sanitized</b> (slashes instead of backslashes, no leading slash)!
    /// </remarks>
    private static readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>(StringComparer.Ordinal);

    /// <summary>
    /// Current mode of the capture/playback system.
    /// - `Idle`: Not capturing or playing back.
    /// - `Capturing`: Currently capturing save files.
    /// - `Playback`: Currently playing back save files.
    /// </summary>
    public static Mode _mode = Mode.Idle;

    /// <summary>
    /// The root directory for the world currently saved.
    /// </summary>
    private static string _root;


    /// <summary>
    /// Checks if the virtual file system has been initialized.
    /// </summary>
    /// <returns><c>true</c> if the virtual file system has been initialized; otherwise, <c>false</c>.</returns>
    public static bool IsInitialized() => _root != null;

    /// <summary>
    /// Initializes the virtual file system with the specified root save directory.
    /// </summary>
    /// <param name="RootSaveDirectory">The root save directory.</param>
    /// <exception cref="InvalidOperationException">Thrown when the virtual file system is already initialized.</exception>
    /// <remarks>
    /// This method must be called before any other operations on the virtual file system.
    /// </remarks>
    public static void Initialize(string RootSaveDirectory)
    {
        if (IsInitialized())
        {
            throw new InvalidOperationException("VirtualFS.Initialize: Already initialized.");
        }
        _root = Path.GetFullPath(RootSaveDirectory);
    }

    /// <summary>
    /// Deinitializes the virtual file system, clearing all stored files and resetting the initialization flag.
    /// </summary>
    /// <remarks>
    /// This method should be called when the virtual file system is no longer needed or before re-initializing it.
    /// </remarks>
    public static void Deinitialize()
    {
        EnsureInitialized(nameof(Deinitialize));
        
        if (_mode != Mode.Idle)
        {
            throw new InvalidOperationException($"VirtualFS.{nameof(Deinitialize)}: Cannot deinitialize while in {_mode} mode. End the current operation first!");
        }

        _root = null;
        _files.Clear();

    }

    /// <summary>
    /// Ensures that the virtual file system has been initialized before performing any operations.
    /// </summary>
    /// <param name="caller">Name of the calling method.</param>
    /// <exception cref="InvalidOperationException">Thrown when the virtual file system is not initialized.</exception>
    private static void EnsureInitialized(string caller)
    {
        if (!IsInitialized())
        {
            throw new InvalidOperationException($"VirtualFS.{caller}: Virtual file system is not initialized. Call Initialize() first!");
        }
    }


    // Utilitary methods for managing the virtual file system

    /// <summary>
    /// Ensures that the current mode matches the expected mode for a given operation.
    /// </summary>
    /// <param name="expected">The expected mode.</param>
    /// <param name="caller">Name of the calling method.</param>
    /// <exception cref="InvalidOperationException">Thrown when the current mode does not match the expected mode.</exception>
    private static void RequiredMode(Mode expected, string caller)
    {
        if (_mode != expected)
        {
            throw new InvalidOperationException($"VirtualFS.{caller}: Expected mode {expected}, but current mode is {_mode}.");
        }
    }

    /// <summary>
    /// Ensures that the virtual file system is empty before performing certain operations.
    /// </summary>
    /// <param name="caller">Name of the calling method.</param>
    /// <exception cref="InvalidOperationException">Thrown when the virtual file system is not empty.</exception>
    private static void RequireEmpty(string caller)
    {
        EnsureInitialized(nameof(RequireEmpty));
        if (_files.Count != 0)
        {
            throw new InvalidOperationException($"VirtualFS.{caller}: Expected empty virtual file system, but found {_files.Count} files. (Use Clear() to reset.)");
        }
    }

    /// <summary>
    /// Ensures that the virtual file system is not empty before performing certain operations.
    /// </summary>
    /// <param name="caller">Name of the calling method.</param>
    /// <exception cref="InvalidOperationException">Thrown when the virtual file system is empty.</exception>
    private static void RequireNotEmpty(string caller)
    {
        EnsureInitialized(nameof(RequireNotEmpty));
        if (_files.Count == 0)
        {
            throw new InvalidOperationException($"VirtualFS.{caller}: Expected non-empty virtual file system, but found 0 files. (A previous capture might not have been properly completed or cleared.)");
        }
    }

    /// <summary>
    /// Converts an absolute save file path to a relative path based on the save root directory.
    /// 
    /// </summary>
    /// <param name="absoluteSaveFilePath">The absolute save file path.</param>
    /// <returns>The relative save file path.</returns>
    /// <remarks>
    /// This method don't sanitize the path!
    /// </remarks>
    private static string ToRelativeSaveFilePath(string absoluteSaveFilePath)
    {
        EnsureInitialized(nameof(ToRelativeSaveFilePath));
        return Path.GetRelativePath(_root, absoluteSaveFilePath);
    }

    /// <summary>
    /// Checks if a given absolute path is within the scope of the current virtual file system operation
    /// </summary>
    /// <param name="absolutePath">The absolute path to check.</param>
    /// <returns><c>true</c> if the path is within the scope; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Check whether the intercepted file belongs to the files in the current save.
    /// </remarks>
    public static bool InScope(string absolutePath) =>
        IsInitialized() && // Check if the vfs is initialized
        Utils.IsSubPathOf(_root, Path.GetFullPath(absolutePath)); // Check if the absolute path is a subpath of the root directory (file in the save folder).

    /// <summary>
    /// Converts a relative path to an absolute path based on the save root directory.
    /// </summary>
    /// <param name="relativePath">The relative path.</param>
    /// <returns>The absolute path.</returns>
    private static string ToAbsoluteFake(string relativePath)
    {
        EnsureInitialized(nameof(ToAbsoluteFake));

        return Path.Combine(_root, relativePath);
    }

    // Methods for managing capture and playback operations

    /// <summary>
    /// Begins capturing save files.
    /// </summary>
    /// <remarks>
    /// <b>The VFS must be initialized before calling this method!</b>
    /// <para>To finish the capture, call <see cref="EndOperation"/>.</para>
    /// </remarks>
    public static void BeginSaveCapture()
    {
        EnsureInitialized(nameof(BeginSaveCapture));
        RequiredMode(Mode.Idle, nameof(BeginSaveCapture));
        _mode = Mode.Capturing;
        MechanicaSaveFix.Log.LogInfo($"Save capture started in \"{_root}\"!");
    }

    /// <summary>
    /// Begins playback of captured save files.
    /// </summary>
    /// <remarks>
    /// <b>The VFS must be initialized before calling this method!</b>
    /// <para>To finish the playback, call <see cref="EndOperation"/>.</para>
    /// </remarks>
    public static void BeginLoadPlayback()
    {
        EnsureInitialized(nameof(BeginLoadPlayback));
        RequiredMode(Mode.Idle, nameof(BeginLoadPlayback));
        _mode = Mode.Playback;
        MechanicaSaveFix.Log.LogInfo($"Save playback started in \"{_root}\"!");
    }

    /// <summary>
    /// Checks if there is an active capture or playback operation.
    /// </summary>
    /// <returns><c>true</c> if an operation is active; otherwise, <c>false</c>.</returns>
    public static bool IsOperationActive() => _mode != Mode.Idle;

    /// <summary>
    /// Ends the current capture or playback operation, returning the system to idle mode.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when there is no active operation to end.</exception>
    /// <remarks>
    /// <b>Throws an exception if called when the system is not in capturing or playback mode.</b>
    /// </remarks>
    public static void EndOperation()
    {
        EnsureInitialized(nameof(EndOperation));
        
        if (!IsOperationActive())
        {
            throw new InvalidOperationException($"VirtualFS.{nameof(EndOperation)}: No active operation to end. Current mode is {_mode}.");
        }

        _mode = Mode.Idle;
        MechanicaSaveFix.Log.LogInfo($"Operation (capture/playback) ended. Current mode is now {_mode}.");
    }


    // Methods for managing files in the virtual file system

    /// <summary>
    /// Checks if a file exists in the virtual file system based on its absolute path.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the file to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    public static bool IsExistFile(string absolutePath)
    {
        EnsureInitialized(nameof(IsExistFile));

        string relativePath = ToRelativeSaveFilePath(absolutePath);
        string sanitizedPath = Utils.SanitizePath(relativePath);

        MechanicaSaveFix.Log.LogDebug($"Checking existence of file in VFS: {sanitizedPath}");

        return _files.ContainsKey(sanitizedPath);
    }

    /// <summary>
    /// Deletes a file from the virtual file system based on its absolute path.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the file to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the file is not found in the virtual file system.</exception>
    public static void DeleteFile(string absolutePath)
    {
        EnsureInitialized(nameof(DeleteFile));

        string relativePath = ToRelativeSaveFilePath(absolutePath);
        string sanitizedPath = Utils.SanitizePath(relativePath);

        if (!_files.Remove(sanitizedPath))
        {
            throw new KeyNotFoundException($"VirtualFS.{nameof(DeleteFile)}: File not found in virtual file system: {sanitizedPath}");
        }

        MechanicaSaveFix.Log.LogDebug($"Deleted file from VFS: {sanitizedPath}");
    }

    /// <summary>
    /// Deletes a file from the virtual file system based on its absolute path without throwing an exception if the file does not exist.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the file to delete.</param>
    public static void DeleteFileNoThrow(string absolutePath)
    {
        EnsureInitialized(nameof(DeleteFileNoThrow));

        string relativePath = ToRelativeSaveFilePath(absolutePath);
        string sanitizedPath = Utils.SanitizePath(relativePath);

        if (_files.Remove(sanitizedPath))
        {
            MechanicaSaveFix.Log.LogDebug($"Deleted file from VFS: {sanitizedPath}");
        }
        else
        {
            MechanicaSaveFix.Log.LogWarning($"VirtualFS.{nameof(DeleteFileNoThrow)}: File not found in virtual file system: {sanitizedPath}");
        }
    }

    /// <summary>
    /// Writes a binary file to the virtual file system with the specified content.
    /// </summary>
    /// <param name="absolutePath">The absolute path where the file will be written.</param>
    /// <param name="bytes">The binary content to write to the file.</param>
    /// <exception cref="InvalidOperationException">Thrown when a file (binary or text) with the same path already exists in the virtual file system.</exception>
    public static void WriteBinaryFile(string absolutePath, byte[] bytes)
    {
        EnsureInitialized(nameof(WriteBinaryFile));

        string relativePath = ToRelativeSaveFilePath(absolutePath);
        string sanitizedPath = Utils.SanitizePath(relativePath);

        if (!_files.TryAdd(sanitizedPath, bytes))
        {
            throw new InvalidOperationException($"VirtualFS.{nameof(WriteBinaryFile)}: File already exists in virtual file system: {sanitizedPath}");
        }

        MechanicaSaveFix.Log.LogDebug($"Writing file {Utils.GetFastHash(bytes)} to VFS: {sanitizedPath}");
    }

    /// <summary>
    /// Reads a binary file from the virtual file system based on its absolute path.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the file to read.</param>
    /// <returns>The content of the file.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the file is not found in the virtual file system.</exception>
    public static byte[] ReadBinaryFile(string absolutePath)
    {
        EnsureInitialized(nameof(ReadBinaryFile));

        string relativePath = ToRelativeSaveFilePath(absolutePath);
        string sanitizedPath = Utils.SanitizePath(relativePath);

        if (!_files.TryGetValue(sanitizedPath, out byte[] fileContent))
        {
            throw new KeyNotFoundException($"VirtualFS.{nameof(ReadBinaryFile)}: File not found in virtual file system: {sanitizedPath}");
        }

        MechanicaSaveFix.Log.LogDebug($"Reading file {Utils.GetFastHash(fileContent)} from VFS: {sanitizedPath}");
        return fileContent;
    }

    /// <summary>
    /// Writes a file and converts it to binary format before writing it to the virtual file system with the specified content.
    /// </summary>
    /// <param name="absolutePath">The path where the file will be written.</param>
    /// <param name="textContent">The text content to write to the file.</param>
    /// <exception cref="InvalidOperationException">Thrown when the file already exists in the virtual file system.</exception>
    /// <remarks>
    /// This method converts the text content to a byte array using UTF-8 encoding before writing it to the virtual file system. (Wrapper for <see cref="WriteBinaryFile"/>)
    /// </remarks>
    public static void WriteTextFile(string absolutePath, string textContent)
    {
        WriteBinaryFile(absolutePath, Utils.TextToBytes(textContent));
    }

    /// <summary>
    /// Reads a file and converts it to text from the virtual file system based on its absolute path.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the file to read.</param>
    /// <returns>The content of the file.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the file is not found in the virtual file system.</exception>
    /// <remarks>
    /// This method reads the binary content of the file and converts it to a string using UTF-8 encoding. (Wrapper for <see cref="ReadBinaryFile"/>)
    /// </remarks>
    public static string ReadTextFile(string absolutePath)
    {
        return Utils.BytesToText(ReadBinaryFile(absolutePath));
    }

    /// <summary>
    /// Queries the virtual file system for entries (files and/or directories) based on the specified parameters.
    /// </summary>
    /// <param name="absoluteDir">The absolute path of the directory to search.</param>
    /// <param name="searchPattern">The wildcard pattern to match against file and directory names.</param>
    /// <param name="recursive">Indicates whether to search recursively within subdirectories.</param>
    /// <param name="kind">The type of entries to include in the results.</param>
    /// <returns>An enumerable collection of matching entry paths.</returns>
    public static IEnumerable<string> QueryEntries(string absoluteDir, string searchPattern, bool recursive, EntryKind kind)
    {
        // Normalize target path to the virtual file system's relative format
        string relDir = ToRelativeSaveFilePath(absoluteDir);
        // Ensure trailing slash for prefix matching unless targeting root
        string prefix = relDir.Length == 0 ? "" : relDir + "/";
        // Convert wildcard pattern (*, ?) into an executable Regex
        Regex regex = Utils.WildcardToRegex(searchPattern);

        // Track yielded directory names to prevent duplicate results
        var seenDirs = new HashSet<string>(StringComparer.Ordinal);

        foreach (string key in _files.Keys)
        {
            // Skip entries outside the target directory path
            if (!key.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            // Get relative path remainder after the prefix
            string remainder = key.Substring(prefix.Length);
            int slashIndex = remainder.IndexOf('/');
            bool isDirectChild = slashIndex < 0;

            if (isDirectChild)
            {
                // Process files located directly inside the target directory
                if ((kind == EntryKind.Files || kind == EntryKind.Both) && regex.IsMatch(remainder))
                    yield return ToAbsoluteFake(key);
            }
            else
            {
                // Extract immediate top-level subdirectory name
                string immediateSubdir = remainder.Substring(0, slashIndex);

                // Yield subdirectories once if requested
                if (kind == EntryKind.Directories || kind == EntryKind.Both)
                {
                    if (seenDirs.Add(immediateSubdir))
                        yield return ToAbsoluteFake(prefix + immediateSubdir);
                }

                // Yield nested files in deeper subdirectories when recursive search is enabled
                if (recursive && (kind == EntryKind.Files || kind == EntryKind.Both))
                {
                    string fileName = remainder.Substring(remainder.LastIndexOf('/') + 1);
                    if (regex.IsMatch(fileName))
                        yield return ToAbsoluteFake(key);
                }
            }
        }
    }

    /// <summary>
    /// Writes the contents of the virtual file system to a zip archive on disk at the specified path.
    /// </summary>
    /// <param name="zipPath">The path where the zip archive will be created.</param>
    public static void WriteZipToDisk(string zipPath)
    {
        EnsureInitialized(nameof(WriteZipToDisk));
        RequiredMode(Mode.Idle, nameof(WriteZipToDisk));
        RequireNotEmpty(nameof(WriteZipToDisk));

        using (var zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            foreach (var kvp in _files)
            {
                var entry = archive.CreateEntry(kvp.Key, CompressionLevel.Optimal);
                using (var entryStream = entry.Open())
                {
                    entryStream.Write(kvp.Value, 0, kvp.Value.Length);
                }
            }
        }

        MechanicaSaveFix.Log.LogInfo($"Wrote {_files.Count} files to zip archive at \"{zipPath}\".");
    }

    /// <summary>
    /// Loads the contents of a zip archive from disk into the virtual file system.
    /// </summary>
    /// <param name="zipPath">The path to the zip archive file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the specified zip file is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a duplicate file entry is found in the zip archive.</exception>
    public static void LoadZipFromDisk(string zipPath)
    {
        EnsureInitialized(nameof(LoadZipFromDisk));
        RequiredMode(Mode.Idle, nameof(LoadZipFromDisk));
        RequireEmpty(nameof(LoadZipFromDisk));

        if (!File.Exists(zipPath))
        {
            throw new FileNotFoundException($"VirtualFS.{nameof(LoadZipFromDisk)}: Zip file not found at \"{zipPath}\".");
        }

        using (var archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                {
                    continue;
                }

                string sanitizePath = Utils.SanitizePath(entry.FullName);

                using (var entryStream = entry.Open())
                using (var ms = new MemoryStream())
                {
                    entryStream.CopyTo(ms);

                    if (!_files.TryAdd(sanitizePath, ms.ToArray()))
                    {
                        throw new InvalidOperationException($"VirtualFS.{nameof(LoadZipFromDisk)}: Duplicate file entry in zip archive: {sanitizePath}");
                    }
                }
            }
        }

        MechanicaSaveFix.Log.LogInfo($"Loaded {_files.Count} files from zip archive at \"{zipPath}\".");
    }

    /// <summary>
    /// Save the current virtual file system to a zip archive on disk, creating a backup of the existing archive if it exists.
    /// </summary>
    /// <param name="finalZipPath">The path to the final zip archive file.</param>
    /// <param name="backupFolder">The folder where the backup archive will be stored.</param>
    /// <exception cref="IOException">Thrown when an I/O error occurs while writing the archive.</exception>
    /// <remarks>
    /// Save the world to a archive file!
    /// </remarks>
    public static void CommitArchive(string finalZipPath, string backupFolder)
    {
        EnsureInitialized(nameof(CommitArchive));
        RequiredMode(Mode.Idle, nameof(CommitArchive));
        RequireNotEmpty(nameof(CommitArchive));

        string tempPath = finalZipPath + ".tmp";

        // Delete a potential orphaned temporary file.
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
            MechanicaSaveFix.Log.LogWarning($"Temporary archive file \"{tempPath}\" already existed and was deleted.");
        }

        WriteZipToDisk(tempPath);
        if (!Utils.VerifyFileValid(tempPath)) // Verify that the temporary file was created successfully.
        {
            throw new IOException($"VirtualFS.CommitArchive: Failed to write temporary archive: {tempPath}");
        }

        // Back up the existing save file, if any.
        if (File.Exists(finalZipPath))
        {
            Directory.CreateDirectory(backupFolder);

            string backupPath = Path.Combine(backupFolder, Path.GetFileName(finalZipPath));

            if (File.Exists(backupPath))
            {   
                // Delete the old backup if it already exists.
                File.Delete(backupPath);
                MechanicaSaveFix.Log.LogInfo($"Old backup archive file \"{backupPath}\" already existed and was deleted.");
            }
            else
            {
                // Do nothing if no previous backup exists.
                MechanicaSaveFix.Log.LogInfo($"No previous backup archive file found at \"{backupPath}\".");
            }

            File.Move(finalZipPath, backupPath);
            if (!Utils.VerifyFileValid(backupPath)) // Verify that the backup file was created successfully.
            {
                throw new IOException($"VirtualFS.CommitArchive: Failed to create backup archive: {backupPath}");
            }
        }

        File.Move(tempPath, finalZipPath);
        if (!Utils.VerifyFileValid(finalZipPath)) // Verify that the final file was created successfully.
        {
            throw new IOException($"VirtualFS.CommitArchive: Failed to write final archive: {finalZipPath}");
        }

        MechanicaSaveFix.Log.LogInfo($"Successfully saved the world to \"{finalZipPath}\" archive, with backup in \"{backupFolder}\".");
    }
}