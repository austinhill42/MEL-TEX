using System.Windows;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for Address.xaml
    /// </summary>
    public partial class Address : UserControl
    {
        public string AddressType { get; set; }

        public Address()
        {
            InitializeComponent();

            this.DataContext = this;
        }
    }
}
