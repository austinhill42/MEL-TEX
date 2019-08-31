using System;
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
        
        public InventoryInbound(Page prev)
        {
            InitializeComponent();

            previousPage = prev;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateItemsComboBox();
        }

        private void CB_ItemID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as ComboBox).SelectedIndex >= 0)
                   PopulateItemInfo();

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
            try
            {
                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "INSERT INTO Inventory ([Inventory_Item], [Cost], [Barcode_No], [Warehouse], [BIN], [Quantity], [QuantityAvail], [PO#], [Notes]) VALUES (@item,@cost,@barcode,@warehouse,@bin,@quantity,@avail,@po,@notes) ";

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

            MessageBox.Show($"Item {CB_ItemID.SelectedValue.ToString()} inbounded successfully");

            UpdateQuantityAvailable();
            Clear();
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
