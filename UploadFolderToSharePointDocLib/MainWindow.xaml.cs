using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UploadFolderToSharePointDocLib
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string siteUrl = txtboxSiteUrl.Text;
            string folderPath = txtboxFolderPath.Text;
            string libraryName = txtboxLibraryName.Text;

            try
            {
                using (ClientContext ctx = new ClientContext(siteUrl))
                {
                    Web web = ctx.Web;
                    string pwd = "******";
                    SecureString password = new SecureString();
                    for (int i = 0; i < pwd.Length; i++)
                    {
                        password.AppendChar(pwd[i]);
                    }
                    ctx.Credentials = new System.Net.NetworkCredential("******", password);
                    ctx.Load(web);
                    ctx.ExecuteQuery();
                    Microsoft.SharePoint.Client.List dUplaod = web.Lists.GetByTitle(libraryName);
                    String[] fileNames = Directory.GetFiles(@folderPath);
                    bool exists = false;
                    DirectoryInfo dInfo = new DirectoryInfo(@folderPath);
                    FolderCollection folders = dUplaod.RootFolder.Folders;
                    char[] sep = { '\\' };
                    ctx.Load(folders);
                    ctx.ExecuteQuery();
                    foreach (Folder eFolder in folders)
                    {
                        if (eFolder.Name.Equals(dInfo.Name))
                        {
                            foreach (String fileName in fileNames)
                            {
                                String[] names = fileName.Split(sep);
                                FileCreationInformation fCInfo = new FileCreationInformation();
                                fCInfo.Content = System.IO.File.ReadAllBytes(fileName);
                                fCInfo.Url = names[names.Length - 1];
                                fCInfo.Overwrite = true;
                                eFolder.Files.Add(fCInfo);
                                exists = true;
                            }

                        }
                    }

                    if (!exists)
                    {
                        Folder tFolder = folders.Add(siteUrl + "/" + libraryName + "/" + dInfo.Name);
                        ctx.ExecuteQuery();                        
                        UploadFile(ctx, tFolder, dInfo);
                    }
                    MessageBox.Show("The Execution is completed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void UploadFile(ClientContext ctx, Folder tFolder, DirectoryInfo dInfo)
        {
            char[] sep = { '\\' };
            if (Directory.GetFiles(dInfo.FullName).Count() > 0)
            {
                String[] tempFileNames = Directory.GetFiles(dInfo.FullName);
                foreach (String fileName in tempFileNames)
                {
                    String[] names = fileName.Split(sep);
                    FileCreationInformation fCInfo = new FileCreationInformation();
                    fCInfo.Content = System.IO.File.ReadAllBytes(fileName);
                    fCInfo.Url = names[names.Length - 1];
                    fCInfo.Overwrite = true;
                    tFolder.Files.Add(fCInfo);
                }
                ctx.ExecuteQuery();
            }

            foreach (var directory in dInfo.GetDirectories())
            {
                if (directory.GetDirectories().Count() >= 0)
                {
                    Folder tempFolder = tFolder.Folders.Add(directory.Name);
                    UploadFile(ctx, tempFolder, directory);
                }

                String[] tempFileNames = Directory.GetFiles(directory.FullName);
                foreach (String fileName in tempFileNames)
                {
                    String[] names = fileName.Split(sep);
                    FileCreationInformation fCInfo = new FileCreationInformation();
                    fCInfo.Content = System.IO.File.ReadAllBytes(fileName);
                    fCInfo.Url = names[names.Length - 1];
                    fCInfo.Overwrite = true;
                    tFolder.Folders.Add(directory.Name).Files.Add(fCInfo);
                }
                ctx.ExecuteQuery();                
            }

        }
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
