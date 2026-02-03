using System;
using Unit4;

public static class StudentAnswer
{

    public static Node<RangeNode> CreateRangeList(Node<int> lst)
    {
        Node<int> p1 = lst;
        Node<int> p2 = lst;
        RangeNode rn = new RangeNode(0, 0);
        Node<RangeNode> rHead = new Node<RangeNode>(rn);
        Node<RangeNode> rTail = rHead;

        while (p1 != null)
        {
            while (p2.HasNext() && p2.GetValue() + 1 == p2.GetNext().GetValue())
            {
                p2 = p2.GetNext();
            }
            rn = new RangeNode(p1.GetValue(), p2.GetValue());
            rTail.SetNext(new Node<RangeNode>(rn));
            rTail = rTail.GetNext();
            p1 = p2.GetNext();
            p2 = p2.GetNext();
        }
        return rHead.GetNext();
    }

    public static Node<int> CreateList(int[] arr)
    {
        Node<int> head = new Node<int>(0);
        Node<int> tail = head;
        for (int i = 0; i < arr.Length; i++)
        {
            Node<int> node = new Node<int>(arr[i]);
            tail.SetNext(node);
            tail = tail.GetNext();
        }
        return head; //.GetNext();
    }
}


