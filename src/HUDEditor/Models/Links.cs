using System.Text.Json.Serialization;

namespace HUDEditor.Models;

public class Links
{
    [JsonPropertyName("Discord")] public string Discord;
    [JsonPropertyName("GitHub")] public string GitHub;
    [JsonPropertyName("TF2Huds")] public string TF2Huds;
    [JsonPropertyName("ComfigHuds")] public string ComfigHuds;
    [JsonPropertyName("Steam")] public string Steam;
    [JsonPropertyName("Update")] public string Update;  // TODO: Change to Download after user migration to 4.0
}