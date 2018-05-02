using System;
using System.Linq;
using ColossalFramework.Plugins;
using ICities;

namespace Painter
{
    public class Loading : LoadingExtensionBase
    {
        private AppMode mode;
        private bool doneUpdate;
        private Hook hook;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            mode = loading.currentMode;
            if (!IsHooked() || loading.currentMode != AppMode.Game) return;
            hook = new Hook();
            hook.Deploy();
        }        

        public override void OnLevelLoaded(LoadMode loadMode)
        {
            base.OnLevelLoaded(loadMode);
            if (mode != AppMode.Game) return;
            while (!doneUpdate)
            {
                if(LoadingManager.instance.m_loadingComplete)
                {
                    Painter.instance.AddColorFieldsToPanels();
                    SimulationManager.instance.AddAction(() => 
                    {
                        for (ushort i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; i++)
                        {
                            if (BuildingManager.instance.m_buildings.m_buffer[i].m_flags == 0) continue;
                            BuildingManager.instance.UpdateBuildingColors(i);
                        }
                    });
                    doneUpdate = true;
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (!IsHooked()) return;
            hook.Revert();
            hook = null;
        }

        public static bool IsHooked()
        {
            var plugins = PluginManager.instance.GetPluginsInfo();
            return (from plugin in plugins.Where(p => p.isEnabled)
                    select plugin.GetInstances<IUserMod>() into instances
                    where instances.Any()
                    select instances[0].Name into name
                    where name == "Prefab Hook"
                    select name).Any();
        }
    }
}
