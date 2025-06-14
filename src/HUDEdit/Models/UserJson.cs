using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HUDEdit.Models;

public class UserJson
{
    [JsonPropertyName("Presets")] public Dictionary<string, Preset> Presets { get; set; } = [];
    [JsonPropertyName("Settings")] public List<Setting> Settings { get; set; } = [];
}