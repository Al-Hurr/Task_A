
namespace Task_A_2
{
    public class ListNode<T>
    {
        public readonly T Value;
        public readonly ListNode<T> Next;

        public ListNode(T value)
        {
            Value = value;
        }

        public ListNode(T value, ListNode<T> next)
        {
            Value = value;
            Next = next;
        }
    }
}
