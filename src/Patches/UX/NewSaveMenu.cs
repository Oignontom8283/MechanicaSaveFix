using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Saving;
using Game.UI;
using Game.Utilities;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

[HarmonyPatch(typeof(NewGameMenu), "CreateClicked")]
public static class Patch_NewGameMenu_CreateClicked
{
    private static readonly AccessTools.FieldRef<NewGameMenu, bool> IsOpenRef =
        AccessTools.FieldRefAccess<NewGameMenu, bool>("isOpen");

    private static readonly AccessTools.FieldRef<NewGameMenu, bool> EntryClickedRef =
        AccessTools.FieldRefAccess<NewGameMenu, bool>("entryClicked");

    private static readonly AccessTools.FieldRef<NewGameMenu, InputField> SaveNameInputFieldRef =
        AccessTools.FieldRefAccess<NewGameMenu, InputField>("saveNameInputField");

    private static readonly AccessTools.FieldRef<NewGameMenu, DifficultySettingsScreen> DifficultyManagerRef =
        AccessTools.FieldRefAccess<NewGameMenu, DifficultySettingsScreen>("difficultyManager");

    private static readonly MethodInfo FadeOutMusicMethod =
        AccessTools.Method(typeof(NewGameMenu), "FadeOutMusic");

    static bool Prefix(NewGameMenu __instance)
    {
        // Checks
        if (!IsOpenRef(__instance)) return false;
        if (EntryClickedRef(__instance)) return false;

        var saveManager = Utils.GameContext.saveManager;
        var saveFolderPath = Utils.GameContext.savesFolderPath;

        // Multiplayer check
        if (!saveManager.connectedToMaster && PhotonNetwork.IsConnected)
        {
            MechanicaSaveFix.Log.LogWarning("Not connected to master!");
            return false;
        }

        EntryClickedRef(__instance) = true;
        __instance.StartCoroutine((IEnumerator)FadeOutMusicMethod.Invoke(__instance, null));

        string saveName = SaveNameInputFieldRef(__instance).text;
        if (string.IsNullOrEmpty(saveName)) saveName = MechanicaSaveFix.defaultSaveName.Value;

        // Get names of existing saves containers
        string[] existingSaves = Directory.GetDirectories(saveFolderPath).Concat(
            Directory.GetFiles(saveFolderPath, $"*{MechanicaSaveFix.archiveExtension.Value}").Select(Path.GetFileNameWithoutExtension).ToArray()
        ).ToArray();

        string archiveName = Utils.MakeUniqueName(Utils.SanitizeName(saveName), existingSaves);

        // ! Original code for creating a new GameSave object :
        // GameSave gameSave = new GameSave(saveName, 0, 0, DateTime.Now.ToString("G"), DateTime.Now.ToString("G"),
        //     955, 0f, 0, true, UnityEngine.Random.Range(0, 10), 0, false, saveManager.GetVersion());

        // Create a new GameSave object :
        GameSave gameSave = new GameSave(
            _saveName: saveName,                                  // The name of the save.
            _gameMode: 0,                                         // No clue what this is, but it's 0 in the original code.
            _mapIndex: 0,                                         // Map for this save, 0 is Desert (in all likelihood).
            _dateOfCreation: DateTime.Now.ToString("G"),          // Very bad practice, but mandatory otherwise it breaks save file loading
            _lastPlayedDate: DateTime.Now.ToString("G"),          // Same
            _timeOfDay: 955,                                      // Time of the current day.
            _timeSinceGameCreated: 0f,                            // Time since the save was created.
            _elapsedDays: 0,                                      // Days survived in this save.
            _postUpdate: true,                                    // Not sure, likely caused by one of the game updates.
            _spawnLocationIndex: UnityEngine.Random.Range(0, 10), // Random spawn location.
            _daysSurvived: 0,                                     // Days survived in this save. Duplicate of _elapsedDays no? i don't know.
            _crystalsRandomized: false,                           // No clue, likely a dead/scrapped feature or something I don't know about.
            _creationVersion: saveManager.GetVersion()            // The version of the game when this save was created.
        );
        
        // Get the current difficulty settings for the new save.
        GameDifficultySave difficultySave = DifficultyManagerRef(__instance).RetrieveCurrentSettings();

        string archivePath = Path.Combine(saveFolderPath, archiveName + MechanicaSaveFix.archiveExtension.Value);
        string archivePathFolder = Path.Combine(saveFolderPath, archiveName);

        try
        {
            // Create the save archive with the default files.
            Utils.CreateArchiveWithDefaults(archivePath, new Dictionary<string, string> {
                [MechanicaSaveFix.saveFileNameSaveinfo.Value] = Utils.ToJsonOrThrow(gameSave),
                [MechanicaSaveFix.saveFileNameGameSettings.Value] = Utils.ToJsonOrThrow(difficultySave),
            });
        }
        catch
        {
            MechanicaSaveFix.Log.LogWarning("Error creating new save archive.");
            EntryClickedRef(__instance) = false;
            return false;
        }

        Utils.GameContext.loadingScreen.Show();
        Utils.GameContext.lobby.ShowAllLoadingScreens();

        Utils.GameContext.saveManager.LoadSave(
            archivePathFolder,                        // The supposed save folder path.
            false,                                    // Show loading screen, false, why? I don't know, it's the original code.
            Utils.GameContext.lobby.pCurrent_lobbyID, // The current lobby ID (for multiplayer).
            false                                     // Loade in DEV TEST mode (Removed by the mod).
        );

        return false;
    }
}