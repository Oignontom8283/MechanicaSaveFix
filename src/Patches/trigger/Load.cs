using System.Collections;
using System.IO;
using Game.Saving;
using HarmonyLib;


[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadSave))]
public static class Patch_SaveManager_LoadSave
{
    static void Prefix(string saveFolderPath)
    {

        string saveArchivePath = Path.ChangeExtension(saveFolderPath, ".msa");
        bool isExistArchive = File.Exists(saveArchivePath);

        VirtualFS.Initialize(saveFolderPath);

        if (isExistArchive)
        {
            VirtualFS.LoadZipFromDisk(saveArchivePath);
            MechanicaSaveFix.Log.LogInfo($"Loaded save archive from {saveArchivePath}");
            VirtualFS.BeginLoadPlayback();
        }
        else
        {
            MechanicaSaveFix.Log.LogInfo($"No save archive found at {saveArchivePath}");
        }
    }
}

[HarmonyPatch]
public static class Patch_SaveManager_FinalizeLoad
{
    static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(SaveManager), "FinalizeLoad", new[] { typeof(ulong) });
    }

    private static IEnumerator PlaybackWrapper(IEnumerator original)
    {
        yield return original;

        // If the world is an archive, then we are in playback mode, so we need to end the playback operation.
        if (VirtualFS.IsOperationActive())
        {   
            VirtualFS.EndOperation();
            MechanicaSaveFix.Log.LogInfo("Ended of world load playback operation.");
        }
    }

    static void Postfix(ref IEnumerator __result)
    {
        __result = PlaybackWrapper(__result);
    }
}