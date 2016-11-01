
using System.Windows;

namespace TestVentsCadLibrary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            VentsCadLibrary.VentsCad ventsCadLibrary = new VentsCadLibrary.VentsCad
            {
                ConnectionToSQL = "Data Source = srvkb; Initial Catalog = SWPlusDB; Persist Security Info = True; User ID = sa; Password = PDMadmin; MultipleActiveResultSets = True",
                DestVaultName = "Tets_debag",
                VaultName = "Vents-PDM"
            };   
            
            var Spigot = "";
            ventsCadLibrary.Spigot("20", "1075", "734", out Spigot);
            MessageBox.Show(Spigot);
        }
    }
}
