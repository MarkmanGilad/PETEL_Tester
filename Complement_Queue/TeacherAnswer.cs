using System;
using Unit4;

public static class TeacherAnswer
{
    public static Stack<Node<int>> Complement (Stack<Queue<int>> SQ)
    {
        Stack<Node<int>> SL = new Stack<Node<int>>();
        while (!SQ.IsEmpty())
        {
            Node<int> lst = new Node<int> (0); // Dummy node
            Node<int> tail = lst;
            Queue <int> Q = SQ.Pop ();
            int start = Q.Remove();
            while (!Q.IsEmpty ())
            {
                int end = Q.Remove();
                for (int i = start+1; i < end; i++)
                {
                    tail.SetNext(new Node<int>(i));
                    tail = tail.GetNext();
                }
                start = end;
            }
            SL.Push (lst.GetNext());
        }

        return Reverse(SL);
    }

    public static Stack<Node<int>> Reverse(Stack<Node<int>> SL)
    {
        Stack<Node<int>> SR = new Stack<Node<int>>();
        while (!SL.IsEmpty())
        {
            SR.Push (SL.Pop ());
        }
        return SR;
    }    
}
