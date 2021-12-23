using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace ECCLibrary
{
    /// <summary>
    /// Main and only config file for ECCLibrary.
    /// </summary>
    [Menu("Eel's Creature Creator Settings (ECC Library)")]
    public class ECCConfig : ConfigFile
    {
        /// <summary>
        /// This is called VolumeNew because there was an obsolete property here at one time that ranged only from 0-1. The normalized value should be gotten with the <see cref="ECCHelpers.GetECCVolume"/>.
        /// </summary>
        [Slider("ECC Master volume", 0f, 100f, Step = 1f, DefaultValue = 50f, Tooltip = "This is the volume for all audio in mods that require ECC. Independent from the in-game sound setting.\nRESTART REQUIRED.")]
        public float VolumeNew = 50f;
        /// <summary>
        /// Whether messages are displayed on the screen or not.
        /// </summary>
        [Toggle("ECC Verbose Messages", Tooltip = "Whether to display ECC log messages & errors on the screen.")]
        public bool ECCLogMessages;
    }
}
