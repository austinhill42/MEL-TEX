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