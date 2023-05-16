using DesignAutomationConsole.Attributes;

namespace DesignAutomationConsole.Models
{
    public class RevitParameterOptions
    {
        [ParameterActivityInputOpen]
        [ParameterInput("input.rvt", Required = true)]
        public string RvtFile { get; set; }

        [ParameterOutput("result.rvt", DownloadFile = true)]
        public string Result { get; set; }
    }

    public class AutoCADParameterOptions
    {
        [ParameterActivityInputOpen]
        [ParameterInput("input.dwg", Required = true)]
        public string InputDwg { get; set; }

        [ParameterOutput("layers.txt", DownloadFile = true)]
        public string Result { get; set; }

        [ParameterActivityScript]
        public string Script { get; set; }
    }

    public class MaxParameterOptions
    {
        [ParameterActivityClearBundle]
        [ParameterActivityInput("-sceneFile")]
        [ParameterInput("workingFolder", Required = true, ZipPath = "sceneName.max")]
        public string InputMaxScene { get; set; }
        [ParameterActivityInputArgument]
        [ParameterInput("maxscriptToExecute.ms")]
        public string MaxscriptToExecute { get; set; }

        [ParameterOutput("workingFolder", DownloadFile = true, Zip = true)]
        public string OutputZip { get; set; }
    }

    public class InventorParameterOptions
    {
        [ParameterActivityInputOpen]
        [ParameterInput("Input.ipt", Required = true)]
        public string InventorDoc { get; set; }
        [ParameterActivityInputArgument]
        [ParameterInput("params.json")]
        public InventorModel InventorParams { get; set; }

        [ParameterOutput("ResultSmall.ipt", DownloadFile = true)]
        public string OutputIpt { get; set; }
        //[ParameterOutput("ResultSmall.zip", DownloadFile = true)]
        //public string OutputIam { get; set; }
        [ParameterOutput("ResultSmall.bmp", DownloadFile = true)]
        public string OutputBmp { get; set; }

        public class InventorModel
        {
            public string height { get; set; }
            public string width { get; set; }
        }
    }

}