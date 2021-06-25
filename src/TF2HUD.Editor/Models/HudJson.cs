using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace HUDEditor.Models
{
    public class HudJson
    {
        [JsonPropertyName("Thumbnail")] public string Thumbnail;
        [JsonPropertyName("Background")] public string Background;
        [JsonPropertyName("Controls")] public Dictionary<string, Controls[]> Controls;
        [JsonPropertyName("CustomizationsFolder")] public string CustomizationsFolder;
        [JsonPropertyName("EnabledFolder")] public string EnabledFolder;
        [JsonPropertyName("Layout")] public string[] Layout;
        [JsonPropertyName("Links")] public Links Links;
        [JsonPropertyName("Maximize")] public bool Maximize;
        [JsonPropertyName("Opacity")] public double Opacity = 0.5;
    }

    public class Links
    {
        [JsonPropertyName("Discord")] public string Discord;
        [JsonPropertyName("GitHub")] public string GitHub;
        [JsonPropertyName("HudsTF")] public string HudsTF;
        [JsonPropertyName("Issue")] public string Issue;
        [JsonPropertyName("Steam")] public string Steam;
        [JsonPropertyName("Update")] public string Update;
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

    public class GitJson
    {
        [JsonPropertyName("name")] public string Name;
        [JsonPropertyName("size")] public int Size;
    }
}