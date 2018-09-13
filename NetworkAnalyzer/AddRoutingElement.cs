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
    public partial class AddRoutingElement : Form
    {
        public AddRoutingElement()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Строковое представление всех узлов в сети обслуживания
        /// </summary>
        public string[] Nodes
        {
            get;
            set;
        }

        /// <summary>
        /// Созданный элемент
        /// </summary>
        public Tuple<int, int, double> element
        {
            get;
            set;
        }

        private void AddRoutingElement_Load(object sender, EventArgs e)
        {
            DestanationNodesBox.Items.Clear();
            SourceNodeBox.Items.Clear();

            DestanationNodesBox.Items.AddRange(Nodes);
            SourceNodeBox.Items.AddRange(Nodes);
        }

        /// <summary>
        /// Создает элемент матрицы для выбранной пары узлов 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            int s = SourceNodeBox.SelectedIndex;
            int d = DestanationNodesBox.SelectedIndex;
            double value = double.Parse(RoutingElementText.Text);

            element = new Tuple<int, int, double>(s, d, value);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
