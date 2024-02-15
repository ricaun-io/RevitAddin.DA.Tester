using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ricaun.Revit.UI;
using ricaun.Revit.UI.Drawing;
using System;

namespace RevitAddin.DA.Tester.Revit.UI
{
    [AppLoader]
    public class AppUI : IExternalApplication
    {
        private static RibbonPanel ribbonPanel;
        public Result OnStartup(UIControlledApplication application)
        {
            ribbonPanel = application.CreatePanel("RevitAddin.DA.Tester");
            ribbonPanel.CreatePushButton<Commands.Command>()
                .SetLargeImage(Properties.Resources.Revit.GetBitmapSource());
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            ribbonPanel?.Remove();
            return Result.Succeeded;
        }
    }

}