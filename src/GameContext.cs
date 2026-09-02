using Game.Saving;
using Game.Utilities;

/// <summary>
/// Provides access to the current game context.
/// </summary>
public static class GameContext
{
    public static string savesFolderPath => Singleton<SaveManager>.Instance.pGameSavesFolderPath;
}