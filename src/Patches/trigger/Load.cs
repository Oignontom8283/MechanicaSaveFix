using System.Collections;
using System.IO;
using Game.Saving;
using HarmonyLib;


[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadSave))]
public static class Patch_SaveManager_LoadSave
{
    static void Prefix(string savePath)
    {
        bool isArchive =  Utils.GetPathType(savePath) == Utils.PathType.File;
        
        // Check if the save folder or archive exists
        if (isArchive)
        {
            if (!File.Exists(savePath))
                throw new FileNotFoundException($"Save archive file not found at \"{savePath}\".");
        }
        else
        {
            if (!Directory.Exists(savePath))
                throw new DirectoryNotFoundException($"Save folder not found at \"{savePath}\".");
        }

        // Initialize the virtual file system
        if (VirtualFS.IsInitialized())
        {
            MechanicaSaveFix.Log.LogWarning("VirtualFS was already initialized on world load, force-deinitialized.");
            VirtualFS.Deinitialize();
        }
        VirtualFS.Initialize(savePath);
        MechanicaSaveFix.Log.LogMessage($"VirtualFS initialized!");
        
        MechanicaSaveFix.Log.LogInfo($"Save type: {(isArchive ? "Archive" : "Folder")} at \"{savePath}\".");
        
        // Load the save files into the virtual file system
        int loadedFiles = 0;
        if (isArchive)
        {
            MechanicaSaveFix.Log.LogInfo($"Loading save archive...");
            loadedFiles = VirtualFS.LoadZipFromDisk(savePath);
        }
        else
        {
            MechanicaSaveFix.Log.LogInfo($"Loading save folder...");
            loadedFiles = VirtualFS.LoadFolderFromDisk(savePath);
        }
        MechanicaSaveFix.Log.LogMessage($"Finished loading {loadedFiles} files.");

        // PLayback the world
        VirtualFS.StartIntercepting();
        MechanicaSaveFix.Log.LogMessage($"Start world I/O intercepting.");
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