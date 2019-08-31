﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for InventoryInbound.xaml
    /// </summary>
    public partial class InventoryInbound : Page
    {
        private Page previousPage;
        private static readonly string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        private readonly string connString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}MEL-TEXDB.mdf; Integrated Security = True; Connect Timeout = 30";
        private bool editItem = false;
        DataTable selectedInbound;

        public InventoryInbound(Page prev)
        {
            InitializeComponent();

            previousPage = prev;
        }

        public InventoryInbound(Page prev, bool edit) : this(prev)
        {
            editItem = edit;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Inventory Inbound";

            if (editItem)
                PopulateInboundComboBox();
            else
                PopulateItemsComboBox();
        }

        private void CB_ItemID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as ComboBox).SelectedIndex >= 0)
                {
                    if (editItem)
                        PopulateInboundInfo();
                    else
                        PopulateItemInfo();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        private void Clear()
        {
            CB_ItemID.SelectedIndex = -1;

            foreach (UIElement child in this.Grid.Children)
                if ((child as TextBox) != null)
                    (child as TextBox).Text = "";

            foreach (UIElement child in this.Grid.Children)
                if (child as Label != null)
                    if ((child as Label).Name.StartsWith("L_"))
                        (child as Label).Content = "";
        }

        private void PopulateItemsComboBox()
        {
            try
            {
                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT * FROM Items";

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    DataTable items = new DataTable();

                    adapter.Fill(items);

                    CB_ItemID.DataContext = items.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PopulateInboundComboBox()
        {
            using (SqlConnection sql = new SqlConnection(connString))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();
                com.CommandText = "SELECT * FROM Inventory ";

                com.ExecuteNonQuery();

                SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                {
                    SelectCommand = com
                };

                DataTable table = new DataTable();

                adapter.Fill(table);

                CB_ItemID.DataContext = table.DefaultView;
            }
        }

        private void PopulateItemInfo()
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT * FROM Items WHERE Inventory_Item = @item";

                    com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(table);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            TB_Desc.Text = table.Rows[0]["Description"].ToString();
            L_Group.Content = table.Rows[0]["Group"];
            L_ListPrice.Content = table.Rows[0]["List_Price"];
            L_Mult.Content = table.Rows[0]["Multiplier"];
            L_PublishedCost.Content = table.Rows[0]["Published_Cost"];
            L_PublishedSales.Content = table.Rows[0]["Published_Sales"];
            L_Weight.Content = table.Rows[0]["Weight"];
            TB_ItemNotes.Text = table.Rows[0]["Notes"].ToString().Replace("\n", "\n\n");

        }

        private void PopulateInboundInfo()
        {
            try
            {
                DataTable selectedItem = new DataTable();

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT * FROM Items WHERE Inventory_Item = @item";

                    com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(selectedItem);
                }

                TB_Desc.Text = selectedItem.Rows[0]["Description"].ToString();
                L_Group.Content = selectedItem.Rows[0]["Group"];
                L_ListPrice.Content = selectedItem.Rows[0]["List_Price"];
                L_Mult.Content = selectedItem.Rows[0]["Multiplier"];
                L_PublishedCost.Content = selectedItem.Rows[0]["Published_Cost"];
                L_PublishedSales.Content = selectedItem.Rows[0]["Published_Sales"];
                L_Weight.Content = selectedItem.Rows[0]["Weight"];
                TB_ItemNotes.Text = selectedItem.Rows[0]["Notes"].ToString().Replace("\n", "\n\n");

                selectedInbound = new DataTable();

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT * FROM Inventory WHERE Inventory_Item = @item ";

                    com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(selectedInbound);
                }

                TB_Barcode.Text = selectedInbound.Rows[0]["Barcode_No"].ToString();
                TB_BIN.Text = selectedInbound.Rows[0]["BIN"].ToString();
                TB_Cost.Text = selectedInbound.Rows[0]["Cost"].ToString();
                TB_Warehouse.Text = selectedInbound.Rows[0]["Warehouse"].ToString();
                TB_Quantity.Text = selectedInbound.Rows[0]["Quantity"].ToString();
                TB_PO.Text = selectedInbound.Rows[0]["PO#"].ToString();
                TB_Notes.Text = selectedInbound.Rows[0]["Notes"].ToString();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void UpdateQuantityAvailable()
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

                    com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());

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

                    com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());
                    com.Parameters.AddWithValue("@avail", quantityAvail);

                    com.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            if (editItem)
                EditInboundedItem();
            else
                InboundItem();

            UpdateQuantityAvailable();
            Clear();
        }

        private void InboundItem()
        {
            try
            {
                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "INSERT INTO Inventory ([Inventory_Item], [Cost], [Barcode_No], [Warehouse], [BIN], [Quantity], [QuantityAvail], [PO#], [Notes]) " +
                        "VALUES (@item,@cost,@barcode,@warehouse,@bin,@quantity,@avail,@po,@notes) ";

                    com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());
                    com.Parameters.AddWithValue("@cost", Convert.ToDecimal(TB_Cost.Text));
                    com.Parameters.AddWithValue("@barcode", Convert.ToInt32(TB_Barcode.Text));
                    com.Parameters.AddWithValue("@warehouse", TB_Warehouse.Text);
                    com.Parameters.AddWithValue("@bin", TB_BIN.Text);
                    com.Parameters.AddWithValue("@quantity", Convert.ToInt32(TB_Quantity.Text));
                    com.Parameters.AddWithValue("@avail", 0);
                    com.Parameters.AddWithValue("@po", TB_PO.Text);
                    com.Parameters.AddWithValue("@notes", TB_Notes.Text);

                    com.ExecuteNonQuery();

                    MessageBox.Show($"Item {CB_ItemID.SelectedValue.ToString()} inbounded successfully");
                }
            }
            catch (Exception ex)
            {
                if (ex is System.FormatException)
                    MessageBox.Show("One of the fields was entered with an incorrect format. Check your values and try again.");
                else if (ex is SqlException)
                {
                    if (ex.Message.Contains("conflicted"))
                        MessageBox.Show("The item does not exist. Enter the item into the \"Inventory Items\" page first.");
                }
                else
                    MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        private void EditInboundedItem()
        {
            try
            {
                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "UPDATE Inventory " +
                        "SET [Cost] = @cost, [Barcode_No] = @barcode, [Warehouse] = @warehouse, [BIN] = @bin, [Quantity] = @quantity, [QuantityAvail] = @avail, [PO#] = @po, [Notes] = @notes " +
                        "WHERE [Inventory_Item] = @item AND [Cost] = @oldcost AND [Barcode_No] = @oldbarcode AND [Warehouse] = @oldwarehouse AND [BIN] = @oldbin AND [Quantity] = @oldquantity AND [QuantityAvail] = @oldavail AND [PO#] = @oldpo AND [Notes] = @oldnotes ";

                    com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());
                    com.Parameters.AddWithValue("@cost", Convert.ToDecimal(TB_Cost.Text));
                    com.Parameters.AddWithValue("@barcode", Convert.ToInt32(TB_Barcode.Text));
                    com.Parameters.AddWithValue("@warehouse", TB_Warehouse.Text);
                    com.Parameters.AddWithValue("@bin", TB_BIN.Text);
                    com.Parameters.AddWithValue("@quantity", Convert.ToInt32(TB_Quantity.Text));
                    com.Parameters.AddWithValue("@avail", 0);
                    com.Parameters.AddWithValue("@po", TB_PO.Text);
                    com.Parameters.AddWithValue("@notes", TB_Notes.Text);

                    com.Parameters.AddWithValue("@oldcost", Convert.ToDecimal(selectedInbound.Rows[0]["Cost"].ToString()));
                    com.Parameters.AddWithValue("@oldbarcode", Convert.ToInt32(selectedInbound.Rows[0]["Barcode_No"].ToString()));
                    com.Parameters.AddWithValue("@oldwarehouse", selectedInbound.Rows[0]["Warehouse"].ToString());
                    com.Parameters.AddWithValue("@oldbin", selectedInbound.Rows[0]["BIN"].ToString());
                    com.Parameters.AddWithValue("@oldquantity", Convert.ToInt32(selectedInbound.Rows[0]["Quantity"].ToString()));
                    com.Parameters.AddWithValue("@oldavail", selectedInbound.Rows[0]["QuantityAvail"].ToString());
                    com.Parameters.AddWithValue("@oldpo", selectedInbound.Rows[0]["PO#"].ToString());
                    com.Parameters.AddWithValue("@oldnotes", selectedInbound.Rows[0]["Notes"].ToString());


                    com.ExecuteNonQuery();

                    MessageBox.Show($"Item {CB_ItemID.SelectedValue.ToString()} updated successfully");
                }
            }
            catch (Exception ex)
            {
                if (ex is System.FormatException)
                    MessageBox.Show("One of the fields was entered with an incorrect format. Check your values and try again.");
                else
                    MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            TB_Notes.Text += $"{date} -- {time} -- {TB_AddNote.Text}\n";

            TB_AddNote.Text = "";
        }
    }
}
