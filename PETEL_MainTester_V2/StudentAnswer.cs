using Unit4;

public static class StudentAnswer
{
    
    public static int CountValues(Node<int> head, int value)
    {

        if (head == null)
            return 0;

        int count = head.GetValue() == value ? 1 : 0;
    
        return count + CountValues(head, value);
    }
}


