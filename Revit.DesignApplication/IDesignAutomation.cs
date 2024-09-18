using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Revit.DesignApplication
{
    public interface IDesignAutomation
    {
        bool Execute(Application application, string filePath, Document document);
    }
}