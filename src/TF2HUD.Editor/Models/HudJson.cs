using System.Collections.Generic;
using System.Text.Json.Serialization;

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
}