using Autodesk.Forge.DesignAutomation.Model;
using System;

namespace DesignAutomationConsole.Attributes
{
    public class ParameterActivityLanguageAttribute : ParameterActivityAttribute
    {
        public override Activity UpdateActivity(Activity activity, string name, object value)
        {
            activity.CommandLine[0] += $" /l {value}";

            return base.UpdateActivity(activity, name, value);
        }
    }
    public class ParameterActivityInputOpenAttribute : ParameterActivityAttribute
    {
        public override Activity UpdateActivity(Activity activity, string name, object value)
        {
            activity.CommandLine[0] += $" /i \"$(args[{name}].path)\"";

            return base.UpdateActivity(activity, name, value);
        }
    }

    public class ParameterActivityScriptAttribute : ParameterActivityAttribute
    {
        public override Activity UpdateActivity(Activity activity, string name, object value)
        {
            activity.CommandLine[0] += $" /s \"$(settings[{name}].path)\"";
            activity.Settings[name] = new StringSetting() { Value = value.ToString() };

            return base.UpdateActivity(activity, name, value);
        }
    }

    public class ParameterActivityAttribute : Attribute
    {
        public ParameterActivityAttribute()
        {

        }

        /// <summary>
        /// UpdateActivity
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual Activity UpdateActivity(Activity activity, string name, object value)
        {
            return activity;
        }
    }
}