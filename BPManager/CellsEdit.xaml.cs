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
        ObservableCollection<Breakpoint> BPClass;

        public CellsEdit()
        {
            InitializeComponent();
        }

        public CellsEdit(ObservableCollection<Cells> cells, ObservableCollection<Breakpoint> BPClass)
        {
            InitializeComponent();
            this.cells = cells;
            this.BPClass = BPClass;

            comboCellsList.ItemsSource = this.cells;

        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            Cells selectedCell = comboCellsList.SelectedItem as Cells;
            if (selectedCell == null) return;

            List<Breakpoint> affectedCells = new List<Breakpoint>(BPClass.ToList().FindAll(x => x.BPCell == selectedCell.CellID));

            if (affectedCells.Count == 0)
            {
                    cells.RemoveAt(comboCellsList.SelectedIndex);
            } else
            {
                MessageBox.Show($"Error: \nThis cell is used in {affectedCells.Count} breakpoints. Please delete breakpoints before deleting cell.", "Can't delete cell.");
            }


        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (textNewCellName.Text != String.Empty)
            {
                int x = (cells.Count > 0) ? cells[cells.Count - 1].CellID + 1 : 1;
                cells.Add(new Cells { CellID = x, CellTitle = textNewCellName.Text });
            }
            textNewCellName.Text = String.Empty;

        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }

    }
}
