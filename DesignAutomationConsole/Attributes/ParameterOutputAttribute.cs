using System;

namespace DesignAutomationConsole.Attributes
{
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