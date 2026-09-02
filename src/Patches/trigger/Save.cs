using System.Collections;
using System.IO;
using Game.Saving;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Save))]
public static class Patch_SaveManager_Save
{
    private static readonly AccessTools.FieldRef<SaveManager, string> SavePathRef =
        AccessTools.FieldRefAccess<SaveManager, string>("savePath");

    private static IEnumerator CaptureWrapper(SaveManager instance, IEnumerator original)
    {
        string savePath = SavePathRef(instance);

        string saveArchivePath = Path.ChangeExtension(savePath, MechanicaSaveFix.archiveExtension.Value);
        string saveBackupPath = Path.Combine(Application.persistentDataPath, MechanicaSaveFix.backupFolderName.Value);

        if (!VirtualFS.IsInitialized())
        {
            throw new System.Exception("VirtualFS is not initialized.");
        }
        
        VirtualFS.StartIntercepting();
        MechanicaSaveFix.Log.LogMessage($"Starting world I/O intercepting.");

        yield return original;

        VirtualFS.EndIntercepting();
        MechanicaSaveFix.Log.LogMessage($"World I/O intercepting finished.");

        MechanicaSaveFix.Log.LogInfo($"Saving world archive to disk...");
        var commitedResult = VirtualFS.CommitArchive(saveArchivePath, saveBackupPath);
        MechanicaSaveFix.Log.LogMessage($"Finished saving {commitedResult.committedFiles} files to archive!");
        MechanicaSaveFix.Log.LogInfo($"Archive saved to \"{(commitedResult.backupCreated ? saveArchivePath : "N/A")}\". Backup saved to \"{(commitedResult.oldBackupDeleted ? saveBackupPath : "N/A")}\".");
    }

    static void Postfix(SaveManager __instance, ref IEnumerator __result)
    {
        __result = CaptureWrapper(__instance, __result);
    }
}