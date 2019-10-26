using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for Customer_VendorReport.xaml
    /// </summary>
    public partial class Customer_VendorReport : Page
    {
        private Page previousPage;
        private string conn;
        private string Type;
        private string selectedNumber;
        private string selectedName;
        private string selectedContact;

        public Customer_VendorReport(Page prev, string type)
        {
            InitializeComponent();

            previousPage = prev;
            Type = type;

            conn = Type == "Customer" ? App.SalesDBConnString : App.PurchasingDBConnString;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            WindowTitle = $"{Type} Information";
            L_Header.Content = $"{Type}s";
            BTN_AddCustomerVendor.Content = $"Add {Type}";
            BTN_EditCustomerVendor.Content = $"Edit {Type}";
            BTN_DeleteCustomerVendor.Content = $"Delete {Type}";

            PopulateDataGrid();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGrid.SelectedItem == null)
            {
                BTN_AddContact.IsEnabled = false;
                BTN_EditContact.IsEnabled = false;
                BTN_EditCustomerVendor.IsEnabled = false;
                BTN_DeleteContact.IsEnabled = false;
                BTN_DeleteCustomerVendor.IsEnabled = false;
            }
            else
            {
                BTN_AddContact.IsEnabled = true;
                BTN_EditCustomerVendor.IsEnabled = true;
                BTN_DeleteCustomerVendor.IsEnabled = true;

                selectedNumber = ((DataRowView)DataGrid.SelectedItem).Row["Number"].ToString();
                selectedName = ((DataRowView)DataGrid.SelectedItem).Row["Name"].ToString();

                PopulateContactDataGrid();
            }
        }

        private void ContactsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsDataGrid.SelectedItem == null)
            {
                BTN_EditContact.IsEnabled = false;
                BTN_DeleteContact.IsEnabled = false;
            }
            else
            {
                BTN_EditContact.IsEnabled = true;
                BTN_DeleteContact.IsEnabled = true;

                selectedContact = ((DataRowView)ContactsDataGrid.SelectedItem).Row["Name"].ToString();

                DataTable emailTable = new DataTable();
                DataTable phoneTable = new DataTable();
                DataTable faxTable = new DataTable();

                using (SqlConnection sql = new SqlConnection(conn))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();

                    com.Parameters.AddWithValue("@num", selectedNumber);
                    com.Parameters.AddWithValue("@name", selectedContact);

                    com.CommandText = $"SELECT [Email] FROM {Type}_Contact_Email WHERE [Number] = @num AND [Name] = @name ";

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(emailTable);

                    ContactEmailDataGrid.DataContext = emailTable.DefaultView;

                    com.CommandText = $"SELECT [Fax_No] AS 'Fax Number' FROM {Type}_Contact_Fax WHERE [Number] = @num AND [Name] = @name ";

                    adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(faxTable);

                    ContactFaxDataGrid.DataContext = faxTable.DefaultView;

                    com.CommandText = $"SELECT [Phone_No] AS 'Phone Number' FROM {Type}_Contact_Phones WHERE [Number] = @num AND [Name] = @name ";

                    adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(phoneTable);

                    ContactPhonesDataGrid.DataContext = phoneTable.DefaultView;
                }
            }
        }

        private void TB_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query =
                    "SELECT * " +
                    $"FROM {Type} " +
                    $"WHERE [Number] LIKE @num ";

            if (TB_Search.Text == "")
                PopulateDataGrid();
            else
                SearchTable(query, TB_Search.Text);
        }

        private void PopulateDataGrid()
        {
            DataTable table = new DataTable();

            DataGrid.DataContext = null;

            using (SqlConnection sql = new SqlConnection(conn))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();

                com.CommandText = $"SELECT * FROM {Type} ";

                SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                {
                    SelectCommand = com
                };

                adapter.Fill(table);

                DataGrid.DataContext = FixAddressFormat(table).DefaultView;
            }
        }

        private void PopulateContactDataGrid()
        {
            DataTable table = new DataTable();

            ContactsDataGrid.DataContext = null;

            using (SqlConnection sql = new SqlConnection(conn))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();

                com.CommandText = $"SELECT * FROM {Type}_Contacts WHERE Number = @num ";

                com.Parameters.AddWithValue("@num", selectedNumber);

                SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                {
                    SelectCommand = com
                };

                adapter.Fill(table);

                ContactsDataGrid.DataContext = table.DefaultView;
            }
        }

        private void SearchTable(string query, string search)
        {
            DataTable table = new DataTable();

            using (SqlConnection sql = new SqlConnection(conn))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();

                com.CommandText = query;

                com.Parameters.AddWithValue("@num", '%' + search + '%');

                SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                {
                    SelectCommand = com
                };

                adapter.Fill(table);

                DataGrid.DataContext = FixAddressFormat(table).DefaultView;
            }
        }

        private DataTable FixAddressFormat(DataTable t)
        {
            DataTable table = t;

            foreach (DataRow row in table.Rows)
            {
                if (row.Table.Columns.Contains("Bill_To"))
                    row["Bill_To"] = row["Bill_To"].ToString().Replace("||", "|").Replace('|', '\n');

                if (row.Table.Columns.Contains("Pay_To"))
                    row["Pay_To"] = row["Pay_To"].ToString().Replace("||", "|").Replace('|', '\n');

                if (row.Table.Columns.Contains("Mail_To"))
                    row["Mail_To"] = row["Mail_To"].ToString().Replace("||", "|").Replace('|', '\n');

                if (row.Table.Columns.Contains("Mail_From"))
                    row["Mail_From"] = row["Mail_From"].ToString().Replace("||", "|").Replace('|', '\n');
            }

            return table;
        }

        private void DeleteContact(string name = "1=1")
        {
            try
            {
                using (SqlConnection sql = new SqlConnection(conn))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();

                    com.Parameters.AddWithValue("@num", selectedNumber);
                    com.Parameters.AddWithValue("@name", name);

                    com.CommandText = $"DELETE FROM {Type}_Contact_Fax WHERE Number = @num AND Name = @name ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type}_Contact_Email WHERE Number = @num  AND Name = @name ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type}_Contact_Phones WHERE Number = @num  AND Name = @name ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type}_Contact_Social_Media WHERE Number = @num  AND Name = @name ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type}_Contacts WHERE Number = @num  AND Name = @name ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type}_Ship_Locations WHERE Number = @num  AND Name = @name ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type} WHERE Number = @num  AND Name = @name ";
                    com.ExecuteNonQuery();
                }

                MessageBox.Show($"{Type}: {selectedName} successfully deleted");

                PopulateDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void DeleteCustomerVendor()
        {
            DeleteContact();

            try
            {
                using (SqlConnection sql = new SqlConnection(conn))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();

                    com.Parameters.AddWithValue("@num", selectedNumber);

                    com.CommandText = $"DELETE FROM {Type}_Ship_Locations WHERE Number = @num ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type} WHERE Number = @num ";
                    com.ExecuteNonQuery();
                }

                MessageBox.Show($"{Type}: {selectedName} successfully deleted");

                PopulateDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void BTN_ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TB_Search.Text = "";
        }

        private void BTN_AddCustomerVendor_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = new AddCustomer_Vendor(this, Type, false);
        }

        private void BTN_EditCustomerVendor_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = new AddCustomer_Vendor(this, Type, true, selectedNumber);
        }

        private void BTN_DeleteCustomerVendor_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete {Type}: {selectedName}?\n\nThis will delete all records relating to {selectedName}, including contacts", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            DeleteCustomerVendor();
        }

        private void BTN_AddContact_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = new AddContact(this, Type, selectedNumber);
        }

        private void BTN_EditContact_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BTN_DeleteContact_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete {Type} Contact: {selectedContact}?\n\nThis will delete all records relating to {selectedContact}", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            DeleteContact(selectedContact);
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }
    }
}