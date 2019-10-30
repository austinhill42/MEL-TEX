using System;
using System.Collections.Generic;
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
    /// Interaction logic for WorkOrderItem.xaml
    /// </summary>
    public partial class WorkOrderItem : UserControl
    {
        public string ItemID { get; set; }
        public string Barcode { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }

        public WorkOrderItem(string itemID, string barcode, string description)
        {
            InitializeComponent();

            DataContext = this;

            ItemID = $"Item ID: {itemID}";
            Barcode = $"Barcode: {barcode}";
            Description = $"Description: {description}";
        }
    }
}
