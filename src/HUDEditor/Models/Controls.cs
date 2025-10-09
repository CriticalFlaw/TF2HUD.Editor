using System.Text.Json.Serialization;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;

namespace HUDEditor.Models;

public class Controls
{
    [JsonPropertyName("ComboFiles")] public string[] ComboFiles;
    [JsonPropertyName("ComboDirectories")] public string[] ComboDirectories;
    public Control Control;
    [JsonPropertyName("FileName")] public string FileName;
    [JsonPropertyName("Files")] public JObject Files;
    [JsonPropertyName("Increment")] public int Increment = 2;
    [JsonPropertyName(";")] public string Label;
    [JsonPropertyName("Maximum")] public int Maximum = 30;
    [JsonPropertyName("Minimum")] public int Minimum = 10;
    [JsonPropertyName("Name")] public string Name;
    [JsonPropertyName("Options")] public Option[] Options;
    [JsonPropertyName("Preview")] public string Preview;
    [JsonPropertyName("Pulse")] public bool Pulse;
    [JsonPropertyName("RenameFile")] public RenameFile RenameFile;
    [JsonPropertyName("Restart")] public bool Restart;
    [JsonPropertyName("Shadow")] public bool Shadow;
    [JsonPropertyName("Special")] public string Special;
    [JsonPropertyName("SpecialParameters")] public string[] SpecialParameters;
    [JsonPropertyName("Tooltip")] public string Tooltip;
    [JsonPropertyName("Type")] public string Type;
    [JsonPropertyName("Value")] public string Value = "0";
    [JsonPropertyName("Width")] public int Width;
    [JsonPropertyName("WriteFile")] public WriteFile WriteFile;
}