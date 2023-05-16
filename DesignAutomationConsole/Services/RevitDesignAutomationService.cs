using Autodesk.Forge.Core;

namespace DesignAutomationConsole.Services
{
    public class RevitDesignAutomationService : DesignAutomationService
    {
        public RevitDesignAutomationService(string appName,
            ForgeConfiguration forgeConfiguration = null,
            string forgeEnvironment = "dev") : base(appName, forgeConfiguration, forgeEnvironment)
        {
        }

        public override string CoreConsoleExe()
        {
            return "revitcoreconsole.exe";
        }

        public override string CoreEngine()
        {
            return "Autodesk.Revit";
        }

        public override string[] CoreEngineVersions()
        {
            return new[] {
                //"2019", "2020",
                "2021", "2022",
                "2023", "2024" };
        }
    }
}