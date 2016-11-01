using System.Windows;

namespace MakeDxfSw
{
    /// <summary>
    /// Interaction logic for MakeDxf.xaml
    /// </summary>
    public partial class MakeDxf
    {
        public MakeDxf()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().ShowDialog();
        }
    }
}
