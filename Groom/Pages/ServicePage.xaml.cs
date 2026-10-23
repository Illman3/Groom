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
    public partial class ServiceTabPage : Page
    {
        public ServiceTabPage()
        {
            InitializeComponent();
            UpdateData();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                UpdateData();
            }
        }

        private void UpdateData()
        {
            var db = new GroomingSalonEntities();
            DataGridServices.ItemsSource = db.Services.ToList();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddServicePage());
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var servicesForRemoving = DataGridServices.SelectedItems.Cast<Services>().ToList();

            if (servicesForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите записи для удаления!");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {servicesForRemoving.Count} элементов?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var db = new GroomingSalonEntities();
                    foreach (var service in servicesForRemoving)
                    {
                        db.Services.Attach(service);
                        db.Services.Remove(service);
                    }
                    db.SaveChanges();
                    MessageBox.Show("Данные успешно удалены!");
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddServicePage((sender as Button).DataContext as Services));
        }
    }
}