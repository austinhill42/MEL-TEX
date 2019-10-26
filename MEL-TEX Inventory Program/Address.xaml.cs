using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for Address.xaml
    /// </summary>
    public partial class Address : UserControl
    {
        #region Properties

        public string AddressType { get; set; }

        #endregion Properties

        #region Constructors

        public Address()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #endregion Constructors
    }
}