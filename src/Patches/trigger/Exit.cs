using Game.Saving;
using HarmonyLib;

[HarmonyPatch(typeof(SaveManager), "ExitToMenu")]
public static class Patch_SaveManager_ExitToMenu
{
    static void Prefix()
    {
        if (VirtualFS.IsOperationActive())
        {
            VirtualFS.EndIntercepting();
            MechanicaSaveFix.Log.LogWarning("VFS intercepting was still active on world exit, force-ended.");
        }

        if (VirtualFS.IsInitialized())
        {
            VirtualFS.Deinitialize();
            MechanicaSaveFix.Log.LogInfo("VFS deinitialized on world exit.");
        }
    }
}