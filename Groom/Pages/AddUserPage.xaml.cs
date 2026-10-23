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
    public partial class AddUserPage : Page
    {
        private Users _currentUser;

        public AddUserPage()
        {
            InitializeComponent();
            _currentUser = new Users();
        }

        public AddUserPage(Users selectedUser)
        {
            InitializeComponent();
            _currentUser = selectedUser;

            TBLogin.Text = _currentUser.Login;
            using (var db = new GroomingSalonEntities())
            {
                var client = db.Clients.FirstOrDefault(c => c.UserID == _currentUser.UserID);
            }

            if (_currentUser.RoleID == 1) cmbRole.SelectedIndex = 0;
            else cmbRole.SelectedIndex = 1;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(TBLogin.Text))
                errors.AppendLine("Укажите логин!");
            if (string.IsNullOrWhiteSpace(TBPass.Text))
                errors.AppendLine("Укажите пароль!");
            if (cmbRole.SelectedIndex == -1)
                errors.AppendLine("Выберите роль!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            var db = new GroomingSalonEntities();

            if (_currentUser.UserID == 0)
            {
                _currentUser.Login = TBLogin.Text;
                _currentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TBPass.Text);
                _currentUser.RoleID = cmbRole.SelectedIndex + 1;

                db.Users.Add(_currentUser);
                db.SaveChanges();

                db.Clients.Add(new Clients
                {
                    UserID = _currentUser.UserID,
                    Phone = "+70000000000"
                });
            }
            else
            {
                var userInDb = db.Users.FirstOrDefault(u => u.UserID == _currentUser.UserID);
                if (userInDb != null)
                {
                    userInDb.Login = TBLogin.Text;
                    userInDb.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TBPass.Text);
                    userInDb.RoleID = cmbRole.SelectedIndex + 1;

                    var client = db.Clients.FirstOrDefault(c => c.UserID == userInDb.UserID);
                }
            }

            try
            {
                db.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Text = "";
            TBPass.Text = "";
            cmbRole.SelectedIndex = -1;
            TBPhoto.Text = "";
        }

        private void TBFio_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}