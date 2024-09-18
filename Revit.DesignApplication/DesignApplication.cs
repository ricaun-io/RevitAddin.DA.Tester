using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using System;

namespace Revit.DesignApplication
{
    public abstract class DesignApplication : IExternalDBApplication, IDesignAutomation
    {
        public ControlledApplication Application { get; private set; }
        public abstract void OnStartup();
        public abstract void OnShutdown();
        public abstract bool Execute(Application application, string filePath, Document document);

        private IExternalDBApplication designApplication;
        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            this.Application = application;

            designApplication = DesignApplicationLoader.LoadVersion(this);

            if (designApplication is IExternalDBApplication)
            {
                return designApplication.OnStartup(application);
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"FullName: \t{this.GetType().Assembly.FullName}");
            Console.WriteLine($"AddInName: \t{this.Application.ActiveAddInId?.GetAddInName()}");
            Console.WriteLine("----------------------------------------");

            OnStartup();
            DesignAutomationBridge.DesignAutomationReadyEvent += DesignAutomationReadyEvent;

            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            this.Application = application;

            if (designApplication is IExternalDBApplication)
            {
                try
                {
                    return designApplication.OnShutdown(application);
                }
                finally
                {
                    DesignApplicationLoader.Dispose();
                }
            }

            OnShutdown();
            DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationReadyEvent;

            return ExternalDBApplicationResult.Succeeded;
        }


        private void DesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationReadyEvent;

            var data = e.DesignAutomationData;

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"RevitApp: {data.RevitApp} \tFilePath: {data.FilePath} \tRevitDoc: {data.RevitDoc} \tAddInName:{data.RevitApp.ActiveAddInId?.GetAddInName()}");
            Console.WriteLine("--------------------------------------------------");

            e.Succeeded = Execute(data.RevitApp, data.FilePath, data.RevitDoc);
        }
    }
}