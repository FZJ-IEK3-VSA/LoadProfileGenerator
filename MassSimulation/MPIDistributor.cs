using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using MPI;
using System.Diagnostics;

namespace MassSimulation
{
    /// <summary>
    /// Stores all activity messages collected after distribution via MPI in a suitable format for easy distribution.
    /// </summary>
    /// <param name="finishedActivities">finished activity messages</param>
    /// <param name="newTravelActivities">new travel activity messages</param>
    /// <param name="newPoiActivities">new POI activity messages</param>
    internal class SortedMessageCollection(List<RemoteActivityStart> newTravelActivities,
        Dictionary<PointOfInterestId, List<RemoteActivityStart>> newPoiActivities,
        Dictionary<string, Dictionary<HouseholdKey, Dictionary<string, RemoteActivityFinished>>> finishedActivities)
    {
        public List<RemoteActivityStart> NewTravelActivities = newTravelActivities;
        public Dictionary<PointOfInterestId, List<RemoteActivityStart>> NewPoiActivities = newPoiActivities;
        public Dictionary<string, Dictionary<HouseholdKey, Dictionary<string, RemoteActivityFinished>>> finishedActivities = finishedActivities;
    }

    /// <summary>
    /// Container class that collects all messages addressed to one worker
    /// </summary>
    internal class MessageContainer
    {
        public List<RemoteActivityStart> newActivities = [];
        public List<RemoteActivityFinished> finishedActivities = [];

        /// <summary>
        /// Adds a new activity message intended for a specific worker
        /// </summary>
        /// <param name="message">the activity message to add</param>
        public void AddNewActivity(RemoteActivityStart message) => newActivities.Add(message);

        /// <summary>
        /// Adds a finished activity message intended for a specific worker
        /// </summary>
        /// <param name="message">the activity message to add</param>
        public void AddFinishedActivity(RemoteActivityFinished message) => finishedActivities.Add(message);
    }

    /// <summary>
    /// Collects all activity messages and later distributes them across the MPI workers.
    /// </summary>
    internal class MPIDistributor
    {
        /// <summary>
        /// An array storing all messages per target worker. Each index is for the worker with the
        /// corresponding MPI rank.
        /// </summary>
        private readonly MessageContainer[] objectsForWorkers;

        public MPIDistributor(int numWorkers)
        {
            // initialize the array of data collection objects
            objectsForWorkers = new MessageContainer[numWorkers];
            for (int i = 0; i < objectsForWorkers.Length; i++)
            {
                // create one container object for each worker
                objectsForWorkers[i] = new MessageContainer();
            }
        }

        /// <summary>
        /// Adds a new activity message directed to a specific worker.
        /// </summary>
        /// <param name="workerId">the worker on which the target location of the activity is simulated</param>
        /// <param name="message">the activity message to add</param>
        public void AddNewActivity(int workerId, RemoteActivityStart message)
        {
            // send the message to the worker where it takes place
            objectsForWorkers[workerId].AddNewActivity(message);
        }

        /// <summary>
        /// Adds a finished activity message. As these messages need to be sent to the person
        /// they concern, the target worker can be determined automatically.
        /// </summary>
        /// <param name="message"></param>
        public void AddFinishedActivity(RemoteActivityFinished message)
        {
            // send the message to the worker that simulates the affected person
            objectsForWorkers[message.Person.WorkerId].AddFinishedActivity(message);
        }

        /// <summary>
        /// Adds multiple finished activity messages. As these messages need to be sent to the person
        /// they concern, the target worker can be determined automatically.
        /// </summary>
        /// <param name="finishedActivities">the finished activity messages to add</param>
        public void AddFinishedActivities(IEnumerable<RemoteActivityFinished> finishedActivities)
        {
            foreach (var finishedActivity in finishedActivities)
            {
                objectsForWorkers[finishedActivity.Person.WorkerId].AddFinishedActivity(finishedActivity);
            }
        }

        /// <summary>
        /// Sends all stored messages to the intended workers via MPI. Restructures all received messages 
        /// for easier distribution to the correct simulators.
        /// </summary>
        /// <param name="comm">the MPI communicator for distributing the messages</param>
        /// <returns>collection object containing all received activity messages</returns>
        public SortedMessageCollection DistributeMessages(Intracommunicator comm)
        {
            // distribute messages and retrieve the message objects from all other workers in return
            var messageObjectsFromAllWorkers = comm.Alltoall(objectsForWorkers);

            // sort all activity messages into suitable datastructures depending on their target
            Dictionary<string, Dictionary<HouseholdKey, Dictionary<string, RemoteActivityFinished>>> finishedActivitiesSorted = [];
            List<RemoteActivityStart> newTravelActivities = [];
            Dictionary<PointOfInterestId, List<RemoteActivityStart>> newPoiActivities = [];
            foreach (var messageContainer in messageObjectsFromAllWorkers)
            {
                // sort the finished activity messages
                foreach (var finishedActivity in messageContainer.finishedActivities)
                {
                    // get the list for the correct household from the nested dictionary
                    var targetDict = finishedActivitiesSorted.GetOrAddDefault(finishedActivity.Person.TargetId);
                    var householdDict = targetDict.GetOrAddDefault(finishedActivity.Person.HouseholdKey);
                    Debug.Assert(!householdDict.ContainsKey(finishedActivity.Person.PersonName), "Found two 'finished activity' messages for the same person.");
                    // add the finished activity message
                    householdDict[finishedActivity.Person.PersonName] = finishedActivity;
                }

                // sort the new activity messages
                foreach (var newActivity in messageContainer.newActivities)
                {
                    if (newActivity.IsTravel)
                    {
                        // add the activity to the list of travel activities
                        newTravelActivities.Add(newActivity);
                    }
                    else
                    {
                        // add the activity to the activity list of the target POI
                        var poiList = newPoiActivities.GetOrAddDefault(newActivity.Poi!);
                        poiList.Add(newActivity);
                    }
                }
            }
            return new SortedMessageCollection(newTravelActivities, newPoiActivities, finishedActivitiesSorted);
        }
    }
}
