using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for OpenPO.xaml
    /// </summary>
    public partial class OpenPO : Window
    {
        internal string poNum;

        public OpenPO()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection sql = new SqlConnection(App.PurchasingDBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT Number FROM PO";

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(table);

                    CB_Open.DataContext = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BTN_Open_Click(object sender, RoutedEventArgs e)
        {
            poNum = CB_Open.SelectedValue.ToString();
            DialogResult = true;
            Close();
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
