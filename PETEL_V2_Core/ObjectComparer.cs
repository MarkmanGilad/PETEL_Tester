using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PETEL_VPL
{
    /// <summary>
    /// Handles deep comparison of objects including linked data structures
    /// with cycle detection support.
    /// </summary>
    public class ObjectComparer
    {
        private readonly HashSet<Tuple<object, object>> comparedPairs;

        public ObjectComparer()
        {
            comparedPairs = new HashSet<Tuple<object, object>>(new PairEqualityComparer());
        }

        public bool AreEqual(object expected, object actual)
        {
            comparedPairs.Clear();
            return AreEqualDeep(expected, actual);
        }

        public string GetDifferenceDescription(object expected, object actual)
        {
            if (expected == null || actual == null)
                return expected == null && actual == null ? "" : "One side is null";

            Type type = expected.GetType();

            if (IsBinNodeType(type))
                return GetBinNodeDifference(expected, actual);
            if (IsNodeType(type))
                return GetNodeDifference(expected, actual);
            if (IsStackType(type))
                return "Stack contents differ";
            if (IsQueueType(type))
                return "Queue contents differ";
            if (type.IsArray)
                return GetArrayDifference((Array)expected, (Array)actual);

            return "";
        }

        public static bool ArraysEqual<T>(T[] arr1, T[] arr2)
        {
            if (arr1 == null && arr2 == null) return true;
            if (arr1 == null || arr2 == null) return false;
            if (arr1.Length != arr2.Length) return false;

            for (int i = 0; i < arr1.Length; i++)
            {
                if (!Equals(arr1[i], arr2[i]))
                    return false;
            }
            return true;
        }

        private bool AreEqualDeep(object expected, object actual)
        {
            if (expected == null && actual == null)
                return true;
            if (expected == null || actual == null)
                return false;

            Type expectedType = expected.GetType();
            Type actualType = actual.GetType();

            if (!MatchTypes(expectedType, actualType))
                return false;

            if (expected is string || expectedType.IsPrimitive || expectedType.IsEnum)
                return expected.Equals(actual);

            if (!expectedType.IsValueType)
            {
                var pair = new Tuple<object, object>(expected, actual);
                if (comparedPairs.Contains(pair))
                    return true;
                comparedPairs.Add(pair);
            }

            if (IsBinNodeType(expectedType))
                return CompareBinNodes(expected, actual);

            if (IsNodeType(expectedType))
                return CompareNodes(expected, actual);

            if (IsStackType(expectedType))
                return CompareStacks(expected, actual);

            if (IsQueueType(expectedType))
                return CompareQueues(expected, actual);

            if (expectedType.IsArray)
                return CompareArrays((Array)expected, (Array)actual);

            return CompareMembers(expected, actual);
        }

        private bool CompareMembers(object expected, object actual)
        {
            Type type = expected.GetType();

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            bool hasMembers = fields.Length > 0;
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].CanRead && properties[i].GetIndexParameters().Length == 0)
                {
                    hasMembers = true;
                    break;
                }
            }

            if (!hasMembers)
                return expected.Equals(actual);

            for (int i = 0; i < fields.Length; i++)
            {
                object expectedValue = fields[i].GetValue(expected);
                object actualValue = fields[i].GetValue(actual);
                if (!AreEqualDeep(expectedValue, actualValue))
                    return false;
            }

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo prop = properties[i];
                if (!prop.CanRead || prop.GetIndexParameters().Length != 0)
                    continue;

                object expectedValue = prop.GetValue(expected, null);
                object actualValue = prop.GetValue(actual, null);
                if (!AreEqualDeep(expectedValue, actualValue))
                    return false;
            }

            return true;
        }

        private static bool MatchTypes(Type expectedType, Type actualType)
        {
            if (expectedType.IsGenericType && actualType.IsGenericType)
            {
                if (expectedType.GetGenericTypeDefinition() != actualType.GetGenericTypeDefinition())
                    return false;

                Type[] expectedArgs = expectedType.GetGenericArguments();
                Type[] actualArgs = actualType.GetGenericArguments();

                if (expectedArgs.Length != actualArgs.Length)
                    return false;

                for (int i = 0; i < expectedArgs.Length; i++)
                {
                    if (expectedArgs[i] != actualArgs[i])
                        return false;
                }
            }
            else if (expectedType != actualType)
            {
                return false;
            }

            return true;
        }

        private static bool IsNodeType(Type type) =>
            (type.IsGenericType && type.Name.StartsWith("Node`")) ||
            type == typeof(Unit4.NodeInteger);
        private static bool IsBinNodeType(Type type) => type.IsGenericType && type.Name.StartsWith("BinNode`");
        private static bool IsStackType(Type type) => type.IsGenericType && type.Name.StartsWith("Stack`");
        private static bool IsQueueType(Type type) => type.IsGenericType && type.Name.StartsWith("Queue`");

        private bool CompareNodes(object expected, object actual)
        {
            object currentExpected = expected;
            object currentActual = actual;

            while (currentExpected != null && currentActual != null)
            {
                var getValue = currentExpected.GetType().GetMethod("GetValue");
                var getNext = currentExpected.GetType().GetMethod("GetNext");

                if (getValue == null || getNext == null)
                    return false;

                object expectedValue = getValue.Invoke(currentExpected, null);
                object actualValue = getValue.Invoke(currentActual, null);

                if (!AreEqualDeep(expectedValue, actualValue))
                    return false;

                currentExpected = getNext.Invoke(currentExpected, null);
                currentActual = getNext.Invoke(currentActual, null);
            }

            return currentExpected == null && currentActual == null;
        }

        private bool CompareBinNodes(object expected, object actual)
        {
            var type = expected.GetType();
            var getValue = type.GetMethod("GetValue");
            var getLeft = type.GetMethod("GetLeft");
            var getRight = type.GetMethod("GetRight");

            var expectedValue = getValue.Invoke(expected, null);
            var actualValue = getValue.Invoke(actual, null);

            if (!AreEqualDeep(expectedValue, actualValue))
                return false;

            var expectedLeft = getLeft.Invoke(expected, null);
            var actualLeft = getLeft.Invoke(actual, null);
            var expectedRight = getRight.Invoke(expected, null);
            var actualRight = getRight.Invoke(actual, null);

            if (!AreEqualDeep(expectedLeft, actualLeft))
                return false;

            if (!AreEqualDeep(expectedRight, actualRight))
                return false;

            return true;
        }

        private bool CompareStacks(object expected, object actual)
        {
            var topField = expected.GetType().GetField("top", BindingFlags.NonPublic | BindingFlags.Instance);
            if (topField == null)
                return false;

            object expectedTop = topField.GetValue(expected);
            object actualTop = topField.GetValue(actual);

            return CompareNodes(expectedTop, actualTop);
        }

        private bool CompareQueues(object expected, object actual)
        {
            var headField = expected.GetType().GetField("head", BindingFlags.NonPublic | BindingFlags.Instance);
            if (headField == null)
                return false;

            object expectedHead = headField.GetValue(expected);
            object actualHead = headField.GetValue(actual);

            return CompareNodes(expectedHead, actualHead);
        }

        private bool CompareArrays(Array expected, Array actual)
        {
            if (expected.Length != actual.Length)
                return false;

            for (int i = 0; i < expected.Length; i++)
            {
                if (!AreEqualDeep(expected.GetValue(i), actual.GetValue(i)))
                    return false;
            }

            return true;
        }

        private static string GetBinNodeDifference(object expected, object actual)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Binary tree difference:");
            DescribeBinNodeDifferenceRecursive(expected, actual, sb, "Root");
            return sb.ToString();
        }

        private static void DescribeBinNodeDifferenceRecursive(object expected, object actual, StringBuilder sb, string path)
        {
            if (expected == null && actual == null)
                return;
            if (expected == null)
            {
                sb.AppendLine($"{path}: Expected null node, actual has value");
                return;
            }
            if (actual == null)
            {
                sb.AppendLine($"{path}: Actual null node, expected has value");
                return;
            }

            var type = expected.GetType();
            var getValue = type.GetMethod("GetValue");
            var getLeft = type.GetMethod("GetLeft");
            var getRight = type.GetMethod("GetRight");

            var expVal = getValue.Invoke(expected, null);
            var actVal = getValue.Invoke(actual, null);
            if (!Equals(expVal, actVal))
            {
                sb.AppendLine($"{path}: Expected value '{expVal}', actual '{actVal}'");
            }

            var expLeft = getLeft.Invoke(expected, null);
            var actLeft = getLeft.Invoke(actual, null);
            var expRight = getRight.Invoke(expected, null);
            var actRight = getRight.Invoke(actual, null);

            DescribeBinNodeDifferenceRecursive(expLeft, actLeft, sb, path + ".Left");
            DescribeBinNodeDifferenceRecursive(expRight, actRight, sb, path + ".Right");
        }

        private static string GetNodeDifference(object expected, object actual)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Node list difference:");

            object currentExpected = expected;
            object currentActual = actual;
            int position = 0;

            var getValue = expected.GetType().GetMethod("GetValue");
            var getNext = expected.GetType().GetMethod("GetNext");

            if (getValue == null || getNext == null)
                return "Unable to compare Node structure";

            while (currentExpected != null || currentActual != null)
            {
                if (currentExpected == null)
                {
                    sb.AppendLine($"  Position {position}: Expected end of list, but found more nodes");
                    break;
                }
                if (currentActual == null)
                {
                    sb.AppendLine($"  Position {position}: Expected more nodes, but found end of list");
                    break;
                }

                object expectedValue = getValue.Invoke(currentExpected, null);
                object actualValue = getValue.Invoke(currentActual, null);

                if (!Equals(expectedValue, actualValue))
                {
                    sb.AppendLine($"  Position {position}: Expected '{expectedValue}', but was '{actualValue}'");
                }

                currentExpected = getNext.Invoke(currentExpected, null);
                currentActual = getNext.Invoke(currentActual, null);
                position++;
            }

            return sb.ToString();
        }

        private static string GetArrayDifference(Array expected, Array actual)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Array difference:");

            if (expected.Length != actual.Length)
            {
                sb.AppendLine($"  Length: Expected {expected.Length}, but was {actual.Length}");
                return sb.ToString();
            }

            for (int i = 0; i < expected.Length; i++)
            {
                object expectedValue = expected.GetValue(i);
                object actualValue = actual.GetValue(i);

                if (!Equals(expectedValue, actualValue))
                {
                    sb.AppendLine($"  Index {i}: Expected '{expectedValue}', but was '{actualValue}'");
                }
            }

            return sb.ToString();
        }
    }

    internal class PairEqualityComparer : IEqualityComparer<Tuple<object, object>>
    {
        public bool Equals(Tuple<object, object> x, Tuple<object, object> y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);
        }

        public int GetHashCode(Tuple<object, object> obj)
        {
            if (obj == null) return 0;
            int hash1 = obj.Item1 != null ? System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.Item1) : 0;
            int hash2 = obj.Item2 != null ? System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.Item2) : 0;
            return hash1 ^ hash2;
        }
    }
}
