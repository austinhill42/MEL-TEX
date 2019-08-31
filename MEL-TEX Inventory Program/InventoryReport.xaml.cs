using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for InventoryInbound.xaml
    /// </summary>
    public partial class InventoryReport : Page
    {
        internal Page previousPage;
        private const string COLSTRING = "item.Inventory_Item AS 'Item ID', item.Description, inventory.Barcode_No AS Barcode, inventory.Warehouse, inventory.BIN, inventory.Quantity AS 'QTY on Hand', " +
                    "inventory.QuantityAvail AS 'QTY Available', item.List_Price AS 'List Price', item.Multiplier AS 'Mult.', item.Weight, item.Published_Sales AS 'Pub. Sale', item.Notes ";
        private static readonly string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        private readonly string connString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}MEL-TEXDB.mdf; Integrated Security = True; Connect Timeout = 30";
        private DataTable inventoryDataTable;
        private DataTable quoteDataTable;

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
            quoteDataTable.ImportRow(inventoryDataTable.Rows[rowIndex]);

            AddLineNumbers(quoteDataTable);
        }

        private void QuoteDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int rowIndex = (sender as DataGridRow).GetIndex();
            quoteDataTable.Rows.Remove(quoteDataTable.Rows[rowIndex]);

            AddLineNumbers(quoteDataTable);
            InventoryDataGrid.DataContext = quoteDataTable;

            
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
                using (SqlConnection sql = new SqlConnection(connString))
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
            quoteDataTable = new DataTable();

            foreach (DataColumn col in inventoryDataTable.Columns)
                quoteDataTable.Columns.Add(new DataColumn(col.ColumnName));

            QuoteDataGrid.DataContext = quoteDataTable.DefaultView;
        }

        private void SearchTable(string query)
        {
            InventoryDataGrid.DataContext = null;

            using (SqlConnection sql = new SqlConnection(connString))
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

        }

        private void BTN_OpenQuote_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BTN_OpenSalesOrder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

    }
}
