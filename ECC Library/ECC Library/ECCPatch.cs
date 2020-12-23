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
    }
}
