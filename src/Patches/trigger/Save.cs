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

        VirtualFS.Initialize(savePath);
        VirtualFS.BeginSaveCapture();

        yield return original;

        VirtualFS.EndOperation();
        VirtualFS.CommitArchive(Path.ChangeExtension(savePath, ".msa"), Path.Combine(savePath, "../../SaveBackups"));
        VirtualFS.Deinitialize();
    }

    static void Postfix(SaveManager __instance, ref IEnumerator __result)
    {
        __result = CaptureWrapper(__instance, __result);
    }
}