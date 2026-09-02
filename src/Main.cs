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

    private void Awake()
    {
        Log = Logger; // Set the logger for this plugin
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
    public static ConfigEntry<bool> displayArchiveSave;
    public static ConfigEntry<bool> displayLegacySave;
    public static ConfigEntry<bool> deleteNewSaveFolderArtifacts;

    /// <summary>
    /// Binds configuration settings for the plugin.
    /// </summary>
    private void ConfigBind()
    {
        Config.SaveOnConfigSet = false;

        //                                          Section,  Key,                          Default value,  Description
        archiveExtension             = Config.Bind( "Save",  "Extension",                   ".msa",         "The file extension for the save archive format."                  );
        backupFolderName             = Config.Bind( "Save",  "BackupFolderPath",            "./Backups/",   "The path of the folder where backups will be stored."             );
        backupEnabled                = Config.Bind( "Save",  "BackupEnabled",                true,          "Whether to create backups of save files before overwriting them." );
        deleteNewSaveFolderArtifacts = Config.Bind( "Save",  "DeleteNewSaveFolderArtifacts", true,          "Whether to delete artifacts from the new save folder."            );
        displayArchiveSave           = Config.Bind( "UI",    "DisplayArchiveSave",           true,          "Whether to display the archive save option in the save menu."     );
        displayLegacySave            = Config.Bind( "UI",    "DisplayLegacySave",            true,          "Whether to display the legacy save option in the save menu."      );

        Config.Save();
        Config.SaveOnConfigSet = true;
    }
}