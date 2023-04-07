using System;

namespace Task_A_2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var singleLinkedList = new SingleLinkedList<int>();

            // adding
            singleLinkedList.Add(1);
            singleLinkedList.Add(2);
            singleLinkedList.Add(3);
            singleLinkedList.Add(2);
            singleLinkedList.Add(4);

            foreach (var value in singleLinkedList)
            {
                Console.Write($"{value} ");
            }

            Console.WriteLine();

            // replacing
            singleLinkedList.Replace(1, 5);
            singleLinkedList.Replace(3, 6);
            singleLinkedList.Replace(4, 9);

            foreach (var value in singleLinkedList)
            {
                Console.Write($"{value} ");
            }

            Console.WriteLine();

            // joinig
            var singleLinkedList2 = new SingleLinkedList<int>
            {
                11,
                12,
                13
            };
            singleLinkedList.JoinWith(singleLinkedList2);

            foreach (var value in singleLinkedList)
            {
                Console.Write($"{value} ");
            }

            Console.ReadLine();
        }
    }
}
