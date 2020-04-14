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

            BPClass = LoadXML(BPClass);
            listSearch = BPClass;

            AddListItems();


            finishedLoading = true;


        }

        private void BPCells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (finishedLoading)
            {
                saveFile();
                updateCellsList();
                UpdateListSearch();
            }
 
        }

        public void updateCellsList()
        {
            for (int x = 0; x <= BPClass.Count-1; x++)
            {
                BPClass[x].BPCellNumber = RetCellFromBPID(BPClass[x].BPCell);
                
            }
        }

        public void noSaveFile()
        {
            MessageBoxResult dialogResult = MessageBox.Show("No data file found, do you want to create a new data file?", "No Data File", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                string fileoutput = "";
                fileoutput += "<breakpoints>\n";
                fileoutput += "</breakpoints>";

              //  File.Create(saveName);

                File.WriteAllLines(saveName, fileoutput.Split('\n'));
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                this.Close();
            }
        }



        private ObservableCollection<Breakpoint> LoadXML(ObservableCollection<Breakpoint> BPClass)
        {

        bool insideBreakpoint = false;
        bool insideCell = false;

        Breakpoint newBreakpoint = new Breakpoint();
        Cells newCell = new Cells();


          //  Console.WriteLine(File.Exists(saveName) ? "File exists." : "File does not exist.");
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

            BPList.ItemsSource = listSearch;

            comboBPCell.ItemsSource = BPCells;

            BPSearchCells = new ObservableCollection<Cells>(BPCells);
            BPSearchCells.Add(new Cells { CellID = -1, CellTitle = "Clear" });
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
            // this function goes through the BPCellsID list and returns the cell number of the specified ID. 
            foreach (var item in BPCells)
            {
                if (ID == item.CellID) return item.CellTitle;
            }

            return "";


        }

        private int RetListIDFromBPID(int BPID)
        {
            for (int x = 0; x <= BPClass.Count-1; x++)
            {
                if (BPID == BPClass[x].BPID) return x;

            }
            return -1;
        }

        private void BPList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (BPList.SelectedIndex >= 0)
            {
                // update the info fields with the data from the selected index on BPList
                Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;

                textBPNumber.Text = selectedBreakpoint.BPID.ToString();

                comboBPCell.SelectedIndex = (BPCells.ToList().Exists(x => x.CellID == selectedBreakpoint.BPCell)) ? (ReturnIDFromDropDown(selectedBreakpoint.BPCellNumber)) : -1;

                textBPDescription.Text = selectedBreakpoint.BPDescription;
                dateBPStarted.SelectedDate = DateTime.Parse(selectedBreakpoint.BPStart);
                dateBPFinished.SelectedDate = DateTime.Parse(selectedBreakpoint.BPFinish);

            }


        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            textBPDescription.IsReadOnly = false;
            buttonSave.IsEnabled = true;
            dateBPStarted.IsEnabled = true;
            dateBPFinished.IsEnabled = true;

        }

        private void saveFile()
        {
            string fileoutput = "<breakpoints>\n";
            int currentID = 0;

            foreach (var item in BPCells)
            { 
                fileoutput += "\t<cell>\n";
                fileoutput += $"\t\t<id>{item.CellID}</id>\n";
                fileoutput += $"\t\t<title>{item.CellTitle}</title>\n";
                fileoutput += "\t</cell>\n";
                currentID++;
            }

            foreach (var item in BPClass)
            {
                fileoutput += "\t<breakpoint>\n";
                fileoutput += $"\t\t<id>{item.BPID}</id>\n";
                fileoutput += $"\t\t<cellid>{item.BPCell}</cellid>\n";
                fileoutput += $"\t\t<description>{item.BPDescription}</description>\n";
                fileoutput += $"\t\t<datestarted>{item.BPStart}</datestarted>\n";
                fileoutput += $"\t\t<datefinished>{item.BPFinish}</datefinished>\n";
                fileoutput += "\t</breakpoint>\n";
            }

            fileoutput += "</breakpoints>";
            System.IO.File.WriteAllLines(saveName, fileoutput.Split('\n'));
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
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
            buttonSave.IsEnabled = false;

        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            Cells selectedCell = comboSearchList.SelectedItem as Cells;

            int newCellID = (comboSearchList.SelectedIndex >= 0 && comboSearchList.SelectedIndex < BPSearchCells.Count()-1) ? selectedCell.CellID : 0;
            int newBPID = (BPClass.Count > 0) ? BPClass[BPClass.Count - 1].BPID + 1 : 1;



            BPClass.Add(new Breakpoint { BPID = newBPID, BPCell = newCellID, BPDescription = "New Breakpoint Description", BPStart = DateTime.Today.ToString("MM/dd/yyyy"), BPFinish = DateTime.Now.AddMonths(3).ToString("MM/dd/yyyy"), BPCellNumber = RetCellFromBPID(newCellID) });
            saveFile();

            UpdateListSearch();

            BPList.SelectedIndex = BPList.Items.Count - 1;
            BPList.Focus();

        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            Breakpoint selectedBreakpoint = BPList.SelectedItem as Breakpoint;


            BPClass.RemoveAt(RetListIDFromBPID(selectedBreakpoint.BPID));

            saveFile();
            UpdateListSearch();

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuEditCells_Click(object sender, RoutedEventArgs e)
        {
            new CellsEdit(BPCells).Show();
        }


        private void UpdateListSearch()
        { 


            if (comboSearchList.SelectedIndex >= 0 && comboSearchList.SelectedIndex <= BPSearchCells.Count - 2)
            {
                Cells selectedCell = comboSearchList.SelectedItem as Cells;
                listSearch = new ObservableCollection<Breakpoint>(BPClass.ToList().FindAll(x => x.BPCell == selectedCell.CellID));

            } else
            {
                listSearch = new ObservableCollection<Breakpoint>(BPClass);
            }

            AddListItems();

        }

        private void ComboSearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateListSearch();
        }

    }
}
