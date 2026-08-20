using System.Collections;
using Game.Saving;
using HarmonyLib;

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Save))]
public static class Patch_SaveManager_Save
{
    private static readonly AccessTools.FieldRef<SaveManager, string> SavePathRef =
        AccessTools.FieldRefAccess<SaveManager, string>("savePath");
}