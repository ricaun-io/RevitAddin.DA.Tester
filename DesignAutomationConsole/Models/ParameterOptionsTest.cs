using DesignAutomationConsole.Attributes;
using System;

namespace DesignAutomationConsole.Models
{
    public class ParameterOptionsDownloadTest
    {
        [ParameterInput("output.json", UploadFile = true)]
        public InputModel Input { get; set; } = new InputModel() { Text = $"Text me {DateTime.Now}!" };

        [ParameterOutput("output.json")]
        public InputModel Output { get; set; }
    }

    public class ParameterOptionsTest
    {
        [ParameterInput("input.json",
            Description = "Input file.",
            Required = true)]
        public InputModel Input { get; set; }

        [ParameterInput("inputJson.json")]
        public string InputJson { get; set; }

        [ParameterOutput("output.json",
            Description = "Output file.")]
        public string Output { get; set; }

        [ParameterOutput("output_download.json",
            Description = "OutputDownload file.",
            DownloadFile = true)]
        public OutputModel OutputDownload { get; set; }

        [ParameterInput("output_download.json", UploadFile = true)]
        public string InputUpload { get; set; }
    }
}