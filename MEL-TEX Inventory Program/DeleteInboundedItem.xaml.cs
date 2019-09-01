using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for DeleteInboundedItem.xaml
    /// </summary>
    public partial class DeleteInboundedItem : Page
    {
        private Page previousPage;

        public DeleteInboundedItem(Page prev)
        {
            InitializeComponent();

            previousPage = prev;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Delete Inbounded Item";

            string query = "SELECT DISTINCT Inventory_Item FROM Inventory ";

            DataTable table = new DataTable();

            try
            {
                using (SqlConnection sql = new SqlConnection(App.DBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = query;

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(table);

                    CB_ItemID.DataContext = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void CB_ItemID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_Barcode.Visibility == Visibility.Visible)
            {
                ClearComboBoxVisibility(CB_Barcode, CB_Quantity, CB_PO);
                ClearLabelVisibility(L_Barcode, L_Quantity, L_PO);
            }

            if (CB_ItemID.SelectedIndex >= 0)
            {
                L_Barcode.Visibility = Visibility.Visible;
                CB_Barcode.Visibility = Visibility.Visible;
            }
        }

        private void CB_Barcode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_Quantity.Visibility == Visibility.Visible)
            {
                ClearComboBoxVisibility(CB_Quantity, CB_PO);
                ClearLabelVisibility(L_Quantity, L_PO);
            }

            if (CB_Barcode.SelectedIndex >= 0)
            {
                L_Quantity.Visibility = Visibility.Visible;
                CB_Quantity.Visibility = Visibility.Visible;
            }
        }

        private void CB_Quantity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_PO.Visibility == Visibility.Visible)
            {
                ClearComboBoxVisibility(CB_PO);
                ClearLabelVisibility(L_PO);
            }

            if (CB_Quantity.SelectedIndex >= 0)
            {
                L_PO.Visibility = Visibility.Visible;
                CB_PO.Visibility = Visibility.Visible;
            }
        }

        private void CB_Barcode_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string query = "SELECT Barcode_No FROM Inventory WHERE Inventory_Item = @item";

            if (CB_Barcode.Visibility == Visibility.Visible)
            {
                DataTable table = new DataTable();

                try
                {
                    using (SqlConnection sql = new SqlConnection(App.DBConnString))
                    {
                        sql.Open();
                        SqlCommand com = sql.CreateCommand();
                        com.CommandText = query;

                        com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());

                        com.ExecuteNonQuery();

                        SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                        {
                            SelectCommand = com
                        };

                        adapter.Fill(table);

                        CB_Barcode.DataContext = table.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            
        }

        private void CB_Quantity_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string query = "SELECT Quantity FROM Inventory WHERE Inventory_Item = @item AND Barcode_No = @barcode";

            if (CB_Quantity.Visibility == Visibility.Visible)
            {
                DataTable table = new DataTable();

                try
                {
                    using (SqlConnection sql = new SqlConnection(App.DBConnString))
                    {
                        sql.Open();
                        SqlCommand com = sql.CreateCommand();
                        com.CommandText = query;

                        com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());
                        com.Parameters.AddWithValue("@barcode", Convert.ToDecimal(CB_Barcode.SelectedValue.ToString()));

                        com.ExecuteNonQuery();

                        SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                        {
                            SelectCommand = com
                        };

                        adapter.Fill(table);

                        CB_Quantity.DataContext = table.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void CB_PO_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string query = "SELECT PO# FROM Inventory WHERE Inventory_Item = @item AND Barcode_No = @barcode AND Quantity = @quantity";

            if (CB_PO.Visibility == Visibility.Visible)
            {
                DataTable table = new DataTable();

                try
                {
                    using (SqlConnection sql = new SqlConnection(App.DBConnString))
                    {
                        sql.Open();
                        SqlCommand com = sql.CreateCommand();
                        com.CommandText = query;

                        com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());
                        com.Parameters.AddWithValue("@barcode", Convert.ToDecimal(CB_Barcode.SelectedValue.ToString()));
                        com.Parameters.AddWithValue("@quantity", Convert.ToInt32(CB_Quantity.SelectedValue.ToString()));

                        com.ExecuteNonQuery();

                        SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                        {
                            SelectCommand = com
                        };

                        adapter.Fill(table);

                        CB_PO.DataContext = table.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ClearComboBoxVisibility(params ComboBox[] comboBoxes)
        {
            foreach (ComboBox box in comboBoxes)
                box.Visibility = Visibility.Hidden;
        }

        private void ClearLabelVisibility(params Label[] labels)
        {
            foreach (Label label in labels)
                label.Visibility = Visibility.Hidden;
        }

        private void ClearAll()
        {
            ClearComboBoxVisibility(CB_Barcode, CB_Quantity, CB_PO);
            ClearLabelVisibility(L_Barcode, L_Quantity, L_PO);

            CB_ItemID.SelectedIndex = -1;
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

        private void BTN_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (CB_ItemID.SelectedIndex < 0 || CB_Barcode.SelectedIndex < 0 || CB_Quantity.SelectedIndex < 0 || CB_PO.SelectedIndex < 0)
            {
                MessageBox.Show("You must select from all 4 boxes to delete an inbounded item.");

                return;
            }

            string query = "DELETE FROM Inventory WHERE Inventory_Item = @item AND Barcode_No = @barcode AND Quantity = @quantity AND PO# = @po";

            if (CB_PO.Visibility == Visibility.Visible)
            {
                try
                {
                    using (SqlConnection sql = new SqlConnection(App.DBConnString))
                    {
                        sql.Open();
                        SqlCommand com = sql.CreateCommand();
                        com.CommandText = query;

                        com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());
                        com.Parameters.AddWithValue("@barcode", Convert.ToDecimal(CB_Barcode.SelectedValue.ToString()));
                        com.Parameters.AddWithValue("@quantity", Convert.ToInt32(CB_Quantity.SelectedValue.ToString()));
                        com.Parameters.AddWithValue("@po", CB_PO.SelectedValue.ToString());

                        com.ExecuteNonQuery();

                        App.UpdateQuantityAvailable(CB_ItemID.SelectedValue.ToString());

                        MessageBox.Show($"The inbounded item matching:\n\n\t" +
                            $"Item ID: {CB_ItemID.SelectedValue.ToString()}\n\t" +
                            $"Barcode: {CB_Barcode.SelectedValue.ToString()}\n\t" +
                            $"Quantity: {CB_Quantity.SelectedValue.ToString()}\n\t" +
                            $"PO: {CB_PO.SelectedValue.ToString()}\n\nhas been removed from inventory.");

                        ClearAll();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
