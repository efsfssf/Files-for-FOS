using System;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using Windows.Foundation.Metadata;
using System.Diagnostics;
using Bridge;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace Files
{

    public sealed partial class MainPage : Page
    {
        string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
        string OneDrivePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\OneDrive";
        string PicturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        string MusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        string VideosPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        string FileExplorerIndex = "ms-appx-web:///FileExplorer/Assets/webPayload/index.html";
        //string FileExplorerIndex = "ms-appx-web:///FileExplorer/test.html";

        public MainPage()
        {
            this.InitializeComponent();
            this.IsTextScaleFactorEnabled = true;

            var CoreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            CoreTitleBar.ExtendViewIntoTitleBar = true;
            //DragArea.Height = CoreTitleBar.Height;
            //Window.Current.SetTitleBar(DragArea);

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Color.FromArgb(100, 255, 255, 255);
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(75, 10, 10, 10);
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(75, 10, 10, 10);

            Uri uri = new Uri(FileExplorerIndex);
            Web.Navigate(uri);
            Debug.WriteLine("................." +
                "\n" +
                "\n" +
                "\nSubscribe");


            // Получить идентификатор текущей папки
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;



            OnClipboardChanged();
            StartServer();
            //WelcomeFileCheck(); - Legacy Function to be Removed Eventually
            //ContentFrame.Navigate(typeof(YourHome));
            //auto_suggest.IsEnabled = true;
            //auto_suggest.PlaceholderText = "Search Recents";
        }
        private void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            //Web.AddWebAllowedObject("clipboardOperations", new MyNativeClass());
        }

        private async void OnClipboardChanged()
        {
            // Здесь ваш код, который вызывается при изменении буфера обмена
            await Task.Delay(10000);
            // Вызов JavaScript функции из C#
            await Web.InvokeScriptAsync("eval", new string[] { @"
                var event = new Event('clipboardchanged');
                window.dispatchEvent(event);
            " });
        }


        public async void StartServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:9001/");
            listener.Start();

            // Создайте экземпляр ItemViewModel
            ItemViewModel itemViewModel = new ItemViewModel(@"C:\");
            

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // Обработка CORS
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");

                if (request.HttpMethod == "GET")
                {
                    Debug.Print("\n\n\n\n\nЗАПРОС ПОСТУПИЛ!!!!!!!!!!!!!!!!! " + request.RawUrl + "\n\n");
                }

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                } 
                else if (request.HttpMethod == "GET" && request.RawUrl == "/drives")
                {
                    List<object> drives = await itemViewModel.GetInternalDrives();
                    string responseString = JsonConvert.SerializeObject(new { value = drives });
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                else if (request.HttpMethod == "GET" && request.RawUrl == "/lang")
                {
                    // Загрузите JSON из файла
                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///FileExplorer/Assets/webPayload/languages/my_lang_EN_US.html"));
                    string json = await FileIO.ReadTextAsync(file);
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                else if (request.HttpMethod == "GET" && request.RawUrl.StartsWith("/drives/") && request.RawUrl.Contains("/items/"))
                {
                    // Извлеките идентификатор диска и имя папки из URL
                    var urlParts = request.RawUrl.Split('/');
                    var driveId = urlParts[2];
                    var folderName = urlParts[4];

                    // Получите информацию о папке
                    List<object> folderInfo = await itemViewModel.GetFolderInfo(driveId, folderName);

                    // Сериализуйте информацию о папке в JSON
                    string responseString = JsonConvert.SerializeObject(folderInfo);

                    // Отправьте ответ
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                else if (request.HttpMethod == "GET" && request.RawUrl.StartsWith("/drives/") && !request.RawUrl.Contains("/me/") && !request.RawUrl.Contains("/apps"))
                {
                    string driveId = request.RawUrl.Split('?')[0].Substring(8); // Получаем ID диска из URL
                    if (request.QueryString["$select"].Contains("syncStatus")) // Проверяем, есть ли syncStatus в select
                    {
                        // Здесь вы можете добавить код для обработки запроса
                        // Например, получить информацию о диске с использованием driveId

                        object status = await itemViewModel.DriveStatus(driveId[driveId.Length - 1].ToString());
                    }
                }


            }
        }


        private void navView_ItemInvoked(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem item = args.SelectedItem as NavigationViewItem;
            //if (item.Name == "homeIc")
            //{
            //    ContentFrame.Navigate(typeof(YourHome));
            //    auto_suggest.PlaceholderText = "Search Recents";
            //}
            //else if (item.Name == "DesktopIC")
            //{
            //    ContentFrame.Navigate(typeof(GenericFileBrowser), DesktopPath);
            //    auto_suggest.PlaceholderText = "Search Desktop";
            //}
            //else if (item.Name == "DocumentsIC")
            //{
            //    ContentFrame.Navigate(typeof(GenericFileBrowser), DocumentsPath);
            //    auto_suggest.PlaceholderText = "Search Documents";
            //}
            //else if (item.Name == "DownloadsIC")
            //{
            //    ContentFrame.Navigate(typeof(GenericFileBrowser), DownloadsPath);
            //    auto_suggest.PlaceholderText = "Search Downloads";
            //}
            //else if (item.Name == "PicturesIC")
            //{
            //    ContentFrame.Navigate(typeof(GenericFileBrowser), PicturesPath);
            //    auto_suggest.PlaceholderText = "Search Pictures";
            //}
            //else if (item.Name == "MusicIC")
            //{
            //    ContentFrame.Navigate(typeof(GenericFileBrowser), MusicPath);
            //    auto_suggest.PlaceholderText = "Search Music";
            //}
            //else if (item.Name == "VideosIC")
            //{
            //    ContentFrame.Navigate(typeof(GenericFileBrowser), VideosPath);
            //    auto_suggest.PlaceholderText = "Search Videos";
            //}
            //else if (item.Name == "LocD_IC")
            //{
            //    ContentFrame.Navigate(typeof(GenericFileBrowser), @"C:\");
            //    auto_suggest.PlaceholderText = "Search";
            //}
            //else if (item.Name == "OneD_IC")
            //{
            //    ContentFrame.Navigate(typeof(GenericFileBrowser), OneDrivePath);
            //    auto_suggest.PlaceholderText = "Search OneDrive";
            //}
            //else if(item.Content.Equals("Settings"))
            //{
            //    ContentFrame.Navigate(typeof(Settings));
            //}
        }

        public async void WelcomeFileCheck()
        {
            string env = Environment.ExpandEnvironmentVariables("%userprofile%");
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalCacheFolder;
            string cachePath = storageFolder.Path + @"\welcome.txt";
            //diagText.Text = cachePath;
            FileInfo fInfo = new FileInfo(cachePath);
            if (await storageFolder.TryGetItemAsync("welcome.txt") == null)
            {
               // WelcomeGrid.Visibility = Visibility.Visible;
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
               // WelcomeGrid.Visibility = Visibility.Collapsed;

                var fal = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
                fal.Clear();
                fal.AddOrReplace("CDriveToken", folder);

                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalCacheFolder;
                Windows.Storage.StorageFile sampleFile = await storageFolder.CreateFileAsync("welcome.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            }
        }

        
    }
}