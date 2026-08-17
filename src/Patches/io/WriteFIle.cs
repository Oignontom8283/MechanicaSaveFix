using System.IO;
using HarmonyLib;


[HarmonyPatch]
public static class FileSavePatch
{
    [HarmonyTargetMethod]
    public static System.Reflection.MethodBase TargetMethod()
    {
        // Target the system method File.WriteAllText manually, specifying the parameter types to avoid ambiguity.
        return AccessTools.Method(typeof(File), nameof(File.WriteAllText), new[] { typeof(string), typeof(string) });
    }

    [HarmonyPrefix]
    public static void Prefix(ref string path, ref string contents)
    {
        MechanicaSaveFix.Log.LogDebug($"[Write] Writing file : {path}");
    }
}