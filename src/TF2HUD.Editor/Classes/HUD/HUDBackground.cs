using log4net;
using System;
using System.IO;
using System.Windows;

namespace HUDEditor.Classes
{
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
        private readonly string HUDFolderPath;
        private readonly ILog _logger;
        private readonly INotifier _notifier;
        private readonly VTF _vtf;
        private string customImagePath;
        private string HUDImagePath;
        private bool useCustomBackground;
        private bool useHUDBackground;
        private bool useStockBackgrounds;

        public HUDBackground(string hudPath, ILog logger, INotifier notifier, VTF vtf)
        {
            HUDFolderPath = hudPath;
            _logger = logger;
            _notifier = notifier;
            _vtf = vtf;
        }

        public void SetStockBackgrounds(bool enable)
        {
            _logger.Info("Changing HUD background to: stock.");
            useStockBackgrounds = enable;
        }

        public void SetHUDBackground(string imagePath)
        {
            _logger.Info($"Changing HUD background to: {imagePath}");
            useHUDBackground = true;
            HUDImagePath = imagePath;
        }

        public void SetCustomBackground(string imagePath)
        {
            if (imagePath == "") return;
            _logger.Info($"Setting custom background to: {imagePath}");
            useCustomBackground = true;
            customImagePath = imagePath;
        }

        public void Apply()
        {
            try
            {
                var consoleFolder = HUDFolderPath + "materials\\console\\";
                var disabledFolder = consoleFolder + "_disabled";

                Directory.CreateDirectory(consoleFolder);
                Directory.CreateDirectory(disabledFolder);

                if (useCustomBackground && !string.IsNullOrWhiteSpace(customImagePath))
                {
                    ApplyCustomBackground(consoleFolder, disabledFolder);
                }
                else if (useHUDBackground)
                {
                    ApplyHUDBackground(consoleFolder, disabledFolder);
                }
                else if (useStockBackgrounds)
                {
                    DisableBackground(consoleFolder);
                }
                else
                {
                    EnablePreviouslyDisabledBackground(consoleFolder, disabledFolder);
                }
            }
            catch (Exception e)
            {
                _notifier.ShowMessageBox(MessageBoxImage.Error, e.Message);
            }
        }

        private void EnablePreviouslyDisabledBackground(string consoleFolder, string disabledFolder)
        {
            // Restore console folder
            if (!Directory.Exists(disabledFolder)) return;
            foreach (var filePath in Directory.GetFiles(disabledFolder))
            {
                _logger.Info($"Copying {filePath} to {consoleFolder}");
                File.Move(filePath, $"{consoleFolder}\\{filePath.Split("\\")[^1]}", true);
            }

            Directory.Delete(disabledFolder);

            foreach (var filePath in Directory.GetFiles(consoleFolder))
                if (filePath.EndsWith(".bak"))
                {
                    _logger.Info($"Enabling {filePath} (.BAK => .VTF)");
                    File.Move(filePath, filePath.Replace(".bak", ".vtf"), true);
                }
                else if (filePath.EndsWith(".temp"))
                {
                    _logger.Info($"Enabling {filePath} (.TEMP => .VMT)");
                    File.Move(filePath, filePath.Replace(".temp", ".vmt"), true);
                }
        }

        private void DisableBackground(string consoleFolder)
        {
            foreach (var filePath in Directory.GetFiles(consoleFolder))
                if (filePath.EndsWith(".vtf"))
                {
                    _logger.Info($"Disabling {filePath} (.VTF => .BAK)");
                    File.Move(filePath, filePath.Replace(".vtf", ".bak"), true);
                }
                else if (filePath.EndsWith(".vmt"))
                {
                    _logger.Info($"Disabling {filePath} (.VMT => .TEMP)");
                    File.Move(filePath, filePath.Replace(".vmt", ".temp"), true);
                }
        }

        private void ApplyHUDBackground(string consoleFolder, string disabledFolder)
        {
            // Copy enabled background to console folder
            if (File.Exists($"{disabledFolder}\\{HUDImagePath}.vtf"))
            {
                _logger.Info($"Copying {disabledFolder}\\{HUDImagePath} to {consoleFolder}");
                File.Copy($"{disabledFolder}\\{HUDImagePath}.vtf", $"{consoleFolder}\\background_upward.vtf", true);
            }

            if (File.Exists($"{disabledFolder}\\{HUDImagePath}_widescreen.vtf"))
            {
                _logger.Info($"Copying {disabledFolder}\\{HUDImagePath} to {consoleFolder}");
                File.Copy($"{disabledFolder}\\{HUDImagePath}.vtf", $"{consoleFolder}\\background_upward_widescreen.vtf", true);
            }

            _logger.Info($"Copying chapterbackgrounds.txt to {HUDFolderPath}\\scripts");
            File.Copy("Resources\\chapterbackgrounds.txt", $"{HUDFolderPath}\\scripts\\chapterbackgrounds.txt", true);
        }

        private void ApplyCustomBackground(string consoleFolder, string disabledFolder)
        {
            // Check that the supplied image path is valid
            _logger.Info($"Validating image path: {customImagePath}");
            if (!Uri.TryCreate(customImagePath, UriKind.Absolute, out _)) return;

            // Move all existing files to the disabled folder.
            foreach (var filePath in Directory.GetFiles(consoleFolder))
            {
                _logger.Info($"Moving {filePath} to {disabledFolder}");
                File.Move(filePath, $"{disabledFolder}\\{filePath.Split("\\")[^1]}", true);
            }

            // Convert the provided image into a VTF format.
            var tf2Path = MainWindow.HudPath.Replace("\\tf\\custom\\", string.Empty);
            var output = $"{consoleFolder}\\background_upward.vtf";
            _logger.Info($"Converting image to VTF: {output}");
            _vtf.Convert(tf2Path, customImagePath, output);

            // Copy the generated file to the backgrounds folder.
            _logger.Info($"Copying {output} to {HUDFolderPath}");
            File.Copy(output, output.Replace("background_upward", "background_upward_widescreen"), true);
            File.Copy("Resources\\chapterbackgrounds.txt", $"{HUDFolderPath}\\scripts\\chapterbackgrounds.txt", true);
        }
    }
}