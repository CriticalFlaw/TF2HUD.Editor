using System.Text.Json.Serialization;

namespace Shared.Models;

public class RenameFile
{
    [JsonPropertyName("NewName")] public string NewName;
    [JsonPropertyName("OldName")] public string OldName;
}