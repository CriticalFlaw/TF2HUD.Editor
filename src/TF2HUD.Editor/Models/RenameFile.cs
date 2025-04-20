using System.Text.Json.Serialization;

namespace HUDEditor.Models;

public class RenameFile
{
    [JsonPropertyName("NewName")] public string NewName;
    [JsonPropertyName("OldName")] public string OldName;
}