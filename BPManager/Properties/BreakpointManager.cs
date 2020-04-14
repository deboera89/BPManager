using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BPManager.Properties
{


    class BreakpointManager
    {


        public string saveName = "datafile.xml";


        public ObservableCollection<Breakpoint> BPClass;
        public ObservableCollection<Cells> BPCells;
        public ObservableCollection<Cells> BPSearchCells;
        public ObservableCollection<Breakpoint> listSearch;
        bool finishedLoading = false;


        public BreakpointManager()
        {

        }

        public void start()
        {

            BPCells = new ObservableCollection<Cells> { };
            BPSearchCells = new ObservableCollection<Cells> { };

            BPCells.CollectionChanged += BPCells_CollectionChanged;

            BPClass = new ObservableCollection<Breakpoint> { };

            // sets the BPClass from the loaded xml save file
            BPClass = LoadXML(BPClass);

            // listSearch is the itemsource for the for hte breakpoint list on the main form.
            listSearch = new ObservableCollection<Breakpoint>(BPClass);

            AddShowAllToList();




            // after this bool is true we know BPCells and BPClass is filled so can start checking if there is changes made to them.
            finishedLoading = true;


        }


        private void BPCells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // update the GUI if Cells are added or deleted from the Edit Cells page

            //Add listener for each item on PropertyChanged event
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                BPSearchCells.RemoveAt(BPSearchCells.Count - 1);
                BPSearchCells.RemoveAt(e.OldStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (finishedLoading) BPSearchCells.RemoveAt(BPSearchCells.Count - 1);
                BPSearchCells.Add(BPCells[e.NewStartingIndex]);
            }



                if (finishedLoading)
            {
                SaveFile();
                UpdateCellsList();
                AddShowAllToList();

            }


        }



        public int ReturnIDFromDropDown(string cell)
            {
                // this function goes through each item in the BPCells list to see if input string cell title matches any celltitles in the list
                // it will return the index of the found cell in the list
                //
                // this fixes errors where users delete cells from the middle of the cell list, the CellID will not always match the Index of the item in the combo boxes.
                //

                for (int x = 0; x <= BPCells.Count - 1; x++)
                {
                    if (BPCells[x].CellTitle.ToString() == cell) return x;
                }

                return -1;
            }

            private Cells ReturnCellFromBreakpointCellID(Breakpoint bp)
            {

            foreach (var item in BPCells)
            {
                if (item.CellID == bp.BPCell) return item;
            }
            return null;
            }
            public string RetCellFromCID(int ID)
            {
                // this function goes through the BPCellsID list and returns the cell string title of the specified ID. 
                foreach (var item in BPCells)
                {
                    if (ID == item.CellID) return item.CellTitle;
                }

                return "";


            }

            public int ReturnCellFromBreakpointCellID(int BPID)
            {
                // this function loops through the BPClass list and finds where the specified BPID is located and returns the Index number.
                for (int x = 0; x <= BPClass.Count - 1; x++)
                {
                    if (BPID == BPClass[x].BPID) return x;

                }
                return -1;
            }

            public void UpdateCellsList()
            {
                // mainly used if a cell is deleted, loop through the BPClass and update all the cell numbers (to empty if a cell was deleted with a breakpoint attached to that cell)
                for (int x = 0; x <= BPClass.Count - 1; x++)
                {
                    BPClass[x].BPCellNumber = RetCellFromCID(BPClass[x].BPCell);

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
                    Application.Current.Shutdown();
                }
            }

            public bool DeleteBreakpoint(Breakpoint selectedBreakpoint)
            {
                if (selectedBreakpoint == null) return false;
                BPClass.RemoveAt(ReturnCellFromBreakpointCellID(selectedBreakpoint.BPID));
                SaveFile();
                UpdateListSearch(null);
                return true;
            }

            public bool AddNewBreakpoint(Cells cell)
            {

            int newCellID = cell.CellID;

            // get the last BPID in the BPClass and add 1 to it to get new BPID, if there is no breakpoints in the list then set new BPID to 1.
            int newBPID = (BPClass.Count > 0) ? BPClass[BPClass.Count - 1].BPID + 1 : 1;


            BPClass.Add(new Breakpoint { BPID = newBPID, BPCell = newCellID, BPDescription = "New Breakpoint Description", BPStart = DateTime.Today.ToString("MM/dd/yyyy"), BPFinish = DateTime.Now.AddMonths(3).ToString("MM/dd/yyyy"), BPCellNumber = RetCellFromCID(newCellID) });

            SaveFile();

            UpdateListSearch(cell, true);

            return true;
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

                        newBreakpoint.BPCellNumber = RetCellFromCID(newBreakpoint.BPCell);
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


            public void SaveFile()
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

        public void UpdateListSearch(Cells selectedCell = null, Boolean add = false)
            {
            // if a selection is made from the filter/search combo box then update the BPList to show the selected Cell.
            // The last item in BPSearchCells is the "Clear" item to remove the filter.


            if (listSearch.Count < BPClass.Count && listSearch.Count > 0 && selectedCell == null && !add)
            {
                selectedCell = ReturnCellFromBreakpointCellID(listSearch[0]);
            }

            if (selectedCell != null && selectedCell.CellTitle != "Show All")
            {
                listSearch.Clear();
                var y = new ObservableCollection<Breakpoint>(BPClass.ToList().FindAll(x => x.BPCell == selectedCell.CellID));

                foreach (var item in y)
                {
                    listSearch.Add(item);
                }

            }
            else
            {
                // search criteria was removed so show all breakpoints

                listSearch.Clear();
                foreach (var item in BPClass)
                {
                    listSearch.Add(item);
                }

            }

        }

            private void AddShowAllToList()
            {

                BPSearchCells.Add(new Cells { CellID = -1, CellTitle = "Show All" });


        }


    }
}

