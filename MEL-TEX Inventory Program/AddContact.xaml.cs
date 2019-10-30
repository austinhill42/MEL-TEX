using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MELTEX.Database;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for AddContact.xaml
    /// </summary>
    public partial class AddContact : Page
    {
        #region Fields

        private string contactName;
        private string companyNumber;
        private string connstring;
        private string ContactType;
        private Page PreviousPage;
        internal List<Phone> Numbers;
        private bool edit;
        
        #endregion Fields

        #region Constructors

        public AddContact(Page prev, string type, string number, string name ="", bool edit = false)
        {
            InitializeComponent();

            contactName = name;
            PreviousPage = prev;
            ContactType = type;
            companyNumber = number;
            this.edit = edit;

            connstring = ContactType == "Customer" ? App.SalesDBConnString : App.PurchasingDBConnString;

            Numbers = new List<Phone>();
        }

        #endregion Constructors

        #region Methods

        private void AddNumber(Phone number)
        {
            if (!edit)
                Numbers.Add(number);

            SP_Phone.Children.Add(number);
        }

        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            TB_Notes.Text += $"{date} -- {time} -- {TB_AddNote.Text}\n";

            TB_AddNote.Text = "";
        }

        private void BTN_AddNumber_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(new Phone(this));
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = PreviousPage;
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void BTN_RemoveNumber_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < SP_Phone.Children.Count; i++)
                if (SP_Phone.Children[i] is Button && (SP_Phone.Children[i] as Button).Tag == (sender as Button).Tag)
                    SP_Phone.Children.RemoveAt(i);
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!edit)
                {
                    InsertContact();
                }
                else
                {
                    UpdateContact();

                    using (SqlConnection sql = new SqlConnection(connstring))
                    {
                        sql.Open();

                        string query = $"DELETE FROM {ContactType}_Contact_Phones WHERE Number = @num AND Name = @name";

                        using (SqlCommand cmd = new SqlCommand(query, sql))
                        {
                            cmd.Parameters.AddWithValue("@num", TB_Number.Text);
                            cmd.Parameters.AddWithValue("@name", TB_Name.Text);

                            cmd.ExecuteNonQuery();
                        }

                        query = $"DELETE FROM {ContactType}_Contact_Email WHERE Number = @num AND Name = @name";

                        using (SqlCommand cmd = new SqlCommand(query, sql))
                        {
                            cmd.Parameters.AddWithValue("@num", TB_Number.Text);
                            cmd.Parameters.AddWithValue("@name", TB_Name.Text);

                            cmd.ExecuteNonQuery();
                        }

                        query = $"DELETE FROM {ContactType}_Contact_Social_Media WHERE Number = @num AND Name = @name";

                        using (SqlCommand cmd = new SqlCommand(query, sql))
                        {
                            cmd.Parameters.AddWithValue("@num", TB_Number.Text);
                            cmd.Parameters.AddWithValue("@name", TB_Name.Text);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                Numbers.ForEach(n => InsertPhones(n.number, n.type));

                foreach (string email in TB_Email.Text.Split('\n'))
                    InsertEmail(email);

                foreach (string social in TB_Social.Text.Split('\n'))
                    InsertSocial(social);

                MessageBox.Show("Database Updated");

                Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong, make sure all fields are filled out.\n\n{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void Clear()
        {
            foreach (TextBox tb in FindVisualChildren<TextBox>(Grid))
                tb.Text = "";

            foreach (ComboBox cb in FindVisualChildren<ComboBox>(Grid))
                cb.SelectedIndex = -1;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = $"Add {ContactType} Contact";
            L_Number.Content = $"{ContactType} Name";

            TB_Number.Text = companyNumber;

            if (!edit)
                AddNumber(new Phone(this));
            else
            {
                DataTable contact = new DataTable();
                DataTable contactPhone = new DataTable();
                DataTable contactSocial = new DataTable();
                DataTable contactEmail = new DataTable();

                contact = DBController.GetTableFromQuery(connstring, "Title, Notes", $"{ContactType}_Contacts");
                contactPhone = DBController.GetTableFromQuery(connstring, "Phone_No, Type", $"{ContactType}_Contact_Phones");
                contactEmail = DBController.GetTableFromQuery(connstring, "Email", $"{ContactType}_Contact_Email");
                contactSocial = DBController.GetTableFromQuery(connstring, "Social_Media_Page", $"{ContactType}_Contact_Social_Media");

                TB_Name.Text = contactName;
                TB_Title.Text = contact.Rows[0]["Title"].ToString();
                TB_Notes.Text = contact.Rows[0]["Notes"].ToString();
                TB_Email.Text = string.Join("\n", contactEmail.Rows.OfType<DataRow>().Select(x => x.Field<string>("Email")).ToList());
                TB_Social.Text = string.Join("\n", contactSocial.Rows.OfType<DataRow>().Select(x => x.Field<string>("Social_Media_Page")).ToList());
                Numbers = contactPhone.AsEnumerable().Select(x => new Phone(this) { Number = x[0].ToString(), Type = x[1].ToString() }).ToList();
                Numbers.ForEach(x => Console.WriteLine(x.Number + "  " + x.Type));
                Numbers.ForEach(x => AddNumber(x));
            }
        }

        private void UpdateContact()
        {
            List<Tuple<string, string>> setValues = new List<Tuple<string, string>>();
            List<Tuple<string, string>> whereValues = new List<Tuple<string, string>>();

            setValues.Add(new Tuple<string, string>("Title", TB_Title.Text));
            setValues.Add(new Tuple<string, string>("Notes", TB_Notes.Text));
            whereValues.Add(new Tuple<string, string>("Number", TB_Number.Text));
            whereValues.Add(new Tuple<string, string>("Name", TB_Name.Text));

            DBController.Update(connstring, $"{ContactType}_Contacts", setValues, whereValues);
 
        }

        private void InsertContact()
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
                        cmd.Parameters.AddWithValue("@num", TB_Number.Text);
                        cmd.Parameters.AddWithValue("@name", TB_Name.Text);
                        cmd.Parameters.AddWithValue("@title", TB_Title.Text);
                        cmd.Parameters.AddWithValue("@notes", TB_Notes.Text);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void InsertEmail(string email)
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
                        cmd.Parameters.AddWithValue("@num", TB_Number.Text);
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

        private void InsertPhones(string num, string type)
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
                        cmd.Parameters.AddWithValue("@num", TB_Number.Text);
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

        private void InsertSocial(string social)
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
                        cmd.Parameters.AddWithValue("@num", TB_Number.Text);
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

        #endregion Methods
    }
}