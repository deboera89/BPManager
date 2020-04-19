using System.Xml.Serialization;

namespace BPManager.Properties
{
    [XmlType("Cell")]
    public class Cells
    {

        [XmlElement("ID")]
        public int CellID { get; set; }

        [XmlElement("Title")]
        public string CellTitle { get; set; }

    }
}
