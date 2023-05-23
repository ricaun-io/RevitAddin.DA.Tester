using Autodesk.Forge.DesignAutomation.Model;
using System;

namespace ricaun.Forge.DesignAutomation.Attributes
{
    /// <summary>
    /// ParameterWorkItemAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class ParameterWorkItemAttribute : Attribute
    {
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract WorkItem Update(WorkItem workItem, string name, object value);
    }
}