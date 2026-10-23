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
    public partial class AddPaymentPage : Page
    {
        private Payments _currentPayment;

        public AddPaymentPage()
        {
            InitializeComponent();
            _currentPayment = new Payments();
            LoadComboBoxes();
            TBDate.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }

        public AddPaymentPage(Payments selectedPayment)
        {
            InitializeComponent();
            _currentPayment = selectedPayment;
            LoadComboBoxes();

            TBDate.Text = _currentPayment.PaymentDate.ToString("dd.MM.yyyy HH:mm");
            TBAmount.Text = _currentPayment.Amount.ToString();

            CBPaymentMethod.SelectedItem = CBPaymentMethod.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == _currentPayment.PaymentMethod);

            CBAppointment.SelectedValue = _currentPayment.AppointmentID;
        }

        private void LoadComboBoxes()
        {
            var db = new GroomingSalonEntities();

            var appointmentsFromDb = db.Appointments.ToList();

            var appointments = appointmentsFromDb.Select(a => new
            {
                a.AppointmentID,
                Display = db.Clients.FirstOrDefault(c => c.ClientID == a.ClientID).FullName + " (" + a.AppointmentDate + ")"
            }).ToList();

            CBAppointment.ItemsSource = appointments;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (!DateTime.TryParse(TBDate.Text, out DateTime paymentDate))
                errors.AppendLine("Укажите корректную дату!");

            if (!decimal.TryParse(TBAmount.Text, out decimal amount))
                errors.AppendLine("Укажите корректную сумму!");

            if (CBPaymentMethod.SelectedItem == null)
                errors.AppendLine("Выберите способ оплаты!");

            if (CBAppointment.SelectedValue == null)
                errors.AppendLine("Выберите запись!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var db = new GroomingSalonEntities();
            int selectedAppointmentId = (int)CBAppointment.SelectedValue;
            string method = (CBPaymentMethod.SelectedItem as ComboBoxItem).Content.ToString();

            if (_currentPayment.PaymentID == 0)
            {
                _currentPayment.AppointmentID = selectedAppointmentId;
                _currentPayment.Amount = amount;
                _currentPayment.PaymentDate = paymentDate;
                _currentPayment.PaymentMethod = method;

                db.Payments.Add(_currentPayment);
            }
            else
            {
                var paymentInDb = db.Payments.FirstOrDefault(p => p.PaymentID == _currentPayment.PaymentID);
                if (paymentInDb != null)
                {
                    paymentInDb.AppointmentID = selectedAppointmentId;
                    paymentInDb.Amount = amount;
                    paymentInDb.PaymentDate = paymentDate;
                    paymentInDb.PaymentMethod = method;
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
            TBDate.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            TBAmount.Text = "";
            CBPaymentMethod.SelectedIndex = -1;
            CBAppointment.SelectedIndex = -1;
        }
    }
}