using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
        private string num = "";
        private string date = "";
        private readonly string connString = App.SalesDBConnString;
        internal Data data;
        private bool saved;

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
            internal DataTable table;

            public Data(string num, string buyer, string bill, string selectship, List<string> ship, string shipvia, string terms, string fob, string freight, string repnum, string repname, DataTable table)
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
                this.table = table;
            }
        }

        public GenerateQuote(Page prev, Data d)
        {
            InitializeComponent();

            PreviousPage = prev;
            data = d;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            saved = false;

            foreach (TextBox tb in Grid.Children.OfType<TextBox>())
                tb.TextChanged += ControlChanged;

            DataGrid.CurrentCellChanged += ControlChanged;
            CB_ShipTo.SelectionChanged += ControlChanged;

            date = DateTime.Now.ToString("MM/dd/yyyy");
            num = GetNum();

            L_Date.Content = date;
            L_Num.Content = $"Quote: {num}";

            TB_Buyer.Text = data.buyer;
            TB_BillTo.Text = data.billTo;
            CB_ShipTo.ItemsSource = data.shipTo;
            TB_ShipVia.Text = data.shipVia;
            TB_Terms.Text = data.terms;
            TB_FOB.Text = data.fob;
            TB_FreightTerms.Text = data.freightTerms;
            TB_RepNum.Text = data.repNum;
            TB_RepName.Text = data.repName;

            DataGrid.DataContext = data.table.DefaultView;
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
                    prevdate = Convert.ToInt32(read.Split('-')[0]);
                    prevNum = Convert.ToInt32(read.Split('-')[1]);
                }

                reader.Close();

                if (today > prevdate)
                    return $"{today.ToString()}-1";
                else
                    return $"{today}-{++prevNum}";
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
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            foreach (TextBox tb in Grid.Children.OfType<TextBox>())
                tb.Text = "";
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            data.number = num;
            data.buyer = TB_Buyer.Text;
            data.billTo = TB_BillTo.Text;
            data.selectedShipTo = CB_ShowList.IsChecked ?? false ? CB_ShipTo.SelectedItem.ToString() : TB_ShipTo.Text;
            data.shipVia = TB_ShipVia.Text;
            data.terms = TB_Terms.Text;
            data.fob = TB_FOB.Text;
            data.freightTerms = TB_FreightTerms.Text;
            data.repName = TB_RepName.Text;
            data.repNum = TB_RepNum.Text;
            data.table = ((DataView)DataGrid.ItemsSource).ToTable();

            // TODO finish save, serialize struct, write pyrhon file, incorporate file system structure


            saved = true;
        }
    }
}
