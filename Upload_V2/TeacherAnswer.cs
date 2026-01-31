using Unit4;

public static class TeacherAnswer
{
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
}
