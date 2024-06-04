using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using RevitAddin.DA.Tester.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace RevitAddin.DA.Tester.Services
{
    public class DesignAutomationController
    {
        public static bool Execute(Application application, string filePath = null, Document document = null)
        {
            var inputModel = new InputModel().Load();

            var outputModel = new OutputModel();
            outputModel.VersionBuild = application.VersionBuild;
            outputModel.VersionName = application.VersionName;
            outputModel.Reference = outputModel.GetType().Assembly.GetReferencedAssemblies().FirstOrDefault(e => e.Name.Contains("RevitAPI"))?.Version.ToString();
            outputModel.FrameworkName = outputModel.GetType().Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            outputModel.Text = inputModel.Text;

            outputModel.Save();

            Console.WriteLine("----------------------------------------");
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
            Console.WriteLine("----------------------------------------");

            return true;
        }
    }
}