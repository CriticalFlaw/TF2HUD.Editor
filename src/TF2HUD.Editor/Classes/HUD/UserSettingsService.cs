using HUDEditor.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor.Classes
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly string _userSettingsFilepath;
        private readonly ILog _logger;
        private string AppSettingsPath() => $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor";

        public UserSettingsService(ILog logger)
        {
            var appSettingsDirectory = Directory.CreateDirectory(AppSettingsPath());
            _userSettingsFilepath = Path.Combine(appSettingsDirectory.FullName, "settings.json");
            _logger = logger;
        }

        public UserJson Read()
        {
            return File.Exists(_userSettingsFilepath)
                ? ParseSettingsFile()
                : new UserJson();
        }

        public void Save(UserJson settings)
        {
            File.WriteAllText(_userSettingsFilepath, SerializeSettings(settings));
            _logger.Info($"Saved user settings to: {_userSettingsFilepath}");
        }

        private UserJson ParseSettingsFile()
        {
            return JsonConvert.DeserializeObject<UserJson>(File.ReadAllText(_userSettingsFilepath));
        }

        private string SerializeSettings(UserJson settings)
        {
            return JsonConvert.SerializeObject(settings, Formatting.Indented);
        }
    }
}
