using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TF2HUD.Editor.JSON;

namespace TF2HUD.Editor.Classes
{
    public class Json
    {
        // The folder path to where the desired HUD .json info files are stored
        public string FolderPath;

        // HUDs to manage
        public HUD[] HUDs;

        public Json()
        {
            FolderPath = "JSON";

            var TempHUDs = new List<HUD>();
            foreach (var HUDPath in Directory.EnumerateFiles(FolderPath))
            {
                // Extracts HUD name from the file path
                var HUDArray = HUDPath.Split("\\");
                var FileName = HUDArray[^1];
                var FileInfo = FileName.Split(".");
                var HUDName = FileInfo[0];
                var Extension = FileInfo[^1];
                if (Extension != "json") continue;
                var json = new StreamReader(File.OpenRead(HUDPath), new UTF8Encoding(false)).ReadToEnd();

                // Pass the name and options to HUD.
                TempHUDs.Add(new HUD(HUDName, JsonConvert.DeserializeObject<HudJson>(json)));
            }

            HUDs = TempHUDs.ToArray();
        }

        /// <summary>
        ///     Retrieve the HUD object selected by the user.
        /// </summary>
        /// <param name="HUDName">Name of the HUD the user wants to view.</param>
        public HUD GetHUDByName(string HUDName)
        {
            foreach (var HUDItem in HUDs)
                if (string.Equals(HUDItem.Name, HUDName, StringComparison.InvariantCultureIgnoreCase))
                    return HUDItem;
            throw new Exception("Cannot find HUD " + HUDName + "!");
        }
    }
}