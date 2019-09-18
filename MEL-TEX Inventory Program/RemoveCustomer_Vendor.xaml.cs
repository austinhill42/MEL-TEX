using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for RemoveCustomer_Vendor.xaml
    /// </summary>
    public partial class RemoveCustomer_Vendor : Page
    {
        private Page PreviousPage;
        private string Type;
        private string conn = "";
        private string number = "";
        private string name = "";

        public RemoveCustomer_Vendor(Page prev, string type)
        {
            InitializeComponent();

            PreviousPage = prev;
            Type = type;

            if (Type == "Customer")
                conn = App.SalesDBConnString;
            else if (Type == "Vendor")
                conn = App.PurchasingDBConnString;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = $"Remove {Type}";
            L_Name.Content = $"{Type} Name:";

            PopulateNameComboBox();
        }

        private void PopulateNameComboBox()
        {
            DataTable table = new DataTable();
            try
            {
                using (SqlConnection sql = new SqlConnection(conn))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = $"SELECT Number, Name FROM {Type}";

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(table);

                    CB_Name.DataContext = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = PreviousPage;
        }

        private void BTN_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (CB_Name.SelectedIndex < 0)
            {
                MessageBox.Show($"You must select a {Type} to delete first.");
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete {Type}: {CB_Name.Text}?\n\nThis will delete all records relating to {CB_Name.Text}, including contacts", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
           
            try
            {
                using (SqlConnection sql = new SqlConnection(conn))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();

                    com.Parameters.AddWithValue("@num", CB_Name.SelectedValue.ToString());

                    com.CommandText = $"DELETE FROM {Type}_Contact_Fax WHERE Number = @num ";
                    com.ExecuteNonQuery();


                    com.CommandText = $"DELETE FROM {Type}_Contact_Email WHERE Number = @num ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type}_Contact_Phones WHERE Number = @num ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type}_Contact_Social_Media WHERE Number = @num ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type}_Contacts WHERE Number = @num ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type}_Ship_Locations WHERE Number = @num ";
                    com.ExecuteNonQuery();

                    com.CommandText = $"DELETE FROM {Type} WHERE Number = @num ";
                    com.ExecuteNonQuery();
                }

                MessageBox.Show($"{Type}: {CB_Name.Text} successfully deleted");

                PopulateNameComboBox();
                //CB_Name.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }
    }
}
