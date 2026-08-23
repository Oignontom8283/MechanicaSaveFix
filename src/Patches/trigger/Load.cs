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

        string saveArchivePath = Path.ChangeExtension(saveFolderPath, ".msa");
        bool isExistArchive = File.Exists(saveArchivePath);

        // Load the saveInfo from the appropriate source (archive or folder)
        GameSave saveInfo = ((Func<GameSave>)(() =>
        {
            if (!isExistArchive)
            {
                string saveInfoPath = Path.Combine(saveFolderPath, "saveInfo.txt");
                string saveInfoText = File.ReadAllText(saveInfoPath);
                return Utils.FromJsonOrThrow<GameSave>(saveInfoText);
            }
            else
            {
                string saveInfoText = Utils.ReadSingleTextFileFromZip(saveArchivePath, "saveInfo.txt"); // TODO: Ajouter un throw si le fichier n'existe pas dans l'archive ou une fonction
                return Utils.FromJsonOrThrow<GameSave>(saveInfoText);
            }
        }))();

        // Store the loaded saveInfo in the cache for save use
        Cache.SaveInfo.Set(saveInfo);

        

    }
}