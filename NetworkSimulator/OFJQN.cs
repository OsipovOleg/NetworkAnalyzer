
using NetworkDescriptions;
using RandomVariables;
using System;
using System.Diagnostics;

namespace NetworkSimulator
{
    /// <summary>
    /// Имитционная модель для открытой экспоненциальносй сети массового обслуживания 
    /// с делением и слиянием требований и произвольным числом обслуживающих прибором с дисциплиной FCFS
    /// </summary>
    public static class OFJQN
    {
        /// <summary>
        /// Создает экземпляр класса NetworkModel по описанию сети обслуживания
        /// </summary>
        /// <returns>The network model.</returns>
        /// <param name="Description">Описание сети обслуживания</param>
        public static NetworkModel CreateNetworkModel(DescriptionOFJQN Description, Random rand)
        {
            InfoNode Info = new InfoNode();
            Info.SetCurentTime(0);

            var Nodes = new Node[Description.Theta.Dimention];
            //Источник требований
            Nodes[0] = new SourceNode(0, rand, new ExponentialVariable(rand, Description.Lambda0), Nodes, Info, Description.Theta.RoutingRow(0, 0));
            //Базовые системы
            for (int i = 0; i < Description.S.Length; i++)
            {

                Nodes[Description.S[i]] = new ServiceNode(Description.S[i], rand, new RandomVariables.ExponentialVariable(rand, Description.mu[i]),
                    new QueueFCFS(), Description.kappa[i], Nodes, Info, Description.Theta.RoutingMatrixForNode(Description.S[i]));
            }
            //Дивайдеры
            for (int k = 0; k < Description.F.Length; k++)
            {
                Nodes[Description.F[k]] = new ForkNode(Description.F[k], k + 1, rand, Nodes, Info, Description.Theta.RoutingRow(Description.F[k], k + 1));
            }
            //Интеграторы
            for (int k = 0; k < Description.J.Length; k++)
            {
                Nodes[Description.J[k]] = new JoinNode(Description.J[k], rand, Nodes, Info, Description.Theta.RoutingMatrixForNode(Description.J[k]));
            }



            return new NetworkModel(Nodes, Info, rand); 
        }






    }
}
