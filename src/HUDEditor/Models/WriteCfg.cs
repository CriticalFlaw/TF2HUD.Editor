using System.Text.Json.Serialization;

namespace HUDEditor.Models;

public class WriteCfg
{
    [JsonPropertyName("FileName")] public string FileName;
    [JsonPropertyName("TrueText")] public string TrueText;
    [JsonPropertyName("FalseText")] public string FalseText;
}