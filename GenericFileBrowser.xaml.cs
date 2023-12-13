using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Files
{

    public sealed partial class GenericFileBrowser : Page
    {
        public TextBlock textBlock;
        static DataGrid data;


        public GenericFileBrowser()
        {
            this.InitializeComponent();

            string env = Environment.ExpandEnvironmentVariables("%userprofile%");


        }


        public static void RemoveHiddenColumns()
        {
            if (data.Columns.Count > 5)
            {
                data.Columns[5].Visibility = Visibility.Collapsed;
                data.Columns[6].Visibility = Visibility.Collapsed;
                data.Columns[7].Visibility = Visibility.Collapsed;
                data.Columns[8].Visibility = Visibility.Collapsed;
                data.Columns[9].Visibility = Visibility.Collapsed;
                data.Columns[10].Visibility = Visibility.Collapsed;
                data.Columns[11].Visibility = Visibility.Collapsed;
            }
            else
            {
                Debug.WriteLine("Less than 4 columns in datagrid");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
            var parameters = (string)eventArgs.Parameter;
            if (parameters.Equals(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)))
            {
                //VisiblePath.Text = "Desktop";
            }
            else if (parameters.Equals(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)))
            {
                //VisiblePath.Text = "Documents";
            }
            else if (parameters.Equals(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads"))
            {
                //VisiblePath.Text = "Downloads";
            }
            else if (parameters.Equals(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)))
            {
                //VisiblePath.Text = "Pictures";
            }
            else if (parameters.Equals(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)))
            {
                //VisiblePath.Text = "Music";
            }
            else if (parameters.Equals(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)))
            {
                //VisiblePath.Text = "Videos";
            }
            else
            {
                //VisiblePath.Text = parameters;
            }

        }

        public ItemViewModel ViewModel { get; set; }


        

        bool CancelledBefore = false;



        private void Foward_Click(object sender, RoutedEventArgs e)
        {

        }

        

    }
    public class ListedItem
    {
        public Visibility FolderImg { get; set; }
        public Visibility FileIconVis { get; set; }
        public BitmapImage FileImg { get; set; }
        public string FileName { get; set; }
        public string FileDate { get; set; }
        public string FileExtension { get; set; }
        public string FilePath { get; set; }
        public ListedItem()
        {

        }
    }

    public class ItemViewModel
    {
        public ObservableCollection<ListedItem> folInfoList = new ObservableCollection<ListedItem>();
        public ObservableCollection<ListedItem> FolInfoList { get { return this.folInfoList; } }
        public ObservableCollection<ListedItem> fileInfoList = new ObservableCollection<ListedItem>();
        public ObservableCollection<ListedItem> FileInfoList { get { return this.fileInfoList; } }

        public ObservableCollection<ListedItem> filesAndFolders = new ObservableCollection<ListedItem>();
        public ObservableCollection<ListedItem> FilesAndFolders { get { return this.filesAndFolders; } }


        StorageFolder folder;
        string gotName;
        string gotDate;
        string gotType;
        string gotPath;
        string gottenPath;
        string gotFolName;
        string gotFolDate;
        string gotFolPath;
        string gotFolType;
        Visibility gotFileImgVis;
        Visibility gotFolImg;
        StorageItemThumbnail gotFileImg;
        public IReadOnlyList<StorageFolder> folderList;
        public IReadOnlyList<StorageFile> fileList;




        private ListedItem li = new ListedItem();

        public ItemViewModel(string v)
        {
            V = v;
        }

        public ListedItem LI { get { return this.li; } }

        public string V { get; }




        /* ТУТ МОЙ КОД */

        private async void InitMessageDialogHandler(IUICommand command)
        {
            if ((int)command.Id == 0)
            {
                await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
            }
        }

        public async void GetItemsAsync(string path, CancellationToken ct)
        {


            try
            {
                folder = await StorageFolder.GetFolderFromPathAsync(path);          // Set location to the current directory specified in path
            }
            catch
            {
                MessageDialog dlg = new MessageDialog(
                    "It seems you have not granted permission for this app to access the file system broadly. " +
                    "Without this permission, the app will only be able to access a very limited set of filesystem locations. " +
                    "You can grant this permission in the Settings app, if you wish. You can do this now or later. " +
                    "If you change the setting while this app is running, it will terminate the app so that the " +
                    "setting can be applied. Do you want to do this now?",
                    "File system permissions");
                dlg.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(InitMessageDialogHandler), 0));
                dlg.Commands.Add(new UICommand("No", new UICommandInvokedHandler(InitMessageDialogHandler), 1));
                dlg.DefaultCommandIndex = 0;
                dlg.CancelCommandIndex = 1;
                await dlg.ShowAsync();
                Application.Current.Exit();
            }
            folderList = await folder.GetFoldersAsync();                        // Create a read-only list of all folders in location
            fileList = await folder.GetFilesAsync();                            // Create a read-only list of all files in location
            int NumOfFolders = folderList.Count;                                // How many folders are in the list
            int NumOfFiles = fileList.Count;                                    // How many files are in the list
            int NumOfItems = NumOfFiles + NumOfFolders;
            int NumItemsRead = 0;



            foreach (StorageFolder fol in folderList)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }
                else
                {

                    int ProgressReported = (NumItemsRead * 100 / NumOfItems);
                    gotFolName = fol.Name.ToString();
                    gotFolDate = fol.DateCreated.ToString();
                    gotFolPath = fol.Path.ToString();
                    gotFolType = "Folder";
                    gotFolImg = Visibility.Visible;
                    gotFileImgVis = Visibility.Collapsed;
                    this.filesAndFolders.Add(new ListedItem() { FileImg = null, FileIconVis = gotFileImgVis, FolderImg = gotFolImg, FileName = gotFolName, FileDate = gotFolDate, FileExtension = gotFolType, FilePath = gotFolPath });

                    NumItemsRead++;
                    GenericFileBrowser.RemoveHiddenColumns();
                }

            }
            foreach (StorageFile f in fileList)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }
                else
                {

                    int ProgressReported = (NumItemsRead * 100 / NumOfItems);
                    gotName = f.Name.ToString();
                    gotDate = f.DateCreated.ToString(); // In the future, parse date to human readable format
                    gotType = f.FileType.ToString();
                    gotPath = f.Path.ToString();
                    gotFolImg = Visibility.Collapsed;
                    const uint requestedSize = 20;
                    const ThumbnailMode thumbnailMode = ThumbnailMode.ListView;
                    const ThumbnailOptions thumbnailOptions = ThumbnailOptions.UseCurrentScale;
                    gotFileImg = await f.GetThumbnailAsync(thumbnailMode, requestedSize, thumbnailOptions);
                    BitmapImage icon = new BitmapImage();
                    if (gotFileImg != null)
                    {
                        icon.SetSource(gotFileImg.CloneStream());
                    }
                    gotFileImgVis = Visibility.Visible;
                    this.filesAndFolders.Add(new ListedItem() { FileImg = icon, FileIconVis = gotFileImgVis, FolderImg = gotFolImg, FileName = gotName, FileDate = gotDate, FileExtension = gotType, FilePath = gotPath });
                    NumItemsRead++;
                    GenericFileBrowser.RemoveHiddenColumns();
                }
            }

        }

        public async Task<List<object>> GetInternalDrives()
        {
            string driveLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int driveLettersLen = driveLetters.Length;
            string removableDriveLetters = "";
            string driveLetter;

            List<object> drives = new List<object>();
            StorageFolder removableDevices = KnownFolders.RemovableDevices;
            IReadOnlyList<StorageFolder> folders = await removableDevices.GetFoldersAsync();

            foreach (StorageFolder removableDevice in folders)
            {
                if (string.IsNullOrEmpty(removableDevice.Path)) continue;
                driveLetter = removableDevice.Path.Substring(0, 1).ToUpper();
                if (driveLetters.IndexOf(driveLetter) > -1) removableDriveLetters += driveLetter;
            }

            for (int curDrive = 0; curDrive < driveLettersLen; curDrive++)
            {
                driveLetter = driveLetters.Substring(curDrive, 1);
                if (removableDriveLetters.IndexOf(driveLetter) > -1) continue;

                try
                {
                    StorageFolder drive = await StorageFolder.GetFolderFromPathAsync(driveLetter + ":");
                    var properties = await drive.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace", "System.Capacity" });
                    ulong freeSpace = (ulong)properties["System.FreeSpace"];
                    ulong totalSpace = (ulong)properties["System.Capacity"];
                    ulong usedSpace = totalSpace - freeSpace;

                    string state = "unknown";
                    double usedPercentage = (double)usedSpace / totalSpace;
                    if (usedPercentage < 0.7) state = "normal";
                    else if (usedPercentage < 0.9) state = "nearing";
                    else if (usedPercentage < 1) state = "critical";
                    else state = "exceeded";

                    drives.Add(new
                    {
                        id = "root"+ driveLetter,
                        name = "Local drive ("+ driveLetter+":)",
                        driveType = "local",
                        quota = new
                        {
                            total = totalSpace,
                            used = usedSpace,
                            remaining = freeSpace,
                            state = state
                        },
                        root = new { }
                    });
                }
                catch (Exception)
                {
                    // Пропустить диск, если он не существует
                }
            }

            return drives;
        }



    }
}