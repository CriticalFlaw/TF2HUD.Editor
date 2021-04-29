using System;
using System.IO;
using System.Windows;

namespace HUDEditor.Classes
{
    /// <summary>
    ///     <para>Handles the options and priority of a HUD's custom background</para>
    ///     <c>this.BackgroundManager = new BackgroundManager();</c>
    ///     <c>this.BackgroundManager.SetStockBackgrounds();</c>
    ///     <c>this.BackgroundManager.SetCustomBackground("background.png");</c>
    ///     <c>this.BackgroundManager.Apply();</c>
    ///     <para>
    ///         In this example, we set stock backgrounds AND a custom background.
    ///         When calling the apply method, the BackgroundManager will override
    ///         the stock backgrounds with the custom background and apply changes
    ///         to the HUD
    ///     </para>
    /// </summary>
    internal class HUDBackground
    {
        private readonly string HUDFolderPath;
        private bool useStockBackgrounds;
        private bool useHUDBackground;
        private string HUDImagePath;
        private bool useCustomBackground;
        private string customImagePath;

        public HUDBackground(string hudPath)
        {
            HUDFolderPath = hudPath;
        }

        public void SetStockBackgrounds(bool enable)
        {
            MainWindow.Logger.Info("BackgroundManager: Setting stock backgrounds");
            useStockBackgrounds = enable;
        }

        public void SetHUDBackground(string imagePath)
        {
            MainWindow.Logger.Info($"BackgroundManager: Setting HUD Background to {imagePath}");
            useHUDBackground = true;
            HUDImagePath = imagePath;
        }

        public void SetCustomBackground(string imagePath)
        {
            if (imagePath == "") return;
            MainWindow.Logger.Info($"BackgroundManager: Setting custom Background to {imagePath}");
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

                if (!Directory.Exists(disabledFolder))
                {
                    Directory.CreateDirectory(disabledFolder);
                    foreach (var filePath in Directory.GetFiles(consoleFolder))
                    {
                        File.Move(filePath, $"{disabledFolder}\\{filePath.Split("\\")[^1]}", true);
                    }
                }

                if (useCustomBackground && !string.IsNullOrWhiteSpace(customImagePath))
                {
                    var converter = new VTF(MainWindow.HudPath.Replace("\\tf\\custom\\", string.Empty));

                    var output = $"{consoleFolder}\\background_upward.vtf";

                    if (!Uri.TryCreate(customImagePath, UriKind.Absolute, out var path)) return; 
                    converter.Convert(customImagePath, output);

                    File.Copy(output, output.Replace("background_upward", "background_upward_widescreen"), true);
                    File.Copy("Resources\\chapterbackgrounds.txt", $"{HUDFolderPath}\\scripts\\chapterbackgrounds.txt", true);
                }
                else
                {
                    if (useHUDBackground)
                    {
                        // Copy enabled background to console folder
                        if (File.Exists($"{disabledFolder}\\{HUDImagePath}.vtf"))
                            File.Copy($"{disabledFolder}\\{HUDImagePath}.vtf", $"{consoleFolder}\\background_upward.vtf", true);

                        if (File.Exists($"{disabledFolder}\\{HUDImagePath}_widescreen.vtf"))
                            File.Copy($"{disabledFolder}\\{HUDImagePath}.vtf", $"{consoleFolder}\\background_upward_widescreen.vtf", true);

                        File.Copy("Resources\\chapterbackgrounds.txt", $"{HUDFolderPath}\\scripts\\chapterbackgrounds.txt");
                    }
                    else if (useStockBackgrounds)
                    {
                        // Delete custom background
                        File.Delete($"{consoleFolder}\\background_upward.vtf");
                        File.Delete($"{consoleFolder}\\background_upward_widescreen.vtf");
                        File.Delete($"{HUDFolderPath}\\scripts\\chapterbackgrounds.txt");
                    }
                    else
                    {
                        // Restore console folder
                        foreach (var filePath in Directory.GetFiles(disabledFolder))
                            File.Move(filePath, $"{consoleFolder}\\{filePath.Split("\\")[^1]}", true);
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, ex.Message);
            }
        }
    }
}