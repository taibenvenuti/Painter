using ICities;

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
    }
}
