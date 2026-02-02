using System;
using System.Reflection;
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

            if (type.IsValueType || obj is string)
                return obj;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Node<>))
                return CloneNodeList(obj, type);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Queue<>))
                return CloneQueue(obj, type);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Stack<>))
                return CloneStack(obj, type);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BinNode<>))
                return CloneBinNode(obj, type);

            if (type.IsArray)
                return CloneArray((Array)obj);

            return CloneByReflection(obj, type);
        }

        private static object CloneByReflection(object obj, Type type)
        {
            object clone = Activator.CreateInstance(type, nonPublic: true)
                           ?? throw new InvalidOperationException($"Failed to create instance of type '{type.FullName}'.");

            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var value = f.GetValue(obj);
                f.SetValue(clone, DeepClone(value));
            }

            return clone;
        }

        private static object? CloneNodeList(object node, Type nodeType)
        {
            if (node == null)
                return null;

            var getValue = nodeType.GetMethod("GetValue")!;
            var getNext = nodeType.GetMethod("GetNext")!;
            var elementType = nodeType.GetGenericArguments()[0];

            var value = getValue.Invoke(node, null);
            var next = getNext.Invoke(node, null);

            var clonedValue = DeepClone(value);
            var clonedNext = CloneNodeList(next, nodeType);

            var constructor = nodeType.GetConstructor(new[] { elementType })!;
            var clonedNode = constructor.Invoke(new[] { clonedValue });

            var setNext = nodeType.GetMethod("SetNext")!;
            setNext.Invoke(clonedNode, new[] { clonedNext });

            return clonedNode;
        }

        private static object CloneQueue(object queue, Type queueType)
        {
            var isEmpty = queueType.GetMethod("IsEmpty")!;
            var remove = queueType.GetMethod("Remove")!;
            var insert = queueType.GetMethod("Insert")!;

            var clonedQueue = Activator.CreateInstance(queueType)!;

            var items = new System.Collections.Generic.List<object?>();

            while (!(bool)isEmpty.Invoke(queue, null)!)
                items.Add(remove.Invoke(queue, null));

            foreach (var item in items)
            {
                insert.Invoke(queue, new[] { item });
                insert.Invoke(clonedQueue, new[] { DeepClone(item) });
            }

            return clonedQueue;
        }

        private static object CloneStack(object stack, Type stackType)
        {
            var isEmpty = stackType.GetMethod("IsEmpty")!;
            var pop = stackType.GetMethod("Pop")!;
            var push = stackType.GetMethod("Push")!;

            var clonedStack = Activator.CreateInstance(stackType)!;

            var items = new System.Collections.Generic.List<object?>();

            while (!(bool)isEmpty.Invoke(stack, null)!)
                items.Add(pop.Invoke(stack, null));

            for (int i = items.Count - 1; i >= 0; i--)
            {
                push.Invoke(stack, new[] { items[i] });
                push.Invoke(clonedStack, new[] { DeepClone(items[i]) });
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

            var value = getValue.Invoke(node, null);
            var left = getLeft.Invoke(node, null);
            var right = getRight.Invoke(node, null);

            var clonedValue = DeepClone(value);

            var constructor = nodeType.GetConstructor(new[] { elementType })!;
            var clonedNode = constructor.Invoke(new[] { clonedValue });

            var clonedLeft = CloneBinNode(left, nodeType);
            var clonedRight = CloneBinNode(right, nodeType);

            setLeft.Invoke(clonedNode, new[] { clonedLeft });
            setRight.Invoke(clonedNode, new[] { clonedRight });

            return clonedNode;
        }

        private static Array CloneArray(Array arr)
        {
            var elementType = arr.GetType().GetElementType()!;
            var cloned = Array.CreateInstance(elementType, arr.Length);

            for (int i = 0; i < arr.Length; i++)
                cloned.SetValue(DeepClone(arr.GetValue(i)), i);

            return cloned;
        }

        public static object[] DeepCloneArray(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return parameters;

            object[] cloned = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                cloned[i] = DeepClone(parameters[i])!;

            return cloned;
        }
    }
}
