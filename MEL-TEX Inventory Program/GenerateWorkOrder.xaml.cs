using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for GenerateWorkOrder.xaml
    /// </summary>
    public partial class GenerateWorkOrder : Page
    {
        private string date = "";
        private Page PreviousPage;
        internal Data data;
        private DataTable items;
        private List<WorkOrderItem> LineItems = new List<WorkOrderItem>();

        public GenerateWorkOrder(Page prev, Data data)
        {
            InitializeComponent();

            PreviousPage = prev;
            this.data = data;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            date = DateTime.Now.ToString("MM/dd/yyyy");
            L_Date.Content = date;

            TB_Buyer.Text = data.buyer;
            TB_BillTo.Text = data.billTo;
            TB_ShipTo.Text = data.selectedShipTo;
            //CB_ShipTo.ItemsSource = data.shipTo;
            //CB_ShipTo.Text = data.selectedShipTo;
            TB_ShipVia.Text = data.shipVia;
            TB_Terms.Text = data.terms;
            TB_FOB.Text = data.fob;
            TB_FreightTerms.Text = data.freightTerms;
            TB_RepNum.Text = data.repNum;
            TB_RepName.Text = data.repName;

            L_Num.Content = $"Sales Order: {data.number}";

            items = data.table.AsDataView().ToTable(false, "Item ID", "Barcode", "Description");

            //items.AsEnumerable().ToList().ForEach(r => PopulateItems(r));

            foreach (DataRow row in items.Rows)
                PopulateItems(row);

        }

        private void PopulateItems(DataRow r)
        {
            WorkOrderItem item = new WorkOrderItem(r["Item ID"].ToString(), r["Barcode"].ToString(), r["Description"].ToString());
            RowDefinition row = new RowDefinition();
            row.Height = new GridLength(200);
            Grid.RowDefinitions.Add(row);
            Grid.Children.Add(item);
            Grid.SetRow(item, Grid.RowDefinitions.Count - 1);

            LineItems.Add(item);
            
        }

        #region Structs

        [Serializable]
        public struct Data
        {
            #region Fields

            internal string billTo;
            internal string buyer;
            internal string fob;
            internal string freightTerms;
            //internal string notes;
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

            public Data(string num, string quote, string buyer, string bill, string selectship, List<string> ship, string shipvia, string terms, string fob, string freight, string repnum, string repname, /*string notes,*/ DataTable table)
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
                //this.notes = notes;
                this.table = table;
            }

            #endregion Constructors
        }

        #endregion Structs

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = PreviousPage;
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            foreach (TextBox tb in App.FindVisualChildren<TextBox>(Grid))
                tb.Text = "";

            foreach (ComboBox cb in App.FindVisualChildren<ComboBox>(Grid))
                cb.SelectedIndex = -1;
        }

        private void BTN_Print_Click(object sender, RoutedEventArgs e)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string workOrderLoc = App.loc + "Data\\Work Orders\\";

            Directory.CreateDirectory(workOrderLoc);

            FileStream stream = new FileStream(workOrderLoc + data.number, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, data);
            stream.Close();

            StreamWriter write = new StreamWriter(workOrderLoc + data.number + ".csv");

            write.WriteLine($"{DateTime.Now.Date.ToShortDateString()};Sales Order: {data.number};Quote: {data.quoteNum};{data.buyer.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};" +
                $"{data.selectedShipTo.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};{data.billTo.Replace("\n", "\\n").Replace("\r", "\\n").Replace(Environment.NewLine, "\\n")};" +
                $"{data.shipVia};{data.terms};{data.fob};{data.freightTerms};{data.repNum};{data.repName}");

            foreach (WorkOrderItem item in LineItems)
            {
                string id = item.ItemID + "<br/>";
                string barcode = item.Barcode + "<br/>";
                string desc = item.Description + "<br/>";
                string notes = item.Notes.Replace(Environment.NewLine, "<br/>");

                write.Write($";{id}{barcode}{desc}<br/><br/>{notes}<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>");

            }

            write.Close();

            generatePDF(true);
        }

        private void generatePDF(bool print)
        {
            string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            string pyexe = $"{loc}Python37_64\\python.exe";
            string pyscript = $"{loc}Work Order PDF.py";
            string loadfile = $"{loc}Data\\Work Orders\\{data.number}.csv";
            string savefile = $"{loc}Work Orders\\{data.number}.pdf";

            Directory.CreateDirectory($"{loc}Data\\Work Orders");
            Directory.CreateDirectory($"{loc}Work Orders");

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = pyexe,
                Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" {3}", pyscript, loadfile, savefile, print ? "print" : ""),
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using (Process process = Process.Start(start))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                    MessageBox.Show("Something went wrong!!");
                else if (!print)
                    MessageBox.Show("Work Order Saved");
            }
        }
    }
}
