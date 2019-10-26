using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for Phone.xaml
    /// </summary>
    public partial class Phone : UserControl
    {
        internal string number = "";
        internal string type = "";

        public Phone()
        {
            InitializeComponent();
        }

        private void TB_Phone_TextChanged(object sender, TextChangedEventArgs e)
        {
            number = TB_Phone.Text;
        }

        private void CB_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            type = CB_Type.Text;
        }
    }
}