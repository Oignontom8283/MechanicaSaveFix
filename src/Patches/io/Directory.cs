using System.IO;
using HarmonyLib;
using System.Collections.Generic;
using Game.Saving;


[HarmonyPatch]
public static class Patch_Directory_Exists
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.Exists), new[] { typeof(string) });
    }
    
    static bool Prefix(string path, ref bool __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.DirectoryExists(path);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_CreateDirectory
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.CreateDirectory), new[] { typeof(string) });
    }

    static bool Prefix(string path, ref DirectoryInfo __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        VirtualFS.CreateDirectory(path);
        __result = new DirectoryInfo(path);
        return false;
    }
}

[HarmonyPatch(typeof(Directory), nameof(Directory.Delete), new[] { typeof(string) })]
public static class Patch_Directory_Delete_1
{
    static bool Prefix(string path)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        Forward.DeleteDirectory(path, recursive: false);
        return false;
    }
}


[HarmonyPatch(typeof(Directory), nameof(Directory.Delete), new[] { typeof(string), typeof(bool) })]
public static class Patch_Directory_Delete_2
{
    static bool Prefix(string path, bool recursive)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        Forward.DeleteDirectory(path, recursive);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_GetFiles_1
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.GetFiles), new[] { typeof(string) });
    }

    static bool Prefix(string path, ref string[] __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        __result = Forward.GetFiles(path);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_GetFiles_2
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.GetFiles), new[] { typeof(string), typeof(string) });
    }

    static bool Prefix(string path, string searchPattern, ref string[] __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        __result = Forward.GetFiles(path, searchPattern);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_GetFiles_3
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.GetFiles), new[] { typeof(string), typeof(string), typeof(SearchOption) });
    }

    static bool Prefix(string path, string searchPattern, SearchOption searchOption, ref string[] __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.GetFiles(path, searchPattern, searchOption);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_GetDirectories_1
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.GetDirectories), new[] { typeof(string) });
    }

    static bool Prefix(string path, ref string[] __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.GetDirectories(path);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_GetDirectories_2
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.GetDirectories), new[] { typeof(string), typeof(string) });
    }

    static bool Prefix(string path, string searchPattern, ref string[] __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.GetDirectories(path, searchPattern);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_GetDirectories_3
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.GetDirectories), new[] { typeof(string), typeof(string), typeof(SearchOption) });
    }

    static bool Prefix(string path, string searchPattern, SearchOption searchOption, ref string[] __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.GetDirectories(path, searchPattern, searchOption);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_GetFileSystemEntries_1
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.GetFileSystemEntries), new[] { typeof(string) });
    }

    static bool Prefix(string path, ref string[] __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.GetFileSystemEntries(path);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_GetFileSystemEntries_2
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.GetFileSystemEntries), new[] { typeof(string), typeof(string) });
    }

    static bool Prefix(string path, string searchPattern, ref string[] __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.GetFileSystemEntries(path, searchPattern);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_EnumerateFiles_1
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.EnumerateFiles), new[] { typeof(string) });
    }

    static bool Prefix(string path, ref IEnumerable<string> __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.EnumerateFiles(path);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_EnumerateDirectories_1
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.EnumerateDirectories), new[] { typeof(string) });
    }

    static bool Prefix(string path, ref IEnumerable<string> __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.EnumerateDirectories(path);
        return false;
    }
}


[HarmonyPatch]
public static class Patch_Directory_EnumerateFileSystemEntries_1
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Directory), nameof(Directory.EnumerateFileSystemEntries), new[] { typeof(string) });
    }

    static bool Prefix(string path, ref IEnumerable<string> __result)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }
        
        __result = Forward.EnumerateFileSystemEntries(path);
        return false;
    }
}


[HarmonyPatch(typeof(SaveManager), "DeleteDirectory", new[] { typeof(string) })]
public static class Patch_SaveManager_DeleteDirectory
{
    static bool Prefix(string path)
    {
        if (!VirtualFS.InScope(path))
        {
            return true;
        }

        Forward.DeleteDirectory(path, recursive: true);
        return false;
    }
}