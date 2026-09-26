using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

[BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
public class MechanicaSaveFix : BaseUnityPlugin
{    
    public const string MOD_GUID = "com.oignontom8283.savefix";
    public const string MOD_NAME = nameof(MechanicaSaveFix);
    public const string MOD_VERSION = BuildInfo.Version;
    public const string MOD_COMMIT_HASH = BuildInfo.CommitHash;
    public const string MOD_BUILD_DATE = BuildInfo.BuildDateUtc;
    public const string GITGUB_REPO_ID = "Oignontom8283/MechanicaSaveFix";

    internal static ManualLogSource Log;
    private readonly Harmony harmony = new Harmony(MOD_GUID);
    private new ConfigFile Config;

    private void Awake()
    {
        Log = Logger; // Set the logger for this plugin
        Config = new ConfigFile(Path.Combine(Paths.ConfigPath, $"{MOD_NAME}.cfg"), true); // Set the configuration file for this plugin
        ConfigBind(); // Bind configuration settings

        Log.LogInfo(" ");
        Log.LogInfo($" {MOD_NAME} initialized!");
        Log.LogInfo($"   v{MOD_VERSION} - {MOD_COMMIT_HASH[..12]}");
        Log.LogInfo($"  Built on {MOD_BUILD_DATE}");
        Log.LogInfo(" ");

        this.StartCoroutine(CheckUpdate()); // Start the update check coroutine
        
        // Apply Harmony patches
        harmony.PatchAll();
    }

    public static string configHeader;

    public static ConfigEntry<bool> checkForUpdatesEnabled;
    public static ConfigEntry<bool> updateCheckPromptShown;
    public static ConfigEntry<string> archiveExtension;
    public static ConfigEntry<string> backupFolderName;
    public static ConfigEntry<bool> backupEnabled;
    public static ConfigEntry<string> saveFileNameThumbnail;
    public static ConfigEntry<string> saveFileNameSaveinfo;
    public static ConfigEntry<string> saveFileNameGameSettings;
    public static ConfigEntry<Utils.SaveCompressionLevel> saveCompressionLevel;
    public static ConfigEntry<string> defaultSaveName;
    public static ConfigEntry<bool> displayArchiveSave;
    public static ConfigEntry<bool> displayLegacySave;

    /// <summary>
    /// Binds configuration settings for the plugin.
    /// </summary>
    private void ConfigBind()
    {
        // Reapply the configuration header whenever a setting is changed
        Config.SettingChanged += (_, _) => ReapplyConfigHeader();


        Config.SaveOnConfigSet = false;

        configHeader = $"{MOD_NAME} v{MOD_VERSION} Configuration!\n" +
                        "--------------------------------\n" +
                        $"This configuration file allows you to customize the behavior of the {MOD_NAME} plugin.\n" +
                        "You can modify the settings below to suit your preferences.\n" +
                        "\n" +
                        "Changing certain settings could cause the game to malfunction. Only modify what you understand!\n" +
                        "--------------------------------";

        //                                      Section,   Key,                    Default value,                      Description
        checkForUpdatesEnabled   = Config.Bind( "Updates", "CheckForUpdates",      false,                              "Whether the mod checks GitHub for a newer version on startup."                         );
        updateCheckPromptShown   = Config.Bind( "Updates", "PromptShown",          false,                              "Internal: whether the player has already been asked about automatic update checks. Do not edit." );
        archiveExtension         = Config.Bind( "Save",    "Extension",            ".msa",                             "The file extension for the save archive format."                  ); // yes
        backupFolderName         = Config.Bind( "Save",    "BackupFolderPath",     "./Backups/",                       "The path of the folder where backups will be stored."             ); // yes
        backupEnabled            = Config.Bind( "Save",    "BackupEnabled",        true,                               "Whether to create backups of save files before overwriting them." ); // yes
        saveFileNameThumbnail    = Config.Bind( "Save",    "ThumbnailFileName",    "thumbnail.jpg",                    "The name of the thumbnail file within each save."                 ); // yes
        saveFileNameSaveinfo     = Config.Bind( "Save",    "SaveinfoFileName",     "saveinfo.txt",                     "The name of the info file within each save."                      ); // yes
        saveFileNameGameSettings = Config.Bind( "Save",    "GameSettingsFileName", "gamesettings.txt",                 "The name of the game settings file within each save."             ); // yes
        saveCompressionLevel     = Config.Bind( "Save",    "CompressionLevel",     Utils.SaveCompressionLevel.Optimal, "The level of compression to use when creating save archives."     ); // yes
        defaultSaveName          = Config.Bind( "Save",    "DefaultSaveName",      "New World",                        "The default name for new saves when creating a new game."         ); // yes
        displayArchiveSave       = Config.Bind( "UI",      "DisplayArchiveSave",   true,                               "Whether to display the archive save option in the save menu."     ); // yes
        displayLegacySave        = Config.Bind( "UI",      "DisplayLegacySave",    true,                               "Whether to display the legacy save option in the save menu."      ); // yes
    
        Config.Save();
        Config.SaveOnConfigSet = true;

        ReapplyConfigHeader();
    }

    private void ReapplyConfigHeader()
    {   
        if (!File.Exists(Config.ConfigFilePath)) return;

        string prefixLine = "# ";

        string fileContent = File.ReadAllText(Config.ConfigFilePath);

        // Add a header to the configuration file content
        string headerContent = prefixLine + configHeader.Replace("\n", "\n" + prefixLine);
        string newFileContent = headerContent + "\n\n" + fileContent;

        // Write the modified content back to the configuration file
        File.WriteAllText(Config.ConfigFilePath, newFileContent);
    }

    private IEnumerator CheckUpdate()
    {
        if (!updateCheckPromptShown.Value)
        {
            bool accepted = NativeMessageBox.ShowYesNo(
                $"Would you like {MechanicaSaveFix.MOD_NAME} to automatically check for new updates on startup?\n\n" +
                "This only checks GitHub for a newer version; nothing is downloaded automatically!\n" +
                "No telemetry. If enabled, a single request is made to GitHub's API per launch to check for the latest release.\n\n" +
                "You can change this setting later in the configuration file.",
                $"{MechanicaSaveFix.MOD_NAME} - Update Check");

            updateCheckPromptShown.Value = true;
            checkForUpdatesEnabled.Value = accepted;
        }

        if (!checkForUpdatesEnabled.Value)
        {
            Log.LogInfo("Update checks are disabled. Skipping update check.");
            yield break; // Exit the coroutine if update checks are disabled
        }


        string githubApiUrl = $"https://api.github.com/repos/{GITGUB_REPO_ID}/releases/latest";

        using (UnityWebRequest request = UnityWebRequest.Get(githubApiUrl))
        {
            request.SetRequestHeader("User-Agent", MOD_NAME);

            yield return request.SendWebRequest();


            if (request.responseCode == 404)
            {
                Log.LogInfo("No releases published yet on GitHub. Skipping update check.");
                yield break;
            }

            if (!string.IsNullOrEmpty(request.error))
            {
                Log.LogWarning($"Failed to check for updates: {request.error}");
                yield break;
            }

            GitHubRelease release = Utils.FromJsonOrNull<GitHubRelease>(request.downloadHandler.text);
            if (release == null || string.IsNullOrEmpty(release.tag_name))
            {
                Log.LogWarning("Failed to parse GitHub release information.");
                yield break;
            }

            if (!Utils.IsNewerVersion(release.tag_name, MOD_VERSION))
            {
                Log.LogInfo($"No updates found. Current version: {MOD_VERSION}, Latest version: {release.tag_name}");
                yield break;
            }

            Log.LogMessage($"A new version is available: {release.tag_name}");

            bool openPage = NativeMessageBox.ShowYesNo(
                $"A new version of {MechanicaSaveFix.MOD_NAME} is available ({release.tag_name})!\n\n" +
                "Do you want to open the version page? The game would close.",
                $"{MechanicaSaveFix.MOD_NAME} - Update Available");

            if (openPage)
            {
                Process.Start(release.html_url);
                Application.Quit();
            }
        }
    }

    [Serializable]
    private class GitHubRelease
    {
        public string tag_name = string.Empty;
        public string html_url = string.Empty;
    }
}
