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
}