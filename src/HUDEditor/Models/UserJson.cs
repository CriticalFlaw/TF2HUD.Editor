using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HUDEditor.Models;

public class UserJson
{
    [JsonPropertyName("Settings")]
    public List<Setting> Settings { get; set; } = [];

    [JsonPropertyName("Presets")]
    public Dictionary<string, Preset> Presets { get; set; } = [];

}

public class Setting
{
    [JsonPropertyName("HUD")]
    public string Hud { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("Preset")]
    public Preset Preset { get; set; } = Preset.A;
}

public enum Preset
{
    A,
    B,
    C,
    D
}