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
    /// Логика взаимодействия для Rename_message.xaml
    /// </summary>
    public partial class Rename_message : Window
    {
        public Rename_message()
        {
            InitializeComponent();
            Rename_message_text.Focus();
            if (MainWindow.current_lang == MainWindow.Languages.en)
            {
                Rename_message_background_text.Text = Telegram_cloud.Resources.Rename_message_english.Rename_message_background_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ua)
            {
                Rename_message_background_text.Text = Telegram_cloud.Resources.Rename_message_ukrainian.Rename_message_background_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ru)
            {
                Rename_message_background_text.Text = Telegram_cloud.Resources.Rename_message_russian.Rename_message_background_text_Text;
            }
        }

        private void Rename_message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;
            MainWindow.new_name_file = Rename_message_text.Text;
            this.Close();
        }
    }
}
