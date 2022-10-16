# Discord Status for foobar2000 (Rich Presence)

Shows the currently playing track in foobar2000 in your Discord status. Works similarly to the Spotify integration.

## Features
- Displays album art thumbnails, similar to the Spotify integration (images are fetched from Deezer)
- The "now playing" status disappears completely when you pause or stop the playback
- Fully customizable format of displayed text

## Installation
1. Install the [.NET 5 Desktop Runtime (x86)](https://dotnet.microsoft.com/en-us/download/dotnet/5.0) (make sure to select the x86 version)
2. Download and install the [asmejkal/foo_dotnet_component_host](https://github.com/asmejkal/foo_dotnet_component_host/releases/) component
3. Download DiscordFoobarStatus from the [Releases](https://github.com/asmejkal/DiscordFoobarStatus/releases) section
4. Install the component in foobar2000 Preferences -> Components -> .NET Component Host -> Install...
5. Make sure your Activity Privacy settings in Discord allow status messages to be displayed
