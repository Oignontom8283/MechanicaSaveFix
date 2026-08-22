using System;
using System.Collections;
using System.IO;
using Game.Saving;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadSave))]
public static class Patch_SaveManager_LoadSave
{
    static void Prefix(string saveFolderPath)
    {

        GameSave saveInfo = ((Func<GameSave>)(() =>
        {
            if (!File.Exists(Path.ChangeExtension(saveFolderPath, ".msa")))
            {
                string saveInfoPath = Path.Combine(saveFolderPath, "saveInfo.txt");
                string saveInfoText = File.ReadAllText(saveInfoPath);
                return Utils.FromJsonOrThrow<GameSave>(saveInfoText);
            }
            else
            {
                MechanicaSaveFix.Log.LogError("NOT IMPLEMENTED");
                return default(GameSave);
            }
        }))();

        Cache.SaveInfo.Set(saveInfo);

    }
}