using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using HUDEditor.Models;
using Newtonsoft.Json;

namespace HUDEditor.Classes
{
    public class HUDSettings
    {
        public static readonly string UserFile = System.Threading.Tasks.Task.Run(() =>
        {
            Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor");
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor\\settings.json";
        }).Result;

        private static readonly List<Setting> UserSettings = File.Exists(UserFile)
            ? JsonConvert.DeserializeObject<Dictionary<string, List<Setting>>>(File.ReadAllText(UserFile))?["Settings"]
            : new List<Setting>();

        public string HUDName;

        public HUDSettings(string name)
        {
            HUDName = name;

            // Write empty settings object to file as creating without Settings key causes a null reference if settings are not saved by next application start
            if (!File.Exists(UserFile)) File.WriteAllText(UserFile, "{ \"Settings\": [] }");
        }

        /// <summary>
        ///     Add a new setting to user settings.
        /// </summary>
        public void AddSetting(string name, Controls control)
        {
            if (UserSettings.FirstOrDefault(x => x.Name == name) == null)
                UserSettings.Add(new Setting
                {
                    HUD = HUDName,
                    Name = name,
                    Type = control.Type,
                    Value = control.Value
                });
        }

        /// <summary>
        ///     Retrieve a specific user setting by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        public Setting GetSetting(string name)
        {
            return UserSettings.First(x => x.Name == name);
        }

        /// <summary>
        ///     Retrieve a specific user setting or just the value, by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        /// <param name="returnVal">
        ///     Indicate if you want to retrieve just the setting value (true) or the whole setting object (false).
        /// </param>
        public dynamic GetSetting(string name, bool returnVal)
        {
            var setting = UserSettings.First(x => x.Name == name);
            return returnVal ? setting.Value : setting;
        }

        /// <summary>
        ///     Retrieve a specific user setting or just the value, by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        public T GetSetting<T>(string name)
        {
            var setting = UserSettings.First(x => x.Name == name);
            var value = setting.Value;

            switch (typeof(T).Name)
            {
                case "Boolean":
                    var evaluatedValue = value is "1" or "True" or "true";
                    return (T) (object) evaluatedValue;
                case "Color":
                    var colors = Array.ConvertAll(value.Split(' '), byte.Parse);
                    return (T) (object) Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
                case "Int32":
                    return (T) (object) int.Parse(value);
                case "String":
                    return (T) (object) value;
                default:
                    throw new Exception($"Unexpected setting type {typeof(T).Name}!");
            }
        }

        /// <summary>
        ///     Set a new user setting value.
        /// </summary>
        /// <param name="name">Name of the setting to update.</param>
        /// <param name="value">New value for updating setting.</param>
        public void SetSetting(string name, string value)
        {
            UserSettings.First(x => x.Name == name).Value = value;
        }

        /// <summary>
        ///     Save a user setting value to file.
        /// </summary>
        public void SaveSettings()
        {
            var settings = new Dictionary<string, List<Setting>>
            {
                ["Settings"] = UserSettings
            };
            File.WriteAllText(UserFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
            MainWindow.Logger.Info($"Saved user settings to: {UserFile}");
        }
    }
}