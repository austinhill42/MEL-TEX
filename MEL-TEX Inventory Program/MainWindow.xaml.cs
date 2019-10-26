using System.Windows;

namespace MELTEX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            Content = new MainPage();
        }

        #endregion Constructors
    }
}