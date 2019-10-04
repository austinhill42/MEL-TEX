using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for AddCustomer.xaml
    /// </summary>
    public partial class AddCustomer_Vendor : Page
    {
        private Page previousPage;
        public ObservableCollection<Address> ShipToAddresses { get; set; }
        private string Type;
        

        public AddCustomer_Vendor(Page prev, string type)
        {
            InitializeComponent();

            previousPage = prev;
            Type = type;
            ShipToAddresses = new ObservableCollection<Address>();

            Address a;

            if (Type == "Customer")
            {
                ((Address)Grid.FindName("BillTo")).AddressType = "Bill To: ";
                BTN_AddShipTo.Content = "Add Ship To";
                BTN_RemoveShipTo.Content = "Remove Ship To";
            }
            else
            {
                ((Address)Grid.FindName("BillTo")).AddressType = "Pay To: ";
                BTN_AddShipTo.Content = "Add Ship From";
                BTN_RemoveShipTo.Content = "Remove Ship From";
            }

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = $"Add {Type}";

            L_Number.Content = $"{Type} No:";
            L_Name.Content = $"{Type} Name:";

            Address a;

            if (Type == "Customer")
                a = new Address() { AddressType = "Ship To:" };
            else
                a = new Address() { AddressType = "Ship From:" };

            AddShipTo(a);

            
        }

        private void Clear()
        {
            foreach (TextBox tb in FindVisualChildren<TextBox>(Grid))
                tb.Text = "";
        }

        private void AddShipTo(Address address)
        {
            ShipToAddresses.Add(address);

            RowDefinition rowDef = new RowDefinition
            {
                Height = new GridLength(250)
            };

            Grid.RowDefinitions.Add(rowDef);

            Grid.SetRow(G_Notes, Grid.GetRow(G_Notes) + 1);
            
            Grid.Children.Add(address);
            Grid.SetRow(address, Grid.GetRow(G_Notes) - 1);
            Grid.SetRow(BTN_AddShipTo, Grid.GetRow(G_Notes) - 1);
            Grid.SetRow(BTN_RemoveShipTo, Grid.GetRow(G_Notes) - 1);
            Grid.SetColumn(address, 1);
            Grid.SetColumnSpan(address, 2);
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            string num = TB_Number.Text;
            string name = TB_Name.Text;
            string website = TB_Website.Text;
            string terms = TB_Terms.Text;
            string billto = $"{BillTo.TB_Address1.Text}|{BillTo.TB_Address2.Text}|{BillTo.TB_City.Text}|{BillTo.TB_State.Text}|{BillTo.TB_Zip.Text}|";
            string mailto = $"{MailTo.TB_Address1.Text}|{MailTo.TB_Address2.Text}|{MailTo.TB_City.Text}|{MailTo.TB_State.Text}|{MailTo.TB_Zip.Text}|";
            string notes = TB_Notes.Text;
            List<string> shipto = new List<string>();

            foreach (Address a in ShipToAddresses)
                shipto.Add($"{a.TB_Address1.Text}|{a.TB_Address2.Text}|{a.TB_City.Text}|{a.TB_State.Text}|{a.TB_Zip.Text}|");

            try
            {
                string conn = "";
                string query;

                if (Type == "Customer")
                    query = $"INSERT INTO {Type} ([Number], [Name], [Website], [Bill_To], [Mail_To], [Terms], [Notes]) " +
                               "VALUES (@num,@name,@website,@bill,@mail,@terms,@notes) ";
                else
                    query = $"INSERT INTO {Type} ([Number], [Name], [Website], [Pay_To], [Mail_From], [Terms], [Notes]) " +
                                   "VALUES (@num,@name,@website,@bill,@mail,@terms,@notes) ";


                if (Type == "Customer")
                    conn = App.SalesDBConnString;
                else if (Type == "Vendor")
                    conn = App.PurchasingDBConnString;

                using (SqlConnection sql = new SqlConnection(conn))
                {
                    sql.Open();

                    using (SqlCommand cmd = new SqlCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("@num", num);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@website", website);
                        cmd.Parameters.AddWithValue("@bill", billto);
                        cmd.Parameters.AddWithValue("@mail", mailto);
                        cmd.Parameters.AddWithValue("@terms", terms);
                        cmd.Parameters.AddWithValue("@notes", notes);

                        cmd.ExecuteNonQuery();
                    }

                    for (int i = 0; i < shipto.Count; i++)
                    {
                        if (Type == "Customer")
                            query = $"INSERT INTO {Type}_Ship_Locations ([Number], [Ship_To]) " +
                                $"VALUES (@num,@{i}) ";
                        else
                            query = $"INSERT INTO {Type}_Ship_Locations ([Number], [Ship_From]) " +
                                $"VALUES (@num,@{i}) ";


                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(query, sql))
                            {
                                cmd.Parameters.AddWithValue("@num", num);
                                cmd.Parameters.AddWithValue($"@{i}", shipto[i]);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show($"Couldn't enter address:\n\n\t{shipto[i].Replace("|", "\n\t")}");
                        }
                    }

                    
                }

                MessageBox.Show($"{Type} {TB_Name.Text} added to database");

                Clear();
            }
            catch (Exception ex)
            {
                if (ex is System.FormatException)
                    MessageBox.Show("One of the fields was entered with an incorrect format...\n\nOr a required value was left black.\n\nCheck your values and try again.");
                else if (ex is SqlException)
                {
                    if (ex.Message.Contains("duplicate"))
                        MessageBox.Show("The item already exists in the database.");

                    else
                        MessageBox.Show(ex.Message);
                }
                else
                    MessageBox.Show(ex.Message);
            }
        }

        private void BTN_AddShipTo_Click(object sender, RoutedEventArgs e)
        {
            Address a;

            if (Type == "Customer")
                a = new Address() { AddressType = "Ship To:" };
            else
                a = new Address() { AddressType = "Ship From:" };

            AddShipTo(a);

            ScrollView.ScrollToBottom();
        }

        private void BTN_RemoveShipTo_Click(object sender, RoutedEventArgs e)
        {
            Grid.Children.Remove(ShipToAddresses[ShipToAddresses.Count - 1]);
            Grid.RowDefinitions.RemoveAt(Grid.GetRow(G_Notes) - 1);
            ShipToAddresses.RemoveAt(ShipToAddresses.Count - 1);

            Grid.SetRow(BTN_AddShipTo, Grid.GetRow(BTN_AddShipTo) - 1);
            Grid.SetRow(BTN_RemoveShipTo, Grid.GetRow(BTN_RemoveShipTo) - 1);
            Grid.SetRow(G_Notes, Grid.GetRow(G_Notes) - 1);
        }
        
        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            TB_Notes.Text += $"{date} -- {time} -- {TB_AddNote.Text}\n";

            TB_AddNote.Text = "";
        }

        // Get all the textboxes so I can clear them, since some are deeply nested
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
