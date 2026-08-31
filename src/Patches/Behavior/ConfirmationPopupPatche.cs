// Handles Escape/Delete key shortcuts while the confirmation popup is open.
using Game.UI;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(LoadGameMenu), "Update")]
public static class Patch_LoadGameMenu_Update
{
    static bool Prefix()
    {
        if (!ConfirmationPopup.IsOpen) return true; // popup closed: run the original Update

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ConfirmationPopup.CancelIfOpen();
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            ConfirmationPopup.ConfirmIfOpen();
        }

        return false; // popup open: skip the rest of Update
    }
}
