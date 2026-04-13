using System;
using System.Collections.Generic;
using System.IO;

namespace HUDEditor.Classes;

/// <summary>
///     <para>Handles the options and priority of a HUD's custom background</para>
///     <br><c>this.BackgroundManager = new BackgroundManager();</c></br>
///     <br><c>this.BackgroundManager.SetStockBackgrounds();</c></br>
///     <br><c>this.BackgroundManager.SetCustomBackground("background.png");</c></br>
///     <br><c>this.BackgroundManager.Apply();</c></br>
///     <para>
///         In this example, we set stock backgrounds AND a custom background.
///         When calling the apply method, the BackgroundManager will override the stock backgrounds with the custom background and apply changes to the HUD.
///     </para>
/// </summary>
internal class HUDBackground
{
    private readonly string _hudFolderPath;
    private string _hudImagePath;
    private Uri _customImagePath;
    private bool _useCustomBackground;
    private bool _useHUDBackground;
    private bool _useStockBackgrounds;

    public HUDBackground(string hudPath)
    {
        if (string.IsNullOrWhiteSpace(hudPath))
            throw new ArgumentException("HUD folder path cannot be empty.", nameof(hudPath));
        _hudFolderPath = hudPath;
    }

    public void SetStockBackground(bool enable)
    {
        App.Logger.Info($"Setting stock backgrounds: {enable}");
        _useStockBackgrounds = enable;
    }

    public void SetHUDBackground(string imagePath)
    {
        App.Logger.Info($"Setting HUD background to: {imagePath}");
        _hudImagePath = imagePath;
        _useHUDBackground = true;
    }

    public void SetCustomBackground(Uri imagePath)
    {
        if (imagePath is null) return;
        App.Logger.Info($"Setting custom background to: {imagePath}");
        _customImagePath = imagePath;
        _useCustomBackground = true;
    }

    public void ApplyBackground()
    {
        try
        {
            var consoleFolder = Path.Combine(_hudFolderPath, "materials", "console");
            var disabledFolder = Path.Combine(consoleFolder, "_disabled");

            Directory.CreateDirectory(consoleFolder);
            Directory.CreateDirectory(disabledFolder);

            if (_useCustomBackground && _customImagePath is not null)
                ApplyCustomBackground(consoleFolder, disabledFolder);
            else if (_useHUDBackground)
                ApplyHUDBackground(consoleFolder, disabledFolder);
            else if (_useStockBackgrounds)
                DisableCustomFiles(consoleFolder);
            else
                RestoreDisabledFiles(consoleFolder, disabledFolder);
        }
        catch (Exception e)
        {
            App.Logger.Error($"Failed to apply background: {e.Message}");
            _ = Utilities.ShowMessageBox(e.Message, MsBox.Avalonia.Enums.Icon.Error);
        }
    }

    private void ApplyCustomBackground(string consoleFolder, string disabledFolder)
    {
        // Move existing files — track what was moved for rollback.
        var moved = new List<(string From, string To)>();
        try
        {
            foreach (var filePath in Directory.GetFiles(consoleFolder))
            {
                var dest = Path.Combine(disabledFolder, Path.GetFileName(filePath));
                App.Logger.Info($"Moving: \"{filePath}\" → \"{dest}\"");
                File.Move(filePath, dest, overwrite: true);
                moved.Add((dest, filePath)); // reverse mapping for rollback
            }

            VTF.Convert(_customImagePath);
            Utilities.CreateChapterBackgroundsFile(_hudFolderPath);
        }
        catch
        {
            // Rollback: restore files we already moved
            foreach (var (from, to) in moved)
            {
                try
                {
                    App.Logger.Info($"Rolling back: \"{from}\" → \"{to}\"");
                    File.Move(from, to, overwrite: true);
                }
                catch (Exception re)
                {
                    App.Logger.Error($"Rollback failed for \"{from}\": {re.Message}");
                }
            }
            throw;
        }
    }

    private void ApplyHUDBackground(string consoleFolder, string disabledFolder)
    {
        if (string.IsNullOrWhiteSpace(_hudImagePath))
        {
            App.Logger.Warn("HUD image path is empty; skipping.");
            return;
        }

        CopyIfExists(
            Path.Combine(disabledFolder, $"{_hudImagePath}.vtf"),
            Path.Combine(consoleFolder, "background_upward.vtf"));

        CopyIfExists(
            Path.Combine(disabledFolder, $"{_hudImagePath}_widescreen.vtf"),
            Path.Combine(consoleFolder, "background_upward_widescreen.vtf"));

        Utilities.CreateChapterBackgroundsFile(_hudFolderPath);
    }

    private static void CopyIfExists(string source, string dest)
    {
        if (!File.Exists(source))
        {
            App.Logger.Warn($"Source not found, skipping copy: \"{source}\"");
            return;
        }
        App.Logger.Info($"Copying \"{source}\" → \"{dest}\"");
        File.Copy(source, dest, overwrite: true);
    }

    private static void DisableCustomFiles(string consoleFolder)
    {
        App.Logger.Info("Disabling VTF/VMT files for stock backgrounds.");
        foreach (var filePath in Directory.GetFiles(consoleFolder))
        {
            if (filePath.EndsWith(".vtf", StringComparison.OrdinalIgnoreCase))
                Utilities.RenameExtension(filePath, ".vtf", ".bak");
            else if (filePath.EndsWith(".vmt", StringComparison.OrdinalIgnoreCase))
                Utilities.RenameExtension(filePath, ".vmt", ".temp");
        }
    }

    private static void RestoreDisabledFiles(string consoleFolder, string disabledFolder)
    {
        if (!Directory.Exists(disabledFolder)) return;

        var moved = new List<(string From, string To)>();
        try
        {
            foreach (var filePath in Directory.GetFiles(disabledFolder))
            {
                var dest = Path.Combine(consoleFolder, Path.GetFileName(filePath));
                App.Logger.Info($"Restoring \"{filePath}\" → \"{dest}\"");
                File.Move(filePath, dest, overwrite: true);
                moved.Add((dest, filePath));
            }

            Utilities.DeleteDirectory(disabledFolder);

            App.Logger.Info("Re-enabling VTF/VMT files.");
            foreach (var filePath in Directory.GetFiles(consoleFolder))
            {
                if (filePath.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                    Utilities.RenameExtension(filePath, ".bak", ".vtf");
                else if (filePath.EndsWith(".temp", StringComparison.OrdinalIgnoreCase))
                    Utilities.RenameExtension(filePath, ".temp", ".vmt");
            }
        }
        catch
        {
            foreach (var (from, to) in moved)
            {
                try { File.Move(from, to, overwrite: true); }
                catch (Exception re) { App.Logger.Error($"Restore rollback failed for \"{from}\": {re.Message}"); }
            }
            throw;
        }
    }
}