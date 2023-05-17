using Autodesk.Forge.DesignAutomation.Model;
using System.Linq;

namespace DesignAutomationConsole.Attributes
{
    public class ParameterActivityClearBundleAttribute : ParameterActivityAttribute
    {
        public override Activity Update(Activity activity, string name, object value)
        {
            var commandLine = "/al";
            if (activity.CommandLine.Remove(activity.CommandLine.FirstOrDefault(e => e.StartsWith(commandLine))))
            {
                activity.Appbundles.Clear();
            }
            return activity;
        }
    }
}