using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections;
using MELTEX.DBController;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for InventoryInbound.xaml
    /// </summary>
    public partial class InventoryReport : Page
    {
        internal Page previousPage;
        private const string COLSTRING = "item.Inventory_Item AS 'Item ID', item.Description, inventory.Barcode_No AS Barcode, inventory.Warehouse, inventory.BIN, inventory.Quantity AS 'QTY on Hand', " +
                    "inventory.QuantityAvail AS 'QTY Available', item.List_Price AS 'List Price', item.Multiplier AS 'Mult', item.Weight, item.Published_Sales AS 'Pub Sale', item.Notes ";
        private DataTable inventoryDataTable;
        public DataTable QuoteDataTable { get; set; }

        public InventoryReport(Page page)
        {
            InitializeComponent();

            previousPage = page;
        }

        /*********************************************
         * 
         * 
         *      EVENT HANDLERS:
         *          InventoryReportPage_Loaded
         *          TB_SearchID_TextChanged
         *          TB_SearchDesc_TextChanged 
         *          InventoryDataGrid_MouseDoubleClick
         * 
         * 
         ********************************************/

        private void InventoryReportPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Inventory Report";
           
            PopulateInventoryDataGrid();
            InitializeQuoteDataGrid();
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
            QuoteDataTable.ImportRow(inventoryDataTable.Rows[rowIndex]);

            AddLineNumbers(QuoteDataTable);
            QuoteDataGrid.UpdateLayout();
        }

        private void QuoteDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int rowIndex = (sender as DataGridRow).GetIndex();
            QuoteDataTable.Rows.Remove(QuoteDataTable.Rows[rowIndex]);

            AddLineNumbers(QuoteDataTable);


        }

        /*******************************************
         * 
         *      CodeBehind Methods
         *      
         ******************************************/

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

        private void InitializeQuoteDataGrid()
        {
            QuoteDataTable = new DataTable();

            foreach (DataColumn col in inventoryDataTable.Columns)
                QuoteDataTable.Columns.Add(new DataColumn(col.ColumnName));

            QuoteDataGrid.DataContext = QuoteDataTable;
            //QuoteDataGrid.UpdateLayout();
        }

        private void PopulateCustomersComboBox()
        {
            using (SqlConnection sql = new SqlConnection(App.SalesDBConnString))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();

                com.CommandText = "SELECT Number, Name FROM Customer ";

                SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                {
                    SelectCommand = com
                };

                DataTable table = new DataTable();

                adapter.Fill(table);

                CB_Customers.DataContext = table.DefaultView;
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


        /********************************************
         * 
         *      BUTTON HANDLING
         *      
         *******************************************/

        private void BTN_ClearSelected_Click(object sender, RoutedEventArgs e)
        {
            InitializeQuoteDataGrid();
        }

        private void BTN_ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TB_SearchDesc.Text = "";
            TB_SearchID.Text = "";
        }

        private void BTN_CreateQuote_Click(object sender, RoutedEventArgs e)
        {
            if (CB_Customers.SelectedIndex < 0)
            {
                MessageBox.Show("You must select a customer to create a quote");
                return;
            }

            DataTable table = new DataTable();
            List<string> shipTo = new List<string>();

            try
            {
                using (SqlConnection sql = new SqlConnection(App.SalesDBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "Select * FROM Customer WHERE Number = @num";

                    com.Parameters.AddWithValue("@num", CB_Customers.SelectedValue.ToString());
                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(table);

                    com.CommandText = "SELECT Ship_To FROM Customer_Ship_Locations WHERE Number = @num";
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        shipTo.Add(reader.GetValue(0).ToString().Replace('|', '\n'));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            string buyer = table.Rows[0]["Name"].ToString();
            string billto = table.Rows[0]["Bill_To"].ToString().Replace('|', '\n');
            string terms = table.Rows[0]["Terms"].ToString();
            DataTable items = ((DataView)QuoteDataGrid.ItemsSource).ToTable();

            items.Columns.Remove("Barcode");
            items.Columns.Remove("Warehouse");
            items.Columns.Remove("BIN");
            items.Columns.Remove("List Price");
            items.Columns.Remove("Mult");
            items.Columns.Remove("Notes");

            GenerateQuote.Data data = new GenerateQuote.Data("", buyer, billto, "", shipTo, "", terms, "", "", "", "", "", items);

            MainWindow.GetWindow(this).Content = new GenerateQuote(this, data);
        }

        private void BTN_OpenQuote_Click(object sender, RoutedEventArgs e)
        {
            OpenQuote open = new OpenQuote();

            if (open.ShowDialog() ?? false)
                MainWindow.GetWindow(this).Content = new GenerateQuote(this, open.quotenum);

        }

        private void BTN_OpenSalesOrder_Click(object sender, RoutedEventArgs e)
        {
            OpenSalesOrder open = new OpenSalesOrder();

            if (open.ShowDialog() ?? false)
                MainWindow.GetWindow(this).Content = new GenerateSalesOrder(this, open.salesordernum);
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }
    }
}
