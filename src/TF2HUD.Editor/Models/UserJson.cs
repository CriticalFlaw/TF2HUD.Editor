using HUDEditor.Classes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HUDEditor.Models
{
    public class UserJson
    {
        [JsonPropertyName("Presets")] public Dictionary<string, Preset> Presets { get; set; } = new();
        [JsonPropertyName("Settings")] public List<Setting> Settings { get; set; } = new();
    }

    public class Setting
    {
        [JsonPropertyName("HUD")] public string Hud { get; set; }
        [JsonPropertyName("Name")] public string Name { get; set; }
        [JsonPropertyName("Type")] public string Type { get; set; }
        [JsonPropertyName("Value")] public string Value { get; set; }
        [JsonPropertyName("Preset")] public Preset Preset { get; set; }
    }
}