---
title: Troubleshooting
---

This section is for common issues you may encounter and how to resolve them.

* For issues not on this page, please [open a ticket on our issue tracker][issues-link].
* For questions not covered in the documentation, [visit our Discord server][discord-link].

---

### The latest release does not contain the executable.
You most likely downloaded the source code instead of the editor. On the [releases][releases-link] page, make sure to download the file named **tf2-hud-editor_X.X.zip** and extract it into a separate folder.

---

### The editor does not launch after downloading and extracting it.
Make sure to install the version of [Microsoft .NET 5.0 Runtime][runtime-link] inteded for running **desktop apps**. If you just installed it and the editor still does not launch, then restart your system.

---

### Customization changes are not being shown in-game.
To see your selected changes in-game, open the console and input `hud_reloadscheme`. This will refresh the HUD with your selected customizations.

:::note
Certain settings may require the game to be restarted, this mainly applies to color and main menu changes. If the game is running, a message will display notifying you that a game restart is required.
:::

---

### Error when applying or resetting HUD customizations.
Most errors you'll encounter will be caused by an outdated version of the HUD being installed. An outdated HUD may not have the latest changes that the editor would expect and when that happens, an error is returned. Best thing to do is reinstall the HUD through the editor and reapply the customizations.

---

### Access to the path ... temp.zip is denied.
Try running the editor as administrator.

If that didn't work, double-check that your antivirus program isn't denying access to the editor program. [For example,][example-avast-issue] Avast has been known to prevent the editor from creating files in certain paths.

<!-- MARKDOWN LINKS -->
[issues-link]: https://github.com/CriticalFlaw/TF2HUD.Editor/issues
[discord-link]: https://discord.gg/hTdtK9vBhE
[releases-link]: https://github.com/CriticalFlaw/TF2HUD.Editor/releases
[runtime-link]: https://dotnet.microsoft.com/download/dotnet/5.0/runtime
[example-avast-issue]: https://github.com/CriticalFlaw/TF2HUD.Editor/issues/107
