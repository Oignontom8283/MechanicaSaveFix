using System;
using System.IO;
using System.Collections.Generic;

public enum Mode { Idle, Capturing, Playback,  }

public static class VirtualFS
{
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


    // Utilitary methods for managing the virtual file system

    /// <summary>
    /// Ensures that the current mode matches the expected mode for a given operation.
    /// </summary>
    /// <param name="expected">The expected mode.</param>
    /// <param name="caller">Name of the calling method.</param>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// <exception cref="InvalidOperationException"></exception>
    private static void RequireEmpty(string caller)
    {
        if (_files.Count != 0)
        {
            throw new InvalidOperationException($"VirtualFS.{caller}: Expected empty virtual file system, but found {_files.Count} files. (Use Clear() to reset.)");
        }
    }

    /// <summary>
    /// Ensures that the virtual file system is not empty before performing certain operations.
    /// </summary>
    /// <param name="caller">Name of the calling method.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void RequireNotEmpty(string caller)
    {
        if (_files.Count == 0)
        {
            throw new InvalidOperationException($"VirtualFS.{caller}: Expected non-empty virtual file system, but found 0 files. (A previous capture might not have been properly completed or cleared.)");
        }
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