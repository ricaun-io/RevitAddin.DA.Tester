using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace RevitAddin.DA.Tester.Revit
{
    public interface IDesignAutomation
    {
        bool Execute(Application application, string filePath, Document document);
    }
}