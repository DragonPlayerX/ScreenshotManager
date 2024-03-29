﻿using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC.UI.Elements;
using VRC.UI.Elements.Controls;

using Object = UnityEngine.Object;
using VRCQuickMenu = VRC.UI.Elements.QuickMenu;

using ScreenshotManager.Utils;
using ScreenshotManager.UI.Elements;

namespace ScreenshotManager.UI
{
    public class UiManager
    {
        public static Transform TempUIParent;

        public static VRCQuickMenu QuickMenu;
        public static MenuStateController QMStateController { get; private set; }

        private static ModalAlert quickMenuAlert;

        private static MethodInfo modalAlertMethod;
        private static MethodInfo infoPopupMethod;
        private static MethodInfo popupMethod;
        private static MethodInfo openSubMenuMethod;
        private static MethodInfo popSubMenuMethod;

        public static void UiInit()
        {
            TempUIParent = new GameObject("ScreenshotManagerTempUIParent").transform;
            Object.DontDestroyOnLoad(TempUIParent.gameObject);

            GameObject quickMenuObject = GameObject.Find("UserInterface").transform.Find("Canvas_QuickMenu(Clone)").gameObject;
            QMStateController = quickMenuObject.GetComponent<MenuStateController>();
            QuickMenu = quickMenuObject.GetComponent<VRCQuickMenu>();
            quickMenuAlert = quickMenuObject.transform.Find("Container/Window/QMParent/Modal_Alert").GetComponent<ModalAlert>();

            modalAlertMethod = MethodUtils.FindMethod("ModalAlert", () => typeof(ModalAlert).GetMethods().First(method => method.Name.StartsWith("Method_Public_Void_String_") && method.GetParameters().Length == 1 && MethodUtils.IsUsingMethod(method, "Method_Private_Void_") && !MethodUtils.IsUsingMethod(method, "Method_Private_Void_PDM")));
            infoPopupMethod = MethodUtils.FindMethod("InfoPopup", () => typeof(VRCQuickMenu).GetMethods().First(method => method.Name.StartsWith("Method_Public_Void_String_String_Action_") && method.GetParameters().Length == 5 && MethodUtils.ContainsString(method, "ConfirmDialog")));
            popupMethod = MethodUtils.FindMethod("Popup", () => typeof(VRCQuickMenu).GetMethods().First(method => method.Name.StartsWith("Method_Public_Void_String_String_String_String_Action_Action_") && method.GetParameters().Length == 6 && MethodUtils.ContainsString(method, "ConfirmDialog")));
            openSubMenuMethod = MethodUtils.FindMethod("Open SubMenu", () => typeof(UIPage).GetMethods().First(method => method.Name.StartsWith("Method_Public_Void_UIPage_TransitionType_") && !method.Name.Contains("PDM") && method.GetParameters().Length == 2 && MethodUtils.IsUsingMethod(method, "Add") && MethodUtils.IsUsedByType(method, typeof(MenuStateController))));
            popSubMenuMethod = MethodUtils.FindMethod("Pop SubMenu", () => typeof(UIPage).GetMethods().First(method => method.Name.StartsWith("Method_Public_Void_") && method.GetParameters().Length == 0 && MethodUtils.IsUsingMethod(method, "Method_Public_Void_Predicate_1_UIPage_0") && !MethodUtils.IsUsingMethod(method, "ThrowArgumentOutOfRangeException") && MethodUtils.IsUsingMethod(method, "op_Equality")));
#if DEBUG
            ScreenshotManagerMod.Logger.Msg("ModalAlert Method: " + modalAlertMethod?.Name);
            ScreenshotManagerMod.Logger.Msg("InfoPopup Method: " + infoPopupMethod?.Name);
            ScreenshotManagerMod.Logger.Msg("Popup Method: " + popupMethod?.Name);
            ScreenshotManagerMod.Logger.Msg("Open SubMenu Method: " + openSubMenuMethod?.Name);
            ScreenshotManagerMod.Logger.Msg("Pop SubMenu Method: " + popSubMenuMethod?.Name);
#endif
        }

        public static void PushQuickMenuAlert(string text) => modalAlertMethod?.Invoke(quickMenuAlert, new object[] { text });

        public static void ShowQuickMenuInformationPopup(string title, string body, Action action, string okLabel) => infoPopupMethod?.Invoke(QuickMenu, new object[] { title, body, (Il2CppSystem.Action)action, okLabel, false });

        public static void ShowQuickMenuPopup(string title, string body, string yesLabel, string noLabel, Action yesAction, Action noAction) => popupMethod?.Invoke(QuickMenu, new object[] { title, body, yesLabel, noLabel, (Il2CppSystem.Action)yesAction, (Il2CppSystem.Action)noAction });

        public static void OpenUIPageSubMenu(UIPage uiPage, SubMenu subMenu) => openSubMenuMethod?.Invoke(uiPage, new object[] { subMenu.UiPage, UIPage.TransitionType.Right });

        public static void PopUIPageSubMenu(UIPage uiPage) => popSubMenuMethod?.Invoke(uiPage, null);
    }
}
