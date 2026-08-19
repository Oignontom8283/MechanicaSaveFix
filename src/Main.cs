using BepInEx;
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
        Log.LogInfo(" ");
        Log.LogInfo($" {MOD_NAME} initialized!");
        Log.LogInfo($"   v{MOD_VERSION} - {MOD_COMMIT_HASH[..12]}");
        Log.LogInfo($"  Built on {MOD_BUILD_DATE}");
        Log.LogInfo(" ");
        
        // Apply Harmony patches
        harmony.PatchAll();
    }
}