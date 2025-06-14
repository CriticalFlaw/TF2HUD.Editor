using System.Text.Json.Serialization;

namespace HUDEdit.Models;

public class GitJson
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("sha")] public string SHA { get; set; }
    [JsonPropertyName("download_url")] public string Download { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; }
}