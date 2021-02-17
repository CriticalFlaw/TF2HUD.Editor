using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TF2HUD.Editor.Common
{
    public class Json
    {
        // The folder path to where the desired HUD .json info files are stored
        public string FolderPath;

        // HUDs to manage
        public HUD[] HUDs;

        public Json(string FolderPath)
        {
            this.FolderPath = FolderPath;

            var TempHUDs = new List<HUD>();
            foreach (var HUDPath in Directory.EnumerateFiles(this.FolderPath))
            {
                // Extracts HUD name from the file path
                var HUDArray = HUDPath.Split("\\");
                var FileName = HUDArray[^1];
                var FileInfo = FileName.Split(".");
                var HUDName = FileInfo[0];
                var Extension = FileInfo[^1];
                if (Extension != "json") continue;
                var json = new StreamReader(File.OpenRead(HUDPath), new UTF8Encoding(false)).ReadToEnd();

                // Pass the name and options to hud.
                TempHUDs.Add(new HUD(HUDName, JsonConvert.DeserializeObject<HUDRoot>(json)));
            }

            HUDs = TempHUDs.ToArray();
        }

        public HUD GetHUDByName(string HUDName)
        {
            foreach (var HUDItem in HUDs)
                if (HUDItem.Name.ToLowerInvariant() == HUDName.ToLowerInvariant())
                    return HUDItem;
            throw new Exception("Cannot find HUD " + HUDName + "!");
        }
    }

    #region MODEL

    public class UserSettings
    {
        [JsonPropertyName("Settings")] public List<Setting> Settings { get; set; }
    }

    public class Setting
    {
        [JsonPropertyName("HUD")] public string HUD { get; set; }
        [JsonPropertyName("Name")] public string Name { get; set; }
        [JsonPropertyName("Type")] public string Type { get; set; }
        [JsonPropertyName("Value")] public string Value { get; set; }
    }

    #endregion
}