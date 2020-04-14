using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPManager.Properties
{
    public class Breakpoint : IEquatable<Breakpoint>
    {

        public int BPID { get; set; }
        public int BPCell { get; set; }
        public string BPDescription { get; set; }
        public string BPStart { get; set; }
        public string BPFinish { get; set; }
        public string BPCellNumber { get; set; }

        public bool Equals(Breakpoint other)
        {
            return false;
        }

        public override string ToString()
        {
            return "ID: " + BPID + "   CellID: " + BPCell + " Cell Number: " + BPCellNumber;
        }


    }
}
