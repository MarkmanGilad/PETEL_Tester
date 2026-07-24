using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unit4;

namespace PETEL_VPL
{
    public class ObjectCloning
    {
        public static object? DeepClone(object? obj)
        {
            return DeepClone(obj, new CloneContext());
        }

        private static object? DeepClone(object? obj, CloneContext context)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();

            if (type.IsValueType || obj is string)
                return obj;

            if (context.TryGetClone(obj, out object existingClone))
                return existingClone;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Node<>))
                return CloneNodeList(obj, type, context);

            if (type == typeof(NodeInteger))
                return CloneNodeInteger((NodeInteger)obj, context);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Unit4.Queue<>))
                return CloneQueue(obj, type, context);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Unit4.Stack<>))
                return CloneStack(obj, type, context);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BinNode<>))
                return CloneBinNode(obj, type, context);

            if (type.IsArray)
                return CloneArray((Array)obj, context);

            return CloneByReflection(obj, type, context);
        }

        private static object CloneByReflection(object obj, Type type, CloneContext context)
        {
            object clone = Activator.CreateInstance(type, nonPublic: true)
                           ?? throw new InvalidOperationException($"Failed to create instance of type '{type.FullName}'.");

            // Register before copying fields so a field may refer back to this object.
            context.Remember(obj, clone);

            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var value = f.GetValue(obj);
                f.SetValue(clone, DeepClone(value, context));
            }

            return clone;
        }

        private static object? CloneNodeList(object node, Type nodeType, CloneContext context)
        {
            var getValue = nodeType.GetMethod("GetValue")!;
            var getNext = nodeType.GetMethod("GetNext")!;
            var setNext = nodeType.GetMethod("SetNext")!;
            var elementType = nodeType.GetGenericArguments()[0];

            var value = getValue.Invoke(node, null);
            var clonedValue = DeepClone(value, context);

            var constructor = nodeType.GetConstructor(new[] { elementType })!;
            var clonedNode = constructor.Invoke(new[] { clonedValue });

            // Register before cloning Next. This retains pointers into the list and supports cycles.
            context.Remember(node, clonedNode);

            var next = getNext.Invoke(node, null);
            var clonedNext = DeepClone(next, context);
            setNext.Invoke(clonedNode, new[] { clonedNext });

            return clonedNode;
        }

        private static NodeInteger CloneNodeInteger(
            NodeInteger node,
            CloneContext context)
        {
            var clonedNode = new NodeInteger(node.GetValue());

            // Register before cloning Next. A separately passed pointer into this list
            // must resolve to the corresponding node in the cloned list.
            context.Remember(node, clonedNode);
            clonedNode.SetNext((NodeInteger?)DeepClone(node.GetNext(), context));

            return clonedNode;
        }

        private static object CloneQueue(object queue, Type queueType, CloneContext context)
        {
            var isEmpty = queueType.GetMethod("IsEmpty")!;
            var remove = queueType.GetMethod("Remove")!;
            var insert = queueType.GetMethod("Insert")!;

            var clonedQueue = Activator.CreateInstance(queueType)!;
            context.Remember(queue, clonedQueue);

            var items = new List<object?>();

            while (!(bool)isEmpty.Invoke(queue, null)!)
                items.Add(remove.Invoke(queue, null));

            foreach (var item in items)
            {
                insert.Invoke(queue, new[] { item });
                insert.Invoke(clonedQueue, new[] { DeepClone(item, context) });
            }

            return clonedQueue;
        }

        private static object CloneStack(object stack, Type stackType, CloneContext context)
        {
            var isEmpty = stackType.GetMethod("IsEmpty")!;
            var pop = stackType.GetMethod("Pop")!;
            var push = stackType.GetMethod("Push")!;

            var clonedStack = Activator.CreateInstance(stackType)!;
            context.Remember(stack, clonedStack);

            var items = new List<object?>();

            while (!(bool)isEmpty.Invoke(stack, null)!)
                items.Add(pop.Invoke(stack, null));

            for (int i = items.Count - 1; i >= 0; i--)
            {
                push.Invoke(stack, new[] { items[i] });
                push.Invoke(clonedStack, new[] { DeepClone(items[i], context) });
            }

            return clonedStack;
        }

        private static object CloneBinNode(object node, Type nodeType, CloneContext context)
        {
            var getValue = nodeType.GetMethod("GetValue")!;
            var getLeft = nodeType.GetMethod("GetLeft")!;
            var getRight = nodeType.GetMethod("GetRight")!;
            var setLeft = nodeType.GetMethod("SetLeft")!;
            var setRight = nodeType.GetMethod("SetRight")!;
            var elementType = nodeType.GetGenericArguments()[0];

            var value = getValue.Invoke(node, null);
            var clonedValue = DeepClone(value, context);

            var constructor = nodeType.GetConstructor(new[] { elementType })!;
            var clonedNode = constructor.Invoke(new[] { clonedValue });

            // Register before cloning children so shared children and cycles stay intact.
            context.Remember(node, clonedNode);

            var left = getLeft.Invoke(node, null);
            var right = getRight.Invoke(node, null);
            setLeft.Invoke(clonedNode, new[] { DeepClone(left, context) });
            setRight.Invoke(clonedNode, new[] { DeepClone(right, context) });

            return clonedNode;
        }

        private static Array CloneArray(Array arr, CloneContext context)
        {
            var elementType = arr.GetType().GetElementType()!;
            var cloned = Array.CreateInstance(elementType, arr.Length);
            context.Remember(arr, cloned);

            for (int i = 0; i < arr.Length; i++)
                cloned.SetValue(DeepClone(arr.GetValue(i), context), i);

            return cloned;
        }

        public static object[] DeepCloneArray(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return parameters;

            var context = new CloneContext();
            object[] cloned = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                cloned[i] = DeepClone(parameters[i], context)!;

            return cloned;
        }

        private sealed class CloneContext
        {
            private readonly Dictionary<object, object> clones =
                new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

            public bool TryGetClone(object original, out object clone)
            {
                return clones.TryGetValue(original, out clone!);
            }

            public void Remember(object original, object clone)
            {
                clones.Add(original, clone);
            }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            public new bool Equals(object? x, object? y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
