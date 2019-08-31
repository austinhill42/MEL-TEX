using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
        private static readonly string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        private readonly string connString = $"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {loc}MEL-TEXDB.mdf; Integrated Security = True; Connect Timeout = 30";
        private bool editItem = false;
        private DataTable items;

        public AddItem(Page prev)
        {
            InitializeComponent();

            previousPage = prev;

            items = new DataTable();
        }

        public AddItem(Page prev, bool edit) : this(prev)
        {
            editItem = edit;


        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateGroupsComboBox();

            if (editItem)
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
                    TB_PublishedSales.Text = Convert.ToString(m * p);
                }
            }
        }

        private void PopulateItemIDComboBox()
        {
            DataTable table = new DataTable();
            try
            {
                using (SqlConnection sql = new SqlConnection(connString))
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

            using (SqlConnection sql = new SqlConnection(connString))
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
                CB_Group.DataContext = table.DefaultView;
            }

        }

        private void PopulateItemInfo()
        {
            items = new DataTable();

            try
            {
                using (SqlConnection sql = new SqlConnection(connString))
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

                    adapter.Fill(items);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            TB_Desc.Text = items.Rows[0]["Description"].ToString();
            CB_Group.SelectedItem = items.Rows[0]["Group"].ToString();
            TB_ListPrice.Text = items.Rows[0]["List_Price"].ToString();
            TB_Mult.Text = items.Rows[0]["Multiplier"].ToString();
            TB_PublishedCost.Text = items.Rows[0]["Published_Cost"].ToString();
            TB_PublishedSales.Text = items.Rows[0]["Published_Sales"].ToString();
            TB_Weight.Text = items.Rows[0]["Weight"].ToString();
            TB_Notes.Text = items.Rows[0]["Notes"].ToString();

        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void AddItemToDatabase()
        {
            try
            {
                string query = "INSERT INTO Items ([Inventory_Item], [Description], [Weight], [List_Price], [Group], [Multiplier], [Published_Sales], [Published_Cost], [Notes]) " +
                               "VALUES (@item,@description,@weight,@listprice,@group,@multiplier,@sales,@cost,@notes) ";

                using (SqlConnection sql = new SqlConnection(connString))
                {
                    sql.Open();

                    using (SqlCommand cmd = new SqlCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("@item", TB_ItemID.Text);
                        cmd.Parameters.AddWithValue("@description", TB_Desc.Text);
                        cmd.Parameters.AddWithValue("@weight", Convert.ToDecimal(TB_Weight.Text));
                        cmd.Parameters.AddWithValue("@listprice", Convert.ToDecimal(TB_ListPrice.Text));
                        cmd.Parameters.AddWithValue("@group", ((DataRowView)CB_Group.SelectedItem).Row[0].ToString());
                        cmd.Parameters.AddWithValue("@multiplier", Convert.ToDecimal(TB_Mult.Text));
                        cmd.Parameters.AddWithValue("@sales", Convert.ToDecimal(TB_PublishedSales.Text));
                        cmd.Parameters.AddWithValue("@cost", Convert.ToDecimal(TB_PublishedCost.Text));
                        cmd.Parameters.AddWithValue("@notes", TB_Notes.Text);

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
                else if (ex is SqlException)
                {
                    if (ex.Message.Contains("duplicate"))
                        MessageBox.Show("The item already exists in the database.");

                    else
                        MessageBox.Show(ex.Message);
                }
                else
                    MessageBox.Show(ex.Message);
            }
        }

        private void UpdateItemInDatabase()
        {
            try
            {
                string query = "UPDATE Items " +
                    "SET [Description] = @desc, [Weight] = @weight, [List_Price] = @list, [Group] = @group, [Multiplier] = @mult, [Published_Sales] = @pubSale, [Published_Cost] = @pubCost, [Notes] = @notes " +
                    "WHERE [Inventory_Item] = @item AND [Description] = @oldDesc AND [Weight] = @oldWeight AND [List_Price] = @oldList AND [Group] = @oldGroup AND [Multiplier] = @oldMult AND [Published_Sales] = @oldPubSale AND [Published_Cost] = @oldPubCost AND [Notes] = @oldNotes ";

                using (SqlConnection sql = new SqlConnection(connString))
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
                        cmd.Parameters.AddWithValue("@oldDesc", items.Rows[0]["Description"].ToString());
                        cmd.Parameters.AddWithValue("@oldWeight", Convert.ToDecimal(items.Rows[0]["Weight"].ToString()));
                        cmd.Parameters.AddWithValue("@oldList", Convert.ToDecimal(items.Rows[0]["List_Price"].ToString()));
                        cmd.Parameters.AddWithValue("@oldGroup", items.Rows[0]["Group"].ToString());
                        cmd.Parameters.AddWithValue("@oldMult", Convert.ToDecimal(items.Rows[0]["Multiplier"].ToString()));
                        cmd.Parameters.AddWithValue("@oldPubSale", Convert.ToDecimal(items.Rows[0]["Published_Sales"].ToString()));
                        cmd.Parameters.AddWithValue("@oldPubCost", Convert.ToDecimal(items.Rows[0]["Published_Cost"].ToString()));
                        cmd.Parameters.AddWithValue("@oldNotes", items.Rows[0]["Notes"].ToString());

                        

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
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            if (editItem)
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

            TB_Notes.Text += $"{date} -- {time} -- {TB_AddNote.Text}\n";

            TB_AddNote.Text = "";
        }
    }
}
