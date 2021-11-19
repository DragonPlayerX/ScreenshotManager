using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements.Controls;

namespace ScreenshotManager.UI.Elements
{

    public class TabButton : VRCSelectable
    {

        public TabMenu SubMenu { get; private set; }
        public MenuTab MenuTab { get; private set; }

        private readonly Image _image;

        public Sprite Sprite
        {
            get => _image.sprite;
            set => _image.sprite = value;
        }

        public TabButton(Sprite icon, string pageTitle, string pageName, string tooltip, string gameObjectName) : base(null, UiManager.QMStateController.transform.Find("Container/Window/Page_Buttons_QM/HorizontalLayoutGroup"), UiManager.QMStateController.transform.Find("Container/Window/Page_Buttons_QM/HorizontalLayoutGroup/Page_Dashboard").gameObject, gameObjectName)
        {
            MenuTab = GameObject.GetComponent<MenuTab>();
            MenuTab.field_Private_MenuStateController_0 = UiManager.QMStateController;
            MenuTab.field_Public_String_0 = pageName;

            _image = RectTransform.Find("Icon").GetComponent<Image>();
            _image.sprite = icon;

            TooltipText = tooltip;

            SubMenu = new TabMenu(pageName, "Menu_" + pageName, pageTitle);
        }
    }
}
