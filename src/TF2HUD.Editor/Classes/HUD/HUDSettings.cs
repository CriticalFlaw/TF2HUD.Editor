using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using HUDEditor.Models;
using log4net;
using Newtonsoft.Json;

namespace HUDEditor.Classes
{
    public class HUDSettings
    {
        public static readonly string UserFile =
            $"{Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor").FullName}\\settings.json";

        private static readonly UserJson Json = File.Exists(UserFile)
            ? JsonConvert.DeserializeObject<UserJson>(File.ReadAllText(UserFile))
            : new UserJson();

        private static readonly Dictionary<string, HUDSettingsPreset> Presets = Json.Presets;

        private static readonly List<Setting> UserSettings = Json.Settings;

        private HUDSettingsPreset _Preset;

        public string HUDName;
        private readonly ILog _logger;

        public HUDSettings(string name, ILog logger)
        {
            HUDName = name;
            _logger = logger;
            if (!Presets.ContainsKey(name)) Preset = HUDSettingsPreset.A;
        }

        public HUDSettingsPreset Preset
        {
            get => _Preset;
            set => _Preset = Presets[HUDName] = value;
        }

        /// <summary>
        ///     Add a new setting to user settings.
        /// </summary>
        public void AddSetting(string name, Controls control)
        {
            if (GetSetting(name) is null)
                UserSettings.Add(new Setting
                {
                    HUD = HUDName,
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
            return UserSettings.FirstOrDefault(x => x.Name == name && x.Preset == Preset);
        }

        /// <summary>
        ///     Retrieve a specific user setting or just the value, by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        public T GetSetting<T>(string name)
        {
            var value = GetSetting(name).Value;

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
            GetSetting(name).Value = value;
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
            _logger.Info($"Saved user settings to: {UserFile}");
        }
    }
}