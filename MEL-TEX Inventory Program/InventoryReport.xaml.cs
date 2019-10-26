using MELTEX.Database;
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
    /// Interaction logic for InventoryInbound.xaml
    /// </summary>
    public partial class InventoryReport : Page
    {
        #region Fields

        private const string COLSTRING = "item.Inventory_Item AS 'Item ID', item.Description, inventory.Barcode_No AS Barcode, inventory.Warehouse, inventory.BIN, inventory.Quantity AS 'QTY on Hand', " +
                    "inventory.QuantityAvail AS 'QTY Available', item.List_Price AS 'List Price', item.Multiplier AS 'Mult', item.Weight, item.Published_Sales AS 'Pub Sale', item.Notes ";

        private DataTable inventoryDataTable;
        internal Page previousPage;

        #endregion Fields

        #region Properties

        public DataTable QuoteDataTable { get; set; }

        #endregion Properties

        #region Constructors

        public InventoryReport(Page page)
        {
            InitializeComponent();

            previousPage = page;
        }

        #endregion Constructors

        /*********************************************
         *
         *      EVENT HANDLERS:
         *
         ********************************************/

        #region Methods

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

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

        private void BTN_ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TB_SearchDesc.Text = "";
            TB_SearchID.Text = "";
        }

        private void BTN_ClearSelected_Click(object sender, RoutedEventArgs e)
        {
            InitializeQuoteDataGrid();
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

        private void InitializeQuoteDataGrid()
        {
            QuoteDataTable = new DataTable();

            foreach (DataColumn col in inventoryDataTable.Columns)
                QuoteDataTable.Columns.Add(new DataColumn(col.ColumnName));

            QuoteDataGrid.DataContext = QuoteDataTable;
        }

        private void InventoryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int rowIndex = (sender as DataGridRow).GetIndex();
            QuoteDataTable.ImportRow(inventoryDataTable.Rows[rowIndex]);

            AddLineNumbers(QuoteDataTable);
            QuoteDataGrid.UpdateLayout();
        }

        private void InventoryReportPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Inventory Report";

            PopulateInventoryDataGrid();
            InitializeQuoteDataGrid();
            PopulateCustomersComboBox();
        }

        private void PopulateCustomersComboBox()
        {
            DataTable table = DBController.GetTableFromQuery(sqlconn: App.SalesDBConnString, columns: "Name, Number", t1: "Customer");
            CB_Customers.DataContext = table.DefaultView;
        }

        private void PopulateInventoryDataGrid(string searchText = "", string searchCol = "")
        {
            inventoryDataTable = new DataTable();

            try
            {
                inventoryDataTable = DBController.GetTableFromQuery(App.DBConnString, COLSTRING, "Items", "Inventory_Item", "item", "Inventory", "Inventory_Item", "inventory", "item", "Inventory_Item", "item", searchCol, searchText);

                AddLineNumbers(inventoryDataTable);

                InventoryDataGrid.DataContext = inventoryDataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem populating Inventory DataGrid:\n\n" + ex.Message);
            }
        }

        private void QuoteDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int rowIndex = (sender as DataGridRow).GetIndex();
            QuoteDataTable.Rows.Remove(QuoteDataTable.Rows[rowIndex]);

            AddLineNumbers(QuoteDataTable);
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchcol = "Inventory_Item";

            if ((sender as TextBox).Name == "TB_Description")
                searchcol = "Description";

            PopulateInventoryDataGrid((sender as TextBox).Text, searchcol);
        }

        /*******************************************
         *
         *      CodeBehind Methods
         *
         ******************************************/
        /********************************************
         *
         *      BUTTON HANDLING
         *
         *******************************************/

        private void TB_SearchID_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        #endregion Methods
    }
}