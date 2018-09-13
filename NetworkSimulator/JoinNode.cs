using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    public class JoinNode : Node
    {

        /// <summary>
        /// Матрица для маршрутизции фрагмнентов
        /// Элемент матрицы i,j задает вероятность для i-фрагмента поступить в j узел
        /// </summary>
        protected double[,] RouteMatrixForNode
        {
            get;
            set;
        }

        /// <summary>
        /// Буффер для хранения фрагментов в дивадере
        /// </summary>
        protected List<Fragment> InBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Процедура приема фрагмента в интегратор 
        /// </summary>
        /// <param name="f"></param>
        public override void Receive(Fragment f)
        {
            //Поиск родственных фрагментов
            for (int i = 0; i < InBuffer.Count; i++)
            {
                if (InBuffer[i].Sigma.ParentFragment == f.Sigma.ParentFragment)
                {
                    InBuffer[i].NumberOfParts--;
                    if (InBuffer[i].NumberOfParts == 1)
                    {
                        var parent = InBuffer[i].Sigma.ParentFragment;
                        InBuffer.RemoveAt(i);
                        Route(parent);
                    }
                    return;
                }
            }
            //Если таких фрагментов нет, то просто добавляем фрагмент в очередь на ожидание 
            InBuffer.Add(f);
            //Время активации - бескончность
            NextEventTime = Double.PositiveInfinity;
        }

        /// <summary>
        /// Отправляет фрагмент по сети
        /// </summary>
        /// <param name="f"></param>
        protected override void Route(Fragment f)
        {
            double rand = r.NextDouble();
            double p = 0;
            int k = f.Sigma.ForkNodeID;

            for (int i = 0; i < RouteMatrixForNode.GetLength(1); i++)
            {
                p += RouteMatrixForNode[k, i];
                if (rand < p)
                {
                    //Посылаем фрагмент в указанный узел
                    Send(f, Nodes[i]);
                    break;
                }
            }
        }

        /// <summary>
        /// Отправка фрагмент в заданый узел сетиобслуживания
        /// </summary>
        /// <param name="f">Отправляемый фрагмент</param>
        /// <param name="N">Заданный узел</param>
        protected override void Send(Fragment f, Node N)
        {
            N.Receive(f);
        }
        /// <summary>
        /// Выполняемое действие дивайдера
        /// </summary>
        public override void Activate()
        {
            //Следующий момент активации 
            NextEventTime = double.PositiveInfinity;
        }

        /// <summary>
        /// Создание и инициализация интератора
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="r"></param>
        /// <param name="RouteMatrixForNode">Элемент матрицы i,j задает вероятность для i-фрагмента поступить в j узел</param>
        public JoinNode(int ID, Random r, Node[] Nodes, InfoNode Info, double[,] RouteMatrixForNode)
        {
            this.ID = ID;
            this.r = r;
            this.Nodes = Nodes;
            this.Info = Info;
            this.RouteMatrixForNode = RouteMatrixForNode;
            this.NextEventTime = double.PositiveInfinity;

            InBuffer = new List<Fragment>();
        }


    }
}
