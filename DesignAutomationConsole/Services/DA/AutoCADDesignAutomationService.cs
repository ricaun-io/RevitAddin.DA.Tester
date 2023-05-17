using Autodesk.Forge.Core;

namespace DesignAutomationConsole.Services
{
    public class AutoCADDesignAutomationService : DesignAutomationService
    {
        public AutoCADDesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null) : base(appName, forgeConfiguration)
        {
        }

        public override string CoreConsoleExe()
        {
            return DefineDesignAutomation.AutoCAD.Core;
        }

        public override string CoreEngine()
        {
            return DefineDesignAutomation.AutoCAD.Engine;
        }

        public override string[] CoreEngineVersions()
        {
            return new[] { "24", "24_1" };
        }
    }
}