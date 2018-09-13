﻿using RandomVariables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    /// <summary>
    /// Метка требования, находящегося на обслуживании (на приборе)
    /// </summary>
    public class Label : IComparable<Label>
    {
        /// <summary>
        /// Время ухода фрагмента
        /// </summary>
        public double TimeLeave
        {
            get;
            set;
        }
        /// <summary>
        /// Идентификатор метки
        /// </summary>
        public long ID
        {
            get;
            set;
        }

        public Label(double TimeLeave, long ID)
        {
            this.TimeLeave = TimeLeave;
            this.ID = ID;
        }

        public int CompareTo(Label other)
        {
            if (this.TimeLeave > other.TimeLeave)
            {
                return 1;
            }
            if (this.TimeLeave < other.TimeLeave)
            {
                return -1;
            }
            return 0;
        }
    }

    /// <summary>
    /// Базовая система
    /// </summary>
    public class ServiceNode : Node
    {
        /// <summary>
        /// Матрица для маршрутизации фрагментов
        ///Элемент матрицы i,j задает вероятность для i-фрагмента поступить в j узел
        /// </summary>
        protected double[,] RouteMatrix
        {
            get;
            set;
        }

        /// <summary>
        /// Очередь базовой системы
        /// </summary>
        protected Buffer InBuffer
        {
            get;
            set;
        }
        /// <summary>
        /// Число одинаковых обслуживающих приборов
        /// </summary>
        public int Kappa
        {
            get;
            private set;
        }
        /// <summary>
        /// Случайная величина - длительность обслуживания фрагмента на приборе
        /// </summary>
        protected RandomVariable ServiceTime
        {
            get;
            set;
        }

        /// <summary>
        /// Возвращает число фрагментов в базовой системе (очередь + приборы) 
        /// </summary>
        /// <returns></returns>
        public int NumberOfFragments()
        {
            return ListOfFragments.Count + InBuffer.Length();
        }


        //Для статистики
        public const int MaxNumber = 10;
        public double[] StateProbabilities
        {
            get;
            set;
        }
        public double[] ArrivalStateProbabilities
        {
            get;
            set;
        }

        /// <summary>
        /// Время предыдущего события
        /// </summary>
        private double PredEventTime
        {
            get;
            set;
        }
        ///////////

        /// <summary>
        /// Обновляет время активации узла 
        /// </summary>
        protected void UpdateActionTime()
        {
            if (ListOfFragments.Count > 0)
            {
                NextEventTime = ListOfFragments.Keys.Min().TimeLeave; 
            }
            else
            {
                NextEventTime = double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Упорядоченный список номеров приборов согласно времени возникновения в них событий 
        /// Нулевой элемент списка - прибор которому нужно передать упралвение, 
        /// если в списке нет элементов, значит нет загруженных приборов - время акивациии неизвестно 
        /// </summary>
        protected SortedDictionary<Label, Fragment> ListOfFragments
        {
            get;
            set;
        }

        /// <summary>
        /// Базовая система
        /// </summary>
        /// <param name="r">Генратор случайных чисел</param>
        /// <param name="InBuffer">Буффер для фргаментов</param>
        /// <param name="kappa">Число однотипных обслуживающих приборов</param>
        /// <param name="ServiceTime">Случайная величина - длительность обслуживания фрагмента прибором</param>
        /// <param name="RouteMatrix">Матрица маршрутная 
        /// Элемент i,j задает вероятность для i фрагмента попасть в j узел</param>
        /// <param name="Nodes">Узлы</param>
        /// <param name="Info">Информационный узел</param>
        public ServiceNode(int ID, Random r, RandomVariable ServiceTime, Buffer InBuffer, int kappa, Node[] Nodes, InfoNode Info, double[,] RouteMatrix)
        {
            //Копирование параметров
            this.ID = ID;
            this.r = r;
            this.ServiceTime = ServiceTime;
            this.RouteMatrix = RouteMatrix;
            this.Nodes = Nodes;
            this.Info = Info;
            this.InBuffer = InBuffer;
            this.Kappa = kappa;

            //Время активизации узла
            this.NextEventTime = Double.PositiveInfinity;

            //Создаем список фаргментов на приборах
            ListOfFragments = new SortedDictionary<Label, Fragment>();
            //Число поступивших фрагментов
            NumberOfArrivedDemads = 0;



            //Для статистики 
            this.StateProbabilities = new double[MaxNumber];
            this.ArrivalStateProbabilities = new double[MaxNumber];
            this.PredEventTime = 0;
        }

        /// <summary>
        /// Проверка существования свобдного прибора
        /// </summary>
        /// <returns></returns>
        protected bool ExistFreeServer()
        {
            return (Kappa > ListOfFragments.Count());
        }


        /// <summary>
        /// Берет фрагмент из очереди и начинает его обслуживание
        /// </summary>
        protected void StartService()
        {
            //Берем фрагмент из очереди 
            var new_f = InBuffer.Take();
            //Направляем этот фрагмент на свободный обслуживающий прибор, определив время обслуживания
            new_f.TimeStartService = Info.GetCurentTime();
            new_f.TimeLeave = new_f.TimeStartService + ServiceTime.NextValue();
            //Увеличиваем число поступивших на прибор требований
            NumberOfArrivedDemads++;
            //Добавление фрагмента на прибор
            ListOfFragments.Add(new Label(new_f.TimeLeave, NumberOfArrivedDemads), new_f);

            UpdateActionTime();
        }


        /// <summary>
        /// Процедура получения фрагмента базовой системой
        /// Фрагмент ставится в очередь или сразу начинается его обслуживание 
        /// Реализация сегмента поступления фрагмента
        /// </summary>
        /// <param name="f">Получаемый фрагмент</param>
        public override void Receive(Fragment f)
        {
            //Для статистики - Фрагмент застает систему в некотором состоянии
            if (NumberOfFragments() < MaxNumber)
            {
                ArrivalStateProbabilities[NumberOfFragments()]++;

                //Состояние системы изменилось
                //Система находилась в этом состоянии некоторое время
                double delta = Info.GetCurentTime() - PredEventTime;
                PredEventTime = Info.GetCurentTime();
                StateProbabilities[NumberOfFragments()] += delta;

            }



            //Увеличение числа поступивших фрагментов
            NumberOfArrivedDemads++;
            //Устанавливаем для фрагмента текущее время
            f.TimeArrival = Info.GetCurentTime();
            //Плмещаем фрагмент в очередь
            this.InBuffer.Put(f);
            //Если существует свободный сервер то можно начать обслуживание
            if (ExistFreeServer())
            {
                StartService();
            }

        }

        /// <summary>
        /// Направляет фрагмент в какой-либо узел согласно установленным правилам маршрутизации
        /// </summary>
        /// <param name="f">Фрагмент для передачи</param>
        protected override void Route(Fragment f)
        {
            //Для статистики 
            if (NumberOfFragments() < MaxNumber - 1)
            {
                //Состояние системы изменилось
                //Система находилась в этом состоянии некоторое время
                double delta = Info.GetCurentTime() - PredEventTime;
                PredEventTime = Info.GetCurentTime();
                StateProbabilities[NumberOfFragments() + 1] += delta;

            }



            double rand = r.NextDouble();
            double p = 0;
            int k = f.Sigma.ForkNodeID;

            for (int i = 0; i < RouteMatrix.GetLength(1); i++)
            {
                p += RouteMatrix[k, i];
                if (rand < p)
                {
                    //Посылаем фрагмент в указанный узел
                    if (Nodes[i] == this)
                    {
                        Console.WriteLine("Петля");
                    }
                    Send(f, Nodes[i]);
                    break;
                }
            }
        }

        /// <summary>
        /// Посылает фрагмент в указанный узел
        /// </summary>
        /// <param name="f">Посылаемый фрагмент</param>
        /// <param name="N">Узел для отправки</param>
        protected override void Send(Fragment f, Node N)
        {
            //Посылаем фрагмент
            N.Receive(f);
        }

        /// <summary>
        /// Передача управления базовой системе
        /// </summary>
        public override void Activate()
        {
            //Единственное действие это окончание обслуживания
            var key = ListOfFragments.Keys.Min();
            var value = ListOfFragments[key];
            //Удаляем из списка обслуживаемых фрамгентов
            ListOfFragments.Remove(key);
            //Прибор свободен, пытаемся взять на обслуживание новый фрагмент 
            if (InBuffer.Length() > 0)
            {
                StartService();
            }
            else
            {
                //Обновляем время активации 
                UpdateActionTime();
            }
            //Отправляем обслуженный фрагмент в другие узлы 
            Route(value);


        }

    }
}
