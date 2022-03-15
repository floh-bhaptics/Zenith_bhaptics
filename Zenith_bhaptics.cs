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
    [BepInPlugin("org.bepinex.plugins.Zenith_bhaptics", "Zenith bhaptics integration", "1.1.0")]
    public class Plugin : BepInEx.IL2CPP.BasePlugin
    {
#pragma warning disable CS0109 // Remove unnecessary warning
        internal static new ManualLogSource Log;
#pragma warning restore CS0109
        public static TactsuitVR tactsuitVr;

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

        #region World Interaction
        
        [HarmonyPatch(typeof(Zenith.Locomotion.ZenithGlidingProvider), "BeginGliding", new Type[] { })]
        public class StartGliding
        {
            public static void Postfix()
            {
                tactsuitVr.StartGliding();
            }
        }

        [HarmonyPatch(typeof(Zenith.Locomotion.ZenithGlidingProvider), "HandleGliding", new Type[] { })]
        public class HandleGliding
        {
            public static void Postfix(Zenith.Locomotion.ZenithGlidingProvider __instance)
            {
                tactsuitVr.updateGlideSpeed(__instance.velocity.magnitude);
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

        [HarmonyPatch(typeof(BNG.GrappleShot), "shootGrapple", new Type[] {  })]
        public class ShootGrapple
        {
            public static void Postfix(BNG.GrappleShot __instance)
            {
                bool isRight = (__instance.thisGrabber.HandSide == BNG.ControllerHand.Right);
                tactsuitVr.SwordRecoil(isRight);
                //tactsuitVr.LOG("Grapple shot");
            }
        }
        

        #endregion

        #region Health
        
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
        
        
        [HarmonyPatch(typeof(PlayerCharacterHealthSystem), "OnDeath", new Type[] {  })]
        public class PlayerCharacterDeath
        {
            public static void Postfix()
            {
                tactsuitVr.StopThreads();
            }
        }

        [HarmonyPatch(typeof(HealthSystem), "OnDeath", new Type[] { })]
        public class PlayerDeath
        {
            public static void Postfix()
            {
                tactsuitVr.StopThreads();
            }
        }

        [HarmonyPatch(typeof(Zenith.Instancing.InstanceRespawnSystem), "OnPlayerDeath", new Type[] { typeof(ServerPlayerInstance) })]
        public class ServerPlayerDeath
        {
            public static void Postfix()
            {
                tactsuitVr.StopThreads();
            }
        }

        [HarmonyPatch(typeof(Zenith.Instancing.InstanceRespawnSystem), "RespawnPlayer", new Type[] { typeof(ServerPlayerInstance), typeof(Vector3) })]
        public class ServerPlayerRespawn
        {
            public static void Postfix()
            {
                tactsuitVr.StopThreads();
            }
        }
        
        #endregion


        #region Combat
        
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
        
        
        [HarmonyPatch(typeof(SpellGestureActivationManager), "EndGesture", new Type[] { typeof(UnityEngine.XR.XRNode) })]
        public class CastSpell
        {
            public static void Postfix(SpellGestureActivationManager __instance, UnityEngine.XR.XRNode hand)
            {
                if (!__instance.enabled) return;
                bool isRight = (hand == UnityEngine.XR.XRNode.RightHand);
                tactsuitVr.Spell(isRight);
            }
        }

        [HarmonyPatch(typeof(BNG.RaycastWeapon), "Shoot", new Type[] {  })]
        public class ShootRayCast
        {
            public static void Postfix(BNG.RaycastWeapon __instance)
            {
                bool isRight = (__instance.thisGrabber.HandSide == BNG.ControllerHand.Right);
                tactsuitVr.ShootRecoil(isRight);
            }
        }
        
        [HarmonyPatch(typeof(ElementalWeaponProjectileLauncher), "Shoot", new Type[] { })]
        public class ShootProjectile
        {
            public static void Postfix(ElementalWeaponProjectileLauncher __instance)
            {
                bool isRight = true;
                try { isRight = (!__instance.grabbable.grabbedByLeft); }
                catch (Exception) { return; }
                tactsuitVr.ShootRecoil(isRight);
            }
        }

        
        [HarmonyPatch(typeof(BNG.Arrow), "ShootArrow", new Type[] { typeof(Vector3) })]
        public class ShootArrow
        {
            public static void Postfix(BNG.Arrow __instance)
            {
                tactsuitVr.PlaybackHaptics("RecoilArms_L", 0.5f);
                tactsuitVr.PlaybackHaptics("RecoilArms_R", 0.5f);
                //tactsuitVr.LOG("Bow shot");
            }
        }
        
        
        private static KeyValuePair<float, float> getAngleAndShift(Transform player, Vector3 hit)
        {
            // bhaptics pattern starts in the front, then rotates to the left. 0° is front, 90° is left, 270° is right.
            // y is "up", z is "forward" in local coordinates
            Vector3 patternOrigin = new Vector3(0f, 0f, 1f);
            Vector3 hitPosition = hit - player.position;
            Quaternion myPlayerRotation = player.rotation;
            Vector3 playerDir = myPlayerRotation.eulerAngles;
            // get rid of the up/down component to analyze xz-rotation
            Vector3 flattenedHit = new Vector3(hitPosition.x, 0f, hitPosition.z);

            // get angle. .Net < 4.0 does not have a "SignedAngle" function...
            float hitAngle = Vector3.Angle(flattenedHit, patternOrigin);
            // check if cross product points up or down, to make signed angle myself
            Vector3 crossProduct = Vector3.Cross(flattenedHit, patternOrigin);
            if (crossProduct.y > 0f) { hitAngle *= -1f; }
            // relative to player direction
            float myRotation = hitAngle - playerDir.y;
            // switch directions (bhaptics angles are in mathematically negative direction)
            myRotation *= -1f;
            // convert signed angle into [0, 360] rotation
            if (myRotation < 0f) { myRotation = 360f + myRotation; }


            // up/down shift is in y-direction
            // in Shadow Legend, the torso Transform has y=0 at the neck,
            // and the torso ends at roughly -0.5 (that's in meters)
            // so cap the shift to [-0.5, 0]...
            float hitShift = hitPosition.y;
            //tactsuitVr.LOG("HitShift: " + hitShift.ToString());
            float upperBound = 1.7f;
            float lowerBound = 0.8f;
            if (hitShift > upperBound) { hitShift = 0.5f; }
            else if (hitShift < lowerBound) { hitShift = -0.5f; }
            // ...and then spread/shift it to [-0.5, 0.5]
            else { hitShift = (hitShift - lowerBound) / (upperBound - lowerBound) - 0.5f; }

            //tactsuitVr.LOG("Relative x-z-position: " + relativeHitDir.x.ToString() + " "  + relativeHitDir.z.ToString());
            //tactsuitVr.LOG("HitAngle: " + hitAngle.ToString());
            //tactsuitVr.LOG("HitShift: " + hitShift.ToString());

            // No tuple returns available in .NET < 4.0, so this is the easiest quickfix
            return new KeyValuePair<float, float>(myRotation, hitShift);
        }
        
        [HarmonyPatch(typeof(CombatSystem), "Attack", new Type[] { typeof(Zenith.Combat.CombatHittableCollider), typeof(float), typeof(string), typeof(CombatHitType), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(string) })]
        public class PlayerAttack
        {
            public static void Postfix(CombatSystem __instance, Vector3 position, CombatHitType types, CombatAttackData __result)
            {
                tactsuitVr.LOG("Attack: " + __instance.tempIsEnemy.ToString() + " " + __result.Types.ToString());
                if (!__instance.tempIsEnemy) return;
                if (__result.Blocked) { tactsuitVr.PlaybackHaptics("Block"); return; }
                //tactsuitVr.LOG("Hit: " + types.ToString());
                string feedBack = "Impact";
                //if (types == CombatHitType.PLAYER_ORIGINATED) return;
                if (types == CombatHitType.NONE) feedBack = "Impact";
                if (types == CombatHitType.MELEE) feedBack = "BladeHit";
                if (types == CombatHitType.RANGED) feedBack = "BulletHit";
                var angleShift = getAngleAndShift(__instance.transform, position);
                tactsuitVr.PlayBackHit(feedBack, angleShift.Key, angleShift.Value);

                //tactsuitVr.LOG("Hit: " + __instance.transform.position.x.ToString() + " " + __instance.transform.position.z.ToString() + " " + position.x.ToString() + " " + position.z.ToString());
                //tactsuitVr.PlaybackHaptics(feedBack);
            }
        }
        
        #endregion

    }
}
