using Dalamud.Configuration;
using System;

namespace PFInfoPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool showName { get; set; } = true;
        public bool showDescription { get; set; } = true;
        public bool showObjective { get; set; } = true;
        public bool showMinIlvl { get; set; } = true;


        // the below exist just to make saving less cumbersome
        public void Save()
        {
            Plugin.PluginInterface.SavePluginConfig(this);
        }
    }
}
