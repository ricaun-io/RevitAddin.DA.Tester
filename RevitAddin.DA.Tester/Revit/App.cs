using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using RevitAddin.DA.Tester.Services;
using System;

namespace RevitAddin.DA.Tester.Revit
{
    public class App : IExternalDBApplication
    {
        IDisposable designAutomation;
        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"FullName: \t{this.GetType().Assembly.FullName}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Location: {this.GetType().Assembly.Location}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"AddInName: \t{application.ActiveAddInId?.GetAddInName()}");
            Console.WriteLine("----------------------------------------");

            designAutomation = new DesignAutomationLoadVersion(typeof(DesignAutomationController));
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            designAutomation?.Dispose();
            return ExternalDBApplicationResult.Succeeded;
        }
    }
}