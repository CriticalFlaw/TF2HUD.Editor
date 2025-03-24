using System.Text.Json.Serialization;

namespace Shared.Models;

public class Links
{
    [JsonPropertyName("Discord")] public string Discord;
    [JsonPropertyName("GitHub")] public string GitHub;
    [JsonPropertyName("TF2Huds")] public string TF2Huds;
    [JsonPropertyName("ComfigHuds")] public string ComfigHuds;
    [JsonPropertyName("Steam")] public string Steam;
    [JsonPropertyName("Download")] public Download[] Download;
}