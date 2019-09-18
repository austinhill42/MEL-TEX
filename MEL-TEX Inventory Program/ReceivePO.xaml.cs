using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;


namespace MELTEX
{
    /// <summary>
    /// Interaction logic for ReceivePO.xaml
    /// </summary>
    public partial class ReceivePO : Page
    {
        private Page PreviousPage;
        private string date = "";
        private readonly string connString = App.PurchasingDBConnString;
        internal CreatePO.Data data;
        private bool saved;
        private bool opened;
        private Queue<Tuple<string, string, string>> toReceive = new Queue<Tuple<string, string, string>>();
        private List<Tuple<string, string>> toInbound = new List<Tuple<string, string>>();

        [Serializable]
        public struct Data
        {
            internal string number;
            internal decimal Weight;
            internal decimal Cost;
            internal decimal ReceivedCostByWeight;
            internal bool FullyReceived;
            internal string seller;
            internal string payTo;
            internal string selectedShipFrom;
            internal List<string> shipFrom;
            internal string shipVia;
            internal string terms;
            internal string fob;
            internal string freightTerms;
            internal string repNum;
            internal string repName;
            internal string notes;
            internal DataTable table;

            public Data(string num, decimal weight, decimal cost, decimal byweight, bool fullyreceived, string seller, string pay, string selectship, List<string> ship, string shipvia, string terms, string fob, string freight, string repnum, string repname, string notes, DataTable table)
            {
                number = num;
                Weight = weight;
                Cost = cost;
                ReceivedCostByWeight = byweight;
                FullyReceived = fullyreceived;
                this.seller = seller;
                payTo = pay;
                selectedShipFrom = selectship;
                shipFrom = ship;
                shipVia = shipvia;
                this.terms = terms;
                this.fob = fob;
                freightTerms = freight;
                repNum = repnum;
                repName = repname;
                this.notes = notes;
                this.table = table;
            }
        }

        public ReceivePO(Page prev, string open)
        {
            InitializeComponent();

            PreviousPage = prev;
            saved = true;
            opened = true;

            byte[] binData = null;
            byte[] binTable = null;

            try
            {
                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT Data, Items FROM PO_Standard WHERE Number = @num";

                    com.Parameters.AddWithValue("@num", open);

                    com.ExecuteNonQuery();

                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        binData = (byte[])reader["Data"];
                        binTable = (byte[])reader["Items"];
                    }

                    reader.Close();
                }

                using (MemoryStream stream = new MemoryStream(binData))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    data = (CreatePO.Data)formatter.Deserialize(stream);
                }

                using (MemoryStream stream = new MemoryStream(binTable))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    data.table = (DataTable)formatter.Deserialize(stream);
                }

                if (!data.table.Columns.Contains("Selected"))
                {
                    DataColumn selected = new DataColumn("Selected", typeof(bool));
                    selected.DefaultValue = false;
                    data.table.Columns.Add(selected);
                }

                if (!data.table.Columns.Contains("Received"))
                {
                    DataColumn received = new DataColumn("Received", typeof(bool));
                    received.ReadOnly = true;
                    received.DefaultValue = false;
                    data.table.Columns.Add(received);
                }

                if (!data.table.Columns.Contains("Inbounded"))
                {
                    DataColumn inbounded = new DataColumn("Inbounded", typeof(bool));
                    inbounded.ReadOnly = true;
                    inbounded.DefaultValue = false;
                    data.table.Columns.Add(inbounded);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (TextBox tb in Grid.Children.OfType<TextBox>())
                tb.TextChanged += ControlChanged;

            DataGrid.CurrentCellChanged += ControlChanged;
            CB_ShipFrom.SelectionChanged += ControlChanged;

            DateTime poDate;
            DateTime.TryParseExact(data.number.Split('-')[0], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out poDate);
            L_Date.Content = poDate.ToString("MM/dd/yyyy");

            L_ReceivedCost.Content = $"Received Cost by Weight: {(data.ReceivedCostByWeight != 0 ? data.ReceivedCostByWeight.ToString("0.00") : "N/A")}";

            TB_Seller.Text = data.seller;
            TB_PayTo.Text = data.payTo;
            CB_ShipFrom.ItemsSource = data.shipFrom;
            CB_ShipFrom.Text = data.selectedShipFrom;
            TB_ShipVia.Text = data.shipVia;
            TB_Terms.Text = data.terms;
            TB_FOB.Text = data.fob;
            TB_FreightTerms.Text = data.freightTerms;
            TB_RepNum.Text = data.repNum;
            TB_RepName.Text = data.repName;
            TB_Notes.Text = data.notes;

            DataGrid.DataContext = data.table.DefaultView;

            L_Num.Content = $"PO: {data.number}";

            if (data.table.Columns.Contains("Pub. Sale"))
                data.table.Columns["Pub. Sale"].ColumnName = "Sale Price";

            saved = true;
        }

        private void ControlChanged(object sender, EventArgs e)
        {
            saved = false;
        }

        private void CB_ShowList_Checked(object sender, RoutedEventArgs e)
        {
            CB_ShipFrom.Visibility = Visibility.Visible;
            TB_ShipFrom.Visibility = Visibility.Hidden;
        }

        private void CB_ShowList_Unchecked(object sender, RoutedEventArgs e)
        {
            CB_ShipFrom.Visibility = Visibility.Hidden;
            TB_ShipFrom.Visibility = Visibility.Visible;
        }

        private void Clear()
        {
            foreach (TextBox tb in Grid.Children.OfType<TextBox>())
                tb.Text = "";

            InitializaDataGrid();
        }

        private void InitializaDataGrid()
        {
            DataTable table = new DataTable();

            foreach (DataColumn col in ((DataView)DataGrid.ItemsSource).ToTable().Columns)
                table.Columns.Add(new DataColumn(col.ColumnName));

            DataGrid.DataContext = table.DefaultView;
        }

        private void UpdateTableInDatabase()
        {
            byte[] binTable;

            try
            {

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, data.table);
                binTable = stream.ToArray();
            }
                string query = "UPDATE PO_Standard " +
                                   "SET [FullyReceived] = @received, [Items] = @items " +
                                   "WHERE [Number] = @num";

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = query;

                    com.Parameters.AddWithValue("@num", data.number);
                    com.Parameters.AddWithValue("@received", data.FullyReceived);
                    com.Parameters.AddWithValue("@items", binTable);

                    com.ExecuteNonQuery();

                    saved = true;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking received in database:\n\n{ex.Message}\n\n{ex.StackTrace}");
            }

        }

        private void UpdateDataInDatabase()
        {
            byte[] binData;

            try
            {

                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data);
                    binData = stream.ToArray();
                }
                string query = "UPDATE PO_Standard " +
                                   "SET [Data] = @data " +
                                   "WHERE [Number] = @num";

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = query;

                    com.Parameters.AddWithValue("@num", data.number);
                    com.Parameters.AddWithValue("@data", binData);

                    com.ExecuteNonQuery();

                    saved = true;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating data in database:\n\n{ex.Message}\n\n{ex.StackTrace}");
            }

        }

        private void UpdateReceivedByWeight()
        {
            try
            {
                string query = "UPDATE PO_Standard " +
                                   "SET [ReceivedCostByWeight] = @cost " +
                                   "WHERE [Number] = @num";

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = query;

                    com.Parameters.AddWithValue("@num", data.number);
                    com.Parameters.AddWithValue("@cost", data.ReceivedCostByWeight);

                    com.ExecuteNonQuery();

                    saved = true;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating received cost by weight in database:\n\n{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            TB_Notes.Text += $"{date} -- {time} -- {TB_AddNote.Text}\n";

            TB_AddNote.Text = "";
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            if (saved)
                MainWindow.GetWindow(this).Content = PreviousPage;
            else
                if (MessageBox.Show("Do you want to exit without saving?\n\n", "Save or Quit", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                return;

        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                data.seller = TB_Seller.Text;
                data.payTo = TB_PayTo.Text;
                data.selectedShipFrom = CB_ShowList.IsChecked ?? false ? CB_ShipFrom.SelectedItem.ToString() : TB_ShipFrom.Text;
                data.shipVia = TB_ShipVia.Text;
                data.terms = TB_Terms.Text;
                data.fob = TB_FOB.Text;
                data.freightTerms = TB_FreightTerms.Text;
                data.repName = TB_RepName.Text;
                data.repNum = TB_RepNum.Text;
                data.notes = TB_Notes.Text;
                data.table = ((DataView)DataGrid.ItemsSource).ToTable();
                byte[] binTable;
                byte[] binData;

                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data);
                    binData = stream.ToArray();
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data.table);
                    binTable = stream.ToArray();
                }

                string query = "UPDATE PO_Standard " +
                               "SET [Weight] = @weight, [Cost] = @cost, [ReceivedCostByWeight] = @costByWeight, [FullyReceived] = @received, [Seller] = @seller, [PayTo] = @pay, [ShipFrom] = @ship, [ShipVia] = @shipvia, [Terms] = @terms, [FOB] = @fob, [FreightTerms] = @freight, [RepNumber] = @repnum, [RepName] = @repname, [Notes] = @notes, [Items] = @items, [Data] = @data " +
                               "WHERE [Number] = @num";

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = query;

                    com.Parameters.AddWithValue("@num", data.number);
                    com.Parameters.AddWithValue("@weight", data.Weight);
                    com.Parameters.AddWithValue("@cost", data.Cost);
                    com.Parameters.AddWithValue("@costByWeight", data.ReceivedCostByWeight);
                    com.Parameters.AddWithValue("@received", data.FullyReceived);
                    com.Parameters.AddWithValue("@seller", data.seller);
                    com.Parameters.AddWithValue("@pay", data.payTo);
                    com.Parameters.AddWithValue("@ship", data.selectedShipFrom);
                    com.Parameters.AddWithValue("@shipvia", data.shipVia);
                    com.Parameters.AddWithValue("@terms", data.terms);
                    com.Parameters.AddWithValue("@fob", data.fob);
                    com.Parameters.AddWithValue("@freight", data.freightTerms);
                    com.Parameters.AddWithValue("@repnum", data.repNum);
                    com.Parameters.AddWithValue("@repname", data.repName);
                    com.Parameters.AddWithValue("@notes", data.notes);
                    com.Parameters.AddWithValue("@items", binTable);
                    com.Parameters.AddWithValue("@data", binData);

                    com.ExecuteNonQuery();

                    MessageBox.Show($"PO: {data.number} updated in database.");

                    saved = true;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void BTN_ReceiveSelected_Click(object sender, RoutedEventArgs e)
        {
            decimal weight = 0;
            decimal cost = 0;

            foreach (DataRow row in data.table.Rows)
            {
                if ((bool)row["Selected"])
                {
                    toReceive.Enqueue(new Tuple<string, string, string>(row["Item ID"].ToString(), row["QTY"].ToString(), data.number));
                    data.table.Columns["Received"].ReadOnly = false;
                    row["Received"] = true;
                    data.table.Columns["Received"].ReadOnly = true;
                }

                weight += (decimal)row["Weight"];
                cost += (decimal)row["Sale Price"];
            }

            data.ReceivedCostByWeight = cost / weight;
            L_ReceivedCost.Content = $"Received Cost by Weight: {data.ReceivedCostByWeight.ToString("0.00")}";
            UpdateReceivedByWeight();

            string str = "The following items will be marked received:";

            foreach (Tuple<string, string, string> item in toReceive)
                str += $"\n\t{item.Item1}";

            str += "\n\nDo you wish to inbound them now?";

            if (MessageBox.Show(str, "Inbound Prompt", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                MainWindow.GetWindow(this).Content = new InventoryInbound(this, toReceive);

            UpdateTableInDatabase();
            UpdateDataInDatabase();
        }

        private void BTN_ReceiveAll_Click(object sender, RoutedEventArgs e)
        {
            decimal weight = 0;
            decimal cost = 0;

            foreach (DataRow row in data.table.Rows)
            {
                toReceive.Enqueue(new Tuple<string, string, string>(row["Item ID"].ToString(), row["QTY"].ToString(), data.number));
                data.table.Columns["Received"].ReadOnly = false;
                row["Received"] = true;
                data.table.Columns["Received"].ReadOnly = true;

                weight += Convert.ToDecimal(row["Weight"]);
                cost += Convert.ToDecimal(row["Sale Price"]);
            }

            data.ReceivedCostByWeight = cost / weight;
            L_ReceivedCost.Content = $"Received Cost by Weight: {data.ReceivedCostByWeight.ToString("0.00")}";
            UpdateReceivedByWeight();

            string str = "All items will be marked received. Do you wish to inbound them now?";

            if (MessageBox.Show(str, "Inbound Prompt", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                MainWindow.GetWindow(this).Content = new InventoryInbound(this, toReceive);

            UpdateTableInDatabase();
            UpdateDataInDatabase();
        }
    }
}
