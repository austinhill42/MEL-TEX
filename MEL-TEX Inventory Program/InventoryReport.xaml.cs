using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for InventoryInbound.xaml
    /// </summary>
    public partial class InventoryReport : Page
    {
        internal Page previousPage;

        public InventoryReport(Page page)
        {
            InitializeComponent();

            previousPage = page;
        }

        private void InventoryReportPage_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Title = "Inventory Report";
        }

        private void BTN_ClearSelected_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BTN_ClearSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BTN_CreateQuote_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BTN_OpenQuote_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BTN_OpenSalesOrder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }
    }
}
