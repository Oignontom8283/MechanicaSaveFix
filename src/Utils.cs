using System.IO;
using System.Text;

public static class Utils
{

    /// <summary>
    /// Checks if a target path (file or directory) is located inside a parent directory.
    /// </summary>
    /// <param name="parentPath">The root directory path.</param>
    /// <param name="targetPath">The path (file or folder) to check.</param>
    /// <returns><c>true</c> if <paramref name="targetPath"/> is inside <paramref name="parentPath"/>; otherwise, <c>false</c>.</returns>
    public static bool IsSubPathOf(string parentPath, string targetPath)
    {
        string relativePath = Path.GetRelativePath(parentPath, targetPath);

        return
            !relativePath.StartsWith("..") && // Ensures path doesn't traverse up out of the parent folder
            !Path.IsPathRooted(relativePath); // Handles edge cases on different drives/roots (e.g., C:\ vs D:\)
    }
    
    /// <summary>
    /// Sanitizes a file path by replacing backslashes with forward slashes and removing any leading slashes.
    /// </summary>
    /// <param name="path">The file path to sanitize.</param>
    /// <returns>The sanitized file path.</returns>
    /// <remarks>
    /// <b>Necessary for zip file compatibility!</b>
    /// </remarks>
    public static string SanitizePath(string path) => path.Replace('\\', '/').TrimStart('/');

    /// <summary>
    /// Calculates a fast hash for a given string using the FNV-1a algorithm.
    /// </summary>
    /// <param name="input">The string to hash.</param>
    /// <returns>The calculated hash.</returns>
    public static string GetFastHash(string input)
    {
        if (string.IsNullOrEmpty(input)) return "00000000";

        byte[] bytes = Encoding.UTF8.GetBytes(input);
        uint hash = 2166136261; 

        foreach (byte b in bytes)
        {
            hash ^= b;
            hash *= 16777619;
        }

        return hash.ToString("X8");
    }
}
