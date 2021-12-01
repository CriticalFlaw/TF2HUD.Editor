using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using HUDEditor.Models;
using Newtonsoft.Json;

namespace HUDEditor.Classes
{
    public enum Preset
    {
        A,
        B,
        C,
        D
    }

    public class HUDSettings
    {
        public static readonly string UserFile =
            $"{Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor").FullName}\\settings.json";

        private static readonly UserJson Json = File.Exists(UserFile)
            ? JsonConvert.DeserializeObject<UserJson>(File.ReadAllText(UserFile))
            : new UserJson();

        private static readonly Dictionary<string, Preset> Presets = Json.Presets;

        private static readonly List<Setting> UserSettings = Json.Settings;

        private Preset _Preset;

        public string HUDName;

        public HUDSettings(string name)
        {
            HUDName = name;

            if (!Presets.ContainsKey(name)) Preset = Preset.A;
        }

        public Preset Preset
        {
            get => _Preset;
            set => _Preset = Presets[HUDName] = value;
        }

        /// <summary>
        ///     Add a new setting to user settings.
        /// </summary>
        public void AddSetting(string name, Controls control)
        {
            if (UserSettings.FirstOrDefault(x => x.Name == name && x.Preset == Preset) is null)
                UserSettings.Add(new Setting
                {
                    Hud = HUDName,
                    Name = name,
                    Type = control.Type,
                    Value = control.Value,
                    Preset = Preset
                });
        }

        /// <summary>
        ///     Retrieve a specific user setting by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        public Setting GetSetting(string name)
        {
            return UserSettings.First(x => x.Name == name && x.Preset == Preset);
        }

        /// <summary>
        ///     Retrieve a specific user setting or just the value, by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        public T GetSetting<T>(string name)
        {
            var value = UserSettings.First(x => x.Name == name && x.Preset == Preset).Value;

            switch (typeof(T).Name)
            {
                case "Boolean":
                    var evaluatedValue = value is "1" or "True" or "true";
                    return (T)(object)evaluatedValue;
                case "Color":
                    var colors = Array.ConvertAll(value.Split(' '), byte.Parse);
                    return (T)(object)Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
                case "Int32":
                    return (T)(object)int.Parse(value);
                case "String":
                    return (T)(object)value;
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
            UserSettings.First(x => x.Name == name && x.Preset == Preset).Value = value;
        }

        /// <summary>
        ///     Save a user setting value to file.
        /// </summary>
        public void SaveSettings()
        {
            var settings = new UserJson
            {
                Presets = Presets,
                Settings = UserSettings
            };
            File.WriteAllText(UserFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
            MainWindow.Logger.Info($"Saved user settings to: {UserFile}");
        }
    }
}