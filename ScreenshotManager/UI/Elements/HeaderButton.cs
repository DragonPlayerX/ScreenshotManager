using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Core.Styles;

namespace ScreenshotManager.UI.Elements
{
    public class HeaderButton : VRCSelectable
    {

        private Image _image;

        public Sprite Sprite
        {
            get => _image.sprite;
            set => _image.sprite = value;
        }

        public HeaderButton(Action onClick, Sprite icon, string tooltip, string gameObjectName) : base(onClick, null, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Settings/QMHeader_H1/RightItemContainer/Button_QM_Expand").gameObject, gameObjectName)
        {
            RectTransform.GetComponent<StyleElement>().field_Public_String_0 = icon.name;
            _image = RectTransform.Find("Icon").GetComponent<Image>();
            _image.sprite = icon;

            TooltipText = tooltip;
        }
    }
}