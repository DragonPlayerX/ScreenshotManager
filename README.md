# Screenshot Manager

## Requirements

- [MelonLoader 0.4.x](https://melonwiki.xyz/)
- [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/tree/main/VRChatUtilityKit)

## Features

- View all your Screenshots directly ingame
- View only pictures you made today (configurable hour offset)
- Single and page mode (also you can enlarge an image by clicking on it)
- Image operations:
    - Move picture to favorite folder (this is a subdirectory in the default screenshot folder)
    - Open Windows Explorer and select image
    - Delete picture (with confirm)
    - Share picture via Discord Webhook (requires setup in config)

By default the tab button is enabled but it can be moved to the camera quickmenu.

### Menu Screenshot

![UI Screenshot](https://i.imgur.com/ECNbY5e.png)

## Texture loading

You may experience a little bit of lag when going through your pictures (only really noticeable in multi view or with high resolution screenshots).

## Discord Webhook
 
You can enable the Discord Webhook (disabled by default) directly ingame or in the MelonPreferences file. You have to provide an Webhook URL and maybe change some other stuff.

The code to send the Discord Webhook data is currently in an external executable file ([source code](DiscordWebhook)) wich is extracted at startup. I don't know why but when I try to make any web requests to discord.com within the mod they get blocked. My guess would therefore be that MelonLoader blocks this to take action against malicious mods. Or maybe I missed something and there is another solution. If someone knows something about it please let me know because i don't like this workaround.