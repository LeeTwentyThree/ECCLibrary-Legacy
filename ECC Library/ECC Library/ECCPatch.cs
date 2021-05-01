using QModManager.API.ModLoading;
using UnityEngine;
using SMLHelper.V2.Handlers;
using HarmonyLib;
using System.Reflection;

namespace ECCLibrary
{
    [QModCore]
    public class ECCPatch
    {
        public static string staticCreaturesPath = "uninitialized";
        public static ECCConfig config = OptionsPanelHandler.Main.RegisterModOptions<ECCConfig>();

        [QModPatch]
        public static void Patch()
        {
            LanguageHandler.SetLanguageLine("EncyPath_Lifeforms/Fauna/Eggs", "Creature Eggs");
            Harmony harmony = new Harmony("Lee23.ECC");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Internal.ECCLog.AddMessage("Eel's Creature Creator loaded.");
        }
    }
}
