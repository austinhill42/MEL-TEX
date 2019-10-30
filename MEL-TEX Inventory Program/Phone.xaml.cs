using System.ComponentModel;
using System.Windows.Controls;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for Phone.xaml
    /// </summary>
    public partial class Phone : UserControl, INotifyPropertyChanged
    {
        #region Fields

        internal string number = "";
        internal string type = "";
        private AddContact contact;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Number { get => number; set { number = value; OnPropertyChanged("Number"); } }
        public string Type { get => type; set { type = value; OnPropertyChanged("Type"); } }

        #endregion Fields

        #region Constructors

        public Phone(AddContact c)
        {
            InitializeComponent();

            contact = c;

            DataContext = this;
        }

        #endregion Constructors

        #region Methods

        #endregion Methods

        private void BTN_Delete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            (Parent as StackPanel).Children.Remove(this);
            contact.Numbers.Remove(this);
        }
        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}