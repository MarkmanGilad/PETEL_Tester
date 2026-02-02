using System;
using Unit4;

public static class TeacherAnswer
{
    /// <summary>
    /// Counts how many times a value appears in a linked list using recursion
    /// </summary>
    /// <param name="head">Head of the linked list</param>
    /// <param name="value">Value to count</param>
    /// <returns>Number of occurrences</returns>
    public static int CountValues(Node<int> head, int value)
    {
        // Base case: empty list
        if (head == null)
            return 0;

        // Check if current node matches
        int count = head.GetValue() == value ? 1 : 0;

        // Recursive call on rest of list
        return count + CountValues(head.GetNext(), value);
    }

    public static int DFS_FindMax(BinNode<int> node)
    {
        if (node == null)
            return int.MinValue;
        int max = Math.Max(DFS_FindMax(node.GetLeft()), DFS_FindMax(node.GetRight()));
        return Math.Max(max, node.GetValue());
    }

    public static Stack<int> CopyStack(Stack<int> s)
    {
        Stack<int> stack = new Stack<int>();
        Stack<int> temp = new Stack<int>();

        while (!s.IsEmpty())
        {
            int value = s.Pop();
            temp.Push(value);
        }
        while (!temp.IsEmpty())
        {
            int value = temp.Pop();
            s.Push(value);
            stack.Push(value);
        }
        return stack;
    }
}
