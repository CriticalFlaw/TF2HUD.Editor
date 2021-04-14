This section is for common issues you may encounter and how to resolve them.

* For error or issues not on this page, please [open a ticket on our issue tracker][issues-link].
* For questions not covered in this documentation, [post in the discussions forum][discuss-link].

### The latest, downloaded release does not contain the executable.
You most likely downloaded the source code instead of the editor. On the [releases][releases-link] page, be sure to download the file named **TF2HUD.Editor.zip** and extract it into a separate folder.

### The editor does not launch after downloading and extracting it.
Make sure to install [Microsoft .NET 5.0 Runtime][runtime-link] ([x64][runtime64-link] for 64-bit systems and [x86][runtime86-link] for 32-bit). If you just installed it and the editor still does not launch, then restart your computer.

### My applied settings are not being shown in-game.
To see your applied changes in-game, open the console and input `hud_reloadscheme`. This will refresh the HUD and will display your selected customizations.

!!! note
    Certain settings may require the game to be restarted, this mainly applies to color and main menu changes.

### I'm receiving an error when applying or resetting my settings.
Majority of the errors you may encounter will be caused by having an outdated version of the HUD installed in your tf/custom directory. Each customization option has a set of instructions on what changes need to be applied and where. So if your HUD is very outdated, chances are that the editor will not what it is supposed to update and return an error. For best result, be sure you always have the latest version of both the HUD and the editor installed.

<!-- MARKDOWN LINKS -->
[issues-link]: https://github.com/CriticalFlaw/TF2HUD.Editor/issues
[discuss-link]: https://github.com/CriticalFlaw/TF2HUD.Editor/discussions
[releases-link]: https://github.com/CriticalFlaw/TF2HUD.Editor/releases
[json-link]: https://www.criticalflaw.ca/TF2HUD.Editor/json/
[runtime-link]: https://dotnet.microsoft.com/download/dotnet/5.0/runtime
[runtime86-link]: https://download.visualstudio.microsoft.com/download/pr/c089205d-4f58-4f8d-ad84-c92eaf2f3411/5cd3f9b3bd089c09df14dbbfb64124a4/windowsdesktop-runtime-5.0.5-win-x86.exe
[runtime64-link]: https://download.visualstudio.microsoft.com/download/pr/c1ef0b3f-9663-4fc5-85eb-4a9cadacdb87/52b890f91e6bd4350d29d2482038df1c/windowsdesktop-runtime-5.0.5-win-x64.exe
