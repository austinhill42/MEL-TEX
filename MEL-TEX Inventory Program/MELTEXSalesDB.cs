using System.ComponentModel;

namespace MELTEX.Database
{
    internal class MELTEXSalesDB : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion Methods

        #region Classes

        internal class Customer : MELTEXSalesDB
        {
            #region Fields

            private string _Bill_To;
            private string _Fax;
            private string _Mail_To;
            private string _Name;
            private string _Notes;
            private string _Number;
            private string _Phone;
            private string _Terms;
            private string _Website;

            #endregion Fields

            #region Properties

            public string Bill_To { get => _Bill_To; set { _Bill_To = value; OnPropertyChanged("Bill_To"); } }
            public string Fax { get => _Fax; set { _Fax = value; OnPropertyChanged("Fax"); } }
            public string Mail_To { get => _Mail_To; set { _Mail_To = value; OnPropertyChanged("Mail_To"); } }
            public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); } }
            public string Notes { get => _Notes; set { _Notes = value; OnPropertyChanged("Notes"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string Phone { get => _Phone; set { _Phone = value; OnPropertyChanged("Phone"); } }
            public string Terms { get => _Terms; set { _Terms = value; OnPropertyChanged("Terms"); } }
            public string Website { get => _Website; set { _Website = value; OnPropertyChanged("Website"); } }

            #endregion Properties
        }

        internal class Customer_Contact_Email : MELTEXSalesDB
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

        internal class Customer_Contact_Fax : MELTEXSalesDB
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

        internal class Customer_Contact_Phones : MELTEXSalesDB
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

        internal class Customer_Contact_Social_Media : MELTEXSalesDB
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

        internal class Customer_Contacts : MELTEXSalesDB
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

        internal class Customer_Ship_Locations : MELTEXSalesDB
        {
            #region Fields

            private string _Number;
            private string _Ship_To;

            #endregion Fields

            #region Properties

            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string Ship_To { get => _Ship_To; set { _Ship_To = value; OnPropertyChanged("Ship_To"); } }

            #endregion Properties
        }

        internal class Quotes : MELTEXSalesDB
        {
            #region Fields

            private string _BillTo;
            private string _Buyer;
            private string _Data;
            private string _FOB;
            private string _FreightTerms;
            private string _Items;
            private string _Notes;
            private string _Number;
            private string _RepName;
            private string _RepNumber;
            private string _ShipTo;
            private string _ShipVia;
            private string _Terms;

            #endregion Fields

            #region Properties

            public string BillTo { get => _BillTo; set { _BillTo = value; OnPropertyChanged("BillTo"); } }
            public string Buyer { get => _Buyer; set { _Buyer = value; OnPropertyChanged("Buyer"); } }
            public string Data { get => _Data; set { _Data = value; OnPropertyChanged("Data"); } }
            public string FOB { get => _FOB; set { _FOB = value; OnPropertyChanged("FOB"); } }
            public string FreightTerms { get => _FreightTerms; set { _FreightTerms = value; OnPropertyChanged("FreightTerms"); } }
            public string Items { get => _Items; set { _Items = value; OnPropertyChanged("Items"); } }
            public string Notes { get => _Notes; set { _Notes = value; OnPropertyChanged("Notes"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string RepName { get => _RepName; set { _RepName = value; OnPropertyChanged("RepName"); } }
            public string RepNumber { get => _RepNumber; set { _RepNumber = value; OnPropertyChanged("RepNumber"); } }
            public string ShipTo { get => _ShipTo; set { _ShipTo = value; OnPropertyChanged("ShipTo"); } }
            public string ShipVia { get => _ShipVia; set { _ShipVia = value; OnPropertyChanged("ShipVia"); } }
            public string Terms { get => _Terms; set { _Terms = value; OnPropertyChanged("Terms"); } }

            #endregion Properties
        }

        internal class SalesOrders : MELTEXSalesDB
        {
            #region Fields

            private string _BillTo;
            private string _Buyer;
            private string _Data;
            private string _FOB;
            private string _FreightTerms;
            private string _Items;
            private string _Notes;
            private string _Number;
            private string _QuoteNumber;
            private string _RepName;
            private string _RepNumber;
            private string _ShipTo;
            private string _ShipVia;
            private string _Terms;

            #endregion Fields

            #region Properties

            public string BillTo { get => _BillTo; set { _BillTo = value; OnPropertyChanged("BillTo"); } }
            public string Buyer { get => _Buyer; set { _Buyer = value; OnPropertyChanged("Buyer"); } }
            public string Data { get => _Data; set { _Data = value; OnPropertyChanged("Data"); } }
            public string FOB { get => _FOB; set { _FOB = value; OnPropertyChanged("FOB"); } }
            public string FreightTerms { get => _FreightTerms; set { _FreightTerms = value; OnPropertyChanged("FreightTerms"); } }
            public string Items { get => _Items; set { _Items = value; OnPropertyChanged("Items"); } }
            public string Notes { get => _Notes; set { _Notes = value; OnPropertyChanged("Notes"); } }
            public string Number { get => _Number; set { _Number = value; OnPropertyChanged("Number"); } }
            public string QuoteNumber { get => _QuoteNumber; set { _QuoteNumber = value; OnPropertyChanged("QuoteNumber"); } }
            public string RepName { get => _RepName; set { _RepName = value; OnPropertyChanged("RepName"); } }
            public string RepNumber { get => _RepNumber; set { _RepNumber = value; OnPropertyChanged("RepNumber"); } }
            public string ShipTo { get => _ShipTo; set { _ShipTo = value; OnPropertyChanged("ShipTo"); } }
            public string ShipVia { get => _ShipVia; set { _ShipVia = value; OnPropertyChanged("ShipVia"); } }
            public string Terms { get => _Terms; set { _Terms = value; OnPropertyChanged("Terms"); } }

            #endregion Properties
        }

        #endregion Classes
    }
}