using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QModManager.API.ModLoading;
using QModManager.Utility;
using UnityEngine;
using SMLHelper.V2.Handlers;

namespace ECCLibrary
{
    [QModCore]
    public class ECCPatch
    {
        public static ECCConfig config = OptionsPanelHandler.Main.RegisterModOptions<ECCConfig>();

        [QModPatch]
        public static void Patch()
        {
            Debug.Log("Eel's Creature Creator loaded.");
        }

        private static void MaterialDebug_Unused()
        {
            GameObject peeper = CraftData.GetPrefabForTechType(TechType.Peeper);
            foreach (Renderer renderer in peeper.GetComponentsInChildren<Renderer>())
            {
                Debug.Log("ECC Test----");
                Debug.Log(renderer.gameObject.name);
                foreach (Material mat in renderer.materials)
                {
                    Debug.Log("Looking over material " + mat.name);
                    Debug.Log("Shader name: " + mat.shader.name);
                    Debug.Log("Keywords:");
                    foreach (string keyw in mat.shaderKeywords)
                    {
                        Debug.Log("Keyword: " + keyw);
                        Debug.Log("Keyword enabled: " + mat.IsKeywordEnabled(keyw));
                    }
                    Debug.Log("Properties of material " + mat.name);
                    Debug.Log("_SpecInt: " + mat.GetFloat("_SpecInt"));
                    Debug.Log("_Shininess: " + mat.GetFloat("_Shininess"));
                }
            }
        }
    }
}
