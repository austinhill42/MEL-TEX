using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for EditGroups.xaml
    /// </summary>
    public partial class EditGroups : Page
    {
        #region Fields

        private Page previousPage;

        #endregion Fields

        #region Constructors

        public EditGroups(Page prev)
        {
            InitializeComponent();

            previousPage = prev;
        }

        #endregion Constructors

        #region Methods

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            string group = CB_Groups.Text;
            decimal mult = Convert.ToDecimal(TB_Mult.Text);
            string updateGroup = $"UPDATE Groups SET Multiplier = @mult WHERE [Group] = @group ";
            string updateItems = "UPDATE Items SET Multiplier = @mult WHERE [Group] = @group";

            try
            {
                using (SqlConnection sql = new SqlConnection(App.DBConnString))
                {
                    sql.Open();

                    using (SqlCommand cmd = new SqlCommand(updateGroup, sql))
                    {
                        cmd.Parameters.AddWithValue("@group", group);
                        cmd.Parameters.AddWithValue("@mult", mult);

                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand(updateItems, sql))
                    {
                        cmd.Parameters.AddWithValue("@group", group);
                        cmd.Parameters.AddWithValue("@mult", mult);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong with the update:\n\n" + ex.Message);
            }

            MessageBox.Show("Update successful. Groups and item listings now updated.");

            CB_Groups.SelectedIndex = -1;
        }

        private void CB_Groups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selected = CB_Groups.SelectedValue;

            TB_Mult.Text = selected == null ? "" : selected.ToString();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Edit Groups";

            UpdateGroupsComboBox();
        }

        private void UpdateGroupsComboBox()
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
                CB_Groups.DataContext = table.DefaultView;
            }
        }

        #endregion Methods
    }
}