using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            items = data.table.AsDataView().ToTable(true, "Item ID", "Barcode", "Description");

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

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
