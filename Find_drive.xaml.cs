using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Telegram_cloud
{
    /// <summary>
    /// Логика взаимодействия для Find_drive.xaml
    /// </summary>
    public partial class Find_drive : Window
    {
        private static List<CheckBox> CheckBoxes = new List<CheckBox>();
        public Find_drive()
        {
            InitializeComponent();
            TdCloud.Find_Channel();
            Thickness tmp_margin = new Thickness(5, 15, 0, 0);
            CheckBoxes.Clear();
            foreach (var item in TdCloud.dict_chats)
            {
                var tmp = new CheckBox();
                tmp.Content = item.Value;
                tmp.ToolTip = item.Key;
                if (Storage_drives.drives.ContainsKey(item.Key))
                    tmp.IsChecked = true;
                tmp.Margin = tmp_margin;
                CheckBoxes.Add(tmp);
                //Stack_chekboxes.Children.Add(tmp);
            }
            CheckBoxes = CheckBoxes.OrderByDescending(checkBox => checkBox.IsChecked == true).ToList();
            Stack_chekboxes.Children.Clear();
            foreach (var checkBox in CheckBoxes)
            {
                Stack_chekboxes.Children.Add(checkBox);
            }

            Scroll_viewer.Content = Stack_chekboxes;

            Find_drive_text.Focus();
            if (MainWindow.current_lang == MainWindow.Languages.en)
            {
                Find_drive_background_text.Text = Telegram_cloud.Resources.Find_drive_english.Find_drive_background_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ua)
            {
                Find_drive_background_text.Text = Telegram_cloud.Resources.Find_drive_ukrainian.Find_drive_background_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ru)
            {
                Find_drive_background_text.Text = Telegram_cloud.Resources.Find_drive_russian.Find_drive_background_text_Text;
            }
        }

        private void Stack_chekboxes_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach (var item in CheckBoxes)
            {
                if (item.IsChecked == true && !Storage_drives.drives.ContainsKey((long)item.ToolTip))
                    Storage_drives.Add_drives(item.Content.ToString(), (long)item.ToolTip);
                if (item.IsChecked == false && Storage_drives.drives.ContainsKey((long)item.ToolTip))
                    Storage_drives.Delete_drives((long)item.ToolTip);
            }
        }

        private void Name_drive_TextChanged(object sender, TextChangedEventArgs e)
        {
            Thickness tmp_margin = new Thickness(5, 15, 0, 0);
            Stack_chekboxes.Children.Clear();
            foreach (var item in TdCloud.dict_chats)
            {
                if (item.Value.ToLower().Contains(Find_drive_text.Text.ToLower()))
                {
                    var tmp = new CheckBox();
                    tmp.Content = item.Value;
                    tmp.ToolTip = item.Key;
                    if (Storage_drives.drives.ContainsKey(item.Key))
                        tmp.IsChecked = true;
                    tmp.Margin = tmp_margin;
                    CheckBoxes.Add(tmp);
                    Stack_chekboxes.Children.Add(tmp);
                }
            }
            Scroll_viewer.Content = Stack_chekboxes;
        }
    }
}
