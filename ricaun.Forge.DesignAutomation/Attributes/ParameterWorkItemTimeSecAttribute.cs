using Autodesk.Forge.DesignAutomation.Model;

namespace ricaun.Forge.DesignAutomation.Attributes
{
    public class ParameterWorkItemTimeSecAttribute : ParameterWorkItemAttribute
    {
        public override WorkItem Update(WorkItem workItem, string name, object value)
        {
            if (value is int valueInt)
                workItem.LimitProcessingTimeSec = valueInt;

            return workItem;
        }
    }
}