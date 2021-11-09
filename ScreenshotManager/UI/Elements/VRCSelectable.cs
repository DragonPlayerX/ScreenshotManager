using System;
using UnityEngine;
using UnityEngine.UI;

namespace ScreenshotManager.UI.Elements
{
    public class VRCSelectable : ElementBase
    {

        public Action OnClick { get; set; }
        public Button ButtonComponent { get; private set; }
        public VRC.UI.Elements.Tooltips.UiTooltip Tooltip { get; private set; }

        public string TooltipText
        {
            get => Tooltip.field_Public_String_0;
            set => Tooltip.field_Public_String_0 = value;
        }

        public VRCSelectable(Action onClick, Transform parent, GameObject template, string gameObjectName) : base(parent, template, gameObjectName)
        {
            ButtonComponent = GameObject.GetComponent<Button>();
            Tooltip = GameObject.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>();
            OnClick = onClick;

            ButtonComponent.onClick.AddListener(new Action(() => OnClick.Invoke()));
        }
    }
}
