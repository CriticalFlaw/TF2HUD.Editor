using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Classes
{
    /// <summary>
    ///     <para>Handles the options and priority of a HUD's custom background</para>
    ///     <code>this.BackgroundManager = new BackgroundManager();</code>
    ///     <code>this.BackgroundManager.SetStockBackgrounds();</code>
    ///     <code>this.BackgroundManager.SetCustomBackground("background.png");</code>
    ///     <code>this.BackgroundManager.Apply();</code>
    ///     <para>
    ///         In this example, we set stock backgrounds AND a custom background.
    ///         When calling the apply method, the BackgroundManager will override
    ///         the stock backgrounds with the custom background and apply changes
    ///         to the HUD
    ///     </para>
    /// </summary>
    internal class BackgroundManager
    {
        private readonly string HUDFolderPath;
        private bool useStockBackgrounds;
        private bool useHUDBackground;
        private string HUDImagePath;
        private bool useCustomBackground;
        private string customImagePath;

        public BackgroundManager(string hudPath)
        {
            HUDFolderPath = hudPath;
        }

        public void SetStockBackgrounds(bool enable)
        {
            MainWindow.Logger.Info($"BackgroundManager: Setting stock backgrounds");
            useStockBackgrounds = true;
        }

        public void SetHUDBackground(string imagePath)
        {
            MainWindow.Logger.Info($"BackgroundManager: Setting HUD Background to {imagePath}");
            useHUDBackground = true;
            HUDImagePath = imagePath;
        }

        public void SetCustomBackground(string imagePath)
        {
            if (imagePath != "")
            {
                MainWindow.Logger.Info($"BackgroundManager: Setting custom Background to {imagePath}");
                useCustomBackground = true;
                customImagePath = imagePath;
            }
        }

        public void Apply()
        {
            try
            {
                var path = HUDFolderPath;
                var consoleFolder = path + "materials\\console\\";
                var disabledFolder = consoleFolder + "TF2HUD.Editor.DisabledBackgrounds";

                Directory.CreateDirectory(consoleFolder);

                if (!Directory.Exists(disabledFolder))
                {
                    Directory.CreateDirectory(disabledFolder);
                    foreach (var filePath in Directory.GetFiles(consoleFolder))
                    {
                        File.Move(filePath, $"{disabledFolder}\\{filePath.Split("\\")[^1]}", true);
                    }
                }

                if (useCustomBackground && customImagePath != "")
                {
                    var converter = new VTF(MainWindow.HudPath.Replace("\\tf\\custom\\", string.Empty));

                    var output = $"{consoleFolder}\\background_upward.vtf";
                    converter.Convert(customImagePath, output);

                    File.Copy(output, output.Replace("background_upward", "background_upward_widescreen"), true);
                    File.Copy("Resources\\chapterbackgrounds.txt", $"{HUDFolderPath}\\scripts\\chapterbackgrounds.txt", true);
                }
                else
                {
                    if (useHUDBackground)
                    {
                        if (File.Exists($"{disabledFolder}\\{HUDImagePath}.vtf"))
                            File.Copy($"{disabledFolder}\\{HUDImagePath}.vtf", $"{consoleFolder}\\background_upward.vtf", true);

                        if (File.Exists($"{disabledFolder}\\{HUDImagePath}_widescreen.vtf"))
                            File.Copy($"{disabledFolder}\\{HUDImagePath}.vtf", $"{consoleFolder}\\background_upward_widescreen.vtf", true);

                        File.Copy("Resources\\chapterbackgrounds.txt", $"{HUDFolderPath}\\scripts\\chapterbackgrounds.txt");
                    }
                    else if (useStockBackgrounds)
                    {
                        File.Delete($"{consoleFolder}\\background_upward.vtf");
                        File.Delete($"{consoleFolder}\\background_upward_widescreen.vtf");
                        File.Delete($"{HUDFolderPath}\\scripts\\chapterbackgrounds.txt");
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