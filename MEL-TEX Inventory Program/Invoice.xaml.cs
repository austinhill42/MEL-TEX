using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for AP.xaml
    /// </summary>
    public partial class Invoice : Page
    {
        private readonly string connString = App.AccountingDBConnString;
        private Page previousPage;

        public Invoice(Page prev)
        {
            InitializeComponent();

            previousPage = prev;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitle = "Invoice";
        }

        private void CB_Paid_Checked(object sender, RoutedEventArgs e)
        {
            TB_CheckNo.IsReadOnly = false;
            TB_Bank.IsReadOnly = false;
            TB_Confirmation.IsReadOnly = false;
        }

        private void CB_Paid_Unchecked(object sender, RoutedEventArgs e)
        {
            TB_CheckNo.IsReadOnly = true;
            TB_Bank.IsReadOnly = true;
            TB_Confirmation.IsReadOnly = true;
        }

        private void Clear()
        {
            foreach (TextBox tb in FindVisualChildren<TextBox>(Grid))
                tb.Text = "";

            foreach (DatePicker tb in FindVisualChildren<DatePicker>(Grid))
                tb.SelectedDate = null;

            foreach (CheckBox tb in FindVisualChildren<CheckBox>(Grid))
                tb.IsChecked = false;
        }

        private string GetNum()
        {
            int prevdate = 0;
            int prevNum = 0;

            using (SqlConnection sql = new SqlConnection(connString))
            {
                sql.Open();
                SqlCommand com = sql.CreateCommand();
                com.CommandText = "SELECT InvoiceNo FROM Invoice";

                DateTime now = DateTime.Now;

                int today = Convert.ToInt32(now.ToString("yyyy") + now.ToString("MM") + now.ToString("dd"));

                SqlDataReader reader = com.ExecuteReader();

                while (reader.Read())
                {
                    string read = reader.GetValue(0).ToString();
                    int testDate = Convert.ToInt32(read.Split('-')[0]);
                    int testNum = Convert.ToInt32(read.Split('-')[1]);
                    prevdate = prevdate > testDate ? prevdate : testDate;
                    prevNum = prevNum > testNum ? prevNum : testNum;
                }

                reader.Close();

                if (today > prevdate)
                    return $"{today.ToString()}-1";
                else
                    return $"{today}-{++prevNum}";
            }
        }

        private void BTN_Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Content = previousPage;
        }

        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection sql = new SqlConnection(connString))
                {
                    string query = "INSERT INTO Invoice ([InvoiceNo], [AccountNo], [Name], [DateReceived], [DateDue], [Acknowledged], [Paid], [CheckNo], [Bank], [Confirmation], [Notes]) " +
                               "VALUES (@num,@account,@name,@daterec,@due,@ack,@paid,@check,@bank,@confirm,@notes)";

                    sql.Open();
                    SqlCommand com = sql.CreateCommand();
                    com.CommandText = "SELECT InvoiceNo from Invoice WHERE InvoiceNo = @num";

                    com.Parameters.AddWithValue("@num", TB_Invoice.Text);

                    SqlDataReader reader = com.ExecuteReader();

                    com.Parameters.RemoveAt("@num");

                    if (reader.HasRows)
                    {
                        MessageBoxResult result = MessageBox.Show("Invoice already exists, do you want to overwrite?", "Invoice Exists", MessageBoxButton.YesNoCancel);

                        if (result != MessageBoxResult.Yes)
                            return;

                        query = "UPDATE Invoice " +
                               "SET [InvoiceNo] = @num, [AccountNo] = @account, [Name] = @name, [DateReceived] = @daterec, [DateDue] = @due, [Acknowledged] = @ack, [Paid] = @paid, [CheckNo] = @check, [Bank] = @bank, [Confirmation] = @confirm, [Notes] = @notes " +
                               "WHERE [InvoiceNo] = @num";
                    }

                    reader.Close();

                    com.CommandText = query;

                    com.Parameters.AddWithValue("@num", TB_Invoice.Text);
                    com.Parameters.AddWithValue("@account", TB_Account.Text);
                    com.Parameters.AddWithValue("@name", TB_Name.Text);
                    com.Parameters.AddWithValue("@daterec", DP_DateReceived.SelectedDate);
                    com.Parameters.AddWithValue("@due", DP_DateDue.SelectedDate);
                    com.Parameters.AddWithValue("@ack", CB_Ack.IsChecked);
                    com.Parameters.AddWithValue("@paid", CB_Paid.IsChecked);
                    com.Parameters.AddWithValue("@check", TB_CheckNo.Text);
                    com.Parameters.AddWithValue("@bank", TB_Bank.Text);
                    com.Parameters.AddWithValue("@confirm", TB_Confirmation.Text);
                    com.Parameters.AddWithValue("@notes", TB_Notes.Text);

                    com.ExecuteNonQuery();

                    MessageBox.Show($"Invoice: {TB_Invoice.Text} saved to database.");

                    //generatePDF(true);

                    //saved = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void BTN_AddNote_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            TB_Notes.Text += $"{date} -- {time} -- {TB_AddNote.Text}\n";

            TB_AddNote.Text = "";
        }

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