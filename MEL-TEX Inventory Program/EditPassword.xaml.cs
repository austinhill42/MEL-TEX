using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for EditPassword.xaml
    /// </summary>

    public partial class EditPassword : Page
    {
        #region Fields

        private bool correctLogin;
        private Page previousPage;

        #endregion Fields

        #region Constructors

        public EditPassword(Page prev)
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
            if (TB_Password.Password == "")
            {
                MessageBox.Show("Password can't be blank");
                return;
            }

            if (TB_Password.Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters");
                return;
            }

            if (TB_Password.Password != TB_ConfirmPassword.Password)
            {
                MessageBox.Show("Passwords do not match");
                return;
            }

            byte[] salt;
            byte[] hash;
            byte[] hashedBytes = new byte[36];
            string savedPasswordHash;
            string query = "UPDATE Passwords SET Password = @pass WHERE [Username] = @user ";

            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            hash = (new Rfc2898DeriveBytes(TB_Password.Password, salt, 10000)).GetBytes(20);

            Array.Copy(salt, 0, hashedBytes, 0, 16);
            Array.Copy(hash, 0, hashedBytes, 16, 20);

            savedPasswordHash = Convert.ToBase64String(hashedBytes);

            try
            {
                using (SqlConnection sql = new SqlConnection(App.PWDDBConnString))
                {
                    sql.Open();

                    using (SqlCommand cmd = new SqlCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("@user", CB_Users.Text);
                        cmd.Parameters.AddWithValue("@pass", savedPasswordHash);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong:\n\n{ex.Message}");
            }

            MessageBox.Show("Password Updated");

            CB_Users.SelectedIndex = -1;
            TB_Password.Password = "";
            TB_ConfirmPassword.Password = "";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Change Password";

            UpdateUsersComboBox();
        }

        private void TB_ConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (TB_Password.Password != TB_ConfirmPassword.Password)
                L_ComparePasswords.Content = "Passwords do not match";
            else
                L_ComparePasswords.Content = "";
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

        #endregion Methods
    }
}