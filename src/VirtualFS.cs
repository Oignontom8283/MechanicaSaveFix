using System;
using System.IO;
using System.Collections.Generic;

public enum Mode { Idle, Capturing, Playback,  }

public static class VirtualFS
{
    /// <summary>
    /// Indicates whether the virtual file system has been initialized. This flag is set to true after the first call to Initialize() and prevents re-initialization.
    /// </summary>
    private static bool _isInitialized = false;

    /// <summary>
    /// A dictionary that maps file paths to their corresponding content in the virtual file system.
    /// </summary>
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
    /// Initializes the virtual file system with the specified root save directory.
    /// </summary>
    /// <param name="RootSaveDirectory">The root save directory.</param>
    /// <exception cref="InvalidOperationException">Thrown when the virtual file system is already initialized.</exception>
    /// <remarks>
    /// This method must be called before any other operations on the virtual file system.
    /// </remarks>
    public static void Initialize(string RootSaveDirectory)
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("VirtualFS.Initialize: Already initialized.");
        }
        _isInitialized = true;
        _root = Path.GetFullPath(RootSaveDirectory);
    }

    /// <summary>
    /// Checks if the virtual file system has been initialized.
    /// </summary>
    /// <returns><c>true</c> if the virtual file system has been initialized; otherwise, <c>false</c>.</returns>
    public static bool IsInitialized() => _isInitialized;

    /// <summary>
    /// Deinitializes the virtual file system, clearing all stored files and resetting the initialization flag.
    /// </summary>
    /// <remarks>
    /// This method should be called when the virtual file system is no longer needed or before re-initializing it.
    /// </remarks>
    public static void Deinitialize()
    {
        EnsureInitialized(nameof(Deinitialize));
        
        _root = null;
        _files.Clear();
        _isInitialized = false;
    }

    /// <summary>
    /// Ensures that the virtual file system has been initialized before performing any operations.
    /// </summary>
    /// <param name="caller">Name of the calling method.</param>
    /// <exception cref="InvalidOperationException">Thrown when the virtual file system is not initialized.</exception>
    private static void EnsureInitialized(string caller)
    {
        if (!_isInitialized)
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
    private static void RequiredMod(Mode expected, string caller)
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
    /// Sanitizes a file path by replacing backslashes with forward slashes and removing any leading slashes. Necessary for zip file compatibility!
    /// </summary>
    /// <param name="path">The file path to sanitize.</param>
    /// <returns>The sanitized file path.</returns>
    private static string SanitizePath(String path) => path.Replace('\\', '/').TrimStart('/');

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
    /// Begins capturing save files.
    /// </summary>
    /// <param name="absoluteRoot"></param>
    public static void BeginSaveCapture(string absoluteRoot)
    {

    }

    /// <summary>
    /// Begins playback of previously captured save files.
    /// </summary>
    /// <param name="absoluteRoot"></param>
    public static void BeginLoadPlayback(string absoluteRoot)
    {

    }
}