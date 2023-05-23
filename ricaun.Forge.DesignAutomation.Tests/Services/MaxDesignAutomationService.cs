using Autodesk.Forge.Core;
using ricaun.Forge.DesignAutomation.Services;

namespace ricaun.Forge.DesignAutomation.Tests.Services
{
    public class MaxDesignAutomationService : DesignAutomationService
    {
        public MaxDesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null) :
            base(appName, forgeConfiguration)
        {
        }

        public override string CoreConsoleExe()
        {
            return DefineDesignAutomation.Max.Core;
        }

        public override string CoreEngine()
        {
            return DefineDesignAutomation.Max.Engine;
        }

        public override string[] CoreEngineVersions()
        {
            return new[] { "2021", };
        }
    }
}