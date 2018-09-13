using System;
using System.Linq;
using BLAS;

namespace PhaseTypeDistribution
{
    /// <summary>
    /// Описывает случайную величину с фазовым расределением
    /// </summary>
    public class PhaseTypeVarible
    {
        /// <summary>
        /// Генератор для фазового распределения 
        /// </summary>
        private Matrix subgenerator;
        /// <summary>
        /// Начальное распределение
        /// </summary>
        private double[] initialProbabilities;

        /// <summary>
        /// Генератор фазового распределения
        /// </summary>
        public Matrix SubGenerator
        {
            get
            {
                return subgenerator;
            }
            private set
            {
                subgenerator = value;
            }
        }

        /// <summary>
        /// Начальное распределение для фазового распределения
        /// </summary>
        public double[] InitialDistribution
        {
            get
            {
                return initialProbabilities;
            }
            private set
            {
                initialProbabilities = value;
            }
        }


        /// <summary>
        /// Число фаз в фазовом распределении
        /// </summary>
        public int NumberOfPhases
        {
            get
            {
                return subgenerator.CountColumn;
            }
        }



        /// <summary>
        /// Создает фазовое распределение по его матрице и начальному распределению
        /// </summary>
        /// <param name="Generator">Генератор фазового распределения</param>
        /// <param name="InitialProbabilities">Начальое распределение</param>
        public PhaseTypeVarible(Matrix Generator, double[] InitialProbabilities)
        {
            if (Generator.isSquare && (Generator.CountColumn == InitialProbabilities.Length))
            {
                this.subgenerator = Generator;
                this.initialProbabilities = InitialProbabilities;
            }
            else
            {
                throw new Exception("Размерности не согласуются");
            }
        }

        /// <summary>
        /// Создает фазовое распределение
        /// </summary>
        public PhaseTypeVarible()
        {
            this.SubGenerator = new Matrix(0, 0);
            this.InitialDistribution = new double[0];
        }


        /// <summary>
        /// Фазовое распределение для суммы двух случайных величин с фазовыми распределениями
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static PhaseTypeVarible operator +(PhaseTypeVarible A, PhaseTypeVarible B)
        {
            //Матрицы квадратные 
            int n = A.subgenerator.CountColumn;
            int m = B.subgenerator.CountColumn;


            Matrix zeros = new Matrix(m, n);
            Matrix alphabetta = new Matrix(n, m);
            var alpha = A.InitialDistribution;
            var betta = B.InitialDistribution;

            for (int i = 0; i < alpha.Length; i++)
            {
                for (int j = 0; j < betta.Length; j++)
                {
                    alphabetta[i, j] = alpha[i] * betta[j];
                }
            }
            //Создаем новую матрицу фазового распределения по правилу 
            Matrix gen = new Matrix(new Matrix[,]
            { {A.subgenerator, alphabetta },
            {zeros, B.subgenerator} });

            double[] gamma = new double[n + m];
            for (int i = 0; i < n; i++)
            {
                gamma[i] = alpha[i];
            }
            double alpha0 = 1 - alpha.Sum();
            for (int i = n; i < n + m; i++)
            {
                gamma[i] = alpha0 * betta[i - n];
            }

            return new PhaseTypeVarible(gen, gamma);

        }

        /// <summary>
        /// Математическое ожидание для случайной величины 
        /// </summary>
        /// <returns></returns>
        public double ExpectedValue()
        {
            Matrix alpha = new Matrix(1, this.NumberOfPhases);
            for (int i = 0; i < this.NumberOfPhases; i++)
            {
                alpha[0, i] = this.InitialDistribution[i];
            }
            return (-alpha * (this.SubGenerator.Inv()) * Computation.OnesColumn(this.NumberOfPhases))[0, 0];
        }


        public override string ToString()
        {
            Matrix alpha = new Matrix(1, this.NumberOfPhases);
            for (int i = 0; i < this.NumberOfPhases; i++)
            {
                alpha[0, i] = this.InitialDistribution[i];
            }

            return SubGenerator.ToString() + Environment.NewLine + Environment.NewLine + alpha.ToString();
        }

        /// <summary>
        /// Дисперсия случайной величины
        /// </summary>
        /// <returns></returns>
        public double Variance()
        {
            Matrix GeneratorInv = this.SubGenerator.Inv(); 

            Matrix alpha = new Matrix(1, this.NumberOfPhases);
            for (int i = 0; i < this.NumberOfPhases; i++)
            {
                alpha[0, i] = this.InitialDistribution[i];
            }
            return (2 * alpha * (this.SubGenerator * this.SubGenerator).Inv() * 
                Computation.OnesColumn(this.NumberOfPhases) -
                (alpha * GeneratorInv * Computation.OnesColumn(this.NumberOfPhases)) * 
                (alpha * GeneratorInv * Computation.OnesColumn(this.NumberOfPhases)))[0, 0];
        }
    }
}
