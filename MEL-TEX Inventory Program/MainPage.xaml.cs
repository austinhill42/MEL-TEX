using System.Windows;
using System.Windows.Controls;

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

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedValue != null)
            {
                switch ((sender as ComboBox).SelectedValue.ToString())
                {
                    case "Inventory Report":
                        MainWindow.GetWindow(this).Content = new InventoryReport(this);
                        break;
                    case "Add Inventory Item":
                        MainWindow.GetWindow(this).Content = new AddItem(this);
                        break;
                    case "Inventory Inbound":
                        MainWindow.GetWindow(this).Content = new InventoryInbound(this);
                        break;
                    case "Edit Inventory Item":
                        MainWindow.GetWindow(this).Content = new AddItem(this, true);
                        break;
                    case "Edit Inbounded Item":
                        MainWindow.GetWindow(this).Content = new InventoryInbound(this, true);
                        break;
                    case "Remove Inventory Item":
                        MainWindow.GetWindow(this).Content = new DeleteItem(this);
                        break;
                    case "Remove Inbounded Item":
                        MainWindow.GetWindow(this).Content = new DeleteInboundedItem(this);
                        break;
                    case "Customer Information":
                        break;
                    case "Add Customer":
                        MainWindow.GetWindow(this).Content = new AddCustomer_Vendor(this, "Customer");
                        break;
                    case "Edit Customer":
                        break;
                    case "Remove Customer":
                        break;
                    case "Vendor Information":
                        break;
                    case "Add Vendor":
                        MainWindow.GetWindow(this).Content = new AddCustomer_Vendor(this, "Vendor");
                        break;
                    case "Edit Vendor":
                        break;
                    case "Remove Vendor":
                        break;
                    case "Edit Groups":
                        LoginWrapper(new EditGroups(this));
                        break;
                    case "Edit Password":
                        LoginWrapper(new EditPassword(this));
                        break;
                    default:
                        break;
                } 
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Main Window";
        }

        private void LoginWrapper(Page page)
        {
            LoginForm login = new LoginForm();

            login.ShowDialog();

            if (login.correctLogin)
            {
                MainWindow.GetWindow(this).Content = page;
            }
            else
            {

                CB_Settings.SelectedIndex = -1;

                if (login.attempted)
                    MessageBox.Show("Incorrect Login");
            }
        }

        private void Main_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CB_Inventory.SelectedIndex = -1;
            CB_Purchasing.SelectedIndex = -1;
            CB_Sales.SelectedIndex = -1;
            CB_Settings.SelectedIndex = -1;
        }
    }
}
