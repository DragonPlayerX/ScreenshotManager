# Screenshot Manager

## Known Issues

- Automatic image loading is **not** compatible with LagFreeScreenshots

*Will be fixed soon*

## Requirements

- [MelonLoader 0.4.x](https://melonwiki.xyz/)

## Features

- View all your Screenshots directly ingame
- Categorize pictures (all, today, yesterday, favorites)
- Single and page mode
- Tab and Camera Menu button available
- Image Functions:
    - Move picture to favorite folder (this is a subdirectory in the default screenshot folder)
    - Rotate picture ingame
    - Open Windows Explorer and select image
    - Delete picture (with confirm)
    - Share picture via Discord Webhook (you can add multiple webhooks with the config files)
    - Upload picture to VRC+ Gallery
    - Import picture directly to Steam
    - Reading world metadata from [LagFreeScreenshots](https://github.com/knah/VRCMods/tree/master/LagFreeScreenshots)
    - Generating own world metadata without LagFreeScreenshots
    - Optional automatic compression on webhook uploads
- File Organization:
    - Change the location where VRChat Screenshots are saved.
    - Automatic sorting of new screenshots by placing them in subfolders named by the current day
        - You are able to convert old screenshots to the folders by pressing a button in the menu
        - You can revert the organization and move all images back to the main folder
        - You can customize the naming of directories and files in MelonPreferences file

---

### Planned Features

- Automatic loading of new screenshots without pressing reload or switching category (works without LagFreeScreenshots currently)

---

### Menu Screenshot

![UI Screenshot 1](https://i.imgur.com/gta8f8G.png) 
![UI Screenshot 2](https://i.imgur.com/oki8Wfn.png)
![UI Screenshot 3](https://i.imgur.com/ZBC2uyr.png)

## Texture loading

You may experience a little bit of lag when going through your pictures (only really noticeable in multi view or with high resolution screenshots). This is caused by Unity's texture loading system which is not complete non-blocking. I also tried [ImageSharp](https://github.com/SixLabors/ImageSharp) for better texture loading but it turned out that my current solution works better than ImageSharp. If someone have an idea to improve this let me know it.

## File Organization

Some code for the file organization comes from an older mod ([PhotoOrganization](https://github.com/dave-kun/PhotoOrganization)) wich is not managed anymore.

**PhotoOrganization seems to be broken now caused by the new UI update.**

If you used the PhotoOrganization Mod before you may encounter a problem that the images won't load. In this case please go to the "Screenshot Manager Menu" -> "File Organization" -> click on "Reset Organization" and then click on "Manually Organization". The mod will reset all things (restore filenames and deleting old folders) and then create new folders and move the files.

## Discord Webhook
 
You can enable the Discord Webhook (disabled by default) directly ingame or in the MelonPreferences file.

The code to send the Discord Webhook data is currently in an external executable file ([source code](DiscordWebhook)) wich is extracted at startup. The reason for this is that MelonLoader blocks all requests wich are send to discord.com to take action against malicious mods.

### How to create a Webhook?

To create a Webhook you have to go to "VRChat/UserData/ScreenshotManager/DiscordWebhooks". You will see a template file for a Webhook. The name of the file will be displayed ingame and in the file you can define things like WebhookURL, Username, Message etc. To add multiple Webhooks you can simply copy the template and paste it in the same folder under a new name. In the ingame menu you have a scroll view of all Webhooks when you click on share.

The webhook file supports some tags with these symbols "{" "}". You can find an example here [DiscordWebhookTemplate](https://github.com/DragonPlayerX/ScreenshotManager/blob/master/ScreenshotManager/Resources/DiscordWebhookTemplate.cfg)

Available tags for username and message of the webhook:

- Username:
    - {vrcname} = VRChat name

- Message:
    - {vrcname} = VRChat name
    - {world} = World name the image is taken in (requires the built-in metadata saving or metadata from LagFreeScreenshots to be enabled)
    - {creationtime} = Image creation time formatted with the "CreationTimeFormat" value of the webhook file
    - {timestamp:\<value\>} = Embed the Discord Timestamp support

### How does the compression works?

The Webhook config file contains an property called "CompressionThreshold" (default value is -1). Already existing Webhook config files does not have this property so it will use the default value but you can just add it to the config if you need to.

You only have to set the compression threshold to the upload limit (in megabyte) of your Discord Webhook (Normal Server: 8 MB, Level 2 Boost: 50 MB, Level 3 Boost: 100 MB). Setting the value to -1 will result in disabling the compression.

The image compression will only work for **PNG** images because it converts these to **JPEG** images which are less quality but also less file size.

### How to use Discord Timestamps

![Discord Timestamps](https://i.imgur.com/lDvBjQn.png)

## Steam Integration

This mod uses the native Steam API and invokes internal screenshot methods to import the image to steam. It's not modifying the screenshots.vdf like other screenshots importer might do.

## Files & Time

ScreenshotManager supports two ways to sort and handle your images. The first (and default) is the last write time of the file because it persists against file copy. The second one is the actual creation time of the file but this could be reset if you copy files or maybe in some other cases. Otherwise the last write time gets modified when you edit the image outside of the game (ingame rotation does keep the old time value). Normally you should be fine with the default but you can change it in the MelonPreferences file.

## Credits

- I used very much code of [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/tree/main/VRChatUtilityKit) by [loukylor](https://github.com/loukylor) to implement the needed features directly to this mod
- I used the async/await utilities from [UIExpansionKit](https://github.com/knah/VRCMods/blob/master/UIExpansionKit) as reference for my own implementation.
- As mentioned before, i used [PhotoOrganization](https://github.com/dave-kun/PhotoOrganization) as my reference for the File Organization

### Credits for all Icon Authors

- https://www.flaticon.com/authors/kirill-kazachek
- https://www.flaticon.com/authors/freepik
- https://www.flaticon.com/authors/twentyfour
- https://www.flaticon.com/authors/becris
- https://www.flaticon.com/authors/pixel-perfect
- https://www.flaticon.com/authors/aldo-cervantes
- https://www.flaticon.com/authors/stockio
- https://www.flaticon.com/authors/dave-gandy
