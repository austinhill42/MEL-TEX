using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for OpenQuote.xaml
    /// </summary>
    public partial class OpenQuote : Window
    {
        #region Fields

        internal string quotenum;

        #endregion Fields

        #region Constructors

        public OpenQuote()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BTN_Open_Click(object sender, RoutedEventArgs e)
        {
            quotenum = CB_Open.SelectedValue.ToString();
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection sql = new SqlConnection(App.SalesDBConnString))
                {
                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT Number FROM Quotes";

                    com.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(com.CommandText, sql)
                    {
                        SelectCommand = com
                    };

                    adapter.Fill(table);

                    CB_Open.DataContext = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion Methods
    }
}