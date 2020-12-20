using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QModManager.API.ModLoading;
using QModManager.Utility;
using UnityEngine;

namespace ECC_Library
{
    [QModCore]
    public class QPatch
    {
        [QModPatch]
        public static void Patch()
        {
            Debug.Log("Eel's Creature Creator loaded.");
        }
    }
}
