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
    