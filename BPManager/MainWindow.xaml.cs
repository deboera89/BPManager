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

        ObservableCollection<Breakpoint> BPClass;
        ObservableCollection<Cells> BPCells;


        public ExitEventHandler()
        {
            InitializeComponent();
            start();

        }


        private void start()
        {

            BPCells = new ObservableCollection<Cells> { };
            BPClass = new ObservableCollection<Breakpoint> { };

            BPClass = LoadXML(BPClass);

            addListItems();


        }





        private ObservableCollection<Breakpoint> LoadXML(ObservableCollection<Breakpoint> BPClass)
        {

        bool insideBreakpoint = false;
        bool insideCell = false;

        Breakpoint newBreakpoint = new Breakpoint();
        Cells newCell = new Cells();



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

                    newBreakpoint.BPCellNumber = retCellFromBPID(newBreakpoint.BPCell);
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



        private void addListItems()
        {

            BPList.ItemsSource = BPClass;
            comboBPCell.ItemsSource = BPCells;

        }

        private int returnIDFromDropDown(string cell)
        {

            foreach (Cells item in BPCells)
            {
                if (cell == item.CellTitle.ToString()) return item.CellID;
            }
            return 0;


        }

        private string retCellFromBPID(int ID)
        {
            // this function goes through the BPCellsID list and returns the cell number of the specified ID. 
            foreach (var item in BPCells)
            {
                if (ID == item.CellID) return item.CellTitle;
            }

            return "";


        }

        private void BPList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (BPList.SelectedIndex >= 0)
            {
                // update the info fields with the data from the selected index on BPList

                textBPNumber.Text = BPClass[BPList.SelectedIndex].BPID.ToString();
                comboBPCell.SelectedIndex = BPClass[BPList.SelectedIndex].BPCell - 1;
                textBPDescription.Text = BPClass[BPList.SelectedIndex].BPDescription;
                dateBPStarted.SelectedDate = DateTime.Parse(BPClass[BPList.SelectedIndex].BPStart);
                dateBPFinished.SelectedDate = DateTime.Parse(BPClass[BPList.SelectedIndex].BPFinish);

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
            int selected = BPList.SelectedIndex;

            BPClass[BPList.SelectedIndex].BPCell = BPCells[comboBPCell.SelectedIndex].CellID;
            BPClass[BPList.SelectedIndex].BPDescription = textBPDescription.Text;
            BPClass[BPList.SelectedIndex].BPStart = DateTime.Parse(dateBPStarted.SelectedDate.ToString()).ToString("MM/dd/yyyy");
            BPClass[BPList.SelectedIndex].BPFinish = DateTime.Parse(dateBPFinished.SelectedDate.ToString()).ToString("MM/dd/yyyy"); 
            BPClass[BPList.SelectedIndex].BPCellNumber = retCellFromBPID(BPClass[BPList.SelectedIndex].BPCell);

            saveFile();
            start();


            buttonSave.IsEnabled = false;
            BPList.SelectedIndex = selected;


        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {

            BPClass.Add(new Breakpoint { BPID = BPClass[BPClass.Count-1].BPID+1, BPCell = 0, BPDescription = "New Breakpoint Description", BPStart = DateTime.Today.ToString("MM/dd/yyyy"), BPFinish = DateTime.Now.AddMonths(3).ToString("MM/dd/yyyy"), BPCellNumber = retCellFromBPID(0) });
            saveFile();

            BPList.SelectedIndex = BPClass.Count - 1;
            BPList.Focus();

        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            BPClass.RemoveAt(BPList.SelectedIndex);

            saveFile();
            addListItems();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ComboBPCell_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MenuEditCells_Click(object sender, RoutedEventArgs e)
        {
            new CellsEdit(BPCells).Show();
        }
    }
}
