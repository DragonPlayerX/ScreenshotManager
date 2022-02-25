using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;

using Object = UnityEngine.Object;

namespace ScreenshotManager.UI.Elements
{
    public class SubMenu : ElementBase
    {
        public UIPage UiPage { get; private set; }
        public VerticalLayoutGroup PageLayoutGroup { get; private set; }
        public TextMeshProUGUI TitleText { get; private set; }

        public string Text
        {
            get => TitleText.text;
            set => TitleText.text = value;
        }

        public IReadOnlyList<ButtonGroup> ButtonGroups => _buttonGroups;
        private readonly List<ButtonGroup> _buttonGroups = new List<ButtonGroup>();

        public SubMenu(string pageName, string gameObjectName, string pageTitle, bool backButtonVisible = false, bool enableScrolling = false) : base(UiManager.QMStateController.transform.Find("Container/Window/QMParent"), UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard").gameObject, gameObjectName)
        {
            Object.DestroyImmediate(GameObject.GetComponent<UIPage>());
            Object.Destroy(RectTransform.Find("Header_H1/RightItemContainer/Button_QM_Expand").gameObject);

            PageLayoutGroup = RectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup").GetComponent<VerticalLayoutGroup>();
            PageLayoutGroup.childControlHeight = false;
            for (int i = PageLayoutGroup.rectTransform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(PageLayoutGroup.transform.GetChild(i).gameObject);

            TitleText = RectTransform.Find("Header_H1/LeftItemContainer/Text_Title").GetComponent<TextMeshProUGUI>();
            UiPage = GameObject.AddComponent<UIPage>();
            UiPage.field_Protected_MenuStateController_0 = UiManager.QMStateController;
            UiPage.field_Public_String_0 = pageName;

            UiManager.QMStateController.field_Private_Dictionary_2_String_UIPage_0.Add(pageName, UiPage);
            Text = pageTitle;

            RectTransform.Find("Header_H1/LeftItemContainer/Button_Back").gameObject.SetActive(backButtonVisible);

            if (enableScrolling)
            {
                ScrollRect scrollRect = RectTransform.Find("ScrollRect").GetComponent<ScrollRect>();
                scrollRect.enabled = true;
                scrollRect.vertical = true;
                scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                scrollRect.transform.Find("Viewport").GetComponent<RectMask2D>().enabled = true;
                Scrollbar scrollbar = scrollRect.transform.Find("Scrollbar").GetComponent<Scrollbar>();
                scrollRect.verticalScrollbar = scrollbar;
            }

            RectTransform.SetSiblingIndex(UiManager.QMStateController.transform.Find("Container/Window/QMParent/Modal_AddMessage").GetSiblingIndex());
        }

        public void AddHeaderButton(HeaderButton button)
        {
            button.GameObject.transform.parent = RectTransform.Find("Header_H1/RightItemContainer");
            button.GameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        public void AddButtonGroup(ButtonGroup buttonGroup)
        {
            buttonGroup.ParentMenu = this;
            if (buttonGroup.Header != null)
                buttonGroup.Header.RectTransform.parent = PageLayoutGroup.rectTransform;
            buttonGroup.RectTransform.parent = PageLayoutGroup.rectTransform;
            buttonGroup.RectTransform.localRotation = Quaternion.Euler(Vector3.zero);
            _buttonGroups.Add(buttonGroup);
        }

        public void RemoveButtonGroup(ButtonGroup buttonGroup)
        {
            _buttonGroups.Remove(buttonGroup);
            Object.DestroyImmediate(buttonGroup.GameObject);
        }

        public void ClearButtonGroups()
        {
            foreach (ButtonGroup buttonGroup in _buttonGroups)
            {
                if (buttonGroup.Header != null)
                    Object.DestroyImmediate(buttonGroup.Header.GameObject);
                Object.DestroyImmediate(buttonGroup.GameObject);
            }
            _buttonGroups.Clear();
        }
    }
}