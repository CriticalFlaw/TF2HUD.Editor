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
        // HUDs to manage
        public HUD[] HUDList;

        public Json()
        {
            var hudList = new List<HUD>();
            foreach (var jsonFile in Directory.EnumerateFiles("JSON"))
            {
                // Extract HUD information from the file path.
                var fileName = jsonFile.Split("\\")[^1];
                var fileInfo = fileName.Split(".");
                var hudName = fileInfo[0];
                var extension = fileInfo[^1];
                if (extension != "json") continue;
                var json = new StreamReader(File.OpenRead(jsonFile), new UTF8Encoding(false)).ReadToEnd();

                // Add the HUD object to the list.
                hudList.Add(new HUD(hudName, JsonConvert.DeserializeObject<HudJson>(json)));
            }

            HUDList = hudList.ToArray();
        }

        /// <summary>
        ///     Find and retrieve a HUD object selected by the user.
        /// </summary>
        /// <param name="name">Name of the HUD the user wants to view.</param>
        public HUD GetHUDByName(string name)
        {
            foreach (var hud in HUDList)
                if (string.Equals(hud.Name, name, StringComparison.InvariantCultureIgnoreCase))
                    return hud;
            throw new Exception($"Cannot find HUD {name}!");
        }
    }
}