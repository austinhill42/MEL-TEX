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

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void Inventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedValue != null)
            {
                switch ((sender as ComboBox).SelectedValue.ToString())
                {
                    case "Inventory Report":
                        MainWindow.GetWindow(this).Content = new InventoryReport(this);
                        break;
                    default:
                        break;
                } 
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Title = "Main Window";

            CB_Inventory.SelectedIndex = -1;
            CB_Purchasing.SelectedIndex = -1;
            CB_Sales.SelectedIndex = -1;
        }
    }
}
