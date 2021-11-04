# Kebler - Transmission Remote GUI (Windows only) x64
![Kebler](https://github.com/Rebell81/Kebler/raw/master/Images/1.png)
![Kebler](https://github.com/Rebell81/Kebler/raw/master/Images/2.png)

## Some info

Kebler is GUI for Transmission Daemon (local or remote)
Kebler developed using .Net 5 and still in beta


[Official Transmission RPC specs](https://github.com/transmission/transmission/blob/master/extras/rpc-spec.txt)

## Installation

Go to [Releases](https://github.com/Rebell81/Kebler/releases/latest) page and download one of builds. Or clone repository and build it by ur self using [Visual Studio 2019](https://visualstudio.microsoft.com/) and preinstalled [.Net 5](https://dotnet.microsoft.com/download/dotnet/5.0)

Be carefull, portable version doesnot include auto update feature yet

Also you can download <B>prerelease installer</B> and receive updates from <B>develop</B> branch.
Or edit `app.config` at `C:\Users\{USERNAME}\AppData\Roaming\Kebler` and set `AllowPreRelease=True`


#### Using [WinGet](https://docs.microsoft.com/en-us/windows/package-manager/winget/)

Run `winget install Kebler` to install the latest version of Kebler Transmission Remote GUI.


## Fixed hotkeys

- <kbd>Ctrl</kbd> + <kbd>C</kbd> : Connection manager
- <kbd>Ctrl</kbd> + <kbd>N</kbd> : Add new torrent
- <kbd>Ctrl</kbd> + <kbd>M</kbd> : Add magnet link
- <kbd>Ctrl</kbd> + <kbd>F</kbd> : Find torrent
- <kbd>Alt</kbd> + <kbd>X</kbd> : Exit
- <kbd>F2</kbd> : Rename torrent
- <kbd>Shift</kbd> + <kbd>Delete</kbd> : Remove torrent with <B>Data</B>
- <kbd>Delete</kbd> : Remove torrent and save data on disk
- <kbd>Escape</kbd> : Hide props
- <kbd>Space</kbd> : Start or Pause torrent
## License

Copyright (c) 2019 by Rebell81

Kebler is free opensource software;

KeblerI is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the Apache-2.0 License for more details.
