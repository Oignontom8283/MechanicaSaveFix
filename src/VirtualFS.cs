using System;
using System.IO;
using System.Collections.Generic;

public enum Mode { Idle, Capturing, Playback }

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

    /// <summary>
    /// Initializes or resets the virtual file system.
    /// </summary>
    /// <param name="absoluteRoot"></param>
    private static void Reset(string absoluteRoot)
    {
        _files.Clear();
        _root = Path.GetFullPath(absoluteRoot);
    }

    /// <summary>
    /// Begins capturing save files.
    /// </summary>
    /// <param name="absoluteRoot"></param>
    public static void BeginSaveCapture(string absoluteRoot)
    {
        Reset(absoluteRoot);
        _mode = Mode.Capturing;
    }
}