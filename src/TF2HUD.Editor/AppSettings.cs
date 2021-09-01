using HUDEditor.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor
{
    public class AppSettings : IAppSettings
    {
        public string HudDirectory { get => Settings.Default.hud_directory; set => Settings.Default.hud_directory = value; }

        public string HudSelected { get => Settings.Default.hud_selected; set => Settings.Default.hud_selected = value; }
        public string UserLanguage { get => Settings.Default.user_language; set => Settings.Default.user_language = value; }
        public string JsonList { get => Settings.Default.json_list; }
        public string JsonFile { get => Settings.Default.json_file; }
        public string AppUpdate { get => Settings.Default.app_update; }
        public string AppTracker { get => Settings.Default.app_tracker; }
        public string AppDocs { get => Settings.Default.app_docs; }

        public void Save()
        {
            Settings.Default.Save();
        }
    }
}
