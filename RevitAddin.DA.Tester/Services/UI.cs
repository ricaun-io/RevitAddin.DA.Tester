namespace RevitAddin.DA.Tester.Services
{
    /// <summary>
    /// Utility class to check if Revit UI is available.
    /// </summary>
    public class UI
    {
        /// <summary>
        /// Check if is possible to use <see cref="Autodesk.Revit.UI.UIApplication"/> reference.
        /// </summary>
        public static bool IsValid()
        {
            try
            {
                var uiapp = UIApplication;
                System.Console.WriteLine($"UIApplication:\t{uiapp}");
                return true;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"UIApplication:\t{ex.Message}");
                return false;
            }
        }

        private static System.Type UIApplication => typeof(Autodesk.Revit.UI.UIApplication);
    }
}