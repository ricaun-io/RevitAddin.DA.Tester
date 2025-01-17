namespace RevitAddin.DA.Tester.Services
{
    public static class ApplicationExtensions
    {
        public static bool InAddInContext(this Autodesk.Revit.ApplicationServices.Application application)
        {
            return application.ActiveAddInId is not null;
        }

        public static bool InEventContext(this Autodesk.Revit.ApplicationServices.Application application)
        {
            void Application_ProgressChanged(object sender, Autodesk.Revit.DB.Events.ProgressChangedEventArgs e) { }
            try
            {
                application.ProgressChanged += Application_ProgressChanged;
                application.ProgressChanged -= Application_ProgressChanged;
                return true;
            }
            catch { }
            return false;
        }

    }
}