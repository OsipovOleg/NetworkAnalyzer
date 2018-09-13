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
    public partial class RoutingMatrixCreator : Form
    {
        public RoutingMatrixCreator()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Строковое описание узлов
        /// </summary>
        public string[] Nodes
        {
            get; set;
        }

        /// <summary>
        /// Список элементов матрицы
        /// </summary>
        public List<Tuple<int, int, double>> RoutingElements
        {
            get;
            set;
        }


        private void AddButton_Click(object sender, EventArgs e)
        {
            var f = new AddRoutingElement();
            f.Nodes = Nodes;

            if (f.ShowDialog() == DialogResult.OK)
            {
                var element = f.element;
                RoutingElements.Add(element);
                RoutingList.Items.Add(string.Format("{0} -> {1}:  {2:f4}", Nodes[element.Item1], Nodes[element.Item2], element.Item3));
            }

        }

        private void RoutingMatrixCreator_Load(object sender, EventArgs e)
        {
            
            
            //RoutingList.Items.Clear();
           //foreach (var element in RoutingElements)
           // {
               // RoutingList.Items.Add(string.Format("{0} -> {1}:  {2:f4}", Nodes[element.Item1], Nodes[element.Item2], element.Item3));
          //  }

        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Удаляет выбранный элемент
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, EventArgs e)
        {

        }
    }
}
