using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    /// <summary>
    /// Узел сети массового обслуживания
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// Число поступивших фрагментов
        /// </summary>
        public long NumberOfArrivedDemads
        {
            get;
            protected set;
        }


        /// <summary>
        /// Идентификатор узла
        /// </summary>
        public int ID
        {
            get;
            protected set;
        }

        /// <summary>
        /// Для генерации случайных чисел
        /// </summary>
        protected Random r;


        /// <summary>
        /// Отправляет фрагмент по сети обслуживания согласно маршрутизации 
        /// </summary>
        /// <param name="f">Фрагмент для отправки</param>
        protected abstract void Route(Fragment f);

        /// <summary>
        /// Отправляет фрагмент в узел
        /// </summary>
        /// <param name="f">Отправляемый фрагмент</param>
        /// <param name="N">отУзел-получатель</param>
        protected abstract void Send(Fragment f, Node N);


        /// <summary>
        /// Узлы с которыми происходит обмен фрагментами
        /// </summary>
        public Node[] Nodes
        {
            get;
            protected set;
        }

        /// <summary>
        /// Информационный узел
        /// </summary>
        public InfoNode Info
        {
            get;
            protected set;
        }


        /// <summary>
        /// Получение фрагмента узлом
        /// </summary>
        /// <param name="f">Получаемый фрагмент</param>
        public abstract void Receive(Fragment f);


        /// <summary>
        /// Время активации узла
        /// </summary>
        public double NextEventTime
        {
            get;
            protected set;
        }

        /// <summary>
        /// Активация узла
        /// </summary>
        public abstract void Activate();
    }
}
