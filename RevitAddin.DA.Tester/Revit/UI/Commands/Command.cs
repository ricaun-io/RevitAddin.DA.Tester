using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitAddin.DA.Tester.Models;
using RevitAddin.DA.Tester.Revit.UI.Services;
using RevitAddin.DA.Tester.Services;
using System;
using System.IO;

namespace RevitAddin.DA.Tester.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            UIApplication uiapp = commandData.Application;
            var application = uiapp.Application;

            using (new CurrentDirectory())
            {
                var input = new InputModel();
                input.Text = $"{this.GetType().Assembly.FullName}";
                input.Save();

                DesignAutomationController.Execute(application);
                var inputModel = new InputModel().Load();
                var outputModel = new OutputModel().Load();
            }

            //System.Windows.MessageBox.Show(uiapp.Application.VersionName);

            return Result.Succeeded;
        }
    }
}
