using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSimulator
{
    /// <summary>
    /// Абстрактный класс для буфера (очередь)
    /// </summary>
    public abstract class Buffer
    {
        /// <summary>
        /// Помещает фрагмент в буффер
        /// </summary>
        /// <param name="f">Помещаемый фрагмент</param>
        public abstract void Put(Fragment f);

        /// <summary>
        /// Берет фрагмент из буффера
        /// </summary>
        /// <returns></returns>
        public abstract Fragment Take();

        /// <summary>
        /// Число элеметов в буффере
        /// </summary>
        /// <returns></returns>
        public abstract int Length();

        /// <summary>
        /// Проверка буффера на пустоту
        /// </summary>
        /// <returns>Возвращает true если буффер пуст, иначе flase</returns>
        public abstract bool IsEmpty();
    }
}
