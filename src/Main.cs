using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

[BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
public class MechanicaSaveFix : BaseUnityPlugin
{    
    public const string MOD_GUID = "com.oignontom8283.savefix";
    public const string MOD_NAME = nameof(MechanicaSaveFix);
    public const string MOD_VERSION = BuildInfo.Version;
    public const string MOD_COMMIT_HASH = BuildInfo.CommitHash;
    public const string MOD_BUILD_DATE = BuildInfo.BuildDateUtc;

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
        
        // Apply Harmony patches
        harmony.PatchAll();
    }

    public static ConfigEntry<string> archiveExtension;
    public static ConfigEntry<string> backupFolderName;
    public static ConfigEntry<bool> backupEnabled;
    public static ConfigEntry<string> saveFileNameThumbnail;
    public static ConfigEntry<string> saveFileNameSaveinfo;
    public static ConfigEntry<string> saveFileNameGameSettings;
    public static ConfigEntry<Utils.SaveCompressionLevel> saveCompressionLevel;
    public static ConfigEntry<bool> displayArchiveSave;
    public static ConfigEntry<bool> displayLegacySave;

    /// <summary>
    /// Binds configuration settings for the plugin.
    /// </summary>
    private void ConfigBind()
    {
        Config.SaveOnConfigSet = false;

        string configHeader = $"{MOD_NAME} v{MOD_VERSION} Configuration!\n" +
                              "--------------------------------\n" +
                              $"This configuration file allows you to customize the behavior of the {MOD_NAME} plugin.\n" +
                              "You can modify the settings below to suit your preferences.\n" +
                              "\n" +
                              "Changing certain settings could cause the game to malfunction. Only modify what you understand.\n" +
                              "--------------------------------";

        //                                      Section, Key,                    Default value,                      Description
        archiveExtension         = Config.Bind( "Save",  "Extension",            ".msa",                             "The file extension for the save archive format."                  ); // yes
        backupFolderName         = Config.Bind( "Save",  "BackupFolderPath",     "./Backups/",                       "The path of the folder where backups will be stored."             ); // yes
        backupEnabled            = Config.Bind( "Save",  "BackupEnabled",        true,                               "Whether to create backups of save files before overwriting them." ); // yes
        saveFileNameThumbnail    = Config.Bind( "Save",  "ThumbnailFileName",    "thumbnail.jpg",                    "The name of the thumbnail file within each save."                 ); // yes
        saveFileNameSaveinfo     = Config.Bind( "Save",  "SaveinfoFileName",     "saveinfo.txt",                     "The name of the info file within each save."                      ); // yes
        saveFileNameGameSettings = Config.Bind( "Save",  "GameSettingsFileName", "gamesettings.txt",                 "The name of the game settings file within each save."            ); // yes
        saveCompressionLevel     = Config.Bind( "Save",  "CompressionLevel",     Utils.SaveCompressionLevel.Optimal, "The level of compression to use when creating save archives."      ); // yes
        displayArchiveSave       = Config.Bind( "UI",    "DisplayArchiveSave",   true,                               "Whether to display the archive save option in the save menu."     ); // yes
        displayLegacySave        = Config.Bind( "UI",    "DisplayLegacySave",    true,                               "Whether to display the legacy save option in the save menu."      ); // yes

        Config.Save();
        Config.SaveOnConfigSet = true;

        
        // Add a header to the configuration file
        string prefixLine = "# ";

        if (!File.Exists(Config.ConfigFilePath))
            return;

        string fileContent = File.ReadAllText(Config.ConfigFilePath);

        // Add a header to the configuration file
        string headerContent = prefixLine + configHeader.Replace("\n", "\n" + prefixLine);
        string newFileContent = headerContent + "\n\n" + fileContent;

        // Write the modified content back to the configuration file
        File.WriteAllText(Config.ConfigFilePath, newFileContent);
    }
}