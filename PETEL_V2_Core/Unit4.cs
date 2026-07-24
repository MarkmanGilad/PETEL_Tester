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

    public class NodeInteger
    {
        private int value;
        private NodeInteger next;

        public NodeInteger(int value)
        {
            this.value = value;
            this.next = null;
        }

        public NodeInteger(int value, NodeInteger next)
        {
            this.value = value;
            this.next = next;
        }

        public int GetValue() => this.value;

        public void SetValue(int value) => this.value = value;

        public NodeInteger GetNext() => this.next;

        public void SetNext(NodeInteger next) => this.next = next;

        public bool HasNext() => this.next != null;

        public override string ToString() => this.value.ToString();
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

        public static NodeInteger? BuildNodeIntegerList(int[] array)
        {
            if (array == null || array.Length == 0)
                return null;

            NodeInteger head = new NodeInteger(array[0]);
            NodeInteger current = head;

            for (int i = 1; i < array.Length; i++)
            {
                NodeInteger newNode = new NodeInteger(array[i]);
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

        public static T[] NodeListToArray<T>(Node<T>? head)
        {
            if (head == null)
                return new T[0];

            int count = 0;
            Node<T>? current = head;
            while (current != null)
            {
                count++;
                current = current.GetNext();
            }

            T[] array = new T[count];
            current = head;
            int index = 0;
            while (current != null)
            {
                array[index++] = current.GetValue();
                current = current.GetNext();
            }

            return array;
        }

        public static T[] StackToArray<T>(Stack<T> stack)
        {
            if (stack.IsEmpty())
                return new T[0];

            Stack<T> temp = new Stack<T>();
            System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();

            while (!stack.IsEmpty())
            {
                T value = stack.Pop();
                list.Add(value);
                temp.Push(value);
            }

            while (!temp.IsEmpty())
            {
                stack.Push(temp.Pop());
            }

            return list.ToArray();
        }

        public static T[] QueueToArray<T>(Queue<T> queue)
        {
            if (queue.IsEmpty())
                return new T[0];

            Queue<T> temp = Clone(queue);
            System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();

            while (!temp.IsEmpty())
            {
                list.Add(temp.Remove());
            }

            return list.ToArray();
        }

        public static BinNode<T>? BuildBinaryTree<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Tree file not found: {filePath}");

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length == 0)
                return null;

            int currentLine = 0;
            return BuildTreeRecursive<T>(lines, ref currentLine, 0);
        }

        private static BinNode<T>? BuildTreeRecursive<T>(string[] lines, ref int currentLine, int expectedDepth)
        {
            while (currentLine < lines.Length && string.IsNullOrWhiteSpace(lines[currentLine]))
            {
                currentLine++;
            }

            if (currentLine >= lines.Length)
                return null;

            string line = lines[currentLine];

            int depth = 0;
            while (depth < line.Length && line[depth] == '\t')
            {
                depth++;
            }

            if (depth != expectedDepth)
                return null;

            string trimmedLine = line.TrimStart('\t');
            string valueStr = trimmedLine;

            if (trimmedLine.StartsWith("Left:"))
                valueStr = trimmedLine.Substring(5);
            else if (trimmedLine.StartsWith("Right:"))
                valueStr = trimmedLine.Substring(6);

            T value = ConvertToType<T>(valueStr);

            BinNode<T> node = new BinNode<T>(value);
            currentLine++;

            while (currentLine < lines.Length)
            {
                while (currentLine < lines.Length && string.IsNullOrWhiteSpace(lines[currentLine]))
                {
                    currentLine++;
                }

                if (currentLine >= lines.Length)
                    break;

                string nextLine = lines[currentLine];

                int nextDepth = 0;
                while (nextDepth < nextLine.Length && nextLine[nextDepth] == '\t')
                {
                    nextDepth++;
                }

                if (nextDepth != depth + 1)
                    break;

                string trimmedNextLine = nextLine.TrimStart('\t');

                if (trimmedNextLine.StartsWith("Left:"))
                {
                    node.SetLeft(BuildTreeRecursive<T>(lines, ref currentLine, depth + 1));
                }
                else if (trimmedNextLine.StartsWith("Right:"))
                {
                    node.SetRight(BuildTreeRecursive<T>(lines, ref currentLine, depth + 1));
                }
                else
                {
                    break;
                }
            }

            return node;
        }

        private static T ConvertToType<T>(string value)
        {
            try
            {
                Type targetType = typeof(T);

                if (targetType == typeof(string))
                    return (T)(object)value;

                if (targetType == typeof(int))
                    return (T)(object)int.Parse(value);

                if (targetType == typeof(double))
                    return (T)(object)double.Parse(value);

                if (targetType == typeof(float))
                    return (T)(object)float.Parse(value);

                if (targetType == typeof(bool))
                    return (T)(object)bool.Parse(value);

                if (targetType == typeof(char) && value.Length == 1)
                    return (T)(object)value[0];

                return (T)Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot convert '{value}' to type {typeof(T).Name}", ex);
            }
        }

        public static string BinaryTreeToString<T>(BinNode<T>? root)
        {
            if (root == null)
                return "null";

            var sb = new StringBuilder();
            BinaryTreeToStringRecursive(root, sb, "", "");
            return sb.ToString();
        }

        private static void BinaryTreeToStringRecursive<T>(BinNode<T>? node, StringBuilder sb, string prefix, string childPrefix)
        {
            if (node == null)
                return;

            sb.AppendLine(prefix + node.GetValue());

            if (node.HasLeft() || node.hasRight())
            {
                if (node.HasLeft())
                {
                    sb.Append(childPrefix + "├─Left: ");
                    BinaryTreeToStringRecursive(node.GetLeft(), sb, "", childPrefix + "│  ");
                }
                else
                {
                    sb.AppendLine(childPrefix + "├─Left: null");
                }

                if (node.hasRight())
                {
                    sb.Append(childPrefix + "└─Right: ");
                    BinaryTreeToStringRecursive(node.GetRight(), sb, "", childPrefix + "   ");
                }
                else
                {
                    sb.AppendLine(childPrefix + "└─Right: null");
                }
            }
        }

        public static void PrintBinaryTree<T>(BinNode<T>? root)
        {
            if (root == null)
            {
                Console.WriteLine("Tree is empty (null)");
                return;
            }

            Console.WriteLine("Binary Tree Structure:");
            Console.WriteLine("======================");
            PrintBinaryTreeRecursive(root, "", "", true);
            Console.WriteLine("======================");
        }

        private static void PrintBinaryTreeRecursive<T>(BinNode<T>? node, string indent, string pointer, bool isRoot)
        {
            if (node == null)
                return;

            Console.Write(indent);
            if (!isRoot)
                Console.Write(pointer);
            Console.WriteLine(node.GetValue());

            string childIndent = indent;
            if (!isRoot)
            {
                childIndent += (pointer == "└── " ? "    " : "│   ");
            }

            if (node.HasLeft() || node.hasRight())
            {
                if (node.HasLeft())
                {
                    PrintBinaryTreeRecursive(node.GetLeft(), childIndent, "├── ", false);
                }
                else if (node.hasRight())
                {
                    Console.WriteLine(childIndent + "├── (null)");
                }

                if (node.hasRight())
                {
                    PrintBinaryTreeRecursive(node.GetRight(), childIndent, "└── ", false);
                }
                else if (node.HasLeft())
                {
                    Console.WriteLine(childIndent + "└── (null)");
                }
            }
        }

        public static void PrintBinaryTreeColored<T>(BinNode<T>? root, bool useColors = true)
        {
            if (root == null)
            {
                Console.WriteLine("Tree is empty (null)");
                return;
            }

            Console.WriteLine("Binary Tree Structure:");
            Console.WriteLine("======================");
            PrintBinaryTreeColoredRecursive(root, "", "", true, 0, useColors);
            Console.WriteLine("======================");
        }

        private static void PrintBinaryTreeColoredRecursive<T>(BinNode<T>? node, string indent, string pointer, bool isRoot, int depth, bool useColors)
        {
            if (node == null)
                return;

            ConsoleColor[] colors = new ConsoleColor[]
            {
                ConsoleColor.Cyan,
                ConsoleColor.Yellow,
                ConsoleColor.Green,
                ConsoleColor.Magenta,
                ConsoleColor.Blue,
                ConsoleColor.Red
            };

            Console.Write(indent);
            if (!isRoot)
                Console.Write(pointer);

            if (useColors)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = colors[depth % colors.Length];
                Console.WriteLine(node.GetValue());
                Console.ForegroundColor = originalColor;
            }
            else
            {
                Console.WriteLine(node.GetValue());
            }

            string childIndent = indent;
            if (!isRoot)
            {
                childIndent += (pointer == "└── " ? "    " : "│   ");
            }

            if (node.HasLeft() || node.hasRight())
            {
                if (node.HasLeft())
                {
                    PrintBinaryTreeColoredRecursive(node.GetLeft(), childIndent, "├── ", false, depth + 1, useColors);
                }
                else if (node.hasRight())
                {
                    Console.WriteLine(childIndent + "├── (null)");
                }

                if (node.hasRight())
                {
                    PrintBinaryTreeColoredRecursive(node.GetRight(), childIndent, "└── ", false, depth + 1, useColors);
                }
                else if (node.HasLeft())
                {
                    Console.WriteLine(childIndent + "└── (null)");
                }
            }
        }

        public static void PrintList<T>(Node<T>? head)
        {
            Console.Write("[");
            Node<T>? current = head;
            bool first = true;

            while (current != null)
            {
                if (!first) Console.Write(", ");
                T value = current.GetValue();
                Console.Write(value != null ? value.ToString() : "null");
                first = false;
                current = current.GetNext();
            }

            Console.WriteLine("]");
        }

        public static string GetTreeFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return filePath;

            if (Path.IsPathRooted(filePath))
                return filePath;

            var baseDir = AppContext.BaseDirectory;
            var currentDir = Directory.GetCurrentDirectory();

            string[] possiblePaths =
            {
                filePath,
                Path.Combine(currentDir, filePath),
                Path.Combine(baseDir, filePath),
                Path.Combine(baseDir, "..", filePath),
                Path.Combine(baseDir, "..", "..", filePath),
                Path.Combine(baseDir, "..", "..", "..", filePath),
                Path.Combine(baseDir, "..", "..", "..", "..", filePath),
                Path.Combine(currentDir, "..", filePath),
                Path.Combine(currentDir, "..", "..", filePath),
                Path.Combine(currentDir, "..", "..", "..", filePath),
                Path.Combine(currentDir, "..", "..", "..", "..", filePath)
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    var full = Path.GetFullPath(path);
                    if (File.Exists(full))
                        return full;
                }
                catch
                {
                    continue;
                }
            }

            return filePath;
        }
    }
}
