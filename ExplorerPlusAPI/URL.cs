using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace ExplorerPlus.API
{
   
   public class URL
    {
        [XmlAttribute]
        public string FileUrl;
        public int views;

        public void addview()
        {
            views++;
        
        }

        public URL()
        {
        }
        ~URL() { }


    }
}
