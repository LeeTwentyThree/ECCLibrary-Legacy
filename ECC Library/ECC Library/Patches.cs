using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECCLibrary.Internal;
using HarmonyLib;
using UnityEngine;
using SMLHelper.V2.Utility;
using System.IO;
using ECCLibrary;

namespace ECCLibrary
{
    internal class Patches
    {
        [HarmonyPatch(typeof(LargeWorldStreamer), nameof(LargeWorldStreamer.Initialize))]
        public static class LWSInitPatch
        {
            [HarmonyPostfix()]
            public static void Postfix()
            {
                string directory = SaveUtils.GetCurrentSaveDataDir();
                ECCPatch.staticCreaturesPath = Path.Combine(directory, "StaticCreatures.data");
                if (!File.Exists(ECCPatch.staticCreaturesPath))
                {
                    FileStream stream = File.Create(ECCPatch.staticCreaturesPath, 4096);
                    stream.Close();
                }
                StaticCreatureSpawns.InstantiateAllPrefabSpawners();
            }
        }

#if SN1
        [HarmonyPatch(typeof(TrailManager), nameof(TrailManager.UpdateTrails))]
        public static class TrailManagerUpdateTrailsPatch
        {
            [HarmonyPrefix()]
            public static bool Prefix(TrailManager __instance, float deltaTime)
            {
                if (!ShouldUseNewUpdateMethod(__instance))
                {
                    return true;
                }
                float scale = __instance.GetScale();
                Vector3 vector = __instance.rootSegment.position;
                for (int i = 0; i < __instance.trails.Length; i++)
                {
                    __instance.trails[i].localRotation = Quaternion.identity;
                    __instance.trails[i].localPosition = __instance.trailStartPositions[i];
                    Vector3 position = __instance.trails[i].position;
                    Vector3 value = position - __instance.prevPositions[i];
                    Vector3 b = Vector3.Slerp(__instance.prevPositions[i], position, deltaTime * __instance.segmentSnapSpeed * scale);
                    if (__instance.maxSegmentOffset > 0f && (position - b).sqrMagnitude > __instance.maxSegmentOffset * __instance.maxSegmentOffset)
                    {
                        b = position - value.normalized * __instance.maxSegmentOffset;
                    }
                    Vector3 normalized = __instance.trails[i].InverseTransformVector(vector - b).normalized;
                    Vector3 normalized2 = Vector3.ProjectOnPlane(__instance.trails[i].InverseTransformVector(__instance.prevUpDirection[i]), normalized).normalized;
                    Vector3 vector2 = Quaternion.LookRotation(normalized, normalized2).eulerAngles;
                    vector2 = TrailManager.LerpAngle((__instance.prevRotation[i] * __instance.trailSpaceRotOffset[i]).eulerAngles, vector2, __instance.rotationMultipliers[i] * Time.timeScale);
                    Quaternion quaternion = Quaternion.Slerp(Quaternion.Euler(vector2) * Quaternion.Inverse(__instance.trailSpaceRotOffset[i]), __instance.trailStartRotations[i], TrailManager.lerpRotationSpeed * deltaTime);
                    __instance.trails[i].localRotation = quaternion;
                    Vector3 vector3 = -__instance.trails[i].TransformDirection(__instance.trailSpaceForward[i]) * __instance.distances[i] * scale + vector;
                    __instance.trails[i].position = vector3;
                    __instance.prevPositions[i] = vector3;
                    __instance.prevRotation[i] = quaternion;
                    __instance.prevUpDirection[i] = __instance.trails[i].TransformDirection(__instance.trailSpaceUp[i]);
                    vector = vector3;
                }
                return false;
            }

            private static bool ShouldUseNewUpdateMethod(TrailManager __instance)
            {
                if (__instance is TrailManagerECC trailManagerECC)
                {
                    return trailManagerECC.usesNewECCTrailBehaviour;
                }
                return false;
            }
        }
#endif
    }
}
