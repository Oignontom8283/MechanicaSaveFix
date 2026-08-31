using Game.UI;
using HarmonyLib;

[HarmonyPatch(typeof(LoadGameMenu), "CancelSettingsChange")]
public static class Patch_LoadGameMenu_CancelSettingsChange
{
    static bool Prefix()
    {
        SettingsScreenController.Cancel();
        return false; // skip the original, which only touched fields we no longer use
    }
}