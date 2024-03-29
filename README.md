# Disclaimer

Since 27/07/2022 VRChat implemented [Easy Anti-Cheat](https://easy.ac) into their game.
This decision is horrible and will totally harm VRChat more than it will help.

They basically removed all access to Quality of Life mods including serval protections mods against common types of crashing and also all the mods that added features to the game which the community very much wanted but VRChat takes a shit ton of time to implement barely anything of it.

If you think EAC will prevent malicious people from harming you, you're wrong. The Anti-Cheat can be bypassed and they will continue doing their stuff.

The community tried to stop this with as much effort as possible, including the most upvoted [Feedback Post](https://feedback.vrchat.com/open-beta/p/eac-in-a-social-vr-game-creates-more-problems-than-it-solves) with 22k votes, a [Petition](https://www.change.org/p/vrchat-delete-anticheat-system) with almost 14.000 signatures and serval YouTube videos, posts and general social media activity. But they haven't listened to us.

**If you're currently subscribed to VRC+ then please consider of cancelling the subscription and leaving the game entirely.**

Also check out [ChilloutVR](https://discord.gg/abi) and [NeosVR](https://discord.gg/NeosVR).

# Screenshot Manager

## Requirements

- [MelonLoader 0.5.3+](https://melonwiki.xyz/)

**Version 2.4.0+ of ScreenshotManager is not compatible with MelonLoader below version 0.5.3!**

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

- *Nothing currently, you can request features by creating a github issue*

---

### Menu Screenshot

![UI Screenshot 1](https://i.imgur.com/QkaeRSc.png) 
![UI Screenshot 2](https://i.imgur.com/CNo4VaJ.png)
![UI Screenshot 3](https://i.imgur.com/cbEfaRp.png)

## Texture loading

You may experience a little bit of lag when going through your pictures (only really noticeable in multi view or with high resolution screenshots). This is caused by Unity's texture loading system which is not complete non-blocking. I also tried [ImageSharp](https://github.com/SixLabors/ImageSharp) for better texture loading but it turned out that my current solution works better than ImageSharp. If someone have an idea to improve this let me know it.

## File Organization

Some code for the file organization comes from an older mod ([PhotoOrganization](https://github.com/dave-kun/PhotoOrganization)) which is not managed anymore.

If you used the PhotoOrganization Mod before you may encounter a problem that the images won't load. In this case, please go to the "Screenshot Manager Menu" -> "File Organization" -> click on "Reset Organization" and then click on "Manually Organization". The mod will reset all things (restore filenames and deleting old folders) and then create new folders and move the files.

You can fully modify the name of your images with the config entry called "FileOrganizationNameFormat". The default value is "VRChat_{timestamp}" and **must** contain "{timestamp}" otherwise it would get reset. Another possible option would be to put "{resolution}" into it, it will put the image resolution in the file name.

## Discord Webhook
 
You can enable the Discord Webhook (disabled by default) directly ingame or in the MelonPreferences file.

The code to send the Discord Webhook data is currently in an external executable file ([source code](DiscordWebhook)) which is extracted at startup. The reason for this is that MelonLoader blocks all requests which are send to discord.com to take action against malicious mods.

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

### How to use Discord Timestamps

![Discord Timestamps](https://i.imgur.com/lDvBjQn.png)

### How does the compression works?

The Webhook config file contains an property called "CompressionThreshold" (default value is -1). Already existing Webhook config files does not have this property so it will use the default value but you can just add it to the config if you need to.

You only have to set the compression threshold to the upload limit (in megabyte) of your Discord Webhook (Normal Server: 8 MB, Level 2 Boost: 50 MB, Level 3 Boost: 100 MB). Setting the value to -1 will result in disabling the compression.

The image compression will only work for **PNG** images because it converts these to **JPEG** images which are less quality but also less file size.

### How does automatic uploading works?

The Webhook config file contains a property called "AutoUpload" (default value is false). Already existing Webhook config files does not have this property so it will use the default value but you can just add it to the config if you need to.

If you set the AutoUpload to true, it will automatically send a new taken picture after 3 seconds to the corresponding webhook.

### Custom Webhook URLs

You can set the "AllowCustomURL" property in the webhook config to true if you want to disable the url filtering.

## Steam Integration

This mod uses the native Steam API and invokes internal screenshot methods to import the image to steam. It's not modifying the screenshots.vdf like other screenshots importer might do.

## Files & Time

ScreenshotManager supports two ways to sort and handle your images. The first (and default) is the last write time of the file because it persists against file copy. The second one is the actual creation time of the file but this could be reset if you copy files or maybe in some other cases. Otherwise the last write time gets modified when you edit the image outside of the game (ingame rotation does keep the old time value). Normally you should be fine with the default but you can change it in the MelonPreferences file.

## Credits

- I used very much code of [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/tree/main/VRChatUtilityKit) by [loukylor](https://github.com/loukylor) to implement the needed features directly to this mod
- I used the async/await utilities from [UIExpansionKit](https://github.com/knah/VRCMods/blob/master/UIExpansionKit) by [knah](https://github.com/knah) as reference for my own implementation.
- I used two methods related to PNG CRC from [LagFreeScreenshots](https://github.com/knah/VRCMods/blob/master/LagFreeScreenshots) by [knah](https://github.com/knah)
- As mentioned before, I used [PhotoOrganization](https://github.com/dave-kun/PhotoOrganization) as my reference for the File Organization

### Credits for all Icon Authors

- https://www.flaticon.com/authors/kirill-kazachek
- https://www.flaticon.com/authors/freepik
- https://www.flaticon.com/authors/twentyfour
- https://www.flaticon.com/authors/becris
- https://www.flaticon.com/authors/pixel-perfect
- https://www.flaticon.com/authors/aldo-cervantes
- https://www.flaticon.com/authors/dave-gandy
- https://www.flaticon.com/authors/ilham-fitrotul-hayat
- https://www.flaticon.com/authors/bamicon
