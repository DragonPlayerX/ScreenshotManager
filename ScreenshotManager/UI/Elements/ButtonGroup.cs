using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScreenshotManager.UI.Elements
{

    public class ButtonGroup : ElementBase
    {

        public SubMenu ParentMenu { get; internal set; }
        public ButtonHeader Header { get; private set; }
        public GridLayoutGroup ButtonLayoutGroup { get; private set; }

        public IReadOnlyList<ElementBase> Buttons => _buttons;
        private readonly List<ElementBase> _buttons = new List<ElementBase>();

        public ButtonGroup(string name, string headerText = null, bool adjustAlignment = false, Transform parent = null) : base(parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks").gameObject, $"Buttons_{name}")
        {
            ButtonLayoutGroup = GameObject.GetComponent<GridLayoutGroup>();
            for (int i = ButtonLayoutGroup.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(ButtonLayoutGroup.transform.GetChild(i).gameObject);

            if (headerText != null)
                AddButtonHeader(headerText, "Header_" + name);

            if (adjustAlignment)
            {
                ButtonLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                ButtonLayoutGroup.padding.left = 64;
                ButtonLayoutGroup.padding.right = 64;
            }
        }

        public ButtonGroup AddButtonHeader(string text, string gameObjectName)
        {
            Header = new ButtonHeader(RectTransform.parent, text, gameObjectName);
            RectTransform.SetSiblingIndex(RectTransform.GetSiblingIndex() + 1);
            return this;
        }

        public ButtonGroup RemoveButtonHeader()
        {
            Object.DestroyImmediate(Header.GameObject);
            Header = null;
            return this;
        }

        public ButtonGroup AddButton(ElementBase button)
        {
            button.RectTransform.parent = ButtonLayoutGroup.transform;
            _buttons.Add(button);
            return this;
        }

        public ButtonGroup RemoveButton(ElementBase button)
        {
            _buttons.Remove(button);
            Object.DestroyImmediate(button.GameObject);
            return this;
        }

        public ButtonGroup ClearButtons()
        {
            foreach (ElementBase button in _buttons)
                Object.Destroy(button.GameObject);
            _buttons.Clear();
            return this;
        }
    }
}
