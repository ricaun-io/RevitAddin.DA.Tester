using Autodesk.Forge.DesignAutomation.Model;
using System;

namespace DesignAutomationConsole.Services
{
    /// <summary>
    /// ForgeDAExtension
    /// </summary>
    public static class ForgeDAExtension
    {
        /// <summary>
        /// ProgressEstimateCosts
        /// </summary>
        /// <param name="workItemStatus"></param>
        /// <returns></returns>
        public static WorkItemStatus ProgressEstimateCosts(this WorkItemStatus workItemStatus)
        {
            workItemStatus.Progress = $"EstimateCosts: {workItemStatus.EstimateCosts()}";
            return workItemStatus;
        }


        /// <summary>
        /// EstimateCosts
        /// </summary>
        /// <param name="workItemStatus"></param>
        /// <returns></returns>
        public static double EstimateCosts(this WorkItemStatus workItemStatus, double costMultiplier = 2.0)
        {
            var costs = 0.0;

            if (workItemStatus.Stats is Statistics statistics)
            {
                if (statistics.TimeDownloadStarted is DateTime started)
                {
                    if (statistics.TimeUploadEnded is DateTime ended)
                    {
                        costs = (ended - started).TotalHours;
                    }
                }
            }
            return costs * costMultiplier;
        }
    }
}
