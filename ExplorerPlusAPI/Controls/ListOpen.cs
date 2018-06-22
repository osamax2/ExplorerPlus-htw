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


        private void Readxml()
        {
            try
            {
                XmlReader xmlFile;
                xmlFile = XmlReader.Create(@"C:\Users\osama\Desktop\ExplorerPlus-master\ExplorerPlus\bin\Debug\path.xml", new XmlReaderSettings());
                DataSet ds = new DataSet();
                ds.ReadXml(xmlFile);
                if (ds.Tables.Count > 1)
                {
                    dataGridView1.DataSource = ds.Tables[1];
                    dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
                }
                else
                    dataGridView1.DataSource = ds.Tables[0];


                
                xmlFile.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        public override void Refresh()
        {
            base.Refresh();
            Readxml();
        }

        public ListOpen()
        {
            InitializeComponent();
            Readxml();
        }
    }
    
}
