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
using System.Windows.Data;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static readonly string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        private static readonly string connString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}MEL-TEXDB.mdf; Integrated Security = True; Connect Timeout = 30";

        internal static void UpdateQuantityAvailable(string item)
        {
            DataTable quantities = new DataTable();

            try
            {
                string query = "SELECT Quantity FROM Inventory WHERE Inventory_Item = @item";

                using (SqlConnection sql = new SqlConnection(connString))
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

                using (SqlConnection sql = new SqlConnection(connString))
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
    }
}
