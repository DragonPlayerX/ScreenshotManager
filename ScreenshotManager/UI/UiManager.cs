using System;
using UnityEngine;
using VRC.UI.Elements;
using VRC.UI.Elements.Controls;

using Object = UnityEngine.Object;

namespace ScreenshotManager.UI
{
    public class UiManager
    {
        public static Transform TempUIParent;

        public static VRC.UI.Elements.QuickMenu QuickMenu;
        public static MenuStateController QMStateController { get; private set; }

        private static ModalAlert quickMenuAlert;

        public static void UiInit()
        {
            TempUIParent = new GameObject("ScreenshotManagerTempUIParent").transform;
            Object.DontDestroyOnLoad(TempUIParent.gameObject);

            GameObject quickMenuObject = GameObject.Find("UserInterface").transform.Find("Canvas_QuickMenu(Clone)").gameObject;
            QMStateController = quickMenuObject.GetComponent<MenuStateController>();
            QuickMenu = quickMenuObject.GetComponent<VRC.UI.Elements.QuickMenu>();

            quickMenuAlert = quickMenuObject.transform.Find("Container/Window/QMParent/Modal_Alert").GetComponent<ModalAlert>();
        }

        public static void PushQuickMenuAlert(string text) => quickMenuAlert.Method_Public_Void_String_3(text);

        public static void ShowQuickMenuInformationPopup(string title, string body, Action action) => QuickMenu.Method_Public_Void_String_String_Action_PDM_0(title, body, action);

        public static void ShowQuickMenuPopup(string title, string body, string yesLabel, string noLabel, Action yesAction, Action noAction) => QuickMenu.Method_Public_Void_String_String_String_String_Action_Action_0(title, body, yesLabel, noLabel, yesAction, noAction);
    }
}
