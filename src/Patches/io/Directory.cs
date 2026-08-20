using System.IO;
using HarmonyLib;
using System.Collections.Generic;


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
