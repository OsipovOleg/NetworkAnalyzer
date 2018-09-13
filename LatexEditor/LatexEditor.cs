using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using BLAS;
using System.Diagnostics;

namespace Latex
{
    public static class LatexEditor
    {
        /// <summary>
        /// Формирует систему линейных алгебраических уравнений A*x = b в виде кода LaTex
        /// </summary>
        /// <param name="A_str">Текстовое представление матрицы A</param>
        /// <param name="b_str">Текстовое представление строки b</param>
        /// <param name="x_str">Текстовое представление столбца x</param>
        /// <param name="A">Численные значения матрицы A</param>
        /// <param name="b">Численные значения строки b</param>
        /// <returns></returns>
        public static string LatexSystemOfEquation(string[,] A_str, string[] b_str, string[] x_str,
            Matrix A, double[] b)
        {
            StringBuilder res = new StringBuilder();
            res.AppendLine();
            res.AppendLine(@"\begin{equation*}");
            res.AppendLine(@"\begin{cases}");

            int n = A.CountColumn;
            for (int i = 0; i < n; i++)
            {
                int j = 0;
                while (A[i, j] == 0)
                {
                    j++;
                }
                res.AppendFormat("{0}{1}", A_str[i, j], x_str[j]);
                j++;
                for (; j < n; j++)
                {
                    if (A[i, j] != 0)
                    {
                        res.AppendFormat("+{0}{1}", A_str[i, j], x_str[j]);
                    }
                }
                res.AppendFormat("= {0},", b_str[i]);
                if (i < n - 1)
                {
                    res.Append(@"\\");
                }
                res.AppendLine();
            }
            res.AppendLine(@"\end{cases}");
            res.AppendLine(@"\end{equation*}");

            res.AppendLine(@"\\");
            //Подстановка чисел
            res.AppendLine(@"\begin{equation*}");
            res.AppendLine(@"\begin{cases}");

            for (int i = 0; i < n; i++)
            {
                if (A[i, 0] != 0)
                {
                    res.AppendFormat("{0}{1}", A[i, 0], x_str[0]);
                }
                for (int j = 1; j < n; j++)
                {
                    if (A[i, j] > 0)
                    {
                        res.AppendFormat("+{0}{1}", A[i, j], x_str[j]);
                    }
                    if (A[i, j] < 0)
                    {
                        res.AppendFormat("{0}{1}", A[i, j], x_str[j]);

                    }

                }
                res.AppendFormat("= {0},", b[i]);
                if (i < n - 1)
                {
                    res.Append(@"\\");
                }
                res.AppendLine();
            }
            res.AppendLine(@"\end{cases}");
            res.AppendLine(@"\end{equation*}");


            res.AppendLine(@"\\");
            res.AppendLine(@"Решение СЛАУ\\");

            var x = BLAS.Computation.Gauss(A, b);
            res.AppendLine(@"\begin{equation*}");
            res.AppendLine(@"\begin{cases}");
            for (int i = 0; i < n; i++)
            {
                res.AppendFormat("{0}= {1:f4}; ", x_str[i], x[i]);
                if (i < n - 1)
                {
                    res.AppendLine(@"\\");
                }
            }
            res.AppendLine(@"\end{cases}");
            res.AppendLine(@"\end{equation*}");
            return res.ToString();
        }


        /// <summary>
        /// Формирует шапку tex-документа 
        /// </summary>
        /// <returns></returns>
        public static string BeginLaTex()
        {
            return @"
\documentclass[paper=a4, fontsize=16pt]{report}
\usepackage{setspace}
\linespread{1.6}


\usepackage[landscape,pdftex]{geometry}
\usepackage[T2A]{fontenc}
\usepackage[cp1251]{inputenc}
\usepackage[english,russian]{babel}
\usepackage{float}
\usepackage{amsthm}
\usepackage{amsmath}
\usepackage{amssymb}
\usepackage{amsfonts}
\usepackage{mathtext}
\usepackage{mathrsfs}
\usepackage{cite}
\usepackage{graphicx}
\usepackage{epic}
\usepackage{multicol}
\usepackage{longtable}





\theoremstyle{plain}
%Нумерованные
\newtheorem{theorem}{\indent Теорема}
\newtheorem{lemma}{\indent Лемма}
\newtheorem{corollary}{\indent Следствие}
\newtheorem{prop}{\indent Утверждение}
\newtheorem{propos}{\indent Предложение}
\newtheorem{problem}{\indent Задача}
\newtheorem{Theorem}{\indent Theorem}
\newtheorem{hypothesis}{\indent Предположение}
\newtheorem{Etheorem}{\indent Theorem}
\newtheorem{Eproposition}{\indent Proposition}
\newtheorem{Corollary}{\indent Corollary}
\newtheorem{Lemma}{\indent Lemma}
\newtheorem{theoremb}{\indent Теорема}
%Нумерация теорем A, B, C
\renewcommand{\thetheoremb}{\Alph{theoremb}}

\theoremstyle{remark}
\newtheorem{Example}{\indent Example}


%Двойная нумерация
\newtheorem{theoremnn}{\indent Теорема}[section]
\newtheorem{lemmann}{\indent Лемма}[section]
\newtheorem{corollarynn}{\indent Следствие}[section]
\newtheorem{propnn}{\indent Утверждение}[section]
\newtheorem{proposnn}{\indent Предложение}[section]
\newtheorem{hypothesisnn}{\indent Предположение}[section]

\theoremstyle{plain}
%Ненумерованные окружения
%Просто теорема (без номера)
\newtheorem*{theorem*}{\indent Теорема}
\newtheorem*{lemma*}{\indent Лемма}
\newtheorem*{corollary*}{\indent Следствие}
\newtheorem*{Corollary*}{\indent Corollary}
\newtheorem*{prop*}{\indent Утверждение}
\newtheorem*{propos*}{\indent Предложение}
\newtheorem*{hypothesis*}{\indent Предположение}
%-------------------------------------------------
\theoremstyle{definition}
\newtheorem{definitionn}{\indent Определение}[section]
\newtheorem{remarknn}{\indent Замечание}[section]
\newtheorem{examplenn}{\indent Пример}[section]

\newtheorem{definition}{\indent Определение}
\newtheorem{remark}{\indent Замечание}
\newtheorem{example}{\indent Пример}
\newtheorem{Remark}{\indent Remark}
\newtheorem{Question}{\indent Question}
\newtheorem{Definition}{\indent Definition}

\newtheorem*{definition*}{\indent Определение}
\newtheorem*{remark*}{\indent Замечание}
\newtheorem*{example*}{\indent Пример}

\renewenvironment{proof}{\indent\textbf{Доказательство. }}{\hfill$\Box$}

\newcommand{\const}{\mathrm{const}}
\newcommand{\Span}{\mathrm{Span}\,}
\renewcommand{\Re}{\,\mathrm{Re}\,}
\renewcommand{\Im}{\,\mathrm{Im}\,}
\newcommand{\sgn}{\mathrm{sgn}\,}
\newcommand{\diag}{\mathrm{diag}\,}

%Двойная нумерация формул
%\numberwithin{equation}{section}


\bibliographystyle{gost2008}
\usepackage{bm}
\usepackage{hyperref}
\usepackage{xcolor}
\definecolor{linkcolor}{HTML}{799B03} % цвет ссылок
\definecolor{urlcolor}{HTML}{799B03} % цвет гиперссылок

\hypersetup{pdfstartview=FitH,  linkcolor=linkcolor,urlcolor=urlcolor, colorlinks=true}




%Сеть массового обслуживания 
\newcommand{\Network}{\mathcal{N}}
%Узел 
\newcommand{\Node}{N}
%Подсеть массового обслуживания
\newcommand{\SubNetwork}{H}
%Команды для параметра #1 элементарной подсети
\newcommand{\Trivial}[1]{\hat{#1}}
%Элементарная сеть 
\newcommand{\TrivialSubNetwork}{\Trivial{\SubNetwork}}
%Множество всех элементарных подсетей массового обслуживания
\newcommand{\SetTrivialSubNetwork}{\mathcal{\TrivialSubNetwork}}
\newcommand{\SetIndTrivialSubNetwork}{X_{\SetTrivialSubNetwork}}

\newcommand{\RT}{\tau}
\newcommand{\TrivialRT}{\Trivial{\RT}}

%Множестово систем в ветке от дивайдера #1 начиная с системы #2
\newcommand{\BranchSequence}[2]{\mathcal{B}{(#1,S_{#2})}}
\newcommand{\ElementBranchSequence}[1]{b_{#1}}

%Множество всех систем смежных с дивайдером #1
\newcommand{\ReachableSet}[1]{\mathcal{R}^{(#1)}}
\newcommand{\ElementReachableSet}[2]{r^{(#1)}_{#2}}


%Множества базовых систем, дивайдеров и интеграторов
\newcommand{\SetS}{\mathcal{S}}
\newcommand{\SetF}{\mathcal{F}}
\newcommand{\SetJ}{\mathcal{J}}

%Число всех систем, базовых систем, двйдеров и интеграторов
\newcommand{\NumberNodes}{L}
\newcommand{\NumberS}{\NumberNodes_{\SetS}}
\newcommand{\NumberF}{\NumberNodes_{\SetF}}
\newcommand{\NumberJ}{\NumberNodes_{\SetF}}
%Множество номеров систем облуживания, базовых систем
%дивайдеров и интеграторов
\newcommand{\SetIndNodes}{X}
\newcommand{\SetIndS}{\SetIndNodes_{\SetS}}
\newcommand{\SetIndF}{\SetIndNodes_{\SetF}}
\newcommand{\SetIndJ}{\SetIndNodes_{\SetJ}}

%Последовательность номеров дивайдеров интеграторов и базовых систем
\newcommand{\IndS}{x^\SetS}
\newcommand{\IndF}{x^\SetF}
\newcommand{\IndJ}{x^\SetJ}

%Оператор математического ожидания
\newcommand{\Expect}{\mathsf{E}}

% Цепь Маркова 
\newcommand{\MC}{\mathfrak{C}}
\newcommand{\StateMC}{\mathfrak{s}}
\newcommand{\InitialProbMC}{\alpha}
\newcommand{\InitialDistMC}{\bm{\InitialProbMC} }
\newcommand{\GeneratorMC}{\bm{Q}}

%Фазовое распределение 
\newcommand{\PH}{\mathrm{PH}}
%Матрица фазового распределения
\newcommand{\MatrixPH}{\bm{A}}
\newcommand{\TransitionRate}{a}
%Операия максимума для фазовых с.в.
\newcommand{\MaxPH}{\vee}
\newcommand{\BigMaxPH}{\bigvee}
\newcommand{\DimPH}{Y}
\newcommand{\TrivialDimPH}{\Trivial{\DimPH}}
\newcommand{\TrivialTransitionRate}{\Trivial{\TransitionRate}}

%Мощность пространства состояний и других мат моделей 
\newcommand{\Card}{c}



%Матрицы
%Едничная матрица 
\newcommand{\Eye}{\bm{I}}

%Марица передачи
\newcommand{\RouteMatrix}{\bm{\Theta}}
\newcommand{\ForkRouteMatrix}[1]{\RouteMatrix^{(#1)}}
\newcommand{\ElementRouteMatrix}[3]{\theta^{(#1)}_{#2,#3}}

\newcommand{\ReductionRouteMatrix}{\tilde{\bm{\Theta}}}
\newcommand{\ReductionForkRouteMatrix}[1]{\ReductionRouteMatrix^{(#1)}}
\newcommand{\ReductionElementRouteMatrix}[3]{\tilde{\theta}^{(#1)}_{#2,#3}}

\newcommand{\ReductionNetwork}{\tilde{\Network}}

\newcommand{\Task}{\sigma}
\newcommand{\PredTask}{\Task^*}
\newcommand{\MoveVec}{\bm{v}}
\newcommand{\SetMove}{\mathcal{V}}


\begin{document}";

        }

        /// <summary>
        /// Формирует окончание tex-документа
        /// </summary>
        /// <returns></returns>
        public static string EndLaTex()
        {
            return @"\end{document}";

        }


        /// <summary>
        /// Сохраняет документ
        /// </summary>
        /// <param name="code">Текст документа</param>
        /// <param name="name">Имя файла без разширения</param>
        public static void SaveLaTex(string code, string FileName)
        {
            using (StreamWriter sw = new StreamWriter(FileName + ".tex", false, Encoding.GetEncoding(1251)))
            {
                sw.WriteLine(code);
            }

        }

        /// <summary>
        /// Компилирует файл (pdflatex.exe)
        /// </summary>
        /// <param name="FileName">Имя файла без расширения</param>
        public static void CompileLaTex(string FileName)
        {
            string cmd = @"-synctex=1 -interaction=nonstopmode " + FileName + ".tex";
            var pr = Process.Start("pdflatex.exe ", cmd);
        }

        /// <summary>
        /// Отображает PDF документ с данными (SumatraPDF)
        /// </summary>
        /// <param name="FileName">Имя файла без расширения</param>
        public static void ShowPDF(string FileName)
        {

            Process psi = new Process();

            string cmd = FileName + ".pdf";
            psi.StartInfo = new ProcessStartInfo(@"C:\SumatraPDF.exe", cmd);
            //Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe
            psi.Start();
            psi.WaitForExit();


        }

        /// <summary>
        /// Переход на новую строку
        /// </summary>
        public static string NewLine
        {
            get
            {
                return @"\\";
            }
        }

        public static string NewPage
        {
            get
            {
                return @"\newpage";
            }
        }





    }
}
