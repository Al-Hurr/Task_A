using System;

namespace Task_A_1
{
    class Program
    {
        static void Main(string[] args)
        {
            NodeTree node = new NodeTree
            {
                Data = 8,
                Left = new NodeTree
                {
                    Data = 3,
                    Left = new NodeTree
                    {
                        Data = 1
                    },
                    Rigth = new NodeTree
                    {
                        Data = 6,
                        Left = new NodeTree
                        {
                            Data = 4
                        },
                        Rigth = new NodeTree
                        {
                            Data = 7
                        }
                    }
                },
                Rigth = new NodeTree
                {
                    Data = 10,
                    Rigth = new NodeTree
                    {
                        Data = 14,
                        Left = new NodeTree
                        {
                            Data = 13
                        }
                    }
                }
            };

            NodeTree.PrintTreeToDepth(node);
            Console.WriteLine();

            node.PutNode(new NodeTree(9));
            node.PutNode(new NodeTree(0));
            node.PutNode(new NodeTree(16));

            NodeTree.PrintTreeToDepth(node);
            Console.WriteLine();
            NodeTree.PrintTreeToWidth(node);

            Console.ReadLine();
        }
    }
}
