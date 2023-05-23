using Autodesk.Forge.DesignAutomation.Model;

namespace ricaun.Forge.DesignAutomation.Attributes
{
    public class ParameterActivityScriptAttribute : ParameterActivityAttribute
    {
        public override Activity Update(Activity activity, string name, object value)
        {
            var commandLine = $"/s \"$(settings[{name}].path)\"";
            activity.CommandLine.Add(commandLine);
            activity.Settings[name] = new StringSetting() { Value = value.ToString() };

            return activity;
        }
    }
}