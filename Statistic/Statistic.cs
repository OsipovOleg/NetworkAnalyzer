using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatisticsLib
{


    /// <summary>
    /// Делегат для функции распределения
    /// </summary>
    /// <param name="x">Аргумент функции распределения </param>
    /// <param name="param">Параметры для функции распределения</param>
    /// <returns></returns>
    public delegate double DistrFunc(double x, params double[] param);

    /// <summary>
    /// Класс, реализующий основные статистические опреации
    /// </summary>
    public static class Statistics
    {


        static void Main(string[] args)
        {
        }


        private const double eps = 1e-6;


        public static void Density(double[] values, out double[] x, out double[] f, int k)
        {
            //Массив выборочной плотности распределения
            f = new double[k + 1];
            x = new double[k + 1]; 
            //Минимальное и максимальное значение выборки
            double min = values.Min();
            double max = values.Max();

            int index;
            double h = (max - min) / k;

            if (Math.Abs(max - min) < eps)
            {
                throw new Exception("Случайная велчина детерминированная или имеет очень малую дисперсию");
            }

            //Подсчет числа значений с.в, попавших в каждый из интервалов
            for (int i = 0; i < values.Length; i++)
            {
                index = (int)((values[i] - min) / h);
                f[index]++;
            }
            //Нормализация плотности распределения
            for (int i = 0; i <= k; i++)
            {
                f[i] = f[i] / (values.Length * h);
                x[i] = min + i * h; 
            }
        }




        /// <summary>
        /// Возвращает статистическую плотность распределения 
        /// </summary>
        /// <param name="values">Выборка </param>
        /// <param name="k">Число интервалов на который будет разбита выборка</param>
        /// <returns>Плотность распределения размерности k+1</returns>
        public static double[] Density(double[] values, int k)
        {
            //Массив выборочной плотности распределения
            double[] f = new double[k + 1];
            //Минимальное и максимальное значение выборки
            double min = values.Min();
            double max = values.Max();

            int index;
            double h = (max - min) / k;

            if (Math.Abs(max - min) < eps)
            {
                throw new Exception("Случайная велчина детерминированная или имеет очень малую дисперсию");
            }

            //Подсчет числа значений с.в, попавших в каждый из интервалов
            for (int i = 0; i < values.Length; i++)
            {
                index = (int)((values[i] - min) / h);
                f[index]++;
            }
            //Нормализация плотности распределения
            for (int i = 0; i <= k; i++)
            {
                f[i] = f[i] / (values.Length * h);
            }
            return f;

        }
        /// <summary>
        /// Возвращает статистическую функцию распределения 
        /// </summary>
        /// <param name="values">Выборка </param>
        /// <param name="k">Число интервалов на который будет разбита выборка</param>
        /// <returns>Функция распределения размерности k+1</returns>
        public static double[] DistributionFunction(double[] values, int k)
        {
            //Массив выборочной ф.р
            double[] f = new double[k + 1];
            //Минимальное и максимальное значение выборки
            double min = values.Min();
            double max = values.Max();

            int index;
            double h = (max - min) / k;

            if (Math.Abs(max - min) < eps)
            {
                throw new Exception("Случайная велчина детерминированная или имеет очень малую дисперсию");
            }

            //Подсчет числа значений с.в, попавших в каждый из интервалов
            for (int i = 0; i < values.Length; i++)
            {
                index = (int)((values[i] - min) / h);
                f[index]++;
            }
            //Нормализация ф.р распределения
            for (int i = 0; i <= k; i++)
            {
                f[i] = f[i] / (values.Length);
            }
            //Суммирование частот 
            for (int i = 1; i < f.Length; i++)
            {
                f[i] += f[i - 1];
            }
            return f;

        }



        /// <summary>
        /// Возвращает статистическую функцию распределения 
        /// </summary>
        /// <param name="values">Выборка </param>
        /// <param name="k">Число интервалов на который будет разбита выборка</param>
        /// <returns>Функция распределения размерности k+1</returns>
        public static void DistributionFunction(double[] values, out double[] x, out double[] f,  int k)
        {
            //Массив выборочной ф.р
            f = new double[k + 1];
            x = new double[k + 1]; 
            //Минимальное и максимальное значение выборки
            double min = values.Min();
            double max = values.Max();

            int index;
            double h = (max - min) / k;

            if (Math.Abs(max - min) < eps)
            {
                throw new Exception("Случайная велчина детерминированная или имеет очень малую дисперсию");
            }

            //Подсчет числа значений с.в, попавших в каждый из интервалов
            for (int i = 0; i < values.Length; i++)
            {
                index = (int)((values[i] - min) / h);
                f[index]++;
            }
            //Нормализация ф.р распределения
            for (int i = 0; i <= k; i++)
            {
                f[i] = f[i] / (values.Length);
            }
            //Суммирование частот 
            for (int i = 1; i < f.Length; i++)
            {
                f[i] += f[i - 1];
                x[i] = i * h; 
            }

        }


        /// <summary>
        /// Возвращает статистическую функцию распределения с автоматичечким количеством интервалов 
        /// Идеально подходит для  теста Колмогорова-Смирнова
        /// </summary>
        /// <param name="values">Выборка </param>
        public static void DistributionFunction(List<double> values, out double[] x, out double[] f)
        {
            //Объем выборки
            int N = values.Count; 
            //Массив выборочной ф.р
            f = new double[N];
            x = new double[N]; 
            for (int i = 0; i < values.Count; i++)
            {
                x[i] = values[i]; 
            }

            Array.Sort(x); 
            for (int i = 0; i < N; i++)
            {
                f[i] = (i + 1) / (double)N; 
                
            }
        }

        /// <summary>
        /// Статистика Колмогорова-Смирнова
        /// </summary>
        /// <param name="f">Статистическая функция распределения</param>
        /// <param name="a">Нижняя граница участка построения ф.р.</param>
        /// <param name="b">Верхняя граница участка построения ф.р.</param>
        /// <param name="n">Объем выборки по которой построена ф.р.</param>
        /// <param name="df">Теоретическая функция распределения </param>
        /// <param name="param">Параметры для теоретической ф.р. если имеются</param>
        /// <returns></returns>
        public static double KolmogovStatistic(double[] f, double a, double b, DistrFunc df, params double[] param)
        {
            int k = f.Length;
            double[] D = new double[k];
            double h = (b - a) / (k - 1);
            if (Math.Abs(b - a) < eps)
            {
                throw new Exception("Случайная велчина детерминированная или имеет очень малую дисперсию");
            }

            double x = a;
            for (int i = 1; i < f.Length - 1; i++)
            {
                D[i] = Math.Abs(f[i] - df(x, param));
                x += h;
            }
            return Math.Sqrt(k) * D.Max();
        }

        /// <summary>
        /// Значения уровня доверия для вероятности 0,15
        /// </summary>
        public static readonly double KS015 = 1.379;
        /// <summary>
        /// Значения уровня доверия для вероятности 0,1
        /// </summary>
        public static readonly double KS01 = 1.2238;
        /// <summary>
        /// Значения уровня доверия для вероятности 0,05
        /// </summary>
        public static readonly double KS005 = 1.4802;
        /// <summary>
        /// Значения уровня доверия для вероятности 0,01
        /// </summary>
        public static readonly double KS001 = 1.6276;

    }



}