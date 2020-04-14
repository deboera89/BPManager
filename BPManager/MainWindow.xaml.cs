using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using BPManager.Properties;
using System.Collections.Specialized;
using System.Data;
using System.IO;

namespace BPManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>




    public partial class ExitEventHandler : Window
    {



        // observable collections over normal List<> so the items using these as data sources will update real time. 
        BreakpointManager BPManager = new BreakpointManager();


        public ExitEventHandler()
        {
            InitializeComponent();
            BPManager.start();

            BPList.ItemsSource = BPManager.listSearch;

            comboBPCell.ItemsSource = BPManager.BPCells;

            comboSearchList.ItemsSource = BPManager.BPSearchCells;

            comboSearchList.SelectedIndex = comboSearchList.Items.Count - 1;

        }

        private void BPList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // update the info fields with the data from the selected index on BPList

            if (BPList.SelectedIndex >= 0)
            {
                Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;
                textBPNumber.Text = selectedBreakpoint.BPID.ToString();

                comboBPCell.SelectedIndex = (BPManager.BPCells.ToList().Exists(x => x.CellID == selectedBreakpoint.BPCell)) ? (BPManager.ReturnIDFromDropDown(selectedBreakpoint.BPCellNumber)) : -1;

                textBPDescription.Text = selectedBreakpoint.BPDescription;
                dateBPStarted.SelectedDate = DateTime.Parse(selectedBreakpoint.BPStart);
                dateBPFinished.SelectedDate = DateTime.Parse(selectedBreakpoint.BPFinish);

            } else
            {
                // if none selected (usually after a breakpoint is deleted) then clear all values

                textBPDescription.Text = String.Empty;
                textBPNumber.Text = String.Empty;
                comboBPCell.SelectedIndex = -1;
                dateBPStarted.SelectedDate = null;
                dateBPFinished.SelectedDate = null;

            }


        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            // enable all info fields if edit button is clicked

            textBPDescription.IsReadOnly = false;
            comboBPCell.IsEnabled = true;
            buttonSave.IsEnabled = true;
            dateBPStarted.IsEnabled = true;
            dateBPFinished.IsEnabled = true;
            

        }


        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // edit the breakpoint BPClass with the new data and save to the xml file

            Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;

            if (selectedBreakpoint != null) {
                int selected = BPList.SelectedIndex;
                int BPClassIndex = BPManager.ReturnCellFromBreakpointCellID(selectedBreakpoint.BPID);
                BPManager.BPClass[BPClassIndex].BPCell = BPManager.BPCells[comboBPCell.SelectedIndex].CellID;
                BPManager.BPClass[BPClassIndex].BPDescription = textBPDescription.Text;
                BPManager.BPClass[BPClassIndex].BPStart = DateTime.Parse(dateBPStarted.SelectedDate.ToString()).ToString("MM/dd/yyyy");
                BPManager.BPClass[BPClassIndex].BPFinish = DateTime.Parse(dateBPFinished.SelectedDate.ToString()).ToString("MM/dd/yyyy");
                BPManager.BPClass[BPClassIndex].BPCellNumber = BPManager.RetCellFromCID(BPManager.BPClass[BPClassIndex].BPCell);

                BPManager.SaveFile();

                BPManager.UpdateListSearch(comboSearchList.SelectedItem as Cells);

                BPList.SelectedIndex = selected;
            }

            // once save is finished, disable all the info fields
            textBPDescription.IsReadOnly = true;
            comboBPCell.IsEnabled = false;
            buttonSave.IsEnabled = false;
            dateBPStarted.IsEnabled = false;
            dateBPFinished.IsEnabled = false;

        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            // creates a new breakpoint and adds it to the BPClass list

            // if the search/filter combo box is being used, pass the reference along
            Cells selectedCell = comboSearchList.SelectedItem as Cells;
            BPManager.AddNewBreakpoint(selectedCell);

            // set the focus to the newly added Breakpoint
            BPList.SelectedIndex = BPList.Items.Count - 1;
            BPList.Focus();

        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            // delete the selected breakpoint from the BPClass list
            Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;
            BPManager.DeleteBreakpoint(selectedBreakpoint);

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // exits application
            this.Close();
        }

        private void MenuEditCells_Click(object sender, RoutedEventArgs e)
        {
            // opens settings, pass BPCells & BPClass through so we can add Cells from the settings 
            new CellsEdit(BPManager.BPCells, BPManager.BPClass).Show();

        }

        private void ComboSearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // updates the Breakpoint list with the filter
            BPManager.UpdateListSearch(comboSearchList.SelectedItem as Cells);
        }

    }
}
