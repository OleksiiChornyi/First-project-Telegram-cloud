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
    /// Логика взаимодействия для Create_directory.xaml
    /// </summary>
    public partial class Create_directory : Window
    {
        public Create_directory()
        {
            InitializeComponent();
            Name_directory.Focus();
            if (MainWindow.current_lang == MainWindow.Languages.en)
            {
                Name_directory_background_text.Text = Telegram_cloud.Resources.Create_directory_english.Name_directory_background_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ua)
            {
                Name_directory_background_text.Text = Telegram_cloud.Resources.Create_directory_ukrainian.Name_directory_background_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ru)
            {
                Name_directory_background_text.Text = Telegram_cloud.Resources.Create_directory_russian.Name_directory_background_text_Text;
            }
        }

        private void Name_directory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || Name_directory.Text.Replace(" ", "") == "")
                return;
            if (!Struct_cloud.Check_directory_in_children(Name_directory.Text))
                TdCloud.Add_directory(Struct_cloud.IdDrive, Name_directory.Text, Struct_cloud.Get_path());
            this.Close();
        }
    }
}
