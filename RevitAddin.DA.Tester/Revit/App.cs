using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using RevitAddin.DA.Tester.Services;
using System;

namespace RevitAddin.DA.Tester.Revit
{
    public class App : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine(this.GetType().Assembly.FullName);
            Console.WriteLine("----------------------------------------");

            DesignAutomationBridge.DesignAutomationReadyEvent += DesignAutomationBridge_DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationBridge_DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        private void DesignAutomationBridge_DesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            var data = e.DesignAutomationData;
            e.Succeeded = DesignAutomationController.Execute(data.RevitApp, data.FilePath, data.RevitDoc);
        }
    }
}