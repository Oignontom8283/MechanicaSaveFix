
# Mechanica Save Fix

A BepInEx mod for **Mechanica** that replaces the game's "one small file per object" save format with a single archive file per world.

This mod is no miracle; at best, it's a bandage on a gaping wound.

Large worlds suffer from what I call the "Infinite Save Loading" (ISL): saving simply never finishes, forcing you to restart the game and lose your progress.

## Read this first

This project is **a lot of hacking**. The game's save system was not designed to be replaced, so the mod does not rewrite it. It sits on top of it: it intercepts the game's file operations and redirects them to memory, then packs the result into an archive. It works, but it's held together by a fair amount of workarounds.

- **It does not fix multiplayer.** Multiplayer issues are out of scope, and multiplayer behavior with this mod is not guaranteed.
- **It does not repair corrupted worlds.** A world that was already broken stays broken. The mod only changes how saves are stored.
- **Saves are converted to `.msa` archives.** On the first save, a world is converted. Your original folder is left untouched, but the game itself cannot read `.msa` files, so without the mod only the original folder is usable.
  **The `.msa` file is simply an archive containing the normal save files.** To restore a standard save the base game can use, extract the contents of the `.msa` file into a folder.
- **Keep backups of your worlds.** The mod has a backup option, but this is beta software.

## Installation

### 1. Own the game

Legally, please.

### 2. Install BepInEx

- Download [BepInEx 5.4.23.5 win_x86](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5). You should be able to use any version of BepInEx 5, but I recommend using the same one used for the mod's development.
- Extract the archive's content into the game's installation folder. By default, that's `C:\Program Files (x86)\Steam\steamapps\common\Mechanica\`.
- Launch the game once so BepInEx can create the necessary folders, then close the game.

### 3. Install the mod

- Download the latest version of the mod [here](https://github.com/Oignontom8283/MechanicaSaveFix/releases). It comes as a `.dll` file.
- Copy the `.dll` file into the game's `BepInEx\plugins` folder. By default, that's `C:\Program Files (x86)\Steam\steamapps\common\Mechanica\BepInEx\plugins`.

> [!INFO]
> For Linux users, it would apparently be necessary to allow DLL injection in the Proton settings.
> This has not been tested (my Linux machine died), but it should work. Feel free to share your feedback on this.

### 4. Done!

## Work done (non-exhaustive)

- A virtual file system that stores the whole save in memory.
- Interception of the `System.IO` calls the game uses (`File`, `Directory`, `FileInfo`, `DirectoryInfo`).
- Hooks around the game's save, load, and exit-to-menu flow to start and stop interception at the right moments.
- A rewrite of the save selection menu and its popups.
- A rewrite of the new-world creation flow.
- A config file and an opt-in update check.

## Problems caused by how the game works

- **One file per entity.** Every object, robot, weld, link, and so on is saved as its own small file in nested folders. A small test world already means hundreds of folders, and big worlds reach thousands of files. This is the suspected cause of the endless "saving/loading" hangs on large worlds (ISL). That's not proven, and the hangs are very hard to reproduce.
- **No central save code.** Each system (objects, robots, welds, natural resources, storage, links...) reads and writes its own files through its own methods. There's no single place to hook into.
- **Incremental saving plus a cleanup pass.** The game only rewrites entities that changed, then deletes every file that's not on its list of valid paths, compared as exact strings. The mod therefore has to keep the complete world state in memory and reproduce the game's path strings exactly, including mixed `/` and `\` separators, letter case, and spaces.
- **Folders only exist implicitly.** The game checks whether folders exist before reading them, and some of them are legitimately empty. The virtual file system has to claim that folders exist even when nothing is inside them.
- **Not everything goes through `File` and `Directory`.** Some code paths use `FileInfo` and `DirectoryInfo` instead, which needed their own interception.
- **Link loading retry loop.** Links are loaded with a retry loop of up to 100 passes that's supposed to yield every few loads. From the decompiled code, its counter is never incremented, so it runs synchronously.
- **Save sub-tasks can fail silently.** The save runs several coroutines in parallel and waits for each one to report "done". An exception thrown inside one of them after it has started is not caught, so its flag may never be set and the main save waits forever. This is a possible cause of the hangs, also unconfirmed.
- **The save menu is built on dictionaries keyed by UI objects.** Its buttons find their data by looking UI objects up in dictionaries, and much of its UI is wired in the Unity editor, so it's invisible in decompiled code. The layout had to be inspected at runtime, and the menu was rewritten.

## Contribution

I don't use Visual Studio, I don't like that software. So it's very simple.

### 1. Have the game installed on your computer

Legally, please.

### 2. Install BepInEx

See the "Installation" section above.

### 3. Install the .NET development SDK

Install the .NET development SDK (to develop in C#):
```
winget install Microsoft.DotNet.SDK.10
```

Verify that the SDK is properly installed:
```
dotnet --version
```

### 4. Clone your fork of the project

### 5. Compilation

It's really simple! To compile the project, just run:
```
dotnet build -c Release
```
This generates the `MechanicaSaveFix.dll` file in the `bin/Release/netstandard2.1/` folder.

But I recommend using the build script instead, which compiles the `.dll` and places it directly in the `MECHANICA_FOLDER/BepInEx/plugins/` folder for you.

To do so, run:
```
./build.ps1
```
or use `CTRL + SHIFT + B` in Visual Studio Code.

> [!NOTE]
> On the first run of the script, follow the instructions to create the configuration file!
>
> If your game is located in a non-standard folder, change the path in the configuration file the script asks you to create.

### 6. Launch the game (test)

Launch your game normally... There you go (:

> [!TIP]
> To enable the BepInEx console, edit `MECHANICA_FOLDER/BepInEx/config/BepInEx.cfg` and set `Enabled` under `[Logging.Console]` to `true`:
> ```ini
> [Logging.Console]
>
> ## Enables showing a console for log output.
> # Setting type: Boolean
> # Default value: false
> Enabled = true    <-- Here!
> ```

## Help

If you run into problems, feel free to open a ticket on the GitHub repository.

Feel free to share your experience in Discussions or an Issue (:

If you want help or information about how Mechanica works and how to mod it, you can contact me however you like :3

## License

This project is licensed under the AGPL-v3.0 (GNU Affero General Public License v3.0). See the [LICENSE](LICENSE) file for details.

This project is not affiliated with Deimos Interactive, the developer of Mechanica, or any other company or entity. All rights to the game Mechanica are owned by Deimos Interactive.
