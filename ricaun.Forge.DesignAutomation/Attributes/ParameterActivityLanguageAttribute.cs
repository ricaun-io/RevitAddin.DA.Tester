using Autodesk.Forge.DesignAutomation.Model;

namespace ricaun.Forge.DesignAutomation.Attributes
{
    public class ParameterActivityLanguageAttribute : ParameterActivityAttribute
    {
        public override Activity Update(Activity activity, string name, object value)
        {
            var commandLine = $"/l {value}";
            activity.CommandLine.Add(commandLine);

            return activity;
        }
    }
}