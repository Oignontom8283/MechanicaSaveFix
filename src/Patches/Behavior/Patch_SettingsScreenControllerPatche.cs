using Game.UI;
using HarmonyLib;

[HarmonyPatch(typeof(LoadGameMenu), "CancelSettingsChange")]
public static class Patch_LoadGameMenu_CancelSettingsChange
{
    static bool Prefix()
    {
        SettingsScreenController.Cancel();
        return false;
    }
}

[HarmonyPatch(typeof(LoadGameMenu), "SubmitSettingsChange")]
public static class Patch_LoadGameMenu_SubmitSettingsChange
{
    static bool Prefix()
    {
        SettingsScreenController.Submit();
        return false;
    }
}
