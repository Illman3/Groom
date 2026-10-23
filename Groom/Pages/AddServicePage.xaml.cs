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
    public partial class AddServicePage : Page
    {
        private Services _currentService;

        public AddServicePage()
        {
            InitializeComponent();
            _currentService = new Services();
        }

        public AddServicePage(Services selectedService)
        {
            InitializeComponent();
            _currentService = selectedService;

            TBServiceName.Text = _currentService.ServiceName;
            TBPrice.Text = _currentService.Price.ToString();
            TBDuration.Text = _currentService.DurationMinutes.ToString();
            TBDescription.Text = _currentService.Description;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TBServiceName.Text))
                errors.AppendLine("Укажите название услуги!");
            decimal price = 0;
            int duration = 0;
            if (string.IsNullOrWhiteSpace(TBPrice.Text) || !decimal.TryParse(TBPrice.Text, out price))
                errors.AppendLine("Укажите корректную цену!");
            if (string.IsNullOrWhiteSpace(TBDuration.Text) || !int.TryParse(TBDuration.Text, out duration))
                errors.AppendLine("Укажите корректную длительность!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var db = new GroomingSalonEntities();

            if (_currentService.ServiceID == 0)
            {
                _currentService.ServiceName = TBServiceName.Text;
                _currentService.Price = price;
                _currentService.DurationMinutes = duration;
                _currentService.Description = TBDescription.Text;

                db.Services.Add(_currentService);
            }
            else
            {
                var serviceInDb = db.Services.FirstOrDefault(s => s.ServiceID == _currentService.ServiceID);
                if (serviceInDb != null)
                {
                    serviceInDb.ServiceName = TBServiceName.Text;
                    serviceInDb.Price = price;
                    serviceInDb.DurationMinutes = duration;
                    serviceInDb.Description = TBDescription.Text;
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
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBServiceName.Text = "";
            TBPrice.Text = "";
            TBDuration.Text = "";
            TBDescription.Text = "";
        }
    }
}