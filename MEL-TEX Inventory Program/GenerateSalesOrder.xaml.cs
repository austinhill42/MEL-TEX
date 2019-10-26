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
    /// Interaction logic for GenerateSalesOrder.xaml
    /// </summary>
    public partial class GenerateSalesOrder : Page
    {
        #region Fields

        private readonly string connString = App.SalesDBConnString;
        private string date = "";
        private bool opened;
        private Page PreviousPage;
        private bool saved;
        internal Data data;

        #endregion Fields

        #region Constructors

        public GenerateSalesOrder(Page prev, Data d)
        {
            InitializeComponent();

            PreviousPage = prev;
            data = d;
            saved = false;
            opened = false;
        }

        public GenerateSalesOrder(Page prev, string open)
        {
            InitializeComponent();

            PreviousPage = prev;
            saved = true;
            opened = true;

            byte[] binData = null;
            byte[] binTable = null;

            try
            {
                using (SqlConnection sql = new SqlConnection(App.SalesDBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT Data, Items FROM SalesOrders WHERE Number = @num";

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
                    data = (Data)formatter.Deserialize(stream);
                }

                using (MemoryStream stream = new MemoryStream(binTable))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    data.table = (DataTable)formatter.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion Constructors

        #region Methods

        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            TB_Notes.Text += $"{date} -- {time} -- {TB_AddNote.Text}\n";

            TB_AddNote.Text = "";
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = PreviousPage;
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            foreach (TextBox tb in Grid.Children.OfType<TextBox>())
                tb.Text = "";
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                data.buyer = TB_Buyer.Text;
                data.billTo = TB_BillTo.Text;
                data.selectedShipTo = CB_ShowList.IsChecked ?? false ? CB_ShipTo.SelectedItem.ToString() : TB_ShipTo.Text;
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

                string query = "INSERT INTO SalesOrders ([Number], [QuoteNumber], [Buyer], [BillTo], [ShipTo], [ShipVia], [Terms], [FOB], [FreightTerms], [RepNumber], [RepName], [Notes], [Items], [Data])" +
                    "VALUES (@num,@quote,@buyer,@bill,@ship,@shipvia,@terms,@fob,@freight,@repnum,@repname,@notes,@items,@data)";

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT Number from SalesOrders WHERE Number = @num";

                    com.Parameters.AddWithValue("@num", data.number);

                    SqlDataReader reader = com.ExecuteReader();

                    com.Parameters.RemoveAt("@num");

                    if (reader.HasRows)
                    {
                        MessageBoxResult result = MessageBox.Show("Sales Order already exists, do you want to overwrite? Selecting \"No\" will save the sales order under a new sales order number.", "Overwrite", MessageBoxButton.YesNoCancel);
                        if (result == MessageBoxResult.Yes)
                        {
                            query = "UPDATE SalesOrders " +
                               "SET [QuoteNumber] = @quote, [Buyer] = @buyer, [BillTo] = @bill, [ShipTo] = @ship, [ShipVia] = @shipvia, [Terms] = @terms, [FOB] = @fob, [FreightTerms] = @freight, [RepNumber] = @repnum, [RepName] = @repname, [Notes] = @notes, [Items] = @items, [Data] = @data " +
                               "WHERE [Number] = @num";
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            data.number = GetNum();
                            L_Num.Content = $"Sales Order: {data.number}";
                        }
                        else
                            return;
                    }

                    reader.Close();

                    com.CommandText = query;

                    com.Parameters.AddWithValue("@num", data.number);
                    com.Parameters.AddWithValue("@quote", data.quoteNum);
                    com.Parameters.AddWithValue("@buyer", data.buyer);
                    com.Parameters.AddWithValue("@bill", data.billTo);
                    com.Parameters.AddWithValue("@ship", data.selectedShipTo);
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

                    MessageBox.Show($"Sales Order: {data.number} saved to database.");

                    generatePDF(true);

                    saved = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void BTN_WorkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!saved)
            {
                MessageBox.Show("You have to save before creating the Work Order");
                return;
            }

            GeneratePresale.Data pData = new GeneratePresale.Data(data.number, data.buyer, data.billTo, data.selectedShipTo, data.shipTo, data.shipVia, data.terms, data.fob, data.freightTerms, data.repNum, data.repName, "", data.table);
            GeneratePresale presale = new GeneratePresale(this, pData);
            MainWindow.GetWindow(this).Content = presale;
        }

        private void CB_ShowList_Checked(object sender, RoutedEventArgs e)
        {
            CB_ShipTo.Visibility = Visibility.Visible;
            TB_ShipTo.Visibility = Visibility.Hidden;
        }

        private void CB_ShowList_Unchecked(object sender, RoutedEventArgs e)
        {
            CB_ShipTo.Visibility = Visibility.Hidden;
            TB_ShipTo.Visibility = Visibility.Visible;
        }

        private void ControlChanged(object sender, EventArgs e)
        {
            saved = false;
        }

        private void generatePDF(bool print)
        {
            string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            string pyexe = $"{loc}Python37_64\\python.exe";
            string pyscript = $"{loc}Sales Order PDF.py";
            string loadfile = $"{loc}Data\\Sales Orders\\{data.number}.csv";
            string savefile = $"{loc}Sales Orders\\{data.number}.pdf";

            Directory.CreateDirectory($"{loc}Data\\Sales Orders");
            Directory.CreateDirectory($"{loc}Sales Orders");

            StreamWriter writer = new StreamWriter(loadfile);

            writer.WriteLine($"{date};Sales Order: {data.number};Quote: {data.quoteNum};{data.buyer.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};" +
                $"{data.selectedShipTo.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};{data.billTo.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};" +
                $"{data.shipVia};{data.terms};{data.fob};{data.freightTerms};{data.repNum};{data.repName};");

            DataTable items = data.table;
            if (items.Columns.Contains("Notes"))
                items.Columns.Remove("Notes");

            for (int i = 0; i < items.Columns.Count; i++)
            {
                string sep = ";";
                if (i == items.Columns.Count - 1)
                    sep = "";

                if (items.Columns[i].ColumnName == "QTY on Hand")
                    writer.Write($"QTY\\non\\nHand{sep}");
                else if (items.Columns[i].ColumnName == "QTY Available")
                    writer.Write($"QTY\\nAvail.{sep}");
                else if (items.Columns[i].ColumnName == "Pub. Sale")
                    writer.Write($"Sale\\nPrice{sep}");
                else
                    writer.Write($"{items.Columns[i].ColumnName}{sep}");
            }

            foreach (DataRow row in items.Rows)
            {
                writer.WriteLine();

                object last = row.ItemArray.Last();

                foreach (object val in row.ItemArray)
                {
                    if (val.Equals(last))
                        writer.Write($"{val.ToString()}");
                    else
                        writer.Write($"{val.ToString()};");
                }
            }

            writer.Close();

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = pyexe,
                Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" {3}", pyscript, loadfile, savefile, print ? "print" : ""),
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            try
            {
                using (Process process = Process.Start(start))
                {
                    process.WaitForExit();

                    Console.WriteLine($"\n\n{process.StandardOutput.ReadToEnd()}\n\n");

                    if (process.ExitCode != 0)
                        MessageBox.Show($"Something went wrong creating PDF: {process.StandardError.ReadToEnd()}");
                    if (!print)
                        MessageBox.Show($"Sales Order: {data.number} saved to PDF");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong creating PDF: {ex.StackTrace}");
            }
        }

        private string GetNum()
        {
            int prevNum = 0;

            using (SqlConnection sql = new SqlConnection(connString))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();
                com.CommandText = "SELECT Number FROM SalesOrders";

                DateTime now = DateTime.Now;

                int today = Convert.ToInt32(now.ToString("yyyy"));

                SqlDataReader reader = com.ExecuteReader();

                while (reader.Read())
                {
                    string read = reader.GetValue(0).ToString();
                    int testNum = Convert.ToInt32(read);
                    prevNum = prevNum > testNum ? prevNum : testNum;
                }

                reader.Close();

                if (today > prevNum)
                    return $"{today.ToString()}{1.ToString("0000")}";
                else
                    return $"{++prevNum}";
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (TextBox tb in Grid.Children.OfType<TextBox>())
                tb.TextChanged += ControlChanged;

            DataGrid.CurrentCellChanged += ControlChanged;
            CB_ShipTo.SelectionChanged += ControlChanged;

            date = DateTime.Now.ToString("MM/dd/yyyy");
            L_Date.Content = date;

            TB_Buyer.Text = data.buyer;
            TB_BillTo.Text = data.billTo;
            CB_ShipTo.ItemsSource = data.shipTo;
            CB_ShipTo.Text = data.selectedShipTo;
            TB_ShipVia.Text = data.shipVia;
            TB_Terms.Text = data.terms;
            TB_FOB.Text = data.fob;
            TB_FreightTerms.Text = data.freightTerms;
            TB_RepNum.Text = data.repNum;
            TB_RepName.Text = data.repName;
            TB_Notes.Text = data.notes;

            DataGrid.DataContext = data.table.DefaultView;

            if (opened)
            {
                L_Num.Content = $"Sales Order: {data.number}";
                saved = true;
            }
            else
            {
                data.number = GetNum();

                L_Num.Content = $"Sales Order: {data.number}";
            }
        }

        #endregion Methods

        #region Structs

        [Serializable]
        public struct Data
        {
            #region Fields

            internal string billTo;
            internal string buyer;
            internal string fob;
            internal string freightTerms;
            internal string notes;
            internal string number;
            internal string quoteNum;
            internal string repName;
            internal string repNum;
            internal string selectedShipTo;
            internal List<string> shipTo;
            internal string shipVia;
            internal DataTable table;
            internal string terms;

            #endregion Fields

            #region Constructors

            public Data(string num, string quote, string buyer, string bill, string selectship, List<string> ship, string shipvia, string terms, string fob, string freight, string repnum, string repname, string notes, DataTable table)
            {
                number = num;
                quoteNum = quote;
                this.buyer = buyer;
                billTo = bill;
                selectedShipTo = selectship;
                shipTo = ship;
                shipVia = shipvia;
                this.terms = terms;
                this.fob = fob;
                freightTerms = freight;
                repNum = repnum;
                repName = repname;
                this.notes = notes;
                this.table = table;
            }

            #endregion Constructors
        }

        #endregion Structs
    }
}