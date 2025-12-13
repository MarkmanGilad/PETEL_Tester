using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PETEL_VPL
{
    /// <summary>
    /// Handles deep cloning of objects using binary serialization.
    /// Recursively clones entire object graphs including nested structures.
    /// </summary>
    public class ObjectCloning
    {
        /// <summary>
        /// Deep clone any object using binary serialization.
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

            // Binary serialization recursively clones everything
            try
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, obj);
                    ms.Position = 0;
                    return formatter.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to clone object of type '{type.Name}'. " +
                    $"Ensure the type and all nested types are marked with [Serializable] attribute. " +
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