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

        MechanicaSaveFix.Log.LogInfo($"Save archive at {saveArchivePath} exists: {isExistArchive}");
        if (isExistArchive)
        {
            MechanicaSaveFix.Log.LogInfo($"Loading save archive...");
            VirtualFS.LoadZipFromDisk(saveArchivePath);
        }
        else
        {
            MechanicaSaveFix.Log.LogInfo($"Loading save folder...");
            VirtualFS.LoadFolderFromDisk(saveFolderPath);
        }
        MechanicaSaveFix.Log.LogInfo($"Save load finished.");

        // PLayback the world
        MechanicaSaveFix.Log.LogInfo($"Starting world load playback.");
        VirtualFS.BeginLoadPlayback();
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
            MechanicaSaveFix.Log.LogInfo("World load playback finished.");
        }
    }

    static void Postfix(ref IEnumerator __result)
    {
        __result = PlaybackWrapper(__result);
    }
}