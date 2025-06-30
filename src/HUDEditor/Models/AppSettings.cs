namespace HUDEditor.Models;

using System.Text.Json.Serialization;

public class ConfigurationModel
{
    [JsonPropertyName("Settings")]
    public ConfigSettings ConfigSettings { get; set; } = new();
}

public class ConfigSettings
{
    [JsonPropertyName("User")]
    public UserPreferences UserPrefs { get; set; } = new();

    [JsonPropertyName("Application")]
    public AppConfig AppConfig { get; set; } = new();
}

public class UserPreferences
{
    [JsonPropertyName("hud_selected")]
    public string SelectedHUD { get; set; } = string.Empty;

    [JsonPropertyName("hud_directory")]
    public string HUDDirectory { get; set; } = string.Empty;

    [JsonPropertyName("user_language")]
    public string Language { get; set; } = "en-US";

    [JsonPropertyName("app_update_auto")]
    public bool AutoUpdate { get; set; } = true;

    [JsonPropertyName("app_path_bypass")]
    public bool PathBypass { get; set; } = false;

    [JsonPropertyName("app_xhair_persist")]
    public bool CrosshairPersistence { get; set; } = false;

    [JsonPropertyName("app_xhair_enabled")]
    public bool CrosshairEnabled { get; set; } = false;

    [JsonPropertyName("app_xhair_style")]
    public string CrosshairStyle { get; set; } = "$";

    [JsonPropertyName("app_xhair_color")]
    public string CrosshairColor { get; set; } = string.Empty;

    [JsonPropertyName("app_xhair_size")]
    public int CrosshairSize { get; set; } = 0;
}

public class AppConfig
{
    [JsonPropertyName("app_docs")]
    public string DocumentationURL { get; set; } = "https://criticalflaw.ca/TF2HUD.Editor/";

    [JsonPropertyName("json_file")]
    public string JsonFileURL { get; set; } = "https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/{0}";

    [JsonPropertyName("json_list")]
    public string JsonListURL { get; set; } = "https://api.github.com/repos/CriticalFlaw/TF2HUD.Editor/contents/src/TF2HUD.Editor/JSON";

    [JsonPropertyName("app_tracker")]
    public string IssueTrackerURL { get; set; } = "https://github.com/CriticalFlaw/TF2HUD.Editor/issues";

    [JsonPropertyName("app_update")]
    public string LatestUpdateURL { get; set; } = "https://github.com/CriticalFlaw/TF2HUD.Editor/releases/latest";

    [JsonPropertyName("mastercomfig_vpk")]
    public string MastercomfigVpkURL { get; set; } = "https://github.com/mastercomfig/mastercomfig/releases/download/9.5.2/mastercomfig-transparent-viewmodels-addon.vpk";

    [JsonPropertyName("tf2_hud_crosshairs_zip")]
    public string CrosshairPackURL { get; set; } = "https://github.com/Hypnootize/TF2-HUD-Crosshairs/archive/refs/heads/master.zip";

    [JsonPropertyName("castingessentials_zip")]
    public string CastingEssentialsURL { get; set; } = "https://github.com/Hypnootize/TF2-HUD-Crosshairs/archive/refs/heads/master.zip";

    [JsonPropertyName("sentry_dsn")]
    public string SentryDsn { get; set; } = "https://4e922f3f78a96ed10bd15cc899a69924@o287333.ingest.us.sentry.io/4509027513597952";
}
