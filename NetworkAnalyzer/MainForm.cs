using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopNetworkAnalyzator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new RoutingMatrixCreator();
            f.ShowDialog(); 
             
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
