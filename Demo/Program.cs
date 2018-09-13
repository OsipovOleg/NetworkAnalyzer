using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using BLAS;
using NetworkSimulator;
using ExactNetworkAnalysis;
using System.Threading;
using System.Globalization;
using NetworkDescriptions;
using Accord.Math;
using StatisticsLib;
using PhaseTypeDistribution;
using LibOptimization.Optimization;
using LibOptimization.Util;


namespace Analysis
{
    /// <summary>
    /// Класс с константами
    /// </summary>
    public static class Constants
    {
        public const double EPS = 0.0000000001;
        public const double WeightEPS = EPS * 100;

    }


    public static class ServiceRate
    {
        /// <summary>
        /// Перерасчитывает интенсивность входящего потока
        /// </summary>
        /// <returns>The rate.</returns>
        /// <param name="mu">Mu.</param>
        /// <param name="lambdaw">Lambdaw.</param>
        /// <param name="w">The width.</param>
        public static double UpdateRate(double mu, double lambdaw, double w)
        {
            return 0; 
        }
    }


    
    public class ResponseTimeFunction : LibOptimization.Optimization.absObjectiveFunction
    {
        private double _maxEval = 0.0;
        private bool _oneShot = true;
        private int dim = 0;
        public bool print
        {
            get;
            set;
        }
        //Описание сети обслуживания
        private DescriptionOFJQN Description;
        //Список смежных систем
        private List<List<int>> A;
        //Маршрутная матрица с весами
        private RoutingMatrix WeigtedRoute;


        //Маршрутная без некоторых веток Срезали некоторые ветки по которым не уидут потоки их вес слишком мал
        private RoutingMatrix CutRoute;

        /// <summary>
        /// Число смежных систем для каждого дивайдера 
        /// </summary>
        /// <returns>The dim.</returns>
        public int[] WeightsDim()
        {
            var res = new int[A.Count]; 
            for (int i = 0; i < A.Count; i++)
            {
                res[i] = A[i].Count; 
            }
            return res;
        }

        /// <summary>
        /// Начальные интенсивности потоков
        /// </summary>
        private double[] MU;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dim"></param>
        public ResponseTimeFunction(DescriptionOFJQN Description, bool print)
        {

            this.Description = Description; 
            this.print = print; 

            {
                //Описание сети обслуживания
                Console.WriteLine(Description); 
            }


            //Список всех систем смежных с каждым дивайдером
            this.A = new List<List<int>>(); 
            for (int k = 0; k < Description.F.Length; k++)
            {
                var temp = Utils.AdjacentNodes(Description.Theta[k + 1], Description.F[k]).ToList(); 
                A.Add(temp); 


                Console.WriteLine("Дивайдер F{0}:", k + 1);
                foreach (var item in temp)
                {
                    Console.Write("{0} ", item); 
                }
                Console.WriteLine(); 
            }

            this.dim = this.WeightsDim().Sum(); 

            //Получаем описание для сети массового осблуживания с деленеим и слиянием требованием

            //Матрица с весами 
            //Копируем матрицу для решения уравнения потоков
            WeigtedRoute = new RoutingMatrix(Description.Theta.Dimention, Description.Theta.CountForker); 
            CutRoute = new RoutingMatrix(Description.Theta.Dimention, Description.Theta.CountForker);
            //Запись в матрицы вероятностей  
            for (int k = 0; k < WeigtedRoute.CountForker; k++)
            {
                int N = Description.Theta.Dimention; 
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        WeigtedRoute[k][i, j] = Description.Theta[k][i, j]; 
                        CutRoute[k][i, j] = Description.Theta[k][i, j]; 
                    }
                }
            }


            MU = new double[Description.mu.Length]; 
            for (int i = 0; i < MU.Length; i++)
            {
                MU[i] = Description.mu[i]; 
            }

        }

        /// <summary>
        /// Считает все мю и ламбда для данной интенсивности входящего потока
        /// Массивы каждый раз создаются заново
        /// </summary>
        /// <returns>The mu.</returns>
        /// <param name="W">W.</param>
        /// <param name="MU">M.</param>
        /// <param name="LAMBDA">LAMBD.</param>
        public void  LambdasMu(double[] W, out double[] ResultMU, out double[] ResultLAMBDA)
        {
            //Изменение матрицы с весами и матрицы с обрезанныами дугами 
            int pos = 0; 
            for (int k = 0; k < A.Count; k++)
            {
                //Делаем задачу безусловной оптимизации
                Double sum = 0; 
                double eps = Constants.EPS;

                //Модифицируем матрицу весов 
                for (int i = 0; i < A[k].Count; i++)
                {
                    if (W[pos] < 0)
                    {
                        throw new Exception("Отрицательный вес"); 
                    }

                    sum += W[pos]; 

                    WeigtedRoute[k + 1][Description.F[k], A[k][i]] = W[pos]; 

                    //Если по дуге идет незначительный вес, то усекаем его 
                    if (W[pos] > Constants.WeightEPS)
                    {
                        CutRoute[k + 1][Description.F[k], A[k][i]] = 1; 
                    }
                    else
                    {
                        CutRoute[k + 1][Description.F[k], A[k][i]] = 0; 
                    }


                    pos++; 
                }

                //Если сумма не равна единице
                if ((sum > 1 + eps) || (sum < 1 - eps))
                {
                    throw new Exception("Сумма не равна единице при подсчете мю и лабмда"); 

                }
            }




            //Вычисление интенсивностей входящего потока в каждую из систем обслуживания
            ResultLAMBDA = InfinityServerOpenForkJoinAnalizator.TotalInputRates(
                InfinityServerOpenForkJoinAnalizator.InputRates(Description.S, Description.F, Description.J,
                    Description.Lambda0, WeigtedRoute)); 


            ResultMU = new double[MU.Length]; 
            for (int i = 0; i < Description.S.Length; i++)
            {
              
                ResultMU[i] = MU[i] - ResultLAMBDA[i]; 
            }
        }

        /// <summary>
        /// eval
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public override double F(List<double> W)
        {
            //Изменение матрицы с весами и матрицы с обрезанныами дугами 
            int pos = 0; 
            for (int k = 0; k < A.Count; k++)
            {
                //Делаем задачу безусловной оптимизации
                Double sum = 0; 
                double eps = Constants.EPS;

                //Модифицируем матрицу весов 
                for (int i = 0; i < A[k].Count; i++)
                {
                    if (W[pos] < 0)
                    {
                        return Double.PositiveInfinity;                         
                    }

                    sum += W[pos]; 

                    WeigtedRoute[k + 1][Description.F[k], A[k][i]] = W[pos]; 

                    //Если по дуге идет незначительный вес, то усекаем его 
                    if (W[pos] > Constants.WeightEPS)
                    {
                        CutRoute[k + 1][Description.F[k], A[k][i]] = 1; 
                    }
                    else
                    {
                        CutRoute[k + 1][Description.F[k], A[k][i]] = 0; 
                    }


                    pos++; 
                }

                //Если сумма не равна единице
                if ((sum > 1 + eps) || (sum < 1 - eps))
                {
                    return double.PositiveInfinity; 
                }
            }
                



            //Вычисление интенсивностей входящего потока в каждую из систем обслуживания
            var Lambda = InfinityServerOpenForkJoinAnalizator.TotalInputRates(
                             InfinityServerOpenForkJoinAnalizator.InputRates(Description.S, Description.F, Description.J,
                                 Description.Lambda0, WeigtedRoute)); 
            if (print)
            {
                for (int i = 0; i < Lambda.Length; i++)
                {
                    Console.WriteLine("Lambda[{0}]= {1:f4}", i + 1, Lambda[i]); 
                }
            }


            var rho = new double[Description.S.Length]; 
            for (int i = 0; i < Description.S.Length; i++)
            {
                //Увеличиваем число обслуживающих приборов до "бесконечности"
                Description.kappa[i] = 1000000;

                //Считаем ro так как если бы это была одноприборная сеть обслуживания
                rho[i] = Lambda[i] / Description.mu[i]; 

                //Модифицируем интенсинвность обслуживания фрагментов одним прибором

                Description.mu[i] = MU[i] - Lambda[i]; 
                


                if (print)
                {
                    Console.WriteLine("mu[{0}] = {1:f4}, ro[{0}] = {2:f4}", i + 1, Description.mu[i], rho[i]);
                }
                if (Description.mu[i] <= 0.01)
                {
                    if (print)
                    {
                        Console.WriteLine("Отсутствие стационарного режима");
                    }
                    return Double.PositiveInfinity; 
                }
            }


            if (print)
            {
                Console.WriteLine("Аналитическое моделирование");
            }



            //Модификация описания для сети обслуживани (УБИРАЕМ ВЕТКИ С НУЛЕВЫМИ ВЕСАМИ)
            var temp = Description.Theta; 
            Description.Theta = CutRoute; 

            var ph = InfinityServerOpenForkJoinAnalizator.ResponseTimeDistribution(Description);
            //Возвращаем матрицу на место 
            Description.Theta = temp; 

            if (print)
            {
                Console.WriteLine("Число фаз {0}", ph.NumberOfPhases);
            }

            double ApproximationRT = ph.ExpectedValue(); 
            if (print)
            {
                Console.WriteLine("E(tau) = {0:f4}", ApproximationRT);
            }
            //Console.WriteLine("Var(tau) = {0:f4}", ph.Variance());

            if (print)
            {
                Console.WriteLine("Press any key ..."); 
                Console.ReadKey();
            }

            if (print)
            {
                Program.PrintVector(W.ToArray());
                Console.WriteLine(ApproximationRT);
            }

            return ApproximationRT; 
        }

        public override List<double> Gradient(List<double> aa)
        {
            return null;
        }

        public override List<List<double>> Hessian(List<double> aa)
        {
            return null;
        }



        public override int NumberOfVariable()
        {
            return dim;
        }
    }


    /// <summary>
    /// Location state.
    /// </summary>
    public class LocationState
    {
        public  int[] Set;

        /// <summary>
        /// Конструктор со встроенной сортировкой
        /// </summary>
        /// <param name="array">Array.</param>
        public LocationState(int[] array)
        {
            this.Set = array; 
            Array.Sort(Set); 
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode(); 
            
        }

        public override bool Equals(object obj)
        {
            return this.ToString() == obj.ToString(); 
        }

        public override string ToString()
        {
            string res = string.Empty; 
            for (int i = 0; i < Set.Length; i++)
            {
                res += Set[i].ToString();
                if (i < Set.Length - 1)
                {
                    res += ":"; 
                }
                    
            }
            return res; 
        }
    }

    public static class Program
    {
        /// <summary>
        /// Имя файла с параметрами сети обслуживания
        /// </summary>
        static string file = "OneServerExample1";
        /// <summary>
        /// Максимальное значения абциссы для гистограммы и функции распределения 
        /// </summary>
        static double xmax = 30;

        /// <summary>
        /// Печать вектора 
        /// </summary>
        /// <param name="w">The width.</param>
        public static void PrintVector(double[] w)
        {
            for (int i = 0; i < w.Length; i++)
            {
                Console.Write("{0:f4}  ", w[i]); 
            }
        }


       



        /// <summary>
        /// Генерирует  распределение Весов без выделения лишней памяти
        /// </summary>
        /// <returns>The weights distr.</returns>
        /// <param name="dims">Dims.</param>
        /// <param name="W">W.</param>
        /// <param name="r">The red component.</param>
        public static void RandomWeightsDistr(int[] dims, double[] W, Random r)
        {

            int pos = 0; 
            for (int i = 0; i < dims.Length; i++)
            {
                var temp = RandomVector(dims[i], r); 
                for (int j = 0; j < temp.Length; j++)
                {
                    W[pos] = temp[j]; 
                    pos++; 

                }
            }
        }




        /// <summary>
        /// Генерирует  распределение Весов без выделения лишней памяти
        /// </summary>
        /// <returns>The weights distr.</returns>
        /// <param name="dims">Dims.</param>
        /// <param name="W">W.</param>
        /// <param name="r">The red component.</param>
        public static void RandomWeightsDistr(int[] dims, double[] W, double[] eps, Random r)
        {

            int pos = 0; 


            for (int i = 0; i < dims.Length; i++)
            {
                //Генерим вектор для одного дивайдера
                double[] predw = new double[dims[i]];
                for (int k = 0; k < predw.Length; k++)
                {
                    //Копируем вектор
                    predw[k] = W[pos + k]; 
                }

                //Модификация подвектора
                var temp = RandomEpsilon(predw, r, eps[i]);  
                for (int j = 0; j < temp.Length; j++)
                {
                    W[pos] = temp[j]; 
                    pos++; 

                }
            }

        }

        /// <summary>
        /// Генерация рандомного вектора с суммой единица 
        /// </summary>
        /// <returns>The vector.</returns>
        /// <param name="N">N.</param>
        /// <param name="r">The red component.</param>
        public static double[] RandomVector(int N, Random r)
        {
            
            double[] res = new double[N]; 
            for (int i = 0; i < N; i++)
            {
                res[i] = r.NextDouble(); 
            }
            double sum = res.Sum(); 
            for (int i = 0; i < N; i++)
            {
                res[i] = res[i] / sum; 
            }

            return res; 
        }

        /// <summary>
        /// Модифицирует вектор добавляя случаын шум в некоторой области
        /// пропорциональной размерку каждой компоненте и к-ту eps
        /// </summary>
        /// <returns>The epsilon.</returns>
        /// <param name="Vector">Vector.</param>
        /// <param name="r">The red component.</param>
        /// <param name="eps">Eps.</param>
        public static double[] RandomEpsilon(double[] InitialVector, Random r, double eps)
        {
            

            double[] Vector = new double[InitialVector.Length]; 

            for (int i = 0; i < Vector.Length; i++)
            {
                //Копируем вектор и меняем копию 
                Vector[i] = InitialVector[i]; 
                Vector[i] = Math.Max(Vector[i] + (r.NextDouble() - 0.5) * 2 * eps, Constants.EPS); 
                if (Vector[i] < 0)
                {
                    Console.WriteLine("Вектор оказался меньше нуля"); 
                    throw new Exception(); 
                }
            }
            double sum = Vector.Sum(); 
            for (int i = 0; i < Vector.Length; i++)
            {
                Vector[i] = Vector[i] / sum; 
            }

            return Vector; 
                
        }


     

        /// <summary>
        /// Точка внутри отрезка 
        /// не средняя
        /// </summary>
        /// <param name="X">X.</param>
        /// <param name="Y">Y.</param>
        /// <param name="Midle">Midle.</param>
        /// <param name="alphax">Alphax.</param>
        /// <param name="alphay">Alphay.</param>
        public static void MidlePoint(double[] X, double[] Y, double[] Midle, double alphax, double alphay)
        {
            for (int i = 0; i < X.Length; i++)
            {
                Midle[i] = X[i] * alphax + Y[i] * alphay; 
            }
            
        }

        /// <summary>
        /// Линейная аппроксимация значения функции в некоторой точке 
        /// Для упорядоченной на входе таблицы значений
        /// </summary>
        /// <returns>The approximation.</returns>
        /// <param name="value">Value.</param>
        /// <param name="X">X.</param>
        /// <param name="Y">Y.</param>
        public static double LinearApproximation(double value, double[] X, double[] Y)
        {
            
            int n;
            for (n = 0; n < X.Length; n++)
            {
                if (value < X[n])
                {
                    break; 
                }
            }

            if (n == 0)
            {
                return Y[0]; 
            }
            if (n == X.Length)
            {
                return Y[n - 1]; 
            }

            return (Y[n] - Y[n - 1]) / (X[n] - X[n - 1]) * (value - X[n - 1]) + Y[n - 1]; 
        }









        /// <summary>
        /// Сохраняет таблицу значений функции в файл 
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="f">F.</param>
        /// <param name="path">Path.</param>
        public static void SaveFunction(double[] x, double[] f, string ColumnNames,
                                        string path, int step, double xmax)
        {
            //Запись в файл 
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(ColumnNames); 
                for (int i = 0; i < x.Length; i = i + step)
                {
                    if (x[i] < xmax)
                    {
                        sw.WriteLine("{0:f4} {1:f4}", x[i], f[i]); 
                    }
                    else
                    {
                        return; 
                    }

                }
            }
            
        }



        /// <summary>
        /// Функция распределения для величины с фазовым распределением
        /// </summary>
        /// <returns>The type cum func.</returns>
        /// <param name="ph">Ph.</param>
        /// <param name="t">T.</param>
        public  static double PhaseTypePDF(PhaseTypeVarible ph, double t)
        {
            Matrix alpha = new Matrix(ph.InitialDistribution); 
            Matrix ones = Computation.OnesColumn(ph.NumberOfPhases); 
            Matrix a = -ph.SubGenerator * ones; 

            return (alpha * Expm(ph.SubGenerator, t) * a)[0, 0];         

        }


        public  static double PhaseTypeCDF(PhaseTypeVarible ph, double t)
        {
            Matrix alpha = new Matrix(ph.InitialDistribution); 
            Matrix ones = Computation.OnesColumn(ph.NumberOfPhases); 

            return 1 - (alpha * Expm(ph.SubGenerator, t) * ones)[0, 0];         

        }


        /// <summary>
        /// Вычисляет матричную экспоненту по методу Паде
        /// </summary>
        /// <param name="A">A.</param>
        /// <param name="t">T.</param>
        public static Matrix Expm(Matrix A, double t)
        {
            Matrix AA = A * A; 
            Matrix AAA = AA * A; 

            Matrix I = Computation.Eye(A.CountColumn); 
            return (I
            + (t / 2) * A
            + (t * t / 10) * AA
            + (t * t * t / 120) * AAA)

            * (I
            - (t / 2) * A
            + (t * t / 10) * AA
            - (t * t * t / 120) * AAA).Inv(); 
        }





        /// <summary>
        /// Анализ однопориборной сети обслуживания и проверка на имитационной модели
        /// </summary>
        public static bool FiniteAnalysis(Random rand, double FinishTime, 
                                          double lambda, 
                                          out double SimulationRT, out double ApproximationRT, out double[] rho)
        {
            //Предустановленные значения
            SimulationRT = 0; 
            ApproximationRT = 0; 


            Console.Clear(); 



            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";


            //Получаем описание для сети массового осблуживания с деленеим и слиянием требованием
            var Description = new DescriptionOFJQN(filename); 
            //Console.WriteLine(Description); 

            //Меняем интенсивность входящего потока требований согласно поднанной на вход в функцию
            Description.Lambda0 = lambda; 


            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("================================================================================");
            Console.WriteLine("Lambda0 = {0:f4}", lambda); 
            Console.WriteLine("Имитационное моделирование одноприборной сети обслуживания");

            //Создаем модель по её описанию 
            NetworkModel OriginalModel = OFJQN.CreateNetworkModel(Description, rand); 
            //Запускаем модель
            OriginalModel.Run(FinishTime); 
            //Собираем статистику по модели
            OriginalModel.Analysis(out SimulationRT); 

           

            Console.WriteLine("Выполнение приближённого метода анализа");
            //Вычисление интенсивностей входящего потока в каждую из систем обслуживания
            var Lambda = InfinityServerOpenForkJoinAnalizator.TotalInputRates(
                             InfinityServerOpenForkJoinAnalizator.InputRates(Description.S, Description.F, Description.J,
                                 Description.Lambda0, Description.Theta)); 

            //Выполняется преобразование интенсивностей и увеличение числа приборов
            Console.WriteLine("Выполняем переход к бесконечноприборной сети с интенсивностями обслуживания:"); 
            //Меняем описание для сети

            rho = new double[Description.S.Length]; 
            for (int i = 0; i < Description.S.Length; i++)
            {
                //Увеличиваем число обслуживающих приборов до "бесконечности"
                Description.kappa[i] = 1000000;

                //Считаем ro так как если бы это была одноприборная сеть обслуживания
                rho[i] = Lambda[i] / Description.mu[i]; 

                //Модифицируем интенсинвность обслуживания фрагментов одним прибором
                Description.mu[i] = Description.mu[i] - Lambda[i]; 

                Console.WriteLine("mu[{0}] = {1:f4}, ro[{0}] = {2:f4}", i + 1, Description.mu[i], rho[i]);
                if (Description.mu[i] <= 0.01)
                {
                    Console.WriteLine("Отсутствие стационарного режима");
                    return false; 
                }
            }

        

            Console.WriteLine("Аналитическое моделирование"); 
            var ph = InfinityServerOpenForkJoinAnalizator.ResponseTimeDistribution(Description);
            Console.WriteLine("Число фаз {0}", ph.NumberOfPhases); 
            ApproximationRT = ph.ExpectedValue(); 
            Console.WriteLine("E(tau) = {0:f4}", ApproximationRT);
            //Console.WriteLine("Var(tau) = {0:f4}", ph.Variance());

            Console.WriteLine("Имитационное моделирование бесконечноприборной - проверка");
            NetworkModel TransformedModel = OFJQN.CreateNetworkModel(Description, rand); 
            TransformedModel.Run(FinishTime); 
            double temp; 
            TransformedModel.Analysis(out temp); 



            Console.WriteLine("Press any key ..."); 
            Console.ReadKey();

           

            return true; 



        }


        /// <summary>
        /// Моделирует сеть, зависимую от нагрузки выдает графики 
        /// </summary>
        public static void Example1()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("ru-RU");


            Random rand = new Random(); 
            double FinishTime = 1000000;  

            //Console.Clear(); 
            //CreateFile(5); 

            double LambdaMin = 0.5; 
            double LambdaMax = 4.4; 
            double Lambda = 0; 
            double h = 0.2; 


            //Списки с результатами моделирования
            List<double> ListLambda = new List<double>(); 
            List<double> ListApproximationRT = new List<double>(); 
            List<double> ListSimulationRT = new List<double>(); 
            List<double[]> ListRho = new List<double[]>(); 


            bool stationary = true; 
            //Пока существует стационарный режим и не перешли границу по LambdaMax

            Lambda = LambdaMin; 
            do
            {
                double ApproximationRT = 0; 
                double SimulationRT = 0; 
                double[] rho; 


                if (!FiniteAnalysis(rand, FinishTime, Lambda, out SimulationRT, out ApproximationRT, out rho))
                {
                    stationary = false; 
                }
                else
                {
                    ListLambda.Add(Lambda); 
                    ListApproximationRT.Add(ApproximationRT);
                    ListSimulationRT.Add(SimulationRT); 
                    ListRho.Add(rho); 
                }


                Lambda += h; 



            }
            while((stationary) && (Lambda < LambdaMax));




            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

            using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Results/" + file + ".csv"))
            {
                sw.Write("Lambda App Sim"); 
                for (int j = 0; j < ListRho[0].Length; j++)
                {
                    sw.Write(" rho{0}", j + 1); 
                }
                sw.WriteLine(); 

                for (int i = 0; i < ListLambda.Count(); i++)
                {
                    sw.Write("{0:f4} {1:f4} {2:f4}", ListLambda[i], ListApproximationRT[i], ListSimulationRT[i]); 
                    for (int j = 0; j < ListRho[i].Length; j++)
                    {
                        sw.Write(" {0:f4}", ListRho[i][j]); 
                    }
                    sw.WriteLine(); 
                }

            }




            Console.WriteLine("Программа завершила свою работу");
            Console.ReadKey(); 


        }












        /// <summary>
        /// Вычисление стационарных вероятностей для каждой базовой системы обслуживания в сети
        ///  Бесконечноприборные системы обслуживания 
        /// </summary>
        public static void Example2()
        {

            Console.WriteLine("=== Example2 ===");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("ru-RU");


            Random rand = new Random(); 
            double FinishTime = 10000000;  
            double lambda = 1;



            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";


            //Получаем описание для сети массового осблуживания с деленеим и слиянием требованием
            var Description = new DescriptionOFJQN(filename); 
            //Console.WriteLine(Description); 

            //Меняем интенсивность входящего потока требований согласно поднанной на вход в функцию
            Description.Lambda0 = lambda; 





            //Вычисление интенсивностей входящего потока в каждую из систем обслуживания
            var Lambda = InfinityServerOpenForkJoinAnalizator.TotalInputRates(
                             InfinityServerOpenForkJoinAnalizator.InputRates(Description.S, Description.F, Description.J,
                                 Description.Lambda0, Description.Theta)); 

            double[] rho = new double[Description.S.Length]; 
            for (int i = 0; i < Description.S.Length; i++)
            {
                //Увеличиваем число обслуживающих приборов до "бесконечности"
                Description.kappa[i] = 100000;

                //Считаем ro так как если бы это была одноприборная сеть обслуживания
                rho[i] = Lambda[i] / Description.mu[i]; 

                Console.WriteLine("mu[{0}] = {1:f4}, ro[{0}] = {2:f4}", i + 1, Description.mu[i], rho[i]);
            }


            //Создаем модель по её описанию 
            NetworkModel Model = OFJQN.CreateNetworkModel(Description, rand); 
            Model.Run(FinishTime); 

            for (int i = 0; i < Description.S.Length; i++)
            {
                ServiceNode s = Model.Nodes[Description.S[i]] as ServiceNode; 
                for (int j = 0; j < 10; j++)
                {
                    Console.WriteLine("P{0}({1})= {2:f4}", i, j, (s as ServiceNode).StateProbabilities[j] / FinishTime); 
                }
                Console.WriteLine(); 
            }

            Console.WriteLine("Программа завершила свою работу");
            Console.ReadKey(); 
        }



        /// <summary>
        /// Функция распределения для критерия Колмогорова-Смирнова 
        /// </summary>
        public static void Example3()
        {
            int step = 100; 

            int intervals = 50; 

            bool cdf = true; 

            if (cdf)
            {
                Console.WriteLine("Построение выборочной функции распределения"); 
            }
            else
            {
                Console.WriteLine("Построение выборочное плотности распределения"); 
            }

            //Интенсивность входящего потока
            double lambda = 1; 
            double FinishTime = 10000; 
            Random rand = new Random(); 

            //Имитационное моделирование одноприборное 
            double[] Fsim, Xsim; 
            //Приближенный аналитический результат в тех же точках 
            double[] Fappr, Xappr; 
            //Имитационное моделирование для приближенной модели
            double[] Fcheck, Xcheck; 




            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";


            //Получаем описание для сети массового осблуживания с деленеим и слиянием требованием
            var Description = new DescriptionOFJQN(filename); 
            //Console.WriteLine(Description); 

            //Меняем интенсивность входящего потока требований согласно поднанной на вход в функцию
            Description.Lambda0 = lambda; 


            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("================================================================================");
            Console.WriteLine("Lambda0 = {0:f4}", lambda); 
            Console.WriteLine("Имитационное моделирование одноприборной сети обслуживания");

            //Создаем модель по её описанию 
            NetworkModel OriginalModel = OFJQN.CreateNetworkModel(Description, rand); 
            //Запускаем модель
            OriginalModel.Run(FinishTime); 



            //Построение функции распределения
            if (cdf)
            {
                Statistics.DistributionFunction((OriginalModel.Nodes[0] as SourceNode).ResponseTimes, 
                    out Xsim, out Fsim); 

            }
            else
            {
                Statistics.Density((OriginalModel.Nodes[0] as SourceNode).ResponseTimes.ToArray(), 
                    out Xsim, out Fsim, intervals); 
            }


            Console.WriteLine("Выполнение приближённого метода анализа");
            //Вычисление интенсивностей входящего потока в каждую из систем обслуживания
            var Lambda = InfinityServerOpenForkJoinAnalizator.TotalInputRates(
                             InfinityServerOpenForkJoinAnalizator.InputRates(Description.S, Description.F, Description.J,
                                 Description.Lambda0, Description.Theta)); 

            //Выполняется преобразование интенсивностей и увеличение числа приборов
            Console.WriteLine("Выполняем переход к бесконечноприборной сети с интенсивностями обслуживания:"); 
            //Меняем описание для сети

            double[] rho = new double[Description.S.Length]; 
            for (int i = 0; i < Description.S.Length; i++)
            {
                //Увеличиваем число обслуживающих приборов до "бесконечности"
                Description.kappa[i] = 1000000;

                //Считаем ro так как если бы это была одноприборная сеть обслуживания
                rho[i] = Lambda[i] / Description.mu[i]; 

                //Модифицируем интенсинвность обслуживания фрагментов одним прибором
                Description.mu[i] = Description.mu[i] - Lambda[i]; 

                Console.WriteLine("mu[{0}] = {1:f4}, ro[{0}] = {2:f4}", i + 1, Description.mu[i], rho[i]);
            }



            Console.WriteLine("Аналитическое моделирование"); 
            var ph = InfinityServerOpenForkJoinAnalizator.ResponseTimeDistribution(Description);


            //////////////////////////////////////////////////////
            //Аналитическая приближенная функция распределения; 
            Xappr = Xsim; 
            Fappr = new double[Xappr.Length]; 
            for (int i = 0; i < Xappr.Length; i++)
            {
                if (cdf)
                {
                    Fappr[i] = PhaseTypeCDF(ph, Xappr[i]); 
                }
                else
                {
                    Fappr[i] = PhaseTypePDF(ph, Xappr[i]); 

                }
            }
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////

            Console.WriteLine("Имитационное моделирование бесконечноприборной - проверка");
            NetworkModel TransformedModel = OFJQN.CreateNetworkModel(Description, rand); 
            TransformedModel.Run(FinishTime); 

            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //Построение функции распределения
            if (cdf)
            {
                Statistics.DistributionFunction((TransformedModel.Nodes[0] as SourceNode).ResponseTimes, out Xcheck, out Fcheck); 
            }
            else
            {
                Statistics.Density((TransformedModel.Nodes[0] as SourceNode).ResponseTimes.ToArray(), 
                    out Xcheck, out Fcheck, intervals); 
            }
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////






            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Results/"; 
            //Запись в файл 
            SaveFunction(Xappr, Fappr, "t Appr", path + file + "AppCDF" + ".csv", step, xmax); 
            SaveFunction(Xsim, Fsim, "t Sim", path + file + "SimCDF" + ".csv", step, xmax); 
            SaveFunction(Xcheck, Fcheck, "t Check", path + file + "CheckCDF" + ".csv", step, xmax); 

            Console.WriteLine("Программа завершила свою работу"); 
            Console.ReadKey();
            
        }









        /// <summary>
        /// Построение плотности распределения или функции распределения для ГРАФИКОВ диссертаци 
        /// Не использовать для дистанции Колмогорова-Смирнова
        /// </summary>
        public static void Example4()
        {
            //Число интервалов для построения гистограммы или выборочной фр
            bool cdf = false; 
            //Интенсивность входящего потока 
            double lambda = 1; 
            //Время моделирования
            double FinishTime = 50000;
            int intervals = (int)Math.Pow(FinishTime, 1.0 / 3) + 1;  

            Console.WriteLine("Получено {0} интервалов", intervals); 


            if (cdf)
            {
                Console.WriteLine("Построение выборочной функции распределения"); 
            }
            else
            {
                Console.WriteLine("Построение выборочное плотности распределения"); 
            }

            Random rand = new Random(); 

            //Имитационное моделирование одноприборное 
            double[] Fsim, Xsim; 
            //Приближенный аналитический результат в тех же точках 
            double[] Fappr, Xappr; 
            //Имитационное моделирование для приближенной модели
            double[] Fcheck, Xcheck; 




            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";


            //Получаем описание для сети массового осблуживания с деленеим и слиянием требованием
            var Description = new DescriptionOFJQN(filename); 
            //Console.WriteLine(Description); 

            //Меняем интенсивность входящего потока требований согласно поднанной на вход в функцию
            Description.Lambda0 = lambda; 


            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("================================================================================");
            Console.WriteLine("Lambda0 = {0:f4}", lambda); 
            Console.WriteLine("Имитационное моделирование одноприборной сети обслуживания");

            //Создаем модель по её описанию 
            NetworkModel OriginalModel = OFJQN.CreateNetworkModel(Description, rand); 
            //Запускаем модель
            OriginalModel.Run(FinishTime); 



            //Построение функции распределения
            if (cdf)
            {
                Statistics.DistributionFunction((OriginalModel.Nodes[0] as SourceNode).ResponseTimes.ToArray(), 
                    out Xsim, out Fsim, intervals); 

            }
            else
            {
                Statistics.Density((OriginalModel.Nodes[0] as SourceNode).ResponseTimes.ToArray(), 
                    out Xsim, out Fsim, intervals); 
            }


            Console.WriteLine("Выполнение приближённого метода анализа");
            //Вычисление интенсивностей входящего потока в каждую из систем обслуживания
            var Lambda = InfinityServerOpenForkJoinAnalizator.TotalInputRates(
                             InfinityServerOpenForkJoinAnalizator.InputRates(Description.S, Description.F, Description.J,
                                 Description.Lambda0, Description.Theta)); 

            //Выполняется преобразование интенсивностей и увеличение числа приборов
            Console.WriteLine("Выполняем переход к бесконечноприборной сети с интенсивностями обслуживания:"); 
            //Меняем описание для сети

            double[] rho = new double[Description.S.Length]; 
            for (int i = 0; i < Description.S.Length; i++)
            {
                //Увеличиваем число обслуживающих приборов до "бесконечности"
                Description.kappa[i] = 1000000;

                //Считаем ro так как если бы это была одноприборная сеть обслуживания
                rho[i] = Lambda[i] / Description.mu[i]; 

                //Модифицируем интенсинвность обслуживания фрагментов одним прибором
                Description.mu[i] = Description.mu[i] - Lambda[i]; 

                Console.WriteLine("mu[{0}] = {1:f4}, ro[{0}] = {2:f4}", i + 1, Description.mu[i], rho[i]);
            }



            Console.WriteLine("Аналитическое моделирование"); 
            var ph = InfinityServerOpenForkJoinAnalizator.ResponseTimeDistribution(Description);
            //////////////////////////////////////////////////////
            //Аналитическая приближенная функция распределения; 
            Xappr = Xsim; 
            Fappr = new double[Xappr.Length]; 
            for (int i = 0; i < Xappr.Length; i++)
            {
                if (cdf)
                {
                    Fappr[i] = PhaseTypeCDF(ph, Xappr[i]); 
                }
                else
                {
                    Fappr[i] = PhaseTypePDF(ph, Xappr[i]); 

                }
            }
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////

            Console.WriteLine("Имитационное моделирование бесконечноприборной - проверка");
            NetworkModel TransformedModel = OFJQN.CreateNetworkModel(Description, rand); 
            TransformedModel.Run(FinishTime); 

            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //Построение функции распределения
            if (cdf)
            {
                Statistics.DistributionFunction((TransformedModel.Nodes[0] as SourceNode).ResponseTimes.ToArray(), 
                    out Xcheck, out Fcheck,
                    intervals); 
            }
            else
            {
                Statistics.Density((TransformedModel.Nodes[0] as SourceNode).ResponseTimes.ToArray(), 
                    out Xcheck, out Fcheck, intervals); 
            }
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////






            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Results/"; 
            //Запись в файл 
            SaveFunction(Xappr, Fappr, "t Appr", path + file + "AppPDF" + ".csv", 1, xmax); 
            SaveFunction(Xsim, Fsim, "t Sim", path + file + "SimPDF" + ".csv", 1, xmax); 
            SaveFunction(Xcheck, Fcheck, "t Check", path + file + "CheckPDF" + ".csv", 1, xmax); 

            Console.WriteLine("Программа завершила свою работу"); 
            Console.ReadKey();

            
        }

        /// <summary>
        /// Построение сети размещения
        /// и вычисление стационарного распределения
        /// </summary>
        public static void Example5()
        {
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";


            //Получаем описание для сети массового осблуживания с деленеим и слиянием требованием
            var Description = new DescriptionOFJQN(filename); 
            //Console.WriteLine(Description); 
            Console.WriteLine(Description.ToString()); 


            //Множество размещений
            List<LocationState> Locations = new List<LocationState>(); 
            List<LocationState> LocationsNext = new List<LocationState>(); 
            //Номера размещений
            Dictionary<string, int> LocationsID = new Dictionary<string, int>(); 
            List<double> MuLocation = new List<double>(); 




            var Theta = Description.Theta[1]; 

            int[] F = { Description.F[0] }; 
            int[] A = Utils.AdjacentNodes(Theta, Description.F[0]); 
            //Вероятности переходов
            int Number = (int)Math.Pow(Description.S.Length, A.Length) + 1; 
            Console.WriteLine("Number = {0}", Number);
            double[,] Probabilities = new double[Number, Number];  


            Locations.Add(new LocationState(F)); 
            LocationsNext.Add(new LocationState(A)); 
            LocationsID.Add(new LocationState(F).ToString(), 0); 
            LocationsID.Add(new LocationState(A).ToString(), 1); 
            Probabilities[0, 1] = 1; 


            Console.WriteLine("LocationsNext:"); 
            foreach (var item in LocationsNext)
            {
                Console.WriteLine(item); 
            }

            //Пока список на посещение не пуст делать
            while (LocationsNext.Count > 0)
            {
                //Берем первый элемент из списка
                LocationState location = LocationsNext[0];
                //Удаляем его из этого списка
                LocationsNext.Remove(location); 
                //Добавляем его в список всех размещений 
                Locations.Add(location);

                //Определяем для него интенсивность обслуживания
                double mu = 0; 
                for (int i = 0; i < location.Set.Length; i++)
                {
                    int s = Array.IndexOf(Description.S, location.Set[i]); 
                    //Если элемент есть базовая система, то 
                    if (s >= 0)
                    {
                        mu += Description.mu[s]; 
                    }
                }
                MuLocation.Add(mu); 
                Console.WriteLine("mu({0}) = {1:f4}", location.ToString(), mu); 


                //Определяем смежные системы и вероятности переходов
                for (int i = 0; i < location.Set.Length; i++)
                {
                    int n = location.Set[i]; 
                    int s = Array.IndexOf(Description.S, location.Set[i]);
                    //Если система - интергратор, то из нее никуда не уйдем 
                    if (s == -1)
                    {
                        continue; 
                    }

                    //Список всех смежных систем
                    var nodes = Utils.AdjacentNodes(Theta, n); 
                    for (int j = 0; j < nodes.Length; j++)
                    {
                        //Вероятность перехода
                        double p = Theta[n, nodes[j]] * (Description.mu[s] / mu); 
                        //Генерим новое состояние, в которое перешли 

                        int[] array = new int[location.Set.Length];
                        //Перенос в массив старых элементов и замена одного из 
                        for (int k = 0; k < array.Length; k++)
                        {
                            array[k] = location.Set[k]; 
                        }
                        array[i] = nodes[j]; 
                        //Если новое состояние есть (JJJJ) то это равновсильно возврату в источник, 
                        //его точно не нужно добавлять в список общий Locations
                        bool flag = false; 
                        for (int k = 0; k < array.Length; k++)
                        {
                            if (array[k] != Description.J[0])
                            {
                                flag = true; 
                                break; 
                            }
                        }

                        LocationState newlocation; 
                        if (flag)
                        {
                            newlocation = new LocationState(array); 
                            //Добавляем в список следующего посещения если он еще не был посещен
                            if (!(Locations.Contains(newlocation) || LocationsNext.Contains(newlocation)))
                            {
                                LocationsNext.Add(newlocation); 
                            }
                        }
                        else
                        {
                            //Возвращение в источник
                            newlocation = new LocationState(F); 
                        }

                        //Добавляем его в словарь 

                        if (!(LocationsID.ContainsKey(newlocation.ToString())))
                        {
                            LocationsID.Add(newlocation.ToString(), LocationsID.Count); 
                        }

                        //Console.WriteLine("LocationsID");
                        //foreach (var item in LocationsID)
                        {
                            // Console.WriteLine(item.Key); 
                        }
                        //Console.WriteLine("Location1 ={0}", location);
                        //Console.WriteLine("Location2 ={0}", newlocation);

                        Probabilities[LocationsID[location.ToString()], LocationsID[newlocation.ToString()]] += p; 
                        Console.WriteLine("P(({0}); ({1})) = {2}", location, newlocation, Probabilities[LocationsID[location.ToString()], LocationsID[newlocation.ToString()]]); 

                    }
                    
                }

                
            }


            Console.WriteLine("Собираем все вместе");
            int NumberOfLocation = Locations.Count; 
            double[,] route = new double[NumberOfLocation, NumberOfLocation];
            double[] MU = MuLocation.ToArray(); 
            for (int i = 0; i < NumberOfLocation; i++)
            {
                double sum = 0; 
                for (int j = 0; j < NumberOfLocation; j++)
                {
                    route[i, j] = Probabilities[i, j]; 
                    sum += route[i, j]; 
                }
                Console.WriteLine(sum);


            }
            Console.WriteLine("Число систем = {0}", NumberOfLocation - 1); 

            Console.WriteLine("Маршрутная матрица сети размещений"); 
            Console.WriteLine(new Matrix(route)); 
            Console.WriteLine("Locations:");
            for (int m = 0; m < NumberOfLocation; m++)
            {

                Console.Write("{0}:  \t {1} \t", m, Locations[m]); 

                if (m > 0)
                {
                    Console.WriteLine(MU[m - 1].ToString()); 
                }
                else
                {
                    Console.WriteLine(); 
                }
            }








            //Поиск стационарных характеристик сети обслуживания
            Matrix Am = new Matrix(route); 
            for (int i = 0; i < Am.CountColumn; i++)
            {
                Am[i, i]--; 

            }
            Am[0, 1] = 1; 
            double[] b = new double[NumberOfLocation]; 
            b[0] = 1; 
            double[] omega = Computation.Gauss(Am.Transpose(), b); 

            double[] lambda = new double[NumberOfLocation]; 
            for (int i = 0; i < NumberOfLocation; i++)
            {
                lambda[i] = Description.Lambda0 * omega[i] / omega[0]; 
                Console.WriteLine("lambda{0}={1:f4}", i, lambda[i]);
            }

            //Получение стационарного распределения 
            double[] rho = new double[NumberOfLocation - 1]; 
            for (int i = 0; i < rho.Length; i++)
            {
                rho[i] = lambda[i + 1] / MU[i]; 
                Console.WriteLine("rho{0}={1:f4}", i, rho[i]);

            }

            for (int i = 0; i < Description.S.Length; i++)
            {
                //номер базовой системы
                int s = Description.S[i];
                Console.WriteLine("Исследуем S{0} = N{1}", i + 1, s); 

                //Список значимых лоакаций
                List<LocationState> ImportantLocations = new List<LocationState>(); 
                List<int> ImportantPhi = new List<int>(); 
                List<double> ImportantRho = new List<double>(); 



                //Проход по всем  локациям и выделение значимых
                for (int l = 1; l < Locations.Count; l++)
                {
                    //Взяли локацию 
                    var location = Locations[l]; 

                    //Расщепляем все системы в локации 
                    string[] temp = location.ToString().Split(':'); 
                    int[] bs = new int[temp.Length]; 
                    for (int j = 0; j < bs.Length; j++)
                    {
                        bs[j] = int.Parse(temp[j]); 
                    }
                    //Считаем фи
                    int phi = bs.Count(x => x == s); 
                    if (phi > 0)
                    {

                        //Локацию считаем значимой для системы 
                        ImportantLocations.Add(location); 
                        ImportantPhi.Add(phi); 
                        ImportantRho.Add(lambda[l] / MU[l - 1]); 
                        //Console.WriteLine("{0} =  {1}", location.ToString(), phi); 

                    }
                }

                //Считаем стационарные вероятности только по значимым локациям

                //Максимальное число требований в системе (и локации)
                int Q = 4; 
                //Массив с вероятностями 
                double[] P = new double[Q]; 

                //Будем генерировать все состояния в которых каждая значимая локация содержит от 0 до q требований
                long[] state = new long[ImportantLocations.Count]; 

                long maxnumber = (long)(Math.Pow(Q, state.Length)); 
                Console.WriteLine(maxnumber);

                //проход по всем состояниям
                for (long number = 0; number < maxnumber; number++)
                {

                    long rem = number; 
                    state = new long[ImportantLocations.Count]; 
                    int pos = 0;
                    do
                    {
                        state[pos] = rem % Q; 
                        rem = rem / Q; 
                        pos++; 
                            
                    }
                    while(rem > 0);



                    long CountOfDemand = 0; 
                    for (int j = 0; j < state.Length; j++)
                    {
                        //Сколько требований есть в этой локации
                        CountOfDemand += ImportantPhi[j] * state[j]; 
                    }


                    if (CountOfDemand < P.Length)
                    {
                           
                        double prod = 1; 
                        for (int j = 0; j < state.Length; j++)
                        {

                            prod *= Math.Pow(ImportantRho[j], (int)state[j]) *
                            Math.Exp(-ImportantRho[j]) /
                            (double)MathNet.Numerics.SpecialFunctions.Factorial(state[j]); 
                        
                        }
                        P[CountOfDemand] += prod;  

                    }
                   


                }

                for (int m = 0; m < P.Length; m++)
                {
                    Console.WriteLine("P({0}) = {1}", m, P[m]); 
                }



            }
               



        }


        /// <summary>
        /// Вычисляет дистанцию Колмогорова для сети обслуживания и сохраняет в файл эту дистанцию 
        /// </summary>
        public static void Example6()
        {
            Console.WriteLine("Example6"); 
            Console.WriteLine("Дистанция Колмогорова Смирнова для различных параметров сети обслуживания"); 


            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("ru-RU");


            Random rand = new Random(); 
            double FinishTime = 100000;  



            double LambdaMin = 0.5; 
            double LambdaMax = 1.3; 
            if (file == "OneServerExample2")
            {
                LambdaMax = 3.6; 
            }
            double lambda = 0; 
            double h = 0.2; 


            //Списки с результатами моделирования
            List<double> ListLambda = new List<double>(); 
            List<double> Distances = new List<double>(); 
            List<double> VolumeOfSamples = new List<double>(); 


            lambda = LambdaMin; 
            do
            {


                //Имитационное моделирование одноприборное 
                double[] Fsim, Xsim; 
                //Имитационное моделирование для приближенной модели
                double[] Fcheck, Xcheck; 




                string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";


                //Получаем описание для сети массового осблуживания с деленеим и слиянием требованием
                var Description = new DescriptionOFJQN(filename); 
                //Console.WriteLine(Description); 

                //Меняем интенсивность входящего потока требований согласно поднанной на вход в функцию
                Description.Lambda0 = lambda; 


                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("================================================================================");
                Console.WriteLine("Lambda0 = {0:f4}", lambda); 
                Console.WriteLine("Имитационное моделирование одноприборной сети обслуживания");

                //Создаем модель по её описанию 
                NetworkModel OriginalModel = OFJQN.CreateNetworkModel(Description, rand); 
                //Запускаем модель
                OriginalModel.Run(FinishTime); 



                //Построение функции распределения

                Statistics.DistributionFunction((OriginalModel.Nodes[0] as SourceNode).ResponseTimes, 
                    out Xsim, out Fsim); 



                Console.WriteLine("Выполнение приближённого метода анализа");
                //Вычисление интенсивностей входящего потока в каждую из систем обслуживания
                var Lambda = InfinityServerOpenForkJoinAnalizator.TotalInputRates(
                                 InfinityServerOpenForkJoinAnalizator.InputRates(Description.S, Description.F, Description.J,
                                     Description.Lambda0, Description.Theta)); 

                //Выполняется преобразование интенсивностей и увеличение числа приборов
                Console.WriteLine("Выполняем переход к бесконечноприборной сети с интенсивностями обслуживания:"); 
                //Меняем описание для сети

                double[] rho = new double[Description.S.Length]; 
                for (int i = 0; i < Description.S.Length; i++)
                {
                    //Увеличиваем число обслуживающих приборов до "бесконечности"
                    Description.kappa[i] = 1000000;

                    //Считаем ro так как если бы это была одноприборная сеть обслуживания
                    rho[i] = Lambda[i] / Description.mu[i]; 

                    //Модифицируем интенсинвность обслуживания фрагментов одним прибором
                    Description.mu[i] = Description.mu[i] - Lambda[i]; 

                    Console.WriteLine("mu[{0}] = {1:f4}, ro[{0}] = {2:f4}", i + 1, Description.mu[i], rho[i]);
                }




                Console.WriteLine("Имитационное моделирование бесконечноприборной - проверка");
                NetworkModel TransformedModel = OFJQN.CreateNetworkModel(Description, rand); 
                TransformedModel.Run(FinishTime); 

                //////////////////////////////////////////////////////
                //////////////////////////////////////////////////////
                //////////////////////////////////////////////////////
                //Построение функции распределения

                Statistics.DistributionFunction((TransformedModel.Nodes[0] as SourceNode).ResponseTimes,
                    out Xcheck, out Fcheck);



                //Вычисление максимальной погрешности с использованием линейной аппроксимации

                double distance = 0; 
                for (int i = 0; i < Xsim.Length; i++)
                {
                    double exact = LinearApproximation(Xsim[i], Xcheck, Fcheck); 
                    //Console.WriteLine("{0:f4}   {1:f4}    {2:f4}", Xsim[i], exact, Fsim[i]); 

                    double delta = Math.Abs(exact - Fsim[i]); 
                    if (delta > distance)
                    {
                        distance = exact; 
                    }


                }

                Console.WriteLine("Объем выборки n = {0}", Xsim.Length);
                Console.WriteLine("Дистанция Колмогорова {0:f4}", distance); 





                ListLambda.Add(lambda); 
                Distances.Add(distance);
                VolumeOfSamples.Add(Xsim.Length); 


                lambda += h; 

            }
            while (lambda < LambdaMax);







            Console.WriteLine("Результаты: ");

            for (int i = 0; i < ListLambda.Count; i++)
            {
                Console.WriteLine("{0} {1:f4} {2:f4} {3}", i, ListLambda[i], Distances[i], VolumeOfSamples[i]);
            }



            //Сохранение в файл
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

            using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Results/" + file + ".csv"))
            {
                sw.Write("Lambda"); 
                for (int j = 0; j < ListLambda.Count; j++)
                {
                    sw.Write(" {0:f4}", ListLambda[j]); 
                }
                sw.WriteLine(); 



                sw.Write("Dist");
                for (int j = 0; j < Distances.Count; j++)
                {
                    sw.Write(" {0:f4}", Distances[j]); 
                }
                sw.WriteLine(); 



                sw.Write("Volume"); 
                for (int j = 0; j < VolumeOfSamples.Count; j++)
                {
                    sw.Write(" {0:f4}", VolumeOfSamples[j]); 
                }
                sw.WriteLine(); 



            }






            Console.WriteLine("Программа завершила свою работу");
            Console.ReadKey(); 

        }


        /// <summary>
        /// Построение сети размещеий без вычисления стационарногоо распределения
        /// 
        /// Возможно не будет работать при наличии петель 
        /// В диссертации модиИЦИРОВАННЫЙ ВАРИАНТ 
        /// 
        /// </summary>
        public static void Example7()
        {
            Console.WriteLine("Example7");
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";


            //Получаем описание для сети массового осблуживания с деленеим и слиянием требованием
            var Description = new DescriptionOFJQN(filename); 
            //Console.WriteLine(Description); 
            Console.WriteLine(Description.ToString()); 


            //Множество размещений
            List<LocationState> Locations = new List<LocationState>(); 
            List<LocationState> LocationsNext = new List<LocationState>(); 
            //Номера размещений
            Dictionary<string, int> LocationsID = new Dictionary<string, int>(); 
            List<double> MuLocation = new List<double>(); 




            var Theta = Description.Theta[1]; 

            int[] F = { Description.F[0] }; 
            int[] A = Utils.AdjacentNodes(Theta, Description.F[0]); 
            //Вероятности переходов
            int Number = (int)Math.Pow(Description.S.Length, A.Length) + 1; 
            Console.WriteLine("Number = {0}", Number);
            double[,] Probabilities = new double[Number, Number];  


            Locations.Add(new LocationState(F)); 
            LocationsNext.Add(new LocationState(A)); 
            LocationsID.Add(new LocationState(F).ToString(), 0); 
            LocationsID.Add(new LocationState(A).ToString(), 1); 
            Probabilities[0, 1] = 1; 


            Console.WriteLine("LocationsNext:"); 
            foreach (var item in LocationsNext)
            {
                Console.WriteLine(item); 
            }

            //Пока список на посещение не пуст делать
            while (LocationsNext.Count > 0)
            {
                //Берем первый элемент из списка
                LocationState location = LocationsNext[0];
                //Удаляем его из этого списка
                LocationsNext.Remove(location); 
                //Добавляем его в список всех размещений 
                Locations.Add(location);

                //Определяем для него интенсивность обслуживания
                double mu = 0; 
                for (int i = 0; i < location.Set.Length; i++)
                {
                    int s = Array.IndexOf(Description.S, location.Set[i]); 
                    //Если элемент есть базовая система, то 
                    if (s >= 0)
                    {
                        mu += Description.mu[s]; 
                    }
                }
                MuLocation.Add(mu); 
                Console.WriteLine("mu({0}) = {1:f4}", location.ToString(), mu); 


                //Определяем смежные системы и вероятности переходов
                for (int i = 0; i < location.Set.Length; i++)
                {
                    int n = location.Set[i]; 
                    int s = Array.IndexOf(Description.S, location.Set[i]);
                    //Если система - интергратор, то из нее никуда не уйдем 
                    if (s == -1)
                    {
                        continue; 
                    }

                    //Список всех смежных систем
                    var nodes = Utils.AdjacentNodes(Theta, n); 
                    for (int j = 0; j < nodes.Length; j++)
                    {
                        //Вероятность перехода
                        double p = Theta[n, nodes[j]] * (Description.mu[s] / mu); 
                        //Генерим новое состояние, в которое перешли 

                        int[] array = new int[location.Set.Length];
                        //Перенос в массив старых элементов и замена одного из 
                        for (int k = 0; k < array.Length; k++)
                        {
                            array[k] = location.Set[k]; 
                        }
                        array[i] = nodes[j]; 
                        //Если новое состояние есть (JJJJ) то это равновсильно возврату в источник, 
                        //его точно не нужно добавлять в список общий Locations
                        bool flag = false; 
                        for (int k = 0; k < array.Length; k++)
                        {
                            if (array[k] != Description.J[0])
                            {
                                flag = true; 
                                break; 
                            }
                        }

                        LocationState newlocation; 
                        if (flag)
                        {
                            newlocation = new LocationState(array); 
                            //Добавляем в список следующего посещения если он еще не был посещен
                            if (!(Locations.Contains(newlocation) || LocationsNext.Contains(newlocation)))
                            {
                                LocationsNext.Add(newlocation); 
                            }
                        }
                        else
                        {
                            //Возвращение в источник
                            newlocation = new LocationState(F); 
                        }

                        //Добавляем его в словарь 

                        if (!(LocationsID.ContainsKey(newlocation.ToString())))
                        {
                            LocationsID.Add(newlocation.ToString(), LocationsID.Count); 
                        }

                        //Console.WriteLine("LocationsID");
                        //foreach (var item in LocationsID)
                        {
                            // Console.WriteLine(item.Key); 
                        }
                        //Console.WriteLine("Location1 ={0}", location);
                        //Console.WriteLine("Location2 ={0}", newlocation);

                        Probabilities[LocationsID[location.ToString()], LocationsID[newlocation.ToString()]] += p; 
                        Console.WriteLine("P(({0}); ({1})) = {2}", location, newlocation, Probabilities[LocationsID[location.ToString()], LocationsID[newlocation.ToString()]]); 

                    }

                }


            }


            Console.WriteLine("Собираем все вместе");
            int NumberOfLocation = Locations.Count; 
            double[,] route = new double[NumberOfLocation, NumberOfLocation];
            double[] MU = MuLocation.ToArray(); 
            for (int i = 0; i < NumberOfLocation; i++)
            {
                double sum = 0; 
                for (int j = 0; j < NumberOfLocation; j++)
                {
                    route[i, j] = Probabilities[i, j]; 
                    sum += route[i, j]; 
                }
                Console.WriteLine(sum);


            }
            Console.WriteLine("Число систем = {0}", NumberOfLocation - 1); 

            Console.WriteLine("Маршрутная матрица сети размещений"); 
            Console.WriteLine(new Matrix(route)); 
            Console.WriteLine("Locations:");
            for (int m = 0; m < NumberOfLocation; m++)
            {

                Console.Write("{0}:  \t {1} \t", m, Locations[m]); 

                if (m > 0)
                {
                    Console.WriteLine(MU[m - 1].ToString()); 
                }
                else
                {
                    Console.WriteLine(); 
                }
            }




            Console.WriteLine("Ненулевые элементы маршрутной матрицы");
            Matrix temp = new Matrix(route); 
            for (int i = 0; i < temp.CountRow; i++)
            {
                Console.WriteLine("****  " + i.ToString() + "  *****");

                for (int j = 0; j < temp.CountColumn; j++)
                {
                    if (temp[i, j] > 0)
                    {
                        Console.WriteLine("{0}->{1} ({3}->{4}))\t {2:f4}", Locations[i], Locations[j], temp[i, j], i, j); 
                    }
                }

            }


            Console.WriteLine("Example7 закончил свою работу");
        }





        /// <summary>
        /// Библиотечная оптимизация 
        /// </summary>
        public static List<double>  Example8(double[] W, bool print, int IterationMax, double Lambda0, 
            out double TargetValue)
        {

            TargetValue = double.PositiveInfinity; 

            Console.WriteLine("Example 8 - Библиотечная оптимизация");
            Random r = new Random(); 



            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";
            DescriptionOFJQN Description = new DescriptionOFJQN(filename); 
            Description.Lambda0 = Lambda0; 

            ResponseTimeFunction RT = new ResponseTimeFunction(Description, print); 

            var opt = new LibOptimization.Optimization.clsOptDEJADE(RT); 

            //Начальное распределение 
            var dims = RT.WeightsDim();

            if (W == null)
            {
                W = new double[dims.Sum()]; 
                RandomWeightsDistr(dims, W, r); 
            }


            Console.WriteLine("W0 = ");
            PrintVector(W); 
            Console.WriteLine();




            //Настраиваем
            opt.LowerBounds = new double[RT.NumberOfVariable()];
            opt.UpperBounds = new double[RT.NumberOfVariable()];
            for (int i = 0; i < RT.NumberOfVariable(); i++)
            {
                opt.UpperBounds[i] = 1; 
                opt.LowerBounds[i] = 0; 
            }


            opt.InitialPosition = W; 
            opt.Iteration = IterationMax;
            //Применяем настройки
            opt.Init(); 

            //Optimization
            opt.DoIteration();


            //Check Error
            if (opt.IsRecentError() == true)
            {
                return null;
            }
            else
            {
                //Get Result
                clsUtil.DebugValue(opt);
            }

            return opt.Result; 
            TargetValue = RT.F(opt.Result); 
            Console.WriteLine("sum = {0:f6}", opt.Result.Sum());
           
        }



        /// <summary>
        /// Стохастическая оптмизация 
        /// </summary>
        public static double[] Example9(int FindN, bool print, out double TargetValue, double Lambda0)
        {
            Console.WriteLine("Example 9 - Стохастическая оптимизация");

            Random r = new Random(); 

           
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";
            DescriptionOFJQN Description = new DescriptionOFJQN(filename); 
            Description.Lambda0 = Lambda0; 

            ResponseTimeFunction RT = new ResponseTimeFunction(Description, print); 

            //Начальное распределение 
            var dims = RT.WeightsDim();

            //Генерация начального распределения 
            double[] W = new double[dims.Sum()]; 
            double[] Wnew = new double[dims.Sum()]; 

            RandomWeightsDistr(dims, W, r); 
            for (int i = 0; i < W.Length; i++)
            {
                Wnew[i] = W[i]; 
            }
            Console.WriteLine("W0 = ");
            PrintVector(W); 
            Console.WriteLine();



            double min = Double.PositiveInfinity; 



            double[] eps = new double[dims.Length]; 
            for (int i = 0; i < eps.Length; i++)
            {
                eps[i] = 1; 
            }

            double delta = 0.1; 

            int dimsSum = dims.Sum(); 
            while (eps.Max() > delta)
            {
                
                Console.WriteLine(eps.Max());

                for (int i = 0; i < FindN; i++)
                {
                    RandomWeightsDistr(dims, Wnew, eps, r); 

                    double temp = RT.F(Wnew.ToList()); 

                    if (temp < min)
                    {
                        min = temp; 
                        PrintVector(Wnew); 
                        Console.WriteLine("* {0:f4}, |{1:f4}", temp, eps.Max());
                        //Сохраненение решения
                        for (int j = 0; j < Wnew.Length; j++)
                        {
                            W[j] = Wnew[j]; 
                        }
                    }
                }

                for (int k = 0; k < eps.Length; k++)
                {
                    eps[k] -= delta / dimsSum;
                
                }
            }

            Console.WriteLine("***********************************************"); 
            Console.WriteLine("***********************************************"); 
            Console.WriteLine("***********************************************"); 

            Console.WriteLine("Результаты прогона со всеми хорошими параметрами"); 
            RT.print = true; 
            RT.F(W.ToList()); 

            Console.WriteLine("***********************************************"); 
            Console.WriteLine("***********************************************"); 
            Console.WriteLine("***********************************************"); 


            TargetValue = min; 
            return W; 
        }

        /// <summary>
        /// Проверка функии на выпуклость в рандомных точках 
        /// </summary>
        /// <param name="N">Число рандомных пар точек</param>
        /// <param name="h">Число точек на отрезке</param>
        public static void RandomConvexCheck(int N, int M)
        {
            Console.WriteLine("Проверка функции на выпуклость рандомным перебором точек"); 
            Random r = new Random(); 


            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";
            DescriptionOFJQN Description = new DescriptionOFJQN(filename); 

            ResponseTimeFunction RT = new ResponseTimeFunction(Description, false); 

            //Начальное распределение 
            var dims = RT.WeightsDim();

            //Две точки
            double[] X = new double[dims.Sum()]; 
            double[] Y = new double[dims.Sum()]; 

            //Средняя точка
            double[] mid = new double[dims.Sum()]; 
            mid.Initialize(); 


            for (int i = 0; i < N; i++)
            {
                //Генирим рандомно концы отрезка
                RandomWeightsDistr(dims, X, r); 
                RandomWeightsDistr(dims, Y, r); 

                double fx = RT.F(X.ToList()); 
                double fy = RT.F(Y.ToList()); 
                if (fx + fy == Double.PositiveInfinity)
                {
                    Console.WriteLine("Бесконечность");
                    continue;

                }

                //Иначе отрезок должен принадлежать всей хорошей области 
 
                for (int j = 0; j < M; j++)
                {
                    double alphax = r.NextDouble(); 
                    double alphay = 1 - alphax; 

                    MidlePoint(X, Y, mid, alphax, alphay); 

                    double fmid = RT.F(mid.ToList()); 
                    if (fmid > alphax * fx + alphay * fy)
                    {
                        Console.WriteLine("Нарушение выпуклости в точке"); 
                        return; 
                    }
                }
            }



            Console.WriteLine("Выпуклость не была нарушена"); 

           
        }



        /// <summary>
        /// Оптимизация сети обслуживания стохастическим и билиотечным методом
        /// </summary>
        public static void Optimazer(double Lambda0)
        {
            //Example5(); 
            //Example2(); 



            //Example7(); 


            //file = "OneServerExample1"; 
            //Example6(); 

            //file = "OneServerExample2"; 
            //Example6(); 




            //RandomConvexCheck(100, 10);




            double RT9 = 0; 
            var W9 = Example9(100, false, out RT9, Lambda0);

            double[] W10 = new double[W9.Length]; 
            Array.Copy(W9, W10, W9.Length); 
            double RT8 = 0; 
            Example8(W10, false, 1000, Lambda0, out RT8); 




            Console.WriteLine("Результаты оптмизации"); 
            Console.WriteLine("Результат стохастической оптимизации:");
            PrintVector(W9); 
            Console.WriteLine();
            Console.WriteLine("RT9 = {0:f4}", RT9);
            Console.WriteLine("End MAIN");
        }



        /// <summary>
        /// Строит таблицу для оптимальных значений распределения весов в сети обслуживания для различных значений 
        /// интенсивности входящего потока
        /// выдает интенсивности входящего потока в каждую систему 
        /// 
        /// </summary>
        public static void TableOptimize(int FindN)
        {
            double LambdaMin = 1; 
            double LambdaMax = 4.1; 
            double LambdaStep = 0.2; 

            double Lambda0;


            List<double[]> W = new List<double[]>(); 
            List<double> Lambdas = new List<double>();
            List<double> Targets = new List<double>(); 
            List<double[]> NodesMU = new List<double[]>(); 
            List<double[]> NodesLambda = new List<double[]>(); 

            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";
            DescriptionOFJQN Description = new DescriptionOFJQN(filename); 



            List<bool> Winners = new List<bool>(); 

            for (Lambda0 = LambdaMin; Lambda0 < LambdaMax; Lambda0 += LambdaStep)
            {
                double TargetValueStochastic = 0; 
                double TargetValueLib = 0; 


                var OptimalWeightStochastic= Example9(FindN, false, out TargetValueStochastic, Lambda0); 
                var OptimalWeightLib = Example8(OptimalWeightStochastic, false, 1000, Lambda0, out TargetValueLib ); 

                //Смортрим лучший вариант оптимизации
                var OptimalWeight = OptimalWeightLib.ToArray(); 
                var TargetValue = TargetValueLib;
                //Если стохастическая оптимизаци оказалась лучше, то берем еe

                if (TargetValueStochastic <= TargetValueLib)
                {
                    TargetValue = TargetValueStochastic; 
                    OptimalWeight = OptimalWeightStochastic.ToArray(); 
                    Winners.Add(false); 
  
                }
                else
                {
                    Winners.Add(true); 
                }



                //Сохранение результатов 
                W.Add(OptimalWeight); 
                Lambdas.Add(Lambda0); 
                Targets.Add(TargetValue); 


                //Подсчет интенсивностей 
               
                Description.Lambda0 = Lambda0; 
                ResponseTimeFunction RT = new ResponseTimeFunction(Description, false);
                double[] MU; 
                double[] LAMBDA; 
                RT.LambdasMu(OptimalWeight, out MU, out LAMBDA); 

                //Сохранение интесвностей 
                NodesMU.Add(MU); 
                NodesLambda.Add(LAMBDA); 
            }


            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/OPTIM" + file + ".txt";



            Console.WriteLine("Выполняется сохраненеи в формат LATEX"); 
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write("Lambda0 ");
                sw.Write("RTopt ");
                for (int i = 0; i < W[0].Length; i++)
                {
                    sw.Write("w{0} ", i); 
                }
                for (int i = 0; i<Description.S.Length; i++)
                {
                    sw.Write("mu{0} ", i); 
                }


                for (int i = 0; i < Lambdas.Count; i++)
                {
                    sw.WriteLine(); 
        
                    sw.Write("{0:f4} ", Lambdas[i]);
                    sw.Write("{0:f4} ", Targets[i]); 
                    for (int j = 0; j < W[i].Length; j++)
                    {
                        sw.Write("{0:f4} ", W[i][j]); 
                    }
                    //Вывод урезанных интенсивностей обслуживания 
                    for (int j = 0; j < NodesMU[i].Length; j++)
                    {
                        sw.Write("{0:f4} ", NodesMU[i][j]); 
                    }
                }
            }
            Console.WriteLine("Сохраненение выполнено успешно");



            foreach (var item in Winners)
            {
                if (item)
                {
                    Console.WriteLine("Библиотечная оптимизация точнее");

                }
                else
                {
                    Console.WriteLine("Стохастическая оптимизация точнее");

                }
            }
        }

        /// <summary>
        /// Поиск хороших параметров для интенсивности обслуживания 
        /// </summary>
        public static void  FindGoodRates(double Lambda0, double MinMu, double MaxMu)
        {
            int FindN = 500;
            int RoutersNumber = 4; 

            Random r = new Random(); 




            double[] MU = new double[RoutersNumber]; 


           

            int N = 5; 
            double h = (MaxMu - MinMu) / N; 


            for (int i1 = 0; i1 < N; i1++)
            {
                MU[0] = MinMu + i1 * h;
                for (int i2 = 0; i2 < N; i2++)
                {
                    MU[1] = MinMu + i2 * h;


                    for (int i3 = 0; i3 < N; i3++)
                    {
                        MU[2] = MinMu + i3 * h;

                        for (int i4 = 0; i4 < N; i4++)
                        {
                            MU[3] = MinMu + i4 * h;

                            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + file + ".txt";
                            DescriptionOFJQN Description = new DescriptionOFJQN(filename); 
                            Description.Lambda0 = Lambda0; 

                            for (int k = 0; k < RoutersNumber; k++)
                            {
                                Description.mu[k] = MU[k];

                            }

                            Console.WriteLine ("MU = ");
                            PrintVector(MU);
                            Console.WriteLine ();

                            ResponseTimeFunction RT = new ResponseTimeFunction(Description, false); 

                            //Начальное распределение 
                            var dims = RT.WeightsDim();

                            //Генерация начального распределения 
                            double[] W = new double[dims.Sum()]; 
                            double[] Wnew = new double[dims.Sum()]; 

                            RandomWeightsDistr(dims, W, r); 
                            for (int i = 0; i < W.Length; i++)
                            {
                                Wnew[i] = W[i]; 
                            }
                            Console.WriteLine("W0 = ");
                            PrintVector(W); 
                            Console.WriteLine();




                            double[] eps = new double[dims.Length]; 
                            for (int i = 0; i < eps.Length; i++)
                            {
                                eps[i] = 1; 
                            }



                            double min = Double.PositiveInfinity; 
                            for (int i = 0; i < FindN; i++)
                            {
                                RandomWeightsDistr(dims, Wnew, eps, r); 

                                double temp = RT.F(Wnew.ToList()); 

                                if (temp < min)
                                {
                                    min = temp; 
                                    PrintVector(Wnew); 
                                    Console.WriteLine("* {0:f4}, |{1:f4}", temp, eps.Max());
                                    //Сохраненение решения
                                    for (int j = 0; j < Wnew.Length; j++)
                                    {
                                        W[j] = Wnew[j]; 
                                    }
                                }
                            }

                            if (min == double.PositiveInfinity)
                            {
                                continue;
                            }


                            double delta = 0.1; 
                            bool success = true; 
                            for (int i = 0; i < W.Length; i++)
                            {

                                if (!((W[i] > delta) && (W[i] < 1 - delta)))
                                {
                                    success = false;

                                    break;
                                }
                            }
                            if (success)
                            {

                                Console.WriteLine("Найдены подходящие интесивности ");
                                Console.WriteLine("RT = {0:f4}", min);
                                Console.WriteLine("Rates =");
                                PrintVector(MU);
                                Console.WriteLine();
                                PrintVector(Description.mu);
                                Console.WriteLine();
                                Console.WriteLine("W = ");
                                PrintVector(W); 
                                return; 
                            }



                        }
                    }
                        
                }
                    
                //MU[i] = Math.Round(MinMu + (MaxMu - MinMu) * r.NextDouble(), 1);
                //Description.mu[i] = MU[i]; 



                    
            }


        }

        public static void Main(string[] args)
        {

            file = "OneServerExample3";
            //Example1(); 
            //Optimazer(1); 
            TableOptimize(1000);
            //FindGoodRates(3, 0.1, 6); 

        }





    }
}
