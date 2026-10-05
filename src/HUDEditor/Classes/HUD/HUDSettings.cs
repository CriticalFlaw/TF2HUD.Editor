using HUDEditor.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HUDEditor.Classes;

public class HUDSettings
{
    public static readonly string UserFile = Path.Combine(Directory.CreateDirectory(Utilities.UserDataFolder).FullName, "settings.json");

    private static readonly UserJson Json = LoadUserFile();

    private static readonly Dictionary<string, Preset> Presets = Json.Presets;
    private static readonly List<Setting> UserSettings = Json.Settings;
    private Preset _Preset;
    public string HUDName;

    /// <summary>
    /// Loads the user settings file. A corrupt file is backed up and replaced instead of crashing the app on startup.
    /// </summary>
    private static UserJson LoadUserFile()
    {
        try
        {
            if (File.Exists(UserFile))
            {
                var json = JsonConvert.DeserializeObject<UserJson>(File.ReadAllText(UserFile));
                if (json is not null)
                {
                    json.Settings ??= [];
                    json.Presets ??= [];
                    return json;
                }
            }
        }
        catch (Exception e)
        {
            var backup = UserFile + ".bak";
            App.Logger.Error($"Failed to read user settings, backing up to \"{backup}\": {e.Message}");
            try { File.Copy(UserFile, backup, true); } catch { /* best effort */ }
        }

        return new UserJson();
    }

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
    /// Adds a new user setting.
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
    /// Retrieves a user setting by name.
    /// </summary>
    /// <param name="name">Name of the setting to retrieve.</param>
    public Setting? GetSetting(string name)
    {
        return UserSettings.FirstOrDefault(x => x.Name == name && x.Preset == Preset);
    }

    /// <summary>
    /// Retrieves a user setting or just the value, by name.
    /// </summary>
    /// <param name="name">Name of the setting to retrieve.</param>
    public T GetSetting<T>(string name)
    {
        var value = GetSetting(name)?.Value ?? string.Empty;

        switch (typeof(T).Name)
        {
            case "Boolean":
                var evaluatedValue = value is "1" or "True" or "true";
                return (T)(object)evaluatedValue;

            case "Color":
                return (T)(object)Utilities.ConvertToColor(value);

            case "Int32":
                return (T)(object)(int.TryParse(value, out var result) ? result : 0);

            case "String":
                return (T)(object)value;

            default:
                throw new Exception($"Unexpected setting type {typeof(T).Name}!");
        }
    }

    /// <summary>
    /// Sets a new user setting value.
    /// </summary>
    /// <param name="name">Name of the setting to update.</param>
    /// <param name="value">New value for updating setting.</param>
    public void SetSetting(string? name, string? value)
    {
        var setting = name is null ? null : GetSetting(name);
        if (setting is null)
        {
            App.Logger.Warn($"Tried to set unknown setting \"{name}\" on {HUDName}.");
            return;
        }
        setting.Value = value ?? string.Empty;
    }

    /// <summary>
    /// Saves a user setting value to file.
    /// </summary>
    public void SaveSettings()
    {
        var settings = new UserJson
        {
            Presets = Presets,
            Settings = UserSettings
        };
        // Write to a temp file first, then replace, so a crash mid-write can't corrupt the settings.
        Directory.CreateDirectory(Path.GetDirectoryName(UserFile)!); // May have been removed by "Clear cache".
        var tempFile = UserFile + ".tmp";
        File.WriteAllText(tempFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
        File.Move(tempFile, UserFile, overwrite: true);
        App.Logger.Info($"Saved user settings to: {UserFile}");
    }
}