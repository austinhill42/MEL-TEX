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

                    case "Open Quote":
                        OpenQuote openquote = new OpenQuote();

                        if (openquote.ShowDialog() ?? false)
                            MainWindow.GetWindow(this).Content = new GenerateQuote(this, openquote.quotenum);
                        break;

                    case "Customer Information":
                        MainWindow.GetWindow(this).Content = new Customer_VendorReport(this, "Customer");
                        break;

                    case "Add Customer":
                        MainWindow.GetWindow(this).Content = new AddCustomer_Vendor(this, "Customer", false);
                        break;

                    case "Add Customer Contact":
                        //MainWindow.GetWindow(this).Content = new AddContact(this, "Customer");
                        break;

                    case "Edit Customer":
                        break;

                    case "Edit Customer Contact":
                        break;

                    case "Remove Customer":
                        MainWindow.GetWindow(this).Content = new RemoveCustomer_Vendor(this, "Customer");
                        break;

                    case "Remove Customer Contact":
                        break;

                    case "Generate PO":
                        MainWindow.GetWindow(this).Content = new GeneratePO(this);
                        break;

                    case "Receive PO":
                        OpenPO open = new OpenPO();

                        if (open.ShowDialog() ?? false)
                            MainWindow.GetWindow(this).Content = new ReceivePO(this, open.poNum);
                        else
                            Main_IsVisibleChanged(null, new DependencyPropertyChangedEventArgs());
                        break;

                    case "Vendor Information":
                        MainWindow.GetWindow(this).Content = new Customer_VendorReport(this, "Vendor");
                        break;

                    case "Add Vendor":
                        MainWindow.GetWindow(this).Content = new AddCustomer_Vendor(this, "Vendor", false);
                        break;

                    case "Add Vendor Contact":
                        //MainWindow.GetWindow(this).Content = new AddContact(this, "Vendor");
                        break;

                    case "Edit Vendor":
                        break;

                    case "Edit Vendor Contact":
                        break;

                    case "Remove Vendor":
                        MainWindow.GetWindow(this).Content = new RemoveCustomer_Vendor(this, "Vendor");
                        break;

                    case "Remove Vendor Contact":
                        break;

                    case "Edit Groups":
                        LoginWrapper(new EditGroups(this));
                        break;

                    case "Edit Password":
                        LoginWrapper(new EditPassword(this));
                        break;

                    case "Clear Data":
                        LoginWrapper(new ClearData());
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
                Main_IsVisibleChanged(null, new DependencyPropertyChangedEventArgs());

                if (login.attempted)
                    MessageBox.Show("Incorrect Login");
            }
        }

        private void LoginWrapper(Window window)
        {
            LoginForm login = new LoginForm();

            login.ShowDialog();

            if (login.correctLogin)
            {
                window.Show();
            }
            else
            {
                if (login.attempted)
                    MessageBox.Show("Incorrect Login");
            }

            Main_IsVisibleChanged(null, new DependencyPropertyChangedEventArgs());
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