using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Saving;
using Game.Settings;
using Game.UI;
using Game.Utilities;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

[HarmonyPatch(typeof(LoadGameMenu), "Start")]
public static class Patch_LoadGameMenu_Start
{
    // Provides access to the protected Unity UI fields and game methods that this patch needs to manipulate.
    