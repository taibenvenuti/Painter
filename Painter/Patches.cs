using Harmony;
using System.Reflection;
using UnityEngine;

namespace Painter
{
    [HarmonyPatch(typeof(BuildingAI), "GetColor")]
    class BuildingAIPatch
    {
        static void Postfix(ref Color __result, ushort buildingID)
        {
            if (Painter.instance.Colors.TryGetValue(buildingID, out SerializableColor color)) __result = color;
        }
    }

    [HarmonyPatch(typeof(ShelterWorldInfoPanel), "OnSetTarget")]
    class ShelterWorldInfoPanelPatch
    {
        static void Postfix(ShelterWorldInfoPanel __instance)
        {
            Painter.instance.BuildingID = ((InstanceID)__instance.GetType().GetField("m_InstanceID", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).Building;
            Painter.instance.ColorFields[PanelType.Shelter].selectedColor = Painter.instance.GetColor();
        }
    }

    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
    class CityServiceWorldInfoPanelPatch
    {
        static void Postfix(CityServiceWorldInfoPanel __instance)
        {
            Painter.instance.BuildingID = ((InstanceID)__instance.GetType().GetField("m_InstanceID", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).Building;
            Painter.instance.ColorFields[PanelType.Service].selectedColor = Painter.instance.GetColor();
        }
    }

    [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel), "OnSetTarget")]
    class ZonedBuildingWorldInfoPanelPatch
    {
        static void Postfix(ZonedBuildingWorldInfoPanel __instance)
        {
            Painter.instance.BuildingID = ((InstanceID)__instance.GetType().GetField("m_InstanceID", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).Building;
            Painter.instance.ColorFields[PanelType.Zoned].selectedColor = Painter.instance.GetColor();
        }
    }
}
