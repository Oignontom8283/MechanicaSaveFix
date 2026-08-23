using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Game.Saving;
using HarmonyLib;
using UnityEngine;
using UnityEngine.PlayerLoop;

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

        // // Load the saveInfo from the appropriate source (archive or folder)
        // GameSave saveInfo = ((Func<GameSave>)(() =>
        // {
        //     if (!isExistArchive)
        //     {
        //         string saveInfoPath = Path.Combine(saveFolderPath, "saveInfo.txt");
        //         string saveInfoText = File.ReadAllText(saveInfoPath);
        //         return Utils.FromJsonOrThrow<GameSave>(saveInfoText);
        //     }
        //     else
        //     {
        //         string saveInfoText = Utils.ReadSingleTextFileFromZip(saveArchivePath, "saveInfo.txt"); // TODO: Ajouter un throw si le fichier n'existe pas dans l'archive ou une fonction
        //         return Utils.FromJsonOrThrow<GameSave>(saveInfoText);
        //     }
        // }))();

        // // Store the loaded saveInfo in the cache for save use
        // Cache.SaveInfo.Set(saveInfo);

    }
}

[HarmonyPatch]
public static class patch_SaveManager_FinalizeLoad
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(SaveManager), "FinalizeLoad", new[] { typeof(ulong) });
    }

    private static IEnumerator EndLoadWrapper(IEnumerator original)
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
        __result = EndLoadWrapper(__result);
    }
}