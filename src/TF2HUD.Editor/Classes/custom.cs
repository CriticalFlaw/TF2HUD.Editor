using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Classes
{
    public class custom
    {
        /// <summary>
        ///     Enable/disable background image files by renaming their extensions.
        /// </summary>
        public static bool SetStockBackgrounds(string path, bool enable = false)
        {
            try
            {
                var directoryPath = new DirectoryInfo(path);

                // Revert all changes before reapplying them.
                foreach (var file in directoryPath.GetFiles())
                {
                    if (file.Name.EndsWith("bak"))
                        File.Move(file.FullName, file.FullName.Replace("bak", "vtf"));
                    if (file.Name.EndsWith("tmp"))
                        File.Move(file.FullName, file.FullName.Replace("tmp", "vmt"));
                    if (file.Name.EndsWith("temp"))
                        File.Move(file.FullName, file.FullName.Replace("temp", "txt"));
                }

                if (!enable) return true;
                foreach (var file in directoryPath.GetFiles())
                {
                    if (file.Name.EndsWith("vtf"))
                        File.Move(file.FullName, file.FullName.Replace("vtf", "bak"));
                    if (file.Name.EndsWith("vmt"))
                        File.Move(file.FullName, file.FullName.Replace("vmt", "tmp"));
                    if (file.Name.EndsWith("txt"))
                        File.Move(file.FullName, file.FullName.Replace("txt", "temp"));
                }

                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_menu_background,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Copy configuration files for transparent viewmodels.
        /// </summary>
        public static bool CopyTransparentViewmodelCfg(string path, bool enable = false)
        {
            try
            {
                // Copy the config file required for this feature
                if (!enable) return true;
                if (!Directory.Exists(path + "\\cfg"))
                    Directory.CreateDirectory(path + "\\cfg");
                File.Copy(Directory.GetCurrentDirectory() + "\\Resources\\hud.cfg",
                    string.Format(Resources.file_cfg, path), true);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_transparent_vm, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Generate or remove a user provided background.
        /// </summary>
        public static bool CustomBackground()
        {
            try
            {
                var converter = new VTF(MainWindow.HudPath);
                var output = string.Format(Resources.file_background, MainWindow.HudPath, MainWindow.HudSelection,
                    "background_upward.vtf");
                converter.Convert(Settings.Default.image_path, output);
                File.Copy(output, output.Replace("background_upward", "background_upward_widescreen"), true);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_seasonal_backgrounds, ex.Message);
                return false;
            }
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            using var browser = new OpenFileDialog();
            browser.ShowDialog();
            if (string.IsNullOrWhiteSpace(browser.FileName)) return;
            //LblImagePath.Content = browser.FileName;
            //Settings.Default.image_path = browser.FileName;
            //Settings.Default.Save();
            //PreviewImage.Source = new BitmapImage(new Uri(browser.FileName));
            //CbCustomBg.IsChecked = true;
            //SetBackgroundControls();
        }
    }
}