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

        if (VirtualFS.IsInitialized())
        {
            MechanicaSaveFix.Log.LogWarning("VirtualFS was already initialized on world load, force-deinitialized.");
            VirtualFS.Deinitialize();
        }
        VirtualFS.Initialize(saveFolderPath);
        MechanicaSaveFix.Log.LogMessage($"VirtualFS initialized!");
        
        MechanicaSaveFix.Log.LogInfo($"Save root path: {saveFolderPath}");
        MechanicaSaveFix.Log.LogInfo($"Save archive finded? - {isExistArchive} at \"{saveArchivePath}\".");
        
        int loadedFiles = 0;
        if (isExistArchive)
        {
            MechanicaSaveFix.Log.LogInfo($"Loading save archive...");
            loadedFiles = VirtualFS.LoadZipFromDisk(saveArchivePath);
        }
        else
        {
            MechanicaSaveFix.Log.LogInfo($"Loading save folder...");
            loadedFiles = VirtualFS.LoadFolderFromDisk(saveFolderPath);
        }
        MechanicaSaveFix.Log.LogMessage($"Finished loading {loadedFiles} files.");

        // PLayback the world
        VirtualFS.StartIntercepting();
        MechanicaSaveFix.Log.LogInfo($"Start world I/O intercepting.");
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
            VirtualFS.EndIntercepting();
            MechanicaSaveFix.Log.LogMessage("World loading I/O intercepting finished.");
        }
    }

    static void Postfix(ref IEnumerator __result)
    {
        __result = PlaybackWrapper(__result);
    }
}