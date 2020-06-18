using BPManager.Classes;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace BPManager.Properties
{
    
    public class BreakpointManager
    {


        private string _saveName = "test.xml";

        public ObservableCollection<Breakpoint> _bpClass { get; private set; }
        public ObservableCollection<Breakpoint> _listSearch { get; private set; }
        public ObservableCollection<Cells> _bpCells { get; private set; }
        public ObservableCollection<Cells> _bpSearchCells { get; private set; }
        public BPSettings _settings;

        private bool finishedLoading = false;

        public BreakpointManager()
        {

            _settings = LoadSettings();

            _saveName = _settings.SaveFile;
            _bpCells = new ObservableCollection<Cells> { };
            _bpSearchCells = new ObservableCollection<Cells> { };


            AddShowAllToList();



            _bpClass = new ObservableCollection<Breakpoint> { };
            if (!LoadXML())
            {
                // error if xml loaded incorrectly

                MessageBox.Show("There was an error loading the data file", "Data file corrupt");
                Application.Current.Shutdown();

            }

            // listSearch is the itemsource for the for the breakpoint list on the main form.

            _listSearch = new ObservableCollection<Breakpoint>(_bpClass);

            // after this bool is true we know BPCells and BPClass is filled so can start checking if there is changes made to them.

            finishedLoading = true;

        }

        private BPSettings LoadSettings()
        {

            string settingsLocation = "settings.xml";

            // reads XML datafile and sets up the BPClass and BPCells lists. 

            if (!File.Exists(settingsLocation))
            {
                // No save data file found, call noSaveFile function to create a blank.
                //noSaveFile();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(BPSettings));

            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

            BPSettings bps;

            using (Stream reader = new FileStream(settingsLocation, FileMode.Open))
            {
                bps = (BPSettings)serializer.Deserialize(reader);
            }

            return bps;

        }


        private void BPCells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // update the GUI if Cells are added or deleted from the Edit Cells page
            //Add listener for each item on PropertyChanged event            

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                _bpSearchCells.RemoveAt(e.OldStartingIndex+1);
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                _bpSearchCells.Add(_bpCells[e.NewStartingIndex]);
            }



            if (finishedLoading)
            {
                // only save and update the cell list if a cell is added or removed from the edit cells page, not during the initial loading.
                if (!_settings.isURL)
                {
                    SaveFile();
                }
                UpdateCellsList();

            }

        }



        public int ReturnIDFromDropDown(string cell)
        {
            // this function goes through each item in the BPCells list to see if input string cell title matches any celltitles in the list
            // it will return the index of the found cell in the list
            //
            // this fixes errors where users delete cells from the middle of the cell list, the CellID will not always match the Index of the item in the combo boxes.
            //

            for (var x = 0; x <= _bpCells.Count - 1; x++)
            {
                if (_bpCells[x].CellTitle.ToString() == cell) return x;
            }

            return -1;
        }

        private Cells ReturnCellFromBreakpointCellID(Breakpoint bp)
        {
            // returns a Cells item where the CellID matches the Cell id in the breakpoint

            foreach (var item in _bpCells)
            {
                if (item.CellID == bp.BPCell) return item;
            }

            return null;
        }


        private string RetCellFromCID(int ID)
        {
            // this function goes through the BPCells ID list and returns the string title of the specified ID. 

            foreach (var item in _bpCells)
            {
                if (ID == item.CellID) return item.CellTitle;
            }

            return String.Empty;

        }

        private int ReturnCellFromBreakpointCellID(int BPID)
        {
            // this function loops through the BPClass list and finds where the specified BPID is located and returns the Index number in the list.

            for (var x = 0; x <= _bpClass.Count - 1; x++)
            {
                if (BPID == _bpClass[x].BPID) return x;
            }

            return -1;
        }

        private void UpdateCellsList()
        {
            // loop through the BPClass and update all the cell numbers

            for (var x = 0; x <= _bpClass.Count - 1; x++)
            {
                _bpClass[x].BPCellNumber = RetCellFromCID(_bpClass[x].BPCell);
            }

        }

        private void noSaveFile()
        {
            // small function to create a blank XML datafile if the program can't find it at start up. 

            MessageBoxResult dialogResult = MessageBox.Show("No data file found, do you want to create a new data file?", "No Data File", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                SaveFile();
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                Application.Current.Shutdown();
            }

        }

        public bool DeleteBreakpoint(Breakpoint selectedBreakpoint)
        {
            // remove the referenced breakpoint from the BPClass list, save the file and update the searchlist. 

            if (selectedBreakpoint == null) return false;
            _bpClass.RemoveAt(ReturnCellFromBreakpointCellID(selectedBreakpoint.BPID));
            SaveFile();
            UpdateListSearch();
            return true;

        }

        public bool AddNewBreakpoint(Cells cell)
        {
            // adds a new breakpoint to BPCLass
            // the Cells reference passed is the one selected in the Search/Filter combo box.
            // if Show All is selected the CellID will be -1, No Cell title

            int newCellID = cell.CellID;

            // get the last BPID in the BPClass and add 1 to it to get new BPID, if there is no breakpoints in the list then set new BPID to 1.

            int newBPID = (_bpClass.Count > 0) ? _bpClass[_bpClass.Count - 1].BPID + 1 : 1;
            _bpClass.Add(new Breakpoint { BPID = newBPID, BPCell = newCellID, BPDescription = "New Breakpoint Description", BPStart = DateTime.Today.ToString("MM/dd/yyyy"), BPFinish = DateTime.Now.AddMonths(3).ToString("MM/dd/yyyy"), BPCellNumber = RetCellFromCID(newCellID) });
            SaveFile();
            UpdateListSearch(cell, true);

            return true;

        }

        public bool EditBreakpoint(Cells selectedCell, Breakpoint selectedBreakpoint, string description, string started, string finish, int CellIndex)
        {
            // Edits the breakpoint in BPClass

            int BPClassIndex = ReturnCellFromBreakpointCellID(selectedBreakpoint.BPID);

            _bpClass[BPClassIndex].BPCell = _bpCells[CellIndex].CellID;
            _bpClass[BPClassIndex].BPDescription = description;
            _bpClass[BPClassIndex].BPStart = DateTime.Parse(started).ToString("MM/dd/yyyy");
            _bpClass[BPClassIndex].BPFinish = DateTime.Parse(finish).ToString("MM/dd/yyyy");
            _bpClass[BPClassIndex].BPCellNumber = RetCellFromCID(_bpClass[BPClassIndex].BPCell);

            SaveFile();

            UpdateListSearch(selectedCell);
            return true;

        }


        public bool LoadXML()
        {
            

            // reads XML datafile and sets up the BPClass and BPCells lists. 

            if (!File.Exists(_settings.SaveFile) && !_settings.isURL)
            {
                // No save data file found, call noSaveFile function to create a blank.

                noSaveFile();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(BPSaveClass));

            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

            BPSaveClass bpm;


            if (_settings.SaveFile.Substring(0, 7) == "http://")
            {
                var textFromFile = (new WebClient()).DownloadString(_settings.SaveFile);
                byte[] byteArray = Encoding.ASCII.GetBytes(textFromFile);
                MemoryStream stream = new MemoryStream(byteArray);
                StreamReader reader = new StreamReader(stream);
                bpm = (BPSaveClass)serializer.Deserialize(reader);
            }
            else
            {
                using (Stream reader = new FileStream(_settings.SaveFile, FileMode.Open))
                {
                    bpm = (BPSaveClass)serializer.Deserialize(reader);
                }
            }



            _bpClass = new ObservableCollection<Breakpoint>(bpm.BreakpointList);

            _bpCells = new ObservableCollection<Cells>();
            _bpCells.CollectionChanged += BPCells_CollectionChanged;

            foreach (Cells item in bpm.CellsList)
            {
                _bpCells.Add(item);
            }
            

            UpdateCellsList();

            return true;

        }


        private void SaveFile()
        {
             // save the current BPClass and BPCells to the xml file.

            BPSaveClass bpSave = new BPSaveClass();

            bpSave.BreakpointList = _bpClass.ToList<Breakpoint>();
            bpSave.CellsList = _bpCells.ToList<Cells>();

            XmlSerializer serializer = new XmlSerializer(typeof(BPSaveClass));

            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

            using (var writer = XmlWriter.Create(_settings.SaveFile))
            {
                serializer.Serialize(writer, bpSave);
            }

        }

        private void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
        }

        private void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        public void UpdateListSearch(Cells selectedCell = null, Boolean addedit = false)
        {
            // if a selection is made from the filter/search combo box then update the BPList to show the selected Cell.
            // The first item in BPSearchCells is the "Clear" item to remove the filter.
            // if no selectedCell is passed, check if the listSearch is smaller than the BPClass to determine if a search/filter was being used.

            if (_listSearch.Count <= _bpClass.Count && _listSearch.Count > 0 && selectedCell == null && !addedit)
            {   
                // if a filter is being used get the cell from the CellID from the first item in the list (as all list items should be from the same cell)

                selectedCell = ReturnCellFromBreakpointCellID(_listSearch[0]);
            }

            if (selectedCell != null && selectedCell.CellTitle != "Show All")
            {
                _listSearch.Clear();
                var y = new ObservableCollection<Breakpoint>(_bpClass.ToList().FindAll(x => x.BPCell == selectedCell.CellID));

                foreach (var item in y)
                {
                    _listSearch.Add(item);
                }

            }
            else
            {
                // search criteria was removed so show all breakpoints

                _listSearch.Clear();
                foreach (var item in _bpClass)
                {
                    _listSearch.Add(item);
                }

            }

        }

        private void AddShowAllToList()
        {
            // Adds "Show All" selection to the start of the search combo list itemssource

            _bpSearchCells.Add(new Cells { CellID = -1, CellTitle = "Show All" });
        }

    }

}

