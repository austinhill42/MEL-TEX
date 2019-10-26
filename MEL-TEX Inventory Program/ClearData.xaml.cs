using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for ClearData.xaml
    /// </summary>
    public partial class ClearData : Window
    {
        #region Fields

        private string main = App.DBConnString;
        private string purchasing = App.PurchasingDBConnString;
        private string sales = App.SalesDBConnString;

        #endregion Fields

        #region Constructors

        public ClearData()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BTN_Delete_Click(object sender, RoutedEventArgs e)
        {
            string query;
            SqlCommand cmd;

            try
            {
                using (SqlConnection sql = new SqlConnection(main))
                {
                    sql.Open();

                    if (CB_Inbound.IsChecked ?? false)
                    {
                        query = "DELETE FROM Inventory";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlConnection sql = new SqlConnection(sales))
                {
                    sql.Open();

                    if (CB_Customers.IsChecked ?? false)
                    {
                        query = "DELETE FROM Customer_Contact_Email";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Customer_Contact_Social_Media";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Customer_Contact_Phones";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Customer_Contacts";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Customer_Ship_Locations";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Customer";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();
                    }

                    if (CB_Quotes.IsChecked ?? false)
                    {
                        query = "DELETE FROM Quotes";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();
                    }

                    if (CB_SalesOrders.IsChecked ?? false)
                    {
                        query = "DELETE FROM SalesOrders";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();
                    }

                    //if (CB_WorkOrders.IsChecked ?? false)
                    //{
                    //    query = "DELETE FROM Inventory_Inbound";
                    //    cmd = new SqlCommand(query, sql);
                    //    cmd.ExecuteNonQuery();

                    //}
                }

                using (SqlConnection sql = new SqlConnection(purchasing))
                {
                    sql.Open();

                    if (CB_Vendors.IsChecked ?? false)
                    {
                        query = "DELETE FROM Vendor_Contact_Email";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Vendor_Contact_Social_Media";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Vendor_Contact_Phones";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Vendor_Contacts";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Vendor_Ship_Locations";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();

                        query = "DELETE FROM Vendor";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();
                    }

                    if (CB_POs.IsChecked ?? false)
                    {
                        query = "DELETE FROM PO";
                        cmd = new SqlCommand(query, sql);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Selected data cleared.");

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong.\n\n{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void CB_All_Checked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox cb in FindVisualChildren<CheckBox>(grid))
                if (cb.Content.ToString() != "Select All")
                    cb.IsChecked = true;
        }

        private void CB_All_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox cb in FindVisualChildren<CheckBox>(grid))
                if (cb.Content.ToString() != "Select All")
                    cb.IsChecked = false;
        }

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

        #endregion Methods
    }
}