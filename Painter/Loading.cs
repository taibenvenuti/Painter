using System.Linq;
using ColossalFramework.Plugins;
using ICities;
using PrefabHook;

namespace Painter
{
    public class Loading : LoadingExtensionBase
    {
        private AppMode mode;
        private bool doneUpdate;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            mode = loading.currentMode;
            if (!IsHooked() || loading.currentMode != AppMode.Game) return;
            BuildingInfoHook.OnPreInitialization += OnPreBuildingInit;
            BuildingInfoHook.Deploy();
        }

        public void OnPreBuildingInit(BuildingInfo prefab)
        {
            if(Painter.instance.Colorizer.Colorized.Contains(prefab.name)) Painter.instance.Colorize(prefab, false);
            else if (Painter.instance.Colorizer.Inverted.Contains(prefab.name)) Painter.instance.Colorize(prefab, true);
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
            BuildingInfoHook.OnPreInitialization -= OnPreBuildingInit;
            BuildingInfoHook.Revert();
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
