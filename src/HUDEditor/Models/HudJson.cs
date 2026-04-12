using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HUDEditor.Models;

public class HudJson
{
    [JsonPropertyName("Author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("Background")]
    public string Background { get; set; } = string.Empty;

    [JsonPropertyName("Controls")]
    public Dictionary<string, Controls[]> Controls { get; set; } = [];

    [JsonPropertyName("CustomizationsFolder")]
    public string CustomizationsFolder { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("EnabledFolder")]
    public string EnabledFolder { get; set; } = string.Empty;

    [JsonPropertyName("Layout")]
    public string[] Layout { get; set; } = [];

    [JsonPropertyName("Links")]
    public Links Links { get; set; } = new();

    [JsonPropertyName("Maximize")]
    public bool Maximize { get; set; } = false;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Opacity")]
    public double Opacity { get; set; } = 0.5;

    [JsonPropertyName("Screenshots")]
    public string[] Screenshots { get; set; } = [];

    [JsonPropertyName("Thumbnail")]
    public string Thumbnail { get; set; } = string.Empty;

    [JsonPropertyName("InstallCrosshairs")]
    public bool InstallCrosshairs { get; set; } = false;

    [JsonPropertyName("AppVersion")]
    public string AppVersion { get; set; } = string.Empty;
}

public class Controls
{
    public Control Control { get; set; } = new();

    [JsonPropertyName("ComboFiles")]
    public string[] ComboFiles { get; set; } = [];

    [JsonPropertyName("ComboDirectories")]
    public string[] ComboDirectories { get; set; } = [];

    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("Files")]
    public JObject Files { get; set; } = [];

    [JsonPropertyName("Increment")]
    public int Increment { get; set; } = 2;

    [JsonPropertyName(";")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("Maximum")]
    public int Maximum { get; set; } = 30;

    [JsonPropertyName("Minimum")]
    public int Minimum { get; set; } = 10;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Options")]
    public Option[] Options { get; set; } = [];

    [JsonPropertyName("Preview")]
    public string Preview { get; set; } = string.Empty;

    [JsonPropertyName("Pulse")]
    public bool Pulse { get; set; } = false;

    [JsonPropertyName("RenameFile")]
    public RenameFile RenameFile { get; set; } = new();

    [JsonPropertyName("Restart")]
    public bool Restart { get; set; } = false;

    [JsonPropertyName("Shadow")]
    public bool Shadow { get; set; } = false;

    [JsonPropertyName("Special")]
    public string Special { get; set; } = string.Empty;

    [JsonPropertyName("SpecialParameters")]
    public string[] SpecialParameters { get; set; } = [];

    [JsonPropertyName("Tooltip")]
    public string Tooltip { get; set; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Value")]
    public string Value { get; set; } = "0";

    [JsonPropertyName("Width")]
    public int Width { get; set; } = 0;

    [JsonPropertyName("WriteFile")]
    public WriteFile WriteFile { get; set; } = new();

    [JsonPropertyName("WriteCfg")]
    public WriteCfg WriteCfg { get; set; } = new();
}

public class Option
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("Files")]
    public JObject Files { get; set; } = new();

    [JsonPropertyName("Label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("RenameFile")]
    public RenameFile RenameFile { get; set; } = new();

    [JsonPropertyName("WriteFile")]
    public WriteFile WriteFile { get; set; } = new();

    [JsonPropertyName("WriteCfg")]
    public WriteCfg WriteCfg { get; set; } = new();

    [JsonPropertyName("Special")]
    public string Special { get; set; } = string.Empty;

    [JsonPropertyName("SpecialParameters")]
    public string[] SpecialParameters { get; set; } = [];

    [JsonPropertyName("Value")]
    public string Value { get; set; } = string.Empty;
}

public class Links
{
    [JsonPropertyName("Discord")]
    public string Discord { get; set; } = string.Empty;

    [JsonPropertyName("GitHub")]
    public string GitHub { get; set; } = string.Empty;

    [JsonPropertyName("TF2Huds")]
    public string TF2Huds { get; set; } = string.Empty;

    [JsonPropertyName("ComfigHuds")]
    public string ComfigHuds { get; set; } = string.Empty;

    [JsonPropertyName("GameBanana")]
    public string GameBanana { get; set; } = string.Empty;

    [JsonPropertyName("Steam")]
    public string Steam { get; set; } = string.Empty;

    [JsonPropertyName("Update")]
    public string Update { get; set; } = string.Empty;
}

public class WriteFile
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("TrueText")]
    public string TrueText { get; set; } = string.Empty;

    [JsonPropertyName("FalseText")]
    public string FalseText { get; set; } = string.Empty;
}

public class WriteCfg
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("TrueText")]
    public string TrueText { get; set; } = string.Empty;

    [JsonPropertyName("FalseText")]
    public string FalseText { get; set; } = string.Empty;
}

public class RenameFile
{
    [JsonPropertyName("NewName")]
    public string NewName { get; set; } = string.Empty;

    [JsonPropertyName("OldName")]
    public string OldName { get; set; } = string.Empty;
}