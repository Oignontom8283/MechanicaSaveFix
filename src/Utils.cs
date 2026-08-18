using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
    /// Calculates a fast hash for a byte array using the FNV-1a algorithm.
    /// </summary>
    /// <param name="bytes">The byte array to hash.</param>
    /// <returns>The calculated hash.</returns>
    public static string GetFastHash(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return "00000000";

        uint hash = 2166136261;

        foreach (byte b in bytes)
        {
            hash ^= b;
            hash *= 16777619;
        }

        return hash.ToString("X8");
    }

    /// <summary>
    /// Calculates a fast hash for a UTF-8 string using the FNV-1a algorithm.
    /// </summary>
    /// <param name="text">The UTF-8 string to hash.</param>
    /// <returns>The calculated hash.</returns>
    /// <remarks>
    /// This method is a wrapper around <see cref="GetFastHash(byte[])"/> that converts the string to a byte array using UTF-8 encoding before hashing.
    /// </remarks>
    public static string GetFastHash(string text) => GetFastHash(Encoding.UTF8.GetBytes(text));

    /// <summary>
    /// Converts a wildcard pattern (using '*' and '?') into a regular expression for matching file paths.
    /// </summary>
    /// <param name="pattern">The wildcard pattern to convert.</param>
    /// <returns>The equivalent regular expression.</returns>
    public static Regex WildcardToRegex(string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) pattern = "*";

        var sb = new StringBuilder();
        sb.Append('^');

        foreach (char c in pattern)
        {
            switch (c)
            {
                case '*':
                    sb.Append(".*");
                    break;
                case '?':
                    sb.Append('.');
                    break;
                default:
                    sb.Append(Regex.Escape(c.ToString()));
                    break;
            }
        }

        sb.Append('$');
        return new Regex(sb.ToString(), RegexOptions.Compiled);
    }

    /// <summary>
    /// Converts a string to a byte array using UTF-8 encoding.
    /// </summary>
    /// <param name="text">The UTF-8 string to convert.</param>
    /// <returns>The resulting byte array.</returns>
    public static byte[] TextToBytes(string text) => Encoding.UTF8.GetBytes(text);

    /// <summary>
    /// Converts a byte array to a string using UTF-8 encoding.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The resulting UTF-8 string.</returns>
    public static string BytesToText(byte[] bytes) => Encoding.UTF8.GetString(bytes);
}
