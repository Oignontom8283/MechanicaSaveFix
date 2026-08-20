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
