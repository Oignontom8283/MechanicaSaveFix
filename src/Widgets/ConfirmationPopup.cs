using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generic Yes/No confirmation popup, bound to the game's existing DeleteConfirmation UI
/// hierarchy. Reusable for any confirmation need (delete, overwrite, etc.), not just saves.
/// </summary>
public static class ConfirmationPopup
{
    private static RectTransform _display;
    private static Text _titleText;
    private static Text _subtitleText;
    private static Button _confirmButton;
    private static Button _cancelButton;

    private static Action _onConfirm;
    private static Action _onCancel;

    public static bool IsOpen { get; private set; }

    /// <summary>
    /// Binds to the existing DeleteConfirmation hierarchy. Call once, e.g. from the
    /// LoadGameMenu Start patch, passing the resolved deleteConfirmDisplay RectTransform.
    /// </summary>
    public static void Bind(RectTransform display)
    {
        _display = display;
        _titleText = display.Find("Background/Title").GetComponent<Text>();
        _subtitleText = display.Find("Background/Subtitle").GetComponent<Text>();
        _confirmButton = display.Find("Background/DestroyButton").GetComponent<Button>();
        _cancelButton = display.Find("Background/CancelButton").GetComponent<Button>();

        _confirmButton.onClick.AddListener(() =>
        {
            var confirm = _onConfirm;
            Close();
            confirm?.Invoke();
        });

        _cancelButton.onClick.AddListener(() =>
        {
            var cancel = _onCancel;
            Close();
            cancel?.Invoke();
        });

        _display.gameObject.SetActive(false); // état initial propre
    }

    /// <summary>
    /// Shows the popup with a given title and message. confirmButtonLabel lets callers
    /// rename "DestroyButton"'s text for non-destructive confirmations (e.g. "Overwrite", "Yes").
    /// </summary>
    public static void Show(string title, string message, Action onConfirm, Action onCancel = null, string confirmButtonLabel = null)
    {
        _titleText.text = title;
        _subtitleText.text = message;

        if (confirmButtonLabel != null)
        {
            _confirmButton.GetComponentInChildren<Text>().text = confirmButtonLabel;
        }

        _onConfirm = onConfirm;
        _onCancel = onCancel;

        _display.gameObject.SetActive(true);
        IsOpen = true;
    }

    public static void Close()
    {
        _display.gameObject.SetActive(false);
        IsOpen = false;
        _onConfirm = null;
        _onCancel = null;
    }

    /// <summary>Called from Update's Escape-key handling if a popup is open.</summary>
    public static void CancelIfOpen()
    {
        if (!IsOpen) return;
        var cancel = _onCancel;
        Close();
        cancel?.Invoke();
    }

    /// <summary>Called from Update's Delete-key handling if a popup is open (shortcut for confirm).</summary>
    public static void ConfirmIfOpen()
    {
        if (!IsOpen) return;
        var confirm = _onConfirm;
        Close();
        confirm?.Invoke();
    }
}