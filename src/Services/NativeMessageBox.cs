using System;
using System.Runtime.InteropServices;

/// <summary>
/// Thin wrapper around the native Windows MessageBox (user32.dll), used instead of
/// System.Windows.Forms to avoid pulling in an extra assembly that may not resolve
/// cleanly on this game's Mono runtime.
/// </summary>
public static class NativeMessageBox
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    private const uint MB_OK = 0x00000000;
    private const uint MB_YESNO = 0x00000004;
    private const uint MB_ICONQUESTION = 0x00000020;
    private const uint MB_ICONINFORMATION = 0x00000040;
    private const uint MB_TOPMOST = 0x00040000;

    private const int IDYES = 6;

    /// <summary>Shows a Yes/No dialog and returns true if the player clicked Yes.</summary>
    public static bool ShowYesNo(string message, string title)
    {
        int result = MessageBoxW(IntPtr.Zero, message, title, MB_YESNO | MB_ICONQUESTION | MB_TOPMOST);
        return result == IDYES;
    }

    /// <summary>Shows a simple informational dialog with a single OK button.</summary>
    public static void ShowInfo(string message, string title)
    {
        MessageBoxW(IntPtr.Zero, message, title, MB_OK | MB_ICONINFORMATION | MB_TOPMOST);
    }
}