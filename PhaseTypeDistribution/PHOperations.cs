using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLAS;

namespace PhaseTypeDistribution
{
    /// <summary>
    /// Реализует основные операциии для случайных величин с фазовыс распределнием
    /// </summary>
    public static class PHOperations
    {
        /// <summary>
        /// Сумма случайных ыеличин с фазовым распределением
        /// </summary>
        /// <param name="randomValues"></param>
        /// <returns></returns>
        public static PhaseTypeVarible Sum(PhaseTypeVarible[] randomValues)
        {
            PhaseTypeVarible res = new PhaseTypeVarible();
            for (int i = 0; i < randomValues.Length; i++)
            {
                res += randomValues[i];
            }

            return res;

        }

        /// <summary>
        /// Смесь случайных величин с фазовым распределенеием
        /// </summary>
        /// <param name="randomVariables">Массив с.в.</param>
        /// <param name="p">Массив с вероятностями</param>
        /// <returns></returns>
        public static PhaseTypeVarible ConvexMixture(PhaseTypeVarible[] randomVariables, double[] p)
        {
            //Число случайных величин 
            int N = randomVariables.Length;
            if (N != p.Length)
            {
                throw new Exception("Размерности не совпадают");
            }

            //Число фаз в каждой случайной величине
            int[] n = new int[N];
            for (int i = 0; i < N; i++)
            {
                n[i] = randomVariables[i].NumberOfPhases;
            }

            int NumberOfPhases = n.Sum();
            Matrix C = new Matrix(NumberOfPhases, NumberOfPhases);


            //Начинаем заполнение блочной матрицы
            int offseti = 0;
            int offsetj = 0;
            //Берем матрицу и ставим ее на диагональ
            for (int k = 0; k < N; k++)
            {
                for (int i = 0; i < n[k]; i++)
                {
                    for (int j = 0; j < n[k]; j++)
                    {
                        C[offseti + i, offsetj + j] = randomVariables[k].SubGenerator[i, j];
                    }
                }

                //Смещение положения для последующего заполнения 
                offseti += n[k];
                offsetj += n[k];

            }


            double[] gammma = new double[NumberOfPhases];
            //Проход по каждой случайной величине 
            int offset = 0;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < n[i]; j++)
                {
                    gammma[offset] = p[i] * randomVariables[i].InitialDistribution[j];
                    offset++;
                }
            }

            return new PhaseTypeVarible(C, gammma);

        }


        /// <summary>
        /// Максимум из двух случайных величин с фазовым распределением
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static PhaseTypeVarible Max(PhaseTypeVarible A, PhaseTypeVarible B)
        {
            int n = A.NumberOfPhases;
            int m = B.NumberOfPhases;
            var alpha = A.InitialDistribution;
            var betta = B.InitialDistribution;

            var a = -A.SubGenerator * Computation.OnesColumn(n);
            var b = -B.SubGenerator * Computation.OnesColumn(m);

            var alpha0 = 1 - alpha.Sum();
            var betta0 = 1 - betta.Sum();


            //Генерация вектора gamma 
            double[] gamma = new double[n * m + n + m];
            int offset = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    gamma[offset] = alpha[i] * betta[j];
                    offset++;

                }
            }
            for (int i = 0; i < n; i++)
            {
                gamma[offset] = betta0 * alpha[i];
                offset++;
            }
            for (int i = 0; i < m; i++)
            {
                gamma[offset] = alpha0 * betta[i];
                offset++;
            }

            //Закончили генерировать вектор gamma 

            // Computation.KroneckerProduct(Computation.Eye(n), b); 


            Matrix[,] C = {
                { Computation.KroneckerSum(A.SubGenerator, B.SubGenerator), Computation.KroneckerProduct(Computation.Eye(n), b), Computation.KroneckerProduct(a, Computation.Eye(m)) },
                { new Matrix(n, n*m), A.SubGenerator,  new Matrix(n, m) },
                { new Matrix(m, n*m), new Matrix(m, n), B.SubGenerator}

            };

            return new PhaseTypeVarible(new Matrix(C), gamma);
        }

        /// <summary>
        /// Максимум из произвольного числа случайных величин с фазовым распределением
        /// </summary>
        /// <param name="randomVaribles">Массив PH объектов</param>
        /// <returns></returns>
        public static PhaseTypeVarible Max(PhaseTypeVarible[] randomVaribles)
        {

            PhaseTypeVarible C = randomVaribles[0];

            for (int i = 1; i < randomVaribles.Length; i++)
            {
                C = Max(C, randomVaribles[i]);
            }

            return C;
        }

        /// <summary>
        /// Максимум из произвольного числа случайных величин с фазовым распределением
        /// </summary>
        /// <param name="randomVaribles">Массив PH объектов</param>
        /// <returns></returns>
        public static PhaseTypeVarible Max(List<PhaseTypeVarible> randomVaribles)
        {

            PhaseTypeVarible C = randomVaribles[0];

            for (int i = 1; i < randomVaribles.Count; i++)
            {
                C = Max(C, randomVaribles[i]);
            }

            return C;
        }

        /// <summary>
        /// Фазовое представление для экспоненциального распределения 
        /// </summary>
        /// <param name="mu">Параметр распределения</param>
        /// <returns></returns>
        public static PhaseTypeVarible ExpPH(double mu)
        {
            Matrix A = new Matrix(1, 1);
            A[0, 0] = -mu;
            double[] alpha = new double[1];
            alpha[0] = 1;

            return new PhaseTypeVarible(A, alpha);
        }
    }
}
