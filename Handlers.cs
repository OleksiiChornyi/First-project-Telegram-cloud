using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;
using Telegram_cloud;
using System.Windows;
using Telegram.Td;
using System.IO;

namespace Telegram_cloud
{
    internal class Handlers
    {
        public class DefaultHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                //string qwe = @object.ToString();
                //MessageBox.Show(@object.ToString());
                //Console.WriteLine("default");
                //Console.WriteLine(@object.ToString());
                TdCloud._isResultReceived = true;
            }
        }
        public class SendMessageHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Message))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                }
                catch
                {
                    //TdCloud._isResultReceived = true;
                }
                TdCloud._isResultReceived = true;
            }
        }
        public class EditMessageHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Message))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                }
                catch
                {
                    //TdCloud._isResultReceived = true;
                }
                TdCloud._isResultReceived = true;
            }
        }
        public class MessageBoxHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.User))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    About_account.about_text = (Telegram.Td.Api.User)@object;
                }
                catch { }
                TdCloud._isResultReceived = true;
            }
        }
        public class CheckAccountPremiumHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.User))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    if (((Telegram.Td.Api.User)(@object)).IsPremium)
                    {
                        MainWindow.sizeLoadFileGigabytes = 4.0;
                    }
                    else
                    {
                        MainWindow.sizeLoadFileGigabytes = 2.0;
                    }
                }
                catch { }
                TdCloud._isResultReceived = true;
            }
        }
        public class SendUpdateMessageHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.FoundChatMessages))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    if (TdCloud.all_messages_id[TdCloud.current_chat_id].Contains(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id))
                    {
                        TdCloud._isResultReceived = true;
                        return;
                    }
                    if ((((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).GetType() == typeof(Telegram.Td.Api.MessageDocument))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Document.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;

                    }
                    else if (((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content.GetType() == typeof(Telegram.Td.Api.MessagePhoto))
                    {
                        double timestamp = ((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Date;
                        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        DateTime dateTime = origin.AddSeconds(timestamp).ToLocalTime();
                        string file_name = "photo_" + dateTime.ToString("yyyy-MM-dd_HH-mm-ss") + ((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id + ".jpg";
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessagePhoto)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, file_name));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;
                    }
                    else if (((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content.GetType() == typeof(Telegram.Td.Api.MessageVideo))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, ((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Video.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;
                    }
                    else if (((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content.GetType() == typeof(Telegram.Td.Api.MessageAnimation))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, ((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Animation.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;
                    }
                    else if ((((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).GetType() == typeof(Telegram.Td.Api.MessageText))
                    {
                        TdCloud.all_paths_to_directiryes_without_files[TdCloud.current_chat_id].Add(((Telegram.Td.Api.MessageText)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Text.Text);
                        TdCloud.all_messages_directiryes_without_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;
                    }
                }
                catch
                {
                    TdCloud.result = "";
                }
                TdCloud._isResultReceived = true;
            }
        }
        public class EditUpdateMessageHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.FoundChatMessages))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    if (TdCloud.all_messages_id[TdCloud.current_chat_id].Contains(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id))
                    {
                        TdCloud._isResultReceived = true;
                        return;
                    }
                    if ((((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).GetType() == typeof(Telegram.Td.Api.MessageDocument))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Document.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;

                    }
                    else if (((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content.GetType() == typeof(Telegram.Td.Api.MessagePhoto))
                    {
                        double timestamp = ((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Date;
                        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        DateTime dateTime = origin.AddSeconds(timestamp).ToLocalTime();
                        string file_name = "photo_" + dateTime.ToString("yyyy-MM-dd_HH-mm-ss") + ((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id + ".jpg";
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessagePhoto)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, file_name));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                    }
                    else if (((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content.GetType() == typeof(Telegram.Td.Api.MessageVideo))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, ((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Video.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                    }
                    else if (((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content.GetType() == typeof(Telegram.Td.Api.MessageAnimation))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, ((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Animation.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;
                    }
                    else if ((((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).GetType() == typeof(Telegram.Td.Api.MessageText))
                    {
                        TdCloud.all_paths_to_directiryes_without_files[TdCloud.current_chat_id].Add(((Telegram.Td.Api.MessageText)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Text.Text);
                        TdCloud.all_messages_directiryes_without_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;
                    }
                }
                catch
                {
                    TdCloud.result = "";
                }
                TdCloud._isResultReceived = true;
            }
        }
        public class SendUpdateMoveMessageHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.FoundChatMessages))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    if (TdCloud.all_messages_id[TdCloud.to_chat_id_message_move].Contains(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id))
                    {
                        TdCloud._isResultReceived = true;
                        return;
                    }
                    if ((((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).GetType() == typeof(Telegram.Td.Api.MessageDocument))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.to_chat_id_message_move].Add(Path.Combine(((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Document.FileName));
                        TdCloud.all_messages_files_id[TdCloud.to_chat_id_message_move].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.to_chat_id_message_move].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;

                    }
                    else if (((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content.GetType() == typeof(Telegram.Td.Api.MessagePhoto))
                    {
                        double timestamp = ((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Date;
                        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        DateTime dateTime = origin.AddSeconds(timestamp).ToLocalTime();
                        string file_name = "photo_" + dateTime.ToString("yyyy-MM-dd_HH-mm-ss") + ((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id + ".jpg";
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessagePhoto)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, file_name));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                    }
                    else if (((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content.GetType() == typeof(Telegram.Td.Api.MessageVideo))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, ((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Video.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                    }
                    else if (((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content.GetType() == typeof(Telegram.Td.Api.MessageAnimation))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.to_chat_id_message_move].Add(Path.Combine(((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Caption.Text, ((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Animation.FileName));
                        TdCloud.all_messages_files_id[TdCloud.to_chat_id_message_move].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.to_chat_id_message_move].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;
                    }
                    else if ((((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).GetType() == typeof(Telegram.Td.Api.MessageText))
                    {
                        TdCloud.all_paths_to_directiryes_without_files[TdCloud.to_chat_id_message_move].Add(((Telegram.Td.Api.MessageText)((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Content).Text.Text);
                        TdCloud.all_messages_directiryes_without_files_id[TdCloud.to_chat_id_message_move].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.all_messages_id[TdCloud.to_chat_id_message_move].Add(((Telegram.Td.Api.FoundChatMessages)(@object)).Messages[0].Id);
                        TdCloud.is_updated = true;
                    }
                }
                catch
                {
                    TdCloud.result = "";
                }
                TdCloud._isResultReceived = true;
            }
        }
        public class CountMessagesHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Count))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    TdCloud.count_messages += ((Telegram.Td.Api.Count)(@object)).CountValue;
                }
                catch
                {
                    TdCloud.result = "";
                }
                TdCloud._isResultReceived = true;
            }
        }
        public class MessagesHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Messages))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    if (((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content.GetType() == typeof(Telegram.Td.Api.MessageDocument) && ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Document.DocumentValue.Remote.IsUploadingCompleted)
                    {
                       TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Caption.Text, ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Document.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                    }
                    else if (((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content.GetType() == typeof(Telegram.Td.Api.MessagePhoto) && ((Telegram.Td.Api.MessagePhoto)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Photo.Sizes[0].Photo.Remote.IsUploadingCompleted)
                    {
                        double timestamp = ((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Date;
                        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        DateTime dateTime = origin.AddSeconds(timestamp).ToLocalTime();
                        string file_name = "photo_" + dateTime.ToString("yyyy-MM-dd_HH-mm-ss") + ((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id + ".jpg";
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessagePhoto)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Caption.Text, file_name));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                    }
                    else if (((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content.GetType() == typeof(Telegram.Td.Api.MessageVideo) && ((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Video.VideoValue.Remote.IsUploadingCompleted)
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Caption.Text, ((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Video.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                    }
                    else if (((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content.GetType() == typeof(Telegram.Td.Api.MessageAnimation) && ((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Animation.AnimationValue.Remote.IsUploadingCompleted)
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Add(Path.Combine(((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Caption.Text, ((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Animation.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                    }
                    else if (((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content.GetType() == typeof(Telegram.Td.Api.MessageText))
                    {
                        TdCloud.all_paths_to_directiryes_without_files[TdCloud.current_chat_id].Add(((Telegram.Td.Api.MessageText)((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Content).Text.Text);
                        TdCloud.all_messages_directiryes_without_files_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                    }
                    else
                    {
                        TdCloud.all_messages_id[TdCloud.current_chat_id].Add(((Telegram.Td.Api.Messages)(@object)).MessagesValue[0].Id);
                    }
                }
                catch
                {
                    TdCloud.result = "";
                }
                TdCloud._isResultReceived = true;
            }
        }
        public class DownloadDocHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.File))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                if (!((Telegram.Td.Api.File)(@object)).Local.CanBeDownloaded)
                {
                    //MessageBox.Show("This document cannot be downloaded");
                    TdCloud._isResultReceived = true;
                    TdCloud.can_download = false;
                    return;
                }
                try
                {
                    TdCloud.path = ((Telegram.Td.Api.File)(@object)).Local.Path;
                    TdCloud._isResultReceived = true;
                }
                catch { }
            }
        }
        public class UpdateDeleteHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Ok))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                else
                {
                    try
                    {
                        TdCloud.is_updated = true;
                    }
                    catch { }
                }
                TdCloud._isResultReceived = true;
            }
        }
        public class DeleteDocHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Message))
                {
                    if (@object.GetType() == typeof(Telegram.Td.Api.Error))
                    {
                        TdCloud._isResultReceived = true;
                        return;
                    }
                }
                try
                {
                    if (((Telegram.Td.Api.Message)(@object)).Content.GetType() == typeof(Telegram.Td.Api.MessageDocument))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Remove(Path.Combine(((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.Message)(@object)).Content).Caption.Text, ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.Message)(@object)).Content).Document.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Remove(((Telegram.Td.Api.Message)(@object)).Id);
                    }
                    else if (((Telegram.Td.Api.Message)(@object)).Content.GetType() == typeof(Telegram.Td.Api.MessagePhoto))
                    {
                        double timestamp = ((Telegram.Td.Api.Message)(@object)).Date;
                        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        DateTime dateTime = origin.AddSeconds(timestamp).ToLocalTime();
                        string file_name = "photo_" + dateTime.ToString("yyyy-MM-dd_HH-mm-ss") + ((Telegram.Td.Api.Message)(@object)).Id + ".jpg";
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Remove(Path.Combine(((Telegram.Td.Api.MessagePhoto)((Telegram.Td.Api.Message)(@object)).Content).Caption.Text, file_name));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Remove(((Telegram.Td.Api.Message)(@object)).Id);
                    }
                    else if (((Telegram.Td.Api.Message)(@object)).Content.GetType() == typeof(Telegram.Td.Api.MessageVideo))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Remove(Path.Combine(((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.Message)(@object)).Content).Caption.Text, ((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.Message)(@object)).Content).Video.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Remove(((Telegram.Td.Api.Message)(@object)).Id);
                    }
                    else if (((Telegram.Td.Api.Message)(@object)).Content.GetType() == typeof(Telegram.Td.Api.MessageAnimation))
                    {
                        TdCloud.all_paths_to_files_with_name[TdCloud.current_chat_id].Remove(Path.Combine(((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.Message)(@object)).Content).Caption.Text, ((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.Message)(@object)).Content).Animation.FileName));
                        TdCloud.all_messages_files_id[TdCloud.current_chat_id].Remove(((Telegram.Td.Api.Message)(@object)).Id);
                    }
                    else if (((Telegram.Td.Api.Message)(@object)).Content.GetType() == typeof(Telegram.Td.Api.MessageText))
                    {
                        TdCloud.all_paths_to_directiryes_without_files[TdCloud.current_chat_id].Remove(((Telegram.Td.Api.MessageText)((Telegram.Td.Api.Message)(@object)).Content).Text.Text);
                        TdCloud.all_messages_directiryes_without_files_id[TdCloud.current_chat_id].Remove(((Telegram.Td.Api.Message)(@object)).Id);
                    }
                    TdCloud.all_messages_id[TdCloud.current_chat_id].Remove(((Telegram.Td.Api.Message)(@object)).Id);
                }
                catch
                {
                    TdCloud.result = "";
                }
                TdCloud._isResultReceived = true;
                TdCloud.is_updated = true;
            }
        }
        public class PropertiesHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Message))
                {
                    if (@object.GetType() == typeof(Telegram.Td.Api.Error))
                    {
                        TdCloud._isResultReceived = true;
                        return;
                    }
                    TdCloud.is_updated = true;
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    double timestamp = ((Telegram.Td.Api.Message)(@object)).Date;
                    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    TdCloud.properties_message_datetime = origin.AddSeconds(timestamp).ToLocalTime();
                    TdCloud.properties_message_is_pinned = ((Telegram.Td.Api.Message)(@object)).IsPinned;
                    if (((Telegram.Td.Api.Message)(@object)).Content.GetType() == typeof(Telegram.Td.Api.MessageDocument))
                    {
                        TdCloud.properties_message_name = ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.Message)(@object)).Content).Document.FileName;
                        var bits = ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.Message)(@object)).Content).Document.DocumentValue.Size;
                        TdCloud.properties_message_size = bits / (8 * Math.Pow(10, 6));
                    }
                }
                catch
                {
                    TdCloud.result = "";
                }
                TdCloud._isResultReceived = true;
                TdCloud.is_updated = true;
            }
        }
        public class CreateNewChannel : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(TdApi.Chat))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    TdCloud.chat_id = ((Telegram.Td.Api.Chat)(@object)).Id;
                }
                catch { }
                TdCloud._isResultReceived = true;
            }
        }
        public class GetFileIDHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Message))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    if (((Telegram.Td.Api.Message)(@object)).Content.GetType() == typeof(Telegram.Td.Api.MessagePhoto))
                    {
                        long max_size = 0;
                        foreach (var item in ((Telegram.Td.Api.MessagePhoto)((Telegram.Td.Api.Message)(@object)).Content).Photo.Sizes)
                        {
                            if (item.Photo.Size > max_size)
                            {
                                max_size = item.Photo.Size;
                                TdCloud.doc_id = item.Photo.Id;
                            }
                        }
                        double timestamp = ((Telegram.Td.Api.Message)(@object)).Date;
                        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        DateTime dateTime = origin.AddSeconds(timestamp).ToLocalTime();
                        TdCloud.file_name = "photo_" + dateTime.ToString("yyyy-MM-dd_HH-mm-ss") + ((Telegram.Td.Api.Message)@object).Id + ".jpg";
                        TdCloud._isResultReceived = true;
                    }
                    if (((Telegram.Td.Api.Message)(@object)).Content.GetType() == typeof(Telegram.Td.Api.MessageVideo))
                    {
                        TdCloud.doc_id = ((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.Message)(@object)).Content).Video.VideoValue.Id;
                        TdCloud.file_name = ((Telegram.Td.Api.MessageVideo)((Telegram.Td.Api.Message)(@object)).Content).Video.FileName;
                        TdCloud._isResultReceived = true;
                    }
                    else if (((Telegram.Td.Api.Message)(@object)).Content.GetType() == typeof(Telegram.Td.Api.MessageAnimation))
                    {
                        TdCloud.doc_id = ((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.Message)(@object)).Content).Animation.AnimationValue.Id;
                        TdCloud.file_name = ((Telegram.Td.Api.MessageAnimation)((Telegram.Td.Api.Message)(@object)).Content).Animation.FileName;
                        TdCloud._isResultReceived = true;
                    }
                    else
                    {
                        TdCloud.doc_id = ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.Message)(@object)).Content).Document.DocumentValue.Id;
                        TdCloud.file_name = ((Telegram.Td.Api.MessageDocument)((Telegram.Td.Api.Message)(@object)).Content).Document.FileName;
                        TdCloud._isResultReceived = true;
                    }
                }
                catch { }
            }
        }
        public class CountMessagesChatHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Count))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    TdCloud.count_messages += ((Telegram.Td.Api.Count)(@object)).CountValue;
                }
                catch { }
                TdCloud._isResultReceived = true;
            }
        }
        public class AllChatsIdsHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(TdApi.Chats))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    TdCloud.all_chats_id = ((Telegram.Td.Api.Chats)(@object)).ChatIds.ToList();
                }
                catch { }
                TdCloud._isResultReceived = true;
            }
        }
        public class NameAllChatsHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Chat))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                try
                {
                    TdCloud.dict_chats.Add(((Telegram.Td.Api.Chat)(@object)).Id, ((Telegram.Td.Api.Chat)(@object)).Title);
                }
                catch { }
                TdCloud._isResultReceived = true;
            }
        }
        public class ForwardHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object.GetType() != typeof(Telegram.Td.Api.Message))
                {
                    TdCloud._isResultReceived = true;
                    return;
                }
                TdCloud._isResultReceived = true;
            }
        }
        public class UpdateHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object is TdApi.UpdateAuthorizationState)
                {
                    TdCloud.OnAuthorizationStateUpdated((@object as TdApi.UpdateAuthorizationState).AuthorizationState);
                }
            }
        }

        public class AuthorizationRequestHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object is TdApi.Error)
                {
                    if (((Telegram.Td.Api.Error)(@object)).Code != 500)
                        MessageBox.Show("Receive an error:" + TdCloud._newLine + @object);
                    TdCloud.OnAuthorizationStateUpdated(null); // repeat last action
                }
            }
        }
    }
}
