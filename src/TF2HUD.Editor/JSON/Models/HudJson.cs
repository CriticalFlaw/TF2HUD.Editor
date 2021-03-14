using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TF2HUD.Editor.JSON
{
    public class HudJson
    {
        [JsonPropertyName("Controls")] public Dictionary<string, Controls[]> Controls;

        [JsonPropertyName("CustomisationsFolder")]
        public string CustomisationsFolder;

        [JsonPropertyName("EnabledFolder")] public string EnabledFolder;
        [JsonPropertyName("Layout")] public string[] Layout;
        [JsonPropertyName("URLs")] public Links Links;
    }

    public class Links
    {
        [JsonPropertyName("GitHub")] public string GitHub;
        [JsonPropertyName("HudsTF")] public string HudsTF;
        [JsonPropertyName("Issue")] public string Issue;
        [JsonPropertyName("Steam")] public string Steam;
        [JsonPropertyName("Update")] public string Update;
    }

    public class Controls
    {
        [JsonPropertyName("Default")] public string Default = "0";
        [JsonPropertyName("Increment")] public int Increment = 2;
        [JsonPropertyName("Instructions")] public Newtonsoft.Json.Linq.JObject Instruction;

        [JsonPropertyName("Instruction")] public Instructions[] Instructions;
        [JsonPropertyName(";")] public string Label;
        [JsonPropertyName("Maximum")] public int Maximum = 30;

        [JsonPropertyName("Minimum")] public int Minimum = 10;
        [JsonPropertyName("Name")] public string Name;
        [JsonPropertyName("Options")] public Option[] Options;
        [JsonPropertyName("Type")] public string Type;
    }

    public class Instructions
    {
        [JsonPropertyName("Command")] public string Command;
        [JsonPropertyName("Defaults")] public string[] Defaults;
        [JsonPropertyName("FileName")] public string FileName;
        [JsonPropertyName("Group")] public string Group;
        [JsonPropertyName("Tags")] public string[] Tags;
        [JsonPropertyName("Values")] public string[] Values;
    }

    public class Option
    {
        [JsonPropertyName("Label")] public string Label;
        [JsonPropertyName("Value")] public string Value;
    }
}