using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        internal static readonly string AccountingDBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}Database\\MEL-TEXAccountingDB.mdf; Integrated Security = True; Connect Timeout = 30";
        internal static readonly string DBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = |DataDirectory|Database\\MEL-TEXDB.mdf; Integrated Security = True; Connect Timeout = 30";
        internal static readonly string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        internal static readonly string PurchasingDBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}Database\\MEL-TEXPurchasingDB.mdf; Integrated Security = True; Connect Timeout = 30";
        internal static readonly string PWDDBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}Database\\MEL-TEXPasswordDB.mdf; Integrated Security = True; Connect Timeout = 30";
        internal static readonly string SalesDBConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}Database\\MEL-TEXSalesDB.mdf; Integrated Security = True; Connect Timeout = 30";

        #endregion Fields

        #region Methods

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

        private void TextBox_GotFocus(Object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
        }

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
                string query = "UPDATE Items SET QuantityAvail = @avail WHERE Inventory_Item = @item";

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

        #endregion Methods
    }
}