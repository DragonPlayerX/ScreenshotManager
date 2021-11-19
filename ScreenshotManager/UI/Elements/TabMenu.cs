using UnhollowerBaseLib;
using VRC.UI.Elements;

namespace ScreenshotManager.UI.Elements
{
    public class TabMenu : SubMenu
    {

        public TabMenu(string name, string pageName, string pageTitle) : base(name, pageName, pageTitle)
        {
            Il2CppReferenceArray<UIPage> rootPages = UiManager.QMStateController.field_Public_ArrayOf_UIPage_0;
            Il2CppReferenceArray<UIPage> newRootPages = new Il2CppReferenceArray<UIPage>(rootPages.Count + 1);
            for (int i = 0; i < rootPages.Count; i++)
                newRootPages[i] = rootPages[i];
            newRootPages[rootPages.Count] = UiPage;
            UiManager.QMStateController.field_Public_ArrayOf_UIPage_0 = newRootPages;
        }

        public void OpenSubMenu(SubMenu subMenu) => UiPage.Method_Public_Void_UIPage_1(subMenu.UiPage);

        public void CloseAllSubMenus() => UiPage.Method_Public_Void_Predicate_1_UIPage_0(null);

        public void PopSubMenu() => UiPage.Method_Public_Void_PDM_3();

    }
}
