using System.Text.Json.Serialization;

namespace HUDEditor.Models
{
    public class SteamModel
    {
        [JsonPropertyName("path")] public string Path { get; set; }
        [JsonPropertyName("label")] public string Label { get; set; }
    }
}