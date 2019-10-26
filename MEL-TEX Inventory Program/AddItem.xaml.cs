using MELTEX.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for AddItem.xaml
    /// </summary>
    public partial class AddItem : Page, INotifyPropertyChanged
    {
        private Page previousPage;
        private bool _Edit = false;
        private DataTable selectedItem;
        private DataTable _ItemIDTable;
        private DataTable _GroupTable;

        public bool Edit { get => _Edit; set { _Edit = value; OnPropertyChanged("Edit"); } }
        public DataTable GroupTable { get => _GroupTable; set { _GroupTable = value; OnPropertyChanged("GroupTable"); } }
        public DataTable ItemIDTable { get => _ItemIDTable; set { _ItemIDTable = value; OnPropertyChanged("ItemIDTable"); } }
        internal MELTEXDB.Items Items { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public AddItem(Page prev, bool edit = false)
        {
            InitializeComponent();

            previousPage = prev;

            selectedItem = new DataTable();

            Items = new MELTEXDB.Items();

            DataContext = Items;

            Edit = edit;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateGroupsComboBox();
            this.WindowTitle = "Add Item";

            if (Edit)
            {
                WindowTitle = "Edit Item";

                PopulateItemIDComboBox();
            }
        }

        private void CB_ItemID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex >= 0)
                PopulateItemInfo();
        }

        private void CB_Group_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePublishedSales();

        private void TB_ListPrice_TextChanged(object sender, TextChangedEventArgs e) => UpdatePublishedSales();

        private void Clear()
        {
            foreach (UIElement child in Grid.Children)
            {
                if ((child as TextBox) != null)
                    (child as TextBox).Text = "";

                Items.Inventory_Item = "";
                Items.Group = "";
            }
        }

        private void UpdatePublishedSales()
        {
            if (Items.Group == "")
                Items.Multiplier = "";
            else
            {
                if (Items.List_Price != "")
                {
                    Items.Multiplier = GroupTable.Select($"Group = '{Items.Group}'")[0][1].ToString();
                    decimal m = Convert.ToDecimal(Items.Multiplier);
                    decimal p = Convert.ToDecimal(Items.List_Price);
                    Items.Published_Sales = (m * p).ToString("0.00");
                }
            }
        }

        private void PopulateItemIDComboBox() => ItemIDTable = DBController.GetTableFromQuery(sqlconn: App.DBConnString, columns: "*", t1: "Items");

        private void PopulateGroupsComboBox() => GroupTable = DBController.GetTableFromQuery(sqlconn: App.DBConnString, columns: "*", t1: "Groups");

        private void PopulateItemInfo()
        {
            selectedItem = DBController.GetTableFromQuery(sqlconn: App.DBConnString, columns: "*", t1: "Items", searchTableAlias: "Items", searchCol: "Inventory_Item", searchEqual: CB_ItemID.SelectedValue.ToString());

            Items.Inventory_Item = selectedItem.Rows[0]["Inventory_Item"].ToString();
            Items.Description = selectedItem.Rows[0]["Description"].ToString();
            Items.Group = selectedItem.Rows[0]["Group"].ToString();
            Items.List_Price = selectedItem.Rows[0]["List_Price"].ToString();
            Items.Multiplier = selectedItem.Rows[0]["Multiplier"].ToString();
            Items.Published_Cost = selectedItem.Rows[0]["Published_Cost"].ToString();
            Items.Published_Sales = selectedItem.Rows[0]["Published_Sales"].ToString();
            Items.Weight = selectedItem.Rows[0]["Weight"].ToString();
            Items.Notes = selectedItem.Rows[0]["Notes"].ToString();
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e) => Clear();

        private void AddItemToDatabase()
        {
            ArrayList val = new ArrayList();

            foreach (PropertyInfo prop in Items.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                val.Add(prop.GetValue(Items));

            DBController.Insert(App.DBConnString, "Items", val);
        }

        private void UpdateItemInDatabase()
        {
            List<Tuple<string, string>> setValues = new List<Tuple<string, string>>();
            List<Tuple<string, string>> whereValues = new List<Tuple<string, string>>();

            setValues.AddRange(Items.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(prop => new Tuple<string, string>(prop.Name, prop.GetValue(Items).ToString())));
            whereValues.AddRange(selectedItem.Columns.Cast<DataColumn>().Select(col => new Tuple<string, string>(col.ColumnName, selectedItem.Rows[0][col].ToString())));

            if (DBController.Update(App.DBConnString, "Items", setValues, whereValues))
                MessageBox.Show($"Database Updated:\n\nItem {Items.Inventory_Item} updated successfully");
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            if (Edit)
                UpdateItemInDatabase();
            else
                AddItemToDatabase();
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e) => MainWindow.GetWindow(this).Content = previousPage;

        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            Items.Notes += $"{date} -- {time} -- {TB_AddNote.Text}\n";
            TB_AddNote.Text = "";
        }
    }
}