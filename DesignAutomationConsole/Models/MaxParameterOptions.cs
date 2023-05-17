﻿using DesignAutomationConsole.Attributes;

namespace DesignAutomationConsole.Models
{
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

}