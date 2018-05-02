using PrefabHook;

namespace Painter
{
    public class Hook
    {
        public void Deploy()
        {
            BuildingInfoHook.OnPreInitialization += OnPreBuildingInit;
            BuildingInfoHook.Deploy();
        }

        public void Revert()
        {
            BuildingInfoHook.OnPreInitialization -= OnPreBuildingInit;
            BuildingInfoHook.Revert();
        }

        public void OnPreBuildingInit(BuildingInfo prefab)
        {
            if (Painter.instance.Colorizer.Colorized.Contains(prefab.name)) Painter.instance.Colorize(prefab, false);
            else if (Painter.instance.Colorizer.Inverted.Contains(prefab.name)) Painter.instance.Colorize(prefab, true);
        }
    }
}
