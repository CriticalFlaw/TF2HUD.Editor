using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace HUDEditor.Models
{
    public class Download
    {
        public string Content => Source;
        public string Source { get; set; }
        public string Link { get; set; }
    }
}