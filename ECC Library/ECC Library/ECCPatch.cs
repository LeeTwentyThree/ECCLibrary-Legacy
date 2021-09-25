using QModManager.API.ModLoading;
using UnityEngine;
using SMLHelper.V2.Handlers;
using HarmonyLib;
using System.Reflection;

namespace ECCLibrary
{
    /// <summary>
    /// Do not access this class unless you need to.
    /// </summary>
    [QModCore]
    public class ECCPatch
    {
        /// <summary>
        /// The cached directory for where StaticCreatureSpawns are written to. Is set to `uninitialized`
        /// </summary>
        public static string staticCreaturesPath = "uninitialized";
        /// <summary>
        /// ECCLibrary config.
        /// </summary>
        public static ECCConfig config = OptionsPanelHandler.Main.RegisterModOptions<ECCConfig>();

        /// <summary>
        /// The main Patch method for ECC. Do not call this unless you are insane.
        /// </summary>
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
