#region

using Automation.ResultFiles;
using CalculationEngine.Activities;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace CalculationEngine.HouseholdElements
{
    public class ActivityQueue
    {
        /// <summary>
        /// Stores the next planned activities or activity steps.
        /// </summary>
        private readonly LinkedList<IActivity> activityQueue = [];

        public IActivity CurrentActivity => activityQueue.First?.Value ?? throw new LPGException("There is no currently active activity");

        public bool IsEmpty => activityQueue.Count == 0;

        public void AddActivities(IEnumerable<IActivity> activities)
        {
            foreach (var activity in activities)
            {
                activityQueue.AddLast(activity);
            }
        }

        public void RemoveCurrentActivity() => activityQueue.RemoveFirst();

        public void AddFirst(IEnumerable<IActivity> activities)
        {
            foreach (var activity in activities.Reverse())
            {
                activityQueue.AddFirst(activity);
            }
        }
    }
}