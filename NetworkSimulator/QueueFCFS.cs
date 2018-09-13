using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    /// <summary>
    /// Очередь с дисциплной FCFS
    /// </summary>
    public class QueueFCFS : Buffer
    {

        private Queue<Fragment> queue;
        /// <summary>
        /// Создает очередь FCFS
        /// </summary>
        public QueueFCFS()
        {
            queue = new Queue<Fragment>();
        }

        /// <summary>
        /// Проверка очереди на пустоту
        /// </summary>
        /// <returns></returns>
        public override bool IsEmpty()
        {
            if (queue.Count() == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Число элментов в очереди
        /// </summary>
        /// <returns></returns>
        public override int Length()
        {
            return queue.Count;
        }

        /// <summary>
        /// Помещает элемент в очередь
        /// </summary>
        /// <param name="f">Помещаемый фрагмент</param>
        public override void Put(Fragment f)
        {
            queue.Enqueue(f);
        }


        /// <summary>
        /// Выбор первого элемента из очереди
        /// </summary>
        /// <returns></returns>
        public override Fragment Take()
        {
            return queue.Dequeue();
        }


    }
}
