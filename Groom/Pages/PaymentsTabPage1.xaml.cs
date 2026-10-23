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
    public partial class PaymentsTabPage1 : Page
    {
        public PaymentsTabPage1()
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
            DataGridPayment.ItemsSource = db.Payments.ToList();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPaymentPage());
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var paymentsForRemoving = DataGridPayment.SelectedItems.Cast<Payments>().ToList();

            if (paymentsForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите записи для удаления!");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {paymentsForRemoving.Count} элементов?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var db = new GroomingSalonEntities();
                    foreach (var payment in paymentsForRemoving)
                    {
                        db.Payments.Attach(payment);
                        db.Payments.Remove(payment);
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
            NavigationService?.Navigate(new AddPaymentPage((sender as Button).DataContext as Payments));
        }
    }
}