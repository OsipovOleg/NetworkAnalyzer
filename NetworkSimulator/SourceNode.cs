﻿using RandomVariables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    public class SourceNode : Node
    {
        /// <summary>
        /// Среднее время отклика
        /// </summary>
        public List<double> ResponseTimes
        {
            get;
            private set;
        }








        /// <summary>
        /// Получение требования (возврат требования в исчтоник) 
        /// </summary>
        /// <param name="f"></param>
        public override void Receive(Fragment f)
        {
            //Console.WriteLine("Требование вернулось");
            ResponseTimes.Add(Info.GetCurentTime() - f.TimeGeneration);
        }

        /// <summary>
        /// Отправяляет требование из источника по сети 
        /// </summary>
        /// <param name="f"></param>
        protected override void Route(Fragment f)
        {
            double rand = r.NextDouble();
            double p = 0;
            for (int i = 0; i < RouteRow.Length; i++)
            {
                p += RouteRow[i];
                if (rand < p)
                {
                    //Посылаем фрагмент в указанный узел
                    Send(f, Nodes[i]);
                    break;
                }
            }
        }

        /// <summary>
        /// Отправление требования от источника к другому узлу
        /// </summary>
        /// <param name="f">Требование</param>
        /// <param name="N">Узел-получатель</param>
        protected override void Send(Fragment f, Node N)
        {
            N.Receive(f);
        }

        /// <summary>
        /// Счетчик всех созданных фрагментов
        /// </summary>
        private long FragmentCounter
        {
            get;
            set;
        }


        /// <summary>
        /// Передача управления источнику
        /// </summary>
        public override void Activate()
        {
            //Создаем требование
            Fragment f = new Fragment(Info.GetCurentTime(), FragmentCounter, new Signature(null, 1, 0));
            //Требование ссылается само на себя
            f.Sigma.ParentFragment = f;
            FragmentCounter++;

            //Время следующего события
            NextEventTime = Info.GetCurentTime() + ArrivalInterval.NextValue();
            //Отправка требования по сети обслуживания
            Route(f);
        }

        /// <summary>
        /// Случайная величина между (интервалы между требованиями) 
        /// </summary>
        protected RandomVariable ArrivalInterval
        {
            get;
            set;
        }


        /// <summary>
        /// Маршрутная строка только для смежных узлов
        /// </summary>
        private double[] RouteRow
        {
            get;
            set;
        }

        /// <summary>
        /// Инициализация источника требований
        /// </summary>
        /// <param name="r">Интервалы между поступлениями требований</param>
        /// <param name="RouteRow">Строка для маршрутизации требований</param>
        /// <param name="ID">Идентификатор узла</param>
        public SourceNode(int ID, Random r, RandomVariable ArrivalInterval, Node[] Nodes, InfoNode Info, double[] RouteRow)
        {
            //Передача параметров
            this.ID = ID;
            this.r = r;
            this.ArrivalInterval = ArrivalInterval;
            this.Nodes = Nodes;
            this.Info = Info;
            this.r = r;
            this.RouteRow = RouteRow;

            //Первое поступление происходит в нулевой момент времени
            this.NextEventTime = 0;
            FragmentCounter = 0;
            //Для сбора статистики 
            ResponseTimes = new List<double>();
        }
    }
}
