using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnhollowerRuntimeLib;
using VRChatUtilityKit.Components;
using VRChatUtilityKit.Utilities;
using VRChatUtilityKit.Ui;
using UIExpansionKit.API;

using Object = UnityEngine.Object;

using ScreenshotManager.UI;
using ScreenshotManager.Config;

namespace ScreenshotManager.Core
{
    public static class MenuManager
    {

        private static TabButton tabButton;
        private static SingleButton menuButton;
        private static GameObject uixButton;

        public static List<SubMenu> menus = new List<SubMenu>();

        public static GameObject menuUI;
        public static RectTransform menuRect;
        public static GameObject singleViewObject;
        public static GameObject multiViewObject;

        private static bool imageScaled = false;

        public static void PrepareAssets()
        {
            // Asset loading taken from UIExpansionKit by knah
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScreenshotManager.screenshotmanager.assetbundle");
            MemoryStream memoryStream = new MemoryStream((int)stream.Length);

            stream.CopyTo(memoryStream);

            AssetBundle assetBundle = AssetBundle.LoadFromMemory_Internal(memoryStream.ToArray(), 0);
            assetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            menuUI = Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/ScreenshotManagerUI.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), QuickMenu.prop_QuickMenu_0.gameObject.transform);

            menuRect = menuUI.GetComponent<RectTransform>();
            menuRect.anchoredPosition = Converters.ConvertToUnityUnits(new Vector3(5.522f, 1.5f));

            menuRect.Find("Version/Text").GetComponent<Text>().text = "Version\n" + ScreenshotManagerMod.Version;

            singleViewObject = menuRect.Find("SingleViewImage").gameObject;
            singleViewObject.SetActive(!Configuration.MultiViewEntry.Value);
            multiViewObject = menuRect.Find("MultiView").gameObject;
            multiViewObject.SetActive(Configuration.MultiViewEntry.Value);

            menuUI.SetActive(false);
        }

        public static void CreateMenus()
        {
            menus.Add(new SubMenu("UserInterface/QuickMenu", "ScreenshotManagerMenuMain"));
            menus.Add(new SubMenu("UserInterface/QuickMenu", "ScreenshotManagerMenuOrganization"));
            menus.Add(new SubMenu("UserInterface/QuickMenu", "ScreenshotManagerMenuSettings"));

            tabButton = new TabButton(menuRect.Find("Version/Image").GetComponent<Image>().sprite, menus[0]);
            tabButton.ButtonComponent.onClick.AddListener(new Action(() => tabButton.OpenTabMenu()));
            tabButton.gameObject.SetActive(Configuration.TabButtonEntry.Value);

            bool desktopCameraFound = MelonHandler.Mods.Any(mod => mod.Info.Name == "DesktopCamera");

            menuButton = new SingleButton("UserInterface/QuickMenu/CameraMenu", desktopCameraFound ? new Vector3(5, 4) : new Vector3(4, 2), "Screenshot Manager", new Action(() => tabButton.OpenTabMenu()), "Open Screenshot Manager", "ScreenshotManagerButton", true, Color.yellow);
            menuButton.gameObject.SetActive(!Configuration.TabButtonEntry.Value);

            if (MelonHandler.Mods.Any(mod => mod.Info.Name == "UI Expansion Kit"))
            {
                ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("Screenshot Manager", new Action(() => menus[0].OpenSubMenu()), new Action<GameObject>(gameObject =>
                {
                    uixButton = gameObject;
                    gameObject.SetActive(Configuration.UseUIXEntry.Value && !Configuration.TabButtonEntry.Value);
                }));
                Configuration.UseUIXEntry.OnValueChanged += new Action<bool, bool>((oldValue, newValue) =>
                {
                    if (!Configuration.TabButtonEntry.Value)
                    {
                        uixButton.SetActive(newValue);
                        menuButton.gameObject.SetActive(!newValue);
                    }
                });
            }

            // Fix for emmVRC UI color change

            Color uiColor = menuButton.gameObject.GetComponent<Button>().colors.normalColor;

            menuRect.Find("Background").GetComponent<Image>().color = new Color(uiColor.r, uiColor.g, uiColor.b, 0.1f);
            menuRect.Find("Background (1)").GetComponent<Image>().color = new Color(uiColor.r, uiColor.g, uiColor.b, 0.1f);

            // Main menu

            List<ElementBase> buttons = new List<ElementBase>();

            buttons.Add(new SingleButton(menus[0].Path, new Vector3(5, 2), "Back", new Action(() => UiManager.OpenSubMenu("UserInterface/QuickMenu/ShortcutMenu")), "Press to go back", "BackButton", true, Color.yellow));
            buttons.Add(new SingleButton(menus[0].Path, new Vector3(5, 0), "Settings", new Action(() => menus[2].OpenSubMenu()), "Open Settings", "SettingsMenuButton", true));

            ToggleButton viewButton = new ToggleButton(menus[0].Path, new Vector3(5, 1), "Multi View", "Single View", new Action<bool>(state =>
            {
                Configuration.MultiViewEntry.Value = state;
            }), "Change Image View", "Change Image View", "ViewButton", Configuration.MultiViewEntry.Value, true);
            Configuration.MultiViewEntry.OnValueChanged += new Action<bool, bool>((oldValue, newValue) =>
            {
                viewButton.State = newValue;
                singleViewObject.SetActive(!newValue);
                multiViewObject.SetActive(newValue);
                if (newValue)
                    ImageHandler.SetMultiView();
                else
                    ImageHandler.Update(true);
            });

            buttons.Add(viewButton);

            ToggleButton discordWebhookToggleButton = new ToggleButton(menus[0].Path, new Vector3(0, 1), "Discord Webhook", "Disabled", new Action<bool>(state =>
            {
                Configuration.DiscordWebhookEntry.Value = state;
            }), "Enable/Disable Discord Webhook", "Enable/Disable Discord Webhook", "DiscordWebhookButton", Configuration.DiscordWebhookEntry.Value, true);
            Configuration.DiscordWebhookEntry.OnValueChanged += new Action<bool, bool>((oldValue, newValue) =>
            {
                discordWebhookToggleButton.State = newValue;
            });

            buttons.Add(discordWebhookToggleButton);

            buttons.Add(new SingleButton(menus[0].Path, new Vector3(0, 0), "File Organization", new Action(() => menus[1].OpenSubMenu()), "Open File Organization Menu", "OrganizationMenuButton", true));

            HalfButton favoriteButton = new HalfButton(menus[0].Path, new Vector3(2, 2), new Vector3(0.5f, 0), "Favorite", new Action(ImageHandler.MovePicture), "Favorite", "FavoriteButton", true);

            buttons.Add(new HalfButton(menus[0].Path, new Vector3(1, -1), new Vector3(0.5f, 1), "All", new Action(() =>
            {
                ImageHandler.ChangeCategory(ImageHandler.ImageMenuCategory.ALL);
                favoriteButton.TextComponent.text = "Favorite";
                favoriteButton.TextComponent.color = Color.white;
            }), "All", "AllButton", true));

            buttons.Add(new HalfButton(menus[0].Path, new Vector3(2, -1), new Vector3(0.5f, 1), "Today", new Action(() =>
            {
                ImageHandler.ChangeCategory(ImageHandler.ImageMenuCategory.TODAY);
                favoriteButton.TextComponent.text = "Favorite";
                favoriteButton.TextComponent.color = Color.white;
            }), "Today", "TodayButton", true));

            buttons.Add(new HalfButton(menus[0].Path, new Vector3(3, -1), new Vector3(0.5f, 1), "Favorites", new Action(() =>
            {
                ImageHandler.ChangeCategory(ImageHandler.ImageMenuCategory.FAVORITES);
                favoriteButton.TextComponent.text = "Unfavorite";
                favoriteButton.TextComponent.color = Color.yellow;
            }
            ), "Favorites", "FavoritesButton", true));

            buttons.Add(new HalfButton(menus[0].Path, new Vector3(4, -1), new Vector3(0.5f, 1), "Reload", new Action(ImageHandler.Reload), "Reload", "ReloadButton", true, Color.yellow));

            SingleButton deleteConfirmButton = null;
            SingleButton deleteCancelButton = null;

            deleteConfirmButton = new SingleButton(menus[0].Path, new Vector3(1, 2), "Delete", new Action(() =>
            {
                ImageHandler.DeletePicture();
                buttons.ForEach(button => button.gameObject.SetActive(true));
                deleteConfirmButton.gameObject.SetActive(false);
                deleteCancelButton.gameObject.SetActive(false);
            }), "Deletes an Image", "DeleteConfirmButton", true, Color.red);

            deleteCancelButton = new SingleButton(menus[0].Path, new Vector3(4, 2), "Cancel", new Action(() =>
            {
                buttons.ForEach(button => button.gameObject.SetActive(true));
                deleteConfirmButton.gameObject.SetActive(false);
                deleteCancelButton.gameObject.SetActive(false);
                ImageHandler.Update(false);
            }), "Cancel", "DeleteCancelButton", true);

            deleteConfirmButton.gameObject.SetActive(false);
            deleteCancelButton.gameObject.SetActive(false);

            buttons.Add(new SingleButton(menus[0].Path, new Vector3(1, 2), "Previous", new Action(ImageHandler.Previous), "Previous", "PreviousButton", true));
            buttons.Add(favoriteButton);
            buttons.Add(new HalfButton(menus[0].Path, new Vector3(2, 2), new Vector3(0.5f, 1), "Delete", new Action(() =>
            {
                if (!ImageHandler.isReloading)
                {
                    buttons.ForEach(button => button.gameObject.SetActive(false));
                    if (viewButton.State)
                    {
                        viewButton.State = false;
                        viewButton.OnClick.Invoke(false);
                    }
                    deleteConfirmButton.gameObject.SetActive(true);
                    deleteCancelButton.gameObject.SetActive(true);
                    ImageHandler.titleText.text = "Please confirm:";
                    ImageHandler.infoText.text = "You are going to delete this image";
                }
            }), "Delete", "DeleteButton", true, Color.red));

            buttons.Add(new HalfButton(menus[0].Path, new Vector3(3, 2), new Vector3(0.5f, 0), "Show File", new Action(ImageHandler.ShowFileInExplorer), "Show File", "ExplorerButton", true));
            buttons.Add(new HalfButton(menus[0].Path, new Vector3(3, 2), new Vector3(0.5f, 1), "Share", new Action(() =>
            {
                if (Configuration.DiscordWebhookEntry.Value)
                {
                    if (Configuration.DiscordWebhookURLEntry.Value.ToLower().StartsWith("https://discordapp.com/api/webhooks/") || Configuration.DiscordWebhookURLEntry.Value.ToLower().StartsWith("https://discord.com/api/webhooks/"))
                        ImageHandler.SendToDiscordWebhook();
                    else
                        UiManager.OpenSmallPopup("Action required", "No Discord Webhook URL found!", "Ok", new Action(UiManager.ClosePopup));
                }
                else
                {
                    UiManager.OpenSmallPopup("Action required", "Please enable Discord Webhook to use this function.", "Ok", new Action(UiManager.ClosePopup));
                }

            }), "Share", "ShareButton", true));
            buttons.Add(new SingleButton(menus[0].Path, new Vector3(4, 2), "Next", new Action(ImageHandler.Next), "Next", "NextButton", true));

            Animator animator = singleViewObject.GetComponent<Animator>();

            singleViewObject.GetComponent<Button>().onClick.AddListener(new Action(() =>
            {
                if (imageScaled)
                {
                    animator.SetBool("Size Trigger", false);
                    buttons.ForEach(button => button.gameObject.SetActive(true));
                    imageScaled = false;
                }
                else if (!deleteConfirmButton.gameObject.activeSelf)
                {
                    animator.SetBool("Size Trigger", true);
                    buttons.ForEach(button => button.gameObject.SetActive(false));
                    imageScaled = true;
                }
            }));

            GameObject newElements = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements");

            EnableDisableListener menuListener = menus[0].gameObject.AddComponent<EnableDisableListener>();

            menuListener.OnEnableEvent += new Action(() =>
            {
                menuUI.SetActive(true);
                newElements.SetActive(false);
            });

            menuListener.OnDisableEvent += new Action(() =>
            {
                animator.Rebind();
                animator.Update(0);
                animator.SetBool("Size Trigger", false);
                imageScaled = false;
                deleteConfirmButton.gameObject.SetActive(false);
                deleteCancelButton.gameObject.SetActive(false);
                buttons.ForEach(button => button.gameObject.SetActive(true));
                ImageHandler.Update(false);
                menuUI.SetActive(false);
                newElements.SetActive(true);
            });

            // Organization menu

            new SingleButton(menus[1].Path, new Vector3(4, 2), "Back", new Action(() => menus[0].OpenSubMenu()), "Press to go back", "BackButton", true, Color.yellow);

            ToggleButton organizationToggleButton = new ToggleButton(menus[1].Path, new Vector3(1, 0), "File Organization", "Disabled", new Action<bool>(state =>
            {
                Configuration.FileOrganizationEntry.Value = state;
            }), "Toggle File Organization", "Toggle File Organization", "FileOrganizationButton", Configuration.FileOrganizationEntry.Value, true);
            Configuration.FileOrganizationEntry.OnValueChanged += new Action<bool, bool>((oldValue, newValue) =>
            {
                organizationToggleButton.State = newValue;
            });

            new SingleButton(menus[1].Path, new Vector3(2, 0), "Manually Organize", new Action(() =>
            {
                if (Configuration.FileOrganizationEntry.Value)
                    FileOrganization.OrganizeAll();
                else
                    UiManager.OpenSmallPopup("Action required", "Please enable File Organisation.", "Ok", new Action(UiManager.ClosePopup));
            }), "Organize all pictures (can cause lag)", "ManuallyFileOrganizationButton", true);

            new SingleButton(menus[1].Path, new Vector3(3, 0), "Reset Organization", new Action(() => FileOrganization.Reset()), "Reset Organization (can cause lag)", "ResetOrganizationButton", true, Color.red);

            EnableDisableListener organizeMenuListener = menus[1].gameObject.AddComponent<EnableDisableListener>();

            organizeMenuListener.OnEnableEvent += new Action(() =>
            {
                menuUI.SetActive(false);
                newElements.SetActive(true);
            });

            // Settings menu

            new SingleButton(menus[2].Path, new Vector3(4, 2), "Back", new Action(() => menus[0].OpenSubMenu()), "Press to go back", "BackButton", true, Color.yellow);

            ToggleButton menuOptionToggleButton = new ToggleButton(menus[2].Path, new Vector3(1, 0), "Tab Button", "Menu Button", new Action<bool>(state =>
            {
                Configuration.TabButtonEntry.Value = state;
            }), "Change Button Type", "Change Button Type", "ButtonTypeButton", Configuration.TabButtonEntry.Value, true);
            Configuration.TabButtonEntry.OnValueChanged += new Action<bool, bool>((oldValue, newValue) =>
            {
                menuOptionToggleButton.State = newValue;
                tabButton.gameObject.SetActive(newValue);
                if (uixButton != null)
                {
                    uixButton.SetActive(!newValue && Configuration.UseUIXEntry.Value);
                    menuButton.gameObject.SetActive(!newValue && !Configuration.UseUIXEntry.Value);
                }
                else
                {
                    menuButton.gameObject.SetActive(!newValue);
                }
            });

            ToggleButton useUIXToggleButton = new ToggleButton(menus[2].Path, new Vector3(2, 0), "Use UIX", "Disabled", new Action<bool>(state =>
            {
                Configuration.UseUIXEntry.Value = state;
            }), "Use UIX as Menu Button", "Use UIX as Menu Button", "UseUIXButton", Configuration.UseUIXEntry.Value, true);
            Configuration.UseUIXEntry.OnValueChanged += new Action<bool, bool>((oldValue, newValue) =>
            {
                useUIXToggleButton.State = newValue;
            });

            EnableDisableListener optionMenuListener = menus[2].gameObject.AddComponent<EnableDisableListener>();

            optionMenuListener.OnEnableEvent += new Action(() =>
            {
                menuUI.SetActive(false);
                newElements.SetActive(true);
            });

            UiManager.OnQuickMenuClosed += new Action(() => Configuration.Save());
        }
    }
}
