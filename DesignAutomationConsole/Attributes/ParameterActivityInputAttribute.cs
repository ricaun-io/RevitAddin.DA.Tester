using Autodesk.Forge.DesignAutomation.Model;

namespace DesignAutomationConsole.Attributes
{
    public class ParameterActivityInputAttribute : ParameterActivityAttribute
    {
        private readonly string command;

        public ParameterActivityInputAttribute(string command)
        {
            this.command = command;
        }
        public override Activity UpdateActivity(Activity activity, string name, object value)
        {
            var commandLine = $"{command} \"$(args[{name}].path)\"";
            activity.CommandLine.Add(commandLine.Trim());

            return activity;
        }
    }
}