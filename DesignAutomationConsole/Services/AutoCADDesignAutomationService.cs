using Autodesk.Forge.Core;

namespace DesignAutomationConsole.Services
{
    public class AutoCADDesignAutomationService : DesignAutomationService
    {
        public AutoCADDesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null, string forgeEnvironment = "dev") :
            base(appName, forgeConfiguration, forgeEnvironment)
        {
        }

        public override string CoreConsoleExe()
        {
            return "accoreconsole.exe";
        }

        public override string CoreEngine()
        {
            return "Autodesk.AutoCAD";
        }

        public override string[] CoreEngineVersions()
        {
            return new[] { "24", "24_1" };
        }
    }

    public class InventorDesignAutomationService : DesignAutomationService
    {
        public InventorDesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null, string forgeEnvironment = "dev") :
            base(appName, forgeConfiguration, forgeEnvironment)
        {
        }

        public override string CoreConsoleExe()
        {
            return "InventorCoreConsole.exe";
        }

        public override string CoreEngine()
        {
            return "Autodesk.Inventor";
        }

        public override string[] CoreEngineVersions()
        {
            return new[] { "2021", };
        }
    }

    public class MaxDesignAutomationService : DesignAutomationService
    {
        public MaxDesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null, string forgeEnvironment = "dev") :
            base(appName, forgeConfiguration, forgeEnvironment)
        {
        }

        public override string CoreConsoleExe()
        {
            return "3dsmaxbatch.exe";
        }

        public override string CoreEngine()
        {
            return "Autodesk.3dsMax";
        }

        public override string[] CoreEngineVersions()
        {
            return new[] { "2021", };
        }
    }
}