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
    /// Interaction logic for Properties_file.xaml
    /// </summary>
    public partial class Properties_file : Window
    {
        public Properties_file()
        {
            InitializeComponent();
            Add_data();
        }
        private void Add_data()
        {
            Text_block_file_name.Text = TdCloud.properties_message_name;
            Text_block_file_datetime.Text = TdCloud.properties_message_datetime.ToString();
            Text_block_file_is_pinned.Text = TdCloud.properties_message_is_pinned.ToString();
            Text_block_file_size.Text = TdCloud.properties_message_size.ToString("F2") + " MB";
        }
    }
}
