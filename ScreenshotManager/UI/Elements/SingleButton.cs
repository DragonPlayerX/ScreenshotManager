using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Core.Styles;

namespace ScreenshotManager.UI.Elements
{
    public class SingleButton : VRCSelectable
    {

        public TextMeshProUGUI TextComponent { get; private set; }

        public string Text
        {
            get => TextComponent.text;
            set => TextComponent.text = value;
        }

        private readonly Image _image;

        public Sprite Sprite
        {
            get => _image.sprite;
            set => _image.sprite = value;
        }

        public SingleButton(Action onClick, Sprite icon, string text, string tooltip, string gameObjectName) : base(onClick, null, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Worlds").gameObject, gameObjectName)
        {
            RectTransform.Find("Badge_MMJump").gameObject.SetActive(false);

            TextComponent = RectTransform.Find("Text_H4").GetComponent<TextMeshProUGUI>();

            RectTransform.GetComponent<StyleElement>().field_Public_String_0 = icon.name;
            _image = RectTransform.Find("Icon").GetComponent<Image>();
            _image.sprite = icon;

            Text = text;
            TooltipText = tooltip;
        }
    }
}