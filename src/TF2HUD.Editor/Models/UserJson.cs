using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HUDEditor.Models
{
    public class UserJson
    {
        [JsonPropertyName("Settings")] public List<Setting> Settings { get; set; }
    }

    public class Setting
    {
        [JsonPropertyName("HUD")] public string HUD { get; set; }
        [JsonPropertyName("Name")] public string Name { get; set; }
        [JsonPropertyName("Type")] public string Type { get; set; }
        [JsonPropertyName("Value")] public string Value { get; set; }
    }
}