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
    /// Interaction logic for GenerateQuote.xaml
    /// </summary>
    public partial class GenerateQuote : Page
    {

        private Page PreviousPage;
        private string date = "";
        private readonly string connString = App.SalesDBConnString;
        internal Data data;
        private bool saved;
        private bool opened;

        [Serializable]
        public struct Data
        {
            internal string number;
            internal string buyer;
            internal string billTo;
            internal string selectedShipTo;
            internal List<string> shipTo;
            internal string shipVia;
            internal string terms;
            internal string fob;
            internal string freightTerms;
            internal string repNum;
            internal string repName;
            internal string notes;
            internal DataTable table;

            public Data(string num, string buyer, string bill, string selectship, List<string> ship, string shipvia, string terms, string fob, string freight, string repnum, string repname, string notes, DataTable table)
            {
                number = num;
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
        }

        public GenerateQuote(Page prev, Data d)
        {
            InitializeComponent();

            PreviousPage = prev;
            data = d;
            saved = false;
            opened = false;

        }

        public GenerateQuote(Page prev, string open)
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
                    com.CommandText = "SELECT Data, Items FROM Quotes WHERE Number = @num";

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
                L_Num.Content = $"Quote: {data.number}";
                saved = true;
            }
            else
            {
                data.number = GetNum();

               L_Num.Content = $"Quote: {data.number}";
            }
        }

        private void ControlChanged(object sender, EventArgs e)
        {
            saved = false;
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

        private string GetNum()
        {
            int prevdate = 0;
            int prevNum = 0;

            using (SqlConnection sql = new SqlConnection(connString))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();
                com.CommandText = "SELECT Number FROM Quotes";

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
            string pyscript = $"{loc}Quote PDF.py";
            string loadfile = $"{loc}Data\\Quotes\\{data.number}.csv";
            string savefile = $"{loc}Quotes\\{data.number}.pdf";

            Directory.CreateDirectory($"{loc}Data\\Quotes");
            Directory.CreateDirectory($"{loc}Quotes");

            StreamWriter writer = new StreamWriter(loadfile);

            writer.WriteLine($"{DateTime.Now.Date.ToShortDateString()};Quote: {data.number};{data.buyer.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};" +
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
                        MessageBox.Show($"Quote: {data.number} saved to PDF");
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
            MainWindow.GetWindow(this).Content = PreviousPage;
        }

        private void BTN_Presale_Click(object sender, RoutedEventArgs e)
        {
            if (!saved)
            {
                MessageBox.Show("You have to save before creating the Presale Worksheet");
                return;
            }

            GeneratePresale.Data pData = new GeneratePresale.Data(data.number, data.buyer, data.billTo, data.selectedShipTo, data.shipTo, data.shipVia, data.terms, data.fob, data.freightTerms, data.repNum, data.repName, "", data.table);
            GeneratePresale presale = new GeneratePresale(this, pData);
            MainWindow.GetWindow(this).Content = presale;
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

                string query = "INSERT INTO Quotes ([Number], [Buyer], [BillTo], [ShipTo], [ShipVia], [Terms], [FOB], [FreightTerms], [RepNumber], [RepName], [Notes], [Items], [Data])" +
                    "VALUES (@num,@buyer,@bill,@ship,@shipvia,@terms,@fob,@freight,@repnum,@repname,@notes,@items,@data)";

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT Number from Quotes WHERE Number = @num";

                    com.Parameters.AddWithValue("@num", data.number);

                    SqlDataReader reader = com.ExecuteReader();

                    com.Parameters.RemoveAt("@num");

                    if (reader.HasRows)
                    {
                        MessageBoxResult result = MessageBox.Show("Quote already exists, do you want to overwrite? Selecting \"No\" will save the quote under a new quote number.", "Overwrite", MessageBoxButton.YesNoCancel);
                        if (result == MessageBoxResult.Yes)
                        {
                            query = "UPDATE Quotes " +
                               "SET [Buyer] = @buyer, [BillTo] = @bill, [ShipTo] = @ship, [ShipVia] = @shipvia, [Terms] = @terms, [FOB] = @fob, [FreightTerms] = @freight, [RepNumber] = @repnum, [RepName] = @repname, [Notes] = @notes, [Items] = @items, [Data] = @data " +
                               "WHERE [Number] = @num";
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            data.number = GetNum();
                            L_Num.Content = $"Quote: {data.number}";
                        }
                        else
                            return;
                    }

                    reader.Close();

                    com.CommandText = query;

                    com.Parameters.AddWithValue("@num", data.number);
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

                    MessageBox.Show($"Quote: {data.number} saved to database.");

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
