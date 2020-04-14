using BPManager.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BPManager
{
    /// <summary>
    /// Interaction logic for CellsEdit.xaml
    /// </summary>
    /// 

    public partial class CellsEdit : Window
    {
        ObservableCollection<Cells> cells;

        public CellsEdit()
        {
            InitializeComponent();
        }

        public CellsEdit(ObservableCollection<Cells> cells)
        {
            InitializeComponent();
            this.cells = cells;

            comboCellsList.ItemsSource = this.cells;

        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (comboCellsList.SelectedIndex >= 0)
            cells.RemoveAt(comboCellsList.SelectedIndex);

        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {

            int x = (cells.Count > 0) ? cells[cells.Count - 1].CellID + 1 : 1;
            cells.Add(new Cells { CellID = x, CellTitle = textNewCellName.Text });

        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }
    }
}
