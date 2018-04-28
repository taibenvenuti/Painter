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
        private Color32 copyPasteColor;
        internal ushort BuildingID;
        internal bool IsPanelVisible;
        private string CopyText => UserMod.Translation.GetTranslation("PAINTER-COPY");
        private string PasteText => UserMod.Translation.GetTranslation("PAINTER-PASTE");
        private string ResetText => UserMod.Translation.GetTranslation("PAINTER-RESET");
                
        private void Update()
        {
            if (!IsPanelVisible) return;
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(KeyCode.C))
                copyPasteColor = GetColor();
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(KeyCode.V))
                PasteColor();
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
                EraseColor();    
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

        private void EventSelectedColorChangedHandler(UIComponent component, Color value)
        {
            UpdateColor(value, BuildingID);
        }

        private void EventColorPickerOpenHandler(UIColorField colorField, UIColorPicker colorPicker, ref bool overridden)
        {
            colorPicker.component.height += 30f;
            copyButton = CreateButton(colorPicker.component, "Copy");
            pasteButton = CreateButton(colorPicker.component, "Paste");
            resetButton = CreateButton(colorPicker.component, "Reset");
            copyButton.relativePosition = new Vector3(10f, 223f);
            pasteButton.relativePosition = new Vector3(91.33333333333333f, 223f);
            resetButton.relativePosition = new Vector3(172.6666666666667f, 223f);
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
