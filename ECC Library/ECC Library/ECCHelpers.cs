using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using SMLHelper.V2.Assets;
using System.IO;
using FMOD;
using ECCLibrary.Internal;

namespace ECCLibrary
{
    public static class ECCHelpers
    {
        public static AssetBundle LoadAssetBundleFromAssetsFolder(Assembly modAssembly, string assetsFileName)
        {
            return AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(modAssembly.Location), "Assets", assetsFileName));
        }
        public static void ApplySNShaders(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            var newShader = Shader.Find("MarmosetUBER");
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    Material material = renderers[i].materials[j];
                    material.shader = newShader;

                    Texture specularTexture = material.GetTexture("_SpecGlossMap");
                    if (specularTexture != null)
                    {
                        material.SetTexture("_SpecTex", specularTexture);
                        material.SetFloat("_SpecInt", 5f);
                        material.SetFloat("_Shininess", material.GetFloat("_Glossiness") * 10f);
                        material.EnableKeyword("MARMO_SPECMAP");
                        material.SetColor("_SpecColor", new Color(0.796875f, 0.796875f, 0.796875f, 0.796875f));
                        material.SetFloat("_Fresnel", 0f);
                        material.SetVector("_SpecTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    }
                    Texture emissionTexture = material.GetTexture("_EmissionMap");
                    if (material.IsKeywordEnabled("_EMISSION"))
                    {
                        material.EnableKeyword("MARMO_EMISSION");
                        material.SetFloat("_EnableGlow", 1f);
                        material.SetTexture("_Illum", emissionTexture);
                    }

                    if (material.GetTexture("_BumpMap"))
                    {
                        material.EnableKeyword("_NORMALMAP");
                    }
                }
            }
        }
        public static SwimBehaviour EssentialComponentSystem_Swimming(GameObject prefab, float turnSpeed, Rigidbody rb)
        {
            Locomotion locomotion = prefab.AddComponent<Locomotion>();
            locomotion.useRigidbody = rb;
            SplineFollowing splineFollow = prefab.AddComponent<SplineFollowing>();
            splineFollow.respectLOD = false;
            splineFollow.locomotion = locomotion;
            SwimBehaviour swim = prefab.AddComponent<SwimBehaviour>();
            swim.splineFollowing = splineFollow;
            swim.turnSpeed = turnSpeed;
            return swim;
        }
        public static BehaviourLOD EssentialComponent_BehaviourLOD(GameObject prefab, float near, float medium, float far)
        {
            BehaviourLOD bLod = prefab.AddComponent<BehaviourLOD>();
            bLod.veryCloseThreshold = near;
            bLod.closeThreshold = medium;
            bLod.farThreshold = far;
            return bLod;
        }
        public static void MakeAcidImmune(TechType techType)
        {
            List<TechType> acidImmuneList = new List<TechType>(DamageSystem.acidImmune);
            acidImmuneList.Add(techType);
            DamageSystem.acidImmune = acidImmuneList.ToArray();
        }
        public static AnimationCurve Curve_Trail()
        {
            return new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.25f), new Keyframe(1f, 0.75f) });
        }
        public static AnimationCurve Curve_Flat(float value = 1f)
        {
            return new AnimationCurve(new Keyframe[] { new Keyframe(0f, value), new Keyframe(1f, value) });
        }

        public static LiveMixinData CreateNewLiveMixinData()
        {
            return ScriptableObject.CreateInstance<LiveMixinData>();
        }

        public static void SetPrivateField<T>(Type type, T instance, string name, object value)
        {
            var prop = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            prop.SetValue(instance, value);
        }
        public static OutputT GetPrivateField<OutputT>(Type type, object target, string name)
        {
            var prop = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return (OutputT)prop.GetValue(target);
        }
        public static OutputT GetPrivateStaticField<OutputT>(Type type, string name)
        {
            var prop = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
            return (OutputT)prop.GetValue(null);
        }
        public static void PatchBehaviorType(TechType techType, BehaviourType behaviourType)
        {
            GetPrivateStaticField<Dictionary<TechType, BehaviourType>>(typeof(BehaviourData), "behaviourTypeList").Add(techType, behaviourType);
        }
        public static void PatchEquipmentType(TechType techType, EquipmentType equipmentType)
        {
            GetPrivateStaticField<Dictionary<TechType, EquipmentType>>(typeof(CraftData), "equipmentTypes").Add(techType, equipmentType);
        }
        public static float GetECCVolume()
        {
            return ECCPatch.config.Volume;
        }
        public static bool CompareStrings(string original, string compareTo, ECCStringComparison comparisonMode)
        {
            switch (comparisonMode)
            {
                default:
                    return original == compareTo;
                case ECCStringComparison.Equals:
                    return original.ToLower() == compareTo.ToLower();
                case ECCStringComparison.EqualsCaseSensitive:
                    return original == compareTo;
                case ECCStringComparison.StartsWith:
                    return original.ToLower().StartsWith(compareTo.ToLower());
                case ECCStringComparison.StartsWithCaseSensitive:
                    return original.StartsWith(compareTo);
                case ECCStringComparison.Contains:
                    return original.ToLower().Contains(compareTo.ToLower());
                case ECCStringComparison.ContainsCaseSensitive:
                    return original.StartsWith(compareTo);
            }
        }
    }
    public static class GameObjectExtensions
    {
        public static GameObject SearchChild(this GameObject gameObject, string byName, ECCStringComparison stringComparison = ECCStringComparison.Equals)
        {
            GameObject obj = SearchChildRecursive(gameObject, byName, stringComparison);
            if(obj == null)
            {
                ECCLog.AddMessage("No child found in hierarchy by name {0}.", byName);
            }
            return obj;
        }

        static GameObject SearchChildRecursive(GameObject gameObject, string byName, ECCStringComparison stringComparison)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (ECCHelpers.CompareStrings(child.gameObject.name, byName, stringComparison))
                {
                    return child.gameObject;
                }
                GameObject recursive = SearchChildRecursive(child.gameObject, byName, stringComparison);
                if (recursive)
                {
                    return recursive;
                }
            }
            return null;
        }
    }
    public enum ECCStringComparison
    {
        Equals,
        EqualsCaseSensitive,
        StartsWith,
        StartsWithCaseSensitive,
        Contains,
        ContainsCaseSensitive
    }
}
