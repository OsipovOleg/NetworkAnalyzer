using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLAS;

namespace PhaseTypeDistribution
{
    public static class Demo
    {
        public static void DemoPH()
        {
            Console.WriteLine("Демонстрация работы с классом PH");
            double mu1 = 2;
            double mu2 = 2;
            double mu3 = 2;
            double mu4 = 2;
            double mu5 = 2;
            double mu6 = 2;


            double[,] A1 = { { -mu1 } };
            double[] alpha1 = { 1 };

            double[,] A2 = {
                { -mu2, 0.5 * mu2, 0},
                { 0, -mu4, 0.5 * mu4},
                { 0, mu5, -mu5}
            };

            double[] alpha2 = { 1, 0, 0 };


            double[,] A3 = {
                { -mu2, mu2, 0},
                { 0, -mu4, 0.2 * mu4},
                { 0, 0.7 * mu5, -mu5}
            };
            double[] alpha3 = { 1, 0, 0 };


            double[,] A4 = {
                { -mu5, 0.7 * mu5 },
                { 0.2 * mu4, -mu4 }
            };
            double[] alpha4 = { 1, 0 };

            double[,] A5 = {
                { -mu3, 0.7 * mu3, 0, 0},
                {0, -mu2, mu2, 0},
                {0, 0, -mu4, 0.2 * mu4},
                {0, 0, 0.7 * mu5, -mu5}
            };
            double[] alpha5 = { 1, 0, 0, 0 };


            PhaseTypeVarible A1PH = new PhaseTypeVarible(new Matrix(A1), alpha1);
            PhaseTypeVarible A2PH = new PhaseTypeVarible(new Matrix(A2), alpha2);
            PhaseTypeVarible A3PH = new PhaseTypeVarible(new Matrix(A3), alpha3);
            PhaseTypeVarible A4PH = new PhaseTypeVarible(new Matrix(A4), alpha4);
            PhaseTypeVarible A5PH = new PhaseTypeVarible(new Matrix(A5), alpha5);

            // calculate PH Matrix of maximum of times for D1->S1 and D1->S2
            PhaseTypeVarible B12 = PHOperations.Max(A1PH, A2PH);
            PhaseTypeVarible B34 = PHOperations.Max(A3PH, A4PH);
            // PH B345 = PHOperations.Max({ B34, A5PH);
            PhaseTypeVarible[] variable = { A3PH, A4PH, A5PH };
            PhaseTypeVarible B345 = PHOperations.Max(variable);

            Matrix gamma12 = new Matrix(1, B12.NumberOfPhases);
            for (int i = 0; i < B12.NumberOfPhases; i++)
            {
                gamma12[0, i] = B12.InitialDistribution[i];
            }


            Matrix gamma345 = new Matrix(1, B345.NumberOfPhases);
            for (int i = 0; i < B345.NumberOfPhases; i++)
            {
                gamma345[0, i] = B345.InitialDistribution[i]; 
            }



            Matrix tau12 = -gamma12 * (B12.SubGenerator.Inv()) * Computation.OnesColumn(B12.NumberOfPhases);


            Matrix tau345 = -gamma345 * (B345.SubGenerator.Inv()) * Computation.OnesColumn(B345.NumberOfPhases);

            Matrix tau = 0.5 * tau12 + 0.5 * tau345;



            double t = tau[0, 0] + 1 / mu6;

            Console.WriteLine("{0:f6}  ", t);
        }


    }
}
