using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor.Classes
{
    public class JsonFactory : IJsonFactory
    {
        private readonly ILog _logger;
        private readonly IUtilities _utilities;
        private readonly IAppSettings _settings;
        private readonly IHUDUpdateChecker _hudTester;
        private readonly IHUDFactory _hudFactory;

        public JsonFactory(
            ILog logger,
            IUtilities utilities,
            IAppSettings settings,
            IHUDUpdateChecker hudTester,
            IHUDFactory hudFactory)
        {
            _logger = logger;
            _utilities = utilities;
            _settings = settings;
            _hudTester = hudTester;
            _hudFactory = hudFactory;
        }

        public Json CreateJson()
        {
            return new Json(_logger, _utilities, _settings, _hudTester, _hudFactory);
        }
    }
}
