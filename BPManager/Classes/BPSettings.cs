using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BPManager.Classes
{
    public class BPSettings
    {
        private string settingsFile = "settings.xml";
        public string SaveFile;
        public bool isURL;

        public void saveSettings()
        {

            XmlSerializer serializer = new XmlSerializer(typeof(BPSettings));

            using (var writer = XmlWriter.Create(settingsFile))
            {
                serializer.Serialize(writer, this);
            }
        }
    }


}
