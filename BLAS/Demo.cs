using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAS
{
    public static class Demo
    {
        public static void DemoBLAS()
        {

            Console.WriteLine("Демострация работы с классом Matrix"); 
            Matrix A = new Matrix(new double[,]
              { { 1, 2 },
                { 3, 4 } });
            Matrix B = new Matrix(new double[,]
                { { 0, 5 },
                { 6, 7 } });
            Matrix C = new Matrix(new double[,]
                { { 1, 2 },
                { 3, 4 } });
            Matrix D = new Matrix(new double[,]
                { { 1, 2 },
                { 3, 4 } });

            /* Matrix m = new Matrix(new Matrix[,]
                  { {A, B },
                  {C, D } });*/


            // Console.WriteLine(A.ToString());
            Console.WriteLine(Computation.KroneckerProduct(A, B).ToString());
        }




    }
}
