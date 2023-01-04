using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddin.DA.Tester.Revit.UI.Services
{
    public class CurrentDirectory : IDisposable
    {
        private readonly string CurrentDirectoryLast;

        public CurrentDirectory()
        {
            CurrentDirectoryLast = Directory.GetCurrentDirectory();
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(directory);
        }

        public CurrentDirectory(string directory)
        {
            CurrentDirectoryLast = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(directory);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(CurrentDirectoryLast);
        }
    }
}
