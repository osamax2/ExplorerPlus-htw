using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ExplorerPlus.API.Controls
{
    public partial class ListOpen : UserControl
    {
        public ListOpen()
        {
            InitializeComponent();
            try
            {
                XmlReader xmlFile;
                xmlFile = XmlReader.Create(@"C:\Users\osama\Desktop\ExplorerPlus-master\ExplorerPlus\bin\Debug\path.xml", new XmlReaderSettings());
                DataSet ds = new DataSet();
                ds.ReadXml(xmlFile);
                dataGridView1.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
    
}
