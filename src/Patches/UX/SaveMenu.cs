using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Saving;
using Game.Settings;
using Game.UI;
using Game.Utilities;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

[HarmonyPatch(typeof(LoadGameMenu), "Start")]
public static class Patch_LoadGameMenu_Start
{
    // Provides access to the protected Unity UI fields and game methods that this patch needs to manipulate.
    #region Reflection

    private static readonly AccessTools.FieldRef<LoadGameMenu, RectTransform> MenuContentRef =
        AccessTools.FieldRefAccess<LoadGameMenu, RectTransform>("menuContent");

    private static readonly AccessTools.FieldRef<LoadGameMenu, GameObject> SaveEntryPrefabRef =
        AccessTools.FieldRefAccess<LoadGameMenu, GameObject>("saveEntryPrefab");

    private static readonly AccessTools.FieldRef<LoadGameMenu, Transform> EntryContainerRef =
        AccessTools.FieldRefAccess<LoadGameMenu, Transform>("entryContainer");

    private static readonly AccessTools.FieldRef<LoadGameMenu, List<RectTransform>> EntryRectsRef =
        AccessTools.FieldRefAccess<LoadGameMenu, List<RectTransform>>("entryRects");

    private static readonly AccessTools.FieldRef<LoadGameMenu, Behaviour> NoSavesRef =
        AccessTools.FieldRefAccess<LoadGameMenu, Behaviour>("noSaves");

    // private static readonly AccessTools.FieldRef<LoadGameMenu, bool> ChangingSettingsRef =
    //     AccessTools.FieldRefAccess<LoadGameMenu, bool>("changingSettings");

    // private static readonly AccessTools.FieldRef<LoadGameMenu, GameSave> SaveToChangeRef =
    //     AccessTools.FieldRefAccess<LoadGameMenu, GameSave>("saveToChange");

    private static readonly AccessTools.FieldRef<LoadGameMenu, RectTransform> DeleteConfirmDisplayRef =
        AccessTools.FieldRefAccess<LoadGameMenu, RectTransform>("deleteConfirmDisplay");

    // private static readonly AccessTools.FieldRef<LoadGameMenu, RectTransform> SettingsContainerRef =
    //     AccessTools.FieldRefAccess<LoadGameMenu, RectTransform>("settingsContainer");

    private static readonly AccessTools.FieldRef<LoadGameMenu, DifficultySettingsScreen> DifficultySettingsScreenRef =
        AccessTools.FieldRefAccess<LoadGameMenu, DifficultySettingsScreen>("difficultySettingsScreen");

    private static readonly AccessTools.FieldRef<SettingsManager, Action> OnResolutionChangeRef =
        AccessTools.FieldRefAccess<SettingsManager, Action>("onResolutionChange");

    private static readonly MethodInfo RemoveSaveEntriesMethod =
        AccessTools.Method(typeof(LoadGameMenu), "RemoveSaveEntries");

    private static readonly MethodInfo ResizeContainerMethod =
        AccessTools.Method(typeof(LoadGameMenu), "ResizeContainer");

    private static readonly MethodInfo UpdateCanvasScaleMethod =
        AccessTools.Method(typeof(LoadGameMenu), "UpdateCanvasScale");

    private static readonly MethodInfo FadeOutMusicMethod =
        AccessTools.Method(typeof(LoadGameMenu), "FadeOutMusic");

    #endregion


    // Holds the shared save-menu UI references that are reused for every generated entry.
    #region Menu-wide context

    // Holds the UI references shared by every save entry (prefab, container, list).
    private struct MenuContext
    {
        public readonly GameObject Prefab;
        public readonly Transform Container;
        public readonly List<RectTransform> EntryRects;

        public MenuContext(GameObject prefab, Transform container, List<RectTransform> entryRects)
        {
            Prefab = prefab;
            Container = container;
            EntryRects = entryRects;
        }
    }

    // Reads the shared UI references once.
    private static MenuContext GetMenuContext(LoadGameMenu instance) => new MenuContext(
        prefab: SaveEntryPrefabRef(instance),
        container: EntryContainerRef(instance),
        entryRects: EntryRectsRef(instance));

    #endregion


    // Runs when the save menu starts, rebuilds the list, and keeps the menu aligned with the current screen scale.
    #region Entry point (Start)

    /// <summary>
    /// Replaces the original menu startup logic to rebuild the save list and hook the menu to the resolution-change callback.
    /// </summary>
    /// <param name="__instance">The LoadGameMenu instance being initialized.</param>
    /// <returns>Always false so the original Start method is skipped.</returns>
    static bool Prefix(LoadGameMenu __instance)
    {
        // Link the confirmation popup to the game's existing popup UI.
        ConfirmationPopup.Bind(DeleteConfirmDisplayRef(__instance));

        // Move the menu off-screen before it's filled (matches original Start).
        RepositionMenuContent(__instance);

        // Read saves from disk and build one entry per save.
        RebuildSaveEntries(__instance);

        // Move the menu back into place now that its size is known (matches original Start).
        RepositionMenuContent(__instance);

        // Rescale the menu whenever the screen resolution changes.
        SubscribeToResolutionChange(__instance);

        return false; // skip the original Start
    }

    /// <summary>
    /// Moves the menu content offscreen or back into view based on the current content height and canvas scale.
    /// </summary>
    /// <param name="instance">The load menu instance whose content should be repositioned.</param>
    private static void RepositionMenuContent(LoadGameMenu instance)
    {
        RectTransform menuContent = MenuContentRef(instance);
        float canvasScale = Singleton<SettingsManager>.Instance.canvasScale;
        menuContent.anchoredPosition = new Vector2(0f, -Screen.height / 2f / canvasScale - menuContent.rect.height / 2f);
    }

    /// <summary>
    /// Subscribes the menu's canvas-scaling refresh to the settings manager's resolution-change callback.
    /// </summary>
    /// <param name="instance">The load menu instance whose scaling should be refreshed.</param>
    private static void SubscribeToResolutionChange(LoadGameMenu instance)
    {
        SettingsManager settings = Singleton<SettingsManager>.Instance;
        var updateCanvasScale = (Action)Delegate.CreateDelegate(typeof(Action), instance, UpdateCanvasScaleMethod);
        OnResolutionChangeRef(settings) = (Action)Delegate.Combine(OnResolutionChangeRef(settings), updateCanvasScale);
    }

    #endregion


    // Discovers every save on disk, including both legacy folders and archived .msa files.
    #region Save discovery

    /// <summary>
    /// Reads all valid save folders and .msa archives from disk and returns their paths, metadata, and thumbnails.
    /// </summary>
    /// <returns>An array containing each save path and its parsed GameSave data together with its thumbnail bytes.</returns>
    private static (string SavePath, GameSave GameSave, byte[] ThumbnailBytes)[] GetSavesInfo()
    {
        string savesPath = Singleton<SaveManager>.Instance.pGameSavesFolderPath;

        if (!Directory.Exists(savesPath))
        {
            throw new DirectoryNotFoundException($"Saves folder not found at \"{savesPath}\".");
        }

        var savesInfo = new List<(string, GameSave, byte[])>();

        // Legacy saves: read saveinfo and thumbnail straight from the folder.
        foreach (string saveFolderPath in Directory.GetDirectories(savesPath))
        {
            string saveInfoPath = Path.Combine(saveFolderPath, "saveinfo.txt");
            string thumbnailPath = Path.Combine(saveFolderPath, "thumbnail.jpg");

            if (!File.Exists(saveInfoPath) || !File.Exists(thumbnailPath))
            {
                MechanicaSaveFix.Log.LogWarning($"Save info or thumbnail not found at \"{saveInfoPath}\". Skipping.");
                continue;
            }

            string saveInfoText = File.ReadAllText(saveInfoPath);
            byte[] thumbnailBytes = File.ReadAllBytes(thumbnailPath);
            GameSave gameSave = Utils.FromJsonOrThrow<GameSave>(saveInfoText);

            savesInfo.Add((saveFolderPath, gameSave, thumbnailBytes));
        }

        // Archived saves: read saveinfo and thumbnail from inside the .msa zip.
        foreach (string saveArchivePath in Directory.GetFiles(savesPath, "*.msa"))
        {
            string saveInfoText = Utils.ReadSingleTextFileFromZip(saveArchivePath, "saveinfo.txt");
            byte[] thumbnailBytes = Utils.ReadSingleByteFileFromZip(saveArchivePath, "thumbnail.jpg");
            GameSave gameSave = Utils.FromJsonOrThrow<GameSave>(saveInfoText);

            savesInfo.Add((saveArchivePath, gameSave, thumbnailBytes));
        }

        return savesInfo.ToArray();
    }

    #endregion


    // Rebuilds the visible save list from disk and keeps the menu layout in sync with the new entries.
    #region Entry list

    /// <summary>
    /// Clears the current save entries and repopulates the menu from the disk-backed save list.
    /// </summary>
    /// <param name="instance">The load menu whose entries should be rebuilt.</param>
    private static void RebuildSaveEntries(LoadGameMenu instance)
    {
        // Get the list of legacy and archive saves.
        var savesInfo = GetSavesInfo();

        // Clear the existing entries, if any, before adding the new ones.
        RemoveSaveEntriesMethod.Invoke(instance, null);

        // Get the UI references shared by every entry.
        MenuContext menuContext = GetMenuContext(instance);

        // Add each save entry to the menu.
        foreach (var (savePath, gameSave, thumbnailBytes) in savesInfo)
        {
            AddSaveEntry(instance, menuContext, savePath, gameSave, thumbnailBytes);
        }

        // Show the "no saves" message if the list is empty.
        Behaviour noSaves = NoSavesRef(instance);
        if (noSaves != null)
        {
            noSaves.enabled = savesInfo.Length == 0;
        }

        // Resize the container and refresh the canvas scale to fit the new entries.
        ResizeContainerMethod.Invoke(instance, null);
        UpdateCanvasScaleMethod.Invoke(instance, null);
    }

    /// <summary>
    /// Instantiates one save row, fills in its metadata, applies the thumbnail, and wires the action buttons.
    /// </summary>
    /// <param name="instance">The owning load menu.</param>
    /// <param name="menuContext">The shared menu UI references used by every save row.</param>
    /// <param name="savePath">The file or folder path for the save being represented.</param>
    /// <param name="gameSave">The parsed save metadata to display.</param>
    /// <param name="thumbnailBytes">The thumbnail image bytes to render on the row.</param>
    private static void AddSaveEntry(
        LoadGameMenu instance,
        MenuContext menuContext,
        string savePath,
        GameSave gameSave,
        byte[] thumbnailBytes)
    {
        if (gameSave == null) return;

        // Spawn the entry prefab.
        RectTransform newRect = UnityEngine.Object.Instantiate(menuContext.Prefab, menuContext.Container)
            .GetComponent<RectTransform>();

        // Fill in the name and day count.
        newRect.Find("SaveName").GetComponent<Text>().text = gameSave.saveName;
        newRect.Find("Day").GetComponent<Text>().text = "Day " + (gameSave.elapsedDays + 1);

        // Show the thumbnail, if we have one.
        ApplyThumbnail(newRect, thumbnailBytes);

        // Each button gets exactly what it needs (save path, GameSave) directly.
        newRect.gameObject.GetComponent<Button>().onClick.AddListener(() => OnEntryClicked(instance, savePath));
        newRect.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() => OnEntryDeleteClicked(instance, savePath, gameSave));
        newRect.Find("SettingsButton").GetComponent<Button>().onClick.AddListener(() => OnSettingsButtonClicked(instance, savePath, gameSave));

        // Track the new entry like the original does.
        menuContext.EntryRects.Add(newRect);
    }

    /// <summary>
    /// Converts the save thumbnail bytes into a Unity Sprite and assigns it to the row's image component.
    /// </summary>
    /// <param name="entryRect">The save row whose thumbnail should be set.</param>
    /// <param name="thumbnailBytes">The raw image bytes loaded from disk or from an archive.</param>
    private static void ApplyThumbnail(RectTransform entryRect, byte[] thumbnailBytes)
    {
        if (thumbnailBytes == null) return;

        var texture = new Texture2D(2, 2);
        texture.LoadImage(thumbnailBytes);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero, 100f);
        entryRect.Find("Thumbnail").GetComponent<Image>().sprite = sprite;
        entryRect.Find("Thumbnail/ThumbnailMissing").gameObject.SetActive(false);
    }

    #endregion


    // Handles the actions triggered by a save row, including loading, deleting, and editing settings.
    #region Entry callbacks

    /// <summary>
    /// Loads the selected save from either a folder or an archived .msa file while guarding against invalid states.
    /// </summary>
    /// <param name="instance">The menu instance that owns the selected entry.</param>
    /// <param name="savePath">The save path to load.</param>
    private static void OnEntryClicked(LoadGameMenu instance, string savePath)
    {
        if (ConfirmationPopup.IsOpen) return;

        var saveManager = Singleton<SaveManager>.Instance;
        
        // Don't allow loading a save while the game is already loading one.
        if (saveManager.isLoading) return;

        // Don't allow loading a save if we're not connected to the master server (for multiplayer).
        if (!saveManager.connectedToMaster && PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Not connected to master!");
            return;
        }

        // Fade the menu music out and show the loading screens.
        instance.StartCoroutine((IEnumerator)FadeOutMusicMethod.Invoke(instance, null));
        Singleton<LoadingScreen>.Instance.Show();
        Singleton<Lobby>.Instance.ShowAllLoadingScreens();

        saveManager.LoadSave(savePath, true, Singleton<Lobby>.Instance.pCurrent_lobbyID, false);
    }

    /// <summary>
    /// Confirms the save deletion, removes the underlying save data from disk, and refreshes the list afterwards.
    /// </summary>
    /// <param name="instance">The load menu instance whose rows should be refreshed.</param>
    /// <param name="savePath">The save folder or archive to delete.</param>
    /// <param name="gameSave">The metadata for the save being deleted.</param>
    private static void OnEntryDeleteClicked(LoadGameMenu instance, string savePath, GameSave gameSave)
    {
        if (ConfirmationPopup.IsOpen) return;

        ConfirmationPopup.Show(
            title: "Are you sure?",
            message: $"Delete '{gameSave.saveName}'",
            onConfirm: () =>
            {
                // If the save is a folder, delete it recursively. If it's an archive, delete just the file.
                if (Utils.GetPathType(savePath) == Utils.PathType.Directory)
                {   
                    Directory.Delete(savePath, recursive: true);
                    MechanicaSaveFix.Log.LogMessage($"Deleted save folder at \"{savePath}\".");
                }
                else
                {
                    File.Delete(savePath);
                    MechanicaSaveFix.Log.LogMessage($"Deleted save file at \"{savePath}\".");
                }

                // Rebuild from disk instead of removing just one entry, to keep both paths in sync.
                RebuildSaveEntries(instance);
            }
        );
    }

    /// <summary>
    /// Opens the difficulty editor for the selected save, creates default settings if missing, and persists any edits.
    /// </summary>
    /// <param name="instance">The load menu instance owning the save row.</param>
    /// <param name="savePath">The save folder or archive whose settings should be edited.</param>
    /// <param name="gameSave">The metadata associated with the save.</param>
    private static void OnSettingsButtonClicked(LoadGameMenu instance, string savePath, GameSave gameSave)
    {
        if (SettingsScreenController.IsOpen) return;

        string gameSettings = "gamesettings.txt";

        bool isArchive = Utils.GetPathType(savePath) == Utils.PathType.File;
        string settingsPath = isArchive ? savePath : Path.Combine(savePath, gameSettings);

        // Read the raw json from wherever it lives: inside the .msa if it's an archive,
        // or as a loose file next to the save otherwise.
        string settingsText = isArchive
            ? Utils.ReadSingleTextFileFromZip(settingsPath, gameSettings)
            : (File.Exists(settingsPath) ? File.ReadAllText(settingsPath) : null);

        GameDifficultySave settingsSave = settingsText != null
            ? Utils.FromJsonOrNull<GameDifficultySave>(settingsText)
            : null;

        // Missing, or present but unreadable/corrupted: fall back to the screen's current
        // defaults and persist them so the next open finds a valid file.
        if (settingsSave == null)
        {
            settingsSave = DifficultySettingsScreenRef(instance).RetrieveCurrentSettings();
            string defaultJson = Utils.ToJsonOrThrow(settingsSave);

            if (isArchive)
                Utils.WriteSingleTextFileToZip(settingsPath, gameSettings, defaultJson);
            else
                File.WriteAllText(settingsPath, defaultJson);
        }

        SettingsScreenController.Show(
            settingsSave,
            onSubmit: editedSave =>
            {
                string settingsJson = Utils.ToJsonOrThrow(editedSave);

                if (isArchive)
                    Utils.WriteSingleTextFileToZip(settingsPath, gameSettings, settingsJson);
                else
                    File.WriteAllText(settingsPath, settingsJson);

                MechanicaSaveFix.Log.LogMessage($"{gameSave.saveName}'s difficulty settings updated!");
                MechanicaSaveFix.Log.LogDebug($"Saved difficulty settings for '{gameSave.saveName}' to \"{settingsPath}\".");
            }
        );
    }

    #endregion
}