using BPManager.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BPManager.Classes
{
    [XmlRoot("BreakpointManager")]
    public class BPSaveClass
    {

        public List<Cells> CellsList;
        public List<Breakpoint> BreakpointList;


        public BPSaveClass()
        {

            BreakpointList = new List<Breakpoint>();
            CellsList = new List<Cells>();

        }

    }
}
