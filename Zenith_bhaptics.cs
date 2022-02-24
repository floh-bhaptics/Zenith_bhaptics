using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using MyBhapticsTactsuit;



namespace Zenith_bhaptics
{
    [BepInPlugin("org.bepinex.plugins.Zenith_bhaptics", "Zenith bhaptics integration", "1.0.0")]
    public class Plugin : BepInEx.IL2CPP.BasePlugin
    {
#pragma warning disable CS0109 // Remove unnecessary warning
        internal static new ManualLogSource Log;
#pragma warning restore CS0109
        public static TactsuitVR tactsuitVr;
        public static Vector3 playerPosition;
        // I couldn't find a way to read out the max health. So this is a global variable hack that
        // will just store the maximum health ever read.
        public static float maxHealth = 0f;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;
            Log.LogInfo($"Plugin Zenith_bhaptics is loaded!");
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
            // patch all functions
            var harmony = new Harmony("bhaptics.patch.zenith");
            harmony.PatchAll();
        }
    }
}
