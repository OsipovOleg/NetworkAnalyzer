using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace NetworkDescriptions
{
    /// <summary>
    /// Описание открытой экспоненциальной сети массового осблуживания с делением и слиянием требований
    /// Пуассоновский входящий поток 
    /// Многоприборные базовые системы
    /// Дисциплина FCFS
    /// </summary>
    public class DescriptionOFJQN
    {
        /// <summary>
        /// Массив номеров базовы систем
        /// </summary>
        public int[] S
        {
            get;
            set;
        }
        /// <summary>
        /// Массив номеров интеграторов
        /// </summary>
        public int[] J
        {
            get;
            set;
        }
        /// <summary>
        /// Массив номеров дивайдеров
        /// </summary>
        public int[] F
        {
            get;
            set;
        }
        /// <summary>
        /// Матрица передачи 
        /// </summary>
        public RoutingMatrix Theta
        {
            get;
            set;
        }
        /// <summary>
        /// Интенсивность входящего потока
        /// </summary>
        public double Lambda0
        {
            get;
            set;
        }
        /// <summary>
        /// Массив интенсивностей обслуживания
        /// </summary>
        public double[] mu
        {
            get;
            set;
        }

        /// <summary>
        /// Вектор числа приборов для базовых систем в сети обслуживания
        /// </summary>
        public int[] kappa
        {
            get;
            set;
        }

        /// <summary>
        /// Создание описания для открытой сети массового осблуживания с делением и слиянием требований
        /// </summary>
        /// <param name="S">Массив номеров базовых систем</param>
        /// <param name="J">Массив номеров интеграторов</param>
        /// <param name="F">Массив номеров дивайдеров</param>
        /// <param name="mu">Массив интенсивностей обслуживания для базовых систем</param>
        /// <param name="Theta">Матрица передачи</param>
        /// <param name="Lambda0">Интенсивность входящего потока</param>
        /// <param name="kappa">Массив число обслуживающих приборов в каждой базовой системе обслуживания>
        public DescriptionOFJQN(int[] S, int[] F, int[] J, double[] mu, int[] kappa, RoutingMatrix Theta, double Lambda0)
        {
            this.S = S;
            this.F = F;
            this.mu = mu;
            this.kappa = kappa;
            this.J = J;
            this.Theta = Theta;
            this.Lambda0 = Lambda0;
        }





        /// <summary>
        /// Создает открытую сеть с делением и слиянием требований, считывая данные из файла
        /// </summary>
        /// <param name="FileName"></param>
        public DescriptionOFJQN(string FileName)
        {

            using (StreamReader sr = new StreamReader(FileName))
            {
                //Индексы server-node
                var temp = sr.ReadLine().Split(';');
                S = new int[temp.Length];
                for (int i = 0; i < temp.Length; i++)
                {
                    S[i] = int.Parse(temp[i], System.Globalization.CultureInfo.CreateSpecificCulture("RU-ru"));
                }


                //Индексы fork-node
                temp = sr.ReadLine().Split(';');
                if (temp[0].Length != 0)
                {
                    F = new int[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        F[i] = int.Parse(temp[i], System.Globalization.CultureInfo.CreateSpecificCulture("RU-ru"));
                    }
                }
                else
                {
                    F = new int[0];
                }



                //Индексы join-node
                temp = sr.ReadLine().Split(';');
                if (temp[0].Length != 0)
                {
                    J = new int[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        J[i] = int.Parse(temp[i], System.Globalization.CultureInfo.CreateSpecificCulture("RU-ru"));
                    }
                }
                else
                {
                    J = new int[0];
                }



                Theta = new RoutingMatrix(S.Length + J.Length + F.Length + 1, F.Length + 1);

                //Маршрутная матрица
                for (int k = 0; k < Theta.CountForker; k++)
                {
                    for (int i = 0; i < Theta.Dimention; i++)
                    {

                        temp = sr.ReadLine().Split(';');
                        for (int j = 0; j < Theta.Dimention; j++)
                        {

                            Theta[k, i, j] = double.Parse(temp[j], System.Globalization.CultureInfo.CreateSpecificCulture("RU-ru"));
                        }
                    }
                }

                //Параметры систем массового обслуживания
                //Интенсивности обслуживания 
                temp = sr.ReadLine().Split(';');
                mu = new double[S.Length];
                for (int i = 0; i < mu.Length; i++)
                {
                    mu[i] = double.Parse(temp[i], System.Globalization.CultureInfo.CreateSpecificCulture("RU-ru"));
                }

                //Число обслуживающих приборов в каждой системе обслуживания
                temp = sr.ReadLine().Split(';');
                kappa = new int[S.Length];
                for (int i = 0; i < mu.Length; i++)
                {
                    kappa[i] = int.Parse(temp[i], System.Globalization.CultureInfo.CreateSpecificCulture("RU-ru"));
                }

                //Интенсивность входящего потока 
                Lambda0 = double.Parse(sr.ReadLine());


            }



        }


        public override string ToString()
        {
            StringBuilder str = new StringBuilder(string.Empty);
            str.AppendLine("Парметры сети");

            str.AppendLine("S = ");
            for (int i = 0; i < S.Length; i++)
            {
                str.AppendFormat("{0}  ", S[i]);
            }
            str.AppendLine();

            str.AppendLine("F = ");
            for (int i = 0; i < F.Length; i++)
            {
                str.AppendFormat("{0}  ", F[i]);

            }
            str.AppendLine();

            str.AppendLine("J = ");
            for (int i = 0; i < J.Length; i++)
            {
                str.AppendFormat("{0}  ", J[i]);
            }
            str.AppendLine();



     


            str.AppendLine("Theta = ");
            for (int k = 0; k < Theta.CountForker; k++)
            {
                str.AppendFormat("Theta{0} = \n", k);
                for (int i = 0; i < Theta.Dimention; i++)
                {
                    for (int j = 0; j < Theta.Dimention; j++)
                    {
                        str.AppendFormat("{0:f2} ", Theta[k, i, j]);
                    }
                    str.AppendLine();
                }

            }
            //Интесивности обслуживания для одного прибора в системе
            str.AppendLine("mu = ");
            foreach (var item in mu)
            {
                str.AppendFormat("{0:f2}  ", item);
            }
            str.AppendLine();

            //Число обслуживающих приборов в системах
            str.AppendLine("kappa = ");
            foreach (var item in kappa)
            {
                str.AppendFormat("{0:f2}  ", item);
            }
            str.AppendLine();

            //Интенсивность входящего потока
            str.AppendLine("lambda0 = ");
            str.AppendFormat("{0:f2}", Lambda0);


            str.AppendLine();

            return str.ToString();
        }


        /// <summary>
        /// Сохраняет описание сети в виде строки
        /// </summary>
        public string Save()
        {
            StringBuilder str = new StringBuilder(string.Empty);

            //Базовые системы
            for (int i = 0; i < S.Length-1; i++)
            {
                str.AppendFormat("{0};", S[i]);
            }
            str.AppendFormat("{0}", S[S.Length - 1]);
            str.AppendLine();

            //Дивайдеры 
            for (int i = 0; i < F.Length-1; i++)
            {
                str.AppendFormat("{0};", F[i]);
            }
            str.AppendFormat("{0}", F[F.Length - 1]);
            str.AppendLine();

            //Интеграторы
            for (int i = 0; i < J.Length-1; i++)
            {
                str.AppendFormat("{0};", J[i]);
            }
            str.AppendFormat("{0}", J[J.Length - 1]);
            str.AppendLine();
           


            for (int k = 0; k < Theta.CountForker; k++)
            {
                for (int i = 0; i < Theta.Dimention; i++)
                {
                    for (int j = 0; j < Theta.Dimention-1; j++)
                    {
                        str.AppendFormat("{0:f4};", Theta[k, i, j]);
                    }
                    str.AppendFormat("{0:f4} ", Theta[k, i, Theta.Dimention-1]);

                    str.AppendLine();
                }

            }

            //Интенсивности обслуживания 
            for (int i = 0; i < mu.Length-1; i++)
            {
                str.AppendFormat("{0};", mu[i]);
            }
            str.AppendFormat("{0}", mu[mu.Length - 1]);
            str.AppendLine();

            //Число обслуживающих приборов  
            for (int i = 0; i < kappa.Length-1; i++)
            {
                str.AppendFormat("{0};", kappa[i]);
            }
            str.AppendFormat("{0}", kappa[kappa.Length - 1]);
            str.AppendLine();


            str.AppendFormat("{0:f2}", Lambda0);

            return str.ToString();
        }

    }
}
