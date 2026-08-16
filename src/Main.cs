using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;


[BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
public class MechanicaSaveFix : BaseUnityPlugin
{    

    public const string MOD_GUID = "com.mechanica.savefix";
    public const string MOD_NAME = "MechanicaSaveFix";
    public const string MOD_VERSION = "0.1.0";

    internal static ManualLogSource Log;
    private readonly Harmony harmony = new Harmony(MOD_GUID);

    private void Awake()
    {
        Log = Logger; // Set the logger for this plugin
        Log.LogInfo($"Mod {MOD_GUID} is loaded!");
        
        // Apply Harmony patches
        harmony.PatchAll();
    }
}