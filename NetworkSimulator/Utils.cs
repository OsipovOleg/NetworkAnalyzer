using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    public static class Utils
    {
        /// <summary>
        /// Получает массив только смежных узлов и строку маршрутизации для них
        /// </summary>
        /// <param name="Nodes">Все узлы </param>
        /// <param name="RouteRow">Строка маршрутизации</param>
        /// <param name="AdjacentNodes">Смежные узлы</param>
        /// <param name="AdjacentRouteRow">Строка марщрутизации для смежных узлов</param>
        public static void RouteForForkNode(Node[] Nodes, double[] RouteRow, out Node[] AdjacentNodes,
            out double[] AdjacentRouteRow)
        {
            //Число смежных узлов 
            int NumberOfAdjacentNodes = RouteRow.Count(x => x > 0);
            AdjacentNodes = new Node[NumberOfAdjacentNodes];

            AdjacentRouteRow = new double[NumberOfAdjacentNodes];
            int j = 0;
            //Отбор только смежных узлов
            for (int i = 0; i < RouteRow.Length; i++)
            {
                if (RouteRow[i] > 0)
                {
                    AdjacentNodes[j] = Nodes[i];
                    AdjacentRouteRow[j] = RouteRow[i];
                    j++;
                }
            }
        }



        /// <summary>
        /// Массив смежных узлов для узла Node
        /// </summary>
        /// <returns>The nodes.</returns>
        /// <param name="Theta">Theta.</param>
        /// <param name="Node">Node.</param>
        public static int[] AdjacentNodes(double[,] Theta, int Node)
        {
            List<int> nodes = new List<int>(); 
            for (int j = 0; j < Theta.GetLength(0); j++)
            {
                if (Theta[Node, j] > 0)
                {
                    nodes.Add(j); 
                }
                
            }
            return nodes.ToArray(); 
        }
    }
}
