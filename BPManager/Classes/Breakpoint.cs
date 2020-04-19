using System.Xml.Serialization;

namespace BPManager.Properties
{
    [XmlType("Breakpoint")]
    public class Breakpoint
    {
        [XmlElement("ID")]
        public int BPID { get; set; }

        [XmlElement("CellID")]
        public int BPCell { get; set; }

        [XmlElement("Description")]
        public string BPDescription { get; set; }

        [XmlElement("DateDtarted")]
        public string BPStart { get; set; }

        [XmlElement("DateFinished")]
        public string BPFinish { get; set; }

        [XmlIgnore]
        public string BPCellNumber { get; set; }

    }
}
