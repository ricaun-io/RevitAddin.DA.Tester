using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using RevitAddin.DA.Tester.Models;
using RevitAddin.DA.Tester.Revit;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace RevitAddin.DA.Tester.Services
{
    public class DesignAutomationController
    {
        public bool Execute(Application application, string filePath = null, Document document = null)
        {
            var inputModel = new InputModel().Load();

            var outputModel = new OutputModel();
            outputModel.AddInName = application.ActiveAddInId?.GetAddInName();
            outputModel.VersionBuild = application.VersionBuild;
            outputModel.VersionName = application.VersionName;
            outputModel.Reference = outputModel.GetType().Assembly.GetReferencedAssemblies().FirstOrDefault(e => e.Name.Contains("RevitAPI"))?.Version.ToString();
            outputModel.FrameworkName = outputModel.GetType().Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            outputModel.Text = inputModel.Text;

            outputModel.Save();

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Username: \t{application.Username}");
            Console.WriteLine($"LoginUserId: \t{application.LoginUserId}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"AddInName: \t{application.ActiveAddInId?.GetAddInName()}");
            Console.WriteLine($"Input:\t{inputModel}");
            Console.WriteLine($"Output:\t{outputModel}");
            Console.WriteLine("----------------------------------------");

            if (inputModel.Sleep > 0)
            {
                Console.WriteLine($"Sleep\t{inputModel.Sleep}ms");
                System.Threading.Thread.Sleep(inputModel.Sleep);
                Console.WriteLine("----------------------------------------");
            }

            Console.WriteLine($"UI:\t{UI.IsValid()}");
            Console.WriteLine($"App.AddInName:\t{App.AddInName}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"InAddInContext:\t{application.InAddInContext()}");
            Console.WriteLine($"InEventContext:\t{application.InEventContext()}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Shape:\t{typeof(ricaun.Revit.DB.Shape.Colors).Assembly}");
            Console.WriteLine($"Shape Location:\t{typeof(ricaun.Revit.DB.Shape.Colors).Assembly.Location}");
            Console.WriteLine("----------------------------------------");

            application.DocumentCreated += (s, e) => {
                Console.WriteLine($"DocumentCreated:\t{e.Document.Title}");
                Console.WriteLine($"DocumentCreated.AddInName: \t{application.ActiveAddInId?.GetAddInName()}");
                Console.WriteLine("----------------------------------------");
            };

            application.NewProjectDocument(UnitSystem.Metric).Close(false);

            return true;
        }
    }
}