using DesignAutomationConsole.Attributes;

namespace DesignAutomationConsole.Models
{
    public class AutoCADParameterOptions
    {
        [ParameterActivityInputOpen]
        [ParameterInput("input.dwg", Required = true)]
        public string InputDwg { get; set; }

        [ParameterOutput("layers.txt", DownloadFile = true)]
        public string Result { get; set; }

        [ParameterActivityScript]
        public string Script { get; set; }

        [ParameterWorkItemTimeSec]
        public int TimeSec { get; set; } = 30;
    }

}