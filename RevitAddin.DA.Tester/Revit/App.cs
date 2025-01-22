using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using ricaun.Revit.DA;
using RevitAddin.DA.Tester.Services;
using System;

namespace RevitAddin.DA.Tester.Revit
{
    public class App : DesignApplication
    {
        public static string AddInName { get; set; } = "none";
        public override void OnStartup()
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"FullName: \t{this.GetType().Assembly.FullName}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Location: {this.GetType().Assembly.Location}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"AddInName: \t{ControlledApplication.ActiveAddInId?.GetAddInName()}");
            Console.WriteLine("----------------------------------------");
            ControlledApplication.ApplicationInitialized += Application_ApplicationInitialized;   
        }

        private void Application_ApplicationInitialized(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
        {
            var application = (Application)sender;
            AddInName = application.ActiveAddInId?.GetAddInName();
        }

        public override bool Execute(Application application, string filePath, Document document)
        {
            return new DesignAutomationController().Execute(application, filePath, document);
        }
    }
}