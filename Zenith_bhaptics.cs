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

using Zenith;



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

        [HarmonyPatch(typeof(Zenith.Locomotion.ZenithGlidingProvider), "BeginGliding", new Type[] { })]
        public class StartGliding
        {
            public static void Postfix()
            {
                tactsuitVr.StartGliding();
            }
        }

        [HarmonyPatch(typeof(Zenith.Locomotion.ZenithGlidingProvider), "EndGliding", new Type[] { })]
        public class EndGliding
        {
            public static void Postfix()
            {
                tactsuitVr.StopGliding();
            }
        }

        [HarmonyPatch(typeof(Zenith.ClientHapticsSystem), "PlayHaptics", new Type[] { typeof(RGBSchemes.AbsoluteHaptics.Event) })]
        public class MeleeHit
        {
            public static void Postfix(Zenith.Combat.MeleeWeapon __instance, RGBSchemes.AbsoluteHaptics.Event hapticEvent)
            {
                tactsuitVr.LOG("PlayHaptics: " + hapticEvent.ToString());
                switch (hapticEvent)
                {
                    case RGBSchemes.AbsoluteHaptics.Event.PlayerAttackEnemyLeft:
                        tactsuitVr.SwordRecoil(false);
                        break;

                    case RGBSchemes.AbsoluteHaptics.Event.PlayerAttackEnemyRight:
                        tactsuitVr.SwordRecoil(true);
                        break;

                    case RGBSchemes.AbsoluteHaptics.Event.PlayerParryEnemyLeft:
                        tactsuitVr.SwordRecoil(false);
                        break;

                    case RGBSchemes.AbsoluteHaptics.Event.PlayerParryEnemyRight:
                        tactsuitVr.SwordRecoil(true);
                        break;

                    case RGBSchemes.AbsoluteHaptics.Event.EnemyAttackPlayerLeft:
                        tactsuitVr.LOG("Player attacked left");
                        break;

                    case RGBSchemes.AbsoluteHaptics.Event.EnemyAttackPlayerRight:
                        tactsuitVr.LOG("Player attacked right");
                        break;

                    default:
                        return;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerStateFX), "OnLevelUp", new Type[] { typeof(LevelUpEvent) })]
        public class LevelUp
        {
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("LevelUp");
            }
        }

        [HarmonyPatch(typeof(Zenith.Cooking.Food), "Eat", new Type[] { })]
        public class PlayerEating
        {
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Eating");
            }
        }

        [HarmonyPatch(typeof(Zenith.UI.ZenithVignetteManager), "OnHealthChanged", new Type[] { typeof(int) })]
        public class PlayerHealthChanged
        {
            public static void Postfix(Zenith.UI.ZenithVignetteManager __instance, int val)
            {
                if (__instance.currentHealth <= 0) tactsuitVr.StopHeartBeat();
                //if (val > 0.1 * __instance.maxHealth) tactsuitVr.PlaybackHaptics("Healing");
                if (__instance.currentHealth <= 0.25f * __instance.maxHealth) tactsuitVr.StartHeartBeat();
                else tactsuitVr.StopHeartBeat();
            }
        }

        [HarmonyPatch(typeof(Zenith.Combat.MeleeWeapon), "HitHittable", new Type[] { typeof(Zenith.Combat.CombatHittableCollider) })]
        public class MeleeHitHittable
        {
            public static void Postfix(Zenith.Combat.MeleeWeapon __instance, Zenith.Combat.CombatHittableCollider hittable)
            {
                if (hittable.combatSystem == __instance.playerCombatSystem) return;
                bool isRight = true;
                bool twoHanded = false;
                try
                {
                    isRight = !__instance.grabbable.grabbedByLeft;
                    twoHanded = __instance.grabbable.grabbingTwoHands;
                }
                catch (Exception) { return; }
                tactsuitVr.SwordRecoil(isRight);
            }
        }

        [HarmonyPatch(typeof(CombatSystem), "Attack", new Type[] { typeof(Zenith.Combat.CombatHittableCollider), typeof(float), typeof(string), typeof(CombatHitType), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(string) })]
        public class PlayerAttack
        {
            public static void Postfix(CombatSystem __instance, Zenith.Combat.CombatHittableCollider hittable, Vector3 position, CombatHitType types, string itemInstanceId, string attackingObjectId, CombatAttackData __result)
            {
                tactsuitVr.LOG("Attacked: " + __result.Defender.Id.ToString() + " " + __instance.entityId.Id.ToString());
                if (__result.Defender.Id != __instance.entityId.Id) return;
                if (hittable.combatSystem == __instance) return;
                if (types == CombatHitType.PLAYER_ORIGINATED) return;
                if (types == CombatHitType.NONE) return;
                string feedBack = "Impact";
                if (types == CombatHitType.MELEE) feedBack = "BladeHit";
                if (types == CombatHitType.RANGED) feedBack = "BulletHit";
                //tactsuitVr.LOG("Hit: " + __instance.transform.position.x.ToString() + " " + __instance.transform.position.z.ToString() + " " + position.x.ToString() + " " + position.z.ToString());
                tactsuitVr.PlaybackHaptics(feedBack);
            }
        }
    }
}
