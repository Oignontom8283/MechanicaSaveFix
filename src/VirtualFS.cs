using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public enum Mode { Idle, Capturing, Playback,  }

public static class VirtualFS
{
    /// <summary>
    /// A dictionary that maps file paths to their corresponding content in the virtual file system.
    /// </summary>
    /// <remarks>
    /// File paths must be <b>relative to the root</b> save directory <b>AND sanitized</b> (slashes instead of backslashes, no leading slash)!
    /// </remarks>
    private static readonly Dictionary<string, string> _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
    public static bool InScore(string absolutePath) =>
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
    /// Writes a file to the virtual file system with the specified content.
    /// </summary>
    /// <param name="path">The path where the file will be written.</param>
    /// <param name="content">The content to write to the file.</param>
    public static void WriteFile(string absolutePath, string content)
    {
        EnsureInitialized(nameof(WriteFile));

        string relativePath = ToRelativeSaveFilePath(absolutePath);
        string sanitizedPath = Utils.SanitizePath(relativePath);

        _files.Add(sanitizedPath, content);

        MechanicaSaveFix.Log.LogDebug($"File {Utils.GetFastHash(content)} written to VFS: {sanitizedPath}");
    }

    /// <summary>
    /// Reads a file from the virtual file system based on its absolute path.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the file to read.</param>
    /// <returns>The content of the file.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the file is not found in the virtual file system.</exception>
    public static string ReadFile(string absolutePath)
    {
        EnsureInitialized(nameof(ReadFile));

        string relativePath = ToRelativeSaveFilePath(absolutePath);
        string sanitizedPath = Utils.SanitizePath(relativePath);

        // Automatically crash if the file doesn't exist in the virtual file system, as this indicates a serious issue with the capture/playback process.
        return _files[sanitizedPath];
    }

    /// <summary>
    /// Gets all files in the virtual file system that are located directly within the specified absolute directory.
    /// </summary>
    /// <param name="absoluteDir">The absolute path of the directory to search.</param>
    /// <returns>An array of absolute paths for the files found.</returns>
    public static string[] GetFilesIn(string absoluteDir)
    {
        string relDir = ToRelativeSaveFilePath(absoluteDir);
        string prefix = relDir.Length == 0 ? "" : relDir + "/";

        return _files.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                        && k.IndexOf('/', prefix.Length) < 0) // pas de '/' après le préfixe = fichier direct
            .Select(ToAbsoluteFake)
            .ToArray();
    }
}