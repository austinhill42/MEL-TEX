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

        public AddItem(Page prev)
        {
            InitializeComponent();

            previousPage = prev;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGroupsComboBox();
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
                else if ((child as ComboBox) != null)
                    (child as ComboBox).SelectedItem = -1;
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

        private void UpdateItemsDatabase(String str)
        {
            using (SqlConnection sql = new SqlConnection(connString))
            {
                sql.Open();

                using (SqlCommand cmd = new SqlCommand(str, sql))
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
        }

        private void UpdateGroupsComboBox()
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

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateItemsDatabase("INSERT INTO Items ([Inventory_Item], [Description], [Weight], [List_Price], [Group], [Multiplier], [Published_Sales], [Published_Cost], [Notes]) " +
                               "VALUES (@item,@description,@weight,@listprice,@group,@multiplier,@sales,@cost,@notes) ");

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
