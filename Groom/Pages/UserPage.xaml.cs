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
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            UpdateUsers();
        }

        private void clearFiltersButton_Click_1(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
            UpdateUsers();
        }

        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void UpdateUsers()
        {
            try
            {
                using (var db = new GroomingSalonEntities())
                {
                    var currentUsers = db.Users.ToList();

                    if (fioFilterTextBox != null && !string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                    {
                        currentUsers = currentUsers.Where(x => x.Login.ToLower().Contains(fioFilterTextBox.Text.ToLower())).ToList();
                    }

                    if (onlyAdminCheckBox != null && onlyAdminCheckBox.IsChecked == true)
                    {
                        currentUsers = currentUsers.Where(x => x.RoleID == 1).ToList();
                    }

                    if (sortComboBox != null && ListUser != null)
                    {
                        if (sortComboBox.SelectedIndex == 0)
                        {
                            ListUser.ItemsSource = currentUsers.OrderBy(x => x.Login).ToList();
                        }
                        else
                        {
                            ListUser.ItemsSource = currentUsers.OrderByDescending(x => x.Login).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}