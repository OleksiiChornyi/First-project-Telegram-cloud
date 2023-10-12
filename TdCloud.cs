using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;
using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Telegram_cloud;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Controls;
using FFMpegCore;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Telegram_cloud
{
    /// <summary>
    /// Example class for TDLib usage from C#.
    /// </summary>
    class TdCloud
    {
        public static bool _isResultReceived = false;
        public static List<long> all_chats_id = new List<long>();
        public static Dictionary<long, string> dict_chats = new Dictionary<long, string>();
        public static List<string> gif_format= new List<string> { ".gif", ".GIF" };
        public static List<string> images_formats = new List<string> { ".bmp", ".jpg", ".jpeg", ".png", ".tiff", ".exif", ".BMP", ".JPG", ".JPEG", ".PNG", ".TIFF", ".EXIF"};
        public static List<string> videos_formats = new List<string> { ".mp4", ".mov", ".wmv", ".avi", ".mkv", ".flv", ".MP4", ".MOV", ".WMV", ".AVI", ".MKV", ".FLV" };
        public static long chat_id;
        public static Dictionary<long, List<string>> all_paths = new Dictionary<long, List<string>>();
        public static Dictionary<long, List<long>> all_messages_id = new Dictionary<long, List<long>>();
        public static Dictionary<long, List<long>> all_messages_doc_id = new Dictionary<long, List<long>>();
        public static Dictionary<long, List<long>> all_messages_images_id = new Dictionary<long, List<long>>();
        public static Dictionary<long, List<string>> all_images_paths = new Dictionary<long, List<string>>();
        public static Dictionary<long, List<string>> all_paths_to_directiryes = new Dictionary<long, List<string>>();
        public static Dictionary<long, List<long>> all_messages_directiryes_id = new Dictionary<long, List<long>>();
        public static Dictionary<long, List<string>> all_paths_to_files_with_name = new Dictionary<long, List<string>>();
        public static string path = "";

        public static string result = "No empty";
        public static int count = 0;
        public static long count_messages = 0;
        public static int count_messages_text = 0;
        // create Td.Client
        private static Td.Client _client = CreateTdClient();
        public static long current_chat_id = 0;
        //private static Td.Client _client = null;
        private readonly static Td.ClientResultHandler _defaultHandler = new Handlers.DefaultHandler();
        private readonly static Td.ClientResultHandler _messageHandler = new Handlers.MessagesHandler();
        private readonly static Td.ClientResultHandler _count_messageHandler = new Handlers.CountMessagesHandler();
        private readonly static Td.ClientResultHandler _send_messageHandler = new Handlers.SendMessageHandler();
        private readonly static Td.ClientResultHandler _send_update_messageHandler = new Handlers.SendUpdateMessageHandler();
        private readonly static Td.ClientResultHandler _send_update_move_messageHandler = new Handlers.SendUpdateMoveMessageHandler();
        public static long to_chat_id_message_move;

        private readonly static Td.ClientResultHandler _check_account_premium = new Handlers.CheckAccountPremiumHandler();

        //public static long count_cheke_update;
        public static bool is_updated;
        private readonly static Td.ClientResultHandler _delete_messageHandler = new Handlers.DeleteDocHandler();
        private readonly static Td.ClientResultHandler _update_delete_messageHandler = new Handlers.UpdateDeleteHandler();

        //UpdateDeleteHandler
        //private readonly static Td.ClientResultHandler _countmessages_messageHandler = new Handlers.CountMessagesChatHandler();
        //private readonly static Td.ClientResultHandler _move_directoryHandler = new Handlers.MoveDirectoryHandler();
        //public static string new_file_path = "";

        private readonly static Td.ClientResultHandler _getfileidHandler = new Handlers.GetFileIDHandler();
        public static long doc_id = 0;
        public static string file_name = "";
        public static bool can_download = true;
        public static bool is_file_open = true;
        //Properties Handler
        private readonly static Td.ClientResultHandler _properties_messageHandler = new Handlers.PropertiesHandler();
        public static string properties_message_name;
        public static DateTime properties_message_datetime;
        public static bool properties_message_is_pinned;
        public static double properties_message_size;


        //private readonly static Td.ClientResultHandler _documentHandler = new Handlers.DocHandler();
        private readonly static Td.ClientResultHandler _download_documentHandler = new Handlers.DownloadDocHandler();
        private readonly static Td.ClientResultHandler _chatHandler = new Handlers.NameAllChatsHandler();
        private readonly static Td.ClientResultHandler _new_channelHandler = new Handlers.CreateNewChannel();
        private readonly static Td.ClientResultHandler _allchatsHandler = new Handlers.AllChatsIdsHandler();
        public static Authorization autoForm;
        private static string[] name_keys = new string[]
        {
            "Phone Number", "Email Address", "Phone Code", "Email Code"
        };
        private static Dictionary<string, string> dictionary = new Dictionary<string, string>()
        {
            [name_keys[0]] = "",
            [name_keys[1]] = "",       
            [name_keys[2]] = "",
            [name_keys[3]] = ""
        };
        private static Thread Td_cloud_start;
        private static Thread Td_cloud_auto_form;
        private static TdApi.AuthorizationState _authorizationState = null;
        public static volatile bool _haveAuthorization = false;
        public static volatile bool _go_again = false;
        private static volatile bool _needQuit = false;

        private static volatile AutoResetEvent _gotAuthorization = new AutoResetEvent(false);

        public static readonly string _newLine = Environment.NewLine;
        private static readonly string _commandsLine = "Enter command (1 - MY, 2 - open file with windows, 3 - delete all downloaded files, gc <chatId> - GetChat, me - GetMe, sm <chatId> <message> - SendMessage, lo - LogOut, r - Restart, q - Quit): ";
        private static volatile string _currentPrompt = null;

        private static Td.Client CreateTdClient()
        {
            return Td.Client.Create(new Handlers.UpdateHandler());
        }

        private static void Print(string str)
        {
            if (_currentPrompt != null)
            {
                Console.WriteLine();
            }
            //Console.WriteLine(str);
            if (_currentPrompt != null)
            {
                Console.Write(_currentPrompt);
            }
        }

        private static string ReadLine(string str)
        {
            Console.Write(str);
            _currentPrompt = str;
            var result = Console.ReadLine();
            _currentPrompt = null;
            return result;
        }
        private static void Vesible_elements_auto_form(TdApi.AuthorizationState authorizationState)
        {
            if (authorizationState == null)
                return;
            if (authorizationState is TdApi.AuthorizationStateWaitCode)
            {
                autoForm.Dispatcher.Invoke(() =>
                {
                    if (dictionary[name_keys[0]] != "")
                    {
                        Authorization.Visible_TextBox(autoForm.Phone_Number, dictionary[name_keys[0]]);
                        autoForm.Phone_Number.IsReadOnly = true;
                    }
                });
            }
            else if (authorizationState is TdApi.AuthorizationStateWaitEmailCode)
            {
                autoForm.Dispatcher.Invoke(() =>
                {
                    if (dictionary[name_keys[0]] != "")
                    {
                        Authorization.Visible_TextBox(autoForm.Phone_Number, dictionary[name_keys[0]]);
                        autoForm.Phone_Number.IsReadOnly = true;
                    }
                    if (dictionary[name_keys[1]] != "")
                    {
                        Authorization.Visible_TextBox(autoForm.Email_Address, dictionary[name_keys[1]]);
                        autoForm.Email_Address.IsReadOnly = true;
                    }
                    if (dictionary[name_keys[2]] != "")
                    {
                        Authorization.Visible_TextBox(autoForm.Auth_Phone_Code, dictionary[name_keys[2]]);
                        autoForm.Auth_Phone_Code.IsReadOnly = true;
                    }
                });
            }
            else if (authorizationState is TdApi.AuthorizationStateWaitPassword)
            {
                autoForm.Dispatcher.Invoke(() =>
                {
                    if (dictionary[name_keys[0]] != "")
                    {
                        Authorization.Visible_TextBox(autoForm.Phone_Number, dictionary[name_keys[0]]);
                        autoForm.Phone_Number.IsReadOnly = true;
                    }
                    if (dictionary[name_keys[1]] != "")
                    {
                        Authorization.Visible_TextBox(autoForm.Email_Address, dictionary[name_keys[1]]);
                        autoForm.Email_Address.IsReadOnly = true;
                    }
                    if (dictionary[name_keys[2]] != "")
                    {
                        Authorization.Visible_TextBox(autoForm.Auth_Phone_Code, dictionary[name_keys[2]]);
                        autoForm.Auth_Phone_Code.IsReadOnly = true;
                    }
                    if (dictionary[name_keys[3]] != "")
                    {
                        Authorization.Visible_TextBox(autoForm.Auth_Email_Code, dictionary[name_keys[3]]);
                        autoForm.Auth_Email_Code.IsReadOnly = true;
                    }
                });
            }
        }
        public static void OnAuthorizationStateUpdated(TdApi.AuthorizationState authorizationState)
        {
            Vesible_elements_auto_form(authorizationState);
            if (authorizationState != null)
            {
                _authorizationState = authorizationState;
                //Console.WriteLine(authorizationState.ToString());
            }
            if (_authorizationState is TdApi.AuthorizationStateWaitTdlibParameters)
            {
                TdApi.SetTdlibParameters request = new TdApi.SetTdlibParameters();
                request.DatabaseDirectory = "tdlib";
                request.UseMessageDatabase = true;
                request.UseSecretChats = true;
                request.ApiId = 28738351;
                request.ApiHash = "f1960186b809b0bf1931d70874f07688";
                request.SystemLanguageCode = "en";
                request.DeviceModel = "Desktop";
                request.ApplicationVersion = "1.0";
                request.EnableStorageOptimizer = true;
                _client.Send(request, new Handlers.AuthorizationRequestHandler());
            }
            else if (_authorizationState is TdApi.AuthorizationStateWaitPhoneNumber)
            {
                autoForm.Dispatcher.Invoke(() =>
                {
                    Authorization.Visible_TextBox(autoForm.Phone_Number, dictionary[name_keys[0]]);
                });
                while (!autoForm.isReadyPhoneNumber) 
                    Console.WriteLine("autoForm.isReadyPhoneNumber");

                autoForm.Dispatcher.Invoke(() =>
                {
                    _client.Send(new TdApi.SetAuthenticationPhoneNumber(autoForm.Phone_Number.Text, null), new Handlers.AuthorizationRequestHandler());
                    dictionary[name_keys[0]] = autoForm.Phone_Number.Text;
                    autoForm.isReadyPhoneNumber = false;
                });
            }
            else if (_authorizationState is TdApi.AuthorizationStateWaitEmailAddress)
            {
                autoForm.Dispatcher.Invoke(() =>
                {
                    Authorization.Visible_TextBox(autoForm.Email_Address, dictionary[name_keys[1]]);
                });
                while (!autoForm.isReadyEmail)
                    Console.WriteLine("autoForm.isReadyEmail");
                autoForm.Dispatcher.Invoke(() =>
                {
                    _client.Send(new TdApi.SetAuthenticationEmailAddress(autoForm.Email_Address.Text), new Handlers.AuthorizationRequestHandler());
                    dictionary[name_keys[1]] = autoForm.Email_Address.Text;
                    autoForm.isReadyEmail = false;
                });
            }
            else if (_authorizationState is TdApi.AuthorizationStateWaitEmailCode)
            {
                autoForm.Dispatcher.Invoke(() =>
                {
                    Authorization.Visible_TextBox(autoForm.Auth_Email_Code, dictionary[name_keys[3]]);
                });
                while (!autoForm.isReadyEmailCode)
                    Console.WriteLine("autoForm.isReadyEmailCode");
                autoForm.Dispatcher.Invoke(() =>
                {
                    _client.Send(new TdApi.CheckAuthenticationEmailCode(new TdApi.EmailAddressAuthenticationCode(autoForm.Auth_Email_Code.Text)), new Handlers.AuthorizationRequestHandler());
                    dictionary[name_keys[3]] = autoForm.Auth_Email_Code.Text;
                    autoForm.isReadyEmailCode = false;
                });
            }
            else if (_authorizationState is TdApi.AuthorizationStateWaitOtherDeviceConfirmation state)
            {
                MessageBox.Show("Please confirm this login link on another device: " + state.Link);
            }
            else if (_authorizationState is TdApi.AuthorizationStateWaitCode)
            {
                autoForm.Dispatcher.Invoke(() =>
                {
                    Authorization.Visible_TextBox(autoForm.Auth_Phone_Code, dictionary[name_keys[2]]);
                });
                while (!autoForm.isReadyPhoneCode)
                    Console.WriteLine("autoForm.isReadyPhoneCode");
                autoForm.Dispatcher.Invoke(() =>
                {
                    _client.Send(new TdApi.CheckAuthenticationCode(autoForm.Auth_Phone_Code.Text), new Handlers.AuthorizationRequestHandler());
                    dictionary[name_keys[2]] = autoForm.Auth_Phone_Code.Text;
                    autoForm.isReadyPhoneCode = false;
                });
            }
            else if (_authorizationState is TdApi.AuthorizationStateWaitRegistration)
            {
                MessageBox.Show("This user don`t register, please create an account on telegram.org and continue on this app");
            }
            else if (_authorizationState is TdApi.AuthorizationStateWaitPassword)
            {
                autoForm.Dispatcher.Invoke(() =>
                {
                    Authorization.Visible_TextBox(autoForm.Password);
                });
                while (!autoForm.isReadyPassword)
                    Console.WriteLine("autoForm.isReadyPassword");
                autoForm.Dispatcher.Invoke(() =>
                {
                    _client.Send(new TdApi.CheckAuthenticationPassword(autoForm.Password.Text), new Handlers.AuthorizationRequestHandler());
                    autoForm.isReadyPassword = false;
                });
            }
            else if (_authorizationState is TdApi.AuthorizationStateReady)
            {
                _haveAuthorization = true;
                _needQuit = true;
                _gotAuthorization.Set();
                _isResultReceived = false;
                var func_check_account_premium = new TdApi.GetMe();
                _client.Send(func_check_account_premium, _check_account_premium);
                Close_auto_Form();
            }
            else if (_authorizationState is TdApi.AuthorizationStateLoggingOut)
            {
                _haveAuthorization = false;
                Print("Logging out");
            }
            else if (_authorizationState is TdApi.AuthorizationStateClosing)
            {
                _haveAuthorization = false;
                Print("Closing");
            }
            else if (_authorizationState is TdApi.AuthorizationStateClosed)
            {
                Print("Closed");
                if (!_needQuit)
                {
                    _client = CreateTdClient(); // recreate _client after previous has closed
                }
            }
            else
            {
                Print("Unsupported authorization state:" + _newLine + _authorizationState);
            }
        }
        public static void Close_auto_Form()
        {
            if (autoForm == null)
                return;
            autoForm.Dispatcher.Invoke(() =>
            {
                autoForm.Close();
            });

        }
        public static void Exit_autorization()
        {
            if (_client != null)
            {
                _client.Send(new TdApi.Close(), _defaultHandler);
            }
            if (Td_cloud_start != null)
            {
                Td_cloud_start.Abort();
                Td_cloud_start.Join();
            }
            _gotAuthorization.Close();
            _haveAuthorization = false;
            _needQuit = true;
            Close_auto_Form();
        }
        private static long GetChatId(string arg)
        {
            long chatId = 0;
            try
            {
                chatId = Convert.ToInt64(arg);
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            return chatId;
        }
        public static void Get_About_account()
        {
            _isResultReceived = false;
            var func = new TdApi.GetMe();
            _client.Send(func, new Handlers.MessageBoxHandler());
            while (!_isResultReceived)
            {
                Td.Client.Execute(func);
            }
        }
        public static void Create_Channel(string name_channel, string description)
        {
            _isResultReceived = false;
            var func_create_channel = new TdApi.CreateNewSupergroupChat(name_channel, false, true, description, null, 0, false);
            _client.Send(func_create_channel, _new_channelHandler);
            while (!_isResultReceived)
            {
                Td.Client.Execute(func_create_channel);
            }
            Storage_drives.Add_drives(name_channel, chat_id);
        }
        public static void Find_id_channels()
        {
            while (!_haveAuthorization) ;
            all_chats_id.Clear();
            _isResultReceived = false;
            var func_all_chats = new TdApi.GetChats(new TdApi.ChatListMain(), int.MaxValue);
            _client.Send(func_all_chats, _allchatsHandler);
            while (!_isResultReceived)
            {
                Td.Client.Execute(func_all_chats);
            }
        }
        public static void Find_name_channels()
        {
            dict_chats.Clear();
            foreach (var id in all_chats_id)
            {
                _isResultReceived = false;
                chat_id = 0;
                var func_chats = new TdApi.GetChat(id);
                _client.Send(func_chats, _chatHandler);
                while (!_isResultReceived)
                {
                    Td.Client.Execute(func_chats);
                }
            }
        }
        public static void Find_Channel()
        {
            Find_id_channels();
            while (all_chats_id.Count == 0)
                Find_id_channels();
            Find_name_channels();
            while (dict_chats.Count == 0)
                Find_name_channels();
        }
        public static long GetMessagesCount(long id)
        {
            count_messages = 0;

            var func = new TdApi.GetChatMessageCount(id, new TdApi.SearchMessagesFilterDocument(), false);
            _isResultReceived = false;
            _client.Send(func, _count_messageHandler);
            while (!_isResultReceived)
            {
                Td.Client.Execute(func);
            }

            func = new TdApi.GetChatMessageCount(id, new TdApi.SearchMessagesFilterPhoto(), false);
            _isResultReceived = false;
            _client.Send(func, _count_messageHandler);
            while (!_isResultReceived)
            {
                Td.Client.Execute(func);
            }

            func = new TdApi.GetChatMessageCount(id, new TdApi.SearchMessagesFilterVideo(), false);
            _isResultReceived = false;
            _client.Send(func, _count_messageHandler);
            while (!_isResultReceived)
            {
                Td.Client.Execute(func);
            }

            func = new TdApi.GetChatMessageCount(id, new TdApi.SearchMessagesFilterAnimation(), false);
            _isResultReceived = false;
            _client.Send(func, _count_messageHandler);
            while (!_isResultReceived)
            {
                Td.Client.Execute(func);
            }
            return count_messages;

            /*all_messages_id = new List<long> { 0 };
            count_messages = 0;
            result = "No empty";
            while (result != "")
            {
                _isResultReceived = false;
                var func_message = new TdApi.GetChatHistory(id, all_messages_id[all_messages_id.Count - 1], 0, 1, false);
                _client.Send(func_message, _count_messageHandler);
                while (!_isResultReceived)
                {
                    Td.Client.Execute(func_message);
                }
            }
            return count_messages;*/
        }
        public static void Search_all_files(ProgressBar Progress_bar, ProgressBar Progress_bar_add, long id)
        {
            all_paths[id] = new List<string>();
            all_messages_doc_id[id] = new List<long>();
            all_messages_images_id[id] = new List<long>();
            all_images_paths[id] = new List<string>();
            all_paths_to_directiryes[id] = new List<string>();
            all_messages_directiryes_id[id] = new List<long>();
            all_paths_to_files_with_name[id] = new List<string>();
            current_chat_id = id;
            if (Progress_bar != null)
            {
                Progress_bar.Dispatcher.Invoke(() =>
                {
                    Progress_bar.ToolTip = MainWindow.PB_In_progress;
                    Progress_bar_add.IsIndeterminate = true;
                });
            }

            count_messages = GetMessagesCount(id);
            if (count_messages < 1)
            {
                if (Progress_bar != null)
                {
                    Progress_bar.Dispatcher.Invoke(() =>
                    {
                        Progress_bar.ToolTip = MainWindow.PB_Ready;
                        Progress_bar_add.IsIndeterminate = false;
                    });
                }
                return;
            }
            if (Progress_bar != null)
            {
                Progress_bar.Dispatcher.Invoke(() =>
                {
                    Progress_bar.Maximum = count_messages; ///////////////////////////////////////////////////////////////////////////////////////// ПРОВЕРИТЬ РАБОТАЕТ ЛИ ОТДЕЛЬНО
                    Progress_bar.Value = 0;
                });
            }

            result = "No empty";
            all_messages_id[id] = new List<long> { 0 };

            while (result != "")
            {
                _isResultReceived = false;
                var func_message = new TdApi.GetChatHistory(id, all_messages_id[id][all_messages_id[id].Count - 1], 0, 1, false);
                _client.Send(func_message, _messageHandler);
                while (!_isResultReceived)
                {
                    Td.Client.Execute(func_message);
                }
                if (Progress_bar != null)
                {
                    Progress_bar.Dispatcher.Invoke(() =>
                    {
                        if ((string)Progress_bar.ToolTip == MainWindow.PB_In_progress && Progress_bar.Value != Progress_bar.Maximum - 1)
                            Progress_bar.Value++;
                    });
                }
            }
            if (Progress_bar != null)
            {
                Progress_bar.Dispatcher.Invoke(() =>
                {
                    if ((string)Progress_bar.ToolTip == MainWindow.PB_In_progress)
                        Progress_bar.Value = Progress_bar.Maximum;
                });
            }
            all_messages_id[id].Remove(0);
        }
        public static void Send_update_message(long id)
        {
            current_chat_id = id;
            string text_message = "";
            //long count_cheke_update_this = all_messages_id.Count;
            is_updated = false;

            _isResultReceived = false;
            var func_update_message = new TdApi.SearchChatMessages(id, text_message, null, 0, 0, 1, null, 0);
            _client.Send(func_update_message, _send_update_messageHandler);
            Console.WriteLine("2");
            while (!_isResultReceived)
            {
                Td.Client.Execute(func_update_message);
            }
            Console.WriteLine("2 OK");
        }
        public static void Send_message(long id, string file_path, string text_message)
        {
            var file = new TdApi.InputFileLocal(file_path);
            var text = new TdApi.FormattedText(text_message, null);
            //var func_send_file = new TdApi.InputMessagePhoto(file, null, null, 0, 0, text, 0, false);
            var func_send_file = new TdApi.InputMessageDocument(file, null, true, text); // true - выкл автоопределение (всё как файл), false - автоматическое определение, файл это или фото/видео
            var func = new TdApi.SendMessage(id, 0, 0, null, null, func_send_file);
            _isResultReceived = false;
            _client.Send(func, _send_messageHandler);
            Console.WriteLine("1");
            while (!_isResultReceived)
            {
                Td.Client.Execute(func);
            }
            Console.WriteLine("1 OK");
        }
        public static string ChangeVideoToGif(string name_img)
        {
            foreach (string i in gif_format)
            {
                if (name_img.Contains(i))
                    return name_img.Substring(0, name_img.LastIndexOf(i) + i.Length);
            }
            foreach (string i in videos_formats)
            {
                if (name_img.EndsWith(i))
                    return name_img.Replace(i, gif_format[0]);
            }
            return name_img;
        }
        public static void Load_video(string file_path, string name_drive, long id_drive)
        {
            if ((file_path.EndsWith(gif_format[0]) || file_path.EndsWith(gif_format[1])) && all_images_paths[id_drive].Count > 0)
            {
                try
                {
                    var index = all_images_paths[id_drive].Count - 1;
                    string[] substrings = all_images_paths[id_drive][index].Split('\\');
                    List<string> paths_to_images = new List<string>();
                    string result = "";
                    for (int i = 0; i < substrings.Length; i++)
                    {
                        result += substrings[i];
                        paths_to_images.Add(result);
                        if (i < substrings.Length - 1)
                            result += "\\";
                    }
                    paths_to_images.RemoveAt(paths_to_images.Count - 1);
                    foreach (var item in paths_to_images)
                    {
                        if (!Directory.Exists(name_drive + "\\" + item))
                        {
                            Directory.CreateDirectory(name_drive + "\\" + item);
                        }
                    }
                    File.Copy(file_path, name_drive + "\\" + ChangeVideoToGif(all_images_paths[id_drive][index]));
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(System.Runtime.InteropServices.ExternalException))
                        return;
                    if (ex.GetType() == typeof(System.IO.IOException))
                        return;
                    //Console.WriteLine(ex.GetType() + "\n\n");
                    MessageBox.Show("Error in loading preview video 1\n" + ex.ToString());
                }
            }
            else if (videos_formats.Any(file_path.EndsWith) && all_images_paths[id_drive].Count > 0)
            {
                try
                {
                    var index = all_images_paths[id_drive].Count - 1;
                    string[] substrings = all_images_paths[id_drive][index].Split('\\');
                    List<string> paths_to_images = new List<string>();
                    string result = "";
                    for (int i = 0; i < substrings.Length; i++)
                    {
                        result += substrings[i];
                        paths_to_images.Add(result);
                        if (i < substrings.Length - 1)
                            result += "\\";
                    }
                    paths_to_images.RemoveAt(paths_to_images.Count - 1);
                    foreach (var item in paths_to_images)
                    {
                        if (!Directory.Exists(name_drive + "\\" + item))
                        {
                            Directory.CreateDirectory(name_drive + "\\" + item);
                        }
                    }
                    double duration = Get_duration_video(file_path);
                    var max = 5.0;
                    if (duration > max)
                        FFMpeg.GifSnapshot(file_path, name_drive + "\\" + ChangeVideoToGif(all_images_paths[id_drive][index]), new System.Drawing.Size(200, 200), duration: TimeSpan.FromSeconds(max));
                    else
                        FFMpeg.GifSnapshot(file_path, name_drive + "\\" + ChangeVideoToGif(all_images_paths[id_drive][index]), new System.Drawing.Size(200, 200));
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(FFMpegCore.Exceptions.FFMpegException))
                        return;
                    if (ex.GetType() == typeof(System.IO.IOException))
                        return;
                    if (ex.GetType() == typeof(System.Runtime.InteropServices.ExternalException))
                        return;
                    MessageBox.Show("Error in loading preview video\n" + ex.ToString());
                }
            }
        }
        public static void Load_image(string file_path, string name_drive, long id_drive)
        {
            if (images_formats.Any(file_path.EndsWith) && all_images_paths[id_drive].Count > 0)
            {
                var index = all_images_paths[id_drive].Count - 1;
                string[] substrings = all_images_paths[id_drive][index].Split('\\');
                List<string> paths_to_images = new List<string>();
                string result = "";
                for (int i = 0; i < substrings.Length; i++)
                {
                    result += substrings[i];
                    paths_to_images.Add(result);
                    if (i < substrings.Length - 1)
                        result += "\\";
                }
                paths_to_images.RemoveAt(paths_to_images.Count - 1);
                try
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(file_path);
                    int newWidth = 200;
                    int newHeight = 200;
                    Bitmap newImage = new Bitmap(newWidth, newHeight);
                    using (Graphics g = Graphics.FromImage(newImage))
                    {
                        g.DrawImage(image, 0, 0, newWidth, newHeight);
                    }
                    if (!Directory.Exists(name_drive))
                    {
                        Directory.CreateDirectory(name_drive);
                    }

                    foreach (var item in paths_to_images)
                    {
                        if (!Directory.Exists(name_drive + "\\" + item))
                        {
                            Directory.CreateDirectory(name_drive + "\\" + item);
                        }
                    }
                    if (all_images_paths[id_drive][index].EndsWith(images_formats[0]) || all_images_paths[id_drive][index].EndsWith(images_formats[6]))
                        newImage.Save(name_drive + "\\" + all_images_paths[id_drive][index], ImageFormat.Bmp);
                    else if (all_images_paths[id_drive][index].EndsWith(images_formats[1]) || all_images_paths[id_drive][index].EndsWith(images_formats[2]) || all_images_paths[id_drive][index].EndsWith(images_formats[7]) || all_images_paths[id_drive][index].EndsWith(images_formats[8]))
                        newImage.Save(name_drive + "\\" + all_images_paths[id_drive][index], ImageFormat.Jpeg);
                    else if (all_images_paths[id_drive][index].EndsWith(images_formats[3]) || all_images_paths[id_drive][index].EndsWith(images_formats[9]))
                        newImage.Save(name_drive + "\\" + all_images_paths[id_drive][index], ImageFormat.Png);
                    else if (all_images_paths[id_drive][index].EndsWith(images_formats[4]) || all_images_paths[id_drive][index].EndsWith(images_formats[10]))
                        newImage.Save(name_drive + "\\" + all_images_paths[id_drive][index], ImageFormat.Tiff);
                    else if (all_images_paths[id_drive][index].EndsWith(images_formats[5]) || all_images_paths[id_drive][index].EndsWith(images_formats[11]))
                        newImage.Save(name_drive + "\\" + all_images_paths[id_drive][index], ImageFormat.Exif);
                    image.Dispose();
                    //newImage.Dispose();
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(System.Runtime.InteropServices.ExternalException))
                        return;
                    //Console.WriteLine(ex.GetType() + "\n\n");
                    MessageBox.Show("Error in loading preview images\n" + ex.ToString());
                }
                try
                {
                    if (!file_path.Contains("\\tdlib\\"))
                        return;
                    File.Delete(file_path);
                }
                catch { }
            }
        }
        public static void Add_directory(long id, string name, string path)
        {
            var text = new TdApi.FormattedText(Path.Combine(path, name), null);
            var msg = new TdApi.InputMessageText(text, false, true);
            var func = new TdApi.SendMessage(id, 0, 0, null, null, msg);


            _isResultReceived = false;

            _client.Send(func, _send_messageHandler);
            while (!_isResultReceived)
            {
                Td.Client.Execute(func);
            }
            TdCloud.is_updated = false;
            while (!TdCloud.is_updated)
                Send_update_message(id); //, path + name);
            /*_isResultReceived = false;
            _client.Send(func, _send_messageHandler);
            while (!_isResultReceived)
            {
                Td.Client.Execute(func);
            }*/
        }
        private static double Get_duration_video(string file_path)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-i \"{file_path}\" -show_entries format=duration -v quiet -of csv=\"p=0\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();

                var output = process.StandardOutput.ReadToEnd();
                var duration = Double.Parse(output, CultureInfo.InvariantCulture);

                return duration;
            }
        }
        public static void Load_preview_video(string file_path, int index)
        {
            if ((file_path.EndsWith(gif_format[0]) || file_path.EndsWith(gif_format[1])) && all_images_paths[Struct_cloud.IdDrive].Count > 0)
            {
                try
                {
                    string[] substrings = all_images_paths[Struct_cloud.IdDrive][index].Split('\\');
                    List<string> paths_to_images = new List<string>();
                    string result = "";
                    for (int i = 0; i < substrings.Length; i++)
                    {
                        result += substrings[i];
                        paths_to_images.Add(result);
                        if (i < substrings.Length - 1)
                            result += "\\";
                    }
                    paths_to_images.RemoveAt(paths_to_images.Count - 1);
                    foreach (var item in paths_to_images)
                    {
                        if (!Directory.Exists(Struct_cloud.NameDrive + "\\" + item))
                        {
                            Directory.CreateDirectory(Struct_cloud.NameDrive + "\\" + item);
                        }
                    }
                    if (File.Exists(Path.Combine(Struct_cloud.NameDrive, ChangeVideoToGif(all_images_paths[Struct_cloud.IdDrive][index]))))
                        File.Delete(Path.Combine(Struct_cloud.NameDrive, ChangeVideoToGif(all_images_paths[Struct_cloud.IdDrive][index])));
                    File.Copy(file_path, Struct_cloud.NameDrive + "\\" + ChangeVideoToGif(all_images_paths[Struct_cloud.IdDrive][index]));
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(System.Runtime.InteropServices.ExternalException))
                        return;
                    if (ex.GetType() == typeof(System.IO.IOException))
                        return;
                    //Console.WriteLine(ex.GetType() + "\n\n");
                    MessageBox.Show("Error in loading preview video\n" + ex.ToString());
                }
            }
            else if (videos_formats.Any(file_path.EndsWith) && all_images_paths[Struct_cloud.IdDrive].Count > 0)
            {
                try
                {
                    string[] substrings = all_images_paths[Struct_cloud.IdDrive][index].Split('\\');
                    List<string> paths_to_images = new List<string>();
                    string result = "";
                    for (int i = 0; i < substrings.Length; i++)
                    {
                        result += substrings[i];
                        paths_to_images.Add(result);
                        if (i < substrings.Length - 1)
                            result += "\\";
                    }
                    paths_to_images.RemoveAt(paths_to_images.Count - 1);
                    foreach (var item in paths_to_images)
                    {
                        if (!Directory.Exists(Struct_cloud.NameDrive + "\\" + item))
                        {
                            Directory.CreateDirectory(Struct_cloud.NameDrive + "\\" + item);
                        }
                    }
                    if (File.Exists(Path.Combine(Struct_cloud.NameDrive, all_images_paths[Struct_cloud.IdDrive][index])))
                        File.Delete(Path.Combine(Struct_cloud.NameDrive, all_images_paths[Struct_cloud.IdDrive][index]));
                    double duration = Get_duration_video(file_path);
                    var max = 5.0;
                    if (duration > max)
                        FFMpeg.GifSnapshot(file_path, Struct_cloud.NameDrive + "\\" + ChangeVideoToGif(all_images_paths[Struct_cloud.IdDrive][index]), new System.Drawing.Size(200, 200), duration: TimeSpan.FromSeconds(max));
                    else
                        FFMpeg.GifSnapshot(file_path, Struct_cloud.NameDrive + "\\" + ChangeVideoToGif(all_images_paths[Struct_cloud.IdDrive][index]), new System.Drawing.Size(200, 200));
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(FFMpegCore.Exceptions.FFMpegException))
                        return;
                    if (ex.GetType() == typeof(System.Runtime.InteropServices.ExternalException))
                        return;
                    if (ex.GetType() == typeof(System.IO.IOException))
                        return;
                    //Console.WriteLine(ex.GetType() + "\n\n");
                    MessageBox.Show("Error in loading preview video\n" + ex.ToString());
                }
                try
                {
                    if (!file_path.Contains("\\tdlib\\"))
                        return;
                    File.Delete(file_path);
                }
                catch { }
            }
        }
        public static void Load_preview_image(string file_path, int index)
        {
            if (images_formats.Any(file_path.EndsWith) && all_images_paths[Struct_cloud.IdDrive].Count > 0)
            {
                string[] substrings = all_images_paths[Struct_cloud.IdDrive][index].Split('\\');
                List<string> paths_to_images = new List<string>();
                string result = "";
                for (int i = 0; i < substrings.Length; i++)
                {
                    result += substrings[i];
                    paths_to_images.Add(result);
                    if (i < substrings.Length - 1)
                        result += "\\";
                }
                paths_to_images.RemoveAt(paths_to_images.Count - 1);
                try
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(file_path);
                    int newWidth = 200;
                    int newHeight = 200;
                    Bitmap newImage = new Bitmap(newWidth, newHeight);
                    using (Graphics g = Graphics.FromImage(newImage))
                    {
                        g.DrawImage(image, 0, 0, newWidth, newHeight);
                    }
                    if (!Directory.Exists(Struct_cloud.NameDrive))
                    {
                        Directory.CreateDirectory(Struct_cloud.NameDrive);
                    }

                    foreach (var item in paths_to_images)
                    {
                        if (!Directory.Exists(Struct_cloud.NameDrive + "\\" + item))
                        {
                            Directory.CreateDirectory(Struct_cloud.NameDrive + "\\" + item);
                        }
                    }
                    if (File.Exists(Path.Combine(Struct_cloud.NameDrive, all_images_paths[Struct_cloud.IdDrive][index])))
                        File.Delete(Path.Combine(Struct_cloud.NameDrive, all_images_paths[Struct_cloud.IdDrive][index]));
                    if (all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[0]) || all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[6]))
                        newImage.Save(Struct_cloud.NameDrive + "\\" + all_images_paths[Struct_cloud.IdDrive][index], ImageFormat.Bmp);
                    /*else if (all_images_paths[index].EndsWith(images_formats[1]) || all_images_paths[index].EndsWith(images_formats[8]))
                        newImage.Save(Struct_cloud.NameDrive + "\\" + all_images_paths[index], ImageFormat.Gif);*/
                    else if (all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[1]) || all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[2]) || all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[7]) || all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[8]))
                        newImage.Save(Struct_cloud.NameDrive + "\\" + all_images_paths[Struct_cloud.IdDrive][index], ImageFormat.Jpeg);
                    else if (all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[3]) || all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[9]))
                        newImage.Save(Struct_cloud.NameDrive + "\\" + all_images_paths[Struct_cloud.IdDrive][index], ImageFormat.Png);
                    else if (all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[4]) || all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[10]))
                        newImage.Save(Struct_cloud.NameDrive + "\\" + all_images_paths[Struct_cloud.IdDrive][index], ImageFormat.Tiff);
                    else if (all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[5]) || all_images_paths[Struct_cloud.IdDrive][index].EndsWith(images_formats[11]))
                        newImage.Save(Struct_cloud.NameDrive + "\\" + all_images_paths[Struct_cloud.IdDrive][index], ImageFormat.Exif);
                    image.Dispose();
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(System.Runtime.InteropServices.ExternalException))
                        return;
                    if (ex.GetType() == typeof(System.IO.IOException))
                        return;
                    MessageBox.Show("Error in loading preview images\n" + ex.ToString());
                }
                try
                {
                    if (!file_path.Contains("\\tdlib\\"))
                        return;
                    File.Delete(file_path);
                }
                catch { }
            }
        }
        public static void Load_preview_images(long chatId, int index)
        {
            path = "";

            while (path == "")
            {
                _isResultReceived = false;
                var func = new TdApi.GetMessage(chatId, all_messages_images_id[Struct_cloud.IdDrive][index]);
                _client.Send(func, _getfileidHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func);

                var func1 = new TdApi.DownloadFile((Int32)doc_id, 32, 0, 0, true);
                _isResultReceived = false;
                _client.Send(func1, _download_documentHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func1);
            }
            Load_preview_image(path, index);
        }
        public static void Load_preview_videos(long chatId, int index)
        {
            path = "";

            while (path == "")
            {
                _isResultReceived = false;
                var func = new TdApi.GetMessage(chatId, all_messages_images_id[Struct_cloud.IdDrive][index]);
                _client.Send(func, _getfileidHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func);

                var func1 = new TdApi.DownloadFile((Int32)doc_id, 32, 0, 0, true);
                _isResultReceived = false;
                _client.Send(func1, _download_documentHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func1);
            }
            Load_preview_video(path, index);
        }
        public static void Load_preview_one_video(string old_file_path, string path_id_drive)
        {
            if ((old_file_path.EndsWith(gif_format[0]) || old_file_path.EndsWith(gif_format[1])) && all_images_paths[Struct_cloud.IdDrive].Count > 0)
            {
                try
                {
                    string[] substrings = path_id_drive.Split('\\');
                    List<string> paths_to_images = new List<string>();
                    string result = "";
                    for (int i = 0; i < substrings.Length; i++)
                    {
                        result += substrings[i];
                        paths_to_images.Add(result);
                        if (i < substrings.Length - 1)
                            result += "\\";
                    }
                    paths_to_images.RemoveAt(paths_to_images.Count - 1);
                    foreach (var item in paths_to_images)
                    {
                        if (!Directory.Exists(item))
                        {
                            Directory.CreateDirectory(item);
                        }
                    }
                    File.Copy(old_file_path, ChangeVideoToGif(path_id_drive));
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(System.Runtime.InteropServices.ExternalException))
                        return;
                    if (ex.GetType() == typeof(System.IO.IOException))
                        return;
                    //Console.WriteLine(ex.GetType() + "\n\n");
                    MessageBox.Show("Error in loading preview video\n" + ex.ToString());
                }
            }
            else if (videos_formats.Any(old_file_path.EndsWith) && all_images_paths[Struct_cloud.IdDrive].Count > 0)
            {
                try
                {
                    string[] substrings = path_id_drive.Split('\\');
                    List<string> paths_to_images = new List<string>();
                    string result = "";
                    for (int i = 0; i < substrings.Length; i++)
                    {
                        result += substrings[i];
                        paths_to_images.Add(result);
                        if (i < substrings.Length - 1)
                            result += "\\";
                    }
                    paths_to_images.RemoveAt(paths_to_images.Count - 1);
                    foreach (var item in paths_to_images)
                    {
                        if (!Directory.Exists(item))
                        {
                            Directory.CreateDirectory(item);
                        }
                    }
                    double duration = Get_duration_video(old_file_path);
                    var max = 5.0;
                    if (duration > max)
                        FFMpeg.GifSnapshot(old_file_path, ChangeVideoToGif(path_id_drive), new System.Drawing.Size(200, 200), duration: TimeSpan.FromSeconds(max));
                    else
                        FFMpeg.GifSnapshot(old_file_path, ChangeVideoToGif(path_id_drive), new System.Drawing.Size(200, 200));
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(FFMpegCore.Exceptions.FFMpegException))
                        return;
                    if (ex.GetType() == typeof(System.Runtime.InteropServices.ExternalException))
                        return;
                    if (ex.GetType() == typeof(System.IO.IOException))
                        return;
                    //Console.WriteLine(ex.GetType() + "\n\n");
                    MessageBox.Show("Error in loading preview video\n" + ex.ToString());
                }
                try
                {
                    if (!old_file_path.Contains("\\tdlib\\"))
                        return;
                    File.Delete(old_file_path);
                }
                catch { }
            }
        }
        public static void Load_preview_one_image(string old_file_path, string path_id_drive)
        {
            if (images_formats.Any(path_id_drive.EndsWith) && all_images_paths[Struct_cloud.IdDrive].Count > 0)
            {
                string[] substrings = path_id_drive.Split('\\');
                List<string> paths_to_images = new List<string>();
                string result = "";
                for (int i = 0; i < substrings.Length; i++)
                {
                    result += substrings[i];
                    paths_to_images.Add(result);
                    if (i < substrings.Length - 1)
                        result += "\\";
                }
                paths_to_images.RemoveAt(paths_to_images.Count - 1);
                try
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(old_file_path);
                    int newWidth = 200;
                    int newHeight = 200;
                    Bitmap newImage = new Bitmap(newWidth, newHeight);
                    using (Graphics g = Graphics.FromImage(newImage))
                    {
                        g.DrawImage(image, 0, 0, newWidth, newHeight);
                    }
                    foreach (var item in paths_to_images)
                    {
                        if (!Directory.Exists(item))
                        {
                            Directory.CreateDirectory(item);
                        }
                    }
                    if (path_id_drive.EndsWith(images_formats[0]) || path_id_drive.EndsWith(images_formats[6]))
                        newImage.Save(path_id_drive, ImageFormat.Bmp);
                    else if (path_id_drive.EndsWith(images_formats[1]) || path_id_drive.EndsWith(images_formats[2]) || path_id_drive.EndsWith(images_formats[7]) || path_id_drive.EndsWith(images_formats[8]))
                        newImage.Save(path_id_drive, ImageFormat.Jpeg);
                    else if (path_id_drive.EndsWith(images_formats[3]) || path_id_drive.EndsWith(images_formats[9]))
                        newImage.Save(path_id_drive, ImageFormat.Png);
                    else if (path_id_drive.EndsWith(images_formats[4]) || path_id_drive.EndsWith(images_formats[10]))
                        newImage.Save(path_id_drive, ImageFormat.Tiff);
                    else if (path_id_drive.EndsWith(images_formats[5]) || path_id_drive.EndsWith(images_formats[11]))
                        newImage.Save(path_id_drive, ImageFormat.Exif);
                    image.Dispose();
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(System.Runtime.InteropServices.ExternalException))
                        return;
                    if (ex.GetType() == typeof(System.IO.IOException))
                        return;
                    MessageBox.Show("Error in loading preview images\n" + ex.ToString());
                }
                try
                {
                    if (!old_file_path.Contains("\\tdlib\\"))
                        return;
                    File.Delete(old_file_path);
                }
                catch { }
            }
        }
        public static void Load_one_preview(long chatId, long messageId, string new_file_path, string drive_path)
        {
            path = "";
            while (path == "")
            {
                _isResultReceived = false;
                var func = new TdApi.GetMessage(chatId, messageId);
                _client.Send(func, _getfileidHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func);

                var func1 = new TdApi.DownloadFile((Int32)doc_id, 32, 0, 0, true);
                _isResultReceived = false;
                _client.Send(func1, _download_documentHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func1);
            }
            Load_preview_one_image(path, drive_path);
            Load_preview_one_video(path, drive_path);
        }
        public static void Open_file(long chatId, long messageId)
        {
            var tmp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tdlib", "tmp");
            var file_path = Path.Combine(tmp_path, Struct_cloud.Get_name_children_by_id(messageId));
            path = "";
            doc_id = 0;
            is_file_open = false;
            _isResultReceived = false;
            while (!File.Exists(file_path))
                Download_message_file(chatId, messageId, tmp_path);
            try
            {
                Process.Start(file_path);
                is_file_open = true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.GetType());
                if (ex.GetType() == typeof(System.InvalidOperationException))
                {
                    return;
                }
                MessageBox.Show("I can`t open this file with standart windows options\n" + ex.ToString());
            }
        }
        public static void Pin_Message(long chatId, long messageId)
        {
            _isResultReceived = false;
            var func = new TdApi.PinChatMessage(chatId, messageId, true, false);
            _client.Send(func, _defaultHandler);
            while (!_isResultReceived)
                Td.Client.Execute(func);
        }
        public static void Unpin_Message(long chatId, long messageId)
        {
            _isResultReceived = false;
            var func = new TdApi.UnpinChatMessage(chatId, messageId);
            _client.Send(func, _defaultHandler);
            while (!_isResultReceived)
                Td.Client.Execute(func);
        }
        public static void Download_file(int docId, string new_path, string name)
        {
            path = "";
            can_download = true;
            var func1 = new TdApi.DownloadFile(docId, 32, 0, 0, true);
            while (path == "")
            {
                _isResultReceived = false;
                _client.Send(func1, _download_documentHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func1);
                if (!can_download)
                {
                    MessageBox.Show("Update your drive! This file/directory doesn`t exist");
                    return;
                }
            }

            /*if (path == "")
            {
                MessageBox.Show("Update your drive! This file/directory doesn`t exist");
                return;
            }*/
            string newFilePath;
            if (name == "")
            {
                newFilePath = Path.Combine(new_path, Path.GetFileName(path));
            }
            else
            {
                newFilePath = Path.Combine(new_path, Path.GetFileName(name));
            }

            if (newFilePath == path)
                return;

            if (!Directory.Exists(Path.GetDirectoryName(newFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            }
            /*if (File.Exists(newFilePath))
            {
                File.Delete(newFilePath);
            }*/

            if (!File.Exists(newFilePath))
            {
                if (path.Contains("\\tdlib\\"))
                {
                    File.Move(path, newFilePath);
                    File.Delete(path);
                }
                else
                {
                    File.Copy(path, newFilePath);
                }
            }
            else
            {
                if (name == "")
                    return;
                MessageBox.Show("Error in download file\n" + newFilePath + " is allready exist");
            }
        }
        public static void Download_message_file(long chatId, long messageId, string new_path)
        {
            doc_id = 0;
            file_name = "";
            var func = new TdApi.GetMessage(chatId, messageId);
            while (doc_id == 0 || file_name == "")
            {
                _isResultReceived = false;
                _client.Send(func, _getfileidHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func);
            }
            Download_file((Int32)doc_id, new_path, file_name);
        }
        public static void Send_update_move_message(long id)
        {
            to_chat_id_message_move = id;

            string text_message = "";
            //long count_cheke_update_this = all_messages_id.Count;
            is_updated = false;

            _isResultReceived = false;
            var func_update_message = new TdApi.SearchChatMessages(id, text_message, null, 0, 0, 1, null, 0);
            _client.Send(func_update_message, _send_update_move_messageHandler);
            Console.WriteLine("2");
            while (!_isResultReceived)
            {
                Td.Client.Execute(func_update_message);
            }
            Console.WriteLine("2 OK");
        }
        public static void Move_message(long from_chatId, long to_chat_id, long messageId, string new_text)
        {
            try
            {
                if (!all_paths.ContainsKey(to_chat_id))
                {
                    all_paths[to_chat_id] = new List<string>();
                    all_messages_doc_id[to_chat_id] = new List<long>();
                    all_messages_images_id[to_chat_id] = new List<long>();
                    all_images_paths[to_chat_id] = new List<string>();
                    all_paths_to_directiryes[to_chat_id] = new List<string>();
                    all_messages_directiryes_id[to_chat_id] = new List<long>();
                    all_paths_to_files_with_name[to_chat_id] = new List<string>();
                }
                var tmp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tdlib", "tmp", new_text);
                var file_path = Path.Combine(tmp_path, Struct_cloud.Get_name_children_by_id(messageId));
                var text_message = new_text;
                if (text_message.StartsWith(Struct_cloud.NameDrive))
                    text_message = text_message.Substring(Struct_cloud.NameDrive.Length);
                while (!File.Exists(file_path))
                    Download_message_file(from_chatId, messageId, tmp_path);
                Send_message(to_chat_id, file_path, text_message);
                is_updated = false;
                while (!is_updated)
                    Send_update_move_message(to_chat_id);
                if (from_chatId == to_chat_id)
                {
                    Delete_file(from_chatId, new long[1] { messageId });
                }
                if (MainWindow.isDownloadPhotoPreview)
                    Load_image(file_path, Storage_drives.drives[to_chat_id], to_chat_id);
                if (MainWindow.isDownloadVideoPreview)
                    Load_video(file_path, Storage_drives.drives[to_chat_id], to_chat_id);
                if (tmp_path.Contains("\\tdlib\\"))
                    Directory.Delete(tmp_path, true);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(System.IO.FileNotFoundException))
                    return;
                //Console.WriteLine(ex.GetType());
                MessageBox.Show("I can`t move, something error\n" + ex.ToString());
            }
        }
        public static void Rename_file(long chatId, long messageId, string text_message, string new_name)
        {
            try
            {
                var tmp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tdlib", "tmp", text_message);
                var old_file_path = Path.Combine(tmp_path, Struct_cloud.Get_name_children_by_id(messageId));
                var new_file_path = Path.Combine(tmp_path, new_name);
                while (!File.Exists(old_file_path))
                    Download_message_file(chatId, messageId, tmp_path);
                File.Move(old_file_path, new_file_path);
                Send_message(chatId, new_file_path, text_message);
                is_updated = false;
                while (!is_updated)
                    Send_update_message(chatId);
                Delete_file(chatId, new long[1] { messageId });
                if (MainWindow.isDownloadPhotoPreview)
                    Load_image(new_file_path, Storage_drives.drives[chatId], chatId);
                if (MainWindow.isDownloadVideoPreview)
                    Load_video(new_file_path, Storage_drives.drives[chatId], chatId);
                if (tmp_path.Contains("\\tdlib\\"))
                    Directory.Delete(tmp_path, true);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(System.IO.FileNotFoundException))
                    return;
                //Console.WriteLine(ex.GetType());
                MessageBox.Show("I can`t move, something error\n" + ex.ToString());
            }
        }
        public static void Delete_files(long chatId, long[] messageId)
        {
            current_chat_id = chatId;
            //GetMessage
            if (messageId.Length == 0)
                return;
            foreach (var i in messageId)
            {
                _isResultReceived = false;
                is_updated = false;
                var func = new TdApi.GetMessage(chatId, i);
                while (!is_updated)
                {
                    _client.Send(func, _delete_messageHandler);
                    while (!_isResultReceived)
                        Td.Client.Execute(func);
                }
                _isResultReceived = false;
                var func1 = new TdApi.DeleteMessages(chatId, new long[1] { i }, true);
                _client.Send(func1, _defaultHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func1);
            }            
        }
        public static void Delete_file(long chatId, long[] messageId)
        {
            //GetMessage
            current_chat_id = chatId;
            foreach (var i in messageId)
            {
                _isResultReceived = false;
                is_updated = false;
                var func = new TdApi.GetMessage(chatId, i);
                while (!is_updated)
                {
                    _client.Send(func, _delete_messageHandler);
                    while (!_isResultReceived)
                        Td.Client.Execute(func);
                }
            }
            is_updated = false;
            while (!is_updated)
            {
                _isResultReceived = false;
                var func1 = new TdApi.DeleteMessages(chatId, messageId, true);
                _client.Send(func1, _update_delete_messageHandler);
                while (!_isResultReceived)
                    Td.Client.Execute(func1);
            }
        }
        public static void Get_Properties(long chatId, long[] messageId)
        {
            if (messageId.Length == 0)
                return;
            if (messageId.Length > 1)
                return;
            foreach (var i in messageId)
            {
                _isResultReceived = false;
                is_updated = false;
                var func = new TdApi.GetMessage(chatId, i);
                while (!is_updated)
                {
                    _client.Send(func, _properties_messageHandler);
                    while (!_isResultReceived)
                        Td.Client.Execute(func);
                }
            }
        }
        private static void GetCommand()
        {
            string command = ReadLine(_commandsLine);
            string[] commands = command.Split(new char[] { ' ' }, 2);
            try
            {
                switch (commands[0])
                {
                    case "1":
                        //_client.Send(new TdApi.GetChatHistory(-1001864694048, 0, 0, 5, false), _defaultHandler); //повертає максимум 100 повідомлень з чату //але можна брати по 100 повідомлень у циклі, треба починати з айді потрібного повідомлення
                        //_client.Send(new TdApi.GetMessages(-1001864694048, new long[] { 276074332160 }) , _defaultHandler); //можна продивитися всі повідомлення, але треба знайти айді кожного з них
                        //_client.Send(new TdApi.GetChatMessageCount(-1001864694048, new TdApi.SearchMessagesFilterAudio(), false), _defaultHandler); //повертає ПРИБЛИЗНУ (не перевірено) кількість повідомлень по заданому фільтру (фільтри не вивчав)
                        //searchChatMessages пошук повідомлення в чаті по заданному слову
                        //_client.Send(new TdApi.SearchChats("Test", 2), _defaultHandler); //Пошук чатів по імені


                        //Search my chat
                        var func_chats = new TdApi.SearchChatsOnServer("Test drive 1", 1);
                        _client.Send(func_chats, _chatHandler); //Пошук чатів по імені

                        while (!_isResultReceived)
                        {
                            Td.Client.Execute(func_chats);
                        }
                        //Console.WriteLine(chat_id);

                        //Search all messages (for DB)

                        /*List<string> result_list = new List<string>() { };
                        _isResultReceived = false;
                        while (result != "")
                        {
                            var func_message = new TdApi.GetChatHistory(chat_id, the_last_message_id, 0, 1, false);
                            _client.Send(func_message, _messageHandler);
                            while (!_isResultReceived)
                            {
                                Td.Client.Execute(func_message);
                            }
                            _isResultReceived = false;
                            result_list.Add(result);
                        }
                        _isResultReceived = false;
                        result_list = result_list.Distinct().ToList();
                        foreach (string s in result_list)
                            Console.WriteLine(s + " ");
                        //result_list.Clear();
                        the_last_message_id = 0;
                        result = "No empty";*/

                        //Upload file to chat

                        /*var file = new TdApi.InputFileLocal("D:\\work\\Project_telegram_cloud\\test.txt");
                        var text = new TdApi.FormattedText("second message", null);

                        var func_send_file = new TdApi.InputMessageDocument(file, null, false, text);
                        _client.Send(new TdApi.SendMessage(chat_id, 0, 0, null, null, func_send_file), _defaultHandler);*/


                        //Download file from cloud

                        /*var func_download_doc = new TdApi.SearchChatMessages(chat_id, result_list[0], null, 0, 0, 1, null *//*filter may change*//*, 0);
                        _client.Send(func_download_doc, _documentHandler);
                        while (!_isResultReceived)
                        {
                            Td.Client.Execute(func_download_doc);
                        }
                        Console.WriteLine(doc_id);
                        _client.Send(new TdApi.DownloadFile((Int32)doc_id, 1, 0, 0, true), _download_documentHandler); //можна використовувати false для виводу результату завантаження*/


                        break;
                    case "2":
                        string path = @"tdlib\documents\Новий Лист Microsoft Excel.xlsx";

                        Process.Start(path);

                        break;
                    case "3":
                        string directoryPath = @"tdlib\documents";

                        string[] filePaths = Directory.GetFiles(directoryPath);
                        foreach (string filePath in filePaths)
                        {
                            try
                            {
                                File.Delete(filePath);
                            }
                            catch
                            {
                                Console.WriteLine("Error with delete a file");
                            }
                        }
                        break;
                    case "gc":
                        _client.Send(new TdApi.GetChat(GetChatId(commands[1])), _defaultHandler);
                        break;
                    case "me":
                        _client.Send(new TdApi.GetMe(), _defaultHandler);
                        break;
                    case "sm":
                        string[] args = commands[1].Split(new char[] { ' ' }, 2);
                        sendMessage(GetChatId(args[0]), args[1]);
                        break;
                    case "lo":
                        _haveAuthorization = false;
                        _client.Send(new TdApi.LogOut(), _defaultHandler);
                        break;
                    case "r":
                        _haveAuthorization = false;
                        _client.Send(new TdApi.Close(), _defaultHandler);
                        break;
                    case "q":
                        _needQuit = true;
                        _haveAuthorization = false;
                        _client.Send(new TdApi.Close(), _defaultHandler);
                        break;
                    default:
                        Print("Unsupported command: " + command);
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Print("Not enough arguments");
            }
        }

        private static void sendMessage(long chatId, string message)
        {
            // initialize reply markup just for testing
            TdApi.InlineKeyboardButton[] row = { new TdApi.InlineKeyboardButton("https://telegram.org?1", new TdApi.InlineKeyboardButtonTypeUrl()), new TdApi.InlineKeyboardButton("https://telegram.org?2", new TdApi.InlineKeyboardButtonTypeUrl()), new TdApi.InlineKeyboardButton("https://telegram.org?3", new TdApi.InlineKeyboardButtonTypeUrl()) };
            TdApi.ReplyMarkup replyMarkup = new TdApi.ReplyMarkupInlineKeyboard(new TdApi.InlineKeyboardButton[][] { row, row, row });

            TdApi.InputMessageContent content = new TdApi.InputMessageText(new TdApi.FormattedText(message, null), false, true);
            _client.Send(new TdApi.SendMessage(chatId, 0, 0, null, replyMarkup, content), _defaultHandler);
        }

        public static void Start()
        {
            
            //_haveAuthorization = false;
            _needQuit = false;
            _gotAuthorization = new AutoResetEvent(false);
            // disable TDLib log
            Td.Client.Execute(new TdApi.SetLogVerbosityLevel(0));
            if (Td.Client.Execute(new TdApi.SetLogStream(new TdApi.LogStreamFile("tdlib.log", 1 << 27, false))) is TdApi.Error)
            {
                throw new System.IO.IOException("Write access to the current directory is required");
            }
            Td_cloud_start = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Td.Client.Run();
            });
            Td_cloud_start.Start();

            // test Client.Execute
            _defaultHandler.OnResult(Td.Client.Execute(new TdApi.GetTextEntities("@telegram /test_command https://telegram.org telegram.me @gif @test")));
            if (Td_cloud_auto_form != null)
                Td_cloud_auto_form.Abort();
            Td_cloud_auto_form = new Thread(o =>
            {
                autoForm = new Authorization();
                autoForm.Show();
                System.Windows.Threading.Dispatcher.Run();
            });
            Td_cloud_auto_form.SetApartmentState(ApartmentState.STA);
            Td_cloud_auto_form.Start();
            // main loop
            while (!_needQuit)
            {
                _gotAuthorization.Reset();
                _gotAuthorization.WaitOne();
            }
        }
    }
}
