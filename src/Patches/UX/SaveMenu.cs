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
    private readonly struct MenuContext
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
    