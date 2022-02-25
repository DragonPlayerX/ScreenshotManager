using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.DataModel.Core;
using VRC.UI.Elements.Tooltips;

namespace ScreenshotManager.UI.Elements
{
    public class ToggleButton : ElementBase
    {

        public bool State
        {
            get => ToggleComponent.isOn;
            set => ToggleComponent.isOn = value;
        }

        public Toggle ToggleComponent { get; private set; }
        public Action<bool> OnClick { get; set; }
        public TextMeshProUGUI TextComponent { get; private set; }

        public string Text
        {
            get => TextComponent.text;
            set => TextComponent.text = value;
        }

        public UiToggleTooltip Tooltip { get; private set; }

        public string TooltipTextOn
        {
            get => Tooltip.field_Public_String_0;
            set => Tooltip.field_Public_String_0 = value;
        }

        public string TooltipTextOff
        {
            get => Tooltip.field_Public_String_1;
            set => Tooltip.field_Public_String_1 = value;
        }

        private readonly string _onText;
        private readonly string _offText;

        public ToggleButton(Action<bool> onClick, Sprite onIcon, Sprite offIcon, string onText, string offText, string onTooltip, string offTooltip, string gameObjectName, bool defaultState = false) : base(null, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Settings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UI_Elements_Row_1/Button_ToggleQMInfo").gameObject, gameObjectName)
        {
            ToggleComponent = RectTransform.GetComponent<Toggle>();
            ToggleComponent.isOn = defaultState;

            _onText = onText;
            _offText = offText;

            TextComponent = RectTransform.Find("Text_H4").GetComponent<TextMeshProUGUI>();
            if (defaultState)
                Text = onText;
            else
                Text = offText;

            OnClick = onClick;

            Tooltip = GameObject.GetComponent<UiToggleTooltip>();
            TooltipTextOn = onTooltip;
            TooltipTextOff = offTooltip;
            Tooltip.prop_Boolean_0 = !defaultState;

            Image onImage = RectTransform.Find("Icon_On").GetComponent<Image>();
            Image offImage = RectTransform.Find("Icon_Off").GetComponent<Image>();
            onImage.sprite = onIcon;
            offImage.sprite = offIcon;

            ObjectPublicAbstractSealedVoGa9326CoAc63Ac26CoUnique.Method_Public_Static_ToggleBindingHelper_Toggle_Action_1_Boolean_0(ToggleComponent, new Action<bool>(state =>
            {
                if (state)
                    Text = _onText;
                else
                    Text = _offText;
                OnClick.Invoke(state);
            }));
        }
    }
}