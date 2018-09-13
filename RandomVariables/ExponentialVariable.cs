using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomVariables
{
    /// <summary>
    /// Генерирует значения случайной величины с экспоненциальным распределением
    /// </summary>
    public class ExponentialVariable:RandomVariable
    {
        /// <summary>
        /// Параметр для экспоненциально распределенной случайной величины
        /// </summary>
        public double Rate
        {
            get;
            set;
        }

        /// <summary>
        /// Получает следующее значение случайной величины
        /// </summary>
        /// <returns></returns>
        public override double NextValue()
        {
            return -1.0 / Rate* Math.Log(this.r.NextDouble());
        }

        /// <summary>
        /// Создает генератор экспоненциально распределенной случайной величины
        /// </summary>
        /// <param name="r">Генератор случайных чисел</param>
        /// <param name="Rate">Параметр распределения</param>
        public ExponentialVariable(Random r, double Rate)
        {
            this.r = r;
            this.Rate = Rate; 
            
        }
            
    }
}
