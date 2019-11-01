using MELTEX.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for InventoryInbound.xaml
    /// </summary>
    public partial class InventoryReport : Page, INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Fields

        private readonly string InventoryColumns = "Barcode_No AS Barcode, Warehouse, BIN, Quantity AS 'QTY on Hand', QuantityAvail AS 'QTY Available' ";
        private readonly string ItemsColumns = "Inventory_Item AS 'Item ID', Description, Quantity AS 'Total QTY on Hand', QuantityAvail AS 'Total QTY Available', List_Price AS 'List Price', Multiplier AS 'Mult', Weight, Published_Sales AS 'Pub Sale', Notes ";
        private readonly string[] ExcludedFromQuote = { "Warehouse", "BIN", "List Price", "Mult", "Notes", "Total QTY on Hand", "Total QTY Available"};

        private DataTable _InventoryDataTable;
        private DataTable _ItemsDataTable;
        private DataTable _QuoteDataTable;
        private List<string> list;

        internal Page previousPage;

        #endregion Fields

        #region Properties

        public DataTable InventoryDataTable { get => _InventoryDataTable; set { _InventoryDataTable = value; OnPropertyChanged("InventoryDataTable"); } }
        public DataTable ItemsDataTable { get => _ItemsDataTable; set { _ItemsDataTable = value; OnPropertyChanged("ItemsDataTable"); } }
        public List<string> List { get => list; set { list = value; OnPropertyChanged("list"); } }
        public DataTable QuoteDataTable { get => _QuoteDataTable; set { _QuoteDataTable = value; OnPropertyChanged("QuoteDataTable"); } }

        #endregion Properties

        #region Constructors

        public InventoryReport(Page page)
        {
            InitializeComponent();

            previousPage = page;
        }

        #endregion Constructors

        #region Methods

        private DataTable AddLineNumbers(DataTable table)
        {
            DataTable tb = table.Copy();

            if (tb.Columns.Contains("Line:"))
                tb.Columns.Remove("Line:");

            tb.Columns.Add(new DataColumn("Line:", Type.GetType("System.Int32")));

            tb.Columns["Line:"].SetOrdinal(0);

            tb.AsEnumerable().ToList().ForEach(x => x["Line:"] = x.Table.Rows.IndexOf(x) + 1);


            return tb;
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

            ExcludedFromQuote.ToList().ForEach(
                x => {
                    if (items.Columns.Contains(x))
                        items.Columns.Remove(x);
                });

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

        private void InventoryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataTable t1 = ItemsDataTable.Clone();
            DataTable t2 = InventoryDataTable.Clone();

            t1.ImportRow((ItemsDataGrid.SelectedItem as DataRowView).Row);
            t2.ImportRow(((sender as DataGrid).SelectedItem as DataRowView).Row);

            DataTable t3 = MergeTables(t1, t2);

            ExcludedFromQuote.ToList().ForEach(
                x =>
                {
                    if (t3.Columns.Contains(x))
                        t3.Columns.Remove(x);
                });

            if (QuoteDataTable == null)
                QuoteDataTable = t3.Clone();

            t3.AsEnumerable().ToList().ForEach(x => QuoteDataTable.ImportRow(x));

            QuoteDataTable = AddLineNumbers(QuoteDataTable);
        }

        private void InitializeQuoteDataGrid()
        {
            
        }

        private void InventoryReportPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Inventory Report";

            DataContext = this;

            PopulateInventoryDataGrid();
            InitializeQuoteDataGrid();
            PopulateCustomersComboBox();

        }

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        private void PopulateCustomersComboBox()
        {
            DataTable table = DBController.GetTableFromQuery(sqlconn: App.SalesDBConnString, columns: "Name, Number", t1: "Customer");
            CB_Customers.DataContext = table.DefaultView;
        }

        private void PopulateInventoryDataGrid(string searchText = "", string searchCol = "")
        {
            try
            {
                ItemsDataTable = DBController.GetTableFromQuery(sqlconn: App.DBConnString,
                                                                columns: ItemsColumns,
                                                                t1: "Items",
                                                                searchCol: searchCol,
                                                                searchStart: searchText);

                ItemsDataTable = AddLineNumbers(ItemsDataTable);
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

            QuoteDataTable = AddLineNumbers(QuoteDataTable);
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchcol = "Inventory_Item";

            if ((sender as TextBox).Name == "TB_Description")
                searchcol = "Description";

            PopulateInventoryDataGrid((sender as TextBox).Text, searchcol);
        }

        private void ItemsDataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            int i = e.Row.GetIndex();// ItemsDataGrid.SelectedIndex;
            string itemid = ItemsDataTable.Rows[i]["Item ID"].ToString();

            InventoryDataTable = DBController.GetTableFromQuery(sqlconn: App.DBConnString,
                                                                    columns: InventoryColumns,
                                                                    t1: "Inventory",
                                                                    searchCol: "Inventory_Item",
                                                                    searchEqual: itemid);
        }

        public static DataTable MergeTables(DataTable baseTable, params DataTable[] additionalTables)
        {
            // Build combined table columns
            DataTable merged = baseTable;
            foreach (DataTable dt in additionalTables)
            {
                merged = AddTable(merged, dt);
            }
            return merged;
        }

        public static DataTable AddTable(DataTable baseTable, DataTable additionalTable)
        {
            // Build combined table columns
            DataTable merged = baseTable.Clone();                  // Include all columns from base table in result.
            foreach (DataColumn col in additionalTable.Columns)
            {
                string newColumnName = col.ColumnName;
                merged.Columns.Add(newColumnName, col.DataType);
            }
            // Add all rows from both tables
            var bt = baseTable.AsEnumerable();
            var at = additionalTable.AsEnumerable();
            var mergedRows = bt.Zip(at, (r1, r2) => r1.ItemArray.Concat(r2.ItemArray).ToArray());
            foreach (object[] rowFields in mergedRows)
            {
                merged.Rows.Add(rowFields);
            }
            return merged;
        }

        #endregion Methods

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
    }
}