using System.Text.Json.Serialization;

namespace HUDEditor.Models;

public class GitJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sha")]
    public string SHA { get; set; } = string.Empty;

    [JsonPropertyName("download_url")]
    public string Download { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}