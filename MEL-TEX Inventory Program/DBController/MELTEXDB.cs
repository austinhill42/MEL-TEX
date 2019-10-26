using System.ComponentModel;

namespace MELTEX.Database
{
    internal class MELTEXDB : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion Methods

        #region Classes

        internal class Groups : MELTEXDB
        {
            #region Fields

            private string _Group;
            private string _Multiplier;

            #endregion Fields

            #region Properties

            public string Group { get => _Group; set { _Group = value; OnPropertyChanged("Group"); } }
            public string Multiplier { get => _Multiplier; set { _Multiplier = value; OnPropertyChanged("Multiplier"); } }

            #endregion Properties
        }

        internal class Inventory : MELTEXDB
        {
            #region Fields

            private string _Actual_Cost;
            private string _Barcode_No;
            private string _BIN;
            private string _Cost;
            private string _Inventory_Item;
            private string _Notes;
            private string _PO;
            private string _Quantity;
            private string _QuantityAvail;
            private string _Warehouse;

            #endregion Fields

            #region Properties

            public string Actual_Cost { get => _Actual_Cost; set { _Actual_Cost = value; OnPropertyChanged("Actual_Cost"); } }
            public string Barcode_No { get => _Barcode_No; set { _Barcode_No = value; OnPropertyChanged("Barcode_No"); } }
            public string BIN { get => _BIN; set { _BIN = value; OnPropertyChanged("BIN"); } }
            public string Cost { get => _Cost; set { _Cost = value; OnPropertyChanged("Cost"); } }
            public string Inventory_Item { get => _Inventory_Item; set { _Inventory_Item = value; OnPropertyChanged("Inventory_Item"); } }
            public string Notes { get => _Notes; set { _Notes = value; OnPropertyChanged("Notes"); } }
            public string PO { get => _PO; set { _PO = value; OnPropertyChanged("PO"); } }
            public string Quantity { get => _Quantity; set { _Quantity = value; OnPropertyChanged("Quantity"); } }
            public string QuantityAvail { get => _QuantityAvail; set { _QuantityAvail = value; OnPropertyChanged("QuantityAvail"); } }
            public string Warehouse { get => _Warehouse; set { _Warehouse = value; OnPropertyChanged("Warehouse"); } }

            #endregion Properties
        }

        internal class Items : MELTEXDB
        {
            #region Fields

            private string _Description;
            private string _Group;
            private string _Inventory_Item;
            private string _List_Price;
            private string _Multiplier;
            private string _Notes;
            private string _Published_Cost;
            private string _Published_Sales;
            private string _Weight;

            #endregion Fields

            #region Properties

            public string Description { get => _Description; set { _Description = value; OnPropertyChanged("Description"); } }
            public string Group { get => _Group; set { _Group = value; OnPropertyChanged("Group"); } }
            public string Inventory_Item { get => _Inventory_Item; set { _Inventory_Item = value; OnPropertyChanged("Inventory_Item"); } }
            public string List_Price { get => _List_Price; set { _List_Price = value; OnPropertyChanged("List_Price"); } }
            public string Multiplier { get => _Multiplier; set { _Multiplier = value; OnPropertyChanged("Multiplier"); } }
            public string Notes { get => _Notes; set { _Notes = value; OnPropertyChanged("Notes"); } }
            public string Published_Cost { get => _Published_Cost; set { _Published_Cost = value; OnPropertyChanged("Published_Cost"); } }
            public string Published_Sales { get => _Published_Sales; set { _Published_Sales = value; OnPropertyChanged("Published_Sales"); } }
            public string Weight { get => _Weight; set { _Weight = value; OnPropertyChanged("Weight"); } }

            #endregion Properties
        }

        #endregion Classes
    }
}