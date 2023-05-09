﻿using Autodesk.Forge.DesignAutomation.Model;
using System;

namespace DesignAutomationConsole.Attributes
{
    public class ParameterActivityInputArgumentAttribute : ParameterActivityInputAttribute
    {
        public ParameterActivityInputArgumentAttribute() : base("") { }
    }
    public class ParameterActivityInputOpenAttribute : ParameterActivityInputAttribute
    {
        public ParameterActivityInputOpenAttribute() : base("/i") { }
    }
    public class ParameterActivityInputAttribute : ParameterActivityAttribute
    {
        private readonly string command;

        public ParameterActivityInputAttribute(string command)
        {
            this.command = command;
        }
        public override Activity UpdateActivity(Activity activity, string name, object value)
        {
            activity.CommandLine[0] += $" {command} \"$(args[{name}].path)\"";

            return base.UpdateActivity(activity, name, value);
        }
    }
    public class ParameterActivityLanguageAttribute : ParameterActivityAttribute
    {
        public override Activity UpdateActivity(Activity activity, string name, object value)
        {
            activity.CommandLine[0] += $" /l {value}";

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


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterActivityAttribute : Attribute
    {
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