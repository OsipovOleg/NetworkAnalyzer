using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Latex
{
    public static class Demo
    {
        public static void DemoLatex()
        {
            string text = LatexEditor.BeginLaTex();
            text += "Привет Мир\n";
            text += LatexEditor.EndLaTex();
            LatexEditor.SaveLaTex(text, "simple");

            
            LatexEditor.CompileLaTex("simple");
            Thread.Sleep(2000);
            LatexEditor.ShowPDF("simple");
        }

    }
}
