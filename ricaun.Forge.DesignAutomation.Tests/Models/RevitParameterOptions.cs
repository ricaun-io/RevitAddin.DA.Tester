using ricaun.Forge.DesignAutomation.Attributes;

namespace ricaun.Forge.DesignAutomation.Tests.Models
{
    public class RevitParameterOptions
    {
        [ParameterActivityInputOpen]
        [ParameterInput("input.rvt", Required = true)]
        public string RvtFile { get; set; }

        [ParameterOutput("result.rvt", DownloadFile = true)]
        public string Result { get; set; }
    }
}