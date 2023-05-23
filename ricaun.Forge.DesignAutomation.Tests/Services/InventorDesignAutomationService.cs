using Autodesk.Forge.Core;
using ricaun.Forge.DesignAutomation.Services;

namespace ricaun.Forge.DesignAutomation.Tests.Services
{
    public class InventorDesignAutomationService : DesignAutomationService
    {
        public InventorDesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null) :
            base(appName, forgeConfiguration)
        {
        }

        public override string CoreConsoleExe()
        {
            return DefineDesignAutomation.Inventor.Core;
        }

        public override string CoreEngine()
        {
            return DefineDesignAutomation.Inventor.Engine;
        }

        public override string[] CoreEngineVersions()
        {
            return new[] { "2021", };
        }
    }
}