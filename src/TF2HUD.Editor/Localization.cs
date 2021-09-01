using HUDEditor.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor
{
    public class Localization : ILocalization
    {
        public string ErrorAppDirectory => Resources.error_app_directory;
        public string ErrorHudInstall => Resources.error_hud_install;
        public string ErrorHudMissing => Resources.error_hud_missing;
        public string ErrorHudUninstall => Resources.error_hud_uninstall;
        public string ErrorTransparentVM => Resources.error_transparent_vm;
        public string ErrorUnknownExtension => Resources.error_unknown_extension;
        public string HeaderRestartRequired => Resources.header_restart_required;
        public string HeaderUpdateNone => Resources.header_update_none;
        public string InfoAddHud => Resources.info_add_hud;
        public string InfoBackgroundOverride => Resources.info_background_override;
        public string InfoGameRestart => Resources.info_game_restart;
        public string InfoGameRunning => Resources.info_game_running;
        public string InfoHudBackup => Resources.info_hud_backup;
        public string InfoHudReset => Resources.info_hud_reset;
        public string InfoHudUpdate => Resources.info_hud_update;
        public string InfoHudUpdateNone => Resources.info_hud_update_none;
        public string InfoPathBrowser => Resources.info_path_browser;
        public string InfoPathInvalid => Resources.info_path_invalid;
        public string StatusApplied => Resources.status_applied;
        public string StatusInstalled => Resources.status_installed;
        public string StatusInstalledNot => Resources.status_installed_not;
        public string StatusInstalledNow => Resources.status_installed_now;
        public string StatusPathNotSet => Resources.status_pathNotSet;
        public string StatusReset => Resources.status_reset;
        public string TooltipAddHud => Resources.tooltip_addhud;
        public string TooltipDocs => Resources.tooltip_docs;
        public string TooltipPath => Resources.tooltip_path;
        public string TooltipRefresh => Resources.tooltip_refresh;
        public string TooltipReport => Resources.tooltip_report;
        public string UiApply => Resources.ui_apply;
        public string UiAuthor => Resources.ui_author;
        public string UiBack => Resources.ui_back;
        public string UiBrowse => Resources.ui_browse;
        public string UiClear => Resources.ui_clear;
        public string UiCustomize => Resources.ui_customize;
        public string UiDirectory => Resources.ui_directory;
        public string UiInstall => Resources.ui_install;
        public string UiOptions => Resources.ui_options;
        public string UiRefresh => Resources.ui_refresh;
        public string UiReinstall => Resources.ui_reinstall;
        public string UiReset => Resources.ui_reset;
        public string UiSearch => Resources.ui_search;
        public string UiSelect => Resources.ui_select;
        public string UiSwitch => Resources.ui_switch;
        public string UiTitle => Resources.ui_title;
        public string UiUninstall => Resources.ui_uninstall;
    }
}
