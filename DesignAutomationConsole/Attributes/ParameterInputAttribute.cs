using System;

namespace DesignAutomationConsole.Attributes
{
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
        /// Required
        /// </summary>
        public bool Required { get; set; } = false;
        /// <summary>
        /// UploadFile
        /// </summary>
        public bool UploadFile { get; set; } = false;
    }
}