using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace ECCLibrary
{
    [Menu("Creature Creator Settings")]
    public class ECCConfig : ConfigFile
    {
        [Slider("ECC Master volume", 0f, 1f, Step = 0.02f, DefaultValue = 1f, Tooltip = "Not influenced by the in-game sound setting.")]
        public float Volume;
        [Toggle("ECC Log messages")]
        public bool ECCLogMessages;
    }
}
