using System;
using System.Text.Json;

namespace PETEL_VPL
{
    /// <summary>
    /// Handles deep cloning of objects using binary serialization.
    /// Recursively clones entire object graphs including nested structures.
    /// </summary>
    public class ObjectCloning
    {
        /// <summary>
        /// Deep clone any object using JSON serialization.
        /// Handles all types: primitives, arrays, classes with queues, nested objects, etc.
        /// The serialization is fully recursive - clones everything in the object graph.
        /// </summary>
        /// <param name="obj">Object to clone</param>
        /// <returns>Deep copy of the object with all nested structures cloned</returns>
        public static object DeepClone(object obj)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();

            // Value types and strings are immutable - no need to clone
            if (type.IsValueType || obj is string)
                return obj;

            // JSON serialization recursively clones everything
            try
            {
                var options = new JsonSerializerOptions
                {
                    IncludeFields = true,
                    WriteIndented = false,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                };

                string json = JsonSerializer.Serialize(obj, type, options);
                return JsonSerializer.Deserialize(json, type, options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to clone object of type '{type.Name}'. " +
                    $"Ensure the type and all nested types are serializable. " +
                    $"Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deep clone an array of objects.
        /// Each element is cloned independently to prevent cross-contamination.
        /// </summary>
        /// <param name="parameters">Array of objects to clone</param>
        /// <returns>New array with cloned objects</returns>
        public static object[] DeepCloneArray(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return parameters;

            object[] cloned = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                cloned[i] = DeepClone(parameters[i]);
            }
            return cloned;
        }
    }
}