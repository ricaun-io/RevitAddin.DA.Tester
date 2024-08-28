using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace RevitAddin.DA.Tester.Revit
{
    public class DesignAutomationLoadVersion : IDisposable
    {
        IDisposable designAutomation;
        public DesignAutomationLoadVersion(Type type)
        {
            var location = type.Assembly.Location;
            var revitAssemblyReference = type.Assembly.GetReferencedAssemblies().FirstOrDefault(e => e.Name.Equals("RevitAPI"));
            var revitAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(e => e.GetName().Name.Equals("RevitAPI"));

            var revitReferenceVersion = revitAssemblyReference.Version.Major + 2000;
            var revitVersion = revitAssembly.GetName().Version.Major + 2000;

            Console.WriteLine($"DesignAutomationLoadVersion: \t{revitVersion} -> {revitReferenceVersion}");

            for (int version = revitVersion; version >= revitReferenceVersion; version--)
            {
                var directory = System.IO.Path.GetDirectoryName(location);
                var directoryVersionRevit = System.IO.Path.Combine(directory, "..", version.ToString());
                var fileName = System.IO.Path.Combine(directoryVersionRevit, System.IO.Path.GetFileName(location));

                Console.WriteLine($"DesignAutomationLoadVersion Try: \t{version}");

                if (File.Exists(fileName))
                {
                    Console.WriteLine($"DesignAutomationLoadVersion File Exists: \t{new FileInfo(fileName).FullName}");
                    Console.WriteLine($"DesignAutomationLoadVersion Version: \t{version}");
                    Console.WriteLine($"DesignAutomationLoadVersion LoadFile: \t{Path.GetFileName(fileName)}");
                    var assembly = Assembly.LoadFile(fileName);
                    type = assembly.GetType(type.FullName);
                    break;
                }
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"DesignAutomationLoadVersion Type: {type}");
            Console.WriteLine($"DesignAutomationLoadVersion FrameworkName: \t{type.Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}");
            designAutomation = new DesignAutomation(type);
        }
        public void Dispose()
        {
            designAutomation?.Dispose();
        }
    }
}