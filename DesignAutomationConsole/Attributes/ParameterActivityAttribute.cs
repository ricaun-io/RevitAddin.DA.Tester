﻿using Autodesk.Forge.DesignAutomation.Model;
using System;

namespace DesignAutomationConsole.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class ParameterActivityAttribute : Attribute
    {
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract Activity Update(Activity activity, string name, object value);
    }
}