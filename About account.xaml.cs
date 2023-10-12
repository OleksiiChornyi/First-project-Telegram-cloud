using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Telegram_cloud
{
    /// <summary>
    /// Логика взаимодействия для About_account.xaml
    /// </summary>
    public partial class About_account : Window
    {
        public static Telegram.Td.Api.User about_text;
        public About_account()
        {
            InitializeComponent();
            TdCloud.Get_About_account();
            Change_language();
            if (about_text.LastName != null && about_text.FirstName != null)
            {
                Text_block_my_name.Text = about_text.LastName + " " + about_text.FirstName;
            }
            else
            {
                Text_block_my_name.Text = "Some error";
            }
            if (about_text.Id != 0)
            {
                Text_block_my_account_id.Text = about_text.Id.ToString();
            }
            else
            {
                Text_block_my_account_id.Text = "Some error";
            }
            if (about_text.PhoneNumber != null)
            {
                Text_block_my_phone_number.Text = about_text.PhoneNumber.ToString();
            }
            else
            {
                Text_block_my_phone_number.Text = "Some Error";
            }
            if (about_text.ProfilePhoto.Big.Id != 0)
            {
                try
                {
                    string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string exeDir = System.IO.Path.GetDirectoryName(exePath);
                    string file_path = System.IO.Path.Combine(exeDir, "tdlib");
                    TdCloud.Download_file(about_text.ProfilePhoto.Big.Id, file_path, "");
                    string newFilePath = System.IO.Path.Combine(file_path, System.IO.Path.GetFileName(TdCloud.path));
                    BitmapImage bitmapImage = new BitmapImage(new Uri(newFilePath));
                    Image_photo.Source = bitmapImage;
                }
                catch (Exception ex)
                {
                    Text_block_photo.Text = ex.Message;
                }
            }
        }
        private void Change_language()
        {
            if (MainWindow.current_lang == MainWindow.Languages.en)
            {
                Text_block_name.Text = Telegram_cloud.Resources.About_account_english.Text_block_name_Text;
                Text_block_account_id.Text = Telegram_cloud.Resources.About_account_english.Text_block_account_id_Text;
                Text_block_phone_number.Text = Telegram_cloud.Resources.About_account_english.Text_block_phone_number_Text;
                Text_block_photo.Text = Telegram_cloud.Resources.About_account_english.Text_block_photo_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ua)
            {
                Text_block_name.Text = Telegram_cloud.Resources.About_account_ukrainian.Text_block_name_Text;
                Text_block_account_id.Text = Telegram_cloud.Resources.About_account_ukrainian.Text_block_account_id_Text;
                Text_block_phone_number.Text = Telegram_cloud.Resources.About_account_ukrainian.Text_block_phone_number_Text;
                Text_block_photo.Text = Telegram_cloud.Resources.About_account_ukrainian.Text_block_photo_Text;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ru)
            {
                Text_block_name.Text = Telegram_cloud.Resources.About_account_russian.Text_block_name_Text;
                Text_block_account_id.Text = Telegram_cloud.Resources.About_account_russian.Text_block_account_id_Text;
                Text_block_phone_number.Text = Telegram_cloud.Resources.About_account_russian.Text_block_phone_number_Text;
                Text_block_photo.Text = Telegram_cloud.Resources.About_account_russian.Text_block_photo_Text;
            }
        }
    }
}
