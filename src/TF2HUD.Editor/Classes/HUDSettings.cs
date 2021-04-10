using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using TF2HUD.Editor.JSON;
using Newtonsoft.Json;

namespace TF2HUD.Editor.Classes
{
    public class HUDSettings
    {
        private static string FilePath = "settings.json";
        private static List<Setting> userSettings = File.Exists(HUDSettings.FilePath) ? JsonConvert.DeserializeObject<Dictionary<string, List<Setting>>>(File.ReadAllText("settings.json"))["Settings"] : new List<Setting>();

        public string HUDName;

        public HUDSettings(string Name)
        {
            this.HUDName = Name;
        }

        /// <summary>
        ///     Create new a setting
        /// </summary>
        public void AddSetting(string Name, Controls Options)
        {
            var setting = HUDSettings.userSettings.Where((x) => x.Name == Name).FirstOrDefault();
            if (setting == null)
            {
                HUDSettings.userSettings.Add(new Setting
                {
                    HUD = this.HUDName,
                    Name = Name,
                    Type = Options.Type,
                    Value = Options.Default
                });
            }
        }

        public Setting GetSetting(string Key)
        {
            var setting = HUDSettings.userSettings.Where((x) => x.Name == Key).First();
            return setting;
        }

        public dynamic GetSetting(string Key, bool returnVal)
        {
            var setting = HUDSettings.userSettings.Where((x) => x.Name == Key).First();
            return returnVal ? setting.Value : setting;
        }

        public T GetSetting<T>(string Key, bool returnVal = false)
        {
            var setting = HUDSettings.userSettings.Where((x) => x.Name == Key).First();
            var value = setting.Value;

            switch (typeof(T).Name)
            {
                case "Boolean":
                    bool evaluatedValue = (value == "1" || value == "True" || value == "true");
                    return (T)(object)evaluatedValue;
                case "Color":
                    var colors = Array.ConvertAll(value.Split(' '), c => byte.Parse(c));
                    return (T)(object)Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
                case "Int32":
                    return (T)(object)int.Parse(value);
                default:
                    throw new Exception($"Unexpected setting type {typeof(T).Name}!");
            }
        }

        public void SetSetting(string Key, string Value)
        {
            HUDSettings.userSettings.Where((x) => x.Name == Key).First().Value = Value;
        }

        public void SaveSettings()
        {
            var SettingsContainer = new Dictionary<string, List<Setting>>();
            SettingsContainer["Settings"] = HUDSettings.userSettings;
            File.WriteAllText(HUDSettings.FilePath, JsonConvert.SerializeObject(SettingsContainer, Formatting.Indented));
        }
    }
}