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
    /// Логика взаимодействия для About_program.xaml
    /// </summary>
    public partial class About_program : Window
    {
        public About_program()
        {
            InitializeComponent();
            if (MainWindow.current_lang == MainWindow.Languages.en)
            {
                About_program_text.Text = Telegram_cloud.Resources.About_program_english.About_program_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ua)
            {
                About_program_text.Text = Telegram_cloud.Resources.About_program_ukrainian.About_program_text_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ru)
            {
                About_program_text.Text = Telegram_cloud.Resources.About_program_russian.About_program_text_Text;
            }
        }
    }
}
