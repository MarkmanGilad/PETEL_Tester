using Unit4;

namespace PETEL_VPL
{
    public static class StudentAnswer
    {
        /// <summary>
        /// Counts how many times a value appears in a linked list using recursion
        /// </summary>
        /// <param name="head">Head of the linked list</param>
        /// <param name="value">Value to count</param>
        /// <returns>Number of occurrences</returns>
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
}