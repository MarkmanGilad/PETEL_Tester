using Unit4;

class StudentAnswer
{
    public static Queue<int> Copy(Queue<int> q)
    {
        Queue<int> q1 = new Queue<int>();
        while (!q.IsEmpty())
        {
            int item = q.Remove();

            q1.Insert(item);
        }
        return q1;
    }
}

