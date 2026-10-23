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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Groom.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbLogin.Text) ||
                string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return;
            }

            var db = new GroomingSalonEntities();
            var user = db.Users.FirstOrDefault(u => u.Login == TbLogin.Text);

            if (user == null)
            {
                MessageBox.Show("Пользователь с таким логином не найден!");
                return;
            }

            if (!BCrypt.Net.BCrypt.Verify(CurrentPasswordBox.Password, user.PasswordHash))
            {
                MessageBox.Show("Текущий пароль неверный!");
                return;
            }

            if (NewPasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов!");
                return;
            }

            bool hasNumber = false;
            bool hasEnglish = false;

            foreach (char c in NewPasswordBox.Password)
            {
                if (char.IsDigit(c)) hasNumber = true;
                else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) hasEnglish = true;
            }

            if (!hasEnglish)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну английскую букву!");
                return;
            }

            if (!hasNumber)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну цифру!");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Новый пароль и подтверждение не совпадают!");
                return;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPasswordBox.Password);
            db.SaveChanges();

            MessageBox.Show("Пароль успешно изменен!");
            NavigationService?.Navigate(new LoginPage());
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}