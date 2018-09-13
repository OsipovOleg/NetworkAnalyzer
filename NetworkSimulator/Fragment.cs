using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSimulator
{
    /// <summary>
    /// Класс для сигнатуры фрагмента 
    /// </summary>
    public class Signature
    {
        /// <summary>
        /// Указатель на фрагмент-родитель
        /// </summary>
        public Fragment ParentFragment
        {
            get;
            set;
        }
        /// <summary>
        /// Номер фрагмента среди фрагментов, которые были получены при делении
        /// </summary>
        public int SubID
        {
            get;
            set;
        }

        /// <summary>
        /// Идентификатор дивайдера на котором произошло деление фрагмента, 
        /// 0 если фрагмент является требованием 
        /// </summary>
        public int ForkNodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Создает сигнатуру для фрагмента
        /// </summary>
        /// <param name="ParentDemand">Фрагмент-родитель</param>
        /// <param name="SubID">Идентификатор полученого фрагмента</param>
        /// <param name="ForkNodeID">Идентификатор дивайдера, ко котором был получен фрагмент</param>
        public Signature(Fragment ParentDemand, int SubID, int ForkNodeID)
        {
            this.ParentFragment = ParentDemand;
            this.SubID = SubID;
            this.ForkNodeID = ForkNodeID;
        }
    }
    /// <summary>
    /// Фрагмент в сети с делением и слиянием требований
    /// </summary>
    public class Fragment : Demand
    {

        /// <summary>
        /// Сигнатура фрагмента
        /// </summary>
        public Signature Sigma
        {
            get;
            set;
        }

        /// <summary>
        /// Создание фрагмента(требования) 
        /// </summary>
        /// <param name="TimeGeneration">Время создания</param>
        /// <param name="ID">Идентификатор фрагмента</param>
        /// <param name="Sigma">Сигнатура фрамента</param>
        public Fragment(double TimeGeneration, long ID, Signature Sigma)
        {
            this.TimeGeneration = TimeGeneration;
            this.ID = ID;
            this.Sigma = Sigma;

            this.NumberOfParts = 1;
        }

        /// <summary>
        /// Число частей на которые был поделен фрагмент
        /// </summary>
        public int NumberOfParts
        {
            get;
            set;

        }



        /// <summary>
        /// Время поступления
        /// </summary>
        public double TimeArrival { get; set; }
        /// <summary>
        /// Время начала обслуживания
        /// </summary>
        public double TimeStartService { get; set; }
        /// <summary>
        /// Время завершения обслуживания
        /// </summary>
        public double TimeLeave { get; set; }

        /// <summary>
        /// Общее время пребывания в сети обслуживания 
        /// </summary>
        public double TotalTime { get; set; }

    }
}


