namespace HUDEditor
{
    public interface IAppSettings
    {
        string HudDirectory { get; set; }
        string HudSelected { get; set; }
        string UserLanguage { get; set; }
        string JsonList { get; }
        string JsonFile { get; }
        string AppUpdate { get; }
        string AppTracker { get; }
        string AppDocs { get; }
        string MastercomfigVPKDownloadURL { get; }

        void Save();
    }
}