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
    /// Логика взаимодействия для Change_or_nothing.xaml
    /// </summary>
    public partial class Change_or_nothing : Window
    {
        public Change_or_nothing()
        {
            InitializeComponent();
            Change_language();
        }

        private void Button_yes_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBox_do_it_for_all.IsChecked)
                MainWindow.skip_all_next_files = true;
            MainWindow.change_file_in_drive = true;
            this.Close();
        }

        private void Button_no_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBox_do_it_for_all.IsChecked)
                MainWindow.skip_all_next_files = true;
            MainWindow.change_file_in_drive = false;
            this.Close();
        }
        private void Change_language()
        {
            if (MainWindow.current_lang == MainWindow.Languages.en)
            {
                Cange_or_nothing_text.Text = MainWindow.name_exist_file + "\n" + Telegram_cloud.Resources.Change_or_nothing_english.Cange_or_nothing_text_Text;
                CheckBox_do_it_for_all.Content = Telegram_cloud.Resources.Change_or_nothing_english.CheckBox_do_it_for_all_Content;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ua)
            {
                Cange_or_nothing_text.Text = MainWindow.name_exist_file + "\n" + Telegram_cloud.Resources.Change_or_nothing_ukrainian.Cange_or_nothing_text_Text;
                CheckBox_do_it_for_all.Content = Telegram_cloud.Resources.Change_or_nothing_ukrainian.CheckBox_do_it_for_all_Content;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ru)
            {
                Cange_or_nothing_text.Text = MainWindow.name_exist_file + "\n" + Telegram_cloud.Resources.Change_or_nothing_russian.Cange_or_nothing_text_Text;
                CheckBox_do_it_for_all.Content = Telegram_cloud.Resources.Change_or_nothing_russian.CheckBox_do_it_for_all_Content;
            }
        }
    }
}
