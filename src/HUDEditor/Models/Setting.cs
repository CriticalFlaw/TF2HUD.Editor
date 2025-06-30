using System.Text.Json.Serialization;

namespace HUDEditor.Models;

public class Setting
{
    [JsonPropertyName("HUD")] public string Hud { get; set; }
    [JsonPropertyName("Name")] public string Name { get; set; }
    [JsonPropertyName("Type")] public string Type { get; set; }
    [JsonPropertyName("Value")] public string Value { get; set; }
    [JsonPropertyName("Preset")] public Preset Preset { get; set; }
}