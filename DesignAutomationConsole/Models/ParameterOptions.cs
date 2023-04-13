using DesignAutomationConsole.Attributes;

namespace DesignAutomationConsole.Models
{
    public class ParameterOptions
    {
        [ParameterInput("input.json",
            Description = "Input file.",
            Required = true)]
        public InputModel Input { get; set; } = new InputModel();

        [ParameterOutput("output.json",
            Description = "Output file.")]
        public OutputModel Output { get; set; }
    }

    public class ParameterOptionsDownload
    {
        [ParameterInput("input.json",
            Required = true)]
        public InputModel Input { get; set; }

        [ParameterOutput("output.json", DownloadFile = true)]
        public string Output { get; set; }
    }

    public class ParameterOptionsFile
    {
        [ParameterInput("input.json",
            Description = "Input file.",
            Required = true)]
        public string Input { get; set; }

        [ParameterOutput("output.json",
            Description = "Output file.")]
        public OutputModel Output { get; set; }
    }
}