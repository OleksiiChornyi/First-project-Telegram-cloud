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
    /// Логика взаимодействия для Add_drive.xaml
    /// </summary>
    public partial class Add_drive : Window
    {
        public Add_drive()
        {
            InitializeComponent();
            Name_drive.Focus();
            if (MainWindow.current_lang == MainWindow.Languages.en)
            {
                Name_drive_background_text.Text = Telegram_cloud.Resources.Add_drive_english.Name_drive_background_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ua)
            {
                Name_drive_background_text.Text = Telegram_cloud.Resources.Add_drive_ukrainian.Name_drive_background_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ru)
            {
                Name_drive_background_text.Text = Telegram_cloud.Resources.Add_drive_russian.Name_drive_background_text_Text;
            }
        }

        private void Name_drive_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || Name_drive.Text.Replace(" ", "") == "")
                return;
            if (MainWindow.current_lang == MainWindow.Languages.en)
            {
                TdCloud.Create_Channel(Name_drive.Text, Telegram_cloud.Resources.Add_drive_english.Description_drive);
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ua)
            {
                TdCloud.Create_Channel(Name_drive.Text, Telegram_cloud.Resources.Add_drive_ukrainian.Description_drive);
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ru)
            {
                TdCloud.Create_Channel(Name_drive.Text, Telegram_cloud.Resources.Add_drive_russian.Description_drive);
            }
            this.Close();
        }
    }
}
