using System.Windows.Controls;
using AirVentsCadWpf.DataControls;

namespace AirVentsCadWpf
{
    class Switcher
    {
        public static MainWindow PageSwitcher;

        public static void Switch(UserControl newPage)
        {
            PageSwitcher.NavigateMenu(newPage);
        }
        public static void SwitchData(UserControl newPage)
        {
            PageSwitcher.NavigateData(newPage);
        }

        public static AirVentsUnits50Uc PageSwitcherUnits50Uc;

        public static void SwitchUnit(UserControl newPage)
        {
            PageSwitcherUnits50Uc.Navigate(newPage);
        }

        public static Units50Uc SwitcherUnits50Uc;

        public static void SwitchUnitData(UserControl newPage)
        {
            SwitcherUnits50Uc.NavigateData(newPage);
        }

        //public static void SwitchUnits(UserControl newPage)
        //{
        //    SwitcherUnits50Uc.NavigateUnits(newPage);
        //}

    }
}
