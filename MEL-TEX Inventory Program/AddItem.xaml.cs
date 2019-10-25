using MELTEX.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for AddItem.xaml
    /// </summary>
    public partial class AddItem : Page
    {
        private Page previousPage;
        private bool editMode = false;
        private DataTable selectedItem;
        private DataTable GroupTable;
        private DataTable ItemIDTable;
        protected _ItemValues Values { get; set; }

        protected class _ItemValues : INotifyPropertyChanged
        {

            private string _Inventory_Item;
            private string _Description;
            private string _Weight;
            private string _List_Price;
            private string _Group;
            private string _Multiplier;
            private string _Published_Sales;
            private string _Published_Cost;
            private string _Notes;

            public string Inventory_Item { get { return _Inventory_Item; } set { _Inventory_Item = value; OnPropertyChanged("Inventory_Item"); } }
            public string Description { get { return _Description; } set { _Description = value; OnPropertyChanged("Description"); } }
            public string Weight { get { return _Weight; } set { _Weight = value; OnPropertyChanged("Weight"); } }
            public string List_Price { get { return _List_Price; } set { _List_Price = value; OnPropertyChanged("List_Price"); } }
            public string Group { get { return _Group; } set { _Group = value; OnPropertyChanged("Group"); } }
            public string Multiplier { get { return _Multiplier; } set { _Multiplier = value; OnPropertyChanged("Multiplier"); } }
            public string Published_Sales { get { return _Published_Sales; } set { _Published_Sales = value; OnPropertyChanged("Published_Sales"); } }
            public string Published_Cost { get { return _Published_Cost; } set { _Published_Cost = value; OnPropertyChanged("Published_Cost"); } }
            public string Notes { get { return _Notes; } set { _Notes = value; OnPropertyChanged("Notes"); } }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string property)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public AddItem(Page prev)
        {
            InitializeComponent();

            previousPage = prev;

            selectedItem = new DataTable();

            Values = new _ItemValues();
            
            this.DataContext = Values;
        }

        public AddItem(Page prev, bool edit) : this(prev)
        {
            editMode = edit;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateGroupsComboBox();

            if (editMode)
            {
                this.WindowTitle = "Edit Item";
                TB_ItemID.Visibility = Visibility.Hidden;
                CB_ItemID.Visibility = Visibility.Visible;

                PopulateItemIDComboBox();
            }
            else
            {
                this.WindowTitle = "Add Item";
                TB_ItemID.Visibility = Visibility.Visible;
                CB_ItemID.Visibility = Visibility.Hidden;
            }
        }

        private void CB_ItemID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
                if ((sender as ComboBox).SelectedIndex >= 0)
                    PopulateItemInfo();

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            //}
        }

        private void CB_Group_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePublishedSales();
        }

        private void TB_ListPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePublishedSales();
        }

        private void Clear()
        {
            foreach (UIElement child in this.Grid.Children)
            {
                if ((child as TextBox) != null)
                    (child as TextBox).Text = "";

                CB_ItemID.SelectedIndex = -1;
                CB_Group.SelectedIndex = -1;
            }
        }

        private void UpdatePublishedSales()
        {
            if (Values.Group == "")
                Values.Multiplier = "";
            else
            {
                if (Values.List_Price != "")
                {
                    Values.Multiplier = GroupTable.Select($"Group = '{Values.Group}'")[0][1].ToString();
                    decimal m = Convert.ToDecimal(Values.Multiplier);
                    decimal p = Convert.ToDecimal(Values.List_Price);
                    Values.Published_Sales = (m*p).ToString("0.00");
                }
            }
        }

        private void PopulateItemIDComboBox()
        {
            ItemIDTable = DBController.GetTableFromQuery(sqlconn: App.DBConnString, columns: "*", t1: "Items");
            CB_ItemID.ItemsSource = ItemIDTable.DefaultView;
        }

        private void PopulateGroupsComboBox()
        {
            GroupTable = DBController.GetTableFromQuery(sqlconn: App.DBConnString, columns: "*", t1: "Groups");
            CB_Group.ItemsSource = GroupTable.DefaultView;            
        }

        private void PopulateItemInfo()
        {
            selectedItem = DBController.GetTableFromQuery(sqlconn: App.DBConnString, columns: "*", t1: "Items", searchTableAlias: "Items", searchCol: "Inventory_Item", searchEqual: CB_ItemID.SelectedValue.ToString());

            Values.Inventory_Item = selectedItem.Rows[0]["Inventory_Item"].ToString();
            Values.Description = selectedItem.Rows[0]["Description"].ToString();
            Values.Group = selectedItem.Rows[0]["Group"].ToString();
            Values.List_Price = selectedItem.Rows[0]["List_Price"].ToString();
            Values.Multiplier = selectedItem.Rows[0]["Multiplier"].ToString();
            Values.Published_Cost = selectedItem.Rows[0]["Published_Cost"].ToString();
            Values.Published_Sales = selectedItem.Rows[0]["Published_Sales"].ToString();
            Values.Weight = selectedItem.Rows[0]["Weight"].ToString();
            Values.Notes = selectedItem.Rows[0]["Notes"].ToString();

        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void AddItemToDatabase()
        {
            ArrayList val = new ArrayList();

            foreach (PropertyInfo prop in Values.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                val.Add(prop.GetValue(Values));

            DBController.Insert(App.DBConnString, "Items", val);
        }

        private void UpdateItemInDatabase()
        {
            List<Tuple<string, string>> setValues = new List<Tuple<string, string>>();
            List<Tuple<string, string>> whereValues = new List<Tuple<string, string>>();
           
            foreach (PropertyInfo prop in Values.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                setValues.Add(new Tuple<string, string>(prop.Name, prop.GetValue(Values).ToString()));

            foreach (DataColumn col in selectedItem.Columns)
                whereValues.Add(new Tuple<string, string>(col.ColumnName, selectedItem.Rows[0][col].ToString()));

            if (DBController.Update(App.DBConnString, "Items", setValues, whereValues))
                MessageBox.Show($"Database Updated:\n\nItem {Values.Inventory_Item} updated successfully");
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            if (editMode)
                UpdateItemInDatabase();
            else
                AddItemToDatabase();
        }

            private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            Values.Notes += $"{date} -- {time} -- {TB_AddNote.Text}\n";
            TB_AddNote.Text = "";
        }
    }
}
