using System;
using System.Collections.Generic;

public static class VirtualFS
{
    // A dictionary to store save files in memory.
    private static readonly Dictionary<string, string> _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}