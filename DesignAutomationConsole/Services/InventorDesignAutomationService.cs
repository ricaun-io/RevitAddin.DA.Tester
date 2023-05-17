using Autodesk.Forge.Core;

namespace DesignAutomationConsole.Services
{
    public class InventorDesignAutomationService : DesignAutomationService
    {
        public InventorDesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null, string forgeEnvironment = "dev") :
            base(appName, forgeConfiguration, forgeEnvironment)
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