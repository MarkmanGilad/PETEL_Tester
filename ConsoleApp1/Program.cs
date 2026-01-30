using Unit4;


static int CountValues(Node<int> head, int value)
{

    if (head == null)
        return 0;
    int count = head.GetValue() == value ? 1 : 0;

    return count + CountValues(head, value);
}

int[] arr = { 1, 5, 2, 5, 3, 5, 4, 5, 6, 7, 8, 9, 10 };
Node<int> list = Unit4Helper.BuildNodeList(arr);
Console.WriteLine(CountValues(list, 5));