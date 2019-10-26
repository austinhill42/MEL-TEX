using System.ComponentModel;

namespace MELTEX.Database
{
    internal class MELTEXPurchasingDB : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion Methods

        #region Classes

        internal class PO : MELTEXPurchasingDB
        {
            #region Fields

            private string _Cost;
            private string _Data;
            private string _FOB;
            private string _FreightTerms;
            private string _FullyReceived;
            private string _Items;
            private string _Notes;
            private string _Number;
            private string _PayTo;
            private string _ReceivedCostByWeight;
            private string _RepName;
            private string _RepNumber;
            private string _Seller;
            private string _ShipFrom;
            private string _ShipVia;
            private string _Terms;
            private string _Weight;
            private string _WeightRemaining;

            #endregion Fields

            #region Properties

            public string Cost { get => _Cost; set { _Cost = value; OnPropertyChanged("Cost"); } }
            public string Data { get => _Data; set { _Data = value; OnPropertyChanged("Data"); } }
            public string FOB { get => _FOB; set { _FOB = value; OnPropertyChanged("FOB"); } }
            public string FreightTerms { get => _FreightTerms; set { _FreightTerms = value; OnPropertyChanged("FreightTerms"); } }
            public string FullyReceived { get => _FullyReceived; set { _FullyReceived = value; OnPropertyChanged("FullyReceived"); } }
            public string Items { get => _Items; set { _Items = value; OnPropertyChanged("Items"); } }
            public string Notes { get => _Notes; set { _Notes = value; OnPropertyChanged("Notes"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string PayTo { get => _PayTo; set { _PayTo = value; OnPropertyChanged("PayTo"); } }
            public string ReceivedCostByWeight { get => _ReceivedCostByWeight; set { _ReceivedCostByWeight = value; OnPropertyChanged("ReceivedCostByWeight"); } }
            public string RepName { get => _RepName; set { _RepName = value; OnPropertyChanged("RepName"); } }
            public string RepNumber { get => _RepNumber; set { _RepNumber = value; OnPropertyChanged("RepNumber"); } }
            public string Seller { get => _Seller; set { _Seller = value; OnPropertyChanged("Seller"); } }
            public string ShipFrom { get => _ShipFrom; set { _ShipFrom = value; OnPropertyChanged("ShipFrom"); } }
            public string ShipVia { get => _ShipVia; set { _ShipVia = value; OnPropertyChanged("ShipVia"); } }
            public string Terms { get => _Terms; set { _Terms = value; OnPropertyChanged("Terms"); } }
            public string Weight { get => _Weight; set { _Weight = value; OnPropertyChanged("Weight"); } }
            public string WeightRemaining { get => _WeightRemaining; set { _WeightRemaining = value; OnPropertyChanged("WeightRemaining"); } }

            #endregion Properties
        }

        internal class Vendor : MELTEXPurchasingDB
        {
            #region Fields

            private string _Fax;
            private string _Mail_From;
            private string _Name;
            private string _Notes;
            private string _Number;
            private string _Pay_To;
            private string _Phone;
            private string _Terms;
            private string _Website;

            #endregion Fields

            #region Properties

            public string Fax { get => _Fax; set { _Fax = value; OnPropertyChanged("Fax"); } }
            public string Mail_From { get => _Mail_From; set { _Mail_From = value; OnPropertyChanged("Mail_From"); } }
            public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); } }
            public string Notes { get => _Notes; set { _Notes = value; OnPropertyChanged("Notes"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string Pay_To { get => _Pay_To; set { _Pay_To = value; OnPropertyChanged("Pay_To"); } }
            public string Phone { get => _Phone; set { _Phone = value; OnPropertyChanged("Phone"); } }
            public string Terms { get => _Terms; set { _Terms = value; OnPropertyChanged("Terms"); } }
            public string Website { get => _Website; set { _Website = value; OnPropertyChanged("Website"); } }

            #endregion Properties
        }

        internal class Vendor_Contact_Email : MELTEXPurchasingDB
        {
            #region Fields

            private string _Email;
            private string _Name;
            private string _Number;

            #endregion Fields

            #region Properties

            public string Email { get => _Email; set { _Email = value; OnPropertyChanged("Email"); } }
            public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }

            #endregion Properties
        }

        internal class Vendor_Contact_Fax : MELTEXPurchasingDB
        {
            #region Fields

            private string _Fax;
            private string _Name;
            private string _Number;

            #endregion Fields

            #region Properties

            public string Fax { get => _Fax; set { _Fax = value; OnPropertyChanged("Fax"); } }
            public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }

            #endregion Properties
        }

        internal class Vendor_Contact_Phones : MELTEXPurchasingDB
        {
            #region Fields

            private string _Name;
            private string _Number;
            private string _Phone_No;
            private string _Type;

            #endregion Fields

            #region Properties

            public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string Phone_No { get => _Phone_No; set { _Phone_No = value; OnPropertyChanged("Phone_No"); } }
            public string Type { get => _Type; set { _Type = value; OnPropertyChanged("Type"); } }

            #endregion Properties
        }

        internal class Vendor_Contact_Social_Media : MELTEXPurchasingDB
        {
            #region Fields

            private string _Name;
            private string _Number;
            private string _Social_Media_Page;

            #endregion Fields

            #region Properties

            public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string Social_Media_Page { get => _Social_Media_Page; set { _Social_Media_Page = value; OnPropertyChanged("Social_Media_Page"); } }

            #endregion Properties
        }

        internal class Vendor_Contacts : MELTEXPurchasingDB
        {
            #region Fields

            private string _Name;
            private string _Notes;
            private string _Number;
            private string _Title;

            #endregion Fields

            #region Properties

            public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); } }
            public string Notes { get => _Notes; set { _Notes = value; OnPropertyChanged("Notes"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string Title { get => _Title; set { _Title = value; OnPropertyChanged("Title"); } }

            #endregion Properties
        }

        internal class Vendor_Ship_Locations : MELTEXPurchasingDB
        {
            #region Fields

            private string _Number;
            private string _Ship_From;

            #endregion Fields

            #region Properties

            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string Ship_From { get => _Ship_From; set { _Ship_From = value; OnPropertyChanged("Ship_From"); } }

            #endregion Properties
        }

        #endregion Classes
    }
}