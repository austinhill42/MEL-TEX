using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for GeneratePO.xaml
    /// </summary>
    public partial class GeneratePO : Page
    {
        internal Page previousPage;
        private const string COLSTRING = "item.Inventory_Item AS 'Item ID', item.Description, inventory.Barcode_No AS Barcode, inventory.Warehouse, inventory.BIN, inventory.Quantity AS 'QTY on Hand', " +
                    "inventory.QuantityAvail AS 'QTY Available', item.List_Price AS 'List Price', item.Multiplier AS 'Mult.', item.Weight, item.Published_Sales AS 'Pub. Sale', item.Notes ";
        private DataTable inventoryDataTable;
        private DataTable PODataTable;

        public GeneratePO(Page page)
        {
            InitializeComponent();

            previousPage = page;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Generate PO";

            PopulateInventoryDataGrid();
            InitializaPODataGrid();
            PopulateCustomersComboBox();
        }

        private void TB_SearchID_TextChanged(object sender, TextChangedEventArgs e)
        {
            String query =
                    $"SELECT {COLSTRING} " +
                    "FROM (" +
                        "SELECT * " +
                        "FROM Items item " +
                        $"WHERE item.Inventory_Item LIKE '{(sender as TextBox).Text}%'" +
                    ") item " +
                    "LEFT JOIN Inventory inventory ON item.Inventory_Item = inventory.Inventory_Item " +
                    "ORDER BY item.Inventory_Item ASC";

            if ((sender as TextBox).Text == "")
                PopulateInventoryDataGrid();
            else
                SearchTable(query);
        }

        private void TB_SearchDesc_TextChanged(object sender, TextChangedEventArgs e)
        {
            String query =
                    $"SELECT {COLSTRING} " +
                    "FROM (" +
                        "SELECT * " +
                        "FROM Items item " +
                        $"WHERE item.Description LIKE '%{(sender as TextBox).Text}%'" +
                    ") item " +
                    "LEFT JOIN Inventory inventory ON item.Inventory_Item = inventory.Inventory_Item " +
                    "ORDER BY item.Inventory_Item ASC";

            if ((sender as TextBox).Text == "")
                PopulateInventoryDataGrid();
            else
                SearchTable(query);
        }

        private void InventoryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int rowIndex = (sender as DataGridRow).GetIndex();
            PODataTable.ImportRow(inventoryDataTable.Rows[rowIndex]);

            AddLineNumbers(PODataTable);
        }
        
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int rowIndex = (sender as DataGridRow).GetIndex();
            PODataTable.Rows.Remove(PODataTable.Rows[rowIndex]);

            AddLineNumbers(PODataTable);
        }

        private void PopulateInventoryDataGrid()
        {
            string query = $"SELECT {COLSTRING} " +
                    "FROM Inventory inventory " +
                    "FULL JOIN Items item on item.Inventory_Item = Inventory.Inventory_Item " +
                    "ORDER BY item.Inventory_Item ASC";

            PopulateInventoryDataGrid(query);

        }

        private void PopulateInventoryDataGrid(string query)
        {
            inventoryDataTable = new DataTable();

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

                    inventoryDataTable = new DataTable();

                    adapter.Fill(inventoryDataTable);
                }

                AddLineNumbers(inventoryDataTable);

                InventoryDataGrid.DataContext = inventoryDataTable.DefaultView;


            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem populating Inventory DataGrid:\n\n" + ex.Message);
            }
        }

        private void InitializaPODataGrid()
        {
            PODataTable = new DataTable();

            foreach (DataColumn col in inventoryDataTable.Columns)
                PODataTable.Columns.Add(new DataColumn(col.ColumnName));

            PODataGrid.DataContext = PODataTable.DefaultView;
        }

        private void PopulateCustomersComboBox()
        {
            using (SqlConnection sql = new SqlConnection(App.PurchasingDBConnString))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();

                com.CommandText = "SELECT Number, Name FROM Vendor ";

                SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                {
                    SelectCommand = com
                };

                DataTable table = new DataTable();

                adapter.Fill(table);

                CB_Vendors.DataContext = table.DefaultView;
            }
        }

        private void SearchTable(string query)
        {
            InventoryDataGrid.DataContext = null;

            using (SqlConnection sql = new SqlConnection(App.DBConnString))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();

                com.CommandText = query;

                SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                {
                    SelectCommand = com
                };

                inventoryDataTable = new DataTable();

                adapter.Fill(inventoryDataTable);

                InventoryDataGrid.DataContext = inventoryDataTable.DefaultView;
            }
        }

        private void AddLineNumbers(DataTable table)
        {
            if (!table.Columns.Contains("Line:"))
            {
                table.Columns.Add(new DataColumn("Line:", typeof(string)));
                table.Columns["Line:"].SetOrdinal(0);
            }

            int num = 1;

            foreach (DataRow row in table.Rows)
                row[0] = num++;
        }


        private void BTN_ClearSelected_Click(object sender, RoutedEventArgs e)
        {
            InitializaPODataGrid();
        }

        private void BTN_ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TB_SearchDesc.Text = "";
            TB_SearchID.Text = "";
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }


        private void BTN_CreatePO_Click(object sender, RoutedEventArgs e)
        {
            if (CB_Vendors.SelectedIndex < 0)
            {
                MessageBox.Show("You must select a vendor to create a PO");
                return;
            }

            DataTable table = new DataTable();
            List<string> shipFrom = new List<string>();

            try
            {
                using (SqlConnection sql = new SqlConnection(App.PurchasingDBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "Select * FROM Vendor WHERE Number = @num";

                    com.Parameters.AddWithValue("@num", CB_Vendors.SelectedValue.ToString());
                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(table);

                    com.CommandText = "SELECT Ship_From FROM Vendor_Ship_Locations WHERE Number = @num";
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        shipFrom.Add(reader.GetValue(0).ToString().Replace('|', '\n'));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            string seller = table.Rows[0]["Name"].ToString();
            string payTo = table.Rows[0]["Pay_To"].ToString().Replace('|', '\n');
            string terms = table.Rows[0]["Terms"].ToString();
            DataTable items = ((DataView)PODataGrid.ItemsSource).ToTable();

            items.Columns.Remove("Barcode");
            items.Columns.Remove("List Price");
            items.Columns.Remove("Mult.");
            items.Columns.Remove("Warehouse");
            items.Columns.Remove("BIN");
            items.Columns.Remove("QTY Available");

            items.Columns["QTY on Hand"].ColumnName = "QTY";

            foreach (DataRow row in items.Rows)
            {   
                row["QTY"] = 0;
            }

            CreatePO.Data data = new CreatePO.Data("", 0, 0, 0, false, seller, payTo, "", shipFrom, "", terms, "", "", "", "", "", items);

            MainWindow.GetWindow(this).Content = new CreatePO(this, data);
        }
    }
}
