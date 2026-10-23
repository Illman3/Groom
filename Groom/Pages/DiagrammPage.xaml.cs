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
using System.Windows.Forms.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace Groom.Pages
{
    public partial class DiagrammPage : Page
    {
        private GroomingSalonEntities _context = new GroomingSalonEntities();

        public DiagrammPage()
        {
            InitializeComponent();

            ChartPayments.ChartAreas.Add(new ChartArea("Main"));

            var currentSeries = new Series("Сумма платежей")
            {
                IsValueShownAsLabel = true,
                Legend = "Legend1"
            };
            ChartPayments.Series.Add(currentSeries);

            CmbUser.ItemsSource = _context.Clients.ToList();
            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));

            if (CmbUser.Items.Count > 0) CmbUser.SelectedIndex = 0;
            if (CmbDiagram.Items.Count > 0) CmbDiagram.SelectedIndex = 0;
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is Clients selectedClient &&
                CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series["Сумма платежей"];
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var groupedData = (from p in _context.Payments
                                   join a in _context.Appointments on p.AppointmentID equals a.AppointmentID
                                   join aps in _context.AppointmentServices on a.AppointmentID equals aps.AppointmentID
                                   join s in _context.Services on aps.ServiceID equals s.ServiceID
                                   where a.ClientID == selectedClient.ClientID
                                   group p by s.ServiceName into g
                                   select new
                                   {
                                       ServiceName = g.Key,
                                       TotalAmount = g.Sum(payment => payment.Amount)
                                   }).OrderBy(x => x.TotalAmount).ToList();

                foreach (var item in groupedData)
                {
                    currentSeries.Points.AddXY(item.ServiceName, item.TotalAmount);
                }
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allClients = _context.Clients.OrderBy(c => c.FullName).ToList();
                var application = new Excel.Application();
                application.SheetsInNewWorkbook = allClients.Count + 1;
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

                decimal grandTotal = 0;

                for (int i = 0; i < allClients.Count; i++)
                {
                    var client = allClients[i];
                    int startRowIndex = 1;

                    string sheetName = client.FullName.Length > 31 ? client.FullName.Substring(0, 31) : client.FullName;
                    Excel.Worksheet worksheet = workbook.Worksheets.Item[i + 1] as Excel.Worksheet;
                    worksheet.Name = sheetName;

                    worksheet.Cells[startRowIndex, 1] = "Дата платежа";
                    worksheet.Cells[startRowIndex, 2] = "Услуга";
                    worksheet.Cells[startRowIndex, 3] = "Сумма";

                    Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 3]];
                    columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    columnHeaderRange.Font.Bold = true;
                    startRowIndex++;

                    var allClientPayments = (from p in _context.Payments
                                             join a in _context.Appointments on p.AppointmentID equals a.AppointmentID
                                             join aps in _context.AppointmentServices on a.AppointmentID equals aps.AppointmentID
                                             join s in _context.Services on aps.ServiceID equals s.ServiceID
                                             where a.ClientID == client.ClientID
                                             orderby p.PaymentDate
                                             select new
                                             {
                                                 ServiceName = s.ServiceName,
                                                 Payment = p
                                             }).ToList();

                    var clientPaymentsGrouped = allClientPayments
                        .GroupBy(x => x.ServiceName)
                        .Select(g => new
                        {
                            ServiceName = g.Key,
                            Payments = g.Select(x => x.Payment).ToList()
                        }).ToList();

                    foreach (var group in clientPaymentsGrouped)
                    {
                        Excel.Range headerRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 3]];
                        headerRange.Merge();
                        headerRange.Value = group.ServiceName;
                        headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        headerRange.Font.Italic = true;
                        startRowIndex++;

                        int groupStartRow = startRowIndex;

                        foreach (var payment in group.Payments)
                        {
                            worksheet.Cells[startRowIndex, 1] = payment.PaymentDate.ToString("dd.MM.yyyy");
                            worksheet.Cells[startRowIndex, 2] = group.ServiceName;
                            worksheet.Cells[startRowIndex, 3] = payment.Amount;
                            (worksheet.Cells[startRowIndex, 3] as Excel.Range).NumberFormat = "0.00";
                            startRowIndex++;
                        }

                        Excel.Range sumRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 2]];
                        sumRange.Merge();
                        sumRange.Value = "ИТОГО:";
                        sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                        int groupEndRow = startRowIndex - 1;
                        worksheet.Cells[startRowIndex, 3].Formula = $"=SUM(C{groupStartRow}:C{groupEndRow})";
                        (worksheet.Cells[startRowIndex, 3] as Excel.Range).NumberFormat = "0.00";

                        sumRange.Font.Bold = true;
                        (worksheet.Cells[startRowIndex, 3] as Excel.Range).Font.Bold = true;
                        startRowIndex++;

                        grandTotal += group.Payments.Sum(p => p.Amount);
                    }

                    if (startRowIndex > 2)
                    {
                        Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[startRowIndex - 1, 3]];
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                        rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = Excel.XlLineStyle.xlContinuous;
                        rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;
                    }

                    worksheet.Columns.AutoFit();
                }

                Excel.Worksheet summarySheet = workbook.Worksheets.Item[allClients.Count + 1] as Excel.Worksheet;
                summarySheet.Name = "Общий итог";

                summarySheet.Cells[1, 1] = "Общая сумма всех платежей по салону:";
                summarySheet.Cells[1, 2] = grandTotal;
                (summarySheet.Cells[1, 2] as Excel.Range).NumberFormat = "0.00";

                Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
                summaryRange.Font.Bold = true;
                summarySheet.Columns.AutoFit();

                application.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта в Excel: " + ex.Message);
            }
        }

        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allClients = _context.Clients.ToList();
                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                foreach (var client in allClients)
                {
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = client.FullName;
                    userParagraph.set_Style("Заголовок 1"); 
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();
                    document.Paragraphs.Add();

                    var clientPayments = (from p in _context.Payments
                                          join a in _context.Appointments on p.AppointmentID equals a.AppointmentID
                                          join aps in _context.AppointmentServices on a.AppointmentID equals aps.AppointmentID
                                          join s in _context.Services on aps.ServiceID equals s.ServiceID
                                          where a.ClientID == client.ClientID
                                          group p by s.ServiceName into g
                                          select new
                                          {
                                              ServiceName = g.Key,
                                              TotalAmount = g.Sum(payment => payment.Amount)
                                          }).ToList();

                    if (clientPayments.Count > 0)
                    {
                        Word.Paragraph tableParagraph = document.Paragraphs.Add();
                        Word.Range tableRange = tableParagraph.Range;
                        Word.Table paymentsTable = document.Tables.Add(tableRange, clientPayments.Count + 2, 2);

                        paymentsTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                        paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                        paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                        paymentsTable.Cell(1, 1).Range.Text = "Услуга";
                        paymentsTable.Cell(1, 2).Range.Text = "Сумма расходов";
                        paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                        paymentsTable.Rows[1].Range.Font.Size = 14;
                        paymentsTable.Rows[1].Range.Font.Bold = 1;
                        paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        for (int i = 0; i < clientPayments.Count; i++)
                        {
                            var currentService = clientPayments[i];
                            paymentsTable.Cell(i + 2, 1).Range.Text = currentService.ServiceName;
                            paymentsTable.Cell(i + 2, 1).Range.Font.Name = "Times New Roman";
                            paymentsTable.Cell(i + 2, 1).Range.Font.Size = 12;

                            paymentsTable.Cell(i + 2, 2).Range.Text = currentService.TotalAmount.ToString("N2") + " руб.";
                            paymentsTable.Cell(i + 2, 2).Range.Font.Name = "Times New Roman";
                            paymentsTable.Cell(i + 2, 2).Range.Font.Size = 12;
                        }

                        decimal clientTotal = clientPayments.Sum(x => x.TotalAmount);
                        paymentsTable.Cell(clientPayments.Count + 2, 1).Range.Text = "ИТОГО:";
                        paymentsTable.Cell(clientPayments.Count + 2, 1).Range.Font.Bold = 1;
                        paymentsTable.Cell(clientPayments.Count + 2, 2).Range.Text = clientTotal.ToString("N2") + " руб.";
                        paymentsTable.Cell(clientPayments.Count + 2, 2).Range.Font.Bold = 1;

                        document.Paragraphs.Add();

                        var allClientPaymentsList = (from p in _context.Payments
                                                     join a in _context.Appointments on p.AppointmentID equals a.AppointmentID
                                                     join aps in _context.AppointmentServices on a.AppointmentID equals aps.AppointmentID
                                                     join s in _context.Services on aps.ServiceID equals s.ServiceID
                                                     where a.ClientID == client.ClientID
                                                     select new { ServiceName = s.ServiceName, Amount = p.Amount, Date = p.PaymentDate }).ToList();

                        if (allClientPaymentsList.Any())
                        {
                            var maxPayment = allClientPaymentsList.OrderByDescending(p => p.Amount).First();
                            Word.Paragraph maxParagraph = document.Paragraphs.Add();
                            maxParagraph.Range.Text = $"Самый дорогостоящий платеж - {maxPayment.ServiceName} за {maxPayment.Amount:N2} руб. от {maxPayment.Date:dd.MM.yyyy}";
                            maxParagraph.set_Style("Подзаголовок");
                            maxParagraph.Range.Font.Color = Word.WdColor.wdColorDarkRed;
                            maxParagraph.Range.InsertParagraphAfter();

                            var minPayment = allClientPaymentsList.OrderBy(p => p.Amount).First();
                            Word.Paragraph minParagraph = document.Paragraphs.Add();
                            minParagraph.Range.Text = $"Самый дешевый платеж - {minPayment.ServiceName} за {minPayment.Amount:N2} руб. от {minPayment.Date:dd.MM.yyyy}";
                            minParagraph.set_Style("Подзаголовок");
                            minParagraph.Range.Font.Color = Word.WdColor.wdColorDarkGreen;
                            minParagraph.Range.InsertParagraphAfter();
                        }

                        document.Paragraphs.Add();

                        if (client != allClients.LastOrDefault())
                        {
                            object breakPage = Word.WdBreakType.wdPageBreak;
                            document.Words.Last.InsertBreak(ref breakPage);
                        }
                    }
                }

                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter, true);

                    Word.HeaderFooter header = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    header.Range.Text = DateTime.Now.ToString("dd/MM/yyyy");
                    header.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    header.Range.Font.Size = 10;
                }

                application.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта в Word: " + ex.Message);
            }
        }
    }
}