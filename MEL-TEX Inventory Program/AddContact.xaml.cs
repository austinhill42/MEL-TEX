using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for AddContact.xaml
    /// </summary>
    public partial class AddContact : Page
    {
        private string connstring;
        private Page PreviousPage;
        private string ContactType;
        public ObservableCollection<Phone> Numbers { get; set; }

        public AddContact(Page prev, string type)
        {
            InitializeComponent();

            PreviousPage = prev;
            ContactType = type;

            connstring = ContactType == "Customer" ? App.SalesDBConnString : App.PurchasingDBConnString;

            Numbers = new ObservableCollection<Phone>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = $"Add {ContactType} Contact";
            L_Number.Content = $"{ContactType} Name";

            PopulateNumberComboBox();
            AddNumber(new Phone());
        }

        private void PopulateNumberComboBox()
        {
            DataTable table = new DataTable();
            try
            {
                using (SqlConnection sql = new SqlConnection(connstring))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = $"SELECT Number Name FROM {ContactType}";

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(table);

                    CB_Number.DataContext = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateContact()
        {
            try
            {
                using (SqlConnection sql = new SqlConnection(connstring))
                {
                    sql.Open();

                    string query = $"INSERT INTO {ContactType}_Contacts ([Number], [Name], [Title], [Notes]) " +
                        $"VALUES (@num,@name,@title,@notes)";

                    using (SqlCommand cmd = new SqlCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("@num", CB_Number.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@name", TB_Name.Text);
                        cmd.Parameters.AddWithValue("@title", TB_Title.Text);
                        cmd.Parameters.AddWithValue("@notes", TB_Notes.Text);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private void UpdatePhones(string num, string type)
        {
            try
            {
                using (SqlConnection sql = new SqlConnection(connstring))
                {
                    sql.Open();

                    string query = $"INSERT INTO {ContactType}_Contact_Phones ([Number], [Name], [Phone_No], [Type]) " +
                        $"VALUES (@num,@name,@phone,@type)";

                    using (SqlCommand cmd = new SqlCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("@num", CB_Number.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@name", TB_Name.Text);
                        cmd.Parameters.AddWithValue("@phone", num);
                        cmd.Parameters.AddWithValue("@type", type);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void UpdateEmail(string email)
        {
            try
            {
                using (SqlConnection sql = new SqlConnection(connstring))
                {
                    sql.Open();

                    string query = $"INSERT INTO {ContactType}_Contact_Email ([Number], [Name], [Email]) " +
                        $"VALUES (@num,@name,@email)";

                    using (SqlCommand cmd = new SqlCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("@num", CB_Number.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@name", TB_Name.Text);
                        cmd.Parameters.AddWithValue("@email", email);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void UpdateSocial(string social)
        {
            try
            {
                using (SqlConnection sql = new SqlConnection(connstring))
                {
                    sql.Open();

                    string query = $"INSERT INTO {ContactType}_Contact_Social_Media ([Number], [Name], [Social_Media_Page]) " +
                        $"VALUES (@num,@name,@social)";

                    using (SqlCommand cmd = new SqlCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("@num", CB_Number.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@name", TB_Name.Text);
                        cmd.Parameters.AddWithValue("@social", social);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void AddNumber(Phone number)
        {
            Numbers.Add(number);

            RowDefinition rowDef = new RowDefinition
            {
                Height = new GridLength(50)
            };

            Grid.RowDefinitions.Insert(Grid.GetRow(SP_Email), rowDef);
            Grid.Children.Add(number);
            Grid.SetRow(number, Grid.GetRow(SP_Email) - 1);
            Grid.SetRow(SP_Email, Grid.GetRow(SP_Email) + 1);
            Grid.SetRow(SP_Website, Grid.GetRow(SP_Website) + 1);
            Grid.SetRow(SP_Social, Grid.GetRow(SP_Social) + 1);
            Grid.SetRow(G_Notes, Grid.GetRow(G_Notes) + 1);
            Grid.SetRow(BTN_AddNumber, Grid.GetRow(BTN_AddNumber) + 1);
            Grid.SetRow(BTN_RemoveNumber, Grid.GetRow(BTN_RemoveNumber) + 1);
            Grid.SetColumn(number, 1);
            Grid.SetColumnSpan(number, 2);
        }

        private void Clear()
        {
            foreach (TextBox tb in FindVisualChildren<TextBox>(Grid))
                tb.Text = "";

            foreach (ComboBox cb in FindVisualChildren<ComboBox>(Grid))
                cb.SelectedIndex = -1;
        }

        private void BTN_RemoveNumber_Click(object sender, RoutedEventArgs e)
        {
            Grid.Children.Remove(Numbers[Numbers.Count - 1]);
            Grid.RowDefinitions.RemoveAt(Grid.GetRow(G_Notes) - 1);
            Numbers.RemoveAt(Numbers.Count - 1);

            Grid.SetRow(BTN_AddNumber, Numbers.Count + 1);
            Grid.SetRow(BTN_RemoveNumber, Numbers.Count);
            Grid.SetRow(SP_Email, Grid.GetRow(SP_Email) - 1);
            Grid.SetRow(SP_Website, Grid.GetRow(SP_Website) - 1);
            Grid.SetRow(SP_Social, Grid.GetRow(SP_Social) - 1);
            Grid.SetRow(G_Notes, Grid.GetRow(G_Notes) - 1);
        }

        private void BTN_AddNumber_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(new Phone());

            ScrollView.ScrollToBottom();
        }

        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            TB_Notes.Text += $"{date} -- {time} -- {TB_AddNote.Text}\n";

            TB_AddNote.Text = "";
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = PreviousPage;
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateContact();

                foreach (Phone ph in Numbers)
                    UpdatePhones(ph.number, ph.type);

                foreach (string email in TB_Email.Text.Split('\n'))
                    UpdateEmail(email);

                foreach (string social in TB_Social.Text.Split('\n'))
                    UpdateSocial(social);

                MessageBox.Show("Database Updated");

                Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong, make sure all fields are filled out.\n\n{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        // Get all the textboxes so I can clear them, since some are deeply nested
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
