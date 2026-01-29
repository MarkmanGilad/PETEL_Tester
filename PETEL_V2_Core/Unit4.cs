using System;
using System.IO;
using System.Text;

namespace Unit4
{
    [Serializable]
    public class Node<T>
    {
        private T value;
        private Node<T> next;

        public Node(T value)
        {
            this.value = value;
            this.next = null;
        }

        public Node(T value, Node<T> next)
        {
            this.value = value;
            this.next = next;
        }

        public T GetValue() { return this.value; }
        public void SetValue(T value) { this.value = value; }
        public Node<T> GetNext() { return this.next; }
        public void SetNext(Node<T> next) { this.next = next; }
        public bool HasNext() { return this.next != null; }
        public override string ToString() { return "" + this.value; }
    }

    [Serializable]
    public class Queue<T>
    {
        private Node<T> head;
        private Node<T> tale;

        public Queue() { this.head = null; this.tale = null; }

        public bool IsEmpty() { return this.head == null; }

        public void Insert(T x)
        {
            Node<T> newNode = new Node<T>(x);

            if (IsEmpty())
            {
                head = newNode;
                tale = newNode;
            }
            else
            {
                tale.SetNext(newNode);
                tale = newNode;
            }
        }

        public T Remove()
        {
            T x = this.head.GetValue();
            this.head = this.head.GetNext();

            if (this.head == null)
                this.tale = null;

            return x;
        }

        public T Head() { return this.head.GetValue(); }

        public override string ToString()
        {
            string s = "[ ";
            Node<T> p = this.head;
            while (p != null)
            {
                s = s + p.GetValue().ToString() + " ";
                p = p.GetNext();
            }
            return s + "]\n";
        }
    }

    [Serializable]
    public class Stack<T>
    {
        private Node<T> top;

        public Stack() { this.top = null; }

        public bool IsEmpty() { return this.top == null; }

        public void Push(T x) { this.top = new Node<T>(x, this.top); }

        public T Pop()
        {
            T x = this.top.GetValue();
            Node<T> temp = this.top;
            this.top = this.top.GetNext();
            temp.SetNext(null);
            return x;
        }

        public T Top() { return this.top.GetValue(); }

        public override string ToString()
        {
            string s = "[ ";
            Node<T> p = this.top;
            while (p != null)
            {
                s = s + p.GetValue().ToString() + " ";
                p = p.GetNext();
            }
            return (s + "]\n");
        }
    }

    [Serializable]
    public class BinNode<T>
    {
        private BinNode<T> left;
        private T value;
        private BinNode<T> right;

        public BinNode(T value) { this.left = null; this.value = value; this.right = null; }
        public BinNode(BinNode<T> left, T value, BinNode<T> right) { this.left = left; this.value = value; this.right = right; }

        public T GetValue() { return this.value; }
        public BinNode<T> GetLeft() { return this.left; }
        public BinNode<T> GetRight() { return this.right; }
        public bool HasLeft() { return this.left != null; }
        public bool hasRight() { return this.right != null; }
        public void SetValue(T value) { this.value = value; }
        public void SetLeft(BinNode<T> left) { this.left = left; }
        public void SetRight(BinNode<T> right) { this.right = right; }
        public override string ToString() { return this.value?.ToString() ?? string.Empty; }
    }

    public static class Unit4Helper
    {
        public static Node<T>? BuildNodeList<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                return null;

            Node<T> head = new Node<T>(array[0]);
            Node<T> current = head;

            for (int i = 1; i < array.Length; i++)
            {
                Node<T> newNode = new Node<T>(array[i]);
                current.SetNext(newNode);
                current = newNode;
            }

            return head;
        }

        public static Stack<T> BuildStack<T>(T[] array)
        {
            Stack<T> stack = new Stack<T>();
            if (array == null || array.Length == 0)
                return stack;

            for (int i = 0; i < array.Length; i++)
                stack.Push(array[i]);

            return stack;
        }

        public static Queue<T> BuildQueue<T>(T[] array)
        {
            Queue<T> queue = new Queue<T>();
            if (array == null || array.Length == 0)
                return queue;

            for (int i = 0; i < array.Length; i++)
                queue.Insert(array[i]);

            return queue;
        }

        public static Queue<T> Clone<T>(Queue<T> Q)
        {
            Queue<T> temp = new Queue<T>();
            Queue<T> temp1 = new Queue<T>();
            if (Q.IsEmpty())
                return temp;
            while (!Q.IsEmpty())
            {
                temp.Insert(Q.Head());
                temp1.Insert(Q.Remove());
            }
            while (!temp1.IsEmpty())
                Q.Insert(temp1.Remove());
            return temp;
        }

        public static string NodeListToString<T>(Node<T>? head, bool includeBrackets = false, int maxNodes = 256)
        {
            if (head == null)
                return includeBrackets ? "[]" : "";

            var sb = new StringBuilder();
            if (includeBrackets) sb.Append("[");

            var seen = new System.Collections.Generic.HashSet<Node<T>>();
            var current = head;
            bool first = true;
            int count = 0;

            while (current != null && count < maxNodes)
            {
                if (!seen.Add(current))
                {
                    if (!first) sb.Append(", ");
                    sb.Append("...cycle...");
                    break;
                }

                if (!first) sb.Append(", ");
                var val = current.GetValue();
                sb.Append(val != null ? val.ToString() : "null");

                current = current.GetNext();
                first = false;
                count++;
            }

            if (count >= maxNodes)
                sb.Append(", ...truncated...");

            if (includeBrackets) sb.Append("]");
            return sb.ToString();
        }

        public static string NodeListToString(object? head, bool includeBrackets = false, int maxNodes = 256)
        {
            if (head == null)
                return includeBrackets ? "[]" : "";

            var type = head.GetType();
            if (!(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Node<>)))
                return head.ToString() ?? string.Empty;

            var sb = new StringBuilder();
            if (includeBrackets) sb.Append("[");

            var getValue = type.GetMethod("GetValue");
            var getNext = type.GetMethod("GetNext");

            var seen = new System.Collections.Generic.HashSet<object>(ReferenceEqualityComparer.Instance);

            object? current = head;
            bool first = true;
            int count = 0;

            while (current != null && count < maxNodes)
            {
                if (!seen.Add(current))
                {
                    if (!first) sb.Append(", ");
                    sb.Append("...cycle...");
                    break;
                }

                if (!first) sb.Append(", ");
                var value = getValue!.Invoke(current, null);
                sb.Append(value ?? "null");

                current = getNext!.Invoke(current, null);
                first = false;
                count++;
            }

            if (count >= maxNodes)
                sb.Append(", ...truncated...");

            if (includeBrackets) sb.Append("]");
            return sb.ToString();
        }

        private sealed class ReferenceEqualityComparer : System.Collections.Generic.IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            private ReferenceEqualityComparer() { }
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
