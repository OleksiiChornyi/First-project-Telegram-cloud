using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;
using System.Threading;
using System.Text.RegularExpressions;


namespace Telegram_cloud
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public static TdApi.AuthorizationState _authorizationState = null;
        public static readonly string _newLine = Environment.NewLine;
        public bool isReadyPhoneNumber = false;
        public bool isReadyEmail = false;
        public bool isReadyEmailCode = false;
        public bool isReadyPhoneCode = false;
        public bool isReadyPassword = false;
        public bool isReautorization = false;

        public Authorization()
        {
            InitializeComponent();
            Change_language();
        }
        public static void Visible_TextBox(TextBox textBox, string text = "")
        {
            textBox.Text = text;
            textBox.Visibility = Visibility.Visible;
        }

        private void Phone_Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Text;
            int caretIndex = textBox.CaretIndex;

            if (text.Length == 0 || caretIndex == 0)
            {
                if (e.Text == "+")
                {
                    e.Handled = false;
                    return;
                }
            }

            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void Email_Address_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Text;
            int caretIndex = textBox.CaretIndex;

            string pattern = @"/^[a-zA-Z0-9._-]+@[a-zA-Z0-9._-]+\.[a-zA-Z0-9._-]+$/";
            Regex regex = new Regex(pattern);

            if (!regex.IsMatch(text.Insert(caretIndex, e.Text)))
            {
                e.Handled = true;
            }
        }

        private void Phone_Number_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                isReadyPhoneNumber = true;
        }

        private void Email_Address_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                isReadyEmail = true;
        }

        private void Auth_Email_Code_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                isReadyEmailCode = true;
        }

        private void Auth_Phone_Code_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                isReadyPhoneCode = true;
        }

        private void Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                isReadyPassword = true;
        }

        private void Password_MouseEnter(object sender, MouseEventArgs e)
        {
            Password.Effect = null;
        }

        private void Password_MouseLeave(object sender, MouseEventArgs e)
        {
            var blur_effect = new System.Windows.Media.Effects.BlurEffect();
            blur_effect.Radius = 7;
            Password.Effect = blur_effect;
        }
        private void Change_language()
        {
            if (MainWindow.current_lang == MainWindow.Languages.en)
            {
                TextBlock_phone_number_background.Text = Telegram_cloud.Resources.Autorization_english.TextBlock_phone_number_background_Text;
                Phone_Number.Text = Telegram_cloud.Resources.Autorization_english.Phone_Number_Text;
                TextBlock_Auth_Phone_Code_background.Text = Telegram_cloud.Resources.Autorization_english.TextBlock_Auth_Phone_Code_background_Text;
                Auth_Phone_Code.Text = Telegram_cloud.Resources.Autorization_english.Auth_Phone_Code_Text;
                TextBlock_Password_background.Text = Telegram_cloud.Resources.Autorization_english.TextBlock_Password_background_Text;
                Password.Text = Telegram_cloud.Resources.Autorization_english.Password_Text;
                TextBlock_Email_Address_background.Text = Telegram_cloud.Resources.Autorization_english.TextBlock_Email_Address_background_Text;
                Email_Address.Text = Telegram_cloud.Resources.Autorization_english.Email_Address_Text;
                TextBlock_Auth_Email_Code_background.Text = Telegram_cloud.Resources.Autorization_english.TextBlock_Auth_Email_Code_background_Text;
                Auth_Email_Code.Text = Telegram_cloud.Resources.Autorization_english.Auth_Email_Code_Text;
                Phone_Number_label.Content = Telegram_cloud.Resources.Autorization_english.Phone_Number_label_Content;
                Auth_Phone_Code_label.Content = Telegram_cloud.Resources.Autorization_english.Auth_Phone_Code_label_Content;
                Password_label.Content = Telegram_cloud.Resources.Autorization_english.Password_label_Content;
                Email_Address_label.Content = Telegram_cloud.Resources.Autorization_english.Email_Address_label_Content;
                Auth_Email_Code_label.Content = Telegram_cloud.Resources.Autorization_english.Auth_Email_Code_label_Content;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ua)
            {
                TextBlock_phone_number_background.Text = Telegram_cloud.Resources.Autorization_ukrainian.TextBlock_phone_number_background_Text;
                Phone_Number.Text = Telegram_cloud.Resources.Autorization_ukrainian.Phone_Number_Text;
                TextBlock_Auth_Phone_Code_background.Text = Telegram_cloud.Resources.Autorization_ukrainian.TextBlock_Auth_Phone_Code_background_Text;
                Auth_Phone_Code.Text = Telegram_cloud.Resources.Autorization_ukrainian.Auth_Phone_Code_Text;
                TextBlock_Password_background.Text = Telegram_cloud.Resources.Autorization_ukrainian.TextBlock_Password_background_Text;
                Password.Text = Telegram_cloud.Resources.Autorization_ukrainian.Password_Text;
                TextBlock_Email_Address_background.Text = Telegram_cloud.Resources.Autorization_ukrainian.TextBlock_Email_Address_background_Text;
                Email_Address.Text = Telegram_cloud.Resources.Autorization_ukrainian.Email_Address_Text;
                TextBlock_Auth_Email_Code_background.Text = Telegram_cloud.Resources.Autorization_ukrainian.TextBlock_Auth_Email_Code_background_Text;
                Auth_Email_Code.Text = Telegram_cloud.Resources.Autorization_ukrainian.Auth_Email_Code_Text;
                Phone_Number_label.Content = Telegram_cloud.Resources.Autorization_ukrainian.Phone_Number_label_Content;
                Auth_Phone_Code_label.Content = Telegram_cloud.Resources.Autorization_ukrainian.Auth_Phone_Code_label_Content;
                Password_label.Content = Telegram_cloud.Resources.Autorization_ukrainian.Password_label_Content;
                Email_Address_label.Content = Telegram_cloud.Resources.Autorization_ukrainian.Email_Address_label_Content;
                Auth_Email_Code_label.Content = Telegram_cloud.Resources.Autorization_ukrainian.Auth_Email_Code_label_Content;
            }
            else if (MainWindow.current_lang == MainWindow.Languages.ru)
            {
                TextBlock_phone_number_background.Text = Telegram_cloud.Resources.Autorization_russian.TextBlock_phone_number_background_Text;
                Phone_Number.Text = Telegram_cloud.Resources.Autorization_russian.Phone_Number_Text;
                TextBlock_Auth_Phone_Code_background.Text = Telegram_cloud.Resources.Autorization_russian.TextBlock_Auth_Phone_Code_background_Text;
                Auth_Phone_Code.Text = Telegram_cloud.Resources.Autorization_russian.Auth_Phone_Code_Text;
                TextBlock_Password_background.Text = Telegram_cloud.Resources.Autorization_russian.TextBlock_Password_background_Text;
                Password.Text = Telegram_cloud.Resources.Autorization_russian.Password_Text;
                TextBlock_Email_Address_background.Text = Telegram_cloud.Resources.Autorization_russian.TextBlock_Email_Address_background_Text;
                Email_Address.Text = Telegram_cloud.Resources.Autorization_russian.Email_Address_Text;
                TextBlock_Auth_Email_Code_background.Text = Telegram_cloud.Resources.Autorization_russian.TextBlock_Auth_Email_Code_background_Text;
                Auth_Email_Code.Text = Telegram_cloud.Resources.Autorization_russian.Auth_Email_Code_Text;
                Phone_Number_label.Content = Telegram_cloud.Resources.Autorization_russian.Phone_Number_label_Content;
                Auth_Phone_Code_label.Content = Telegram_cloud.Resources.Autorization_russian.Auth_Phone_Code_label_Content;
                Password_label.Content = Telegram_cloud.Resources.Autorization_russian.Password_label_Content;
                Email_Address_label.Content = Telegram_cloud.Resources.Autorization_russian.Email_Address_label_Content;
                Auth_Email_Code_label.Content = Telegram_cloud.Resources.Autorization_russian.Auth_Email_Code_label_Content;
            }
        }
    }
}
