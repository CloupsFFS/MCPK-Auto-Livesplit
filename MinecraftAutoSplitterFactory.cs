using LiveSplit.Model;
using System;

namespace LiveSplit.MinecraftAutoSplitter
{
    public class MinecraftAutoSplitterFactory : IComponentFactory
    {
        public string ComponentName => "Minecraft Auto Splitter";
        public string Description => "Automatic splitting for Minecraft Java Edition speedruns";
        public ComponentCategory Category => ComponentCategory.Control;
        public string UpdateName => ComponentName;
        public string XMLURL => "";
        public string UpdateURL => "";
        public Version Version => Version.Parse("1.0.0");

        public IComponent Create(LiveSplitState state)
        {
            return new MinecraftAutoSplitter(state);
        }
    }
} 