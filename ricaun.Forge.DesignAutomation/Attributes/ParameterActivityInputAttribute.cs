using Autodesk.Forge.DesignAutomation.Model;

namespace ricaun.Forge.DesignAutomation.Attributes
{
    public class ParameterActivityInputAttribute : ParameterActivityAttribute
    {
        private readonly string command;

        public ParameterActivityInputAttribute(string command)
        {
            this.command = command;
        }
        public override Activity Update(Activity activity, string name, object value)
        {
            var commandLine = $"{command} \"$(args[{name}].path)\"";
            activity.CommandLine.Add(commandLine.Trim());

            return activity;
        }
    }
}