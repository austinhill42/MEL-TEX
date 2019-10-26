using System.ComponentModel;

namespace MELTEX.Database
{
    internal class MEL_TEXAccountingDB : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion Methods

        #region Classes

        internal class Invoice : MEL_TEXAccountingDB
        {
            #region Fields

            private string _AccountNo;
            private string _Acknowledged;
            private string _Bank;
            private string _CheckNo;
            private string _Confirmation;
            private string _DateDue;
            private string _DateReceived;
            private string _InvoiceNo;
            private string _Name;
            private string _Notes;
            private string _Paid;

            #endregion Fields

            #region Properties

            public string AccountNo { get => _AccountNo; set { _AccountNo = value; OnPropertyChanged("AccountNo"); } }
            public string Acknowledged { get => _Acknowledged; set { _Acknowledged = value; OnPropertyChanged("Acknowledged"); } }
            public string Bank { get => _Bank; set { _Bank = value; OnPropertyChanged("Bank"); } }
            public string CheckNo { get => _CheckNo; set { _CheckNo = value; OnPropertyChanged("CheckNo"); } }
            public string Confirmation { get => _Confirmation; set { _Confirmation = value; OnPropertyChanged("Confirmation"); } }
            public string DateDue { get => _DateDue; set { _DateDue = value; OnPropertyChanged("DateDue"); } }
            public string DateReceived { get => _DateReceived; set { _DateReceived = value; OnPropertyChanged("DateReceived"); } }
            public string InvoiceNo { get => _InvoiceNo; set { _InvoiceNo = value; OnPropertyChanged("InvoiceNo"); } }
            public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); } }
            public string Notes { get => _Notes; set { _Notes = value; OnPropertyChanged("Notes"); } }
            public string Paid { get => _Paid; set { _Paid = value; OnPropertyChanged("Paid"); } }

            #endregion Properties
        }

        #endregion Classes
    }
}