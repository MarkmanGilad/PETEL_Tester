using System;

namespace LinkedListProject
{
    public class ListUtilDafnaM
    {
        
        public static int CountList (Node<int> list)
        {
            Node<int> temp = list;           //  !temp הערה: אפשר גם בלי 
            int count = 0;
            while (temp != null)
            {
                count++;
                temp = temp.GetNext();
            }
            return count;
        }
        
        public static int SumList(Node<int> list)
        {
            int sum = 0;
            while (list != null)
            {
                sum += list.GetValue();
                list = list.GetNext();
            }
            return sum;
        }
        
        public static int MaxList(Node<int> list)
        {
            if (list == null)
                return int.MaxValue;
            int max = list.GetValue();    
            list = list.GetNext();
            while (list != null)
            {
                if (max < list.GetValue())
                    max = list.GetValue();
                list = list.GetNext();
            }
            return max;
        }
    
    	public static bool IsExistXinList (Node<int> list, int n)
        {
    	    Node<int> temp=list;
        
            while(temp!=null)
            {
                if(n == temp.GetValue())
                   return true;
                temp=temp.GetNext();
            }
            return false;
        }
    
        public static bool IsSortedUp(Node<int> chain)
        {
            Node<int> temp = chain;
            if (temp == null)
                return true;
            if (temp.GetNext() == null)
                return true;
            while (temp.GetNext() != null)
            {
                if (temp.GetValue() > temp.GetNext().GetValue())
                    return false;
                temp = temp.GetNext();
            }
            return true;
        }
        
        public static Node<int> BuildReverseRandom()
        {
            Random rnd = new Random();
            Node<int> chain = null;
            int x = rnd.Next(10, 101);
            while (x != 100)
            {
                chain = new Node<int>(x, chain);
                x = rnd.Next(10, 101);
            }
            return chain;
        }

        public static Node<int> BuildRandom()
        {
            Random rnd = new Random();
            int x = rnd.Next(10, 101);
            if (x == 100)
                return null;
            Node<int> chain = new Node<int>(x);
            x = rnd.Next(10, 101);
            Node<int> temp = chain;
            while (x != 100)
            {
                temp.SetNext(new Node<int>(x));
                x = rnd.Next(10, 101);
                temp = temp.GetNext();
            }
            return chain;
        }

        public static Node<int> Build()
        {
            Console.Write("Enter a 2-digit number, to End with 100: ");
            int x = int.Parse(Console.ReadLine());
            if (x == 100)
                return null;
            Node<int> chain = new Node<int>(x);
            Console.Write("Enter the next 2-digit number, to End with 100: ");
            x = int.Parse(Console.ReadLine());
            Node<int> temp = chain;
            while (x != 100)
            {
                temp.SetNext(new Node<int>(x));
                Console.Write("Enter the next 2-digit number, to End with 100: ");
                x = int.Parse(Console.ReadLine());
                temp = temp.GetNext();
            }
            return chain;
        }

        public static void PrintList (Node<int> list)
        {
            if (list == null)
                Console.WriteLine(" -->||");
            else
            {
                Node<int> temp = list;
                while (temp != null)
                {
                    Console.Write(" --> " + temp.GetValue());
                    temp = temp.GetNext();
                }
                Console.WriteLine(" -->||");
            }
        }

        public static void OddPlaces(Node<int> lst)
        {
            if (lst != null)
            {
                Node<int> p1 = lst;
                while (p1 != null && p1.GetNext() != null)
                {
                    Node<int> p2 = p1.GetNext();
                    p1.SetNext(p2.GetNext());
                    p2.SetNext(null);
                    p1 = p1.GetNext();
                }
            }
        }

    }
}
