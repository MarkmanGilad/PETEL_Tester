using System;
using Unit4;

public static class StudentAnswer
{
    
    public static int CountValues(Node<int> head, int value)
    {

        if (head == null)
            return 0;

        int count = head.GetValue() == value ? 1 : 0;
    
        return count + CountValues(head.GetNext(), value);
    }

    public static int CountValues2(Node<int> head, int value)
    {

        if (head == null)
            return 0;

        int count = head.GetValue() == value ? 1 : 0;

        return count + CountValues2(head, value);
    }
    public static int CountValues3(Node<int> head, int value)
    {

        if (head == null)
            return 0;

        int count = head.GetValue() == value ? 2 : 0;

        return count + CountValues3(head.GetNext(), value);
    }


    public static int DFS_FindMax(BinNode<int> node)
    {
        if (node == null)
            return int.MinValue;
        int max = Math.Max(DFS_FindMax(node.GetLeft()), DFS_FindMax(node.GetRight()));
        return Math.Max(max, node.GetValue());
    }
}


