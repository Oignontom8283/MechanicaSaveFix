using System.IO;
using HarmonyLib;


[HarmonyPatch]
public static class Patch_File_Exists
{
    [HarmonyTargetMethod]
    public static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(File), nameof(File.Exists), new[] { typeof(string) });
    }

    [HarmonyPrefix]
    static bool Prefix(string path, ref bool __result)
    {   

        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        __result = Forward.FileExists(path);
        return false; // Skip the original method
    }
}


[HarmonyPatch]
public static class Patch_File_WriteAllText
{
    public static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(File), nameof(File.WriteAllText), new[] { typeof(string), typeof(string) });
    }

    static bool Prefix(string path, string contents)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        Forward.WriteAllText(path, contents);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_File_ReadAllText
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(File), nameof(File.ReadAllText), new[] { typeof(string) });
    }

    static bool Prefix(string path, ref string __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        __result = Forward.ReadAllText(path);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_File_WriteAllBytes
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(File), nameof(File.WriteAllBytes), new[] { typeof(string), typeof(byte[]) });
    }

    static bool Prefix(string path, byte[] bytes)
    {
        if (!VirtualFS.InScope(path)) return true;
        Forward.WriteAllBytes(path, bytes);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_File_ReadAllBytes
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(File), nameof(File.ReadAllBytes), new[] { typeof(string) });
    }

    static bool Prefix(string path, ref byte[] __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        __result = Forward.ReadAllBytes(path);
        return false;
    }
}
