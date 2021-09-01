namespace HUDEditor
{
    public interface ILocalization
    {
        string ErrorAppDirectory { get; }
        string ErrorHudInstall { get; }
        string ErrorHudMissing { get; }
        string ErrorHudUninstall { get; }
        string ErrorTransparentVM { get; }
        string ErrorUnknownExtension { get; }
        string HeaderRestartRequired { get; }
        string HeaderUpdateNone { get; }
        string InfoAddHud { get; }
        string InfoBackgroundOverride { get; }
        string InfoGameRestart { get; }
        string InfoGameRunning { get; }
        string InfoHudBackup { get; }
        string InfoHudReset { get; }
        string InfoHudUpdate { get; }
        string InfoHudUpdateNone { get; }
        string InfoPathBrowser { get; }
        string InfoPathInvalid { get; }
        string StatusApplied { get; }
        string StatusInstalled { get; }
        string StatusInstalledNot { get; }
        string StatusInstalledNow { get; }
        string StatusPathNotSet { get; }
        string StatusReset { get; }
        string TooltipAddHud { get; }
        string TooltipDocs { get; }
        string TooltipPath { get; }
        string TooltipRefresh { get; }
        string TooltipReport { get; }
        string UiApply { get; }
        string UiAuthor { get; }
        string UiBack { get; }
        string UiBrowse { get; }
        string UiClear { get; }
        string UiCustomize { get; }
        string UiDirectory { get; }
        string UiInstall { get; }
        string UiOptions { get; }
        string UiRefresh { get; }
        string UiReinstall { get; }
        string UiReset { get; }
        string UiSearch { get; }
        string UiSelect { get; }
        string UiSwitch { get; }
        string UiTitle { get; }
        string UiUninstall { get; }
    }
}