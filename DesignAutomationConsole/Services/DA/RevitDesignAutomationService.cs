using Autodesk.Forge.Core;

namespace DesignAutomationConsole.Services
{
    public class RevitDesignAutomationService : DesignAutomationService
    {
        public RevitDesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null) : base(appName, forgeConfiguration)
        {
        }

        public override string CoreConsoleExe()
        {
            return DefineDesignAutomation.Revit.Core;
        }

        public override string CoreEngine()
        {
            return DefineDesignAutomation.Revit.Engine;
        }

        public override string[] CoreEngineVersions()
        {
            return new[] {
                //"2019", "2020",
                "2021", "2022",
                "2023", "2024"
            };
        }
    }
}