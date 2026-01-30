using System;
using System.Text.Json;
using Unit4;

namespace PETEL_VPL
{
    public class ObjectCloning
    {
        public static object? DeepClone(object? obj)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();

            // Value types and strings are immutable - no need to clone
            if (type.IsValueType || obj is string)
                return obj;

            // Special handling for Unit4.Node<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Node<>))
            {
                return CloneNodeList(obj, type);
            }

            // Special handling for Unit4.Queue<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Queue<>))
            {
                return CloneQueue(obj, type);
            }

            // Special handling for Unit4.Stack<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Stack<>))
            {
                return CloneStack(obj, type);
            }

            // Special handling for Unit4.BinNode<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BinNode<>))
            {
                return CloneBinNode(obj, type);
            }

            // Handle arrays
            if (type.IsArray)
            {
                return CloneArray((Array)obj);
            }

            // Fallback to JSON serialization for other types
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

        private static object? CloneNodeList(object node, Type nodeType)
        {
            if (node == null)
                return null;

            var getValue = nodeType.GetMethod("GetValue")!;
            var getNext = nodeType.GetMethod("GetNext")!;
            var elementType = nodeType.GetGenericArguments()[0];

            // Get value and next from original node
            var value = getValue.Invoke(node, null);
            var next = getNext.Invoke(node, null);

            // Clone the value if it's a reference type
            var clonedValue = DeepClone(value);

            // Recursively clone the rest of the list
            var clonedNext = CloneNodeList(next, nodeType);

            // Create new node with cloned value
            var constructor = nodeType.GetConstructor(new[] { elementType })!;
            var clonedNode = constructor.Invoke(new[] { clonedValue });

            // Set the next pointer
            var setNext = nodeType.GetMethod("SetNext")!;
            setNext.Invoke(clonedNode, new[] { clonedNext });

            return clonedNode;
        }

        private static object CloneQueue(object queue, Type queueType)
        {
            var isEmpty = queueType.GetMethod("IsEmpty")!;
            var remove = queueType.GetMethod("Remove")!;
            var insert = queueType.GetMethod("Insert")!;
            var elementType = queueType.GetGenericArguments()[0];

            // Create new queue
            var clonedQueue = Activator.CreateInstance(queueType)!;
            
            // Temporary storage
            var items = new System.Collections.Generic.List<object?>();

            // Remove all items from original queue
            while (!(bool)isEmpty.Invoke(queue, null)!) {
                var item = remove.Invoke(queue, null);
                items.Add(item);
            }

            // Restore original queue and populate cloned queue
            foreach (var item in items)
            {
                insert.Invoke(queue, new[] { item });
                var clonedItem = DeepClone(item);
                insert.Invoke(clonedQueue, new[] { clonedItem });
            }

            return clonedQueue;
        }

        private static object CloneStack(object stack, Type stackType)
        {
            var isEmpty = stackType.GetMethod("IsEmpty")!;
            var pop = stackType.GetMethod("Pop")!;
            var push = stackType.GetMethod("Push")!;

            // Create new stack
            var clonedStack = Activator.CreateInstance(stackType)!;

            // Temporary storage (to preserve order)
            var items = new System.Collections.Generic.List<object?>();

            // Pop all items from original
            while (!(bool)isEmpty.Invoke(stack, null)!) {
                items.Add(pop.Invoke(stack, null));
            }

            // Restore original stack in reverse order and clone to new stack
            for (int i = items.Count - 1; i >= 0; i--)
            {
                push.Invoke(stack, new[] { items[i] });
                var clonedItem = DeepClone(items[i]);
                push.Invoke(clonedStack, new[] { clonedItem });
            }

            return clonedStack;
        }

        private static object? CloneBinNode(object node, Type nodeType)
        {
            if (node == null)
                return null;

            var getValue = nodeType.GetMethod("GetValue")!;
            var getLeft = nodeType.GetMethod("GetLeft")!;
            var getRight = nodeType.GetMethod("GetRight")!;
            var setLeft = nodeType.GetMethod("SetLeft")!;
            var setRight = nodeType.GetMethod("SetRight")!;
            var elementType = nodeType.GetGenericArguments()[0];

            // Get value and children
            var value = getValue.Invoke(node, null);
            var left = getLeft.Invoke(node, null);
            var right = getRight.Invoke(node, null);

            // Clone value
            var clonedValue = DeepClone(value);

            // Create new node
            var constructor = nodeType.GetConstructor(new[] { elementType })!;
            var clonedNode = constructor.Invoke(new[] { clonedValue });

            // Recursively clone children
            var clonedLeft = CloneBinNode(left, nodeType);
            var clonedRight = CloneBinNode(right, nodeType);

            // Set children
            setLeft.Invoke(clonedNode, new[] { clonedLeft });
            setRight.Invoke(clonedNode, new[] { clonedRight });

            return clonedNode;
        }

        private static Array CloneArray(Array arr)
        {
            var elementType = arr.GetType().GetElementType()!;
            var cloned = Array.CreateInstance(elementType, arr.Length);

            for (int i = 0; i < arr.Length; i++)
            {
                cloned.SetValue(DeepClone(arr.GetValue(i)), i);
            }

            return cloned;
        }

        public static object[] DeepCloneArray(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return parameters;

            object[] cloned = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                cloned[i] = DeepClone(parameters[i])!;
            }
            return cloned;
        }
    }
}
