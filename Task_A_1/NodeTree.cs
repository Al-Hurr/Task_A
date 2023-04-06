
using System.Collections.Generic;

namespace Task_A_1
{
    public class NodeTree
    {
        public int Data { get; set; }
        public NodeTree Left { get; set; }
        public NodeTree Rigth { get; set; }

        public static void PrintTreeToDepth(NodeTree nodeTree)
        {
            if(nodeTree == null)
            {
                return;
            }

            System.Console.Write($"{nodeTree.Data} ");

            PrintTreeToDepth(nodeTree.Left);
            PrintTreeToDepth(nodeTree.Rigth);
        }

        public static void PrintTreeToWidth(NodeTree nodeTree)
        {
            if(nodeTree == null)
            {
                return;
            }

            Queue<NodeTree> nodesQueue = new Queue<NodeTree>();
            nodesQueue.Enqueue(nodeTree);

            while(nodesQueue.Count > 0)
            {
                NodeTree node = nodesQueue.Dequeue();

                System.Console.Write($"{node.Data} ");

                if(node.Left != null)
                {
                    nodesQueue.Enqueue(node.Left);
                }
                if(node.Rigth != null)
                {
                    nodesQueue.Enqueue(node.Rigth);
                }
            }
        }

        public void PutNode(NodeTree node)
        {
            if (node == null
                || node.Data == Data)
            {
                return;
            }

            if(node.Data < Data)
            {
                if(Left == null)
                {
                    Left = node;
                }
                else
                {
                    Left.PutNode(node);
                }
            }

            if(node.Data > Data)
            {
                if(Rigth == null)
                {
                    Rigth = node;
                }
                else
                {
                    Rigth.PutNode(node);
                }
            }
        }
    }
}
