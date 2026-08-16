using System;
using System.Collections.Generic;

public static class VirtualFS
{
    // A dictionary to store save files in memory.
    private static readonly Dictionary<string, string> _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Current mode of the capture/playback system.
    /// - `Idle`: Not capturing or playing back.
    /// - `Capturing`: Currently capturing save files.
    /// - `Playback`: Currently playing back save files.
    /// </summary>
    private enum Mode { Idle, Capturing, Playback }
    private static Mode _mode = Mode.Idle;
}