using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace RevitAddin.DA.Tester.Services
{
    [Transaction(TransactionMode.Manual)]
    public class CommandUI : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            UIApplication uiapp = commandData.Application;

            Console.WriteLine($"UI: {UI.IsValid()}");

            return Result.Succeeded;
        }
    }

}