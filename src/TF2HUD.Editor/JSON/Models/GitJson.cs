using System.Text.Json.Serialization;

namespace TF2HUD.Editor.JSON
{
    public class GitJson
    {
        [JsonPropertyName("download_url")] public string DownloadUrl;

        [JsonPropertyName("git_url")] public string GitUrl;

        [JsonPropertyName("html_url")] public string HtmlUrl;

        [JsonPropertyName("name")] public string Name;

        [JsonPropertyName("path")] public string Path;

        [JsonPropertyName("sha")] public string Sha;

        [JsonPropertyName("size")] public int Size;

        [JsonPropertyName("type")] public string Type;

        [JsonPropertyName("url")] public string Url;
    }
}