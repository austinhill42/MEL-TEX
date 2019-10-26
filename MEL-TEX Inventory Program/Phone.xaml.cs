using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for Phone.xaml
    /// </summary>
    public partial class Phone : UserControl
    {
        #region Fields

        internal string number = "";
        internal string type = "";

        #endregion Fields

        #region Constructors

        public Phone()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        private void CB_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            type = CB_Type.Text;
        }

        private void TB_Phone_TextChanged(object sender, TextChangedEventArgs e)
        {
            number = TB_Phone.Text;
        }

        #endregion Methods
    }
}