using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnhollowerRuntimeLib;
using VRC.Core;
using VRC.UI.Core.Styles;

using Object = UnityEngine.Object;

using ScreenshotManager.UI;
using ScreenshotManager.UI.Elements;
using ScreenshotManager.UI.Components;
using ScreenshotManager.Config;
using ScreenshotManager.Tasks;
using ScreenshotManager.Utils;
using System.Threading.Tasks;

namespace ScreenshotManager.Core
{
    public static class MenuManager
    {

        public static TabButton TabButton;

        public static ButtonHeader InfoButtonHeader;
        public static ButtonHeader ActionsMenuInfoButtonHeader;

        public static GameObject MainImageContainer;
        public static RectTransform MainImageContainerRect;
        public static GameObject SecondaryImageContainer;
        public static RectTransform SecondaryImageContainerRect;

        public static Text ImageCreationTimeText;
        public static Text ImageSizeText;
        public static Text ImageWorldNameText;

        public static GameObject SingleImageContainer;
        public static GameObject MultiImageContainer;
        public static Texture2D ErrorTexture;

        public static SingleButton FavoriteButton;

        private static SubMenu webhookSubMenu;

        private static Queue<Action> styleQueue = new Queue<Action>();

        public static Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>()
        {
            { "Gallery", null },
            { "Grid", null },
            { "Category", null },
            { "Actions", null },
            { "Settings", null },
            { "Organization", null },
            { "Manually", null },
            { "Blocked", null },
            { "Reload", null },
            { "GitHub", null },
            { "Share", null },
            { "Trash", null },
            { "Star", null },
            { "Tab", null },
            { "File", null },
            { "Upload", null },
            { "RotateRight", null },
            { "RotateLeft", null },
            { "Steam", null },
            { "Data", null },
        };

        public static void Init()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScreenshotManager.Resources.screenshotmanager.assetbundle");
            MemoryStream memoryStream = new MemoryStream((int)stream.Length);

            stream.CopyTo(memoryStream);

            AssetBundle assetBundle = AssetBundle.LoadFromMemory_Internal(memoryStream.ToArray(), 0);
            assetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            MainImageContainer = Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/MainImage_Container.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), UiManager.QMStateController.transform.Find("Container/Window/QMParent").transform);
            SecondaryImageContainer = Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/SecondaryImage_Container.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), UiManager.QMStateController.transform.Find("Container/Window/QMParent").transform);
            ErrorTexture = assetBundle.LoadAsset("Assets/Sprites/ErrorTexture.png", Il2CppType.Of<Sprite>()).Cast<Sprite>().texture;
            ErrorTexture.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            Dictionary<string, Sprite> newSpriteDict = new Dictionary<string, Sprite>();
            foreach (string key in Sprites.Keys)
            {
                Sprite sprite = assetBundle.LoadAsset("Assets/Sprites/" + key + "_Icon.png", Il2CppType.Of<Sprite>()).Cast<Sprite>();
                sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                newSpriteDict.Add(key, sprite);
            }

            Sprites = newSpriteDict;
            Sprites.Add("X", UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Settings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UI_Elements_Row_1/Button_ToggleQMInfo/Icon_Off").GetComponent<Image>().sprite);

            MainImageContainerRect = MainImageContainer.GetComponent<RectTransform>();
            SecondaryImageContainerRect = SecondaryImageContainer.GetComponent<RectTransform>();

            SingleImageContainer = MainImageContainerRect.Find("SingleImage_Container").gameObject;
            SingleImageContainer.SetActive(!Configuration.MultiView.Value);
            MultiImageContainer = MainImageContainerRect.Find("MultiImage_Container").gameObject;
            MultiImageContainer.SetActive(Configuration.MultiView.Value);

            MainImageContainer.SetLayerRecursive(12);
            SecondaryImageContainer.SetLayerRecursive(12);

            MainImageContainer.SetActive(true);
            SecondaryImageContainer.SetActive(true);

            TabButton = new TabButton(Sprites["Gallery"], "Screenshot Manager", "ScreenshotManager_Main", "Screenshot Manager", "Page_ScreenshotManager");
            TabButton.SubMenu.GameObject.transform.Find("ScrollRect/Viewport").GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
            TabButton.SubMenu.GameObject.transform.Find("ScrollRect/Viewport").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            TabButton.GameObject.SetActive(Configuration.TabButton.Value);
            TabButton.OnClick += new Action(() =>
            {
                for (int i = 0; i < styleQueue.Count; i++)
                    styleQueue.Dequeue().Invoke();

                if (Configuration.AutoSelectLatest.Value)
                    ImageHandler.SelectLatest();
            });

            SingleButton menuButton = new SingleButton(new Action(() => TabButton.MenuTab.ShowTabContent()), Sprites["Gallery"], "Screenshot Manager", "Open Screenshot Manager", "Button_ScreenshotManager");
            menuButton.RectTransform.parent = UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Camera/Scrollrect/Viewport/VerticalLayoutGroup/Buttons");
            menuButton.GameObject.SetActive(!Configuration.TabButton.Value);
            menuButton.OnClick += new Action(() =>
            {
                for (int i = 0; i < styleQueue.Count; i++)
                    styleQueue.Dequeue().Invoke();

                if (Configuration.AutoSelectLatest.Value)
                    ImageHandler.SelectLatest();
            });

            MainImageContainerRect.parent = TabButton.SubMenu.GameObject.transform.Find("ScrollRect/Viewport/VerticalLayoutGroup");
            MainImageContainerRect.sizeDelta = new Vector2(1024, 1152 / 2);

            // Styles & Build-in stuff

            //Color colorLightBlue = new Color(0.4157f, 0.8902f, 0.9765f);
            //Color colorDarkGreen = new Color(0.0275f, 0.1843f, 0.1882f);
            //Color colorLightGreen = new Color(0.298f, 0.5569f, 0.5843f);

            StyleElement singleImageStyle = SingleImageContainer.transform.Find("SingleImage_Mask/Image").gameObject.AddComponent<StyleElement>();
            singleImageStyle.field_Public_String_1 = "Rect";

            for (int i = 1; i <= 9; i++)
                MultiImageContainer.transform.Find("MultiImage_Mask_" + i + "/Image").gameObject.AddComponent<StyleElement>().field_Public_String_1 = "Rect";

            Transform controlLeft = MainImageContainerRect.Find("Control_Left");
            controlLeft.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, 0, -5);

            Button controlLeftNavigateButton = controlLeft.Find("Button_Mask/Button_Navigate").GetComponent<Button>();
            controlLeftNavigateButton.gameObject.AddComponent<StyleElement>().field_Public_String_1 = "ButtonIcon";
            controlLeftNavigateButton.onClick.AddListener(new Action(() => ImageHandler.Previous()));

            Button controlLeftRotateButton = controlLeft.Find("Button_Mask/Button_Rotate").GetComponent<Button>();
            controlLeftRotateButton.gameObject.AddComponent<StyleElement>().field_Public_String_1 = "ButtonIcon";
            controlLeftRotateButton.onClick.AddListener(new Action(() => ImageHandler.RotateImage(ImageHandler.Direction.Left)));

            Transform controlRight = MainImageContainerRect.Find("Control_Right");
            controlRight.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, 0, -5);

            Button controlRightNavigateButton = controlRight.Find("Button_Mask/Button_Navigate").GetComponent<Button>();
            controlRightNavigateButton.gameObject.AddComponent<StyleElement>().field_Public_String_1 = "ButtonIcon";
            controlRightNavigateButton.onClick.AddListener(new Action(() => ImageHandler.Next()));

            Button controlRightRotateButton = controlRight.Find("Button_Mask/Button_Rotate").GetComponent<Button>();
            controlRightRotateButton.gameObject.AddComponent<StyleElement>().field_Public_String_1 = "ButtonIcon";
            controlRightRotateButton.onClick.AddListener(new Action(() => ImageHandler.RotateImage(ImageHandler.Direction.Right)));

            controlLeftRotateButton.gameObject.SetActive(Configuration.ShowRotationButtons.Value);
            controlRightRotateButton.gameObject.SetActive(Configuration.ShowRotationButtons.Value);

            Transform textContainer = SecondaryImageContainerRect.Find("TextContainer");

            Text imageCreationTimeHeader = textContainer.Find("CreationTime/Text_Header").GetComponent<Text>();
            ImageCreationTimeText = textContainer.Find("CreationTime/Text_Content").GetComponent<Text>();

            Text imageSizeHeader = textContainer.Find("FileSize/Text_Header").GetComponent<Text>();
            ImageSizeText = textContainer.Find("FileSize/Text_Content").GetComponent<Text>();

            Text imageWorldNameHeader = textContainer.Find("WorldName/Text_Header").GetComponent<Text>();
            ImageWorldNameText = textContainer.Find("WorldName/Text_Content").GetComponent<Text>();

            GameObject tempObject1 = new GameObject();
            GameObject tempObject2 = new GameObject();
            GameObject tempObject3 = new GameObject();

            tempObject1.transform.parent = TabButton.SubMenu.RectTransform;
            tempObject2.transform.parent = TabButton.SubMenu.RectTransform;
            tempObject3.transform.parent = TabButton.SubMenu.RectTransform;

            TextMeshProUGUI text1 = tempObject1.AddComponent<TextMeshProUGUI>();
            TextMeshProUGUI text2 = tempObject2.AddComponent<TextMeshProUGUI>();
            TextMeshProUGUI text3 = tempObject3.AddComponent<TextMeshProUGUI>();

            StyleElement styleElement1 = tempObject1.AddComponent<StyleElement>();
            styleElement1.field_Public_String_1 = "H3";
            StyleElement styleElement2 = tempObject2.AddComponent<StyleElement>();
            styleElement2.field_Public_String_1 = "H4";
            StyleElement styleElement3 = tempObject3.AddComponent<StyleElement>();
            styleElement3.field_Public_String_1 = "BackgroundPanel";

            styleQueue.Enqueue(new Action(() =>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(50);

                    Color colorLightBlue = text1.color;
                    Color colorLightGreen = text2.color;
                    Color colorDarkGreen = text3.color;

                    await TaskProvider.YieldToMainThread();

                    SecondaryImageContainerRect.Find("Background").GetComponent<Image>().color = colorDarkGreen;
                    controlLeftNavigateButton.colors = new ColorBlock() { colorMultiplier = 1, normalColor = colorLightBlue, highlightedColor = colorLightBlue, pressedColor = colorLightBlue, selectedColor = colorLightBlue, disabledColor = colorLightBlue };
                    controlLeftRotateButton.colors = new ColorBlock() { colorMultiplier = 1, normalColor = colorLightBlue, highlightedColor = colorLightBlue, pressedColor = colorLightBlue, selectedColor = colorLightBlue, disabledColor = colorLightBlue };

                    controlRightNavigateButton.colors = new ColorBlock() { colorMultiplier = 1, normalColor = colorLightBlue, highlightedColor = colorLightBlue, pressedColor = colorLightBlue, selectedColor = colorLightBlue, disabledColor = colorLightBlue };

                    controlRightRotateButton.colors = new ColorBlock() { colorMultiplier = 1, normalColor = colorLightBlue, highlightedColor = colorLightBlue, pressedColor = colorLightBlue, selectedColor = colorLightBlue, disabledColor = colorLightBlue };

                    imageCreationTimeHeader.color = colorLightGreen;
                    ImageCreationTimeText.color = colorLightBlue;
                    imageSizeHeader.color = colorLightGreen;
                    ImageSizeText.color = colorLightBlue;

                    imageWorldNameHeader.color = colorLightGreen;
                    ImageWorldNameText.color = colorLightBlue;

                    Object.Destroy(tempObject1);
                    Object.Destroy(tempObject2);
                    Object.Destroy(tempObject3);
                }).NoAwait();
            }));

            // Menus

            InfoButtonHeader = new ButtonHeader(TabButton.SubMenu.RectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup"), "Title", "Title_Header", "Info");
            InfoButtonHeader.RectTransform.SetSiblingIndex(0);
            InfoButtonHeader.Minimize();

            SubMenu categoriesSubMenu = new SubMenu("ScreenshotManagerCategories", "Menu_ScreenshotManager_Categories", "Select Category", true);
            SubMenu actionsSubMenu = new SubMenu("ScreenshotManagerActions", "Menu_ScreenshotManager_Actions", "Photo Actions", true);
            SubMenu settingsSubMenu = new SubMenu("ScreenshotManagerSettings", "Menu_ScreenshotManager_Settings", "Settings", true);
            webhookSubMenu = new SubMenu("ScreenshotManagerWebhooks", "Menu_ScreenshotManager_Webhooks", "Select Webhook", true, true);

            actionsSubMenu.GameObject.transform.Find("ScrollRect/Viewport").GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
            actionsSubMenu.GameObject.transform.Find("ScrollRect/Viewport").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            SecondaryImageContainerRect.parent = actionsSubMenu.GameObject.transform.Find("ScrollRect/Viewport/VerticalLayoutGroup");
            SecondaryImageContainerRect.sizeDelta = new Vector2(1024, 1152 / 4);

            // Main menu buttons

            ButtonGroup mainMenuButtonGroup = new ButtonGroup("Actions");

            ToggleButton viewButton = new ToggleButton(new Action<bool>(state => Configuration.MultiView.Value = state), Sprites["Grid"], Sprites["Gallery"], "Multi View", "Single View", "Switch to single image view mode", "Switch to multi image view mode", "Button_View", Configuration.MultiView.Value);
            Configuration.MultiView.OnValueChanged += new Action<bool, bool>((oldValue, newValue) =>
            {
                viewButton.State = newValue;
                SingleImageContainer.SetActive(!newValue);
                MultiImageContainer.SetActive(newValue);

                controlLeftRotateButton.gameObject.SetActive(!newValue && Configuration.ShowRotationButtons.Value);
                controlRightRotateButton.gameObject.SetActive(!newValue && Configuration.ShowRotationButtons.Value);

                if (newValue)
                    ImageHandler.SetMultiView();
                else
                    ImageHandler.Update(true);
            });

            SingleButton categoriesButton = new SingleButton(new Action(() => TabButton.SubMenu.OpenSubMenu(categoriesSubMenu)), Sprites["Category"], "Categories", "Select a view category", "Button_Categories");

            SingleButton actionsButton = new SingleButton(new Action(() =>
            {
                if (ImageHandler.IsReloading)
                    return;

                bool result = ImageHandler.UpdateCurrentSelectedFile();
                if (result)
                {
                    TabButton.SubMenu.OpenSubMenu(actionsSubMenu);
                    ImageHandler.FetchSecondaryImage();

                    if (ImageHandler.IsCurrentFavorite())
                    {
                        FavoriteButton.Sprite = Sprites["Star"];
                        FavoriteButton.Text = "Unfavorite";
                        FavoriteButton.TooltipText = "Unfavorite this image";
                    }
                    else
                    {
                        FavoriteButton.Sprite = Sprites["X"];
                        FavoriteButton.Text = "Favorite";
                        FavoriteButton.TooltipText = "Favorite this image";
                    }
                }
                else
                {
                    UiManager.ShowQuickMenuInformationPopup("Warning", "Your selection is empty.", new Action(() => { }));
                }
            }), Sprites["Actions"], "Actions", "Manage the current image", "Button_Actions");

            SingleButton settingsButton = new SingleButton(new Action(() => TabButton.SubMenu.OpenSubMenu(settingsSubMenu)), Sprites["Settings"], "Settings", "Manage settings", "Button_Settings");

            mainMenuButtonGroup.AddButton(viewButton);
            mainMenuButtonGroup.AddButton(categoriesButton);
            mainMenuButtonGroup.AddButton(actionsButton);
            mainMenuButtonGroup.AddButton(settingsButton);
            TabButton.SubMenu.AddButtonGroup(mainMenuButtonGroup);

            new ButtonHeader(TabButton.SubMenu.RectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup"), "Version " + ScreenshotManagerMod.Version, "Version_Header");

            HeaderButton headerButton = new HeaderButton(new Action(() => ImageHandler.ReloadFiles().NoAwait()), Sprites["Reload"], "Reload all images", "Button_ReloadImages");
            TabButton.SubMenu.AddHeaderButton(headerButton);
            headerButton.GameObject.transform.localPosition = new Vector3(0, 0, -1);

            // Category menu buttons

            ButtonGroup categoryMenuButtonGroup = new ButtonGroup("Categories");
            categoryMenuButtonGroup.ButtonLayoutGroup.constraintCount = 1;
            categoryMenuButtonGroup.ButtonLayoutGroup.cellSize = new Vector2(900, 100);

            WideButton categoryAllImagesButton = new WideButton(new Action(() =>
            {
                TabButton.SubMenu.PopSubMenu();
                ImageHandler.ChangeCategory(ImageHandler.ImageMenuCategory.All);
            }), "All Images", "Show all images", "Button_CategoryAll");

            WideButton categoryTodaysImagesButton = new WideButton(new Action(() =>
            {
                TabButton.SubMenu.PopSubMenu();
                ImageHandler.ChangeCategory(ImageHandler.ImageMenuCategory.Today);
            }), "Today's Images", "Show all images from today", "Button_CategoryTodays");

            WideButton categoryYesterdaysImagesButton = new WideButton(new Action(() =>
            {
                TabButton.SubMenu.PopSubMenu();
                ImageHandler.ChangeCategory(ImageHandler.ImageMenuCategory.Yesterday);
            }), "Yesterday's Images", "Show all images from the last day", "Button_CategoryYesterdays");

            WideButton categoryFavoriteImagesButton = new WideButton(new Action(() =>
            {
                TabButton.SubMenu.PopSubMenu();
                ImageHandler.ChangeCategory(ImageHandler.ImageMenuCategory.Favorites);
            }), "Favorite Images", "Show all favorite images", "Button_CategoryFavorite");

            categoryMenuButtonGroup.AddButton(categoryAllImagesButton);
            categoryMenuButtonGroup.AddButton(categoryTodaysImagesButton);
            categoryMenuButtonGroup.AddButton(categoryYesterdaysImagesButton);
            categoryMenuButtonGroup.AddButton(categoryFavoriteImagesButton);
            categoriesSubMenu.AddButtonGroup(categoryMenuButtonGroup);

            // Actions menu buttons

            ActionsMenuInfoButtonHeader = new ButtonHeader(actionsSubMenu.RectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup"), "Actions", "ActionsTitle_Header");
            ActionsMenuInfoButtonHeader.RectTransform.SetSiblingIndex(0);

            ButtonGroup actionsMenuButtonGroup = new ButtonGroup("Actions", adjustAlignment: true);
            new ButtonHeader(actionsSubMenu.RectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup"), "Actions", "Actions_Header");

            FavoriteButton = new SingleButton(new Action(() =>
            {
                ImageHandler.ChangeFavoriteState();
                if (ImageHandler.IsCurrentFavorite())
                {
                    FavoriteButton.Sprite = Sprites["Star"];
                    FavoriteButton.Text = "Unfavorite";
                    FavoriteButton.TooltipText = "Unfavorite this image";
                }
                else
                {
                    FavoriteButton.Sprite = Sprites["X"];
                    FavoriteButton.Text = "Favorite";
                    FavoriteButton.TooltipText = "Favorite this image";
                }
            }), Sprites["X"], "Unfavorite", "Unfavorite this image", "Button_Favorite");

            actionsButton.OnClick += new Action(() =>
            {
                if (ImageHandler.IsCurrentFavorite())
                {
                    FavoriteButton.Sprite = Sprites["Star"];
                    FavoriteButton.Text = "Unfavorite";
                    FavoriteButton.TooltipText = "Unfavorite this image";
                }
                else
                {
                    FavoriteButton.Sprite = Sprites["X"];
                    FavoriteButton.Text = "Favorite";
                    FavoriteButton.TooltipText = "Favorite this image";
                }
            });

            SingleButton shareButton = new SingleButton(new Action(() => TabButton.SubMenu.OpenSubMenu(webhookSubMenu)), Sprites["Share"], "Share", "Share this image on via Discord Webhook", "Button_Share");

            SingleButton uploadToGalleryButton = null;
            uploadToGalleryButton = new SingleButton(new Action(() =>
            {
                if (APIUser.CurrentUser.isSupporter)
                {
                    uploadToGalleryButton.ButtonComponent.enabled = false;
                    ImageHandler.UploadToGallery(new Action(() => UiManager.PushQuickMenuAlert("Gallery - Uploading image...")),
                            new Action(() =>
                            {
                                uploadToGalleryButton.ButtonComponent.enabled = true;
                                UiManager.PushQuickMenuAlert("Gallery - Image was uploaded successfully.");
                            }),
                            new Action(() =>
                            {
                                uploadToGalleryButton.ButtonComponent.enabled = true;
                                UiManager.PushQuickMenuAlert("Gallery - Failed to upload image.");
                            }));
                }
                else
                {
                    UiManager.ShowQuickMenuInformationPopup("Warning", "You need VRC+ to use this function.", null);
                }
            }), Sprites["Upload"], "Upload to Gallery", "Upload this image to your VRChat Gallery (requires VRC+)", "Button_Upload");

            SingleButton deleteButton = new SingleButton(new Action(() => UiManager.ShowQuickMenuPopup("Delete Image", "Do you want to delete this image?", "YES", "NO", new Action(() =>
            {
                ImageHandler.DeletePicture();
                TabButton.SubMenu.CloseAllSubMenus();
            }), new Action(() => { }))), Sprites["Trash"], "Delete", "Delete this image", "Button_Delete");

            SingleButton importToSteamButton = null;
            importToSteamButton = new SingleButton(new Action(() =>
            {
                if (!SteamIntegration.Enabled)
                {
                    UiManager.ShowQuickMenuInformationPopup("Warning", "Steam Integration is disabled because it failed to load the Steam API.", new Action(() => { }));
                    return;
                }

                bool result = ImageHandler.ImportToSteam();
                if (result)
                    UiManager.PushQuickMenuAlert("Steam - Image was imported successfully.");
                else
                    UiManager.PushQuickMenuAlert("Steam - Failed to import image.");
            }), Sprites["Steam"], "Import to Steam", "Import this image to Steam and make it uploadable for the Steam Cloud", "Button_Import");

            SingleButton showInExplorerButton = new SingleButton(new Action(() => ImageHandler.ShowFileInExplorer()), Sprites["File"], "Open Explorer", "Open Windows Explorer and select this image", "Button_Explorer");

            actionsMenuButtonGroup.AddButton(FavoriteButton);
            actionsMenuButtonGroup.AddButton(shareButton);
            actionsMenuButtonGroup.AddButton(uploadToGalleryButton);
            actionsMenuButtonGroup.AddButton(deleteButton);
            actionsMenuButtonGroup.AddButton(importToSteamButton);
            actionsMenuButtonGroup.AddButton(showInExplorerButton);
            actionsSubMenu.AddButtonGroup(actionsMenuButtonGroup);

            // Settings menu buttons

            ButtonGroup settingsMenuButtonGroup1 = new ButtonGroup("File_Settings", adjustAlignment: true);
            new ButtonHeader(settingsSubMenu.RectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup"), "File Settings", "FileSettings_Header");

            ToggleButton fileOrganizationButton = new ToggleButton(new Action<bool>(state => Configuration.FileOrganization.Value = state), Sprites["Organization"], Sprites["X"], "File Organization", "File Organization", "Disable File Organization", "Enable File Organization", "Button_FileOrganization", Configuration.FileOrganization.Value);
            Configuration.FileOrganization.OnValueChanged += new Action<bool, bool>((oldValue, newValue) => fileOrganizationButton.State = newValue);

            SingleButton manuallyOrganizeButton = new SingleButton(new Action(() =>
            {
                if (Configuration.FileOrganization.Value)
                    FileOrganization.OrganizeAll();
                else
                    UiManager.ShowQuickMenuInformationPopup("Warning", "You have to enable File Organization in order to use this.", null);
            }), Sprites["Manually"], "Manually Organize", "Organize all files manually", "ManuallyFileOrganizationButton");

            SingleButton resetOrganizationButton = new SingleButton(new Action(() => UiManager.ShowQuickMenuPopup("Reset File Organization", "Are you sure to reset File Organization?", "YES", "NO", new Action(() => FileOrganization.Reset()), new Action(() => { }))), Sprites["Blocked"], "Reset Organization", "Reset the whole file organization", "Button_ResetFileOrganization");

            ToggleButton writeMetadataButton = new ToggleButton(new Action<bool>(state => Configuration.WriteImageMetadata.Value = state), Sprites["Data"], Sprites["X"], "World Metadata", "World Metadata", "Disable saving of world metadata to images", "Enable saving of world metadata to images", "Button_Metadata", Configuration.WriteImageMetadata.Value);

            settingsMenuButtonGroup1.AddButton(fileOrganizationButton);
            settingsMenuButtonGroup1.AddButton(manuallyOrganizeButton);
            settingsMenuButtonGroup1.AddButton(resetOrganizationButton);
            settingsMenuButtonGroup1.AddButton(writeMetadataButton);
            settingsSubMenu.AddButtonGroup(settingsMenuButtonGroup1);

            ButtonGroup settingsMenuButtonGroup2 = new ButtonGroup("Main_Settings", adjustAlignment: true);
            new ButtonHeader(settingsSubMenu.RectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup"), "Settings", "Settings_Header");

            ToggleButton buttonTypeButton = new ToggleButton(new Action<bool>(state => Configuration.TabButton.Value = state), Sprites["Tab"], Sprites["X"], "Tab Button", "Menu Button", "Switch to Camera Menu Button", "Switch to Tab Button", "Button_MenuType", Configuration.FileOrganization.Value);
            Configuration.TabButton.OnValueChanged += new Action<bool, bool>((oldValue, newValue) =>
            {
                buttonTypeButton.State = newValue;
                TabButton.GameObject.SetActive(newValue);
                menuButton.GameObject.SetActive(!newValue);
            });

            ToggleButton webhookStateButton = new ToggleButton(new Action<bool>(state => Configuration.DiscordWebhook.Value = state), Sprites["Share"], Sprites["X"], "Discord Webhook", "Discord Webhook", "Disable Discord Webhook", "Enable Discord Webhook", "Button_WebhookState", Configuration.DiscordWebhook.Value);
            Configuration.DiscordWebhook.OnValueChanged += new Action<bool, bool>((oldValue, newValue) => webhookStateButton.State = newValue);

            ToggleButton showRotationControlButton = new ToggleButton(new Action<bool>(state => Configuration.ShowRotationButtons.Value = state), Sprites["RotateRight"], Sprites["X"], "Rotation Buttons", "Rotation Buttons", "Hide rotation buttons", "Show rotation buttons", "Button_RotationButtonsState", Configuration.ShowRotationButtons.Value);
            Configuration.ShowRotationButtons.OnValueChanged += new Action<bool, bool>((oldValue, newValue) =>
            {
                showRotationControlButton.State = newValue;
                controlLeftRotateButton.gameObject.SetActive(!Configuration.MultiView.Value && newValue);
                controlRightRotateButton.gameObject.SetActive(!Configuration.MultiView.Value && newValue);
            });

            ToggleButton autoSwitchToLatest = new ToggleButton(new Action<bool>(state => Configuration.AutoSelectLatest.Value = state), Sprites["Gallery"], Sprites["X"], "Auto Select Latest", "Auto Select Latest", "Disable automatic selection of the latest picture on menu open", "Enable automatic selection of the latest picture on menu open", "Button_Metadata", Configuration.AutoSelectLatest.Value);

            settingsMenuButtonGroup2.AddButton(buttonTypeButton);
            settingsMenuButtonGroup2.AddButton(webhookStateButton);
            settingsMenuButtonGroup2.AddButton(showRotationControlButton);
            settingsMenuButtonGroup2.AddButton(autoSwitchToLatest);
            settingsSubMenu.AddButtonGroup(settingsMenuButtonGroup2);

            ButtonGroup settingsMenuButtonGroup3 = new ButtonGroup("Settings_Actions", adjustAlignment: true);
            new ButtonHeader(settingsSubMenu.RectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup"), "Other Actions", "Actions_Header");

            SingleButton reloadWebhooksButton = new SingleButton(new Action(() => ReloadDiscordWebhookButtons()), Sprites["Reload"], "Reload Webhooks", "Reload all Webhook files", "Button_ReloadWebhooks");
            SingleButton openGitHubPageButton = new SingleButton(new Action(() => Application.OpenURL("https://github.com/DragonPlayerX/ScreenshotManager")), Sprites["GitHub"], "Open GitHub", "Open the GitHub page of this mod", "Button_GitHub");

            settingsMenuButtonGroup3.AddButton(reloadWebhooksButton);
            settingsMenuButtonGroup3.AddButton(openGitHubPageButton);
            settingsSubMenu.AddButtonGroup(settingsMenuButtonGroup3);

            // Animations

            Animator sizeAnimator = SingleImageContainer.transform.GetComponent<Animator>();

            SingleImageContainer.transform.Find("SingleImage_Mask/Image").GetComponent<Button>().onClick.AddListener(new Action(() =>
            {
                if (sizeAnimator.GetBool("Size Trigger"))
                {
                    sizeAnimator.SetBool("Size Trigger", false);
                    singleImageStyle.enabled = true;
                }
                else
                {
                    sizeAnimator.SetBool("Size Trigger", true);
                    singleImageStyle.enabled = false;
                }
            }));

            Animator leftControlAnimator = controlLeft.GetComponent<Animator>();
            Animator rightControlAnimator = controlRight.GetComponent<Animator>();

            RectTransform leftControlRect = controlLeft.GetComponent<RectTransform>();
            RectTransform rightControlRect = controlRight.GetComponent<RectTransform>();

            EnableDisableListener menuListener = TabButton.SubMenu.GameObject.AddComponent<EnableDisableListener>();
            menuListener.OnDisableEvent += new Action(() =>
            {
                controlLeft.localScale = new Vector3(0.5f, 0.5f, 1);
                leftControlRect.anchoredPosition = new Vector3(-1024, 0, 0);
                leftControlAnimator.ResetTrigger("Normal");
                leftControlAnimator.ResetTrigger("Highlighted");
                leftControlAnimator.ResetTrigger("Pressed");
                leftControlAnimator.ResetTrigger("Selected");
                leftControlAnimator.ResetTrigger("Disabled");

                controlRight.localScale = new Vector3(0.5f, 0.5f, 1);
                rightControlRect.anchoredPosition = new Vector3(1024, 0, 0);
                rightControlAnimator.ResetTrigger("Normal");
                rightControlAnimator.ResetTrigger("Highlighted");
                rightControlAnimator.ResetTrigger("Pressed");
                rightControlAnimator.ResetTrigger("Selected");
                rightControlAnimator.ResetTrigger("Disabled");

                sizeAnimator.Rebind();
                sizeAnimator.Update(0);
                sizeAnimator.SetBool("Size Trigger", false);
                singleImageStyle.enabled = true;
                Configuration.Save();
            });

            EnableDisableListener settingsMenuListener = settingsSubMenu.GameObject.AddComponent<EnableDisableListener>();
            settingsMenuListener.OnDisableEvent += new Action(() => Configuration.Save());
        }

        public static void ReloadDiscordWebhookButtons()
        {
            webhookSubMenu.ClearButtonGroups();
            Configuration.LoadDiscordWebhooks();
            foreach (KeyValuePair<string, DiscordWebhookConfiguration> webhookConfig in Configuration.DiscordWebHooks)
            {
                ButtonGroup buttonGroup = new ButtonGroup("Webhook_" + webhookConfig.Key, parent: webhookSubMenu.PageLayoutGroup.rectTransform);
                buttonGroup.ButtonLayoutGroup.cellSize = new Vector2(800, 100);
                buttonGroup.ButtonLayoutGroup.rectTransform.sizeDelta = new Vector2(3760, 120);
                buttonGroup.ButtonLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                WideButton button = new WideButton(new Action(() =>
                {
                    if (webhookConfig.Value.WebhookURL.Value.ToLower().StartsWith("https://discordapp.com/api/webhooks/") || webhookConfig.Value.WebhookURL.Value.ToLower().StartsWith("https://discord.com/api/webhooks/") || webhookConfig.Value.WebhookURL.Value.ToLower().StartsWith("https://media.guilded.gg/webhooks/"))
                    {
                        ImageHandler.SendToDiscordWebhook(webhookConfig.Key, webhookConfig.Value,
                            new Action(() => UiManager.PushQuickMenuAlert("Webhook - Uploading image...")),
                            new Action(() => UiManager.PushQuickMenuAlert("Webhook - Image was uploaded successfully.")),
                            new Action(() => UiManager.PushQuickMenuAlert("Webhook - Failed to upload image.")));
                        TabButton.SubMenu.PopSubMenu();
                    }
                    else
                    {
                        UiManager.ShowQuickMenuInformationPopup("Warning", "The given Webhook URL is invalid.", null);
                    }
                }), webhookConfig.Key, "Send to " + webhookConfig.Key, "Button_" + webhookConfig.Key, buttonGroup.ButtonLayoutGroup.transform);
                buttonGroup.AddButton(button);
                webhookSubMenu.AddButtonGroup(buttonGroup);
            }
        }
    }
}
