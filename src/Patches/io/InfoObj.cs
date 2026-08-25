using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;


// DirectoryInfo.GetFiles()

[HarmonyPatch]
public static class Patch_DirectoryInfo_GetFiles_1
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(DirectoryInfo), nameof(DirectoryInfo.GetFiles), System.Type.EmptyTypes);
    }

    static bool Prefix(DirectoryInfo __instance, ref FileInfo[] __result)
    {
        if (!VirtualFS.InScope(__instance.FullName))
        {
            return true;
        }

        __result = Forward.GetFiles(__instance.FullName).Select(p => new FileInfo(p)).ToArray();
        return false;
    }
}

[HarmonyPatch]
public static class Patch_DirectoryInfo_GetFiles_2
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(DirectoryInfo), nameof(DirectoryInfo.GetFiles), new[] { typeof(string) });
    }

    static bool Prefix(DirectoryInfo __instance, string searchPattern, ref FileInfo[] __result)
    {
        if (!VirtualFS.InScope(__instance.FullName))
        {
            return true;
        }

        __result = Forward.GetFiles(__instance.FullName, searchPattern).Select(p => new FileInfo(p)).ToArray();
        return false;
    }
}


// DirectoryInfo.GetDirectories()

[HarmonyPatch]
public static class Patch_DirectoryInfo_GetDirectories_1
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(DirectoryInfo), nameof(DirectoryInfo.GetDirectories), System.Type.EmptyTypes);
    }

    static bool Prefix(DirectoryInfo __instance, ref DirectoryInfo[] __result)
    {
        if (!VirtualFS.InScope(__instance.FullName))
        {
            return true;
        }

        __result = Forward.GetDirectories(__instance.FullName).Select(p => new DirectoryInfo(p)).ToArray();
        return false;
    }
}

[HarmonyPatch]
public static class Patch_DirectoryInfo_GetDirectories_2
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(DirectoryInfo), nameof(DirectoryInfo.GetDirectories), new[] { typeof(string) });
    }

    static bool Prefix(DirectoryInfo __instance, string searchPattern, ref DirectoryInfo[] __result)
    {
        if (!VirtualFS.InScope(__instance.FullName))
        {
            return true;
        }

        __result = Forward.GetDirectories(__instance.FullName, searchPattern).Select(p => new DirectoryInfo(p)).ToArray();
        return false;
    }
}


// DirectoryInfo.CreateSubdirectory(string)

[HarmonyPatch]
public static class Patch_DirectoryInfo_CreateSubdirectory
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(DirectoryInfo), nameof(DirectoryInfo.CreateSubdirectory), new[] { typeof(string) });
    }

    static bool Prefix(DirectoryInfo __instance, string path, ref DirectoryInfo __result)
    {
        if (!VirtualFS.InScope(__instance.FullName))
        {
            return true;
        }

        string combined = Path.Combine(__instance.FullName, path);
        __result = new DirectoryInfo(combined); // no-op : rien à créer, tout vit en RAM
        return false;
    }
}


// DirectoryInfo.Exist

[HarmonyPatch(typeof(DirectoryInfo), nameof(DirectoryInfo.Exists), MethodType.Getter)]
public static class Patch_DirectoryInfo_Exists
{
    static bool Prefix(DirectoryInfo __instance, ref bool __result)
    {
        if (!VirtualFS.InScope(__instance.FullName))
        {
            return true;
        }

        __result = Forward.DirectoryExists(__instance.FullName);
        return false;
    }
}


// FileInfo.Exists

[HarmonyPatch(typeof(FileInfo), nameof(FileInfo.Exists), MethodType.Getter)]
public static class Patch_FileInfo_Exists
{
    static bool Prefix(FileInfo __instance, ref bool __result)
    {
        if (!VirtualFS.InScope(__instance.FullName))
        {
            return true;
        }

        __result = Forward.FileExists(__instance.FullName);
        return false;
    }
}