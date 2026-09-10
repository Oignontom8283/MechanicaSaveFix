using System;
using System.Collections;
using System.IO;
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
        if (!IsOpenRef(__instance)) return false;
        if (EntryClickedRef(__instance)) return false;

        var saveManager = Singleton<SaveManager>.Instance;
        if (!saveManager.connectedToMaster && PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Not connected to master!");
            return false;
        }

        EntryClickedRef(__instance) = true;
        __instance.StartCoroutine((IEnumerator)FadeOutMusicMethod.Invoke(__instance, null));

        string saveName = SaveNameInputFieldRef(__instance).text;
        if (string.IsNullOrEmpty(saveName)) saveName = MechanicaSaveFix.defaultSaveName.Value;

        string archiveName = Utils.MakeUniqueName(Utils.SanitizeName(saveName), []); // TODO

        // Create a new GameSave obj (world settings)
        GameSave gameSave = new GameSave(
            _saveName: saveName,
            _gameMode: 0,
            _mapIndex: 0,
            _dateOfCreation: DateTime.Now.ToString("G"),
            _lastPlayedDate: DateTime.Now.ToString("G"),
            _timeOfDay: 955,
            _timeSinceGameCreated: 0f,
            _elapsedDays: 0,
            _postUpdate: true,
            _spawnLocationIndex: UnityEngine.Random.Range(0, 10),
            _daysSurvived: 0,
            _crystalsRandomized: false,
            _creationVersion: saveManager.GetVersion()
        );

        // GameSave gameSave = new GameSave(saveName, 0, 0, DateTime.Now.ToString("G"), DateTime.Now.ToString("G"),
        //     955, 0f, 0, true, UnityEngine.Random.Range(0, 10), 0, false, saveManager.GetVersion());
        
        // Get the current difficulty settings from this world
        GameDifficultySave difficultySave = DifficultyManagerRef(__instance).RetrieveCurrentSettings();

        string archivePath = Path.Combine(Utils.GameContext.savesFolderPath, archiveName + MechanicaSaveFix.archiveExtension.Value);

        // TODO: Work in progress

        // try
        // {
        //     Utils.WriteSingleTextFileToZip(archivePath, MechanicaSaveFix.saveFileNameSaveinfo.Value, Utils.ToJsonOrThrow(gameSave));
        //     Utils.WriteSingleTextFileToZip(archivePath, MechanicaSaveFix.saveFileNameThumbnail.Value == null ? "gamesettings.txt" : "gamesettings.txt", Utils.ToJsonOrThrow(difficultySave));
        // }
        // catch (IOException)
        // {
        //     Debug.LogWarning("Error creating new save archive: System.IO.IOException");
        //     EntryClickedRef(__instance) = false;
        //     return false;
        // }

        // Singleton<LoadingScreen>.Instance.Show();
        // Singleton<Lobby>.Instance.ShowAllLoadingScreens();

        // bool devTest = Input.GetKey(KeyCode.C) && Debug.isDebugBuild;
        // saveManager.LoadSave(archivePath, false, Singleton<Lobby>.Instance.pCurrent_lobbyID, devTest);

        return false;
    }

    private static string MakeSafeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '-');
        }
        return name;
    }

    private static string MakeUniqueSaveName(string savesFolder, string baseName)
    {
        string candidate = baseName;
        int suffix = 1;

        while (Directory.Exists(Path.Combine(savesFolder, candidate)) ||
               File.Exists(Path.Combine(savesFolder, candidate + MechanicaSaveFix.archiveExtension.Value)))
        {
            candidate = $"{baseName}_{suffix}";
            suffix++;
        }

        return candidate;
    }
}