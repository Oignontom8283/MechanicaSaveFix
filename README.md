
# Mechanica Save Fix

**Mechanica Save Fix** is a [BepInEx](https://github.com/bepinex/bepinex) mod
for the game [Mechanica](https://store.steampowered.com/app/1226990/Mechanica/), a survival game mixing survival and programming developed by 
**Deimos Interactive**, with development abandoned since February 2022.
This mod attempts to fix the save system of the game.

This mod is no miracle; at best, it is a bandage on a gaping wound...

Large worlds have what I call the "Infinite Save Loading" (ISL),
which prevents the game from saving and forces you to restart it, causing you to lose your progress.


## Installation

### 1. Own the game.

Legally, please.


### 2. Install BepInEx

- Download [BepInEx 5.4.23.5 win_x86](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5).
You should be able to use any version of BepInEx 5,
but I recommend using the same one used for the mod's development.

- Extract the archive's content into the game's installation folder. By default, it's `C:\Program Files (x86)\Steam\steamapps\common\Mechanica\`.

- Launch the game once so BepInEx can create the necessary folders, then close the game.


### 3. Install the mod

- Download the latest version of the mod [here](https://github.com/Oignontom8283/MechanicaSaveFix/releases). It comes as a `.dll` file.

- Copy the `.dll` file into the game's `BepInEx\plugins` folder. By default, it's `C:\Program Files (x86)\Steam\steamapps\common\Mechanica\BepInEx\plugins`.

### 4. Done!


## Contribution

I do not use Visual Studio, I don't like that software. So it's very simple.

### 1. Have the game installed on your computer, legally, please.

### 2. Install BepInEx (see the "Installation" section above)

### 3. The .NET development SDK

Install the .NET development SDK (to dev in C#):
```
winget install Microsoft.DotNet.SDK.10
```

Verify that the SDK is properly installed:
```
dotnet --version
```

### 4. Clone your fork of the project

### 5. Compilation

It's really simple! To compile the project, just run the command `dotnet build -c Release`,
which will generate the `MechanicaSaveFix.dll` file in the `bin/Release/netstandard2.1/` folder.

But I recommend using the build script, which will compile the .dll and place it in the `MECHANICA_FOLDER/BepInEx/plugins/` folder for you.

To do so, run the following command, or use `CTRL + SHIFT + B` in Visual Studio Code:
```
./build.ps1
```

> [!NOTE]
> On the first run of the script, follow the instructions to create the configuration file!
> 
> If your game is located in a non-standard folder, change the path in the configuration file the script asks you to create.


### 6. Launch the game (test)

Launch your game normally... There you go (:

> [!TIP]
> To enable the BepInEx console, you need to enable it in the `MECHANICA_FOLDER/BepInEx/config/BepInEx.cfg` file by changing the value of `Enabled` under `Logging.Console` to `true`.
> ```ini {6}
> [Logging.Console]
> 
> ## Enables showing a console for log output.
> # Setting type: Boolean
> # Default value: false
> Enabled = true    <-- Here!
> 
> ## If enabled, will prevent closing the console (either by deleting the close button or in other platform-specific way).
> # Setting type: Boolean
> ```


## License

This project is licensed under the AGPL-v3.0 (GNU Affero General Public License v3.0), see the [LICENSE](LICENSE) file for informations.