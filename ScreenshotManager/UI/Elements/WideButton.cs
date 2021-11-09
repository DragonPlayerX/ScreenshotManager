using System;
using TMPro;
using UnityEngine;

namespace ScreenshotManager.UI.Elements
{
    public class WideButton : VRCSelectable
    {

        public TextMeshProUGUI TextComponent { get; private set; }

        public string Text
        {
            get => TextComponent.text;
            set => TextComponent.text = value;
        }

        public WideButton(Action onClick, string text, string tooltip, string gameObjectName, Transform parent = null) : base(onClick, parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Worlds").gameObject, gameObjectName)
        {
            RectTransform.Find("Badge_MMJump").gameObject.SetActive(false);
            RectTransform.Find("Icon").gameObject.SetActive(false);
            TextComponent = RectTransform.Find("Text_H4").GetComponent<TextMeshProUGUI>();
            TextComponent.fontSize = 50;

            RectTransform textTransform = TextComponent.transform.GetComponent<RectTransform>();
            textTransform.sizeDelta = new Vector2(880, 48);
            textTransform.anchoredPosition = new Vector3(0, 28, 0);

            Text = text;
            TooltipText = tooltip;  
        }
    }
}