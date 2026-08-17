using System.IO;
using HarmonyLib;


[HarmonyPatch]
public static class FileReadPatch
{
    [HarmonyTargetMethod]
    public static System.Reflection.MethodBase TargetMethod()
    {
        // Target the system method File.ReadAllText manually, specifying the parameter types to avoid ambiguity.
        return AccessTools.Method(typeof(File), nameof(File.ReadAllText), new[] { typeof(string) });
    }

    [HarmonyPrefix]
    public static void Prefix(ref string path)
    {
        MechanicaSaveFix.Log.LogDebug($"[Read] Reading file : {path}");
    }
}