using Autodesk.Forge.Core;

namespace DesignAutomationConsole.Services
{
    public class MaxDesignAutomationService : DesignAutomationService
    {
        public MaxDesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null, string forgeEnvironment = "dev") :
            base(appName, forgeConfiguration, forgeEnvironment)
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