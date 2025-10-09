using System.Text.Json.Serialization;

namespace HUDEditor.Models;

public class WriteFile
{
    [JsonPropertyName("FileName")] public string FileName;
    [JsonPropertyName("TrueText")] public string TrueText;
    [JsonPropertyName("FalseText")] public string FalseText;
}