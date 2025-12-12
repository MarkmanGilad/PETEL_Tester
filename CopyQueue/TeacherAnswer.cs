using System;
using Unit4;

namespace PETEL_VPL
{
    class TeacherAnswer
    {
        public static Queue<int> Copy(Queue<int> q)
        {
            Queue<int> q1 = new Queue<int>();
            Queue<int> temp = new Queue<int>();
            while (!q.IsEmpty())
            {
                int item = q.Remove();
                temp.Insert(item);
                q1.Insert(item);
            }
            while (!temp.IsEmpty())
            {
                q.Insert(temp.Remove());
            }
            return q1;
        }
    }
}

