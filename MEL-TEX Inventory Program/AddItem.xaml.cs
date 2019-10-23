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
        protected _ItemValues Values { get; set; }

        protected class _ItemValues : INotifyPropertyChanged
        {

            private string _ItemID;
            private string _Description;
            private string _Weight;
            private string _ListPrice;
            private string _Group;
            private string _Multiplier;
            private string _PubSale;
            private string _PubCost;
            private string _Notes;
            private string _NewNote;

            public string ItemID { get { return _ItemID; } set { _ItemID = value; OnPropertyChanged("ItemID"); } }
            public string Description { get { return _Description; } set { _Description = value; OnPropertyChanged("Description"); } }
            public string Weight { get { return _Weight; } set { _Weight = value; OnPropertyChanged("Weight"); } }
            public string ListPrice { get { return _ListPrice; } set { _ListPrice = value; OnPropertyChanged("ListPrice"); } }
            public string Group { get { return _Group; } set { _Group = value; OnPropertyChanged("Group"); } }
            public string Multiplier { get { return _Multiplier; } set { _Multiplier = value; OnPropertyChanged("Multiplier"); } }
            public string PubSale { get { return _PubSale; } set { _PubSale = value; OnPropertyChanged("PubSale"); } }
            public string PubCost { get { return _PubCost; } set { _PubCost = value; OnPropertyChanged("PubCost"); } }
            public string Notes { get { return _Notes; } set { _Notes += value; OnPropertyChanged("Notes"); } }
            public string NewNote { get { return _NewNote; } set { _NewNote = value; OnPropertyChanged("NewNote"); } }

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

            Values.ItemID = "123456";
        }

        private void CB_ItemID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as ComboBox).SelectedIndex >= 0)
                    PopulateItemInfo();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
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
            if (CB_Group.SelectedIndex == -1)
                TB_Mult.Text = "";
            else
            {
                if (TB_ListPrice.Text != "")
                {
                    TB_Mult.Text = CB_Group.SelectedValue.ToString();
                    decimal m = Convert.ToDecimal(TB_Mult.Text);
                    decimal p = Convert.ToDecimal(TB_ListPrice.Text);
                    TB_PublishedSales.Text = (m*p).ToString("0.00");
                }
            }
        }

        private void PopulateItemIDComboBox()
        {
            DataTable table = new DataTable();
            try
            {
                using (SqlConnection sql = new SqlConnection(App.DBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT * FROM Items";

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(table);

                    CB_ItemID.DataContext = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PopulateGroupsComboBox()
        {
            string query = "SELECT * FROM Groups";

            using (SqlConnection sql = new SqlConnection(App.DBConnString))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();
                com.CommandText = query;

                com.ExecuteNonQuery();

                SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                {
                    SelectCommand = com
                };

                DataTable table = new DataTable();

                adapter.Fill(table);
                CB_Group.ItemsSource = table.DefaultView;
            }

        }

        private void PopulateItemInfo()
        {
            selectedItem = new DataTable();

            try
            {
                using (SqlConnection sql = new SqlConnection(App.DBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT * FROM Items WHERE Inventory_Item = @item";

                    com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(selectedItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            TB_Desc.Text = selectedItem.Rows[0]["Description"].ToString();
            CB_Group.SelectedItem = selectedItem.Rows[0]["Group"].ToString();
            TB_ListPrice.Text = selectedItem.Rows[0]["List_Price"].ToString();
            TB_Mult.Text = selectedItem.Rows[0]["Multiplier"].ToString();
            TB_PublishedCost.Text = selectedItem.Rows[0]["Published_Cost"].ToString();
            TB_PublishedSales.Text = selectedItem.Rows[0]["Published_Sales"].ToString();
            TB_Weight.Text = selectedItem.Rows[0]["Weight"].ToString();
            TB_Notes.Text = selectedItem.Rows[0]["Notes"].ToString();

        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void UpdateItemInDatabase()
        {
            try
            {
                string query = "UPDATE Items " +
                    "SET [Description] = @desc, [Weight] = @weight, [List_Price] = @list, [Group] = @group, [Multiplier] = @mult, [Published_Sales] = @pubSale, [Published_Cost] = @pubCost, [Notes] = @notes " +
                    "WHERE [Inventory_Item] = @item AND [Description] = @oldDesc AND [Weight] = @oldWeight AND [List_Price] = @oldList AND [Group] = @oldGroup AND [Multiplier] = @oldMult AND [Published_Sales] = @oldPubSale AND [Published_Cost] = @oldPubCost AND [Notes] = @oldNotes ";

                using (SqlConnection sql = new SqlConnection(App.DBConnString))
                {
                    sql.Open();

                    using (SqlCommand cmd = new SqlCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString()); ;
                        cmd.Parameters.AddWithValue("@desc", TB_Desc.Text);
                        cmd.Parameters.AddWithValue("@weight", Convert.ToDecimal(TB_Weight.Text));
                        cmd.Parameters.AddWithValue("@list", Convert.ToDecimal(TB_ListPrice.Text));
                        cmd.Parameters.AddWithValue("@group", ((DataRowView)CB_Group.SelectedItem).Row[0].ToString());
                        cmd.Parameters.AddWithValue("@mult", Convert.ToDecimal(TB_Mult.Text));
                        cmd.Parameters.AddWithValue("@pubSale", Convert.ToDecimal(TB_PublishedSales.Text));
                        cmd.Parameters.AddWithValue("@pubCost", Convert.ToDecimal(TB_PublishedCost.Text));
                        cmd.Parameters.AddWithValue("@notes", TB_Notes.Text);

                        cmd.Parameters.AddWithValue("@oldDesc", selectedItem.Rows[0]["Description"].ToString());
                        cmd.Parameters.AddWithValue("@oldWeight", Convert.ToDecimal(selectedItem.Rows[0]["Weight"].ToString()));
                        cmd.Parameters.AddWithValue("@oldList", Convert.ToDecimal(selectedItem.Rows[0]["List_Price"].ToString()));
                        cmd.Parameters.AddWithValue("@oldGroup", selectedItem.Rows[0]["Group"].ToString());
                        cmd.Parameters.AddWithValue("@oldMult", Convert.ToDecimal(selectedItem.Rows[0]["Multiplier"].ToString()));
                        cmd.Parameters.AddWithValue("@oldPubSale", Convert.ToDecimal(selectedItem.Rows[0]["Published_Sales"].ToString()));
                        cmd.Parameters.AddWithValue("@oldPubCost", Convert.ToDecimal(selectedItem.Rows[0]["Published_Cost"].ToString()));
                        cmd.Parameters.AddWithValue("@oldNotes", selectedItem.Rows[0]["Notes"].ToString());


                        cmd.ExecuteNonQuery();
                    }
                }

                Clear();

                MessageBox.Show("Database Updated");
            }
            catch (Exception ex)
            {
                if (ex is System.FormatException)
                    MessageBox.Show("One of the fields was entered with an incorrect format...\n\nOr a required value was left black.\n\nCheck your values and try again.");               
                else
                    MessageBox.Show(ex.Message);
            }

            MessageBox.Show($"Item {CB_ItemID.SelectedValue.ToString()} updated successfully");
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            ArrayList val = new ArrayList();

            foreach (PropertyInfo prop in Values.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (prop.Name != "NewNote")
                    val.Add(prop.GetValue(Values));

            DBController.DBController.Insert(App.DBConnString, "Items", val);
        }

            private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            //TB_Notes.Text += $"{date} -- {time} -- {TB_AddNote.Text}\n";
            Values.Notes = $"{date} -- {time} -- {Values.NewNote}\n";
            Values.NewNote = "";

            //TB_AddNote.Text = "";
        }
    }
}
