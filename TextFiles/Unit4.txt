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
        public override string ToString()
        {
            return "" + this.value;
        }
    }

    [Serializable]
    public class Queue<T>
    {
        private Node<T> head;
        private Node<T> tale;

        public Queue()
        {
            this.head = null;
            this.tale = null;
        }

        public bool IsEmpty()
        {
            return this.head == null;
        }

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
            {
                this.tale = null;
            }

            return x;
        }

        public T Head()
        {

            return this.head.GetValue();
        }

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

        public Stack()
        {
            this.top = null;
        }

        public bool IsEmpty()
        {
            return (this.top == null);
        }

        public void Push(T x)
        {
            this.top = new Node<T>(x, this.top);
        }

        public T Pop()
        {
            T x = this.top.GetValue();
            Node<T> temp = this.top;
            this.top = this.top.GetNext();
            temp.SetNext(null);
            return x;
        }

        public T Top()
        {
            return this.top.GetValue();
        }

        public override String ToString()
        {
            String s = "[ ";
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

        public BinNode(T value)
        {
            this.left = null;
            this.value = value;
            this.right = null;
        }
        public BinNode(BinNode<T> left, T value, BinNode<T> right)
        {
            this.left = left;
            this.value = value;
            this.right = right;
        }

        public T GetValue() { return this.value; }
        public BinNode<T> GetLeft() { return this.left; }
        public BinNode<T> GetRight() { return this.right; }
        public bool HasLeft() { return this.left != null; }
        public bool hasRight() { return this.right != null; }
        public void SetValue(T value) { this.value = value; }
        public void SetLeft(BinNode<T> left) { this.left = left; }
        public void SetRight(BinNode<T> right) { this.right = right; }
        public override string ToString() { return this.value.ToString(); }
    }

    /// <summary>
    /// Helper class for building data structures for unit testing
    /// </summary>
    public static class Unit4Helper
    {
        /// <summary>
        /// Builds a linked list of Node<T> from an array
        /// </summary>
        /// <typeparam name="T">Type of elements in the array</typeparam>
        /// <param name="array">Array to convert to linked list</param>
        /// <returns>Head node of the linked list, or null if array is empty</returns>
        public static Node<T> BuildNodeList<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                return null;

            // Build list from beginning, maintaining order
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

        /// <summary>
        /// Builds a Stack<T> from an array
        /// Elements are pushed in array order (first element will be at bottom)
        /// </summary>
        /// <typeparam name="T">Type of elements in the array</typeparam>
        /// <param name="array">Array to convert to stack</param>
        /// <returns>Stack containing all array elements</returns>
        public static Stack<T> BuildStack<T>(T[] array)
        {
            Stack<T> stack = new Stack<T>();

            if (array == null || array.Length == 0)
                return stack;

            // Push elements in array order
            // First element ends up at bottom, last at top
            for (int i = 0; i < array.Length; i++)
            {
                stack.Push(array[i]);
            }

            return stack;
        }

        /// <summary>
        /// Builds a Queue<T> from an array
        /// Elements are inserted in array order (first element will be at head)
        /// </summary>
        /// <typeparam name="T">Type of elements in the array</typeparam>
        /// <param name="array">Array to convert to queue</param>
        /// <returns>Queue containing all array elements</returns>
        public static Queue<T> BuildQueue<T>(T[] array)
        {
            Queue<T> queue = new Queue<T>();

            if (array == null || array.Length == 0)
                return queue;

            // Insert elements in array order
            // First element ends up at head, last at tail
            for (int i = 0; i < array.Length; i++)
            {
                queue.Insert(array[i]);
            }

            return queue;
        }

        /// <summary>
        /// Clone a queue without changing the original queue
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

        /// <summary>
        /// Converts a linked list back to an array
        /// Useful for testing and verification
        /// </summary>
        /// <typeparam name="T">Type of elements in the list</typeparam>
        /// <param name="head">Head node of the linked list</param>
        /// <returns>Array containing all list elements</returns>
        public static T[] NodeListToArray<T>(Node<T> head)
        {
            if (head == null)
                return new T[0];

            // Count nodes
            int count = 0;
            Node<T> current = head;
            while (current != null)
            {
                count++;
                current = current.GetNext();
            }

            // Build array
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

        /// <summary>
        /// Converts a stack to an array (without modifying the stack)
        /// Elements are in top-to-bottom order
        /// </summary>
        /// <typeparam name="T">Type of elements in the stack</typeparam>
        /// <param name="stack">Stack to convert</param>
        /// <returns>Array containing all stack elements</returns>
        public static T[] StackToArray<T>(Stack<T> stack)
        {
            if (stack.IsEmpty())
                return new T[0];

            // Use temporary stack to preserve original
            Stack<T> temp = new Stack<T>();
            System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();

            // Pop all elements to temp stack and collect values
            while (!stack.IsEmpty())
            {
                T value = stack.Pop();
                list.Add(value);
                temp.Push(value);
            }

            // Restore original stack
            while (!temp.IsEmpty())
            {
                stack.Push(temp.Pop());
            }

            return list.ToArray();
        }

        /// <summary>
        /// Converts a queue to an array (without modifying the queue)
        /// Elements are in head-to-tail order
        /// </summary>
        /// <typeparam name="T">Type of elements in the queue</typeparam>
        /// <param name="queue">Queue to convert</param>
        /// <returns>Array containing all queue elements</returns>
        public static T[] QueueToArray<T>(Queue<T> queue)
        {
            if (queue.IsEmpty())
                return new T[0];

            // Clone the queue to preserve original
            Queue<T> temp = Clone(queue);
            System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();

            // Remove all elements and collect values
            while (!temp.IsEmpty())
            {
                list.Add(temp.Remove());
            }

            return list.ToArray();
        }

        /// <summary>
        /// Builds a binary tree from a text file
        /// The file format uses indentation (tabs) to represent tree structure:
        /// - Each line represents a node
        /// - Indentation level indicates depth
        /// - "Left:" prefix indicates left child
        /// - "Right:" prefix indicates right child
        /// - Empty lines are treated as null nodes
        /// </summary>
        /// <typeparam name="T">Type of elements in the tree (must support conversion from string)</typeparam>
        /// <param name="filePath">Path to the text file containing tree structure</param>
        /// <returns>Root node of the binary tree, or null if file is empty</returns>
        public static BinNode<T> BuildBinaryTree<T>(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException($"Tree file not found: {filePath}");

            string[] lines = System.IO.File.ReadAllLines(filePath);
            if (lines.Length == 0)
                return null;

            int currentLine = 0;
            return BuildTreeRecursive<T>(lines, ref currentLine, 0);
        }

        /// <summary>
        /// Recursively builds the binary tree from file lines
        /// </summary>
        private static BinNode<T> BuildTreeRecursive<T>(string[] lines, ref int currentLine, int expectedDepth)
        {
            // Skip any empty lines before processing
            while (currentLine < lines.Length && string.IsNullOrWhiteSpace(lines[currentLine]))
            {
                currentLine++;
            }

            if (currentLine >= lines.Length)
                return null;

            string line = lines[currentLine];

            // Calculate depth based on leading tabs
            int depth = 0;
            while (depth < line.Length && line[depth] == '\t')
            {
                depth++;
            }

            // If depth doesn't match expected, this node belongs to a different level
            if (depth != expectedDepth)
                return null;

            // Extract the value (remove tabs and prefixes)
            string trimmedLine = line.TrimStart('\t');
            string valueStr = trimmedLine;

            // Remove "Left:" or "Right:" prefix if present
            if (trimmedLine.StartsWith("Left:"))
                valueStr = trimmedLine.Substring(5);
            else if (trimmedLine.StartsWith("Right:"))
                valueStr = trimmedLine.Substring(6);

            // Convert string to type T
            T value = ConvertToType<T>(valueStr);

            // Create the node
            BinNode<T> node = new BinNode<T>(value);
            currentLine++;

            // Process children in a loop to handle both left and right
            while (currentLine < lines.Length)
            {
                // Skip empty lines
                while (currentLine < lines.Length && string.IsNullOrWhiteSpace(lines[currentLine]))
                {
                    currentLine++;
                }

                if (currentLine >= lines.Length)
                    break;

                string nextLine = lines[currentLine];

                // Check depth of next line
                int nextDepth = 0;
                while (nextDepth < nextLine.Length && nextLine[nextDepth] == '\t')
                {
                    nextDepth++;
                }

                // If next line is not a direct child, stop processing
                if (nextDepth != depth + 1)
                    break;

                string trimmedNextLine = nextLine.TrimStart('\t');

                // Process left child
                if (trimmedNextLine.StartsWith("Left:"))
                {
                    node.SetLeft(BuildTreeRecursive<T>(lines, ref currentLine, depth + 1));
                }
                // Process right child
                else if (trimmedNextLine.StartsWith("Right:"))
                {
                    node.SetRight(BuildTreeRecursive<T>(lines, ref currentLine, depth + 1));
                }
                else
                {
                    // Unknown format, stop processing
                    break;
                }
            }

            return node;
        }

        /// <summary>
        /// Converts a string value to the specified type T
        /// </summary>
        private static T ConvertToType<T>(string value)
        {
            try
            {
                // Handle common types
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

                // Use Convert.ChangeType for other types
                return (T)Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot convert '{value}' to type {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Converts a binary tree to a string representation (for debugging/testing)
        /// </summary>
        /// <typeparam name="T">Type of elements in the tree</typeparam>
        /// <param name="root">Root node of the tree</param>
        /// <returns>String representation of the tree</returns>
        public static string BinaryTreeToString<T>(BinNode<T> root)
        {
            if (root == null)
                return "null";

            StringBuilder sb = new StringBuilder();
            BinaryTreeToStringRecursive(root, sb, "", "");
            return sb.ToString();
        }

        /// <summary>
        /// Recursively builds string representation of the tree
        /// </summary>
        private static void BinaryTreeToStringRecursive<T>(BinNode<T> node, StringBuilder sb, string prefix, string childPrefix)
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

        /// <summary>
        /// Prints a binary tree to the console with visual edges
        /// Uses box-drawing characters for a cleaner appearance
        /// </summary>
        /// <typeparam name="T">Type of elements in the tree</typeparam>
        /// <param name="root">Root node of the tree</param>
        public static void PrintBinaryTree<T>(BinNode<T> root)
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

        /// <summary>
        /// Recursively prints the tree structure to console with visual edges
        /// </summary>
        /// <typeparam name="T">Type of elements in the tree</typeparam>
        /// <param name="node">Current node being printed</param>
        /// <param name="indent">Current indentation string</param>
        /// <param name="pointer">Pointer character (├── or └──)</param>
        /// <param name="isRoot">Whether this is the root node</param>
        private static void PrintBinaryTreeRecursive<T>(BinNode<T> node, string indent, string pointer, bool isRoot)
        {
            if (node == null)
                return;

            // Print current node
            Console.Write(indent);
            if (!isRoot)
                Console.Write(pointer);
            Console.WriteLine(node.GetValue());

            // Prepare indentation for children
            string childIndent = indent;
            if (!isRoot)
            {
                childIndent += (pointer == "└── " ? "    " : "│   ");
            }

            // Print left and right children
            if (node.HasLeft() || node.hasRight())
            {
                // Print left child
                if (node.HasLeft())
                {
                    PrintBinaryTreeRecursive(node.GetLeft(), childIndent, "├── ", false);
                }
                else if (node.hasRight())
                {
                    // Show null left child only if right child exists
                    Console.WriteLine(childIndent + "├── (null)");
                }

                // Print right child
                if (node.hasRight())
                {
                    PrintBinaryTreeRecursive(node.GetRight(), childIndent, "└── ", false);
                }
                else if (node.HasLeft())
                {
                    // Show null right child only if left child exists
                    Console.WriteLine(childIndent + "└── (null)");
                }
            }
        }

        /// <summary>
        /// Prints a binary tree to the console with colored output (optional)
        /// Highlights different levels with colors for better visualization
        /// </summary>
        /// <typeparam name="T">Type of elements in the tree</typeparam>
        /// <param name="root">Root node of the tree</param>
        /// <param name="useColors">Whether to use colored output</param>
        public static void PrintBinaryTreeColored<T>(BinNode<T> root, bool useColors = true)
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

        /// <summary>
        /// Recursively prints the tree with optional color coding by depth
        /// </summary>
        private static void PrintBinaryTreeColoredRecursive<T>(BinNode<T> node, string indent, string pointer, bool isRoot, int depth, bool useColors)
        {
            if (node == null)
                return;

            // Color palette for different depths
            ConsoleColor[] colors = new ConsoleColor[]
            {
                ConsoleColor.Cyan,
                ConsoleColor.Yellow,
                ConsoleColor.Green,
                ConsoleColor.Magenta,
                ConsoleColor.Blue,
                ConsoleColor.Red
            };

            // Print current node
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

            // Prepare indentation for children
            string childIndent = indent;
            if (!isRoot)
            {
                childIndent += (pointer == "└── " ? "    " : "│   ");
            }

            // Print left and right children
            if (node.HasLeft() || node.hasRight())
            {
                // Print left child
                if (node.HasLeft())
                {
                    PrintBinaryTreeColoredRecursive(node.GetLeft(), childIndent, "├── ", false, depth + 1, useColors);
                }
                else if (node.hasRight())
                {
                    Console.WriteLine(childIndent + "├── (null)");
                }

                // Print right child
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

        /// <summary>
        /// Prints a singly linked list to the console in one line, formatted as: [v1, v2, v3]
        /// If the head is null, prints [].
        /// </summary>
        /// <typeparam name="T">Type of the node values</typeparam>
        /// <param name="head">Head node of the linked list</param>
        public static void PrintList<T>(Node<T> head)
        {
            Console.Write("[");
            Node<T> current = head;
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

        /// <summary>
        /// Gets the tree file path, compatible with both local and VPL
        /// </summary>
        public static string GetTreeFilePath(string filePath)
        {
            string[] possiblePaths = new[]
            {
                filePath,
                Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, filePath),
                Path.Combine("..", filePath),
                Path.Combine("..", "..", filePath)
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    if (File.Exists(path))
                        return Path.GetFullPath(path);
                }
                catch
                {
                    continue;
                }
            }

            return filePath;
        }

        /// <summary>
        /// Creates a readable string for a Node<T> singly linked list.
        /// Example: 3, 5, -4, 22 (no brackets unless includeBrackets = true)
        /// Handles null, empty, and protects against cycles.
        /// </summary>
        public static string NodeListToString<T>(Node<T> head, bool includeBrackets = false, int maxNodes = 256)
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
                    // Cycle detected
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
            {
                sb.Append(", ...truncated...");
            }

            if (includeBrackets) sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Non-generic runtime version that accepts an object presumed to be Node&lt;T&gt;.
        /// Uses reflection so VPL tester can call it without knowing T.
        /// </summary>
        public static string NodeListToString(object head, bool includeBrackets = false, int maxNodes = 256)
        {
            if (head == null)
                return includeBrackets ? "[]" : "";

            var type = head.GetType();
            if (!(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Node<>)))
                return head.ToString();

            var sb = new StringBuilder();
            if (includeBrackets) sb.Append("[");

            var getValue = type.GetMethod("GetValue");
            var getNext = type.GetMethod("GetNext");

            // FIX: Use the local ReferenceEqualityComparer (System.Collections.Generic.ReferenceEqualityComparer is not available on .NET Framework 4.7.2)
            var seen = new System.Collections.Generic.HashSet<object>(ReferenceEqualityComparer.Instance);

            object current = head;
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
                var value = getValue.Invoke(current, null);
                sb.Append(value ?? "null");

                current = getNext.Invoke(current, null);
                first = false;
                count++;
            }

            if (count >= maxNodes)
            {
                sb.Append(", ...truncated...");
            }

            if (includeBrackets) sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Reference equality comparer to safely detect cycles in reflection-based traversal.
        /// </summary>
        private sealed class ReferenceEqualityComparer : System.Collections.Generic.IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            private ReferenceEqualityComparer() { }
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
