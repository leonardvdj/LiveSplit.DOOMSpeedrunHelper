using System;
using LiveSplit.DOOMSpeedrunHelper;
using LiveSplit.Model;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(DOOMSpeedrunHelperComponentFactory))]

namespace LiveSplit.DOOMSpeedrunHelper
{
    internal class DOOMSpeedrunHelperComponentFactory : IComponentFactory
    {
        public string ComponentName => "DOOM Speedrun Helper";

        public string Description => "Helper for the DOOM speedrun.";

        public ComponentCategory Category => ComponentCategory.Other;

        public string UpdateName => ComponentName;

        public string XMLURL => null;

        public string UpdateURL => null;

        public Version Version => Version.Parse("1.0.1");

        public IComponent Create(LiveSplitState state)
        {
            return new DOOMSpeedrunHelperComponent();
        }
    }
}
