using HUDEditor.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor.Classes
{
    public class HUDFactory : IHUDFactory
    {
        private readonly ILog _logger;
        private readonly IUtilities _utilities;
        private readonly INotifier _notifier;
        private readonly ILocalization _localization;
        private readonly VTF _vtf;
        private readonly IAppSettings _settings;
        private readonly IUserSettingsService _userSettingsService;

        public HUDFactory(
            ILog logger,
            IUtilities utilities,
            INotifier notifier,
            ILocalization localization,
            VTF vtf,
            IAppSettings settings,
            IUserSettingsService userSettingsService)
        {
            _logger = logger;
            _utilities = utilities;
            _notifier = notifier;
            _localization = localization;
            _vtf = vtf;
            _settings = settings;
            _userSettingsService = userSettingsService;
        }

        public HUD CreateHUD(string name, HudJson schema, bool uniq)
        {
            return new HUD(name, schema, uniq, _logger, _utilities, _notifier, _localization, _vtf, _settings, _userSettingsService);
        }
    }
}
