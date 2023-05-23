namespace ricaun.Forge.DesignAutomation.Services
{
    public static class DefineDesignAutomation
    {
        public static class Revit
        {
            public static string Core { get; } = "revitcoreconsole.exe";
            public static string Engine { get; } = "Autodesk.Revit";
        }

        public static class Max
        {
            public static string Core { get; } = "3dsmaxbatch.exe";
            public static string Engine { get; } = "Autodesk.3dsMax";
        }

        public static class AutoCAD
        {
            public static string Core { get; } = "accoreconsole.exe";
            public static string Engine { get; } = "Autodesk.AutoCAD";
        }

        public static class Inventor
        {
            public static string Core { get; } = "InventorCoreConsole.exe";
            public static string Engine { get; } = "Autodesk.Inventor";
        }
    }
}