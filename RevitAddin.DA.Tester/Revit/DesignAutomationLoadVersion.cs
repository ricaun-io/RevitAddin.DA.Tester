using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace RevitAddin.DA.Tester.Revit
{
    public class DesignAutomationLoadVersion<T> : DesignAutomationLoadVersion where T : IDesignAutomation
    {
        public DesignAutomationLoadVersion() : base(typeof(T))
        {

        }
    }

    public class DesignAutomationLoadVersion : IDisposable
    {
        readonly IDisposable designAutomation;
        readonly Assembly loadAssembly;

        public DesignAutomationLoadVersion(Type type)
        {
            var location = type.Assembly.Location;
            var revitAssemblyReference = type.Assembly.GetReferencedAssemblies().FirstOrDefault(e => e.Name.Equals("RevitAPI"));
            var revitAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(e => e.GetName().Name.Equals("RevitAPI"));

            var revitReferenceVersion = revitAssemblyReference.Version.Major + 2000;
            var revitVersion = revitAssembly.GetName().Version.Major + 2000;

            Console.WriteLine($"DesignAutomationLoadVersion: \t{revitVersion} -> {revitReferenceVersion}");

            for (int version = revitVersion; version > revitReferenceVersion; version--)
            {
                var directory = System.IO.Path.GetDirectoryName(location);
                var directoryVersionRevit = System.IO.Path.Combine(directory, "..", version.ToString());
                var fileName = System.IO.Path.Combine(directoryVersionRevit, System.IO.Path.GetFileName(location));

                Console.WriteLine($"DesignAutomationLoadVersion Try: \t{version}");

                if (File.Exists(fileName))
                {
                    fileName = new FileInfo(fileName).FullName;
                    Console.WriteLine($"DesignAutomationLoadVersion File Exists: \t{fileName}");
                    Console.WriteLine($"DesignAutomationLoadVersion Version: \t{version}");
                    Console.WriteLine($"DesignAutomationLoadVersion LoadFile: \t{Path.GetFileName(fileName)}");
                    AppDomain.CurrentDomain.AssemblyResolve += LoadAssemblyResolve;
                    loadAssembly = Assembly.LoadFile(fileName);
                    type = loadAssembly.GetType(type.FullName);
                    break;
                }
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"DesignAutomationLoadVersion Type: {type}");
            Console.WriteLine($"DesignAutomationLoadVersion FrameworkName: \t{type.Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}");
            designAutomation = new DesignAutomation(type);
        }

        private Assembly LoadAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var assemblyPath = Path.Combine(Path.GetDirectoryName(loadAssembly.Location), assemblyName.Name + ".dll");
            if (File.Exists(assemblyPath))
            {
                var folderName = Path.GetFileName(Path.GetDirectoryName(assemblyPath));
                Console.WriteLine($"AssemblyResolve LoadFile: {folderName}\\{assemblyName.Name + ".dll"}");
                return Assembly.LoadFile(assemblyPath);
            }
            return null;
        }

        public void Dispose()
        {
            designAutomation?.Dispose();
            AppDomain.CurrentDomain.AssemblyResolve -= LoadAssemblyResolve;
        }
    }
}