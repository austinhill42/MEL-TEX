using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for GeneratePresale.xaml
    /// </summary>
    public partial class GeneratePresale : Page
    {
        private Page PreviousPage;
        private string date = "";
        internal Data data;

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

        public GeneratePresale(Page prev, Data d)
        {
            InitializeComponent();

            PreviousPage = prev;
            data = d;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            date = DateTime.Now.ToString("MM/dd/yyyy");

            L_Date.Content = date;
            L_Num.Content = $"Presale: {data.number}-PW";

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
            TB_Notes.Text = "";

            DataGrid.DataContext = data.table.DefaultView;
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

        private void generatePDF(bool print)
        {
            string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            string pyexe = $"{loc}Python37_64\\python.exe";
            string pyscript = $"{loc}Presale PDF.py";
            string loadfile = $"{loc}Data\\Presale\\{data.number}-PW.csv";
            string savefile = $"{loc}Presale\\{data.number}.pdf";

            Directory.CreateDirectory($"{loc}Data\\Presale");
            Directory.CreateDirectory($"{loc}Presale");

            StreamWriter writer = new StreamWriter(loadfile);

            writer.WriteLine($"{date};Presale Worksheet: {data.number}-PW;Quote: {data.number};{data.buyer.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};" +
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
                        MessageBox.Show($"Presale Worksheet: {data.number}-PW saved to PDF");
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

        private void BTN_SalesOrder_Click(object sender, RoutedEventArgs e)
        {
            GenerateSalesOrder.Data sData = new GenerateSalesOrder.Data("", data.number, data.buyer, data.billTo, data.selectedShipTo, data.shipTo, data.shipVia, data.terms, data.fob, data.freightTerms, data.repNum, data.repName, "", data.table);
            GenerateSalesOrder salesorder = new GenerateSalesOrder(this, sData);
            MainWindow.GetWindow(this).Content = salesorder;
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            foreach (TextBox tb in Grid.Children.OfType<TextBox>())
                tb.Text = "";
        }

        private void BTN_Print_Click(object sender, RoutedEventArgs e)
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

                generatePDF(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }
    }
}