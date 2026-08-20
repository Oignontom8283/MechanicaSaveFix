using System.Collections;
using Game.Saving;
using HarmonyLib;

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Save))]
public static class Patch_SaveManager_Save
{
    
}