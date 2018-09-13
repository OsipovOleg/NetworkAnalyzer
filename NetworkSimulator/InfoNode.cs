using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    /// <summary>
    /// Информационный узел в сети обслуживания
    /// </summary>
    public class InfoNode : Node
    {
        public override void Receive(Fragment f)
        {
            throw new NotImplementedException();
        }

        protected override void Route(Fragment f)
        {
            throw new NotImplementedException();
        }

        protected override void Send(Fragment f, Node N)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Отображает текущее время
        /// </summary>
        private double CurrentTime;

        /// <summary>
        /// Задает текущее время
        /// </summary>
        /// <param name="t">Значение для текущего времени</param>
        public void SetCurentTime(double t)
        {
            CurrentTime = t;
        }
        /// <summary>
        /// Запрос текущего времени
        /// </summary>
        /// <returns>Возвращает текущее время</returns>
        public double GetCurentTime()
        {
            return CurrentTime;
        }

        public override void Activate()
        {
            throw new NotImplementedException();
        }
    }
}
