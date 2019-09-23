using System;
using System.Collections.Generic;
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
