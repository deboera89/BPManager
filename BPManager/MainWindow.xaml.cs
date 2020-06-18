using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BPManager.Properties;

namespace BPManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>




    public partial class MainWindow : Window
    {

        // observable collections over normal List<> so the items using these as data sources will update real time. 

        BreakpointManager BPManager = new BreakpointManager();

        public MainWindow()
        {
            InitializeComponent();


            BPList.ItemsSource = BPManager._listSearch;
            comboBPCell.ItemsSource = BPManager._bpCells;
            comboSearchList.ItemsSource = BPManager._bpSearchCells;

            // set the Filter to the first item ("Show All")

            comboSearchList.SelectedIndex = 0;

            if (BPManager._settings.isURL == true)
            {

                buttonDelete.IsEnabled = false;
                buttonEdit.IsEnabled = false;
                buttonNew.IsEnabled = false;
                menuEditCells.IsEnabled = false;
                menuNewBP.IsEnabled = false;
                menuDeleteBP.IsEnabled = false;
                menuEditBP.IsEnabled = false;

            }

        }

        private void BPList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // update the info fields with the data from the selected index on BPList

            if (BPList.SelectedIndex >= 0)
            {
                Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;

                textBPNumber.Text = selectedBreakpoint.BPID.ToString();
                comboBPCell.SelectedIndex = (BPManager._bpCells.ToList().Exists(x => x.CellID == selectedBreakpoint.BPCell)) ? (BPManager.ReturnIDFromDropDown(selectedBreakpoint.BPCellNumber)) : -1;
                textBPDescription.Text = selectedBreakpoint.BPDescription;
                dateBPStarted.SelectedDate = DateTime.Parse(selectedBreakpoint.BPStart);
                dateBPFinished.SelectedDate = DateTime.Parse(selectedBreakpoint.BPFinish);

            }
            else
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

            if (BPList.SelectedIndex >= 0)
            {
                textBPDescription.IsReadOnly = false;
                comboBPCell.IsEnabled = true;
                buttonSave.IsEnabled = true;
                dateBPStarted.IsEnabled = true;
                dateBPFinished.IsEnabled = true;
            }

        }


        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // edit the breakpoint BPClass with the new data and save to the xml file

            Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;
            Cells selectedCell = comboSearchList.SelectedItem as Cells;

            if (selectedBreakpoint != null)
            {
                int selected = BPList.SelectedIndex;
                BPManager.EditBreakpoint(selectedCell, selectedBreakpoint, textBPDescription.Text, dateBPStarted.SelectedDate.ToString(), dateBPFinished.SelectedDate.ToString(), comboBPCell.SelectedIndex);
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
            // if the search/filter combo box is being used, pass the reference along to create new breakpoint to the shown cell

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

            new CellsEdit(BPManager._bpCells, BPManager._bpClass).Show();

        }

        private void ComboSearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // updates the Breakpoint list with the filter

            BPManager.UpdateListSearch(comboSearchList.SelectedItem as Cells);
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            // open settings window, add a hook to the window close to run the closingSettings function

            settingsWindow sw = new settingsWindow(BPManager);
            sw.Closing += new System.ComponentModel.CancelEventHandler(closingSettings);
            sw.Show();
        }

        private void closingSettings(object sender, CancelEventArgs e)
        {
            /*  called when the settings window closes
                opens a new window and closes the current one
                this allows the app to load from the new settings file

            */

            new MainWindow().Show();
            this.Close();
        }
    }
}
