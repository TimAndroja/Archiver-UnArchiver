using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Linq;
using System.Collections;
using System.Data;
using SharpCompress.Readers;
using SharpCompress.Archives.Zip;
using SharpCompress.Archives;
using SharpCompress.Common;
using SevenZip;
using SharpCompress.Writers;

namespace Archiver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    public partial class MainWindow : Window
    {


        public class DGContent
        {

            public BitmapImage icon { get; set; }
            public string Name { get; set; }
            public string Extension { get; set; }

            public long Length { get; set; }

            public DateTime LastWriteTime { get; set; }

            public string FullPath { get; set; }

            public bool Compressed { get; set; }

            public bool Folder { get; set; }
          
           
        }

        //CONTENT###########################
        public List<DGContent> content = new List<DGContent>();
        public int depth = 0;
        public string dir;
        public List<DGContent> MainContent;
        public List<DGContent> Compressed_content = new List<DGContent>();
        public bool CompressDG = false;


        public MainWindow()
        {
            InitializeComponent();
            b_back.IsEnabled = false;
            b_main.IsEnabled = false;
            



        }
        //CLONE LIST

        public void MakeCompressedFile()
        {
            string[] pot = tbcompress.Text.Split('.');
            string koncnica = pot[pot.Length - 1].ToLower();
            
                using (var archive = ZipArchive.Create())
                {
                    foreach (DGContent item in content)
                    {
                        if (item.Folder)
                        {
                            archive.AddAllFromDirectory(item.FullPath);
                        }
                        else
                        {
                            FileInfo f = new FileInfo(item.FullPath);
                            archive.AddEntry(item.Name, f);
                        }
                    }

                    
                        archive.SaveTo(tbcompress.Text, CompressionType.Deflate);

                }


            

        }


        public BitmapImage Convert(System.Drawing.Image img)
        {
            using (var memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static BitmapImage imageConverter(String path)
        {
            // Create the image element.
    

            // Create source.
            BitmapImage bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
      
            return bi;
        }

        //IZPISI DATAGRID CONTENT
        public void DGDisplayContent(FileInfo[] fia, int ifia, DirectoryInfo[] dia,  int idia)
        {
           
       
            for (int i = 0; i < idia; i++)
            {
                content.Add(new DGContent()
                {
                    Name = dia[i].Name,
                    Extension = "File folder",
                    LastWriteTime = dia[i].LastWriteTime,
                    icon = imageConverter(@"\folder.png"),
                    FullPath = dia[i].FullName,
                    Folder = true,

                });


            }


            for (int i = 0; i < ifia; i++)
            {
                content.Add(new DGContent()
                {
                    Name = fia[i].Name,
                    Extension = fia[i].Extension,
                    LastWriteTime = fia[i].LastWriteTime,
                    Length = fia[i].Length,
                    icon = Convert(System.Drawing.Icon.ExtractAssociatedIcon(fia[i].FullName).ToBitmap()),
                    FullPath = fia[i].FullName,
                    Compressed = (fia[i].Extension == ".zip"|| fia[i].Extension == ".rar") ? true : false

            });

            }

            List<string> duplikati = new List<string>();
            List<DGContent> duplikati2 = new List<DGContent>();
            duplikati.Clear();
            duplikati2.Clear();
            foreach(var item in content)
            {
                if (duplikati.Contains(item.FullPath))
                {
                    duplikati2.Add(item);
                }
                else
                {
                    duplikati.Add(item.FullPath);
                }

            }

            foreach(var item in duplikati2)
            {
                content.Remove(item);
            }
            DataGrid.ItemsSource = null;
            DataGrid.ItemsSource = content;
            if (depth == 0) { MainContent = new List<DGContent>(content); }
            
            
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {


            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



            // Set filter for file extension and default file extension 
            dlg.Multiselect = true;
            dlg.Filter = "All files (*.*)|*.*";
       


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 

             
                string filename = dlg.FileName;
                tbuncompress.Text = filename;
              
            }
            
        }

        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            if (!CompressDG)
            {

                b_back.IsEnabled = true;
                b_main.IsEnabled = true;
                string[] DroppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                DirectoryInfo[] folders = new DirectoryInfo[DroppedPaths.Length];
                int ifolders = 0;
                FileInfo[] files = new FileInfo[DroppedPaths.Length];
                int ifiles = 0;

                if (e.Data.GetDataPresent(DataFormats.FileDrop))


                    foreach (String path in DroppedPaths)
                    {
                        FileAttributes attr = File.GetAttributes(path);

                        if (attr.HasFlag(FileAttributes.Directory))
                        {
                            folders[ifolders++] = new DirectoryInfo(path);
                        }

                        else
                        {
                            files[ifiles++] = new FileInfo(path);
                        }

                    }

                DGDisplayContent(files, ifiles, folders, ifolders);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); ;
            dialog.DefaultExt = ".zip";
            dialog.Filter = "Choose format ||Compressed file (*.rar)|*.rar|Compressed file (*.zip)|*.zip|Compressed file (*.7z)|*.7z";
            if (content.Count != 0)
            {
                if (content.First().Folder)
                    dialog.FileName = content.First().Name;
                else { 
                string ime = "";
                string[] imena = content.First().Name.Split('.');
               
                for(int i = 0; i < imena.Length - 1; i++)
                    {
                        ime += imena[i];
                    }
                    dialog.FileName = ime;
                 }

            }

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
            {
                tbcompress.Text = dialog.FileName;
            }


                
        }





        private void DataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid myDataGrid = (DataGrid)sender;
            if (!CompressDG)
            {

                if (myDataGrid.SelectedItems.Count > 0)
                {
                    string[] files = new String[myDataGrid.SelectedItems.Count];
                    int ix = 0;
                    foreach (object item in myDataGrid.SelectedItems)
                    {
                        files[ix] = ((DGContent)item).FullPath;
                        ++ix;
                    }
                    string dataFormat = DataFormats.FileDrop;
                    Console.WriteLine("Data format: " + dataFormat);
                    DataObject dataObject = new DataObject(dataFormat, files);
                    DragDrop.DoDragDrop(myDataGrid, dataObject, DragDropEffects.Copy);
                }
                
            }
            
                


            
        }

       

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!CompressDG)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Console.WriteLine("");
                    if (DataGrid.SelectedItem == null) return;
                    DGContent item = (DGContent)DataGrid.SelectedItem;
                    bool path = item.Compressed;
                    Console.WriteLine(path);


                    if (item.Folder)
                    {
                        dir = item.FullPath;
                        string[] folders = Directory.GetDirectories(dir);
                        string[] files = Directory.GetFiles(dir);
                        FileInfo[] _files = new FileInfo[files.Length];
                        DirectoryInfo[] _folders = new DirectoryInfo[folders.Length];
                        for (int i = 0; i < folders.Length; i++)
                        {
                            _folders[i] = new DirectoryInfo(folders[i]);
                        }
                        for (int i = 0; i < files.Length; i++)
                        {
                            _files[i] = new FileInfo(files[i]);
                        }
                        content.Clear();
                        depth++;
                        DataGrid.AllowDrop = false;
                        DGDisplayContent(_files, _files.Length, _folders, _folders.Length);
                    }

                    else if (item.Compressed)
                    {
                        CompressDG = true;
                        Compressed_content.Clear();
                        using (Stream stream = File.OpenRead(item.FullPath))
                        using (var reader = ReaderFactory.Open(stream))
                        {


                            while (reader.MoveToNextEntry())
                            {

                                String[] ext = reader.Entry.Key.Split('.');
                                string extension = "." + ext[ext.Length - 1];

                                

                                if (!reader.Entry.IsDirectory)
                                {
                                    if ((reader.Entry.Key.Split('\\').Length == 1 || reader.Entry.Key.Split('\\')[1].Length == 0) && (reader.Entry.Key.Split('/').Length == 1 || reader.Entry.Key.Split('/')[1].Length == 0))
                                    {
                                        Compressed_content.Add(new DGContent()
                                        {
                                            Name = reader.Entry.Key,
                                            Extension = extension,
                                            LastWriteTime = reader.Entry.LastModifiedTime.Value,
                                            Length = reader.Entry.Size,
                                            icon = imageConverter(@"\compressed.png"),
                                            FullPath = item.FullPath + "/" + reader.Entry.Key,
                                            Compressed = true

                                        });

                                    }
                                }
                                else
                                {
                                    
                              
                                    if ((reader.Entry.Key.Split('\\').Length == 1 || reader.Entry.Key.Split('\\')[1].Length==0)  && (reader.Entry.Key.Split('/').Length == 1 || reader.Entry.Key.Split('/')[1].Length == 0))
                                    {
                                        Console.WriteLine(reader.Entry.Key);
                                        Compressed_content.Add(new DGContent()
                                        {
                                            Name = reader.Entry.Key,
                                            Extension = extension,
                                            LastWriteTime = reader.Entry.LastModifiedTime.Value,
                                            Length = reader.Entry.Size,
                                            icon = imageConverter(@"\folder.png"),
                                            FullPath = item.FullPath + "/" + reader.Entry.Key,
                                            Compressed = true

                                        });
                                    }
                                }






                            }



                        }
                        DataGrid.ItemsSource = null;
                        DataGrid.ItemsSource = Compressed_content;
                        Console.WriteLine("lezs");
                    }
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.SelectedPath = "C:\\";

            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
            if (result.ToString() == "OK") {
                tbuncompress.Text = folderDialog.SelectedPath;
                }


        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
           
            content = new List<DGContent>(MainContent); 
            DataGrid.ItemsSource = null;
            DataGrid.ItemsSource = content;
            depth = 0;
            DataGrid.AllowDrop = true;
            CompressDG = false;

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (!CompressDG)
            {
                if (depth <= 1)
                {

                    Button_Click_3(null, null);
                    depth = 0;

                }
                else
                {
                    depth--;
                    dir = Directory.GetParent(dir).ToString();

                    string[] folders = Directory.GetDirectories(dir);
                    string[] files = Directory.GetFiles(dir);
                    FileInfo[] _files = new FileInfo[files.Length];
                    DirectoryInfo[] _folders = new DirectoryInfo[folders.Length];
                    for (int i = 0; i < folders.Length; i++)
                    {
                        _folders[i] = new DirectoryInfo(folders[i]);
                    }
                    for (int i = 0; i < files.Length; i++)
                    {
                        _files[i] = new FileInfo(files[i]);
                    }
                    content.Clear();
                    DataGrid.AllowDrop = false;
                    DGDisplayContent(_files, _files.Length, _folders, _folders.Length);


                }
            }
            else
            {
                if (depth <= 1)
                {

                    Button_Click_3(null, null);
                    depth = 0;
                    CompressDG = false;

                }
                else
                {
                    

                    string[] folders = Directory.GetDirectories(dir);
                    string[] files = Directory.GetFiles(dir);
                    FileInfo[] _files = new FileInfo[files.Length];
                    DirectoryInfo[] _folders = new DirectoryInfo[folders.Length];
                    for (int i = 0; i < folders.Length; i++)
                    {
                        _folders[i] = new DirectoryInfo(folders[i]);
                    }
                    for (int i = 0; i < files.Length; i++)
                    {
                        _files[i] = new FileInfo(files[i]);
                    }
                    content.Clear();
                    DataGrid.AllowDrop = false;
                    DGDisplayContent(_files, _files.Length, _folders, _folders.Length);
                    CompressDG = false;

                }


            }



        }
            
        

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            content = new List<DGContent>();
            MainContent = new List<DGContent>();
            DataGrid.ItemsSource = null;
            depth = 0;
            DataGrid.AllowDrop = true;
            b_back.IsEnabled = false;
            b_main.IsEnabled = false;
            CompressDG = false;

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid myDataGrid = DataGrid;
            Console.WriteLine("test");
            if (e.Key == Key.Delete)
            {
                if (depth == 0)
                {
                    Console.WriteLine("DELETE");
                    foreach (DGContent item in myDataGrid.SelectedItems)
                    {
                        content.Remove(item);

                    }
                    DataGrid.ItemsSource = null;
                    DataGrid.ItemsSource = content;
                }


                e.Handled = true;
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(tbuncompress.Text) && content.Count != 0)
            {
                foreach (DGContent item in content)
                {
                    if (item.Compressed)
                    {
                        using (Stream stream = File.OpenRead(item.FullPath))
                        using (var reader = ReaderFactory.Open(stream))
                        {
                            while (reader.MoveToNextEntry())
                            {
                                if (!reader.Entry.IsDirectory)
                                {
                                    Console.WriteLine(reader.Entry.Key);
                                    reader.WriteEntryToDirectory(@tbuncompress.Text, new ExtractionOptions()
                                    {
                                        ExtractFullPath = true,
                                        Overwrite = true
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("kliuk");
            string[] a = tbcompress.Text.Split('\\');
            string pot = "";
            for(int i = 0; i < a.Length - 1; i++)
            {
                pot +=  a[i]+"\\";
            }
            Console.WriteLine(pot);
            if (Directory.Exists(pot) && content.Count != 0)
                MakeCompressedFile();
        }
    }
}
