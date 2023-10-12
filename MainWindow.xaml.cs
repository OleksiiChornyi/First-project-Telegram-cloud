using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Windows.Threading;
using WpfAnimatedGif;
using Path = System.IO.Path;

namespace Telegram_cloud
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //name with _ - my variables
        //name with variablesVariables - chatGPT or from Internet
        public enum Languages { en, ua, ru };
        public static Languages current_lang = Languages.en;
        public static string PB_In_progress = Telegram_cloud.Resources.Resource_english.Progress_bar_ToolTip_In_progress;
        public static string PB_Ready = Telegram_cloud.Resources.Resource_english.Progress_bar_ToolTip_Ready; 
        public static Thread td_client;

        public Dictionary<string, string> settings_json = Settings_json.LoadDictionaryFromJson();
        public static bool skip_all_next_files = false;
        public static bool change_file_in_drive = false;
        public static string name_exist_file = "";
        public static string new_text_message = "";

        public static string new_name_file = "";
        public static bool isDownloadPhotoPreview = false;
        public static bool isDownloadVideoPreview = false;
        public static double size_load_file_gigabytes = 2.0;

        private Thread mainThread;
        static CancellationTokenSource mainCancellationTokenSource = new CancellationTokenSource();
        private Thread changeListViewItemsThread;
        static CancellationTokenSource changeListViewItemsCancellationTokenSource = new CancellationTokenSource();

        private readonly DispatcherTimer timer;
        private readonly double animationDuration = 5.0;
        private int timerTicks = 0;
        private List<string> name_seletcedValues_delete = new List<string>();

        private readonly List<string> name_seletcedValues = new List<string>();

        private static bool open_with_one_click = false;
        private static bool is_dark_theme = false;
        private bool need_change_size = false;
        private double _scale = 1;

        //public static string current_path;
        /// <summary>
        /// Start MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            if (!IsCppRedistributableInstalled())
            {
                MessageBox.Show("To use the program, you need to install C++ from Microsoft");
                return;
            }

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.1)
            };
            timer.Tick += Timer_Tick;

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            Change_preview_automatically_Checked();
            Change_open_click_or_double_click();
            Change_language_Initialization();

            if (settings_json["Is_autorization"] == true.ToString())
            {
                Create_client();
                Change_drives_item();
            }
            //Dark_theme.IsChecked = false;
            //Change_theme();
        }
        #region ResizeWindows
        bool ResizeInProcess = false;
        private void Resize_Init(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle senderRect)
            {
                ResizeInProcess = true;
                senderRect.CaptureMouse();
            }
        }

        private void Resize_End(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle senderRect)
            {
                ResizeInProcess = false; ;
                senderRect.ReleaseMouseCapture();
            }
        }

        private void Resizeing_Form(object sender, MouseEventArgs e)
        {
            if (ResizeInProcess)
            {
                Rectangle senderRect = sender as Rectangle;
                Window mainWindow = senderRect.Tag as Window;
                if (senderRect != null)
                {
                    double width = e.GetPosition(mainWindow).X;
                    double height = e.GetPosition(mainWindow).Y;
                    senderRect.CaptureMouse();
                    if (senderRect.Name.ToLower().Contains("right"))
                    {
                        width += 5;
                        if (width > 0)
                            mainWindow.Width = width;
                    }
                    if (senderRect.Name.ToLower().Contains("left"))
                    {
                        width -= 5;
                        mainWindow.Left += width;
                        width = mainWindow.Width - width;
                        if (width > 0)
                        {
                            mainWindow.Width = width;
                        }
                    }
                    if (senderRect.Name.ToLower().Contains("bottom"))
                    {
                        height += 5;
                        if (height > 0)
                            mainWindow.Height = height;
                    }
                    if (senderRect.Name.ToLower().Contains("top"))
                    {
                        height -= 5;
                        mainWindow.Top += height;
                        height = mainWindow.Height - height;
                        if (height > 0)
                        {
                            mainWindow.Height = height;
                        }
                    }
                }
            }
        }
        #endregion
        #region TitleButtons
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    App.Current.MainWindow.DragMove();
                }
            }
        }

        private void AdjustWindowSize()
        {
            if (App.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                App.Current.MainWindow.WindowState = WindowState.Normal;
                MaximizeButton.Content = "";
            }
            else if (App.Current.MainWindow.WindowState == WindowState.Normal)
            {
                App.Current.MainWindow.WindowState = WindowState.Maximized;
                MaximizeButton.Content = "";
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Close();
        }
        #endregion

        /// <summary>
        /// Checking if C++ is installed to interact with a Telegram
        /// </summary>
        /// <returns></returns>
        public static bool IsCppRedistributableInstalled()
        {
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            RegistryView[] registryViews = new RegistryView[]
            {
                RegistryView.Registry32,
                RegistryView.Registry64
            };

            foreach (var view in registryViews)
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    using (var uninstallKey = baseKey.OpenSubKey(registryPath))
                    {
                        foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                        {
                            using (var subKey = uninstallKey.OpenSubKey(subKeyName))
                            {
                                if (subKey.GetValue("DisplayName") is string displayName && displayName.Contains("Visual C++"))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Create a client and autorization in Telegram
        /// </summary>
        private void Create_client()
        {
            Abort_client();
            new Thread(o =>
            {
                TdCloud.Start();
                if (TdCloud._haveAuthorization)
                {
                    settings_json["Is_autorization"] = true.ToString();
                    //Settings_json.SaveDictionaryToJson(settings_json);
                }
                Dispatcher.Run();
            }).Start();
        }
        /// <summary>
        /// Abotr client and log out of Telegram
        /// </summary>
        public static void Abort_client()
        {
            TdCloud.Exit_autorization();
            td_client?.Abort();
            /*if (td_client != null)
                td_client.Abort();*/
        }
        /// <summary>
        /// Exit all program threads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TdCloud_Window_Closed(object sender, EventArgs e)
        {
            if (!TdCloud._haveAuthorization)
                Delete_account();
            Delete_directory(@"tdlib\documents");
            Delete_directory(@"tdlib\tmp");
            settings_json["Current_drive_id"] = Struct_cloud.IdDrive.ToString();
            Settings_json.SaveDictionaryToJson(settings_json);
            Struct_cloud.Save_all_drives();
            Environment.Exit(0);
        }
        /// <summary>
        /// Menu autorization button, autorize in Telegram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_autorize_Click(object sender, RoutedEventArgs e)
        {
            if (TdCloud._haveAuthorization)
                MessageBox.Show("You allready autorize");
            else
            {
                Create_client();
            }
        }
        /// <summary>
        /// Delete a directory from path
        /// </summary>
        /// <param name="directoryPath">Path to Directory</param>
        private void Delete_directory(string directoryPath)
        {
            try
            {
                Directory.Delete(directoryPath, true);
            }
            catch { }
        }
        /// <summary>
        /// Delete a Telegram account
        /// </summary>
        private void Delete_account()
        {
            settings_json["Is_autorization"] = true.ToString();
            Settings_json.SaveDictionaryToJson(settings_json);
            Abort_client();
            Delete_directory(@"tdlib");
            File.Delete(Storage_drives.filePath);
            TdCloud._haveAuthorization = false;
        }
        /// <summary>
        /// Log out from Telegram accuont
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Log_out_account_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                MessageBox.Show("You are not autorize");
                return;
            }
            Delete_account();
        }
        /// <summary>
        /// Get infornation from Telegram account
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_account_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                MessageBox.Show("You are not autorize");
                return;
            }
            About_account About_account_form = new About_account();
            About_account_form.ShowDialog();
        }
        /// <summary>
        /// Change drive items when add a new (or from list) drive
        /// </summary>
        public void Change_drives_item()
        {
            while (!TdCloud._haveAuthorization) ;
            Storage_drives.Update_drives();
            if (Storage_drives.drives == null)
                return;
            List<string> to_delete = new List<string>();
            foreach (MenuItem drive_list_item in Drives_list.Items)
            {
                string name = ((TextBlock)drive_list_item.Header).Text;
                if (!Storage_drives.drives.Values.Any(name.Contains))
                {
                    if (Directory.Exists(name))
                    {
                        try
                        {
                            Directory.Delete(name, true);
                        }
                        catch { }
                    }
                }
            }
            Drives_list.Items.Clear();
            foreach (var item in Storage_drives.drives)
            {
                var new_item_rb = new RadioButton
                {
                    ToolTip = item.Key,
                    Cursor = Cursors.Hand,
                    GroupName = "DrivesList"
                };
                new_item_rb.Checked += Select_new_drive;
                var new_item_tb = new TextBlock
                {
                    Text = item.Value,
                    ToolTip = item.Key,
                    Cursor = Cursors.Hand
                };
                new_item_tb.MouseLeftButtonDown += Select_new_drive;
                Drives_list.Items.Add(new MenuItem
                {
                    Header = new_item_tb,
                    Icon = new_item_rb
                });
            }
            Check_current_drive();
        }
        /// <summary>
        /// Change a selected drive
        /// </summary>
        public void Check_current_drive()
        {
            if (!TdCloud._haveAuthorization)
            {
                return;
            }
            if (Storage_drives.drives == null || Drives_list.Items == null || settings_json["Current_drive_id"] == false.ToString())
                return;
            foreach (var item in Storage_drives.drives)
            {
                if (item.Key.ToString() == settings_json["Current_drive_id"])
                {
                    foreach (MenuItem drive_list_item in Drives_list.Items)
                    {
                        if (drive_list_item.Header is TextBlock textblock && textblock.Text == item.Value)
                        {
                            if (Directory.Exists(item.Value))
                            {
                                ((RadioButton)drive_list_item.Icon).IsChecked = true;
                            }
                        }
                    }
                }
            }
            var is_cheked = false;
            foreach (MenuItem drive_list_item in Drives_list.Items)
            {
                if (((RadioButton)drive_list_item.Icon).IsChecked == true)
                {
                    is_cheked = true;
                }
            }
            if (!is_cheked)
            {
                Directory_list.ItemsSource = new List<string>();
                Full_path.Text = "";
            }
        }
        /// <summary>
        /// Add a new drive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_drive_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                MessageBox.Show("You are not autorize");
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            Add_drive add_drive = new Add_drive();
            add_drive.ShowDialog();
            Change_drives_item();
        }
        /// <summary>
        /// Add a drive from existing channels
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_drive_from_list_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                MessageBox.Show("You are not autorize");
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            Find_drive find_drive = new Find_drive();
            find_drive.ShowDialog();
            Change_drives_item();
        }
        /// <summary>
        /// Change a full path in Window
        /// </summary>
        private void Change_FullPath()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (Struct_cloud.Get_path() == "")
                    Full_path.Text = Struct_cloud.NameDrive;
                else
                    Full_path.Text = Path.Combine(Struct_cloud.NameDrive, Struct_cloud.Get_path());
            });
        }
        /// <summary>
        /// Change all items in window from a child of drive
        /// </summary>
        /// <param name="dir_list">List of where the items are located</param>
        /// <param name="name_drive">A name of drive</param>
        /// <param name="path">Path of elements</param>
        private void Change_ListView_drive(string path)
        {
            if (changeListViewItemsThread != null)
            {
                changeListViewItemsCancellationTokenSource.Cancel();
                changeListViewItemsThread.Join();
                changeListViewItemsCancellationTokenSource = new CancellationTokenSource();
            }
            changeListViewItemsThread = new Thread(o =>
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                string exeDir = Path.GetDirectoryName(exePath);
                string directory_image = Path.Combine(exeDir, "directory.png");
                string file_document = Path.Combine(exeDir, "document.png");
                string image_icon = Path.Combine(exeDir, "image.png");
                string video_icon = Path.Combine(exeDir, "video.png");
                string winrar_archive_image = Path.Combine(exeDir, "archive.png");
                List<string> file_arch = new List<string> { ".zip", ".ZIP", ".rar", ".RAR", ".7z", ".7Z", ".tar", ".TAR", ".gz", ".GZ", ".iso", ".ISO", ".cab", ".CAB", ".lzh", ".LZH" };
                var list_items = Struct_cloud.Search_by_path(path);
                path = Path.Combine(Struct_cloud.NameDrive, path);
                Directory_list.Dispatcher.Invoke(() =>
                {
                    Directory_list.ItemsSource = new ObservableCollection<ImageItem>();
                });
                foreach (var item in list_items)
                {
                    if (changeListViewItemsCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    ImageItem imageitem;
                    if (item.Value == Formats.Directory)
                        imageitem = new ImageItem() { Name = item.Key, Image = directory_image };
                    else
                    {
                        if (TdCloud.images_formats.Any(item.Key.EndsWith))
                        {
                            if (File.Exists(Path.Combine(exeDir, path, item.Key)))
                            {
                                string file_image = Path.Combine(exeDir, path, item.Key);
                                imageitem = new ImageItem() { Name = item.Key, Image = file_image };
                            }
                            else
                            {
                                imageitem = new ImageItem() { Name = item.Key, Image = image_icon };
                            }
                        }
                        else if (TdCloud.videos_formats.Any(item.Key.EndsWith))
                        {
                            if (File.Exists(Path.Combine(exeDir, path, TdCloud.ChangeVideoToGif(item.Key))))
                            {
                                string file_gif = Path.Combine(exeDir, path, TdCloud.ChangeVideoToGif(item.Key));
                                imageitem = new ImageItem() { Name = item.Key, Image = file_gif };
                            }
                            else
                            {
                                imageitem = new ImageItem() { Name = item.Key, Image = video_icon };
                            }
                        }
                        else if (TdCloud.gif_format.Any(item.Key.EndsWith))
                        {
                            if (File.Exists(Path.Combine(exeDir, path, TdCloud.ChangeVideoToGif(item.Key))))
                            {
                                string file_gif = Path.Combine(exeDir, path, TdCloud.ChangeVideoToGif(item.Key));
                                imageitem = new ImageItem() { Name = item.Key, Image = file_gif };
                            }
                            else
                            {
                                imageitem = new ImageItem() { Name = item.Key, Image = video_icon };
                            }
                        }
                        else if (file_arch.Any(item.Key.EndsWith))
                        {
                            imageitem = new ImageItem() { Name = item.Key, Image = winrar_archive_image };
                        }
                        else
                        {
                            imageitem = new ImageItem() { Name = item.Key, Image = file_document };
                        }
                    }
                    if (imageitem != null)
                    {
                        Directory_list.Dispatcher.Invoke(() =>
                        {
                            ((ObservableCollection<ImageItem>)Directory_list.ItemsSource).Add(imageitem);
                            Change_count_files();
                        });
                        need_change_size = true;
                    }
                }
                name_seletcedValues.Clear();
            });
            changeListViewItemsThread.Start();
            //changeListViewItemsCancellationTokenSource
        }
        /// <summary>
        /// Change text about count files
        /// </summary>
        private void Change_count_files()
        {
            Text_block_Count_files.Text = Directory_list.Items.Count.ToString() + " files";
        }
        /// <summary>
        /// Open a drive with saved json files
        /// </summary>
        /// <param name="dir_list">List of where the items are located</param>
        /// <param name="name_drive">A name of drive</param>
        /// <param name="path">Path of elements</param>
        private void Open_Tree(string name_drive, long id_drive)
        {
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = 1;
            Progress_bar.Value = 0;
            mainThread = new Thread(o =>
            {
                Struct_cloud.Open_drive(name_drive, id_drive);
                settings_json["Current_drive_id"] = Struct_cloud.IdDrive.ToString();

                //path = Path.Combine(name_drive, path);
                Change_ListView_drive("");
                Change_FullPath();
                this.Dispatcher.Invoke(() =>
                {
                    Progress_bar.Value++;
                });
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Progress_bar">ProgressBar</param>
        /// <param name="dir_list">List of where the items are located</param>
        /// <param name="name_drive">A name of current drive</param>
        private void Update_tree(ProgressBar Progress_bar, ProgressBar Progress_bar_add, string name_drive, long id_drive)
        {
            TdCloud.Search_all_files(Progress_bar, Progress_bar_add, id_drive);
            Struct_cloud.Update_tree(name_drive, id_drive);
            Struct_cloud.Save_drive(name_drive, id_drive);
            Change_ListView_drive("");
        }
        /// <summary>
        /// Select other drive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Select_new_drive(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                foreach (MenuItem item in Drives_list.Items)
                {
                    if (item.Header is TextBlock textblock && textblock.Text == Struct_cloud.NameDrive)
                    {
                        ((RadioButton)item.Icon).IsChecked = true;
                    }
                }
                return;
            }
            if (sender is TextBlock name)
            {
                foreach (MenuItem item in Drives_list.Items)
                {
                    if (item.Header is TextBlock textblock && textblock.Text == name.Text)
                    {
                        ((RadioButton)item.Icon).IsChecked = true;
                        return;
                    }
                }
            }
            foreach (MenuItem item in Drives_list.Items)
            {
                if (((RadioButton)item.Icon).IsChecked == true)
                {
                    if (Struct_cloud.IdDrive == long.Parse(((RadioButton)item.Icon).ToolTip.ToString()))
                        return;
                    var current_drive_name = ((TextBlock)item.Header).Text;
                    var current_drive_id = long.Parse(((TextBlock)item.Header).ToolTip.ToString());
                    //Struct_cloud.IdDrive = current_drive_id;
                    //Struct_cloud.NameDrive = current_drive_name;
                    if (Directory.Exists(current_drive_name))
                    {
                        Open_Tree(current_drive_name, current_drive_id);
                    }
                    else
                    {
                        mainThread = new Thread(o =>
                        {
                            Update_tree(Progress_bar, Progress_bar_add, current_drive_name, current_drive_id);
                            settings_json["Current_drive_id"] = Struct_cloud.IdDrive.ToString();
                            Change_FullPath();
                            Dispatcher.Run();
                        });
                        mainThread.Start();
                    }
                    //Settings_json.SaveDictionaryToJson(settings_json);
                }
            }
        }
        /// <summary>
        /// Update all items in current drive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_current_drive_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            foreach (MenuItem item in Drives_list.Items)
            {
                if (((RadioButton)item.Icon).IsChecked == true)
                {
                    var current_drive_name = ((TextBlock)item.Header).Text;
                    var current_drive_id = long.Parse(((TextBlock)item.Header).ToolTip.ToString());
                    mainThread = new Thread(o =>
                    {
                        Update_tree(Progress_bar, Progress_bar_add, current_drive_name, current_drive_id);
                        Change_FullPath();
                        Dispatcher.Run();
                    });
                    mainThread.Start();
                }
            }
        }
        /// <summary>
        /// Load all images id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Image_Preview_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            var is_OK = false;
            foreach (MenuItem drive_list_item in Drives_list.Items)
            {
                if (((RadioButton)drive_list_item.Icon).IsChecked == true)
                {
                    is_OK = true;
                    break;
                }
            }
            if (!is_OK)
                return;
            //Struct_cloud.Change_tree();
            if (TdCloud.all_messages_images_id[Struct_cloud.IdDrive].Count < 1)
                return;
            var all_id_images = new List<int>();
            for(int i = 0; i < TdCloud.all_messages_images_id[Struct_cloud.IdDrive].Count; i++)
            {
                if (TdCloud.images_formats.Any(TdCloud.all_images_paths[Struct_cloud.IdDrive][i].EndsWith))
                    all_id_images.Add(i);
            }
            if (all_id_images.Count < 1)
                return;
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = all_id_images.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;
            //Delete_directory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Struct_cloud.NameDrive));
            Struct_cloud.Save_drive(Struct_cloud.NameDrive, Struct_cloud.IdDrive);

            mainThread = new Thread(o =>
            {
                foreach (var i in all_id_images)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    TdCloud.Load_preview_images(Struct_cloud.IdDrive, i);
                    Change_ListView_drive(Struct_cloud.Get_path());

                    this.Dispatcher.Invoke(() =>
                    {
                        Progress_bar.Value++;
                    });
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Load image preview from current directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Current_Directory_Image_Preview_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            var is_OK = false;
            foreach (MenuItem drive_list_item in Drives_list.Items)
            {
                if (((RadioButton)drive_list_item.Icon).IsChecked == true)
                {
                    is_OK = true;
                }
            }
            if (!is_OK)
                return;
            //Struct_cloud.Change_tree();
            var all_children_id_with_path = Struct_cloud.Get_all_current_directory_childdrens_images_index();
            if (all_children_id_with_path.Count < 1)
                return;
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = all_children_id_with_path.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;
            //Delete_directory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Struct_cloud.NameDrive));
            Struct_cloud.Save_drive(Struct_cloud.NameDrive, Struct_cloud.IdDrive);
            mainThread = new Thread(o =>
            {
                foreach (var i in all_children_id_with_path)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        TdCloud.Load_preview_images(Struct_cloud.IdDrive, (int)i);
                    }
                    catch { }

                    Change_ListView_drive(Struct_cloud.Get_path());
                    this.Dispatcher.Invoke(() =>
                    {
                        Progress_bar.Value++;
                    });
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }

        /// <summary>
        /// Load all videos preview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Video_Preview_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            var is_OK = false;
            foreach (MenuItem drive_list_item in Drives_list.Items)
            {
                if (((RadioButton)drive_list_item.Icon).IsChecked == true)
                {
                    is_OK = true;
                }
            }
            if (!is_OK)
                return;
            //Struct_cloud.Change_tree();
            if (TdCloud.all_messages_images_id[Struct_cloud.IdDrive].Count < 1)
                return;
            var all_id_videos = new List<int>();
            for (int i = 0; i < TdCloud.all_messages_images_id[Struct_cloud.IdDrive].Count; i++)
            {
                if (TdCloud.videos_formats.Any(TdCloud.all_images_paths[Struct_cloud.IdDrive][i].EndsWith) || TdCloud.gif_format.Any(TdCloud.all_images_paths[Struct_cloud.IdDrive][i].EndsWith))
                    all_id_videos.Add(i);
            }
            if (all_id_videos.Count < 1)
                return;
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = all_id_videos.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;
            //Delete_directory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Struct_cloud.NameDrive));
            Struct_cloud.Save_drive(Struct_cloud.NameDrive, Struct_cloud.IdDrive);
            mainThread = new Thread(o =>
            {
                foreach (var i in all_id_videos)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        TdCloud.Load_preview_videos(Struct_cloud.IdDrive, i);
                    }
                    catch { }
                    Change_ListView_drive(Struct_cloud.Get_path());

                    this.Dispatcher.Invoke(() =>
                    {
                        Progress_bar.Value++;
                    });
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Load Video preview from current directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Current_Directory_Video_Preview_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            var is_OK = false;
            foreach (MenuItem drive_list_item in Drives_list.Items)
            {
                if (((RadioButton)drive_list_item.Icon).IsChecked == true)
                {
                    is_OK = true;
                }
            }
            if (!is_OK)
                return;
            //Struct_cloud.Change_tree();
            var all_children_id_with_path = Struct_cloud.Get_all_current_directory_childdrens_videos_index();
            if (all_children_id_with_path.Count < 1)
                return;
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = all_children_id_with_path.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;

            Struct_cloud.Save_drive(Struct_cloud.NameDrive, Struct_cloud.IdDrive);
            mainThread = new Thread(o =>
            {
                foreach (var i in all_children_id_with_path)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    TdCloud.Load_preview_videos(Struct_cloud.IdDrive, (int)i);
                    Change_ListView_drive(Struct_cloud.Get_path());

                    this.Dispatcher.Invoke(() =>
                    {
                        Progress_bar.Value++;
                    });
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Open file or go to directory with a doubleclick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Directory_list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((System.Windows.Controls.Primitives.Selector)sender).SelectedValue == null)
                return;

            if (!(sender is ListView listView)) return;
            var selectedItem = listView.SelectedValue;
            if (selectedItem == null) return;
            Point mousePosition = e.GetPosition(listView);
            var elementUnderMouse = listView.InputHitTest(mousePosition) as FrameworkElement;

            if (elementUnderMouse.DataContext == null || elementUnderMouse.DataContext.GetType() != typeof(ImageItem))
            {
                return;
            }

            if (open_with_one_click)
                return;
            
            string name = ((ImageItem)((System.Windows.Controls.Primitives.Selector)sender).SelectedItem).Name.ToString();
            //string name = ((System.Windows.Controls.Primitives.Selector)sender).SelectedValue.ToString();
            if (Struct_cloud.Get_format(name) == Formats.Directory)
            {
                new Thread(o =>
                {
                    Change_ListView_drive(Path.Combine(Struct_cloud.Get_path(), name));
                    Change_FullPath();
                    Dispatcher.Run();
                }).Start();
            }
            else
            {
                if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
                {
                    MessageBox.Show("Please wait for the process to complete");
                    return;
                }
                Progress_bar.ToolTip = PB_In_progress;
                Progress_bar_add.IsIndeterminate = true;
                Progress_bar.Maximum = 10;
                Progress_bar.Value = 0;
                Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Visible;
                TdCloud.is_file_open = false;
                var go = true;
                mainThread = new Thread(o =>
                {
                    while (!TdCloud.is_file_open && go)
                    {
                        if (mainCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        TdCloud.Open_file(Struct_cloud.IdDrive, Struct_cloud.Get_message_id(name));
                        this.Dispatcher.Invoke(() =>
                        {
                            if (Progress_bar.Value + 1 == Progress_bar.Maximum)
                                go = false;
                            Progress_bar.Value++;
                        });
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        if (Progress_bar.Maximum != 0)
                            Progress_bar.Value = Progress_bar.Maximum;
                    });
                    Dispatcher.Run();
                });
                mainThread.Start();
            }
        }
        /// <summary>
        /// Open file or go to directory with a click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Directory_list_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (((System.Windows.Controls.Primitives.Selector)sender).SelectedValue == null)
                return;

            if (!(sender is ListView listView)) return;
            var selectedItem = listView.SelectedValue;
            if (selectedItem == null) return;
            Point mousePosition = e.GetPosition(listView);
            var elementUnderMouse = listView.InputHitTest(mousePosition) as FrameworkElement;

            if (elementUnderMouse != null && elementUnderMouse.DataContext != null && elementUnderMouse.DataContext.GetType() == typeof(ImageItem))
            {
                string new_item = ((ImageItem)elementUnderMouse.DataContext).Name;
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (name_seletcedValues.Contains(new_item))
                    {
                        name_seletcedValues.Remove(new_item);
                    }
                    else
                    {
                        name_seletcedValues.Add(new_item);
                    }
                    if (open_with_one_click)
                        return;
                }
                else
                {
                    name_seletcedValues.Clear();
                    name_seletcedValues.Add(new_item);
                }
            }

            if (!open_with_one_click)
                return;

            if (elementUnderMouse != null && listView.ItemContainerGenerator.ContainerFromItem(selectedItem) is ListViewItem listViewItem)
            {
                if (!elementUnderMouse.IsDescendantOf(listViewItem))
                {
                    return;
                }
            }

            string name = ((ImageItem)((System.Windows.Controls.Primitives.Selector)sender).SelectedItem).Name.ToString();
            if (Struct_cloud.Get_format(name) == Formats.Directory)
            {
                new Thread(o =>
                {
                    Change_ListView_drive(Path.Combine(Struct_cloud.Get_path(), name));
                    Change_FullPath();
                    Dispatcher.Run();
                }).Start();
            }
            else
            {
                if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
                {
                    MessageBox.Show("Please wait for the process to complete");
                    return;
                }
                Progress_bar.ToolTip = PB_In_progress;
                Progress_bar_add.IsIndeterminate = true;
                Progress_bar.Maximum = 10;
                Progress_bar.Value = 0;
                Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Visible;
                TdCloud.is_file_open = false;
                var go = true;
                mainThread = new Thread(o =>
                {
                    while (!TdCloud.is_file_open && go)
                    {
                        if (mainCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        TdCloud.Open_file(Struct_cloud.IdDrive, Struct_cloud.Get_message_id(name));
                        this.Dispatcher.Invoke(() =>
                        {
                            if (Progress_bar.Value + 1 == Progress_bar.Maximum)
                                go = false;
                            Progress_bar.Value++;
                        });
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        if (Progress_bar.Maximum != 0)
                            Progress_bar.Value = Progress_bar.Maximum;
                    });
                    Dispatcher.Run();
                });
                mainThread.Start();
            }
        }

        private void Directory_list_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (((System.Windows.Controls.Primitives.Selector)sender).SelectedValue == null)
                return;

            if (!(sender is ListView listView)) return;
            var selectedItem = listView.SelectedValue;
            if (selectedItem == null) return;
            Point mousePosition = e.GetPosition(listView);
            var elementUnderMouse = listView.InputHitTest(mousePosition) as FrameworkElement;
            if (elementUnderMouse.DataContext != null && elementUnderMouse.DataContext.GetType() == typeof(ImageItem))
            {
                string new_item = ((ImageItem)elementUnderMouse.DataContext).Name;

                if (Keyboard.Modifiers != ModifierKeys.Control && !name_seletcedValues.Contains(new_item))
                {
                    name_seletcedValues.Clear();
                    name_seletcedValues.Add(new_item);
                }
            }
        }
        /// <summary>
        /// Drag Enter to directory list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Directory_list_DragEnter(object sender, DragEventArgs e)
        {
            if (TdCloud._haveAuthorization && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }
        /// <summary>
        /// Open a window if file allready exists
        /// </summary>
        private void If_file_exists_change_or_nothing()
        {
            Change_or_nothing chane_or_nothing_form = new Change_or_nothing();
            chane_or_nothing_form.ShowDialog();
        }
        /// <summary>
        /// Check size of file with path
        /// </summary>
        /// <param name="path"></param>
        private bool Can_load_file(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                return true;
            }
            long fileSizeInBytes = fileInfo.Length;
            double size_load_file_bytes = size_load_file_gigabytes * Math.Pow(1024, 3);
            if (fileSizeInBytes > size_load_file_bytes)
            {
                string message_text = "You can`t load a file " + Path.GetFileName(path) + "\nWith path: " + path + "\nBecause this file is too big.";
                MessageBox.Show(message_text);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Drop a files(s) or directory(s) to directory list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Directory_list_Drop(object sender, DragEventArgs e)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            if (!TdCloud._haveAuthorization)
                return;
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            if (Full_path.Text == "")
            {
                MessageBox.Show("Please select a drive");
                return;
            }
            Dictionary<string, string> msgs = new Dictionary<string, string>();
            string curr_path = Struct_cloud.Get_path();
            foreach (string path in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                if (Directory.Exists(path))
                    foreach (var item in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                    {
                        msgs.Add(item, Path.Combine(curr_path, Path.GetDirectoryName(item.Replace(Path.GetDirectoryName(path), "").Trim('\\'))));
                    }
                else
                    msgs.Add(path, curr_path);
            }
            if (msgs.Count == 0)
            {
                return;
            }
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = msgs.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;
            skip_all_next_files = false;
            change_file_in_drive = false;
            mainThread = new Thread(o =>
            {
                foreach (var msg in msgs)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    bool is_exist = Struct_cloud.Check_exist_in_children(Struct_cloud.NameDrive, msg.Value, Path.GetFileName(msg.Key));
                    if (is_exist)
                    {
                        name_exist_file = Path.GetFileName(msg.Key);
                        if (!skip_all_next_files)
                            this.Dispatcher.Invoke(() =>
                            {
                                If_file_exists_change_or_nothing();
                            });
                        if (!change_file_in_drive)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                Progress_bar.Value++;
                            });
                            continue;
                        }
                    }
                    if (is_exist)
                    {
                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, curr_path, Path.GetFileName(msg.Key));
                        TdCloud.Delete_file(Struct_cloud.IdDrive, new long[1] { Struct_cloud.Get_childdren_id_by_full_path(msg.Value, Path.GetFileName(msg.Key)) });
                        try
                        {
                            if (File.Exists(path))
                                File.Delete(path);
                        }
                        catch { }
                    }
                    if (!Can_load_file(msg.Key))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.Value++;
                        });
                        continue;
                    }
                    TdCloud.Send_message(Struct_cloud.IdDrive, msg.Key, msg.Value);
                    TdCloud.is_updated = false;
                    while (!TdCloud.is_updated)
                        TdCloud.Send_update_message(Struct_cloud.IdDrive);
                    if (isDownloadPhotoPreview)
                        TdCloud.Load_image(msg.Key, Struct_cloud.NameDrive, Struct_cloud.IdDrive);
                    if (isDownloadVideoPreview)
                        TdCloud.Load_video(msg.Key, Struct_cloud.NameDrive, Struct_cloud.IdDrive);
                    Struct_cloud.Update_tree();
                    Change_ListView_drive(Struct_cloud.Get_path());

                    this.Dispatcher.Invoke(() =>
                    {
                        Progress_bar.Value++;
                    });
                }
                //stopwatch.Stop();
                //long elapsedTimeMs = stopwatch.ElapsedMilliseconds;
                //Console.WriteLine($"Time: {elapsedTimeMs} milliseconds");
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Create a new directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_create_directory_Click(object sender, RoutedEventArgs e)
        {
            Create_directory c_dir = new Create_directory();
            c_dir.ShowDialog();
            Struct_cloud.Update_tree();
            //Struct_cloud.Change_tree(Progress_bar, Progress_bar_add);
            Change_ListView_drive(Struct_cloud.Get_path());
        }
        /// <summary>
        /// Add file(s)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_add_file_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                MessageBox.Show("You are not autorize");
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All files (*.*)|*.*",
                InitialDirectory = "c:\\"
            };
            string curr_path = Struct_cloud.Get_path();
            if (ofd.ShowDialog() == true)
            {
                Progress_bar.ToolTip = PB_In_progress;
                Progress_bar_add.IsIndeterminate = true;
                Progress_bar.Maximum = ofd.FileNames.Length;
                Progress_bar.Value = 0;
                Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Visible;
                skip_all_next_files = false;
                change_file_in_drive = false;
                mainThread = new Thread(o =>
                {
                    try
                    {
                        foreach (var msg in ofd.FileNames)
                        {
                            if (mainCancellationTokenSource.Token.IsCancellationRequested)
                            {
                                return;
                            }
                            bool is_exist = Struct_cloud.Check_exist_in_children(Struct_cloud.NameDrive, curr_path, Path.GetFileName(msg));
                            if (is_exist)
                            {
                                name_exist_file = Path.GetFileName(msg);
                                if (!skip_all_next_files)
                                    this.Dispatcher.Invoke(() =>
                                        {
                                            If_file_exists_change_or_nothing();
                                        });
                                if (!change_file_in_drive)
                                {
                                    this.Dispatcher.Invoke(() =>
                                        {
                                            Progress_bar.Value++;
                                        });
                                    continue;
                                }
                            }
                            if (is_exist)
                            {
                                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, curr_path, Path.GetFileName(msg));
                                TdCloud.Delete_file(Struct_cloud.IdDrive, new long[1] { Struct_cloud.Get_childdren_id_by_full_path(curr_path, msg) });

                                try
                                {
                                    if (File.Exists(path))
                                        File.Delete(path);
                                }
                                catch { }
                            }
                            if (!Can_load_file(msg))
                            {
                                this.Dispatcher.Invoke(() =>
                                    {
                                        Progress_bar.Value++;
                                    });
                                continue;
                            }
                            TdCloud.Send_message(Struct_cloud.IdDrive, msg, curr_path);
                            TdCloud.is_updated = false;
                            while (!TdCloud.is_updated)
                                TdCloud.Send_update_message(Struct_cloud.IdDrive);
                            if (isDownloadPhotoPreview)
                                TdCloud.Load_image(msg, Struct_cloud.NameDrive, Struct_cloud.IdDrive);
                            if (isDownloadVideoPreview)
                                TdCloud.Load_video(msg, Struct_cloud.NameDrive, Struct_cloud.IdDrive);
                            Struct_cloud.Update_tree();
                            Change_ListView_drive(Struct_cloud.Get_path());

                            this.Dispatcher.Invoke(() =>
                            {
                                Progress_bar.Value++;
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error with add file" + ex.ToString());
                    }
                    Dispatcher.Run();
                });
                mainThread.Start();
            }
        }
        /// <summary>
        /// Pin a message with file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_pin_Click(object sender, RoutedEventArgs e)
        {
            if (name_seletcedValues.Count == 0)
                return;
            foreach (var name in name_seletcedValues)
            {
                TdCloud.Pin_Message(Struct_cloud.IdDrive, Struct_cloud.Get_message_id(name));
            }
        }
        /// <summary>
        /// Unpin a message with file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_unpin_Click(object sender, RoutedEventArgs e)
        {
            if (name_seletcedValues.Count == 0)
                return;
            foreach (var name in name_seletcedValues)
            {
                TdCloud.Unpin_Message(Struct_cloud.IdDrive, Struct_cloud.Get_message_id(name));
            }
        }
        /// <summary>
        /// Download a file or a directory to PC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_download_Click(object sender, RoutedEventArgs e)
        {
            if (name_seletcedValues.Count == 0)
                return;
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            string selectedFolderPath = null;
            var fbd = new System.Windows.Forms.FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedFolderPath = fbd.SelectedPath;
            }
            if (selectedFolderPath == null)
                return;
            long count = 0;
            var names = new List<string>(name_seletcedValues);
            foreach (var item in names)
            {
                if (Struct_cloud.Get_format(item) == Formats.File)
                {
                    count++;
                }
                else
                {
                    count += Struct_cloud.Get_all_childdrens_id(item).Count;
                }
            }
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;
            mainThread = new Thread(o =>
            {
                foreach (var name in names)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    if (Struct_cloud.Get_format(name) == Formats.File)
                    {
                        try
                        {
                            TdCloud.Download_message_file(Struct_cloud.IdDrive, Struct_cloud.Get_message_id(name), selectedFolderPath);
                            this.Dispatcher.Invoke(() =>
                            {
                                Progress_bar.Value++;
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error with download file\n" + ex.ToString());
                        }
                    }
                    else
                    {
                        try
                        {
                            var all_childrens_path = Struct_cloud.Get_all_childrens_path(name);
                            var all_childrens_id = Struct_cloud.Get_all_childdrens_id(name);
                            for (int i = 0; i < all_childrens_id.Count; i++)
                            {
                                if (mainCancellationTokenSource.Token.IsCancellationRequested)
                                {
                                    return;
                                }
                                string path_tmp = Path.Combine(selectedFolderPath, all_childrens_path[i]);
                                string path = Path.GetDirectoryName(path_tmp);
                                Directory.CreateDirectory(path);
                                TdCloud.Download_message_file(Struct_cloud.IdDrive, all_childrens_id[i], path);
                                this.Dispatcher.Invoke(() =>
                                {
                                    Progress_bar.Value++;
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error with download directory\n" + ex.ToString());
                        }
                    }
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Download a preview of current file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_download_current_preview_Click(object sender, RoutedEventArgs e)
        {
            if (name_seletcedValues.Count == 0)
                return;
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            var names = new List<string>(name_seletcedValues);
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = names.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;

            mainThread = new Thread(o =>
            {
                foreach (var name in names)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    string curr_path = Struct_cloud.Get_path();
                    if (!TdCloud.images_formats.Any(name.EndsWith) && !TdCloud.videos_formats.Any(name.EndsWith) && !TdCloud.gif_format.Any(name.EndsWith))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.Value++;
                        });
                        continue;
                    }
                    try
                    {
                        if (Struct_cloud.Get_format(name) == Formats.File)
                        {
                            var tmp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Struct_cloud.NameDrive);
                            var file_path = Path.Combine(tmp_path, curr_path, name);
                            var drive_path = Path.Combine(Struct_cloud.NameDrive, curr_path, name);
                            if (File.Exists(file_path))
                                File.Delete(file_path);
                            var id = Struct_cloud.Get_message_id(name);
                            TdCloud.Load_one_preview(Struct_cloud.IdDrive, id, file_path, drive_path);
                        }
                        Change_ListView_drive(Struct_cloud.Get_path());

                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.Value++;
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error with download preview\n" + ex.ToString());
                    }
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Delete a preview of current file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_delete_current_preview_Click(object sender, RoutedEventArgs e)
        {
            if (name_seletcedValues.Count == 0)
                return;
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }

            var names = new List<string>(name_seletcedValues);
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = names.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;

            mainThread = new Thread(o =>
            {
                foreach (var name in names)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    if (!TdCloud.images_formats.Any(name.EndsWith) && !TdCloud.videos_formats.Any(name.EndsWith) && !TdCloud.gif_format.Any(name.EndsWith))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.Value++;
                        });
                        continue;
                    }
                    string curr_path = Struct_cloud.Get_path();
                    try
                    {
                        if (Struct_cloud.Get_format(name) == Formats.File)
                        {
                            var tmp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Struct_cloud.NameDrive);
                            var file_path = Path.Combine(tmp_path, curr_path, TdCloud.ChangeVideoToGif(name));
                            if (File.Exists(file_path))
                                File.Delete(file_path);
                        }
                        Change_ListView_drive(Struct_cloud.Get_path());

                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.Value++;
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error with delete preview\n" + ex.ToString());
                    }
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Move selected item in drive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_move_Click(object sender, RoutedEventArgs e)
        {
            if (name_seletcedValues.Count == 0)
                return;
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }

            new_text_message = null;
            skip_all_next_files = false;
            change_file_in_drive = false;
            Window_to_move new_mindow = new Window_to_move();
            new_mindow.ShowDialog();

            if (new_text_message == null)
                return;

            long count = 0;
            var names = new List<string>(name_seletcedValues);
            foreach (var item in names)
            {
                if (Struct_cloud.Get_format(item) == Formats.File)
                {
                    count++;
                }
                else
                {
                    count += Struct_cloud.Get_all_childdrens_id(item).Count;
                }
            }
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;

            string curr_path = Struct_cloud.Get_path();
            string move_drive_name = Window_to_move.move_drive_name;
            long move_drive_id = Window_to_move.move_drive_id;
            mainThread = new Thread(o =>
            {
                foreach (var name in names)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        if (Struct_cloud.Get_format(name) == Formats.File)
                        {
                            var tmp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tdlib", "tmp", new_text_message, Struct_cloud.NameDrive);
                            var file_path = Path.Combine(tmp_path, name);
                            bool is_exist = Struct_cloud.Check_exist_in_children(move_drive_name, new_text_message, Path.GetFileName(file_path));
                            if (is_exist)
                            {
                                name_exist_file = Path.GetFileName(file_path);
                                if (!skip_all_next_files)
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        If_file_exists_change_or_nothing();
                                    });
                                if (!change_file_in_drive)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Progress_bar.Value++;
                                    });
                                    continue;
                                }
                            }
                            if (is_exist)
                            {
                                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, move_drive_name, curr_path, file_path);
                                try
                                {
                                    if (File.Exists(path))
                                        File.Delete(path);
                                }
                                catch { }
                            }
                            TdCloud.Move_message(Struct_cloud.IdDrive, move_drive_id, Struct_cloud.Get_message_id(name), new_text_message);

                            if (is_exist)
                            {
                                try
                                {
                                    TdCloud.Delete_file(move_drive_id, new long[1] { Struct_cloud.Get_childdren_id_by_full_path(new_text_message, Path.GetFileName(file_path)) });
                                }
                                catch { }
                            }
                            this.Dispatcher.Invoke(() =>
                            {
                                Progress_bar.Value++;
                            });
                        }
                        else
                        {
                            var list_child_directory = Struct_cloud.Get_all_childrens_directoryes_id(name);
                            var list_child = Struct_cloud.Get_all_childdrens_id_with_path(name);
                            if (list_child.Count + list_child_directory.Count == 0)
                                continue;
                            foreach (var id in list_child_directory)
                            {
                                TdCloud.Delete_file(Struct_cloud.IdDrive, new long[1] { id });
                                this.Dispatcher.Invoke(() =>
                                {
                                    Progress_bar.Value++;
                                });
                            }
                            foreach (var id in list_child)
                            {
                                if (mainCancellationTokenSource.Token.IsCancellationRequested)
                                {
                                    return;
                                }
                                var tmp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tdlib", "tmp", Path.Combine(new_text_message, Path.GetDirectoryName(id.Value)));
                                var file_path = Path.Combine(tmp_path, Struct_cloud.Get_name_children_by_id(id.Key));
                                bool is_exist = Struct_cloud.Check_exist_in_children(move_drive_name, Path.Combine(new_text_message, Path.GetDirectoryName(id.Value)), Path.GetFileName(file_path));
                                if (is_exist)
                                {
                                    name_exist_file = Path.GetFileName(file_path);
                                    if (!skip_all_next_files)
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            If_file_exists_change_or_nothing();
                                        });
                                    if (!change_file_in_drive)
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            Progress_bar.Value++;
                                        });
                                        continue;
                                    }
                                }
                                if (is_exist)
                                {
                                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, curr_path, file_path);
                                    try
                                    {
                                        if (File.Exists(path))
                                            File.Delete(path);
                                    }
                                    catch { }
                                }
                                //id.Value - name
                                TdCloud.Move_message(Struct_cloud.IdDrive, move_drive_id, id.Key, Path.Combine(new_text_message, Path.GetDirectoryName(id.Value)));

                                if (is_exist)
                                {
                                    try
                                    {
                                        TdCloud.Delete_file(move_drive_id, new long[1] { Struct_cloud.Get_childdren_id_by_full_path(new_text_message, Path.GetFileName(file_path)) });
                                    }
                                    catch { }
                                }
                                if (Struct_cloud.IdDrive != move_drive_id)
                                {
                                    Change_ListView_drive(Struct_cloud.Get_path());
                                }
                                this.Dispatcher.Invoke(() =>
                                {
                                    Progress_bar.Value++;
                                });
                            }
                        }
                        if (Struct_cloud.IdDrive != move_drive_id)
                        {
                            Struct_cloud.Update_tree_another_node(move_drive_name, move_drive_id);
                        }
                        else
                        {
                            Struct_cloud.Update_tree();
                            Change_ListView_drive(Struct_cloud.Get_path());
                            Change_FullPath();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error with move\n" + ex.ToString());
                    }
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Rename a file or directory in current drive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_rename_Click(object sender, RoutedEventArgs e)
        {
            if (name_seletcedValues.Count == 0)
                return;
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            if (name_seletcedValues.Count > 1)
            {
                MessageBox.Show("You can't rename so many elements");
                return;
            }
            new_name_file = null;
            skip_all_next_files = false;
            change_file_in_drive = false;
            Rename_message Rename_message_text_form = new Rename_message();
            Rename_message_text_form.ShowDialog();
            if (new_name_file == null)
                return;

            string name_files = name_seletcedValues[0];

            string curr_path = Struct_cloud.Get_path();
            try
            {
                mainThread = new Thread(o =>
                {
                    if (Struct_cloud.Get_format(name_files) == Formats.File)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.ToolTip = PB_In_progress;
                            Progress_bar_add.IsIndeterminate = true;
                            Progress_bar.Maximum = 1;
                            Progress_bar.Value = 0;
                        });
                        var id = Struct_cloud.Get_message_id(name_files);
                        var old_path = Struct_cloud.Get_full_path_by_id(id);
                        new_name_file += Path.GetExtension(old_path);
                        bool is_exist = Struct_cloud.Check_exist_in_children(Struct_cloud.NameDrive, Path.GetDirectoryName(old_path), Path.GetFileName(new_name_file));
                        if (is_exist)
                        {
                            name_exist_file = Path.GetFileName(new_name_file);
                            if (!skip_all_next_files)
                                this.Dispatcher.Invoke(() =>
                                {
                                    If_file_exists_change_or_nothing();
                                });
                            if (!change_file_in_drive)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    Progress_bar.Value++;
                                });
                                return;
                            }
                        }
                        if (is_exist)
                        {
                            try
                            {
                                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Struct_cloud.NameDrive, curr_path, Path.GetFileName(new_name_file));
                                if (File.Exists(path))
                                    File.Delete(path);
                            }
                            catch { }
                        }
                        TdCloud.Rename_file(Struct_cloud.IdDrive, id, Path.GetDirectoryName(old_path), new_name_file);
                        if (is_exist)
                        {
                            try
                            {
                                TdCloud.Delete_file(Struct_cloud.IdDrive, new long[1] { Struct_cloud.Get_childdren_id_by_full_path(Path.GetDirectoryName(old_path), new_name_file) });
                            }
                            catch { }
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.Value++;
                        });
                    }
                    else
                    {
                        var old_path = Path.GetDirectoryName(Struct_cloud.Get_full_path_by_name_children(name_files));
                        var list_child_directory = Struct_cloud.Get_all_childrens_directoryes_id(old_path);
                        var list_child = Struct_cloud.Get_all_childdrens_id_with_path(old_path);
                        if (list_child.Count + list_child_directory.Count == 0)
                            return;
                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.ToolTip = PB_In_progress;
                            Progress_bar_add.IsIndeterminate = true;
                            Progress_bar.Maximum = list_child.Count + list_child_directory.Count;
                            Progress_bar.Value = 0;
                            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
                            Button_stop.Visibility = Visibility.Visible;
                        });
                        foreach (var id in list_child_directory)
                        {
                            TdCloud.Delete_file(Struct_cloud.IdDrive, new long[1] { id });
                            Change_ListView_drive(Struct_cloud.Get_path());

                            this.Dispatcher.Invoke(() =>
                            {
                                Progress_bar.Value++;
                            });
                        }
                        foreach (var id in list_child)
                        {
                            if (mainCancellationTokenSource.Token.IsCancellationRequested)
                            {
                                return;
                            }
                            var tmp_tmp_path = Path.GetDirectoryName(id.Value);
                            int index = tmp_tmp_path.IndexOf("\\");
                            var new_path = Path.Combine(curr_path, new_name_file);
                            if (index != -1)
                            {
                                new_path += tmp_tmp_path.Substring(index);
                            }
                            var tmp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tdlib", "tmp", new_path);
                            var file_path = Path.Combine(tmp_path, Struct_cloud.Get_name_children_by_id(id.Key));
                            bool is_exist = Struct_cloud.Check_exist_in_children(Struct_cloud.NameDrive, new_path, Path.GetFileName(id.Value));
                            if (is_exist)
                            {
                                name_exist_file = Path.GetFileName(file_path);
                                if (!skip_all_next_files)
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        If_file_exists_change_or_nothing();
                                    });
                                if (!change_file_in_drive)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Progress_bar.Value++;
                                    });
                                    continue;
                                }
                            }
                            if (is_exist)
                            {
                                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, curr_path, tmp_tmp_path, Path.GetFileName(file_path));
                                try
                                {
                                    if (File.Exists(path))
                                        File.Delete(path);
                                }
                                catch { }
                            }
                            TdCloud.Move_message(Struct_cloud.IdDrive, Struct_cloud.IdDrive, id.Key, new_path);
                            if (is_exist)
                            {
                                try
                                {
                                    TdCloud.Delete_file(Struct_cloud.IdDrive, new long[1] { Struct_cloud.Get_childdren_id_by_full_path(new_path, Path.GetFileName(file_path)) });
                                }
                                catch { }
                            }
                            Change_ListView_drive(Struct_cloud.Get_path());

                            this.Dispatcher.Invoke(() =>
                            {
                                Progress_bar.Value++;
                            });
                        }
                    }
                    Struct_cloud.Update_tree();
                    Change_ListView_drive(Struct_cloud.Get_path());
                    Change_FullPath();

                    Dispatcher.Run();
                });
                mainThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error with move\n" + ex.ToString());
            }
        }
        private void Button_stop_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if ((bool)e.NewValue)
                {
                    var circle = (Ellipse)Button_stop.Template.FindName("PART_Circle", Button_stop);
                    circle.StrokeDashOffset = 0;
                    var storyboard = (Storyboard)Button_stop.Resources["StoryboardButtonStop"];
                    Storyboard.SetTarget(storyboard, circle);
                }
            }
            catch { }
        }
        /// <summary>
        /// Delete a file or directory from current drive
        /// Start the timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_delete_Click(object sender, RoutedEventArgs e)
        {
            if (name_seletcedValues.Count == 0)
                return;
            if (Progress_bar.ToolTip.Equals(PB_In_progress))
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }

            List<string> names = name_seletcedValues.Except(name_seletcedValues_delete).ToList();

            var storyboard = (Storyboard)Button_stop.Resources["StoryboardButtonStop"];
            var circle = (Ellipse)Button_stop.Template.FindName("PART_Circle", Button_stop);
            Storyboard.SetTarget(storyboard, circle);
            if (timer.IsEnabled && name_seletcedValues.SequenceEqual(name_seletcedValues_delete))
            {
                timer.Stop();
                storyboard.Stop();
                Grid_Col_2.Width = new GridLength(0, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Hidden;
                Delete_files(name_seletcedValues_delete);
                name_seletcedValues_delete.Clear();
            }
            name_seletcedValues_delete = names;
            if (name_seletcedValues_delete.Count == 0)
            {
                return;
            }
            timerTicks = 0;
            timer.Start();
            storyboard.Begin();
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;
        }

        private void Button_stop_Click(object sender, RoutedEventArgs e)
        {
            name_seletcedValues_delete.Clear();
            if (timer.IsEnabled)
            {
                var storyboard = (Storyboard)Button_stop.Resources["StoryboardButtonStop"];
                var circle = (Ellipse)Button_stop.Template.FindName("PART_Circle", Button_stop);
                Storyboard.SetTarget(storyboard, circle);
                timer.Stop();
                storyboard.Stop();
                Grid_Col_2.Width = new GridLength(0, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Hidden;
            }
            else
            {
                if (mainCancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
                new Thread(o =>
                {
                    mainCancellationTokenSource.Cancel();
                    mainThread.Join();
                    mainCancellationTokenSource = new CancellationTokenSource();
                    this.Dispatcher.Invoke(() =>
                    {
                        Grid_Col_2.Width = new GridLength(0, GridUnitType.Star);
                        Button_stop.Visibility = Visibility.Hidden;
                        if (Progress_bar.ToolTip.Equals(PB_In_progress))
                            Progress_bar.Value = Progress_bar.Maximum;
                    });
                    Dispatcher.Run();
                }).Start();
            }
        }
        private void TdCloud_Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (name_seletcedValues.Count > 1)
                {
                    name_seletcedValues.RemoveRange(0, name_seletcedValues.Count - 1);
                }
                name_seletcedValues_delete.Clear();

                if (timer.IsEnabled)
                {
                    var storyboard = (Storyboard)Button_stop.Resources["StoryboardButtonStop"];
                    var circle = (Ellipse)Button_stop.Template.FindName("PART_Circle", Button_stop);
                    Storyboard.SetTarget(storyboard, circle);
                    timer.Stop();
                    storyboard.Stop();
                    Grid_Col_2.Width = new GridLength(0, GridUnitType.Star);
                    Button_stop.Visibility = Visibility.Hidden;
                }
                if (!timer.IsEnabled)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    new Thread(o =>
                    {
                        mainCancellationTokenSource.Cancel();
                        mainThread.Join();
                        mainCancellationTokenSource = new CancellationTokenSource();
                        this.Dispatcher.Invoke(() =>
                        {
                            Grid_Col_2.Width = new GridLength(0, GridUnitType.Star);
                            Button_stop.Visibility = Visibility.Hidden;
                            if (Progress_bar.ToolTip.Equals(PB_In_progress))
                                Progress_bar.Value = Progress_bar.Maximum;
                        });
                        Dispatcher.Run();
                    }).Start();
                }
            }
            if (e.Key == Key.Delete)
            {
                if (name_seletcedValues.Count == 0)
                    return;
                if (Progress_bar.ToolTip.Equals(PB_In_progress))
                {
                    MessageBox.Show("Please wait for the process to complete");
                    return;
                }

                List<string> names = name_seletcedValues.Except(name_seletcedValues_delete).ToList();

                var storyboard = (Storyboard)Button_stop.Resources["StoryboardButtonStop"];
                var circle = (Ellipse)Button_stop.Template.FindName("PART_Circle", Button_stop);
                Storyboard.SetTarget(storyboard, circle);
                if (timer.IsEnabled && name_seletcedValues.SequenceEqual(name_seletcedValues_delete))
                {
                    timer.Stop();
                    storyboard.Stop();
                    Grid_Col_2.Width = new GridLength(0, GridUnitType.Star);
                    Button_stop.Visibility = Visibility.Hidden;
                    Delete_files(name_seletcedValues_delete);
                    name_seletcedValues_delete.Clear();
                }
                name_seletcedValues_delete = names;
                if (name_seletcedValues_delete.Count == 0)
                {
                    return;
                }
                timerTicks = 0;
                timer.Start();
                storyboard.Begin();
                Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Visible;
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            double currentTime = timer.Interval.TotalSeconds * timerTicks;
            double strokeDashOffset = 23 - (23 * (currentTime / animationDuration));
            var circle = (Ellipse)Button_stop.Template.FindName("PART_Circle", Button_stop);
            circle.StrokeDashOffset = strokeDashOffset;
            var storyboard = (Storyboard)Button_stop.Resources["StoryboardButtonStop"];
            Storyboard.SetTarget(storyboard, circle);
            /*if (Progress_bar.ToolTip.Equals(PB_In_progress))
            {
                timer.Stop();
                storyboard.Stop();
                Grid_Col_2.Width = new GridLength(0, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Hidden;
                Delete_files(name_seletcedValues_delete);
                name_seletcedValues_delete.Clear();
                return;
            }*/

            if (currentTime >= animationDuration)
            {
                timer.Stop();
                storyboard.Stop();
                Grid_Col_2.Width = new GridLength(0, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Hidden;
                Delete_files(name_seletcedValues_delete);
                name_seletcedValues_delete.Clear();
            }
            else
            {
                timerTicks++;
            }
        }
        private void Delete_files(List<string> delete_files)
        {
            if (delete_files.Count == 0)
            {
                return;
            }
            ////  ARE YOU SHURE????????????

            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = delete_files.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;

            List<string> names = new List<string>(delete_files);
            string curr_path = Struct_cloud.Get_path();

            mainThread = new Thread(o =>
            {
                foreach (var name in names)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    if (Struct_cloud.Get_format(name) == Formats.File)
                    {
                        try
                        {
                            TdCloud.Delete_files(Struct_cloud.IdDrive, new long[1] { Struct_cloud.Get_message_id(name) });

                            var file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Struct_cloud.NameDrive, curr_path, TdCloud.ChangeVideoToGif(name));
                            if (File.Exists(file_path))
                                File.Delete(file_path);

                            Struct_cloud.Update_tree();
                            Change_ListView_drive(Struct_cloud.Get_path());
                            Change_FullPath();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error with delete file\n" + ex.ToString());
                        }
                    }
                    else
                    {
                        try
                        {
                            var all_childrens_id = Struct_cloud.Get_all_childrens_directoryes_id(name);
                            all_childrens_id.AddRange(Struct_cloud.Get_all_childdrens_id(name));

                            TdCloud.Delete_files(Struct_cloud.IdDrive, all_childrens_id.ToArray());

                            var file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, curr_path, TdCloud.ChangeVideoToGif(name));
                            if (File.Exists(file_path))
                                File.Delete(file_path);

                            Struct_cloud.Update_tree();
                            Change_ListView_drive(Struct_cloud.Get_path());
                            Change_FullPath();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error with delete directory\n" + ex.ToString());
                        }
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        Progress_bar.Value++;
                    });
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Properties of file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_item_properties_Click(object sender, RoutedEventArgs e)
        {
            if (name_seletcedValues.Count == 0)
                return;
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            List<string> names = name_seletcedValues;
            Progress_bar.ToolTip = MainWindow.PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = names.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;
            mainThread = new Thread(o =>
            {
                try
                {
                    foreach (var name in names)
                    {
                        if (mainCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        if (Struct_cloud.Get_format(name) == Formats.File)
                        {
                            TdCloud.Get_Properties(Struct_cloud.IdDrive, new long[1] { Struct_cloud.Get_message_id(name) });
                            this.Dispatcher.Invoke(() =>
                            {
                                Properties_file Properties_file_form = new Properties_file();
                                Properties_file_form.Show();
                            });
                        }
                        Progress_bar.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.Value++;
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error with delete file\n" + ex.ToString());
                    Progress_bar.Dispatcher.Invoke(() =>
                    {
                        if (!Progress_bar.ToolTip.Equals(PB_Ready))
                            Progress_bar.Value = Progress_bar.Maximum;
                    });
                }
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Download all drive to PC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_all_drive_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                MessageBox.Show("You are not autorize");
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            string selectedFolderPath = null;
            var fbd = new System.Windows.Forms.FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedFolderPath = fbd.SelectedPath;
            }
            if (selectedFolderPath == null)
                return;
            string name = Struct_cloud.NameDrive;
            try
            {
                var all_childrens_path = Struct_cloud.Get_all_paths();
                var all_childrens_id = Struct_cloud.Get_all_files_ids();
                Progress_bar.ToolTip = PB_In_progress;
                Progress_bar_add.IsIndeterminate = true;
                Progress_bar.Maximum = all_childrens_id.Count;
                Progress_bar.Value = 0;
                Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Visible;
                mainThread = new Thread(o =>
                {
                    for (int i = 0; i < all_childrens_id.Count; i++)
                    {
                        if (mainCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        string path_tmp = Path.Combine(selectedFolderPath, all_childrens_path[i]);
                        string path = Path.GetDirectoryName(path_tmp);
                        Directory.CreateDirectory(path);
                        TdCloud.Download_message_file(Struct_cloud.IdDrive, all_childrens_id[i], path);
                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.Value++;
                        });
                    }
                    Dispatcher.Run();
                });
                mainThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error with download full drive\n" + ex.ToString());
            }
        }
        /// <summary>
        /// Download current directory to PC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_current_directory_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                MessageBox.Show("You are not autorize");
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            string selectedFolderPath = null;
            var fbd = new System.Windows.Forms.FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedFolderPath = fbd.SelectedPath;
            }
            if (selectedFolderPath == null)
                return;
            string name = Struct_cloud.NameDrive;
            try
            {
                var all_childrens_path = Struct_cloud.Get_all_children_paths();
                var all_childrens_id = Struct_cloud.Get_all_children_files_ids();
                Progress_bar.ToolTip = PB_In_progress;
                Progress_bar_add.IsIndeterminate = true;
                Progress_bar.Maximum = all_childrens_id.Count;
                Progress_bar.Value = 0;
                Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Visible;
                mainThread = new Thread(o =>
                {
                    for (int i = 0; i < all_childrens_id.Count; i++)
                    {
                        if (mainCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        string path_tmp = Path.Combine(selectedFolderPath, all_childrens_path[i]);
                        string path = Path.GetDirectoryName(path_tmp);
                        Directory.CreateDirectory(path);
                        TdCloud.Download_message_file(Struct_cloud.IdDrive, all_childrens_id[i], path);
                        this.Dispatcher.Invoke(() =>
                        {
                            Progress_bar.Value++;
                        });
                    }
                    Dispatcher.Run();
                });
                mainThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error with download full drive\n" + ex.ToString());
            }
        }
        /// <summary>
        /// Delete all image previews
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_All_Images_Preview_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            var is_OK = false;
            foreach (MenuItem drive_list_item in Drives_list.Items)
            {
                if (((RadioButton)drive_list_item.Icon).IsChecked == true)
                {
                    is_OK = true;
                }
            }
            if (!is_OK)
                return;
            if (TdCloud.all_images_paths[Struct_cloud.IdDrive].Count < 1)
                return;
            var all_path_images = new List<string>();
            for (int i = 0; i < TdCloud.all_images_paths[Struct_cloud.IdDrive].Count; i++)
            {
                if (TdCloud.images_formats.Any(TdCloud.all_images_paths[Struct_cloud.IdDrive][i].EndsWith))
                    all_path_images.Add(TdCloud.all_images_paths[Struct_cloud.IdDrive][i]);
            }
            if (all_path_images.Count < 1)
                return;
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = all_path_images.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;
            Struct_cloud.Save_drive(Struct_cloud.NameDrive, Struct_cloud.IdDrive);
            mainThread = new Thread(o =>
            {
                foreach (var i in all_path_images)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        var path = Path.Combine(Struct_cloud.NameDrive, i);

                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch { }
                    this.Dispatcher.Invoke(() =>
                    {
                        Progress_bar.Value++;
                    });
                }
                Change_ListView_drive(Struct_cloud.Get_path());
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Delete all video previews
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_All_Videos_Preview_Click(object sender, RoutedEventArgs e)
        {
            if (!TdCloud._haveAuthorization)
            {
                return;
            }
            if (Progress_bar.ToolTip.Equals(PB_In_progress) || timer.IsEnabled)
            {
                MessageBox.Show("Please wait for the process to complete");
                return;
            }
            var is_OK = false;
            foreach (MenuItem drive_list_item in Drives_list.Items)
            {
                if (((RadioButton)drive_list_item.Icon).IsChecked == true)
                {
                    is_OK = true;
                }
            }
            if (!is_OK)
                return;
            if (TdCloud.all_images_paths[Struct_cloud.IdDrive].Count < 1)
                return;
            var all_path_images = new List<string>();
            for (int i = 0; i < TdCloud.all_images_paths[Struct_cloud.IdDrive].Count; i++)
            {
                if (TdCloud.videos_formats.Any(TdCloud.all_images_paths[Struct_cloud.IdDrive][i].EndsWith) || TdCloud.gif_format.Any(TdCloud.all_images_paths[Struct_cloud.IdDrive][i].EndsWith))
                    all_path_images.Add(TdCloud.all_images_paths[Struct_cloud.IdDrive][i]);
            }
            if (all_path_images.Count < 1)
                return;
            Progress_bar.ToolTip = PB_In_progress;
            Progress_bar_add.IsIndeterminate = true;
            Progress_bar.Maximum = all_path_images.Count;
            Progress_bar.Value = 0;
            Grid_Col_2.Width = new GridLength(0.1, GridUnitType.Star);
            Button_stop.Visibility = Visibility.Visible;
            Struct_cloud.Save_drive(Struct_cloud.NameDrive, Struct_cloud.IdDrive);
            mainThread = new Thread(o =>
            {
                foreach (var i in all_path_images)
                {
                    if (mainCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        var path = TdCloud.ChangeVideoToGif(Path.Combine(Struct_cloud.NameDrive, i));
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch { }
                    this.Dispatcher.Invoke(() =>
                    {
                        Progress_bar.Value++;
                    });
                }
                Change_ListView_drive(Struct_cloud.Get_path());
                Dispatcher.Run();
            });
            mainThread.Start();
        }
        /// <summary>
        /// Go parent directory in drive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_back_Click(object sender, RoutedEventArgs e)
        {
            if (Struct_cloud.IdDrive == 0)
                return;
            Struct_cloud.Get_parent();
            new Thread(o =>
            {
                Change_ListView_drive(Struct_cloud.Get_path());
                Change_FullPath();

                Dispatcher.Run();
            }).Start();
            
        }
        /// <summary>
        /// Stop a ProgressBar when progress end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Progress_bar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == Progress_bar.Maximum)
            {
                Progress_bar.ToolTip = PB_Ready;
                Progress_bar_add.IsIndeterminate = false;
                Progress_bar.Value = 0;

                Grid_Col_2.Width = new GridLength(0, GridUnitType.Star);
                Button_stop.Visibility = Visibility.Hidden;

                Text_complete.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                Text_complete.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// Download a preview automatically?
        /// </summary>
        private void Change_preview_automatically_Checked()
        {
            if (!settings_json.Keys.Contains("isDownloadPhotoPreview"))
            {
                settings_json.Add("isDownloadPhotoPreview", isDownloadPhotoPreview.ToString());
            }
            if (!settings_json.Keys.Contains("isDownloadVideoPreview"))
            {
                settings_json.Add("isDownloadVideoPreview", isDownloadPhotoPreview.ToString());
            }
            //Settings_json.SaveDictionaryToJson(settings_json);
            if (settings_json["isDownloadPhotoPreview"] == true.ToString())
            {
                Download_photo_previews_automatically.IsChecked = true;
            }
            if (settings_json["isDownloadVideoPreview"] == true.ToString())
            {
                Download_video_previews_automatically.IsChecked = true;
            }
        }
        /// <summary>
        /// Download a image preview when send a photo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_photo_previews_automatically_Checked(object sender, RoutedEventArgs e)
        {
            isDownloadPhotoPreview = true;
            if (!settings_json.Keys.Contains("isDownloadPhotoPreview"))
            {
                settings_json.Add("isDownloadPhotoPreview", isDownloadPhotoPreview.ToString());
            }
            settings_json["isDownloadPhotoPreview"] = isDownloadPhotoPreview.ToString();
            //Settings_json.SaveDictionaryToJson(settings_json);
        }
        /// <summary>
        /// Don`t download a image preview when send a photo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_photo_previews_automatically_Unchecked(object sender, RoutedEventArgs e)
        {
            isDownloadPhotoPreview = false;
            if (!settings_json.Keys.Contains("isDownloadPhotoPreview"))
            {
                settings_json.Add("isDownloadPhotoPreview", isDownloadPhotoPreview.ToString());
            }
            settings_json["isDownloadPhotoPreview"] = isDownloadPhotoPreview.ToString();
            //Settings_json.SaveDictionaryToJson(settings_json);
        }
        /// <summary>
        /// Download a video preview when send a video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_video_previews_automatically_Checked(object sender, RoutedEventArgs e)
        {
            isDownloadVideoPreview = true;
            if (!settings_json.Keys.Contains("isDownloadVideoPreview"))
            {
                settings_json.Add("isDownloadVideoPreview", isDownloadVideoPreview.ToString());
            }
            settings_json["isDownloadVideoPreview"] = isDownloadVideoPreview.ToString();
            //Settings_json.SaveDictionaryToJson(settings_json);
        }
        /// <summary>
        /// Don`t download a video preview when send a video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_video_previews_automatically_Unchecked(object sender, RoutedEventArgs e)
        {
            isDownloadVideoPreview = false;
            if (!settings_json.Keys.Contains("isDownloadVideoPreview"))
            {
                settings_json.Add("isDownloadVideoPreview", isDownloadVideoPreview.ToString());
            }
            settings_json["isDownloadVideoPreview"] = isDownloadVideoPreview.ToString();
            //Settings_json.SaveDictionaryToJson(settings_json);
        }
        /// <summary>
        /// Open file or go to directory automatically?
        /// </summary>
        private void Change_open_click_or_double_click()
        {
            if (!settings_json.Keys.Contains("open_with_one_click"))
            {
                settings_json.Add("open_with_one_click", open_with_one_click.ToString());
            }
            if (settings_json["open_with_one_click"] == true.ToString())
            {
                Open_click_or_double_click.IsChecked = true;
            }
        }
        /// <summary>
        /// Open files and directories with one click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_click_or_double_click_Checked(object sender, RoutedEventArgs e)
        {
            open_with_one_click = true;
            if (!settings_json.Keys.Contains("open_with_one_click"))
            {
                settings_json.Add("open_with_one_click", open_with_one_click.ToString());
            }
            settings_json["open_with_one_click"] = open_with_one_click.ToString();
        }
        /// <summary>
        /// Open files and directories with double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_click_or_double_click_Unchecked(object sender, RoutedEventArgs e)
        {
            open_with_one_click = false;
            if (!settings_json.Keys.Contains("open_with_one_click"))
            {
                settings_json.Add("open_with_one_click", open_with_one_click.ToString());
            }
            settings_json["open_with_one_click"] = open_with_one_click.ToString();
        }
        private void Dark_theme_Checked(object sender, RoutedEventArgs e)
        {
            is_dark_theme = true;
            if (!settings_json.Keys.Contains("Dark_theme"))
            {
                settings_json.Add("Dark_theme", is_dark_theme.ToString());
            }
            settings_json["Dark_theme"] = is_dark_theme.ToString();
            Change_theme();
        }

        private void Dark_theme_Unchecked(object sender, RoutedEventArgs e)
        {
            is_dark_theme = false;
            if (!settings_json.Keys.Contains("Dark_theme"))
            {
                settings_json.Add("Dark_theme", is_dark_theme.ToString());
            }
            settings_json["Dark_theme"] = is_dark_theme.ToString();
            Change_theme();
        }
        /// <summary>
        /// Chenge language start program
        /// </summary>
        private void Change_language_Initialization()
        {
            if (!settings_json.Keys.Contains("language"))
            {
                settings_json.Add("language", Languages.en.ToString());
            }
            if (settings_json["language"] == Languages.en.ToString())
            {
                current_lang = Languages.en;
            }
            else if (settings_json["language"] == Languages.ua.ToString())
            {
                current_lang = Languages.ua;
            }
            else if (settings_json["language"] == Languages.ru.ToString())
            {
                current_lang = Languages.ru;
            }
            Change_language();
        }
        /// <summary>
        /// Change language in all items
        /// </summary>
        private void Change_language()
        {
            if (current_lang == Languages.en)
            {
                Radio_button_settings_language_english.IsChecked = true;
                Account.Header = Telegram_cloud.Resources.Resource_english.Account_Header;
                Menu_autorize.Header = Telegram_cloud.Resources.Resource_english.Menu_autorize_Header;
                About_account.Header = Telegram_cloud.Resources.Resource_english.About_account_Header;
                Log_out_account.Header = Telegram_cloud.Resources.Resource_english.Log_out_account_Header;
                Log_out_account.ToolTip = Telegram_cloud.Resources.Resource_english.Log_out_account_ToolTip;
                Drives_list.Header = Telegram_cloud.Resources.Resource_english.Drives_list_Header;
                Functions.Header = Telegram_cloud.Resources.Resource_english.Functions_Header;
                Settings.Header = Telegram_cloud.Resources.Resource_english.Settings_Header;
                Add_drive.Header = Telegram_cloud.Resources.Resource_english.Add_drive_Header;
                Add_drive.ToolTip = Telegram_cloud.Resources.Resource_english.Add_drive_ToolTip;
                Add_drive_from_list.Header = Telegram_cloud.Resources.Resource_english.Add_drive_from_list_Header;
                Add_drive_from_list.ToolTip = Telegram_cloud.Resources.Resource_english.Add_drive_from_list_ToolTip;
                Update_current_drive.Header = Telegram_cloud.Resources.Resource_english.Update_current_drive_Header;
                Update_current_drive.ToolTip = Telegram_cloud.Resources.Resource_english.Update_current_drive_ToolTip;
                Load_Image_Preview.Header = Telegram_cloud.Resources.Resource_english.Load_Image_Preview_Header;
                Load_Image_Preview.ToolTip = Telegram_cloud.Resources.Resource_english.Load_Image_Preview_ToolTip;
                Load_Video_Preview.Header = Telegram_cloud.Resources.Resource_english.Load_Video_Preview_Header;
                Load_Video_Preview.ToolTip = Telegram_cloud.Resources.Resource_english.Load_Video_Preview_ToolTip;
                Load_Current_Directory_Image_Preview.Header = Telegram_cloud.Resources.Resource_english.Load_Current_Directory_Image_Preview_Header;
                Load_Current_Directory_Image_Preview.ToolTip = Telegram_cloud.Resources.Resource_english.Load_Current_Directory_Image_Preview_ToolTip;
                Load_Current_Directory_Video_Preview.Header = Telegram_cloud.Resources.Resource_english.Load_Current_Directory_Video_Preview_Header;
                Load_Current_Directory_Video_Preview.ToolTip = Telegram_cloud.Resources.Resource_english.Load_Current_Directory_Video_Preview_ToolTip;
                Delete_All_Images_Preview.Header = Telegram_cloud.Resources.Resource_english.Delete_All_Images_Preview_Header;
                Delete_All_Images_Preview.ToolTip = Telegram_cloud.Resources.Resource_english.Delete_All_Images_Preview_ToolTip;
                Delete_All_Videos_Preview.Header = Telegram_cloud.Resources.Resource_english.Delete_All_Videos_Preview_Header;
                Delete_All_Videos_Preview.ToolTip = Telegram_cloud.Resources.Resource_english.Delete_All_Videos_Preview_ToolTip;
                Download_all_drive.Header = Telegram_cloud.Resources.Resource_english.Download_all_drive_Header;
                Download_all_drive.ToolTip = Telegram_cloud.Resources.Resource_english.Download_all_drive_ToolTip;
                Download_current_directory.Header = Telegram_cloud.Resources.Resource_english.Download_current_directory_Header;
                Download_current_directory.ToolTip = Telegram_cloud.Resources.Resource_english.Download_current_directory_ToolTip;
                Settings_language.Header = Telegram_cloud.Resources.Resource_english.Settings_language_Header;
                Text_block_Download_photo_previews_automatically.Text = Telegram_cloud.Resources.Resource_english.Text_block_Download_photo_previews_automatically_Text;
                Download_photo_previews_automatically.ToolTip = Telegram_cloud.Resources.Resource_english.Download_photo_previews_automatically_ToolTip;
                Text_block_Download_video_previews_automatically.Text = Telegram_cloud.Resources.Resource_english.Text_block_Download_video_previews_automatically_Text;
                Download_video_previews_automatically.ToolTip = Telegram_cloud.Resources.Resource_english.Download_video_previews_automatically_ToolTip;
                About_program.Header = Telegram_cloud.Resources.Resource_english.About_program_Header;
                About_program.ToolTip = Telegram_cloud.Resources.Resource_english.About_program_ToolTip;
                if (Progress_bar.ToolTip.Equals(PB_Ready))
                {
                    Progress_bar.ToolTip = Telegram_cloud.Resources.Resource_english.Progress_bar_ToolTip_Ready;
                }
                else
                {
                    Progress_bar.ToolTip = Telegram_cloud.Resources.Resource_english.Progress_bar_ToolTip_In_progress;
                }
                PB_In_progress = Telegram_cloud.Resources.Resource_english.Progress_bar_ToolTip_In_progress;
                PB_Ready = Telegram_cloud.Resources.Resource_english.Progress_bar_ToolTip_Ready;
                Text_complete.Text = Telegram_cloud.Resources.Resource_english.Text_complete_Text;
                Menu_item_create_directory.Header = Telegram_cloud.Resources.Resource_english.Menu_item_create_directory_Header;
                Menu_item_add_file.Header = Telegram_cloud.Resources.Resource_english.Menu_item_add_file_Header;
                Menu_item_pin.Header = Telegram_cloud.Resources.Resource_english.Menu_item_pin_Header;
                Menu_item_unpin.Header = Telegram_cloud.Resources.Resource_english.Menu_item_unpin_Header;
                Menu_item_download.Header = Telegram_cloud.Resources.Resource_english.Menu_item_download_Header;
                Menu_item_download_current_preview.Header = Telegram_cloud.Resources.Resource_english.Menu_item_download_current_preview_Header;
                Menu_item_delete_current_preview.Header = Telegram_cloud.Resources.Resource_english.Menu_item_delete_current_preview_Header;
                Menu_item_delete.Header = Telegram_cloud.Resources.Resource_english.Menu_item_delete_Header;
                Menu_item_move.Header = Telegram_cloud.Resources.Resource_english.Menu_item_move_Header;
                Menu_item_rename.Header = Telegram_cloud.Resources.Resource_english.Menu_item_rename_Header;
            }
            else if (current_lang == Languages.ua)
            {
                Radio_button_settings_language_Ukrainian.IsChecked = true;
                Account.Header = Telegram_cloud.Resources.Resource_ukrainian.Account_Header;
                Menu_autorize.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_autorize_Header;
                About_account.Header = Telegram_cloud.Resources.Resource_ukrainian.About_account_Header;
                Log_out_account.Header = Telegram_cloud.Resources.Resource_ukrainian.Log_out_account_Header;
                Log_out_account.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Log_out_account_ToolTip;
                Drives_list.Header = Telegram_cloud.Resources.Resource_ukrainian.Drives_list_Header;
                Functions.Header = Telegram_cloud.Resources.Resource_ukrainian.Functions_Header;
                Settings.Header = Telegram_cloud.Resources.Resource_ukrainian.Settings_Header;
                Add_drive.Header = Telegram_cloud.Resources.Resource_ukrainian.Add_drive_Header;
                Add_drive.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Add_drive_ToolTip;
                Add_drive_from_list.Header = Telegram_cloud.Resources.Resource_ukrainian.Add_drive_from_list_Header;
                Add_drive_from_list.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Add_drive_from_list_ToolTip;
                Update_current_drive.Header = Telegram_cloud.Resources.Resource_ukrainian.Update_current_drive_Header;
                Update_current_drive.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Update_current_drive_ToolTip;
                Load_Image_Preview.Header = Telegram_cloud.Resources.Resource_ukrainian.Load_Image_Preview_Header;
                Load_Image_Preview.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Load_Image_Preview_ToolTip;
                Load_Video_Preview.Header = Telegram_cloud.Resources.Resource_ukrainian.Load_Video_Preview_Header;
                Load_Video_Preview.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Load_Video_Preview_ToolTip;
                Load_Current_Directory_Image_Preview.Header = Telegram_cloud.Resources.Resource_ukrainian.Load_Current_Directory_Image_Preview_Header;
                Load_Current_Directory_Image_Preview.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Load_Current_Directory_Image_Preview_ToolTip;
                Load_Current_Directory_Video_Preview.Header = Telegram_cloud.Resources.Resource_ukrainian.Load_Current_Directory_Video_Preview_Header;
                Load_Current_Directory_Video_Preview.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Load_Current_Directory_Video_Preview_ToolTip;
                Delete_All_Images_Preview.Header = Telegram_cloud.Resources.Resource_ukrainian.Delete_All_Images_Preview_Header;
                Delete_All_Images_Preview.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Delete_All_Images_Preview_ToolTip;
                Delete_All_Videos_Preview.Header = Telegram_cloud.Resources.Resource_ukrainian.Delete_All_Videos_Preview_Header;
                Delete_All_Videos_Preview.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Delete_All_Videos_Preview_ToolTip;
                Download_all_drive.Header = Telegram_cloud.Resources.Resource_ukrainian.Download_all_drive_Header;
                Download_all_drive.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Download_all_drive_ToolTip;
                Download_current_directory.Header = Telegram_cloud.Resources.Resource_ukrainian.Download_current_directory_Header;
                Download_current_directory.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Download_current_directory_ToolTip;
                Settings_language.Header = Telegram_cloud.Resources.Resource_ukrainian.Settings_language_Header;
                Text_block_Download_photo_previews_automatically.Text = Telegram_cloud.Resources.Resource_ukrainian.Text_block_Download_photo_previews_automatically_Text;
                Download_photo_previews_automatically.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Download_photo_previews_automatically_ToolTip;
                Text_block_Download_video_previews_automatically.Text = Telegram_cloud.Resources.Resource_ukrainian.Text_block_Download_video_previews_automatically_Text;
                Download_video_previews_automatically.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Download_video_previews_automatically_ToolTip;
                About_program.Header = Telegram_cloud.Resources.Resource_ukrainian.About_program_Header;
                About_program.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.About_program_ToolTip;
                if (Progress_bar.ToolTip.Equals(PB_Ready))
                {
                    Progress_bar.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Progress_bar_ToolTip_Ready;
                }
                else
                {
                    Progress_bar.ToolTip = Telegram_cloud.Resources.Resource_ukrainian.Progress_bar_ToolTip_In_progress;
                }
                PB_In_progress = Telegram_cloud.Resources.Resource_ukrainian.Progress_bar_ToolTip_In_progress;
                PB_Ready = Telegram_cloud.Resources.Resource_ukrainian.Progress_bar_ToolTip_Ready;
                Text_complete.Text = Telegram_cloud.Resources.Resource_ukrainian.Text_complete_Text;
                Menu_item_create_directory.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_create_directory_Header;
                Menu_item_add_file.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_add_file_Header;
                Menu_item_pin.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_pin_Header;
                Menu_item_unpin.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_unpin_Header;
                Menu_item_download.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_download_Header;
                Menu_item_download_current_preview.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_download_current_preview_Header;
                Menu_item_delete_current_preview.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_delete_current_preview_Header;
                Menu_item_delete.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_delete_Header;
                Menu_item_move.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_move_Header;
                Menu_item_rename.Header = Telegram_cloud.Resources.Resource_ukrainian.Menu_item_rename_Header;
            }
            else if (current_lang == Languages.ru)
            {
                Radio_button_settings_language_russian.IsChecked = true;
                Account.Header = Telegram_cloud.Resources.Resource_russian.Account_Header;
                Menu_autorize.Header = Telegram_cloud.Resources.Resource_russian.Menu_autorize_Header;
                About_account.Header = Telegram_cloud.Resources.Resource_russian.About_account_Header;
                Log_out_account.Header = Telegram_cloud.Resources.Resource_russian.Log_out_account_Header;
                Log_out_account.ToolTip = Telegram_cloud.Resources.Resource_russian.Log_out_account_ToolTip;
                Drives_list.Header = Telegram_cloud.Resources.Resource_russian.Drives_list_Header;
                Functions.Header = Telegram_cloud.Resources.Resource_russian.Functions_Header;
                Settings.Header = Telegram_cloud.Resources.Resource_russian.Settings_Header;
                Add_drive.Header = Telegram_cloud.Resources.Resource_russian.Add_drive_Header;
                Add_drive.ToolTip = Telegram_cloud.Resources.Resource_russian.Add_drive_ToolTip;
                Add_drive_from_list.Header = Telegram_cloud.Resources.Resource_russian.Add_drive_from_list_Header;
                Add_drive_from_list.ToolTip = Telegram_cloud.Resources.Resource_russian.Add_drive_from_list_ToolTip;
                Update_current_drive.Header = Telegram_cloud.Resources.Resource_russian.Update_current_drive_Header;
                Update_current_drive.ToolTip = Telegram_cloud.Resources.Resource_russian.Update_current_drive_ToolTip;
                Load_Image_Preview.Header = Telegram_cloud.Resources.Resource_russian.Load_Image_Preview_Header;
                Load_Image_Preview.ToolTip = Telegram_cloud.Resources.Resource_russian.Load_Image_Preview_ToolTip;
                Load_Video_Preview.Header = Telegram_cloud.Resources.Resource_russian.Load_Video_Preview_Header;
                Load_Video_Preview.ToolTip = Telegram_cloud.Resources.Resource_russian.Load_Video_Preview_ToolTip;
                Load_Current_Directory_Image_Preview.Header = Telegram_cloud.Resources.Resource_russian.Load_Current_Directory_Image_Preview_Header;
                Load_Current_Directory_Image_Preview.ToolTip = Telegram_cloud.Resources.Resource_russian.Load_Current_Directory_Image_Preview_ToolTip;
                Load_Current_Directory_Video_Preview.Header = Telegram_cloud.Resources.Resource_russian.Load_Current_Directory_Video_Preview_Header;
                Load_Current_Directory_Video_Preview.ToolTip = Telegram_cloud.Resources.Resource_russian.Load_Current_Directory_Video_Preview_ToolTip;
                Delete_All_Images_Preview.Header = Telegram_cloud.Resources.Resource_russian.Delete_All_Images_Preview_Header;
                Delete_All_Images_Preview.ToolTip = Telegram_cloud.Resources.Resource_russian.Delete_All_Images_Preview_ToolTip;
                Delete_All_Videos_Preview.Header = Telegram_cloud.Resources.Resource_russian.Delete_All_Videos_Preview_Header;
                Delete_All_Videos_Preview.ToolTip = Telegram_cloud.Resources.Resource_russian.Delete_All_Videos_Preview_ToolTip;
                Download_all_drive.Header = Telegram_cloud.Resources.Resource_russian.Download_all_drive_Header;
                Download_all_drive.ToolTip = Telegram_cloud.Resources.Resource_russian.Download_all_drive_ToolTip;
                Download_current_directory.Header = Telegram_cloud.Resources.Resource_russian.Download_current_directory_Header;
                Download_current_directory.ToolTip = Telegram_cloud.Resources.Resource_russian.Download_current_directory_ToolTip;
                Settings_language.Header = Telegram_cloud.Resources.Resource_russian.Settings_language_Header;
                Text_block_Download_photo_previews_automatically.Text = Telegram_cloud.Resources.Resource_russian.Text_block_Download_photo_previews_automatically_Text;
                Download_photo_previews_automatically.ToolTip = Telegram_cloud.Resources.Resource_russian.Download_photo_previews_automatically_ToolTip;
                Text_block_Download_video_previews_automatically.Text = Telegram_cloud.Resources.Resource_russian.Text_block_Download_video_previews_automatically_Text;
                Download_video_previews_automatically.ToolTip = Telegram_cloud.Resources.Resource_russian.Download_video_previews_automatically_ToolTip;
                About_program.Header = Telegram_cloud.Resources.Resource_russian.About_program_Header;
                About_program.ToolTip = Telegram_cloud.Resources.Resource_russian.About_program_ToolTip;
                if (Progress_bar.ToolTip.Equals(PB_Ready))
                {
                    Progress_bar.ToolTip = Telegram_cloud.Resources.Resource_russian.Progress_bar_ToolTip_Ready;
                }
                else
                {
                    Progress_bar.ToolTip = Telegram_cloud.Resources.Resource_russian.Progress_bar_ToolTip_In_progress;
                }
                PB_In_progress = Telegram_cloud.Resources.Resource_russian.Progress_bar_ToolTip_In_progress;
                PB_Ready = Telegram_cloud.Resources.Resource_russian.Progress_bar_ToolTip_Ready;
                Text_complete.Text = Telegram_cloud.Resources.Resource_russian.Text_complete_Text;
                Menu_item_create_directory.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_create_directory_Header;
                Menu_item_add_file.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_add_file_Header;
                Menu_item_pin.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_pin_Header;
                Menu_item_unpin.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_unpin_Header;
                Menu_item_download.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_download_Header;
                Menu_item_download_current_preview.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_download_current_preview_Header;
                Menu_item_delete_current_preview.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_delete_current_preview_Header;
                Menu_item_delete.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_delete_Header;
                Menu_item_move.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_move_Header;
                Menu_item_rename.Header = Telegram_cloud.Resources.Resource_russian.Menu_item_rename_Header;
            }
            settings_json["language"] = current_lang.ToString();
            //Settings_json.SaveDictionaryToJson(settings_json);
        }
        /// <summary>
        /// Click the English button in the language menu of the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_language_english_Click(object sender, RoutedEventArgs e)
        {
            current_lang = Languages.en;
            Change_language();
        }
        /// <summary>
        /// Click the Ukrainian button in the language menu of the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_language_Ukrainian_Click(object sender, RoutedEventArgs e)
        {
            current_lang = Languages.ua;
            Change_language();
        }
        /// <summary>
        /// Click the russian button in the language menu of the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_language_russian_Click(object sender, RoutedEventArgs e)
        {
            current_lang = Languages.ru;
            Change_language();
        }
        /// <summary>
        /// Click the "about program" button of the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_program_Click(object sender, RoutedEventArgs e)
        {
            About_program about_program_form = new About_program();
            about_program_form.ShowDialog();
        }
        private void Directory_list_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_scale > 2 && e.Delta > 0)
                    return;
                if (_scale < 0.2 && e.Delta < 0)
                    return;
                _scale += e.Delta > 0 ? 0.1 : -0.1;
                settings_json["_scale"] = _scale.ToString();
                Change_size_Dir_List();

            }
        }
        private void Change_size_Dir_List()
        {
            if (Directory_list.Items == null)
            {
                return;
            }
            if (settings_json["_scale"] != _scale.ToString())
            {
                double.TryParse(settings_json["_scale"], out _scale);
            }
            foreach (var item in Directory_list.Items)
            {
                ListViewItem listViewItem = (ListViewItem)Directory_list.ItemContainerGenerator.ContainerFromItem(item);
                if (listViewItem != null)
                {
                    var stackPanel = FindVisualChild<StackPanel>(listViewItem);
                    if (stackPanel != null)
                    {
                        var image = FindVisualChild<Image>(stackPanel);
                        var Grid_textBlock = FindVisualChild<Grid>(stackPanel);
                        var textBlock = FindVisualChild<TextBlock>(stackPanel);

                        stackPanel.Height = 135 * _scale;
                        image.Width = 100 * _scale;
                        image.Height = 100 * _scale;
                        if (_scale <= 0.5)
                        {
                            stackPanel.Orientation = Orientation.Horizontal;
                            stackPanel.Width = Directory_list.ActualWidth - 135 * _scale;

                            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                            Grid_textBlock.Width = Directory_list.ActualWidth - 135 * _scale;
                        }
                        else
                        {
                            stackPanel.Orientation = Orientation.Vertical;
                            stackPanel.Width = 135 * _scale;

                            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                            Grid_textBlock.Width = 100 * _scale;
                        }
                    }
                }
            }
        }
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem item)
                    return item;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void TdCloud_Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Change_size_Dir_List();
        }
        private void Directory_list_LayoutUpdated(object sender, EventArgs e)
        {
            if (need_change_size)
            {
                need_change_size = false;
                Change_size_Dir_List();
            }
        }
        private void Change_theme()
        {
            Color gridColor, menuItemColor, menuitemHoverColor, buttonColor, buttonHoverColor, textboxColor, textboxHoverColor, foregroundColor, foregroundHoverColor;
            if (is_dark_theme)
            {
                // (Color)ColorConverter.ConvertFromString("#201c1c");
                gridColor = Colors.Black;
                menuItemColor = Colors.Black;
                menuitemHoverColor = Colors.White;
                buttonColor = Colors.Black;
                buttonHoverColor = Colors.White;
                textboxColor = Colors.Black;
                textboxHoverColor = Colors.White;
                foregroundColor = Colors.White;
                foregroundHoverColor = Colors.Black;
            }
            else
            {
                gridColor = Colors.White;
                menuItemColor = Colors.White;
                menuitemHoverColor = Colors.Black;
                buttonColor = Colors.White;
                buttonHoverColor = Colors.Black;
                textboxColor = Colors.White;
                textboxHoverColor = Colors.Black;
                foregroundColor = Colors.Black;
                foregroundHoverColor = Colors.White;
            }
            this.DataContext = new MyViewModel()
            {
                AccountImage = "/account.png",
                GridColor = new SolidColorBrush(gridColor),
                MenuItemColor = new SolidColorBrush(menuItemColor),
                MenuItemHoverColor = new SolidColorBrush(menuitemHoverColor),
                ButtonColor = new SolidColorBrush(buttonColor),
                ButtonHoverColor = new SolidColorBrush(buttonHoverColor),
                TextBoxColor = new SolidColorBrush(textboxColor),
                TextBoxHoverColor = new SolidColorBrush(textboxHoverColor),
                ForeGroundColor = new SolidColorBrush(foregroundColor),
                ForeGroundHoverColor = new SolidColorBrush(foregroundHoverColor)
            };
        }
    }
    /// <summary>
    /// Class to show all items in directory list
    /// </summary>
    public class ImageItem
    {
        /// <summary>
        /// Text of item
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Path to image or gif
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// Create a new image source
        /// </summary>
        public BitmapImage Image_src { 
            get
            {
                return Convert(this.Image);
            }
            set
            {
                this.Image_src = Convert(this.Image);
            }
        }
        /// <summary>
        /// Convert image to Bitmap
        /// </summary>
        /// <param name="file_path"></param>
        /// <returns></returns>
        private BitmapImage Convert(string file_path)
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(file_path);
                image.EndInit();
                return image;
            }
            catch 
            {
                return null;
            }
        }
    }
    public class MyViewModel : INotifyPropertyChanged
    {
        private string _accountImage;
        private SolidColorBrush _gridColor;
        private SolidColorBrush _menuItemColor;
        private SolidColorBrush _menuItemHoverColor;
        private SolidColorBrush _buttonColor;
        private SolidColorBrush _buttonHoverColor;
        private SolidColorBrush _textBoxColor;
        private SolidColorBrush _textBoxHoverColor;
        private SolidColorBrush _foreGroundColor;
        private SolidColorBrush _foreGroundHoverColor;
        
        public string AccountImage
        {
            get { return _accountImage; }
            set
            {
                _accountImage = value;
                OnPropertyChanged("AccountImage");
            }
        }
        public SolidColorBrush GridColor
        {
            get { return _gridColor; }
            set
            {
                _gridColor = value;
                OnPropertyChanged("GridColor");
            }
        }
        public SolidColorBrush MenuItemColor
        {
            get { return _menuItemColor; }
            set
            {
                _menuItemColor = value;
                OnPropertyChanged("MenuItemColor");
            }
        }
        public SolidColorBrush MenuItemHoverColor
        {
            get { return _menuItemHoverColor; }
            set
            {
                _menuItemHoverColor = value;
                OnPropertyChanged("MenuItemHoverColor");
            }
        }
        public SolidColorBrush ButtonColor
        {
            get { return _buttonColor; }
            set
            {
                _buttonColor = value;
                OnPropertyChanged("ButtonColor");
            }
        }
        public SolidColorBrush ButtonHoverColor
        {
            get { return _buttonHoverColor; }
            set
            {
                _buttonHoverColor = value;
                OnPropertyChanged("ButtonHoverColor");
            }
        }
        public SolidColorBrush TextBoxColor
        {
            get { return _textBoxColor; }
            set
            {
                _textBoxColor = value;
                OnPropertyChanged("TextBoxColor");
            }
        }
        public SolidColorBrush TextBoxHoverColor
        {
            get { return _textBoxHoverColor; }
            set
            {
                _textBoxHoverColor = value;
                OnPropertyChanged("TextBoxHoverColor");
            }
        }
        public SolidColorBrush ForeGroundColor
        {
            get { return _foreGroundColor; }
            set
            {
                _foreGroundColor = value;
                OnPropertyChanged("ForeGroundColor");
            }
        }
        public SolidColorBrush ForeGroundHoverColor
        {
            get { return _foreGroundHoverColor; }
            set
            {
                _foreGroundHoverColor = value;
                OnPropertyChanged("ForeGroundHoverColor");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
