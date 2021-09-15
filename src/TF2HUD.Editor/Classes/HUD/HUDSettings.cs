using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using HUDEditor.Models;

namespace HUDEditor.Classes
{
    public class HUDSettings
    {
        public string HUDName;
        private readonly IUserSettingsService _userSettingsService;
        private readonly Dictionary<string, HUDSettingsPreset> _presets;
        private readonly List<Setting> _userSettings;
        private HUDSettingsPreset _preset;

        public HUDSettings(string name, IUserSettingsService userSettingsService)
        {
            HUDName = name;

            _userSettingsService = userSettingsService;
            var settings = _userSettingsService.Read();
            _presets = settings.Presets;
            _userSettings = settings.Settings;

            if (!_presets.ContainsKey(name)) Preset = HUDSettingsPreset.A;
        }

        public HUDSettingsPreset Preset
        {
            get => _preset;
            set => _preset = _presets[HUDName] = value;
        }

        /// <summary>
        ///     Add a new setting to user settings.
        /// </summary>
        public void AddSetting(string name, Controls control)
        {
            if (GetSetting(name) is null)
                _userSettings.Add(new Setting
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
            return _userSettings.FirstOrDefault(x => x.Name == name && x.Preset == Preset);
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
            _userSettingsService.Save(new UserJson
            {
                Presets = _presets,
                Settings = _userSettings
            });
        }
    }
}