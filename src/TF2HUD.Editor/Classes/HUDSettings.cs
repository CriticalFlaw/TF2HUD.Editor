using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using TF2HUD.Editor.JSON;
using Newtonsoft.Json;

namespace TF2HUD.Editor.Classes
{
    public class HUDSettings
    {
        private static string FilePath = $"{Application.LocalUserAppDataPath}\\settings.json";
        private static List<Setting> userSettings = File.Exists(HUDSettings.FilePath) ? JsonConvert.DeserializeObject<Dictionary<string, List<Setting>>>(File.ReadAllText(FilePath))["Settings"] : new List<Setting>();

        public string HUDName;

        public HUDSettings(string Name)
        {
            this.HUDName = Name;

            // Write empty settings object to file as creating without Settings key causes a null
            // reference if settings are not saved by next application start
            if (!File.Exists(HUDSettings.FilePath)) File.WriteAllText(HUDSettings.FilePath, "{ \"Settings\": [] }");
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
                    bool evaluatedValue = (value is "1" or "True" or "true");
                    return (T)(object)evaluatedValue;
                case "Color":
                    var colors = Array.ConvertAll(value.Split(' '), c => byte.Parse(c));
                    return (T)(object)Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
                case "Int32":
                    return (T)(object)int.Parse(value);
                case "String":
                    return (T)(object)value.ToString();
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