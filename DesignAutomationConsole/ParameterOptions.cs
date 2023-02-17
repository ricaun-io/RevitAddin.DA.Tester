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
            Description = "Output file.",
            DownloadFile = true)]
        public string Output { get; set; }

        [ParameterOutput("output2.json",
            Description = "Output2 file.")]
        public OutputModel Output2 { get; set; }

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

    public class ParameterInputAttribute : Attribute
    {
        public ParameterInputAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Provides default name of the file or folder on the processing server for this parameter. Note this name may be overriden in various ways.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The description of the parameter.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Required { get; set; } = false;
    }

    public class ParameterOutputAttribute : Attribute
    {
        public ParameterOutputAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool DownloadFile { get; set; } = false;
    }
}