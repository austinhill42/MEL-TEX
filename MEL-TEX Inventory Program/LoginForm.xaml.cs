using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        #region Fields

        internal bool attempted;
        internal bool correctLogin;

        #endregion Fields

        #region Constructors

        public LoginForm()
        {
            InitializeComponent();

            correctLogin = false;
            attempted = false;
        }

        #endregion Constructors

        #region Methods

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BTN_Login_Click(object sender, RoutedEventArgs e)
        {
            CheckPassword();
            attempted = true;

            this.Close();
        }

        private void CB_Users_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TB_Password.Focus();
        }

        private void CheckPassword()
        {
            string query = "SELECT * from Passwords WHERE [Username] = @user";

            try
            {
                using (SqlConnection sql = new SqlConnection(App.PWDDBConnString))
                {
                    sql.Open();

                    using (SqlCommand cmd = new SqlCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("@user", CB_Users.Text);

                        cmd.ExecuteNonQuery();

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd.CommandText, sql)
                        {
                            SelectCommand = cmd
                        };

                        DataTable table = new DataTable();

                        adapter.Fill(table);

                        sql.Close();

                        if (table.Rows.Count == 1)
                        {
                            string savedPasswordHash = table.Rows[0][1].ToString();
                            byte[] savedHash = Convert.FromBase64String(savedPasswordHash);
                            byte[] salt = new byte[16];
                            byte[] enteredHash;

                            Array.Copy(savedHash, 0, salt, 0, 16);
                            enteredHash = (new Rfc2898DeriveBytes(TB_Password.Password, salt, 10000)).GetBytes(20);

                            correctLogin = true;

                            for (int i = 0; i < 20; i++)
                                if (savedHash[i + 16] != enteredHash[i])
                                    correctLogin = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong:\n\n{ex.Message}");
            }
        }

        private void UpdateUsersComboBox()
        {
            string query = "SELECT * FROM Passwords";

            using (SqlConnection sql = new SqlConnection(App.PWDDBConnString))
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
                CB_Users.DataContext = table.DefaultView;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUsersComboBox();

            CB_Users.SelectedIndex = 0;
        }

        #endregion Methods
    }
}