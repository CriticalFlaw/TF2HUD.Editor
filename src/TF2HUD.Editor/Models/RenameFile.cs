using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace HUDEditor.Models
{
    public class RenameFile
    {
        [JsonPropertyName("NewName")] public string NewName;
        [JsonPropertyName("OldName")] public string OldName;
    }
}