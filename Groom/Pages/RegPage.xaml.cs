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
using BCrypt.Net;

namespace Groom.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            LoadRoles();
            comboBxRole.SelectedIndex = 2;
        }
        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxLog.Focus();
        }
        public void LoadRoles()
        {
            using (var db = new GroomingSalonEntities())
            {
                var roles = db.Roles.ToList();
                comboBxRole.ItemsSource = roles;
                comboBxRole.DisplayMemberPath = "RoleName";
                comboBxRole.SelectedValuePath = "RoleID";
            }
        }

        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtbxLog.Text) ||
                string.IsNullOrWhiteSpace(txtbxFIO.Text) ||
                string.IsNullOrWhiteSpace(passBxFrst.Password) ||
                string.IsNullOrWhiteSpace(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (passBxFrst.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool hasNumber = false;
            bool hasEnglish = false;
            bool hasRussian = false;

            foreach (char c in passBxFrst.Password)
            {
                if (char.IsDigit(c))
                {
                    hasNumber = true;
                }
                else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                {
                    hasEnglish = true;
                }
                else if ((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё')
                {
                    hasRussian = true;
                }
            }

            if (hasRussian)
            {
                MessageBox.Show("Пароль не должен содержать русские буквы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!hasEnglish)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну английскую букву!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!hasNumber)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну цифру!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new GroomingSalonEntities())
            {
                if (db.Users.Any(u => u.Login == txtbxLog.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (comboBxRole.SelectedItem is Roles selectedRole)
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(passBxFrst.Password);

                    Users newUser = new Users
                    {
                        Login = txtbxLog.Text,
                        PasswordHash = hashedPassword,
                        RoleID = selectedRole.RoleID
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    txtbxLog.Clear();
                    txtbxFIO.Clear();
                    passBxFrst.Clear();
                    passBxScnd.Clear();
                    comboBxRole.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Выберите роль из списка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void comboBxRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
