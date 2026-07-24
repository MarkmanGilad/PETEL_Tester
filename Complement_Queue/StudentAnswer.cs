using System;
using Unit4;

public static class StudentAnswer
{

    public static Stack<Node<int>> Complement(Stack<Queue<int>> SQ)
    {
        Stack<Node<int>> SL = new Stack<Node<int>>();
        while (!SQ.IsEmpty())
        {
            Queue<int> Q = SQ.Pop();
            SL.Push(Comp(Q));
        }

        return Reverse(SL);
    }

    public static Node<int> Comp (Queue<int> Q)
    {
        Node<int> lst = new Node<int>(0); // Dummy node
        Node<int> tail = lst;
        
        int start = Q.Remove();
        while (!Q.IsEmpty())
        {
            int end = Q.Remove();
            for (int i = start + 1; i < end; i++)
            {
                tail.SetNext(new Node<int>(i));
                tail = tail.GetNext();
            }
            start = end;
        }
        return lst.GetNext();
    }

    public static Stack<Node<int>> Reverse(Stack<Node<int>> SL)
    {
        Stack<Node<int>> SR = new Stack<Node<int>>();
        while (!SL.IsEmpty())
        {
            SR.Push(SL.Pop());
        }
        return SR;
    }

    public static int Sub_series (Node<int> lst, int idx)
    {
        int count = 0;
        for (int i=0; i < idx; i++)
        {
            lst = lst.GetNext();
        }
        int start = lst.GetValue();
        while (lst != null)
        {
            if (lst.GetValue() == -start)
            {
                return count+1;
                
            }
            lst = lst.GetNext();
            count++;
        }
        return 0;
    }

    public static int max_subList(Node<int> lst)
    {
        int max_length = -1;

        while (lst != null)
        {
            int length = Sub_series(lst, 0);
            if (length > max_length)
            {
                max_length = length;
            }
            lst = lst.GetNext();
        }
        return max_length;
    }

    public static Node<int> CreateLst(int[] arr)
    {
        Node<int> lst = new Node<int>(0);
        Node<int> tail = lst;
        for(int i=0; i < arr.Length; i++)
        {
            tail.SetNext(new Node<int>(arr[i]));
            tail = tail.GetNext();
        }
        return lst;
    }

    public static Node<double> DeleteRange(Node<double>lst, int a, int b, int num)
    {
        lst = new Node<double>(0, lst);
        Node<double> p = lst;
        while (p.HasNext() && num > 0 )
        {
            if (p.GetValue() >=a && p.GetValue() <= b )
            {
                p.SetNext(p.GetNext().GetNext());
                num --;
            }
            else
            {
                p = p.GetNext();
            }
        }
        return lst.GetNext();
    }


    public static void Main ()
    {
        Queue<int> Q1 = Unit4Helper.BuildQueue(new int[] { 4, 5, 6, 7, 8 });
        Queue<int> Q2 = Unit4Helper.BuildQueue(new int[] { 20, 21, 22, 25, 26 });
        Queue<int> Q3 = Unit4Helper.BuildQueue(new int[] { 7, 9, 10});
        Queue<int> Q4 = Unit4Helper.BuildQueue(new int[] { 1, 5, 8, 9, 10 });
        Stack<Queue<int>> SQ = new Stack<Queue<int>>();
        SQ.Push(Q4);
        SQ.Push(Q3);
        SQ.Push(Q2);
        SQ.Push(Q1);
        //Console.WriteLine(SQ);

        //Node<int> lst = Comp(Q2);
        //Unit4Helper.PrintList(lst);
        //Stack<Node<int>> SL = Complement(SQ);
        //while (!SL.IsEmpty())
        //{
        //    Unit4Helper.PrintList(SL.Pop());
        //}

        Node<int> lst1 = Unit4Helper.BuildNodeList(new int[] { -99, 23, 7, -3, 4, 6, 5, -4, 1, 3, 17 });
        Console.WriteLine(Sub_series(lst1, 4));
        Console.WriteLine(Sub_series(lst1, 3));
        Console.WriteLine(Sub_series(lst1, 0));

        Node<int> lst2 = Unit4Helper.BuildNodeList(new int[] { -99, 23, 7, 3, 4, 6, 5, -4, 1, 3, 17 });
        Node<int> lst3= Unit4Helper.BuildNodeList(new int[] { -99, 23, 7, -3, 4, 6, 5, -4, 1, 3, 17, 99 });

        Console.WriteLine(max_subList(lst1));
        Console.WriteLine(max_subList(lst2));
        Console.WriteLine(max_subList(lst3));


        Node<int> lst4 = CreateLst(new int[] { -99, 23, 7, 3, 4, 6, 5, -4, 1, 3, 17 });
        Unit4Helper.PrintList(lst4);

        Node<double> lst5 = Unit4Helper.BuildNodeList(new double[] { 4, 1.5, 2, -2, 4, 0.5, 2.5, 1 });
        Unit4Helper.PrintList(DeleteRange(lst5, 1, 3, 3));
    }
}


