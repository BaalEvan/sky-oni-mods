using Harmony;
using PeterHan.PLib;
using PeterHan.PLib.Lighting;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SkyLib.OniUtils;

namespace DiseasesReimagined
{
    public static class BuildingsPatch
    {
        public static PLightShape uvlight; 

        public static void SemicircleLight(GameObject source, LightingArgs arg)
        {
            SemicircleDownHelper(
                source,
                arg.SourceCell,
                arg.Range,
                arg.Brightness
            );
        }
        
        private static void SemicircleDownHelper(GameObject source, int sourceCell, int range, 
            IDictionary<int, float> brightness)
        {
            var octants = new OctantBuilder(brightness, sourceCell)
            {
                Falloff = 0.5f,
                SmoothLight = true
            };
            octants.AddOctant(range, DiscreteShadowCaster.Octant.E_SE);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SE);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SW);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.W_SW);
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                AddBuildingStrings(UVCleanerConfig.ID, UVCleanerConfig.DISPLAY_NAME, UVCleanerConfig.DESCRIPTION,
                    UVCleanerConfig.EFFECT);
                AddBuildingToBuildMenu("Medical", UVCleanerConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db),"Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                AddBuildingToTech("MedicineIII", UVCleanerConfig.ID);
            }
        }
        
        // Sink germ transfer
        [HarmonyPatch(typeof(HandSanitizer.Work), "OnWorkTick")]
        public static class HandSanitizer_Work_OnWorkTick_Patch
        {
            public static void Prefix(HandSanitizer.Work __instance, float dt)
            {
                GermySinkManager.Instance?.SinkWorkTick(__instance, dt);
            }
        }

        [HarmonyPatch(typeof(HandSanitizer.Work), "OnStartWork")]
        public static class HandSanitizer_Work_OnStartWork_Patch
        {
            public static void Prefix(HandSanitizer.Work __instance)
            {
                GermySinkManager.Instance?.StartGermyWork(__instance);
            }
        }

        [HarmonyPatch(typeof(HandSanitizer.Work), "OnCompleteWork")]
        public static class HandSanitizer_Work_OnCompleteWork_Patch
        {
            public static void Postfix(HandSanitizer.Work __instance, Worker worker)
            {
                GermySinkManager.Instance?.FinishGermyWork(__instance, worker);
            }
        }

        // Shower germ transfer
        [HarmonyPatch(typeof(Shower), "OnWorkTick")]
        public static class Shower_OnWorkTick_Patch
        {
            public static void Prefix(Shower __instance, float dt)
            {
                GermySinkManager.Instance?.ShowerWorkTick(__instance, dt);
            }
        }

        [HarmonyPatch(typeof(Shower), "OnStartWork")]
        public static class Shower_OnStartWork_Patch
        {
            public static void Prefix(Shower __instance)
            {
                GermySinkManager.Instance?.StartGermyWork(__instance);
            }
        }

        [HarmonyPatch(typeof(Shower), "OnAbortWork")]
        public static class Shower_OnAbortWork_Patch
        {
            public static void Postfix(Shower __instance, Worker worker)
            {
                GermySinkManager.Instance?.FinishGermyWork(__instance, worker);
            }
        }

        [HarmonyPatch(typeof(Shower), "OnCompleteWork")]
        public static class Shower_OnCompleteWork_Patch
        {
            public static void Postfix(Shower __instance, Worker worker)
            {
                GermySinkManager.Instance?.FinishGermyWork(__instance, worker);
            }
        }

        // Manage lifecycle of GermySinkManager
        [HarmonyPatch(typeof(Game), "OnPrefabInit")]
        public static class Game_OnPrefabInit_Patch
        {
            public static void Postfix()
            {
                GermySinkManager.CreateInstance();
            }
        }

        [HarmonyPatch(typeof(Game), "DestroyInstances")]
        public static class Game_DestroyInstances_Patch
        {
            public static void Postfix()
            {
                GermySinkManager.DestroyInstance();
            }
        }

        // Prevent OCD hand washing by observing the hand wash cooldown
        [HarmonyPatch]
        public static class WashHandsReactable_InternalCanBegin_Patch
        {
            public static void Postfix(GameObject new_reactor, ref bool __result)
            {
                var cooldown = new_reactor.GetComponent<WashCooldownComponent>();
                if (cooldown != null && !cooldown.CanWash)
                    __result = false;
            }

            public static MethodBase TargetMethod()
            {
                // Find private class by name
                var parentType = typeof(HandSanitizer);
                var childType = parentType.GetNestedType("WashHandsReactable", BindingFlags.
                    NonPublic | BindingFlags.Instance);
                if (childType == null)
                    throw new InvalidOperationException("Could not patch hand wash class!");
                try
                {
                    var targetMethod = childType.GetMethod("InternalCanBegin", BindingFlags.
                        Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (targetMethod == null)
                        throw new InvalidOperationException("Could not patch hand wash method!");
#if DEBUG
                    PUtil.LogDebug("Patched hand wash method: " + targetMethod);
#endif
                    return targetMethod;
                }
                catch (AmbiguousMatchException e)
                {
                    throw new InvalidOperationException("Could not patch hand wash method!", e);
                }
            }
        }
    }
}
