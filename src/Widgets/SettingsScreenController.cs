using System;
using Game.Saving;
using Game.UI;
using UnityEngine;

/// <summary>
/// Drives the game's existing DifficultySettingsScreen UI directly. Knows nothing about
/// saves or disk paths: it just displays a GameDifficultySave and hands the edited one
/// back through a closure on submit. Same shape as ConfirmationPopup.
/// </summary>
public static class SettingsScreenController
{
    private static GameObject _container;
    private static DifficultySettingsScreen _screen;

    private static Action<GameDifficultySave> _onSubmit;
    private static Action _onCancel;

    public static bool IsOpen { get; private set; }

    /// <summary>
    /// Binds to the game's existing settings container and screen. Call once.
    /// </summary>
    public static void Bind(GameObject settingsContainer, DifficultySettingsScreen difficultyScreen)
    {
        if (_container != null) return;

        _container = settingsContainer;
        _screen = difficultyScreen;

        _container.SetActive(false);
    }

    /// <summary>
    /// Shows the screen pre-filled with the given settings. onSubmit receives the edited
    /// GameDifficultySave when the player confirms. onCancel runs if they back out instead.
    /// </summary>
    public static void Show(GameDifficultySave save, Action<GameDifficultySave> onSubmit, Action onCancel = null)
    {
        if (save.treeRegenRate_NEW == 0f)
        {
            save.treeRegenRate_NEW = 1f;
        }

        _screen.ResetAllValues();
        _screen.LoadSettingsFromSave(save);

        _onSubmit = onSubmit;
        _onCancel = onCancel;

        _container.SetActive(true);
        IsOpen = true;
    }

    /// <summary>
    /// Reads the screen's current values and hands them to onSubmit, then closes.
    /// </summary>
    public static void Submit()
    {
        if (!IsOpen) return;

        GameDifficultySave edited = _screen.RetrieveCurrentSettings();
        Action<GameDifficultySave> onSubmit = _onSubmit;

        Close();
        onSubmit?.Invoke(edited);
    }

    /// <summary>
    /// Closes without calling onSubmit.
    /// </summary>
    public static void Cancel()
    {
        if (!IsOpen) return;

        Action onCancel = _onCancel;
        Close();
        onCancel?.Invoke();
    }

    private static void Close()
    {
        _container.SetActive(false);
        IsOpen = false;
        _onSubmit = null;
        _onCancel = null;
    }
}