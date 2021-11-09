using TMPro;
using UnityEngine;

namespace ScreenshotManager.UI.Elements
{
    public class ButtonHeader : ElementBase
    {

        public TextMeshProUGUI LeftTextComponent { get; private set; }

        public string LeftText
        {
            get => LeftTextComponent.text;
            set => LeftTextComponent.text = value;
        }

        public TextMeshProUGUI RightTextComponent { get; private set; }

        public string RightText
        {
            get => RightTextComponent.text;
            set => RightTextComponent.text = value;
        }

        internal ButtonHeader(Transform parent, string leftText, string gameObjectName, string rightText = null) : base(parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Header_QuickLinks").gameObject, gameObjectName)
        {
            LeftTextComponent = RectTransform.Find("LeftItemContainer/Text_Title").GetComponent<TextMeshProUGUI>();
            LeftTextComponent.rectTransform.sizeDelta += new Vector2(200, 0);
            LeftText = leftText;

            if (rightText != null)
            {
                RightTextComponent = Object.Instantiate(LeftTextComponent.gameObject, RectTransform.Find("RightItemContainer").transform).GetComponent<TextMeshProUGUI>();
                RightTextComponent.rectTransform.sizeDelta += new Vector2(200, 0);
                RightTextComponent.alignment = TextAlignmentOptions.MidlineRight;
                RightText = rightText;
            }
        }

        public void Minimize()
        {
            RectTransform.sizeDelta = new Vector2(1024, 40);
            LeftTextComponent.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);

            if (RightTextComponent != null)
                RightTextComponent.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
        }
    }
}
