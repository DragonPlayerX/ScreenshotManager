# Screenshot Manager

## Requirements

- [MelonLoader 0.4.x](https://melonwiki.xyz/)
- [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/tree/main/VRChatUtilityKit)

## Features

- View all your Screenshots directly ingame
- View only pictures you made today (configurable hour offset)
- Single and page mode (also you can enlarge an image by clicking on it)
- Tab and UIX button available (fallback button for no UIX in camera menu)
- Image Operations:
    - Move picture to favorite folder (this is a subdirectory in the default screenshot folder)
    - Open Windows Explorer and select image
    - Delete picture (with confirm)
    - Share picture via Discord Webhook (requires setup in config)
- File Organization:
    - Change the location where VRChat Screenshots are saved. (compatible with [LagFreeScreenshots](https://github.com/knah/VRCMods/tree/master/LagFreeScreenshots))
    - Automatic sorting of new screenshots by placing them in subfolders named by the current day
        - You are able to convert old screenshots to the folders by pressing a button in the menu
        - You can revert the organization and move all images back to the main folder
        - You can customize the naming of directories and files in MelonPreferences file

### Menu Screenshot

![UI Screenshot](https://i.imgur.com/VXotdbg.png)

## Texture loading

You may experience a little bit of lag when going through your pictures (only really noticeable in multi view or with high resolution screenshots). This is caused by Unity's texture loading system which is not complete non-blocking. I also tried [ImageSharp](https://github.com/SixLabors/ImageSharp) for better texture loading but it turned out that my current solution works better than ImageSharp. If someone have an idea to improve this let me know it.

## File Organization

Some code for the file organization comes from an older mod ([PhotoOrganization](https://github.com/dave-kun/PhotoOrganization)) wich is not managed anymore. 

If you used the PhotoOrganization Mod before you may encounter a problem that the images won't load. In this case please go to the "Screenshot Manager Menu" -> "File Organization" -> click on "Reset Organization" and then click on "Manually Organization". The mod will reset all things (restore filenames and deleting old folders) and then create new folders and move the files.

## Discord Webhook
 
You can enable the Discord Webhook (disabled by default) directly ingame or in the MelonPreferences file. You have to provide an Webhook URL and maybe change some other stuff.

The code to send the Discord Webhook data is currently in an external executable file ([source code](DiscordWebhook)) wich is extracted at startup. I don't know why but when I try to make any web requests to discord.com within the mod they get blocked. My guess would therefore be that MelonLoader blocks this to take action against malicious mods. Or maybe I missed something and there is another solution. If someone knows something about it please let me know because i don't like this workaround.

## Credits

I used the async/await utilities from [UIExpansionKit](https://github.com/knah/VRCMods/blob/master/UIExpansionKit) as reference for my own implementation. It really helped me in understanding some of the async/await mechanics.