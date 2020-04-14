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
        // settings
        string saveName = "datafile.xml";


        //

        // globals

        // observable collections over normal List<> so the items using these as data sources will update real time. 
        ObservableCollection<Breakpoint> BPClass;
        ObservableCollection<Cells> BPCells;
        ObservableCollection<Cells> BPSearchCells;
        ObservableCollection<Breakpoint> listSearch;

        bool finishedLoading = false;

        public ExitEventHandler()
        {
            InitializeComponent();
            start();



        }


        private void start()
        {

            BPCells = new ObservableCollection<Cells> { };
            BPSearchCells = new ObservableCollection<Cells> { };

            BPCells.CollectionChanged += BPCells_CollectionChanged;
            BPClass = new ObservableCollection<Breakpoint> { };

            // sets the BPClass from the loaded xml save file
            BPClass = LoadXML(BPClass);

            // listSearch is the itemsource for the for hte breakpoint list on the main form.
            listSearch = BPClass;

            AddListItems();


            // after this bool is true we know BPCells and BPClass is filled so can start checking if there is changes made to them.
            finishedLoading = true;


        }

        private void BPCells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // update the GUI if Cells are added or deleted from the Edit Cells page

            if (finishedLoading)
            {
                saveFile();
                updateCellsList();
                UpdateListSearch();
            }
 
        }

        public void updateCellsList()
        {
            // mainly used if a cell is deleted, loop through the BPClass and update all the cell numbers (to empty if a cell was deleted with a breakpoint attached to that cell)
            for (int x = 0; x <= BPClass.Count-1; x++)
            {
                BPClass[x].BPCellNumber = RetCellFromBPID(BPClass[x].BPCell);
                
            }
        }

        public void noSaveFile()
        { 
            // small function to create a blank XML datafile if the program can't find it at load up. 

            MessageBoxResult dialogResult = MessageBox.Show("No data file found, do you want to create a new data file?", "No Data File", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                StringBuilder fileoutput = new StringBuilder();
                fileoutput.Append("<breakpoints>\n");
                fileoutput.Append("</breakpoints>");

                File.WriteAllLines(saveName, fileoutput.ToString().Split('\n'));
                fileoutput.Clear();
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                this.Close();
            }
        }



        private ObservableCollection<Breakpoint> LoadXML(ObservableCollection<Breakpoint> BPClass)
        {

        // reads XML datafile and sets up the BPClass and BPCells lists. 

        bool insideBreakpoint = false;
        bool insideCell = false;

        Breakpoint newBreakpoint = new Breakpoint();
        Cells newCell = new Cells();

            if (!File.Exists(saveName)) noSaveFile();


            System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(saveName);
            while (reader.Read())
            {
                if (reader.IsStartElement() && reader.Name.ToString() == "breakpoint")
                {
                    insideBreakpoint = true;
                }

                if (reader.IsStartElement() && reader.Name.ToString() == "cell")
                {
                    insideCell = true;
                }


                if (insideBreakpoint)
                {
                    switch (reader.Name.ToString())
                    {
                        case "id":
                            newBreakpoint.BPID = Int32.Parse(reader.ReadString());
                            break;
                        case "cellid":
                            newBreakpoint.BPCell = Int32.Parse(reader.ReadString());
                            break;
                        case "description":
                            newBreakpoint.BPDescription = reader.ReadString();
                            break;
                        case "datestarted":
                            newBreakpoint.BPStart = reader.ReadString();
                            break;
                        case "datefinished": 
                            newBreakpoint.BPFinish = reader.ReadString();
                            break;
                    }
                }

                if (insideCell)
                {
                    switch (reader.Name.ToString())
                    {
                        case "id":
                            newCell.CellID = Int32.Parse(reader.ReadString());
                            break;
                        case "title":
                            newCell.CellTitle = reader.ReadString();
                            break;
                    }
                }

                if (reader.NodeType == System.Xml.XmlNodeType.EndElement && reader.Name.ToString() == "breakpoint")
                {

                    newBreakpoint.BPCellNumber = RetCellFromBPID(newBreakpoint.BPCell);
                    BPClass.Add(newBreakpoint);
                    newBreakpoint = new Breakpoint();
                    insideBreakpoint = false;
                }

                if (reader.NodeType == System.Xml.XmlNodeType.EndElement && reader.Name.ToString() == "cell")
                {

                    BPCells.Add(newCell);
                    newCell = new Cells();
                    insideCell = false;
                }

            }

            reader.Dispose();

            return BPClass;

        }



        private void AddListItems()
        {

            // add items to the lists and combo boxes

            BPList.ItemsSource = listSearch;

            comboBPCell.ItemsSource = BPCells;

            // the filter/search list box has a new item added to it for us to clear the search box

            BPSearchCells = new ObservableCollection<Cells>(BPCells);

            int newCID = (BPSearchCells.Count > 0) ? BPSearchCells[BPSearchCells.Count - 1].CellID + 1 : 1;

            BPSearchCells.Add(new Cells { CellID = newCID, CellTitle = "Clear" });

            comboSearchList.ItemsSource = BPSearchCells;



        }

        private int ReturnIDFromDropDown(string cell)
        {
            // this function goes through each item in the BPCells list to see if input string cell title matches any celltitles in the list
            // it will return the index of the found cell in the list
            //
            // this fixes errors where users delete cells from the middle of the cell list, the CellID will not always match the Index of the item in the combo boxes.
            //

            for (int x = 0; x <= BPCells.Count-1; x++)
            {
                if (BPCells[x].CellTitle.ToString() == cell) return x;
            }

            return -1;
        }

        private string RetCellFromBPID(int ID)
        {
            // this function goes through the BPCellsID list and returns the cell string title of the specified ID. 
            foreach (var item in BPCells)
            {
                if (ID == item.CellID) return item.CellTitle;
            }

            return "";


        }

        private int RetListIDFromBPID(int BPID)
        {
            // this function loops through the BPClass list and finds where the specified BPID is located and returns the Index number.
            for (int x = 0; x <= BPClass.Count-1; x++)
            {
                if (BPID == BPClass[x].BPID) return x;

            }
            return -1;
        }

        private void BPList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // update the info fields with the data from the selected index on BPList

            if (BPList.SelectedIndex >= 0)
            {
                Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;
                textBPNumber.Text = selectedBreakpoint.BPID.ToString();

                comboBPCell.SelectedIndex = (BPCells.ToList().Exists(x => x.CellID == selectedBreakpoint.BPCell)) ? (ReturnIDFromDropDown(selectedBreakpoint.BPCellNumber)) : -1;

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

        private void saveFile()
        {
            // save the current BPClass and BPCells to the xml file.

            StringBuilder fileoutput = new StringBuilder();

            fileoutput.Append("<breakpoints>\n");
            int currentID = 0;

            foreach (var item in BPCells)
            { 
                fileoutput.Append("\t<cell>\n");
                fileoutput.Append($"\t\t<id>{item.CellID}</id>\n");
                fileoutput.Append($"\t\t<title>{item.CellTitle}</title>\n");
                fileoutput.Append("\t</cell>\n");

                currentID++;
            }

            foreach (var item in BPClass)
            {
                fileoutput.Append("\t<breakpoint>\n");
                fileoutput.Append($"\t\t<id>{item.BPID}</id>\n");
                fileoutput.Append($"\t\t<cellid>{item.BPCell}</cellid>\n");
                fileoutput.Append($"\t\t<description>{item.BPDescription}</description>\n");
                fileoutput.Append($"\t\t<datestarted>{item.BPStart}</datestarted>\n");
                fileoutput.Append($"\t\t<datefinished>{item.BPFinish}</datefinished>\n");
                fileoutput.Append("\t</breakpoint>\n");
            }

            fileoutput.Append("</breakpoints>");
            System.IO.File.WriteAllLines(saveName, fileoutput.ToString().Split('\n'));

            fileoutput.Clear();

        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // edit the breakpoint BPClass with the new data and save to the xml file

            Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;

            if (selectedBreakpoint != null) {
                int selected = BPList.SelectedIndex;
                int BPClassIndex = RetListIDFromBPID(selectedBreakpoint.BPID);
                BPClass[BPClassIndex].BPCell = BPCells[comboBPCell.SelectedIndex].CellID;
                BPClass[BPClassIndex].BPDescription = textBPDescription.Text;
                BPClass[BPClassIndex].BPStart = DateTime.Parse(dateBPStarted.SelectedDate.ToString()).ToString("MM/dd/yyyy");
                BPClass[BPClassIndex].BPFinish = DateTime.Parse(dateBPFinished.SelectedDate.ToString()).ToString("MM/dd/yyyy");
                BPClass[BPClassIndex].BPCellNumber = RetCellFromBPID(BPClass[BPClassIndex].BPCell);

                saveFile();

                UpdateListSearch();
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

            // if the search/filter combo box is being used, add the new breakpoint with the selected cell id, otherwise set cell id to 0 (empty)
            Cells selectedCell = comboSearchList.SelectedItem as Cells;
            int newCellID = (comboSearchList.SelectedIndex >= 0 && comboSearchList.SelectedIndex < BPSearchCells.Count()-1) ? selectedCell.CellID : 0;

            // get the last BPID in the BPClass and add 1 to it to get new BPID, if there is no breakpoints in the list then set new BPID to 1.
            int newBPID = (BPClass.Count > 0) ? BPClass[BPClass.Count - 1].BPID + 1 : 1;


            BPClass.Add(new Breakpoint { BPID = newBPID, BPCell = newCellID, BPDescription = "New Breakpoint Description", BPStart = DateTime.Today.ToString("MM/dd/yyyy"), BPFinish = DateTime.Now.AddMonths(3).ToString("MM/dd/yyyy"), BPCellNumber = RetCellFromBPID(newCellID) });


            saveFile();
            UpdateListSearch();


            BPList.SelectedIndex = BPList.Items.Count - 1;
            BPList.Focus();

        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            // delete the selected breakpoint from the BPClass list
            
            Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;

            if (selectedBreakpoint != null)
            {
                BPClass.RemoveAt(RetListIDFromBPID(selectedBreakpoint.BPID));
                saveFile();
                UpdateListSearch();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // exits application
            this.Close();
        }

        private void MenuEditCells_Click(object sender, RoutedEventArgs e)
        {
            // opens settings, pass BPCells & BPClass through so we can add Cells from the settings 
            new CellsEdit(BPCells, BPClass).Show();

        }


        private void UpdateListSearch()
        {
            // if a selection is made from the filter/search combo box then update the BPList to show the selected Cell.
            // The last item in BPSearchCells is the "Clear" item to remove the filter.




            if (comboSearchList.SelectedIndex >= 0 && comboSearchList.SelectedIndex <= BPSearchCells.Count - 2)
            {
                Console.WriteLine(comboSearchList.SelectedIndex);
                Cells selectedCell = comboSearchList.SelectedItem as Cells;
                listSearch = new ObservableCollection<Breakpoint>(BPClass.ToList().FindAll(x => x.BPCell == selectedCell.CellID));

            } else
            {
                // search criteria was removed so show all breakpoints

                listSearch = new ObservableCollection<Breakpoint>(BPClass);
            }

            // update listitems
            AddListItems();

        }

        private void ComboSearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateListSearch();
        }

    }
}
