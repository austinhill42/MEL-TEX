using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for DeleteItem.xaml
    /// </summary>
    public partial class DeleteItem : Page
    {
        private Page previousPage;

        public DeleteItem(Page prev)
        {
            InitializeComponent();

            previousPage = prev;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Delete Inventory Item";

            PopulateItemIDComboBox();
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

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

        private void BTN_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (CB_ItemID.SelectedIndex < 0)
            {
                MessageBox.Show("You must select an item to delete first.");
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete item: {CB_ItemID.SelectedValue.ToString()}?\n\nThis will also remvove all inbounded {CB_ItemID.SelectedValue.ToString()} items.", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;


            try
            {
                using (SqlConnection sql = new SqlConnection(App.DBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "DELETE FROM Items WHERE Inventory_Item = @item ";

                    com.Parameters.AddWithValue("@item", CB_ItemID.SelectedValue.ToString());

                    com.ExecuteNonQuery();


                    com.CommandText = "DELETE FROM Inventory WHERE Inventory_Item = @item ";

                    com.ExecuteNonQuery();
                }

                MessageBox.Show($"Item: {CB_ItemID.SelectedValue.ToString()} successfully deleted");

                CB_ItemID.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }
    }
}
