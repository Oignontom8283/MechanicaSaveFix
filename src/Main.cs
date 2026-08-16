using BepInEx;
using HarmonyLib;
using UnityEngine;


[BepInPlugin("MechanicaSaveFix", "MechanicaSaveFix", "1.0.0")]
public class MechanicaSaveFix : BaseUnityPlugin
{    
    void Awake()
    {
        Debug.Log("[MechanicaSaveFix] starting up...");

        var harmony = new Harmony("MechanicaSaveFix");
        harmony.PatchAll();
    }
}