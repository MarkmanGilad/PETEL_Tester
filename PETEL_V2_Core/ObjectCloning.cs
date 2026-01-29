using System;
using System.Text.Json;

namespace PETEL_VPL
{
    public class ObjectCloning
    {
        public static object? DeepClone(object? obj)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();

            if (type.IsValueType || obj is string)
                return obj;

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
                    $"Failed to clone object of type '{type.Name}'. Ensure the type and all nested types are serializable. Error: {ex.Message}", ex);
            }
        }

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
