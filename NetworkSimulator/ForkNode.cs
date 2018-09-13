using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSimulator
{
    /// <summary>
    /// Дивайдер
    /// </summary>
    public class ForkNode : Node
    {
        /// <summary>
        /// Идентификатор дивайдера (собственный номер дивайдера)
        /// Имеет значение 1,2,... 
        /// </summary>
        public int ForkNodeID
        {
            get;
            protected set;
        }

        /// <summary>
        /// Маршрутная строка для дивайдера
        /// </summary>
        protected double[] RouteRow
        {
            get;
            set;
        }

        /// <summary>
        /// Число фрагментов на которое делится поступающий фрагмент
        /// </summary>
        private int ForkDegree
        {
            get;
        }

        /// <summary>
        /// Создание дивайдера
        /// </summary>
        /// <param name="ID">Идентификатор системы</param>
        /// <param name="ForkNodeID">Идентификатор дивайдера</param>
        /// <param name="r">Генератор случайных чисел</param>
        /// <param name="Nodes">Узлы для обмена фрагментами</param>
        /// <param name="Info">Информационный узел</param>
        /// <param name="RouteRow">Строка для маршрутизации. Элемент строки строки с номером i задает число фрагментов, которые поступят в 
        /// систему Nodes[i]/// </param>
        public ForkNode(int ID, int ForkNodeID, Random r, Node[] Nodes, InfoNode Info, double[] RouteRow)
        {
            //Передача параметров
            this.ID = ID;
            this.ForkNodeID = ForkNodeID;
            this.r = r;
            this.Nodes = Nodes;
            this.Info = Info;
            this.RouteRow = RouteRow;

            //Обнуление числа поступивших фрагментов
            this.NumberOfArrivedDemads = 0;

            //Число фрагметов, получаемых при делении
            this.ForkDegree = (int)RouteRow.Sum();
            NextEventTime = double.PositiveInfinity;
        }


        /// <summary>
        /// Отправляет фрагмент указанному узлу
        /// </summary>
        /// <param name="f">Отправляемый фрагмент</param>
        /// <param name="N">Узел-получатель</param>
        protected override void Send(Fragment f, Node N)
        {
#if DEBUG1
            Console.WriteLine("Отправка фрагмента из дивайдера {0} в узел {1}", this.ForkNodeID, N.ID);
            Console.ReadKey();
#endif
            N.Receive(f);
        }


        /// <summary>
        /// Распределяет фрагмент по узлам
        /// </summary>
        /// <param name="f">Фрагмент, поступивший в дивайдер</param>
        /// <returns></returns>
        protected override void Route(Fragment f)
        {
            //Номер фрагмента начиная с единицы
            int partIndex = 1;
            //Проход по каждому смежному узлу
            for (int i = 0; i < Nodes.Length; i++)
            {
                //Создаем фрагмент необходимое колчиество раз
                for (int j = 0; j < RouteRow[i]; j++)
                {
                    Fragment part = new Fragment(f.TimeGeneration, f.ID, new Signature(f, partIndex, ForkNodeID));
                    part.NumberOfParts = ForkDegree;
                    //Отправляем фрагмент в смежный узел
                    Send(part, Nodes[i]);
                    //Увеличиваем индекс фрагмента
                    partIndex++;
                }
            }
        }


        /// <summary>
        /// Получение фрагмента из какого-то узла
        /// </summary>
        /// <param name="f">Получаемый фрагмент</param>
        public override void Receive(Fragment f)
        {
            NumberOfArrivedDemads++;
            Route(f);
        }

        /// <summary>
        /// Активация дивайдера
        /// </summary>
        public override void Activate()
        {
        }
    }
}
