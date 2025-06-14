using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace HUDEdit.Models;

public class Option
{
    [JsonPropertyName("FileName")] public string FileName;
    [JsonPropertyName("Files")] public JObject Files;
    [JsonPropertyName("Label")] public string Label;
    [JsonPropertyName("RenameFile")] public RenameFile RenameFile;
    [JsonPropertyName("Special")] public string Special;
    [JsonPropertyName("SpecialParameters")] public string[] SpecialParameters;
    [JsonPropertyName("Value")] public string Value;
}