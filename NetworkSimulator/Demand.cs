using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    /// <summary>
    /// Абстрактный класс для требования
    /// </summary>
    public abstract class Demand
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long ID
        {
            get;
            set;
        }
        /// <summary>
        /// Время создания требования
        /// </summary>
        public double TimeGeneration { get; set; }

    }
}
