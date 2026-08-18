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

}