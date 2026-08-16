using BepInEx;
using HarmonyLib;
using UnityEngine;


[BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
public class MechanicaSaveFix : BaseUnityPlugin
{    

    public const string MOD_GUID = "com.mechanica.savefix";
    public const string MOD_NAME = "MechanicaSaveFix";
    public const string MOD_VERSION = "0.1.0";

    void Awake()
    {
        Debug.Log("[MechanicaSaveFix] starting up...");

        var harmony = new Harmony(MOD_GUID);
        harmony.PatchAll();
    }
}