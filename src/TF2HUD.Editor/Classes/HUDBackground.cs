using System;
using System.IO;
using System.Windows;

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
        private string customImagePath;
        private bool useCustomBackground;
        private bool useStockBackgrounds;

        public BackgroundManager(string hudPath)
        {
            HUDFolderPath = hudPath;
        }

        public void SetStockBackgrounds(bool enable)
        {
            useStockBackgrounds = enable;
        }

        public void SetCustomBackground(string imagePath, bool enable)
        {
            useCustomBackground = enable;
            customImagePath = imagePath;
        }

        public void Apply()
        {
            try
            {
                var path = HUDFolderPath;

                // Revert everything back to normal before changing the name extension.
                var directoryPath = new DirectoryInfo(path + "\\materials\\console");
                foreach (var file in directoryPath.GetFiles())
                {
                    var target = file.FullName;
                    if (file.Name.EndsWith("bak"))
                    {
                        target = target.Replace("bak", "vtf");
                        File.Move(file.FullName, target, true);
                    }

                    if (file.Name.EndsWith("temp"))
                    {
                        target = target.Replace("temp", "vmt");
                        File.Move(file.FullName, target, true);
                    }
                }

                // Do the same for the chapter backgrounds file as well.
                var chapterBackgrounds = path + "\\scripts\\chapterbackgrounds.bak";
                if (File.Exists(chapterBackgrounds))
                    File.Move(chapterBackgrounds, chapterBackgrounds.Replace(".bak", ".txt"), true);

                // If we're not enabling stock background, then skip this process.
                if (!useStockBackgrounds) return;

                // Rename the file extensions so that the game does not use them.
                foreach (var file in directoryPath.GetFiles())
                {
                    var target = file.FullName;
                    if (file.Name.EndsWith("vtf"))
                    {
                        target = target.Replace("vtf", "bak");
                        File.Move(file.FullName, target, true);
                    }

                    if (file.Name.EndsWith("vmt"))
                    {
                        target = target.Replace("vmt", "temp");
                        File.Move(file.FullName, target, true);
                    }
                }

                if (File.Exists(chapterBackgrounds.Replace(".bak", ".txt")))
                    File.Move(chapterBackgrounds.Replace(".bak", ".txt"), chapterBackgrounds, true);
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, ex.Message);
            }
        }

        /// <summary>
        ///     Generate or remove a custom background.
        /// </summary>
        public bool ApplyCustomBackground()
        {
            try
            {
                customImagePath = (string.IsNullOrWhiteSpace(customImagePath)) ? GetFilePathFromUser() : customImagePath;
                var converter = new VTF(MainWindow.HudPath.Replace("\\tf\\custom\\", string.Empty));
                var output = $"{HUDFolderPath}\\materials\\console\\background_upward.vtf";
                converter.Convert(customImagePath, output);
                File.Copy(output, output.Replace("background_upward", "background_upward_widescreen"), true);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Get the custom background image path from the user.
        /// </summary>
        /// <returns>String path to the new background image file.</returns>
        public string GetFilePathFromUser()
        {
            using var browser = new System.Windows.Forms.OpenFileDialog();
            browser.ShowDialog();
            return (string.IsNullOrWhiteSpace(browser.FileName)) ? null : browser.FileName;
        }
    }
}