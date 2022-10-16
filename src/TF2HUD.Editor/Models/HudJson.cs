using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;

namespace HUDEditor.Models
{
    public class HudJson
    {
        [JsonPropertyName("Author")] public string Author;
        [JsonPropertyName("Background")] public string Background;
        [JsonPropertyName("Controls")] public Dictionary<string, Controls[]> Controls;
        [JsonPropertyName("CustomizationsFolder")] public string CustomizationsFolder;
        [JsonPropertyName("Description")] public string Description;
        [JsonPropertyName("EnabledFolder")] public string EnabledFolder;
        [JsonPropertyName("Layout")] public string[] Layout;
        [JsonPropertyName("Links")] public Links Links;
        [JsonPropertyName("Maximize")] public bool Maximize;
        [JsonPropertyName("Name")] public string Name;
        [JsonPropertyName("Opacity")] public double Opacity = 0.5;
        [JsonPropertyName("Screenshots")] public string[] Screenshots;
        [JsonPropertyName("Thumbnail")] public string Thumbnail;
        [JsonPropertyName("InstallCrosshairs")] public bool InstallCrosshairs;
    }

    public class Links
    {
        [JsonPropertyName("Discord")] public string Discord;
        [JsonPropertyName("GitHub")] public string GitHub;
        [JsonPropertyName("HudsTF")] public string HudsTf;
        [JsonPropertyName("Steam")] public string Steam;
        [JsonPropertyName("Download")] public Download[] Download;
    }

    public class Controls
    {
        [JsonPropertyName("ComboFiles")] public string[] ComboFiles;
        public UIElement Control;
        [JsonPropertyName("FileName")] public string FileName;
        [JsonPropertyName("Files")] public JObject Files;
        [JsonPropertyName("Increment")] public int Increment = 2;
        [JsonPropertyName(";")] public string Label;
        [JsonPropertyName("Maximum")] public int Maximum = 30;
        [JsonPropertyName("Minimum")] public int Minimum = 10;
        [JsonPropertyName("Name")] public string Name;
        [JsonPropertyName("Options")] public Option[] Options;
        [JsonPropertyName("Preview")] public string Preview;
        [JsonPropertyName("Pulse")] public bool Pulse;
        [JsonPropertyName("RenameFile")] public RenameFile RenameFile;
        [JsonPropertyName("Restart")] public bool Restart;
        [JsonPropertyName("Shadow")] public bool Shadow;
        [JsonPropertyName("Special")] public string Special;
        [JsonPropertyName("SpecialParameters")] public string[] SpecialParameters;
        [JsonPropertyName("Tooltip")] public string Tooltip;
        [JsonPropertyName("Type")] public string Type;
        [JsonPropertyName("Value")] public string Value = "0";
        [JsonPropertyName("Width")] public int Width;
    }

    public class Download
    {
        public string Source { get; set; }
        public string Link { get; set; }
    }

    public class RenameFile
    {
        [JsonPropertyName("NewName")] public string NewName;
        [JsonPropertyName("OldName")] public string OldName;
    }

    public class Option
    {
        [JsonPropertyName("FileName")] public string FileName;
        [JsonPropertyName("Files")] public JObject Files;
        [JsonPropertyName("Label")] public string Label;
        [JsonPropertyName("RenameFile")] public RenameFile RenameFile;
        [JsonPropertyName("Special")] public string Special;
        [JsonPropertyName("SpecialParameters")] public string[] SpecialParameters;
        [JsonPropertyName("Value")] public string Value;
    }
}