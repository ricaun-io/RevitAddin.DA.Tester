using DesignAutomationConsole.Attributes;
using System;

namespace DesignAutomationConsole
{
    class ParameterOptions
    {
        [ParameterInput("input.json",
            Description = "Input file.",
            Required = true)]
        public InputModel Input { get; set; }

        [ParameterInput("input2.json",
            Description = "Input2 file.")]
        public string Input2 { get; set; }

        [ParameterOutput("output.json",
            Description = "Output file.")]
        public string Output { get; set; }

        [ParameterOutput("output_download.json",
            Description = "OutputDownload file.",
            DownloadFile = true)]
        public OutputModel OutputDownload { get; set; }

        [ParameterInput("output_download.json", UploadFile = true)]
        public string InputUpLoad { get; set; }

        public class InputModel
        {
            public string Text { get; set; }
        }

        public class OutputModel
        {
            public string VersionName { get; set; }
            public string VersionBuild { get; set; }
            public DateTime TimeStart { get; set; } = DateTime.UtcNow;
            public string Text { get; set; }
        }
    }
}