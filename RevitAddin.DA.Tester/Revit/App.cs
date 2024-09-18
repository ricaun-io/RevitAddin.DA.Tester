using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Revit.DesignApplication;
using RevitAddin.DA.Tester.Services;
using System;

namespace RevitAddin.DA.Tester.Revit
{
    public class App : DesignApplication
    {
        public override void OnStartup()
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"FullName: \t{this.GetType().Assembly.FullName}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Location: {this.GetType().Assembly.Location}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"AddInName: \t{Application.ActiveAddInId?.GetAddInName()}");
            Console.WriteLine("----------------------------------------");
        }

        public override void OnShutdown()
        {
            
        }
        public override bool Execute(Application application, string filePath, Document document)
        {
            return new DesignAutomationController().Execute(application, filePath, document);
        }
    }
}