using System;

namespace DesignAutomationConsole.Models
{
    public class OutputModel
    {
        public string VersionName { get; set; }
        public string VersionBuild { get; set; }
        public DateTime TimeStart { get; set; } = DateTime.UtcNow;
        public string Text { get; set; }
    }
}