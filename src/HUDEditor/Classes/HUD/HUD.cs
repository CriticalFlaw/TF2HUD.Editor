﻿using System;
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
    public string SteamUrl { get; set; }
    public string DiscordUrl { get; set; }
    public Dictionary<string, Models.Controls[]> ControlOptions;
    public readonly string[] LayoutOptions;
    public List<string> DirtyControls;
    public bool Unique;
    public readonly bool InstallCrosshairs;
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
        Name = schema.Name ?? name;
        Settings = new HUDSettings(Name);
        Opacity = schema.Opacity;
        Maximize = schema.Maximize;
        Thumbnail = schema.Thumbnail;
        Background = schema.Background;
        Description = schema.Description;
        Author = schema.Author;
        CustomizationsFolder = schema.CustomizationsFolder ?? string.Empty;
        EnabledFolder = schema.EnabledFolder ?? string.Empty;
        DownloadUrl = schema.Links.Update;
        GitHubUrl = schema.Links.GitHub ?? string.Empty;
        TF2HudsUrl = schema.Links.TF2Huds ?? string.Empty;
        ComfigHudsUrl = schema.Links.ComfigHuds ?? string.Empty;
        SteamUrl = schema.Links.Steam ?? string.Empty;
        DiscordUrl = schema.Links.Discord ?? string.Empty;
        ControlOptions = schema.Controls;
        LayoutOptions = schema.Layout;
        DirtyControls = new List<string>();
        Unique = isUnique;
        InstallCrosshairs = schema.InstallCrosshairs;
        Screenshots = schema.Screenshots;
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
        App.Logger.Info($"Changing {Name} to Preset-{Settings.Preset}");
    }

    /// <summary>
    /// Resets all user settings to their default values as defined in the schema.
    /// </summary>
    public void ResetAll()
    {
        foreach (var section in ControlOptions.Keys)
            for (var x = 0; x < ControlOptions[section].Length; x++)
                ResetControl(ControlOptions[section][x]);
    }

    /// <summary>
    /// Resets a group of user settings to their default values as defined in the schema.
    /// </summary>
    private void ResetSection(string selection)
    {
        foreach (var section in ControlOptions[selection])
            ResetControl(section);
    }

    /// <summary>
    /// Resets a user setting to its default value as defined in the schema.
    /// </summary>
    private void ResetControl(Models.Controls control)
    {
        try
        {
            switch (control.Control)
            {
                case CheckBox check:
                    if (bool.TryParse(control.Value, out var value))
                        check.IsChecked = value;
                    App.Logger.Info($"Reset {control.Name} to {value}");
                    break;

                case ColorPicker color:
                    color.Color = Utilities.ConvertToColor(control.Value);
                    App.Logger.Info($"Reset {control.Name} to {color.Color}");
                    break;

                case ComboBox combo:
                    var index = 0;
                    // If we're dealing with crosshairs, find the correct index.
                    if (((ComboBoxItem)combo.Items[0]).Classes.Contains("CrosshairBoxItem"))
                    {
                        var xhair = Utilities.CrosshairStyles.IndexOf(control.Value);
                        index = (xhair >= 0) ? xhair : index;
                    }
                    else
                        index = int.Parse(control.Value);
                    combo.SelectedIndex = index;
                    App.Logger.Info($"Reset {control.Name} to \"{control.Value}\"");
                    break;

                case NumericUpDown integer:
                    integer.Text = control.Value;
                    App.Logger.Info($"Reset {control.Name} to \"{control.Value}\"");
                    break;

                case TextBox text:
                    text.Text = control.Value;
                    App.Logger.Info($"Reset {control.Name} to \"{control.Value}\"");
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