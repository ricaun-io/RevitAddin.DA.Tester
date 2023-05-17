﻿using Autodesk.Forge.Core;

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