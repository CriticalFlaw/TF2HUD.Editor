using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using HUDEditor.Models;

namespace HUDEditor.Classes;

public partial class HUD
{
    private Grid Controls = new();
    private HUDBackground HudBackground;
    private bool IsRendered;
    private string[][] Layout;

    #region HUD PROPERTIES

    public string Name { get; set; }
    public HUDSettings Settings { get; set; }
    public double Opacity { get; set; }
    public bool Maximize { get; set; }
    public string Thumbnail { get; set; }
    public Bitmap ThumbnailImage { get; set; }
    public string Background { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string CustomizationsFolder { get; set; }
    public string EnabledFolder { get; set; }
    public string DownloadUrl { get; set; }
    public string GitHubUrl { get; set; }
    public string TF2HudsUrl { get; set; }
    public string ComfigHudsUrl { get; set; }
    public string GameBananaUrl { get; set; }
    public string SteamUrl { get; set; }
    public string DiscordUrl { get; set; }
    public Dictionary<string, Models.Controls[]> ControlOptions;
    public readonly string[] LayoutOptions;
    public List<string> DirtyControls;
    public bool Unique;
    public readonly bool InstallCrosshairs;
    public string AppVersion { get; set; }
    public string[] Screenshots { get; set; }
    public List<Bitmap> ScreenshotImages { get; set; } = new();

    #endregion HUD PROPERTIES

    /// <summary>
    /// Initializes the HUD object with values from the schema.
    /// </summary>
    /// <param name="name">HUD object name.</param>
    /// <param name="schema">HUD schema contents.</param>
    /// <param name="isUnique">Marks the HUD as having unique customizations.</param>
    public HUD(string name, HudJson schema, bool isUnique)
    {
        Name = (!string.IsNullOrEmpty(schema.Name)) ? schema.Name : name;
        Settings = new HUDSettings(Name);
        Opacity = schema.Opacity;
        Maximize = schema.Maximize;
        Thumbnail = schema.Thumbnail ?? string.Empty;
        Background = schema.Background ?? string.Empty;
        Description = schema.Description ?? string.Empty;
        Author = schema.Author ?? string.Empty;
        CustomizationsFolder = schema.CustomizationsFolder ?? string.Empty;
        EnabledFolder = schema.EnabledFolder ?? string.Empty;

        // Links can be null on the schema, so use safe navigation + coalesce to empty.
        DownloadUrl = schema.Links?.Update ?? string.Empty;
        GitHubUrl = schema.Links?.GitHub ?? string.Empty;
        TF2HudsUrl = schema.Links?.TF2Huds ?? string.Empty;
        ComfigHudsUrl = schema.Links?.ComfigHuds ?? string.Empty;
        GameBananaUrl = schema.Links?.GameBanana ?? string.Empty;
        SteamUrl = schema.Links?.Steam ?? string.Empty;
        DiscordUrl = schema.Links?.Discord ?? string.Empty;

        // Collections from schema may be null; provide defaults to avoid CS8600/CS8602.
        ControlOptions = schema.Controls ?? new Dictionary<string, Models.Controls[]>();
        LayoutOptions = schema.Layout ?? Array.Empty<string>();
        DirtyControls = new List<string>();
        Unique = isUnique;
        InstallCrosshairs = schema.InstallCrosshairs;
        AppVersion = schema.AppVersion ?? string.Empty;
        Screenshots = schema.Screenshots ?? Array.Empty<string>();
        ScreenshotImages = new List<Bitmap>();
    }

    /// <summary>
    /// Changes the preset on a given HUD.
    /// </summary>
    /// <param name="preset"></param>
    public void SetPreset(Preset preset)
    {
        Settings.Preset = preset;
        IsRendered = false;
        Controls = new Grid();
        App.Logger.Info($"Changed {Name} to Preset-{Settings.Preset}");
    }

    /// <summary>
    /// Resets all user settings to their default values as defined in the schema.
    /// </summary>
    public void ResetAll()
    {
        if (ControlOptions == null) return;

        foreach (var section in ControlOptions.Keys)
            for (var x = 0; x < (ControlOptions[section]?.Length ?? 0); x++)
                ResetControl(ControlOptions[section][x]);
    }

    /// <summary>
    /// Resets a group of user settings to their default values as defined in the schema.
    /// </summary>
    private void ResetSection(string selection)
    {
        if (ControlOptions == null) return;

        if (!ControlOptions.TryGetValue(selection, out var controls) || controls == null) return;

        foreach (var section in controls)
            ResetControl(section);
    }

    /// <summary>
    /// Resets a user setting to its default value as defined in the schema.
    /// </summary>
    private void ResetControl(Models.Controls control)
    {
        try
        {
            if (control == null) return;

            var ctrl = control.Control;
            if (ctrl == null) return; // nothing to reset

            // Use type pattern matching with null guards
            switch (ctrl)
            {
                case CheckBox check:
                    bool cbValue = false;
                    if (!string.IsNullOrEmpty(control.Value))
                        bool.TryParse(control.Value, out cbValue);
                    check.IsChecked = cbValue;
                    App.Logger.Info($"Resetting {control.Name} to \"{cbValue}\"");
                    break;

                case ColorPicker color:
                    if (!string.IsNullOrEmpty(control.Value))
                    {
                        color.Color = Utilities.ConvertToColor(control.Value);
                        App.Logger.Info($"Resetting {control.Name} to \"{color.Color}\"");
                    }
                    else
                    {
                        App.Logger.Info($"Resetting {control.Name} skipped (no color value)");
                    }
                    break;

                case ComboBox combo:
                    var index = 0;
                    // If we're dealing with crosshairs, find the correct index.
                    ComboBoxItem firstItem = null;
                    if (combo.Items != null)
                    {
                        foreach (var it in combo.Items)
                        {
                            firstItem = it as ComboBoxItem;
                            break;
                        }
                    }

                    if (firstItem != null && firstItem.Classes.Contains("CrosshairBoxItem"))
                    {
                        var xhair = Utilities.CrosshairStyles.IndexOf(control.Value ?? string.Empty);
                        index = (xhair >= 0) ? xhair : index;
                    }
                    else
                    {
                        if (!int.TryParse(control.Value, out index))
                            index = 0;
                    }

                    combo.SelectedIndex = index;
                    App.Logger.Info($"Resetting {control.Name} to \"{control.Value}\"");
                    break;

                case NumericUpDown integer:
                    integer.Text = control.Value ?? string.Empty;
                    App.Logger.Info($"Resetting {control.Name} to \"{control.Value}\"");
                    break;

                case TextBox text:
                    text.Text = control.Value ?? string.Empty;
                    App.Logger.Info($"Resetting {control.Name} to \"{control.Value}\"");
                    break;
            }
        }
        catch (Exception e)
        {
            App.Logger.Error(e.Message);
            Console.WriteLine(e);
            throw;
        }
    }
}