using System;
using Unit4;

namespace CopyQueue
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunStudentCode();
            NodeLIst();
            QueueMethods();
            StackMethods();
            BinTreeMethods();
        }

        public static void RunStudentCode()
        {
            Queue<int> q1 = Unit4Helper.BuildQueue(new int[] { 4, 5, -2, 7, 0, 12 });
            //Queue<int> q2 = StudentAnswer.Copy(q1);
            //Console.WriteLine(q2);
            Console.WriteLine(q1);
        }

        public static void NodeLIst()
        {
            Node<int> lst = Unit4Helper.BuildNodeList(new int[] { 4, -2, 7, 0, -1, 0, 0 });
            Unit4Helper.PrintList(lst);
            int[] arr = Unit4Helper.NodeListToArray(lst);
            string str = Unit4Helper.NodeListToString(lst);
            Console.WriteLine(str);
        }
        
        public static void QueueMethods()
        {
            Queue<int> q = Unit4Helper.BuildQueue(new int[] { 4, -2, 7, 0, -1, 0, 0 });
            Console.WriteLine(q);
            int[] arr = Unit4Helper.QueueToArray(q);
        }

        public static void StackMethods() 
        {
            Stack<int> s = Unit4Helper.BuildStack(new int[] { 4, -2, 7, 0, -1, 0, 0 });
            Console.WriteLine(s);
            int[] arr = Unit4Helper.StackToArray(s);
        }

        public static void BinTreeMethods()
        {
            string path = Unit4Helper.GetTreeFilePath("tree.txt");
            BinNode<int> tree = Unit4Helper.BuildBinaryTree<int>(path);
            Unit4Helper.PrintBinaryTree(tree);
            Unit4Helper.PrintBinaryTreeColored(tree);
            string str = Unit4Helper.BinaryTreeToString(tree);
            Console.WriteLine(str);
        }
    }
}
