using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Painter
{
    public class Painter : Singleton<Painter>
    {
        private Dictionary<ushort, SerializableColor> colors;
        internal Dictionary<ushort, SerializableColor> Colors
        {
            get
            {
                if (colors == null) colors = new Dictionary<ushort, SerializableColor>();
                return colors;
            }
            set
            {
                colors = value;
            }
        }
        private Dictionary<PanelType, BuildingWorldInfoPanel> Panels;
        internal Dictionary<PanelType, UIColorField> ColorFields;
        private UIColorField colorFIeldTemplate;
        private UIButton copyButton;
        private UIButton resetButton;
        private UIButton pasteButton;
        private UICheckBox colorizeCheckbox;
        private UICheckBox invertCheckbox;
        private Color32 copyPasteColor;
        internal ushort BuildingID;
        internal BuildingInfo Building => BuildingManager.instance.m_buildings.m_buffer[BuildingID].Info;
        private string CopyText => UserMod.Translation.GetTranslation("PAINTER-COPY");
        private string PasteText => UserMod.Translation.GetTranslation("PAINTER-PASTE");
        private string ResetText => UserMod.Translation.GetTranslation("PAINTER-RESET");
        private string ColorizeText => UserMod.Translation.GetTranslation("PAINTER-COLORIZE");
        private string InvertText => UserMod.Translation.GetTranslation("PAINTER-INVERT");
        private string ReloadRequiredTooltip => UserMod.Translation.GetTranslation("PAINTER-RELOAD-REQUIRED");

        internal PainterColorizer colorizer;
        public PainterColorizer Colorizer
        {
            get
            {
                if (colorizer == null)
                {
                    colorizer = PainterColorizer.Load();
                    if (colorizer == null)
                    {
                        colorizer = new PainterColorizer();
                        colorizer.Save();
                    }
                }
                return colorizer;
            }
            set
            {
                colorizer = value;
            }
        }

        private Dictionary<string, bool> madeColoredList;
        internal Dictionary<string, bool> MadeColoredList
        {
            get
            {
                if (madeColoredList == null) madeColoredList = new Dictionary<string, bool>();
                return madeColoredList;
            }
            set
            {
                madeColoredList = value;
            }
        }

        internal Color GetColor()
        {
            var building = BuildingManager.instance.m_buildings.m_buffer[BuildingID];
            return building.Info.m_buildingAI.GetColor(BuildingID, ref building, InfoManager.InfoMode.None);
        }

        private void UpdateColor(Color32 color, ushort currentBuilding)
        {
            if (!Colors.TryGetValue(currentBuilding, out SerializableColor col))
                Colors.Add(currentBuilding, color);
            else Colors[currentBuilding] = color;
            BuildingManager.instance.UpdateBuildingColors(currentBuilding);
        }

        private void ResetColor()
        {
            if (Colors.TryGetValue(BuildingID, out SerializableColor color))
                Colors.Remove(BuildingID);
            BuildingManager.instance.UpdateBuildingColors(BuildingID);
        }

        private void EraseColor()
        {
            var field = Panels[PanelType.Service].component.isVisible ? ColorFields[PanelType.Service] : Panels[PanelType.Shelter].component.isVisible ? ColorFields[PanelType.Shelter] : ColorFields[PanelType.Zoned];
            ResetColor();
            field.selectedColor = GetColor();
            field.SendMessage("ClosePopup", false);
            field.SendMessage("OpenPopup");
        }

        private void PasteColor()
        {
            var field = Panels[PanelType.Service].component.isVisible ? ColorFields[PanelType.Service] : Panels[PanelType.Shelter].component.isVisible ? ColorFields[PanelType.Shelter] : ColorFields[PanelType.Zoned];
            UpdateColor(copyPasteColor, BuildingID);
            field.selectedColor = copyPasteColor;
            field.SendMessage("ClosePopup", false);
            field.SendMessage("OpenPopup");
        }

        internal void Colorize(BuildingInfo building, bool invert)
        {
            try
            {
                building.GetComponent<Renderer>().material.UpdateACI(invert);
                building.m_lodObject.GetComponent<Renderer>().material.UpdateACI(invert);
                foreach (var subBuilding in building.m_subMeshes)
                {
                    try
                    {
                        subBuilding.m_subInfo.GetComponent<Renderer>().material.UpdateACI(invert);
                        subBuilding.m_subInfo.m_lodObject.GetComponent<Renderer>().material.UpdateACI(invert);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }

        internal void AddColorFieldsToPanels()
        {
            Panels = new Dictionary<PanelType, BuildingWorldInfoPanel>
            {
                [PanelType.Service] = GameObject.Find("(Library) CityServiceWorldInfoPanel")?.GetComponent<CityServiceWorldInfoPanel>(),
                [PanelType.Shelter] = GameObject.Find("(Library) ShelterWorldInfoPanel")?.GetComponent<ShelterWorldInfoPanel>(),
                [PanelType.Zoned] = GameObject.Find("(Library) ZonedBuildingWorldInfoPanel")?.GetComponent<ZonedBuildingWorldInfoPanel>()
            };
            ColorFields = new Dictionary<PanelType, UIColorField>
            {
                [PanelType.Service] = CreateColorField(Panels[PanelType.Service]?.component),
                [PanelType.Shelter] = CreateColorField(Panels[PanelType.Shelter]?.component),
                [PanelType.Zoned] = CreateColorField(Panels[PanelType.Zoned]?.component),
            };
        }

        private UIColorField CreateColorField(UIComponent parent)
        {
            if (colorFIeldTemplate == null)
            {
                UIComponent template = UITemplateManager.Get("LineTemplate");
                if (template == null) return null;

                colorFIeldTemplate = template.Find<UIColorField>("LineColor");
                if (colorFIeldTemplate == null) return null;
            }

            UIColorField cF = Instantiate(colorFIeldTemplate.gameObject).GetComponent<UIColorField>();
            parent.AttachUIComponent(cF.gameObject);
            cF.name = "PainterColorField";
            cF.AlignTo(parent, UIAlignAnchor.TopRight);
            cF.relativePosition += new Vector3(-40f, 43f, 0f);
            cF.size = new Vector2(26f, 26f);
            cF.pickerPosition = UIColorField.ColorPickerPosition.RightBelow;
            cF.eventSelectedColorChanged += EventSelectedColorChangedHandler;
            cF.eventColorPickerOpen += EventColorPickerOpenHandler;
            return cF;
        }


        private UIButton CreateButton(UIComponent parentComponent, string text)
        {
            UIButton button = parentComponent.AddUIComponent<UIButton>();
            button.name = text + "Button";
            button.text = text == "Copy" ? CopyText : text == "Paste" ? PasteText : ResetText;
            button.width = 71.33333333333333f;
            button.height = 20f;
            button.textPadding = new RectOffset(0, 0, 5, 0);
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textScale = 0.8f;
            button.atlas = UIView.GetAView().defaultAtlas;
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            return button;
        }

        public UICheckBox CreateCheckBox(UIComponent parent, string fieldName)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            checkBox.name = fieldName;
            checkBox.width = 20f;
            checkBox.height = 20f;
            checkBox.relativePosition = Vector3.zero;

            UILabel label = checkBox.AddUIComponent<UILabel>();
            label.text = fieldName == "Colorize" ? ColorizeText : InvertText;
            label.textScale = 0.8f;
            label.relativePosition = new Vector3(22f, 5f);

            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = new Vector3(2f, 2f);

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            var building = BuildingManager.instance.m_buildings.m_buffer[BuildingID].Info.name;
            checkBox.isChecked = fieldName == "Colorize" ? Colorizer.Colorized.Contains(building) : Colorizer.Inverted.Contains(building);
            checkBox.tooltip = ReloadRequiredTooltip;
            return checkBox;
        }

        private void EventSelectedColorChangedHandler(UIComponent component, Color value)
        {
            UpdateColor(value, BuildingID);
        }

        private void EventColorPickerOpenHandler(UIColorField colorField, UIColorPicker colorPicker, ref bool overridden)
        {
            colorPicker.component.height += Loading.IsHooked() ? 60f : 30f;
            if(Loading.IsHooked()) colorizeCheckbox = CreateCheckBox(colorPicker.component, "Colorize");
            if (Loading.IsHooked()) invertCheckbox = CreateCheckBox(colorPicker.component, "Invert");
            copyButton = CreateButton(colorPicker.component, "Copy");
            pasteButton = CreateButton(colorPicker.component, "Paste");
            resetButton = CreateButton(colorPicker.component, "Reset");
            copyButton.relativePosition = new Vector3(10f, 223f);
            pasteButton.relativePosition = new Vector3(91.33333333333333f, 223f);
            resetButton.relativePosition = new Vector3(172.6666666666667f, 223f);
            colorizeCheckbox.relativePosition = new Vector3(10f, 253f);
            invertCheckbox.relativePosition = new Vector3(127f, 253f);
            if (Loading.IsHooked()) colorizeCheckbox.eventCheckChanged += (c, e) =>
            {
                ToggleCheckboxes(Colorizer.Colorized, e);
                if (e) invertCheckbox.isChecked = !e;
                Colorizer.Save();
            };
            if (Loading.IsHooked()) invertCheckbox.eventCheckChanged += (c, e) =>
            {
                ToggleCheckboxes(Colorizer.Inverted, e);
                if (e) colorizeCheckbox.isChecked = !e;
                Colorizer.Save();
            };
            copyButton.eventClick += (c, e) =>
            {
                copyPasteColor = GetColor();
            };
            pasteButton.eventClick += (c, e) =>
            {
                PasteColor();
            };
            resetButton.eventClick += (c, e) =>
            {
                EraseColor();
            };
        }

        private void ToggleCheckboxes(List<string> list, bool value)
        {
            if (value && !list.Contains(Building.name)) list.Add(Building.name);
            if (!value && list.Contains(Building.name)) list.RemoveAll(building => building == Building.name);
        }
    }

    public enum PanelType
    {
        None = -1,
        Service,
        Shelter,
        Zoned,
        Count
    }
}