using System;
using System.Collections.Generic;

public static class VirtualFS
{
    /// <summary>
    /// A dictionary that maps file paths to their corresponding content in the virtual file system.
    /// </summary>
    private static readonly Dictionary<string, string> _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private enum Mode { Idle, Capturing, Playback }
    /// <summary>
    /// Current mode of the capture/playback system.
    /// - `Idle`: Not capturing or playing back.
    /// - `Capturing`: Currently capturing save files.
    /// - `Playback`: Currently playing back save files.
    /// </summary>
    private static Mode _mode = Mode.Idle;
}