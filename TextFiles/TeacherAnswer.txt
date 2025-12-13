using System;
using Unit4;

namespace PETEL_VPL
{

    class TeacherAnswer
    {
        public static int countRemoveItem(Queue<int> q, int num)
        {
            Queue<int> temp = new Queue<int>();
            int count = 0;

            while (!q.IsEmpty())
            {
                int item = q.Remove();
                if (num == item)
                {
                    count++;
                }
                else
                {
                    temp.Insert(item);
                }
            }
            while (!temp.IsEmpty())
            {
                q.Insert(temp.Remove());
            }
            return count;
        }
    }
}