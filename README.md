# Screenshot Manager

## Requirements

- [MelonLoader 0.4.x](https://melonwiki.xyz/)
- [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/tree/main/VRChatUtilityKit)

## Features

- View all your Screenshots directly ingame
- View only pictures you made today (configurable hour offset)
- Single and page mode (also you can enlarge an image by clicking on it)
- Tab and UIX button available (fallback button for no UIX in camera menu)
- Option to move VRC+ Gallery button to ScreenshotManager Menu
- Image Operations:
    - Move picture to favorite folder (this is a subdirectory in the default screenshot folder)
    - Rotate picture ingame
    - Open Windows Explorer and select image
    - Delete picture (with confirm)
    - Share picture via Discord Webhook (you can add multiple webhooks with the config files)
- File Organization:
    - Change the location where VRChat Screenshots are saved. (compatible with [LagFreeScreenshots](https://github.com/knah/VRCMods/tree/master/LagFreeScreenshots))
    - Automatic sorting of new screenshots by placing them in subfolders named by the current day
        - You are able to convert old screenshots to the folders by pressing a button in the menu
        - You can revert the organization and move all images back to the main folder
        - You can customize the naming of directories and files in MelonPreferences file

---

### Planned Features

- Automatic loading of new screenshots without pressing reload or switching category
- Maybe some things with the information of world and players in the metadata from screenshots which are taken with LagFreeScreenshots

---

### Menu Screenshot

![UI Screenshot](https://i.imgur.com/VXotdbg.png)

## Texture loading

You may experience a little bit of lag when going through your pictures (only really noticeable in multi view or with high resolution screenshots). This is caused by Unity's texture loading system which is not complete non-blocking. I also tried [ImageSharp](https://github.com/SixLabors/ImageSharp) for better texture loading but it turned out that my current solution works better than ImageSharp. If someone have an idea to improve this let me know it.

## File Organization

Some code for the file organization comes from an older mod ([PhotoOrganization](https://github.com/dave-kun/PhotoOrganization)) wich is not managed anymore.

**You should not use PhotoOrganization and ScreenshotManager at the same time. They maybe interfere each other.**

If you used the PhotoOrganization Mod before you may encounter a problem that the images won't load. In this case please go to the "Screenshot Manager Menu" -> "File Organization" -> click on "Reset Organization" and then click on "Manually Organization". The mod will reset all things (restore filenames and deleting old folders) and then create new folders and move the files.

## Discord Webhook
 
You can enable the Discord Webhook (disabled by default) directly ingame or in the MelonPreferences file.

The code to send the Discord Webhook data is currently in an external executable file ([source code](DiscordWebhook)) wich is extracted at startup. The reason for this is that MelonLoader blocks all requests wich are send to discord.com to take action against malicious mods.

### How to create a Webhook?

To create a Webhook you have to go to "VRChat/UserData/ScreenshotManager/DiscordWebhooks". You will see a template file for a Webhook. The name of the file will be displayed ingame and in the file you can define things like WebhookURL, Username, Message etc. To add multiple Webhooks you can simply copy the template and paste it in the same folder under a new name. In the ingame menu you have a scroll view of all Webhooks when you click on share.

For "Username" you can use **{vrcname}** and for "Message" you can use **{vrcname}** and **{creationtime}** to implement these values in the Webhook content.

## Files & Time

ScreenshotManager supports two ways to sort and handle your images. The first (and default) is the last write time of the file because it persists against file copy. The second one is the actual creation time of the file but this could be reset if you copy files or maybe in some other cases. Otherwise the last write time gets modified when you edit the image outside of the game (ingame rotation does keep the old time value). Normally you should be fine with the default but you can change it in the MelonPreferences file.

## Credits

I used the async/await utilities from [UIExpansionKit](https://github.com/knah/VRCMods/blob/master/UIExpansionKit) as reference for my own implementation. It really helped me in understanding some of the async/await mechanics.