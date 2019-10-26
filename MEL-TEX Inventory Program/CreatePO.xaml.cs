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
    /// Interaction logic for CreatePO.xaml
    /// </summary>
    public partial class CreatePO : Page
    {
        private Page PreviousPage;
        private string date = "";
        private readonly string connString = App.PurchasingDBConnString;
        internal Data data;
        private bool saved;
        private bool opened;
        private bool byWeight;

        [Serializable]
        public struct Data
        {
            internal string number;
            internal decimal Weight;
            internal decimal WeightRemaining;
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
            internal bool byWeight;

            public Data(string num, decimal weight, decimal weightremaining, decimal cost, decimal costbyweight, bool fullyreceived, string seller, string pay, string selectship, List<string> ship, string shipvia, string terms, string fob, string freight, string repnum, string repname, string notes, DataTable table, bool byweight)
            {
                number = num;
                Weight = weight;
                WeightRemaining = weightremaining;
                Cost = cost;
                ReceivedCostByWeight = costbyweight;
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
                byWeight = byweight;
            }
        }

        public CreatePO(Page prev, Data d)
        {
            InitializeComponent();

            PreviousPage = prev;
            data = d;

            saved = false;
            opened = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (TextBox tb in Grid.Children.OfType<TextBox>())
                tb.TextChanged += ControlChanged;

            DataGrid.CurrentCellChanged += ControlChanged;
            CB_ShipFrom.SelectionChanged += ControlChanged;

            date = DateTime.Now.ToString("MM/dd/yyyy");
            L_Date.Content = date;

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

            if (opened)
            {
                L_Num.Content = $"PO: {data.number}";
                saved = true;
            }
            else
            {
                data.number = GetNum();

                L_Num.Content = $"PO: {data.number}";
            }
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

        private string GetNum()
        {
            int prevdate = 0;
            int prevNum = 0;

            using (SqlConnection sql = new SqlConnection(connString))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();
                com.CommandText = "SELECT Number FROM PO";

                DateTime now = DateTime.Now;

                int today = Convert.ToInt32(now.ToString("yyyy") + now.ToString("MM") + now.ToString("dd"));

                SqlDataReader reader = com.ExecuteReader();

                while (reader.Read())
                {
                    string read = reader.GetValue(0).ToString();
                    int testDate = Convert.ToInt32(read.Split('-')[0]);
                    int testNum = Convert.ToInt32(read.Split('-')[1]);
                    prevdate = prevdate > testDate ? prevdate : testDate;
                    prevNum = prevNum > testNum ? prevNum : testNum;
                }

                reader.Close();

                if (today > prevdate)
                    return $"{today.ToString()}-1";
                else
                    return $"{today}-{++prevNum}";
            }
        }

        private void generatePDF(bool print)
        {
            string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            string pyexe = $"{loc}Python37_64\\python.exe";
            string pyscript = $"{loc}POs - Standard PDF.py";
            string loadfile = $"{loc}Data\\POs - Standard\\{data.number}.csv";
            string savefile = $"{loc}POs - Standard\\{data.number}.pdf";

            Directory.CreateDirectory($"{loc}Data\\POs - Standard");
            Directory.CreateDirectory($"{loc}POs - Standard");

            StreamWriter writer = new StreamWriter(loadfile);

            writer.WriteLine($"{date};PO: {data.number};{data.seller.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};" +
                $"{data.selectedShipFrom.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};{data.payTo.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};" +
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
                    writer.Write($"QTY{sep}");
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
                        MessageBox.Show($"PO: {data.number} saved to PDF");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong creating PDF: {ex.StackTrace}");
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

                string query = "INSERT INTO PO ([Number], [Weight], [WeightRemaining], [Cost], [ReceivedCostByWeight], [FullyReceived], [Seller], [PayTo], [ShipFrom], [ShipVia], [Terms], [FOB], [FreightTerms], [RepNumber], [RepName], [Notes], [Items], [Data])" +
                    "VALUES (@num,@weight,@weight,@cost,@costByWeight,@received,@seller,@pay,@ship,@shipvia,@terms,@fob,@freight,@repnum,@repname,@notes,@items,@data)";

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT Number from PO WHERE Number = @num";

                    com.Parameters.AddWithValue("@num", data.number);

                    SqlDataReader reader = com.ExecuteReader();

                    com.Parameters.RemoveAt("@num");

                    if (reader.HasRows)
                    {
                        MessageBoxResult result = MessageBox.Show("PO already exists, do you want to overwrite? Selecting \"No\" will save the PO under a new PO number.", "Overwrite", MessageBoxButton.YesNoCancel);
                        if (result == MessageBoxResult.Yes)
                        {
                            query = "UPDATE PO " +
                               "SET [Weight] = @weight, [Cost] = @cost, [ReceivedCostByWeight] = @costByWeight, [FullyReceived] = @received, [Seller] = @seller, [PayTo] = @pay, [ShipFrom] = @ship, [ShipVia] = @shipvia, [Terms] = @terms, [FOB] = @fob, [FreightTerms] = @freight, [RepNumber] = @repnum, [RepName] = @repname, [Notes] = @notes, [Items] = @items, [Data] = @data " +
                               "WHERE [Number] = @num";
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            data.number = GetNum();
                            L_Num.Content = $"PO: {data.number}";
                        }
                        else
                            return;
                    }

                    reader.Close();

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

                    MessageBox.Show($"PO: {data.number} saved to database.");

                    generatePDF(true);

                    saved = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }
    }
}