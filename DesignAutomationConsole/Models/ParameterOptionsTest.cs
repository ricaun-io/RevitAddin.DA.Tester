using DesignAutomationConsole.Attributes;
using System;

namespace DesignAutomationConsole.Models
{
    public class ParameterOptionsDownloadTest
    {
        [ParameterInput("output.json", UploadFile = true)]
        public OutputModel Input { get; set; } = new OutputModel() { Text = $"Local OutputModel - {DateTime.Now}!" };

        [ParameterOutput("output.json")]
        public OutputModel Output { get; set; }
    }

    public class ParameterOptionsDownloadTest2
    {
        [ParameterInput("output.txt", UploadFile = true)]
        public string Input { get; set; } = "input test string";

        [ParameterOutput("output.txt", DownloadFile = true)]
        public string Output { get; set; }
    }

    public class ParameterOptionsDownloadTest3
    {
        [ParameterInput("output.json")]
        public string Input { get; set; } = "input.json";

        [ParameterOutput("output.json")]
        public OutputModel Output { get; set; }
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