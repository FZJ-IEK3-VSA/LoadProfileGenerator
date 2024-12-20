using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Automation.ResultFiles;
using Common;

namespace CalculationEngine.ReinforcementLearning
{
    /// <summary>
    /// Represents the state information, including desire states and time of day.
    /// </summary>
    /// <param name="DesireStates">A dictionary containing the desire states with their corresponding levels.</param>
    /// <param name="TimeOfDay">A string representing the time of day associated with this state.</param>
    public record StateInfo(Dictionary<string, int> DesireStates, string TimeOfDay)
    {
        
        /// <summary>
        /// Determines whether the current <see cref="StateInfo"/> instance is equal to another <see cref="StateInfo"/> object.
        /// </summary>
        /// <param name="other">The <see cref="StateInfo"/> object to compare with the current object.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="StateInfo"/> is equal to the current object; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Equals(StateInfo? other)
        {
            if (other == null)
            {
                return false;
            }

            //check if the time of day is equal
            if (!TimeOfDay.Equals(other.TimeOfDay))
            {
                return false;
            }

            //check if the two DesireState dictionaries are equal in count and content
            return DesireStates.Count == other.DesireStates.Count && !DesireStates.Except(other.DesireStates).Any();
        }

        /// <summary>
        /// Serves as the default hash function for the <see cref="StateInfo"/> type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="StateInfo"/> instance.</returns>
        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + TimeOfDay.GetHashCode();

            foreach (var kvp in DesireStates.OrderBy(kvp => kvp.Key))
            {
                hash = hash * 23 + kvp.Key.GetHashCode();
                hash = hash * 23 + kvp.Value.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Serializes the current <see cref="StateInfo"/> instance into a string representation.
        /// </summary>
        /// <returns>A string representation of the current <see cref="StateInfo"/> instance.</returns>
        public string Serialize()
        {
            // Serialize the DesireStates dictionary into a string representation.
            // Each key-value pair in the dictionary is converted to the format "key^value",
            // and pairs are joined together with the "[" delimiter.
            var desireStatesSerialized = string.Join("[", DesireStates.Select(d => $"{d.Key}^{d.Value}"));

            // Combine the serialized DesireStates string with the TimeOfDay string.
            // The two parts are separated by the "]" delimiter.
            return $"{desireStatesSerialized}]{TimeOfDay}";
        }

        /// <summary>
        /// Deserializes a string representation of a <see cref="StateInfo"/> object into an instance.
        /// </summary>
        /// <param name="serialized">The serialized string representation of a <see cref="StateInfo"/>.</param>
        /// <returns>A new <see cref="StateInfo"/> instance created from the serialized string.</returns>
        /// <exception cref="FormatException">
        /// Thrown if the serialized string is not in the expected format.
        /// </exception>
        public static StateInfo Deserialize(string serialized)
        {
            var parts = serialized.Split(']');
            var desireStates = parts[0]
                .Split('[')
                .Select(p => p.Split('^'))
                .ToDictionary(p => p[0], p => int.Parse(p[1]));
            return new StateInfo(desireStates,parts[1]);
        }
    }

    /// <summary>
    /// Represents information about an action, including its Q-value, weight, reward, and the next state.
    /// </summary>
    /// <param name="qValue">The Q-value of the action, representing the expected cumulative reward.</param>
    /// <param name="weightSum">The sum of all weight of affordances, which are relevent with this action.</param>
    /// <param name="rValue">The reward value obtained from performing the action.</param>
    /// <param name="nextState">The resulting <see cref="StateInfo"/> after the action is performed.</param>
    public class ActionInfo(double qValue, int weightSum, double rValue, StateInfo nextState)
    {
        public double QValue { get; set; } = qValue;

        public int WeightSum { get; set; } = weightSum;

        public double RValue { get; set; } = rValue;

        public StateInfo NextState { get; set; } = nextState;

        /// <summary>
        /// Serializes the current <see cref="ActionInfo"/> instance into a string representation.
        /// </summary>
        /// <returns>A string that represents the serialized <see cref="ActionInfo"/> object.</returns>
        public string Serialize()
        {
            // Serialize the NextState property using its Serialize method
            var nextStateSerialized = NextState.Serialize();
            // Combine the QValue, WeightSum, RValue, and the serialized NextState into a single string.
            // Use '%' as the delimiter between the parts.
            return $"{QValue}%{WeightSum}%{RValue}%{nextStateSerialized}";
        }

        /// <summary>
        /// Deserializes a string representation of an <see cref="ActionInfo"/> object into an instance.
        /// </summary>
        /// <param name="serialized">The serialized string representation of an <see cref="ActionInfo"/> object.</param>
        /// <returns>
        /// A new <see cref="ActionInfo"/> instance created from the serialized string.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown if the serialized string is not in the expected format or if the parts cannot be parsed.
        /// </exception>
        public static ActionInfo Deserialize(string serialized)
        {
            // Split the serialized string using '%' as the delimiter.
            var parts = serialized.Split('%');
            // Parse each part to reconstruct the original object.
            var qValue = double.Parse(parts[0]); // Parse QValue
            var weightSum = int.Parse(parts[1]); // Parse WeightSum
            var rValue = double.Parse(parts[2]); // Parse RValue
            // Deserialize the NextState property using StateInfo's Deserialize method.
            var nextState = StateInfo.Deserialize(parts[3]);
            // Return a new ActionInfo instance with the parsed values.
            return new ActionInfo(qValue, weightSum, rValue, nextState);
        }
    }

    /// <summary>
    /// Represents a Q-Table that stores state-action infos for reinforcement learning.
    /// </summary>
    public class QTable
    {
        /// <summary>
        /// Gets the table containing the state-action mappings.
        /// </summary>
        /// <remarks>
        /// The outer dictionary maps <see cref="StateInfo"/> objects to an inner dictionary.
        /// The inner dictionary maps action names (as strings) to their corresponding <see cref="ActionInfo"/> objects.
        /// </remarks>
        public ConcurrentDictionary<StateInfo, ConcurrentDictionary<string, ActionInfo>> Table { get; } = [];

        /// <summary>
        /// Gets the number of States in the Q-table.
        /// </summary>
        public int StatesCount => Table.Count;

        /// <summary>
        /// Adds or updates the action details for a specific state-action pair in the Q-Table.
        /// </summary>
        /// <param name="state">The state information as a <see cref="StateInfo"/> object.</param>
        /// <param name="action">The action name as a string.</param>
        /// <param name="actionDetails">The <see cref="ActionInfo"/> object containing details about the action.</param>
        /// <remarks>
        /// This method is designed for use with a <see cref="ConcurrentDictionary{TKey, TValue}"/>, ensuring thread safety when multiple threads 
        /// simultaneously attempt to add or update entries in the Q-Table. It avoids race conditions by using atomic operations for both
        /// adding new entries and updating existing ones.
        /// </remarks>
        public void AddOrUpdate(StateInfo state, string action, ActionInfo actionDetails)
        {
            // Add or update the Q-Table entry for the given state.
            Table.AddOrUpdate(
                state,
                // If the state does not exist, create a new dictionary with the action.
                new ConcurrentDictionary<string, ActionInfo> { [action] = actionDetails },
                // If the state exists, update the existing dictionary.
                (_, existingActions) =>
                {
                    // Add or update the action details within the state's action dictionary.
                    existingActions.AddOrUpdate(action, actionDetails, (_, __) => actionDetails);
                    return existingActions;
                });
        }

        /// <summary>
        /// Retrieves the <see cref="ActionInfo"/> for a specific state-action pair, or adds a default action if it does not exist.
        /// </summary>
        /// <param name="state">The state information as a <see cref="StateInfo"/> object.</param>
        /// <param name="action">The action name as a string.</param>
        /// <returns>
        /// The <see cref="ActionInfo"/> object associated with the specified state-action pair.
        /// If the pair does not exist, a new default <see cref="ActionInfo"/> object is added and returned.
        /// </returns>
        /// <remarks>
        /// This method is designed for use with a <see cref="ConcurrentDictionary{TKey, TValue}"/>, ensuring thread safety when multiple threads 
        /// simultaneously attempt to retrieve or add entries in the Q-Table. It avoids race conditions by using atomic operations for both
        /// getting existing entries and adding new ones.
        /// </remarks>
        public ActionInfo GetOrAdd(StateInfo state, string action)
        {
            // Get or add the dictionary of actions for the specified state.
            var actions = Table.GetOrAdd(
                state,
                // If the state does not exist, create a new empty dictionary.
                _ => new ConcurrentDictionary<string, ActionInfo>()
            );
            // Get or add the action for the specified state.
            return actions.GetOrAdd(action, _ => new ActionInfo(0, 0, 0, state));
        }

        /// <summary>
        /// Generates the base directory and file path for storing or retrieving the Q-Table based on the person's name.
        /// </summary>
        /// <param name="personName">The name of the person, used to generate the file name.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item>The base directory for storing Q-Table files.</item>
        /// <item>The full file path of the personal Q-Table file.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method ensures that the base directory exists and sanitizes the person's name to avoid invalid file names.
        /// </remarks>
        public static (string, string) GetQTablePath(string personName)
        {
            // Define the base directory for storing Q-Table files.
            string baseDir = @"ActivityModels";

            // Ensure the directory exists; create it if necessary.
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }
            // Sanitize the person's name to remove invalid characters.
            string sanitizedPersonName = personName.Replace("/", "_");
            // Generate the file path for the Q-Table.
            string filePath = Path.Combine(baseDir, $"qTable-{sanitizedPersonName}.json");
            // Return the base directory and the file path as a tuple.
            return (baseDir, filePath);
        }

        /// <summary>
        /// Serializes the Q-Table into a JSON string representation.
        /// </summary>
        /// <returns>A JSON string that represents the serialized Q-Table.</returns>
        /// <remarks>
        /// This method serializes the outer dictionary keys using the <see cref="StateInfo.Serialize"/> method
        /// and the inner dictionary values using the <see cref="ActionInfo.Serialize"/> method.
        /// </remarks>
        public string Serialize()
        {
            // Create a dictionary to hold the serialized Q-Table.
            var convertedQTable = new Dictionary<string, string>();
            foreach (var outerEntry in Table)
            {
                // Serialize the outer key (StateInfo).
                var outerKey = outerEntry.Key.Serialize();
                // Serialize the inner dictionary (ActionInfo).
                var innerDictSerialized = outerEntry.Value.Select(innerEntry =>
                    $"{innerEntry.Key}@{innerEntry.Value.Serialize()}"
                );
                // Combine the serialized inner dictionary into a single string with '~' as the delimiter.
                convertedQTable[outerKey] = string.Join("~", innerDictSerialized);
            }
            // Convert the serialized dictionary to a JSON string with indentation options.
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(convertedQTable, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="QTable"/> object.
        /// </summary>
        /// <param name="jsonString">The JSON string representing a serialized Q-Table.</param>
        /// <returns>
        /// A new <see cref="QTable"/> object reconstructed from the JSON string.
        /// </returns>
        /// <remarks>
        /// This method deserializes the outer dictionary keys using the <see cref="StateInfo.Deserialize"/> method
        /// and the inner dictionary values using the <see cref="ActionInfo.Deserialize"/> method.
        /// </remarks>
        /// <exception cref="JsonException">
        /// Thrown if the JSON string cannot be deserialized into the expected format.
        /// </exception>
        public static QTable Deserialize(string jsonString)
        {
            // Deserialize the JSON string into a dictionary.
            var convertedQTable = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            QTable qTable = new();
            foreach (var outerEntry in convertedQTable)
            {
                // Deserialize the outer key (StateInfo).
                var outerKey = StateInfo.Deserialize(outerEntry.Key);
                // Deserialize the inner dictionary (ActionInfo).
                var innerDict = outerEntry.Value
                    .Split('~')
                    .Select(innerSerialized =>
                    {
                        // Split each serialized entry into action name and serialized ActionInfo.
                        var parts = innerSerialized.Split('@');
                        var actionName = parts[0];
                        var actionInfo = ActionInfo.Deserialize(parts[1]);
                        return new KeyValuePair<string, ActionInfo>(actionName, actionInfo);
                    })
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                // Add the deserialized inner dictionary to the Q-Table.
                qTable.Table[outerKey] = new ConcurrentDictionary<string, ActionInfo>(innerDict);
            }
            return qTable;
        }

        /// <summary>
        /// Saves the current Q-Table to a JSON file for the specified person.
        /// </summary>
        /// <param name="personName">The name of the person, used to generate the file name.</param>
        /// <remarks>
        /// This method serializes the Q-Table into a JSON string and writes it to a file.
        /// It ensures the target directory exists before saving the file. The file path is dynamically 
        /// generated based on the person's name. If an error occurs during the process, an exception is thrown.
        /// </remarks>
        /// <exception cref="LPGException">Thrown if an error occurs while saving the Q-Table.</exception>
        public void SaveQTableToFile_RL(string personName)
        {
            // Get the base directory and file path for the Q-Table.
            (string baseDir, string filePath) = GetQTablePath(personName);

            try
            {
                // Serialize the Q-Table to a JSON string.
                var jsonString = Serialize();

                // Write the serialized string to the file.
                File.WriteAllText(filePath, jsonString);

                // Log and debug the success message with the full file path.
                Logger.Info("QTable has been successfully saved to " + Path.GetFullPath(filePath));
            }
            catch (Exception ex)
            {
                // Log and throw an exception if an error occurs during saving.
                Logger.Info("Error saving QTable: " + ex.Message);
                throw new LPGException("Error in Q-Table Saving");
            }
        }

        /// <summary>
        /// Loads a Q-Table from a JSON file for the specified person.
        /// </summary>
        /// <param name="personName">The name of the person, used to locate the file.</param>
        /// <returns>
        /// A <see cref="QTable"/> object reconstructed from the JSON file.
        /// If no file exists, a new, empty Q-Table is returned.
        /// </returns>
        /// <remarks>
        /// This method reads a JSON file containing the serialized Q-Table and deserializes it 
        /// into a <see cref="QTable"/> object. If the file does not exist, a new Q-Table is initialized.
        /// If an error occurs during deserialization, an exception is thrown.
        /// </remarks>
        /// <exception cref="LPGException">Thrown if an error occurs while loading the Q-Table.</exception>
        public static QTable LoadQTableFromFile_RL(string personName)
        {
            // Get the file path for the Q-Table.
            (_, string filePath) = GetQTablePath(personName);

            // If the file does not exist, return a new, empty Q-Table.
            if (!File.Exists(filePath))
            {
                Logger.Info("No saved QTable found. Initializing a new QTable.");
                return new QTable();
            }

            try
            {
                // Read the JSON string from the file.
                var jsonString = File.ReadAllText(filePath);

                // Deserialize the JSON string into a QTable object.
                var qTable = QTable.Deserialize(jsonString);

                // Log and debug the success message with the full file path.
                Logger.Info("QTable has been successfully loaded from " + Path.GetFullPath(filePath));
                return qTable;
            }
            catch (Exception ex)
            {
                // Log and throw an exception if an error occurs during loading.
                Logger.Info("Error loading QTable: " + ex.Message);
                throw new LPGException("Error in Q-Table Loading");
            }
        }
    }
}