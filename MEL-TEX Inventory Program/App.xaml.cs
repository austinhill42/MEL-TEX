using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        internal static readonly string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        internal static readonly string DBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}Database\\MEL-TEXDB.mdf; Integrated Security = True; Connect Timeout = 30";
        internal static readonly string PWDDBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}Database\\MEL-TEXPasswordDB.mdf; Integrated Security = True; Connect Timeout = 30";
        internal static readonly string SalesDBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}Database\\MEL-TEXSalesDB.mdf; Integrated Security = True; Connect Timeout = 30";
        internal static readonly string PurchasingDBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}Database\\MEL-TEXPurchasingDB.mdf; Integrated Security = True; Connect Timeout = 30";
        internal static readonly string AccountingDBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}Database\\MEL-TEXAccountingDB.mdf; Integrated Security = True; Connect Timeout = 30";

        internal static void UpdateQuantityAvailable(string item)
        {
            DataTable quantities = new DataTable();

            try
            {
                string query = "SELECT Quantity FROM Inventory WHERE Inventory_Item = @item";

                using (SqlConnection sql = new SqlConnection(DBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = query;

                    com.Parameters.AddWithValue("@item", item);

                    com.ExecuteNonQuery();


                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(quantities);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }

            int quantityAvail = 0;

            foreach (DataRow row in quantities.Rows)
                foreach (object cell in row.ItemArray)
                    quantityAvail += Convert.ToInt32(cell);

            try
            {
                string query = "UPDATE Inventory SET QuantityAvail = @avail WHERE Inventory_Item = @item";

                using (SqlConnection sql = new SqlConnection(DBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = query;

                    com.Parameters.AddWithValue("@item", item);
                    com.Parameters.AddWithValue("@avail", quantityAvail);

                    com.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void TextBox_GotFocus(Object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
        }
    }
}
