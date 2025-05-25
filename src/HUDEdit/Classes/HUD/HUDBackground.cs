using HUDEdit.Views;
using System;
using System.IO;

namespace HUDEdit.Classes;

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
    private readonly string hudFolderPath;
    private string hudImagePath;
    private string customImagePath;
    private bool useCustomBackground;
    private bool useHUDBackground;
    private bool useStockBackgrounds;

    public HUDBackground(string hudPath)
    {
        hudFolderPath = hudPath;
    }

    public void SetStockBackground(bool enable)
    {
        App.Logger.Info("Changing HUD background to: stock");
        useStockBackgrounds = enable;
    }

    public void SetHUDBackground(string imagePath)
    {
        App.Logger.Info($"Changing HUD background to: {imagePath}");
        hudImagePath = imagePath;
        useHUDBackground = true;
    }

    public void SetCustomBackground(string imagePath)
    {
        if (imagePath == "") return;
        App.Logger.Info($"Setting custom background to: {imagePath}");
        customImagePath = imagePath;
        useCustomBackground = true;
    }

    public void ApplyBackground()
    {
        try
        {
            var consoleFolder = hudFolderPath + "materials/console/";
            var disabledFolder = consoleFolder + "_disabled";

            if (!Directory.Exists(consoleFolder))
                Directory.CreateDirectory(consoleFolder);

            if (!Directory.Exists(disabledFolder))
                Directory.CreateDirectory(disabledFolder);

            if (useCustomBackground && !string.IsNullOrWhiteSpace(customImagePath))
            {
                // Check that the supplied image path is valid
                App.Logger.Info($"Validating {customImagePath}");
                if (!Uri.TryCreate(customImagePath, UriKind.Absolute, out _)) return;

                // Move all existing files to the disabled folder.
                foreach (var filePath in Directory.GetFiles(consoleFolder))
                {
                    App.Logger.Info($"Moving \"{filePath}\" to \"{disabledFolder}\"");
                    File.Move(filePath, $"{disabledFolder}/{filePath.Split("/")[^1]}", true);
                }

                // Convert the provided image into a VTF format.
                var converter = new VTF(MainWindow.HudPath.Replace("/tf/custom/", string.Empty));
                var output = $"{consoleFolder}/background_upward.vtf";
                App.Logger.Info($"Converting \"{customImagePath}\" to \"{output}\"");
                converter.Convert(customImagePath, output);

                // Copy the generated file to the backgrounds folder.
                App.Logger.Info($"Copying \"{output}\" to \"{hudFolderPath}\"");
                File.Copy(output, output.Replace("background_upward", "background_upward_widescreen"), true);

                // Ensure chapterbackgrounds.txt exists
                Utilities.CreateChapterBackgroundsFile($"{hudFolderPath}/scripts");
            }
            else
            {
                if (useHUDBackground)
                {
                    // Copy enabled background to console folder
                    if (File.Exists($"{disabledFolder}/{hudImagePath}.vtf"))
                    {
                        App.Logger.Info($"Copying \"{disabledFolder}/{hudImagePath}\" to \"{consoleFolder}\"");
                        File.Copy($"{disabledFolder}/{hudImagePath}.vtf", $"{consoleFolder}/background_upward.vtf", true);
                    }

                    if (File.Exists($"{disabledFolder}/{hudImagePath}_widescreen.vtf"))
                    {
                        App.Logger.Info($"Copying \"{disabledFolder}/{hudImagePath}\" to \"{consoleFolder}\"");
                        File.Copy($"{disabledFolder}/{hudImagePath}.vtf", $"{consoleFolder}/background_upward_widescreen.vtf", true);
                    }

                    // Ensure chapterbackgrounds.txt exists
                    Utilities.CreateChapterBackgroundsFile($"{hudFolderPath}/scripts");
                }
                else if (useStockBackgrounds)
                {
                    App.Logger.Info($"Disable VTF/VMT files");
                    foreach (var filePath in Directory.GetFiles(consoleFolder))
                    {
                        if (filePath.EndsWith(".vtf"))
                        {
                            App.Logger.Info($"Converting {filePath} to BAK format");
                            File.Move(filePath, filePath.Replace(".vtf", ".bak"), true);
                        }
                        else if (filePath.EndsWith(".vmt"))
                        {
                            App.Logger.Info($"Converting {filePath} to TEMP format");
                            File.Move(filePath, filePath.Replace(".vmt", ".temp"), true);
                        }
                    }
                }
                else
                {
                    // Restore console folder
                    if (!Directory.Exists(disabledFolder)) return;
                    foreach (var filePath in Directory.GetFiles(disabledFolder))
                    {
                        App.Logger.Info($"Copying \"{filePath}\" to \"{consoleFolder}\"");
                        File.Move(filePath, $"{consoleFolder}/{filePath.Split("/")[^1]}", true);
                    }

                    Directory.Delete(disabledFolder);

                    App.Logger.Info($"Enable VTF/VMT files");
                    foreach (var filePath in Directory.GetFiles(consoleFolder))
                    {
                        if (filePath.EndsWith(".bak"))
                        {
                            App.Logger.Info($"Converting {filePath} to VTF format");
                            File.Move(filePath, filePath.Replace(".bak", ".vtf"), true);
                        }
                        else if (filePath.EndsWith(".temp"))
                        {
                            App.Logger.Info($"Converting {filePath} to VMT format");
                            File.Move(filePath, filePath.Replace(".temp", ".vmt"), true);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            _ = Utilities.ShowMessageBox(e.Message, MsBox.Avalonia.Enums.Icon.Error);
        }
    }
}