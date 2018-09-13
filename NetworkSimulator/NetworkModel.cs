using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    /// <summary>
    ///  Имитационная модель сети обслуживания
    /// </summary>
    public class NetworkModel
    {
        /// <summary>
        /// Узлы в сети массового обслуживания
        /// </summary>
        public Node[] Nodes
        {
            get;
            private set;
        }

        /// <summary>
        /// Информационный узел
        /// </summary>
        public InfoNode Info
        {
            get;
            private set;
        }

        /// <summary>
        /// Генератор случайных чисел
        /// </summary>
        private Random r
        {
            get;
            set;
        }

        public NetworkModel(Node[] Nodes, InfoNode Info, Random r)
        {
            this.Nodes = Nodes;
            this.Info = Info;
            this.r = r;
        }

        /// <summary>
        /// Запуск имитационной модели сети 
        /// </summary>
        /// <param name="FinishTime">Время окончания моделирования</param>
        public void Run(double FinishTime)
        {
            double CurrentTime = 0;
            //Текущее время передаем информационному узлу
            Info.SetCurentTime(CurrentTime);
            Node NextActionNode;
            double NextTime;

            while (CurrentTime <= FinishTime)
            {
                //Выбор узла для передачи управления 
                NextActionNode = Nodes[0];
                NextTime = Nodes[0].NextEventTime;
                for (int i = 1; i < Nodes.Length; i++)
                {
                    if (Nodes[i].NextEventTime < NextTime)
                    {
                        NextActionNode = Nodes[i];
                        NextTime = NextActionNode.NextEventTime;
                    }
                }
                //Установка времени
                CurrentTime = NextTime;
                Info.SetCurentTime(CurrentTime);
                //Console.WriteLine("t = {0:f4}", NextTime);
                //Console.ReadKey();
                //Передаем управление нашему узлу
                NextActionNode.Activate();
            }
        }


        /// <summary>
        /// Выполняет анализ имитационной модели
        /// </summary>
        public void Analysis(out double AverageRT)
        {
            //Вывод статистики
            Console.WriteLine("E(tau) = {0:f4}", (Nodes[0] as SourceNode).ResponseTimes.Average());
            double tau = (Nodes[0] as SourceNode).ResponseTimes.Average();
            AverageRT = tau; 
            var rt = (Nodes[0] as SourceNode).ResponseTimes;
            List<double> vars = new List<double>();
            foreach (var item in rt)
            {
                vars.Add((item - tau) * (item - tau));
            }
            Console.WriteLine("Var(tau) = {0:f4}", vars.Average());
        }
    }
}
