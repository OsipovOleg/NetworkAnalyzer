using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLAS;


namespace NetworkDescriptions
{
    /// <summary>
    /// Маршрутная матрица для сети с делением и слиянием требований
    /// </summary>
    public class RoutingMatrix
    {
        private double[][,] Theta;
        /// <summary>
        /// Число разделяющих систем с учетом источника требований 
        /// </summary>
        public int CountForker
        {
            get
            {
                return Theta.Length;
            }
        }

        /// <summary>
        ///Маршрутная матрица размера DimxDim
        /// </summary>
        /// <param name="L">Число систем обслуживания в сети</param>
        public RoutingMatrix(int Dim, int CountForker)
        {
            Theta = new double[CountForker][,];
            for (int i = 0; i < Theta.Length; i++)
            {
                Theta[i] = new double[Dim, Dim];
            }
        }

        /// <summary>
        /// Размерность маршртной матрицы
        /// </summary>
        public int Dimention
        {
            get
            {
                return Theta[0].GetLength(0);
            }
        }
        /// <summary>
        /// Вероятность перехода из i-го узла в j-ый узел 
        /// </summary>
        /// <param name="k">Выбор маршрутной матрицы</param>
        /// <param name="i">Система из которой переходит переход </param>
        /// <param name="j">Система в которую происходит переход</param>
        /// <returns></returns>
        public double this[int k, int i, int j]
        {
            get
            {
                return Theta[k][i, j];
            }
            set
            {
                Theta[k][i, j] = value;
            }
        }


        public double[,] this[int k]
        {
            get
            {
                return Theta[k];
            }
            set
            {
                Theta[k] = value;
            }
        }

        /// <summary>
        /// Строка k-матрицы передачи 
        /// </summary>
        /// <param name="i">Номер узла(строки)</param>
        /// <param name="k">Номер матры</param>
        /// <returns></returns>
        public double[] RoutingRow(int i, int k)
        {
            double[] row = new double[Dimention];
            for (int j = 0; j < Dimention; j++)
            {
                row[j] = Theta[k][i, j];
            }
            return row;
        }

        /// <summary>
        /// Маршрутная матрица для фиксированного узла
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double[,] RoutingMatrixForNode(int i)
        {
            double[,] m = new double[CountForker, Dimention];
            for (int k = 0; k < CountForker; k++)
            {
                for (int j = 0; j < Dimention; j++)
                {
                    m[k, j] = Theta[k][i, j];
                }
            }
            return m;
        }


        public override string ToString()
        {
            StringBuilder s = new StringBuilder(String.Empty);

            for (int k = 0; k < Theta.Length; k++)
            {
                s.AppendLine(k.ToString());
                for (int i = 0; i < Dimention; i++)
                {
                    for (int j = 0; j < Dimention; j++)
                    {
                        s.AppendFormat("{0:f4} ", Theta[k][i, j]);
                    }
                    s.AppendLine();
                }
            }

            return s.ToString();
        }


        /// <summary>
        /// Удаление системы и соотвествующих строк и столбцов 
        /// </summary>
        /// <param name="IndexOfNode">Номер системы (строки столбца) для удаления</param>
        public void DeleteNode(int IndexOfNode)
        {
            int Dim = Dimention;


            for (int k = 0; k < Theta.Length; k++)
            {
                double[,] temp = new double[Dim - 1, Dim - 1];
                for (int i = 0; i < Dim; i++)
                {
                    for (int j = 0; j < Dim; j++)
                    {
                        if ((i < IndexOfNode) && (j < IndexOfNode))
                        {
                            temp[i, j] = Theta[k][i, j];
                            continue;
                        }
                        if ((i > IndexOfNode) && (j > IndexOfNode))
                        {
                            temp[i - 1, j - 1] = Theta[k][i, j];
                            continue;
                        }
                        if ((i > IndexOfNode) && (j < IndexOfNode))
                        {
                            temp[i - 1, j] = Theta[k][i, j];
                            continue;
                        }
                        if ((i < IndexOfNode) && (j > IndexOfNode))
                        {
                            temp[i, j - 1] = Theta[k][i, j];
                            continue;
                        }

                    }

                }
                Theta[k] = temp;
            }
        }


        /// <summary>
        /// Получает матрицу смежности графа по набору матриц передачи
        /// </summary>
        /// <returns>The matrix.</returns>
        public Matrix AdjacencyMatrix()
        {
            Matrix M = new Matrix(Theta[0]); 
            for (int i = 1; i < Theta.Length; i++)
            {
                M += new Matrix(Theta[i]); 
            }

            for (int i = 0; i < M.CountRow; i++)
            {
                for (int j = 0; j < M.CountColumn; j++)
                {
                    if (M[i, j] > 0)
                    {
                        M[i, j] = 1; 
                    }
                }
            }

            return M; 
            
        }


        /// <summary>
        /// Удаляет матрицу из матрицы передачи для некоторой пары дивайдер-интегратор 
        /// с удалением соотвествующих строк
        /// </summary>
        /// <param name="k">Номер матрицы для удаления (k>0)</param>
        /// <param name="IndexOfForkNode">Индекс дивайдера F_k</param>
        /// <param name="IndexJoinNode">Индекс интегратора J_k</param>
        public void DeleteMatrix(int k, int IndexOfForkNode, int IndexJoinNode)
        {
            //Создание новой матрицы для компирования
            double[][,] ThetaTemp = new double[CountForker - 1][,];
            for (int i = 0; i < ThetaTemp.Length; i++)
            {
                ThetaTemp[i] = new double[Dimention - 2, Dimention - 2];
            }


            //Удаление строк и столбцов 
            if (IndexOfForkNode < IndexJoinNode)
            {
                DeleteNode(IndexJoinNode);
                DeleteNode(IndexOfForkNode);
            }
            else
            {
                DeleteNode(IndexOfForkNode);
                DeleteNode(IndexJoinNode);
            }

            //Копирование и удаление матрицы 
            for (int l = 0; l < CountForker; l++)
            {
                if (l < k)
                {
                    ThetaTemp[l] = this.Theta[l];
                    continue;
                }
                if (l > k)
                {
                    ThetaTemp[l - 1] = this.Theta[l];
                }
            }

            Theta = ThetaTemp;
        }

    }
}
