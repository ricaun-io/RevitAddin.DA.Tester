using Autodesk.Revit.DB;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace Revit.DesignApplication
{
    internal static class DesignApplicationLoader
    {
        private static Assembly loadAssembly;
        public static IExternalDBApplication LoadVersion<T>(T designApplication) where T : DesignApplication
        {
            var type = designApplication.GetType();

            var similar = AppDomain.CurrentDomain.GetAssemblies().Where(e => e.FullName == type.Assembly.FullName);
            if (similar.Count() >= 2)
            {
                return null;
            }

            var location = type.Assembly.Location;
            var revitAssemblyReference = type.Assembly.GetReferencedAssemblies().FirstOrDefault(e => e.Name.Equals("RevitAPI"));
            var revitAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(e => e.GetName().Name.Equals("RevitAPI"));

            var revitReferenceVersion = revitAssemblyReference.Version.Major + 2000;
            var revitVersion = revitAssembly.GetName().Version.Major + 2000;

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"DesignApplicationLoader: \t{revitVersion} -> {revitReferenceVersion}");

            for (int version = revitVersion; version > revitReferenceVersion; version--)
            {
                var directory = Path.GetDirectoryName(location);
                var directoryVersionRevit = Path.Combine(directory, "..", version.ToString());
                var fileName = Path.Combine(directoryVersionRevit, Path.GetFileName(location));

                //Console.WriteLine($"DesignApplicationLoader Try: \t{version}");

                if (File.Exists(fileName))
                {
                    fileName = new FileInfo(fileName).FullName;
                    Console.WriteLine($"DesignApplicationLoader File Exists: \t{fileName}");
                    Console.WriteLine($"DesignApplicationLoader Version: \t{version}");
                    Console.WriteLine($"DesignApplicationLoader LoadFile: \t{Path.GetFileName(fileName)}");
                    AppDomain.CurrentDomain.AssemblyResolve += LoadAssemblyResolve;
                    loadAssembly = Assembly.LoadFile(fileName);
                    break;
                }
            }

            Console.WriteLine("----------------------------------------");

            if (loadAssembly is not null)
            {
                var loadType = loadAssembly.GetType(type.FullName);

                Console.WriteLine($"DesignApplicationLoader Type: {loadType}");
                Console.WriteLine($"DesignApplicationLoader FrameworkName: \t{loadType.Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}");
                Console.WriteLine("----------------------------------------");

                return Activator.CreateInstance(loadType) as IExternalDBApplication;
            }

            return null;
        }

        private static Assembly LoadAssemblyResolve(object sender, ResolveEventArgs args)
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

        public static void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= LoadAssemblyResolve;
        }
    }
}