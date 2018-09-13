using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomVariables
{
    /// <summary>
    /// Абстрактный класс для генератора случайных величин
    /// </summary>
    public abstract class RandomVariable
    {
        /// <summary>
        /// Генератор случайных чисел
        /// </summary>
        protected Random r
        {
            get;
            set; 
        }

        public abstract double NextValue(); 
    }
}
