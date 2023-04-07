using System.Collections;
using System.Collections.Generic;

namespace Task_A_2
{
    public class SingleLinkedList<T> : IReadOnlyCollection<T>
    {
        public int Count => _count;
        public ListNode<T> FirstNode => _firstNode;
        public ListNode<T> LastNode => _lastNode;

        private ListNode<T> _firstNode;
        private ListNode<T> _lastNode;

        private int _count;

        public void Add(T newValue)
        {
            if (_firstNode == null)
            {
                _firstNode = new ListNode<T>(newValue);
                _lastNode = _firstNode;
            }
            else
            {
                _firstNode = RestoreNodeWithAddingValue(_firstNode, newValue);
            }

            _count++;
        }

        public void Replace(T value, T newValue)
        {
            if (_firstNode == null)
            {
                return;
            }
            else
            {
                _firstNode = RestoreNodeListWithReplacing(_firstNode, value, newValue);
            }
        }

        public void JoinWith(SingleLinkedList<T> list)
        {
            if (list == null)
            {
                return;
            }
            if (_firstNode == null)
            {
                _firstNode = list._firstNode;
                _count = list.Count;
                return;
            }
            else
            {
                _firstNode = RestoreNodeListWithAddingList(_firstNode, list);
                _count += list.Count;
            }
        }

        private ListNode<T> RestoreNodeListWithAddingList(ListNode<T> node, SingleLinkedList<T> list)
        {
            if (node.Next == null)
            {
                _lastNode = list.LastNode;
                return new ListNode<T>(node.Value, list.FirstNode);
            }

            var newNode = RestoreNodeListWithAddingList(node.Next, list);

            return new ListNode<T>(node.Value, newNode);
        }

        private ListNode<T> RestoreNodeWithAddingValue(ListNode<T> node, T value)
        {
            if (node.Next == null)
            {
                _lastNode = new ListNode<T>(value);
                return new ListNode<T>(node.Value, _lastNode);
            }

            var newNode = RestoreNodeWithAddingValue(node.Next, value);

            return new ListNode<T>(node.Value, newNode);
        }

        private ListNode<T> RestoreNodeListWithReplacing(ListNode<T> node, T value, T newValue)
        {
            if (node.Value?.Equals(value) ?? false)
            {
                var returnedNode = new ListNode<T>(newValue, node.Next);
                if (node.Next == null)
                {
                    _lastNode = returnedNode;
                }
                return returnedNode;
            }

            if (node.Next == null)
            {
                return node;
            }

            var newNode = RestoreNodeListWithReplacing(node.Next, value, newValue);

            return new ListNode<T>(node.Value, newNode);
        }

        public IEnumerator<T> GetEnumerator()
        {
            ListNode<T> current = _firstNode;
            while (current != null)
            {
                yield return current.Value;
                current = current.Next;
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }
    }
}
