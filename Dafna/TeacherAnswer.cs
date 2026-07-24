using System;
using Unit4;

public static class TeacherAnswer
{
    public static void PrintArray(int[] a)
    {
        PrintArray(a, 0);
    }

    public static void PrintArray(int[] a, int i)
    {
        if (i == a.Length - 1)
            Console.Write(a[i]);
        else
        {
            Console.Write(a[i] + " , ");
            PrintArray(a, i + 1);
        }
    }

    public static void PrintArray1(int[] a)
    {
        PrintArray1(a, a.Length - 1);
    }

    public static void PrintArray1(int[] a, int i)
    {
        if (i == 0)
            Console.Write(a[0]);
        else
        {
            PrintArray1(a, i - 1);
            Console.Write(" , " + a[i]);
        }
    }

    public static int SumArray(int[] a)
    {
        return SumArray(a, 0);
    }

    public static int SumArray(int[] a, int i)
    {
        if (i >= a.Length)
            return 0;
        return SumArray(a, i + 1) + a[i];
    }

    public static int CountArray27or49(int[] a)
    {
        return CountArray27or49(a, 0);
    }

    public static int CountArray27or49(int[] a, int i)
    {
        if (i >= a.Length)
            return 0;
        if ((a[i] % 7 == 2) || (a[i] % 9 == 4))
            return CountArray27or49(a, i + 1) + 1;
        return CountArray27or49(a, i + 1);
    }

    public static int CountIndexEqualsValue(int[] a, int i)
    {
        if (i >= a.Length)
            return 0;
        if (a[i] == i)
            return CountIndexEqualsValue(a, i + 1) + 1;
        return CountIndexEqualsValue(a, i + 1);
    }

    public static int MaxArray(int[] a)
    {
        return MaxArray(a, 0);
    }

    public static int MaxArray(int[] a, int i)
    {
        if (i == a.Length - 1)
            return a[i];
        int max = MaxArray(a, i + 1);
        if (max > a[i])
            return max;
        return a[i];
    }

    public static int MinArray(int[] a)
    {
        return MinArray(a, 0);
    }

    public static int MinArray(int[] a, int i)
    {
        if (i == a.Length - 1)
            return a[i];
        int min = MinArray(a, i + 1);
        if (min < a[i])
            return min;
        return a[i];
    }

    public static void PositionXinArray(int[] a, int x)
    {
        PositionXinArray(a, 0, x);
    }

    public static void PositionXinArray(int[] a, int i, int x)
    {
        if (i >= a.Length)
            Console.WriteLine();
        else
        {
            if (a[i] == x)
                Console.Write(i + " ");
            PositionXinArray(a, i + 1, x);
        }
    }

    public static bool ThreeEqualParts(int[] arr)
    {
        if (arr.Length % 3 != 0)
            return false;
        return ThreeEqualParts(arr, 0, arr.Length / 3);
    }

    public static bool ThreeEqualParts(int[] arr, int i, int n)
    {
        if (i >= n)
            return true;
        if ((arr[i] != arr[n + i]) || (arr[i] != arr[2 * n + i]))
            return false;
        return ThreeEqualParts(arr, i + 1, n);
    }

    public static bool IsExist(int[] arr, int x)
    {
        return IsExist(arr, 0, x);
    }

    public static bool IsExist(int[] arr, int i, int x)
    {
        if (i >= arr.Length)
            return false;
        if (arr[i] == x)
            return true;
        return IsExist(arr, i + 1, x);
    }

    public static bool SEqualsSumOf2(int[] a, int s)
    {
        return SEqualsSumOf2(a, 0, s);
    }

    public static bool SEqualsSumOf2(int[] a, int i, int s)
    {
        if (i >= a.Length)
            return false;
        if (IsExist(a, i + 1, s - a[i]))
            return true;
        return SEqualsSumOf2(a, i + 1, s);
    }

    public static bool IsSortedUpArray(int[] arr)
    {
        return IsSortedUpArray(arr, 0);
    }

    public static bool IsSortedUpArray(int[] arr, int i)
    {
        if (i >= arr.Length - 1)
            return true;
        if (arr[i] > arr[i + 1])
            return false;
        return IsSortedUpArray(arr, i + 1);
    }

    public static int GetIndexBinary(int[] array, int x)
    {
        return GetIndexBinary(array, 0, array.Length - 1, x);
    }

    public static int GetIndexBinary(int[] array, int low, int high, int x)
    {
        if (low > high)
            return -1;

        int mid = (low + high) / 2;
        if (array[mid] == x)
            return mid;
        if (array[mid] > x)
            return GetIndexBinary(array, low, mid - 1, x);
        return GetIndexBinary(array, mid + 1, high, x);
    }

    public static void PrintMatrix(int[][] mat, int i, int j)
    {
        if (i >= mat.Length)
            Console.WriteLine();
        else if (j >= mat[i].Length)
        {
            Console.WriteLine();
            PrintMatrix(mat, i + 1, 0);
        }
        else
        {
            Console.Write(mat[i][j] + "  ");
            PrintMatrix(mat, i, j + 1);
        }
    }

    public static int CharInString(string str, char charToFind)
    {
        if (str.Length == 0)
            return 0;

        if (str[0] == charToFind)
            return 1 + CharInString(str.Substring(1), charToFind);
        else
            return CharInString(str.Substring(1), charToFind);
    }

    public static bool IsPalindrome(string str)
    {
        if (str.Length == 1)
            return true;

        if (str[0] != str[str.Length - 1])
            return false;

        return IsPalindrome(str.Substring(1, str.Length - 2));
    }


    public static Node<int> DelNode(Node<int> lst, Node<int> node)
    {

        lst = new Node<int>(0, lst);
        Node<int> p = lst;
        while (p.HasNext())
        {
            if (p.GetNext() == node)
            {
                p.SetNext(p.GetNext().GetNext());
                return lst.GetNext();
            }
            p = p.GetNext();
        }
        return lst.GetNext();

    }

    public static NodeInteger DelNodeInt(NodeInteger lst, NodeInteger node)
    {

        lst = new NodeInteger(0, lst);
        NodeInteger p = lst;
        while (p.HasNext())
        {
            if (p.GetNext() == node)
            {
                p.SetNext(p.GetNext().GetNext());
                return lst.GetNext();
            }
            p = p.GetNext();
        }
        return lst.GetNext();

    }

}