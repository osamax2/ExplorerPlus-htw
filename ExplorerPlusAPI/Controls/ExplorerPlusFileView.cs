using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ExplorerPlus.API.Controls
{
    public partial class ExplorerPlusFileView : UserControl
    {
        private static string fileName="path.xml";
        private static views viewload;
        private string _selectedpath = "";
        private List<string> recentpaths; //Liste mit den zuletzt ausgewählten Pfaden
        private List<string> undolist = null;
        private int undopos = 0;

        //Events
        public event ExplorerPlusFilesystemHandlerEx SelectedFileClick; //Auch für Ordner gültig
        public event ExplorerPlusFilesystemHandlerEx SelectedFileDoubleClickEx;
        public event ExplorerPlusFilesystemHandler SelectedFileDoubleClick;
        public event ExplorerPlusFilesystemHandler LoadedPath; //Nur verwenden beim Laden des Ordnerinhalts

        public ExplorerPlusFileView()
        {
            InitializeComponent();
            //Erster Eintrag in Liste eintragen
            recentpaths = new List<string>(10);
            recentpaths.Add("");
            Loadviews();
            ShowPathContent();
        }

        //viwes 
        [XmlRootAttribute("views", Namespace = "http://www.cpandl.com",
IsNullable = false)]
        public class views
        {
            [XmlArrayAttribute("Urls")]
            public List<URL> Urls;

            private static int count = 0;

            public static void reset()
            {
                views temp = new views();
                temp.Urls = new List<URL>();

                    // Create an instance of the XmlSerializer class;
                    // specify the type of object to serialize.
                    XmlSerializer serializer =
                    new XmlSerializer(typeof(views));
                    TextWriter writer = new StreamWriter(ExplorerPlusFileView.fileName);
                // Serialize the urls, and close the TextWriter.
                
                    serializer.Serialize(writer, temp);
                    writer.Close();
                    viewload.Urls = temp.Urls;
                    



            }
            public views()
            {
            }
            public void Insert(URL temp)
            {
                Urls[count++] = temp;
            }

        }


        public void Loadviews()
        {
            // Create an instance of the XmlSerializer class;
            // specify the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(views));
            /* If the XML document has been altered with unknown 
            nodes or attributes, handle them with the 
            UnknownNode and UnknownAttribute events.*/
            serializer.UnknownNode += new
            XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new
            XmlAttributeEventHandler(serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.
            FileStream fs = new FileStream(fileName, FileMode.Open);
            // Declare an object variable of the type to be deserialized.

            /* Use the Deserialize method to restore the object's state with
            data from the XML document. */
            viewload = (views)serializer.Deserialize(fs);

            fs.Close();
        }

        private void serializer_UnknownNode
        (object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        private void serializer_UnknownAttribute
        (object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " +
            attr.Name + "='" + attr.Value + "'");
        }

        public string SelectedPath
        {
            get { return _selectedpath; }
            set
            {
                _selectedpath = value;
                ShowPathContent();
            }
        }

        public int ActualUndoPos
        {
            get { return undopos; }
        }

        public int FileListCount
        {
            get { return lvfiles.Items.Count; }
        }

        public void ReloadList()
        {
            //Diese Methode soll die Möglichkeit bieten, einfach ShowPathContent() aufzurufen.
            ShowPathContent();
        }
        public void sevefile()
        {
            // Create an instance of the XmlSerializer class;
            // specify the type of object to serialize.
            XmlSerializer serializer =
            new XmlSerializer(typeof(views));
            TextWriter writer = new StreamWriter(fileName);
            // Serialize the urls, and close the TextWriter.
            serializer.Serialize(writer, viewload);
            writer.Close();


        }
        public void setView(string Path)
        {
            bool check= false;
            foreach(URL temp in viewload.Urls)
            {
                if (temp.FileUrl == Path)
                {
                    temp.views++;
                    check = true;
                    break;
                }
            }
            if(!check)
            {
                URL temp = new URL();
                temp.FileUrl = Path;
                temp.views = 1;
                viewload.Urls.Add(temp);
            }
            sevefile();

        }

        public int getView(string Path)
        {
            // Create an instance of the XmlSerializer class;
            // specify the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(views));
            /* If the XML document has been altered with unknown 
            nodes or attributes, handle them with the 
            UnknownNode and UnknownAttribute events.*/
            serializer.UnknownNode += new
            XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new
            XmlAttributeEventHandler(serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.
            
            FileStream fs = new FileStream(fileName, FileMode.Open);
            // Declare an object variable of the type to be deserialized.
            views po;
            /* Use the Deserialize method to restore the object's state with
            data from the XML document. */
            po = (views)serializer.Deserialize(fs);
            // Read the list of ordered items.
            List<URL> items = po.Urls;
            foreach (URL oi in items)
            {
                if (oi.FileUrl == Path)
                {
                    fs.Close();
                    return oi.views;
                }
            }
            fs.Close();
            return 0;
        }

        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        private void ShowPathContent()
        {
            //ListView leeren
            lvfiles.Items.Clear();

            //ImageListen vorbereiten. Es werden jeweils 2 erstellt, einmal für die SmallIcons und
            //einmal für die LargeIcons
            ImageList sil = new ImageList();
            ImageList lil = new ImageList();
            sil.ColorDepth = ColorDepth.Depth32Bit;
            sil.ImageSize = new Size(16, 16);
            lil.ColorDepth = ColorDepth.Depth32Bit;
            lil.ImageSize = new Size(48, 48);
            sil.Images.Add(FilesystemIcons.ICON_FILE_16x);
            lil.Images.Add(FilesystemIcons.ICON_FILE_32x);
            lvfiles.SmallImageList = sil;
            lvfiles.LargeImageList = lil;

            //Wenn der Pfad leer ist, sollen die Drives angezeigt werden. Wenn aber ein Pfad enthalten ist
            //soll der Inhalt des Pfades (Ordner) angezeigt werden
            if (_selectedpath == "")
            {
                //View hier
               
                //Nun werden alle Drives geladen und angezeigt
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {

        
                    //Icon ermitteln
                    sil.Images.Add(FilesystemIcons.GetSmallIcon(string.Concat(drive.Name.Substring(0, 2) + @"\")));
                    lil.Images.Add(FilesystemIcons.GetLargeIcon(string.Concat(drive.Name.Substring(0, 2) + @"\")));
                    //Jetzt muss geschaut werden, ob der Drive aktiv ist oder nicht und danach entschieden werden
                    if (drive.IsReady)
                    {
                        
                       ListViewItem lvi = new ListViewItem(string.Concat(drive.Name.Substring(0, 2), " ", DriveFunctions.GetVolumeLabel(Convert.ToChar(drive.Name.Substring(0, 1)))), sil.Images.Count - 1);
                        lvi.SubItems.Add(""); //Dummy
                        //Der Typ wird anhand des DriveType angegeben. Bei Size kommt der maximale Speicherplatz
                        if (drive.DriveType == DriveType.Fixed)
                        {
                            //Wenn es eine SSD ist, soll das auch so angegeben werden
                            if (DriveFunctions.IsSSD(Convert.ToChar(drive.Name.Substring(0, 1))))
                            {
                                lvi.SubItems.Add(DriveFunctions.DRIVE_VOLUMELABEL_STD_SSD_DESC);
                            }
                            else
                            {
                                lvi.SubItems.Add(DriveFunctions.DRIVE_VOLUMELABEL_STD_HDD_DESC);
                            }
                        }
                        else if (drive.DriveType == DriveType.CDRom)
                            lvi.SubItems.Add(DriveFunctions.DRIVE_VOLUMELABEL_STD_OPT);
                        else if (drive.DriveType == DriveType.Removable)
                            lvi.SubItems.Add(DriveFunctions.DRIVE_VOLUMELABEL_STD_EXT);
                        else if (drive.DriveType == DriveType.Network)
                            lvi.SubItems.Add(DriveFunctions.DRIVE_VOLUMELABEL_STD_NET);
                        else
                            lvi.SubItems.Add("Unbekannter Laufwerkstyp");

                        lvi.SubItems.Add(ExtraFunctions.UnitChange(drive.TotalSize));


                        lvi.SubItems.Add(getView(drive.Name).ToString());
                        lvfiles.Items.Add(lvi);
                    }
                    else
                    {
                        ListViewItem lvi = new ListViewItem(string.Concat(drive.Name.Substring(0, 2), @"\"), sil.Images.Count - 1);
                        lvi.SubItems.Add(""); //Dummy
                        //Der Typ wird anhand des DriveType angegeben. Bei Size kommt der maximale Speicherplatz
                        if (drive.DriveType == DriveType.Fixed)
                            lvi.SubItems.Add(DriveFunctions.DRIVE_VOLUMELABEL_STD_HDD);
                        else if (drive.DriveType == DriveType.CDRom)
                            lvi.SubItems.Add(DriveFunctions.DRIVE_VOLUMELABEL_STD_OPT);
                        else if (drive.DriveType == DriveType.Removable)
                            lvi.SubItems.Add(DriveFunctions.DRIVE_VOLUMELABEL_STD_EXT);
                        else if (drive.DriveType == DriveType.Network)
                            lvi.SubItems.Add(DriveFunctions.DRIVE_VOLUMELABEL_STD_NET);
                        else
                            lvi.SubItems.Add("Unbekannter Laufwerkstyp");
                        lvi.SubItems.Add(getView(drive.Name).ToString());
                        lvfiles.Items.Add(lvi);
                    }
                }
            }
            else
            {

                //hier implemintiere mich meine code


                DirectoryInfo dirinfo = new DirectoryInfo(_selectedpath);

                if(_selectedpath== @"C:\" || _selectedpath== @"D:\")
                    setView(_selectedpath);
                else if (_selectedpath.EndsWith(@"\"))
                        setView(_selectedpath.Substring(0, _selectedpath.Length - 1));
                    else
                         setView(_selectedpath);
                //Zuerst kommen die Directories
                foreach (DirectoryInfo d in dirinfo.GetDirectories())
                {
                    
                    try
                    {
                        sil.Images.Add(FilesystemIcons.GetSmallIcon(d.FullName));
                        lil.Images.Add(FilesystemIcons.GetLargeIcon(d.FullName));
                    }
                    catch //Wenn das Icon über GetSmallIcon nicht genommen wird
                    {
                        sil.Images.Add(FilesystemIcons.ICON_DIRECTORY_16x); //Ersatzicon
                        lil.Images.Add(FilesystemIcons.ICON_DIRECTORY_32x); //Ersatzicon
                    }
                    ListViewItem lvi = new ListViewItem(d.Name, sil.Images.Count - 1);
                    lvi.SubItems.Add(d.LastWriteTime.ToString());
                    lvi.SubItems.Add(Extra.StringResource.GetStringResourceFromFile("@shell32.dll,-10152").ToString());
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add(getView(d.FullName).ToString());
                    lvfiles.Items.Add(lvi);
                }

                //Jetzt kommen die Files. Hier kann man denselben Code wie in ExplorerPlusFileView verwenden
                //Nun wird die Liste gefüllt
                foreach (FileInfo file in dirinfo.GetFiles())
                {
                    //Anhand der Erweiterung schauen, ob es eine Verknüpfung ist oder nicht
                    try
                    {
                        try
                        {
                            if (file.Extension == ".lnk")
                            {
                                sil.Images.Add(FilesystemIcons.GetSmallIcon(FileFunctions.GetShortcutPath(file.FullName)));
                                lil.Images.Add(FilesystemIcons.GetLargeIcon(FileFunctions.GetShortcutPath(file.FullName)));
                            }
                            else
                            {
                                sil.Images.Add(FilesystemIcons.GetSmallIcon(file.FullName));
                                lil.Images.Add(FilesystemIcons.GetLargeIcon(file.FullName));
                            }
                        }
                        catch
                        {
                            sil.Images.Add(FilesystemIcons.GetIconByExtension_x16(file.Extension));
                            lil.Images.Add(FilesystemIcons.GetIconByExtension_x32(file.Extension));
                        }
                        ListViewItem lvi = new ListViewItem(file.Name, sil.Images.Count - 1);
                        lvi.SubItems.Add(file.LastWriteTime.ToString());
                        lvi.SubItems.Add(FileFunctions.GetFileTypeDescription(file.Extension));

                        //Wenn die dateigröße unter 1024 Bytes ist, soll stattdessen 1 KB ausgegeben werden
                        if (file.Length < 1024)
                        {
                            lvi.SubItems.Add("1 KB");
                        }
                        else
                        {
                            lvi.SubItems.Add(ExtraFunctions.GetFileSizeKB(Convert.ToDouble(file.Length)));
                        }
                        
                        lvi.SubItems.Add(getView(file.FullName).ToString());
                        lvfiles.Items.Add(lvi);
                    }
                    catch (UnauthorizedAccessException)
                    { //Geht nicht, da kein Zugriff
                    }
                    catch
                    {
                        ListViewItem lvi = new ListViewItem(file.Name, 0);
                        lvi.SubItems.Add(file.LastWriteTime.ToString());
                        lvi.SubItems.Add(FileFunctions.GetFileTypeDescription(file.Extension));

                        //Wenn die dateigröße unter 1024 Bytes ist, soll stattdessen 1 KB ausgegeben werden
                        if (file.Length < 1024)
                        {
                            lvi.SubItems.Add("1 KB");
                        }
                        else
                        {
                            lvi.SubItems.Add(ExtraFunctions.GetFileSizeKB(Convert.ToDouble(file.Length)));
                        }
                        lvi.SubItems.Add(getView(file.FullName).ToString());
                        lvfiles.Items.Add(lvi);
                    }
                }
            }

            LoadedPath?.Invoke(_selectedpath);
        }

        private void lvfiles_Click(object sender, EventArgs e)
        {
            string p = _selectedpath + @"\" + lvfiles.SelectedItems[0].Text; //Pfad zur Exe erhalten
            if (_selectedpath == "")
            {
                p = lvfiles.SelectedItems[0].Text;
                if (SelectedFileClick != null)
                    SelectedFileClick(p,ENTRY_TYPE.Drive );
            }
            else
            {
                if (lvfiles.SelectedItems[0].SubItems[2].Text == Extra.StringResource.GetStringResourceFromFile("@shell32.dll,-10152"))
                {
                    if (SelectedFileClick != null)
                        SelectedFileClick(p, ENTRY_TYPE.Directory);
                }
                else
                {
                    if (SelectedFileClick != null)
                        SelectedFileClick(p, ENTRY_TYPE.File );
                }
            }
            
            
        }

        private void lvfiles_DoubleClick(object sender, EventArgs e)
        {
            undopos = 0;
            undolist = null;
            //Das Event, was beim Doppelklick aufgeführt wird, wird danach entschieden, ob es ein Ordner ist oder eine Exe. Wenn man bei "" war, wird sofort der Root-Directory geöffnet
            if (_selectedpath == "")
            {
                _selectedpath = lvfiles.SelectedItems[0].Text.Substring(0, 2) + @"\"; //Pfad für Root einfügen
                AddRecentPath(_selectedpath);
                ShowPathContent();
                SelectedFileDoubleClickEx?.Invoke(_selectedpath,ENTRY_TYPE.Drive);
            }
            else
            {
                if (lvfiles.SelectedItems[0].SubItems[2].Text == Extra.StringResource.GetStringResourceFromFile("@shell32.dll,-10152")) //Wenn es ein ordner ist, soll im Ordner hochgegangen werden
                {
                    _selectedpath += lvfiles.SelectedItems[0].Text + @"\"; //Pfad um Ordner erweitern
                    AddRecentPath(_selectedpath);
                    ShowPathContent();
                    SelectedFileDoubleClickEx?.Invoke(_selectedpath, ENTRY_TYPE.Directory);
                }
                else
                {

                    
                    string p = _selectedpath + @"\" + lvfiles.SelectedItems[0].Text; //Pfad zur Exe erhalten
                    SelectedFileDoubleClickEx?.Invoke(p, ENTRY_TYPE.File);
                    setView(_selectedpath + lvfiles.SelectedItems[0].Text);
                }
            }         
        }

        private void AddRecentPath(string recentpath)
        {
            //Bei diesem Verfahren werden alle Einträge nach hinten geschoben. Wenn der 10. Undo durchgeführt wurde
            //wird dieser blockiert
            if (recentpaths.Count == 10)
                recentpaths.RemoveAt(9); //letzten entfernen

            recentpaths.Insert(0, recentpath);
        }

        public void Undo()
        {
            //Jedes Mal, wenn der Button gedrückt wird, wird undopos um 1 erhöht (bis er auf 9 ist)
            //Eine zweite Liste läuft mit und wenn ein Ordner in der Liste ausgewählt wird oder ein Pfad eingegeben wird, dann soll die Liste
            //gecleart werden. Bei jedem Sprung wird auch die Recentpaths-Liste weiter geführt
            if (undopos == 0 && undolist == null)
            {
                undolist = new List<string>(10);
                //Liste kopieren
                for (int i = 0; i < recentpaths.Count; i++)
                    undolist.Add(recentpaths[i]);

                undopos++;
                _selectedpath = undolist[undopos];
                AddRecentPath(_selectedpath);
                ShowPathContent();
            }
            else
            {
                if (undopos < 9)
                    undopos++;
                _selectedpath = undolist[undopos];
                AddRecentPath(_selectedpath);
                ShowPathContent();
            }
        }

        public void Redo()
        {
            if (undopos > 0)
                undopos--;
            _selectedpath = undolist[undopos];
            AddRecentPath(_selectedpath);
            ShowPathContent();
        }

        public void Delete()
        {
            if (lvfiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Wählen Sie Elemente aus, die gelöscht werden sollen");
                return;
            }
            else
            {
                if (lvfiles.SelectedItems.Count == 1)
                {
                    if (MessageBox.Show("Soll das 1 Element wirklich gelöscht werden?", "Warnung", MessageBoxButtons.YesNo) == DialogResult.No)
                        return;
                }
                else
                    if (MessageBox.Show("Sollen die " + lvfiles.SelectedItems.Count.ToString() + " Elemente wirklich gelöscht werden?", "Warnung", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            //Löschvorgang beginnt.
            for (int i = lvfiles.SelectedItems.Count - 1; i >= 0; i--)
            {
                try //Manche Dateien verweigern ggf. einen Löschvorgang
                {
                    if (lvfiles.SelectedItems[i].SubItems[2].Text == Extra.StringResource.GetStringResourceFromFile("@shell32.dll,-10152"))
                        Directory.Delete(_selectedpath + lvfiles.SelectedItems[i].Text);
                    else
                        File.Delete(_selectedpath + lvfiles.SelectedItems[i].Text);
                    lvfiles.SelectedItems[i].Remove(); //Nachladen verhindern
                }
                catch
                {
                }
            }
        }

        public void CreateFolder(string foldername)
        {
            //Ein DirectoryInfo-Objekt erstellen, damit der Ordner erstellt werden kann
            DirectoryInfo info = new DirectoryInfo(_selectedpath);
            info.CreateSubdirectory(foldername); //Ordner erstellen
            ShowPathContent();
        }
    }
}
