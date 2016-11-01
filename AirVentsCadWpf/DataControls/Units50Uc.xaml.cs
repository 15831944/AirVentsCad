using System.Windows;
using System.Windows.Controls;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for Units50Uc.xaml
    /// </summary>
    public partial class Units50Uc
    {
        /// <summary>
        /// 
        /// </summary>
        public Units50Uc()
        {
            InitializeComponent();
            Switcher.SwitcherUnits50Uc = this;
            
            Switcher.SwitchUnitData(new MonoBlock01Uc50
            {
                TitleUnit = { Visibility = Visibility.Collapsed },
                Build = { Visibility = Visibility.Collapsed },
                SizeOfUnit = { SelectedIndex = SizeOfUnit.SelectedIndex }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nextPage"></param>
        public void NavigateData(UserControl nextPage)
        {
            GridUnitData.Children.Clear();
            GridUnitData.Children.Add(nextPage);
        }

        private void BUILDING_Click(object sender, RoutedEventArgs e)
        {
            GridViewUnits.Children.Clear();
            GridViewUnits.Children.Add(new UnitElement { SizeOfUnitIndex = SizeOfUnit.SelectedIndex });
        }
    }
}
