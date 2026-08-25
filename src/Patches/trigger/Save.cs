using System.Collections;
using System.IO;
using Game.Saving;
using HarmonyLib;

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Save))]
public static class Patch_SaveManager_Save
{
    private static readonly AccessTools.FieldRef<SaveManager, string> SavePathRef =
        AccessTools.FieldRefAccess<SaveManager, string>("savePath");

    private static IEnumerator CaptureWrapper(SaveManager instance, IEnumerator original)
    {
        string savePath = SavePathRef(instance);

        if (!VirtualFS.IsInitialized())
        {
            throw new System.Exception("VirtualFS is not initialized.");
        }
        
        VirtualFS.BeginSaveCapture();
        MechanicaSaveFix.Log.LogInfo($"Starting save capture.");

        yield return original;

        VirtualFS.EndOperation();
        MechanicaSaveFix.Log.LogInfo($"Save capture finished.");

        MechanicaSaveFix.Log.LogInfo($"Committing save archive to disk...");
        VirtualFS.CommitArchive(Path.ChangeExtension(savePath, ".msa"), Path.Combine(savePath, "../../SaveBackups"));
        MechanicaSaveFix.Log.LogInfo($"Save archive committed to disk at {Path.ChangeExtension(savePath, ".msa")}.");
    }

    static void Postfix(SaveManager __instance, ref IEnumerator __result)
    {
        __result = CaptureWrapper(__instance, __result);
    }
}