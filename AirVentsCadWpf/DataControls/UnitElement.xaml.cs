using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for UnitElement.xaml
    /// </summary>
    public partial class UnitElement
    {
        /// <summary>
        /// 
        /// </summary>
        public UnitElement()
        {
            InitializeComponent();
            _uc.TitleUnit.Visibility = Visibility.Collapsed;
            _uc.Build.Visibility = Visibility.Collapsed;

            if (GridR.Children.Contains(_uc) == false)
            {
                BorderR.Visibility = Visibility.Visible;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public int SizeOfUnitIndex ;
        
        readonly MonoBlock01Uc50 _uc = new MonoBlock01Uc50();
        
        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

            GridR.Children.Clear();
            GridR.Children.Add(new UnitElement
            {
              SizeOfUnitIndex = SizeOfUnitIndex,
              GridL = {Visibility = Visibility.Collapsed},
              GridForm = { Visibility = Visibility.Collapsed }
            });
            BorderR.Visibility = Visibility.Collapsed;
            
        }

        private void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            GridL.Children.Clear();
            GridL.Children.Add(new UnitElement
            {
                SizeOfUnitIndex = SizeOfUnitIndex ,
                GridR = { Visibility = Visibility.Collapsed },
                GridForm = { Visibility = Visibility.Collapsed }
            });
        }

        private void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            _uc.SizeOfUnit.SelectedIndex = SizeOfUnitIndex;
            Dispatcher.InvokeAsync(() => Switcher.SwitchUnitData(_uc));
        }

        private void Unit_MouseEnter(object sender, MouseEventArgs e)
        {
            Unit.Background = Brushes.Green;
        }

        private void Unit_MouseLeave(object sender, MouseEventArgs e)
        {
            Unit.Background = Brushes.AliceBlue;
            var menu = new ContextMenu();
            menu.Items.Add("Delete");
            menu.Visibility = Visibility.Visible;
        }

        private void UserControl_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            var lenght = $"{Convert.ToString(GridForm.Width)}";
            WidthUnitS.Content = lenght;

            if (GridR.Children.Contains(_uc) == false)
            {
                BorderR.Visibility = Visibility.Visible;
            }
        }

        private void Unit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var menu = new ContextMenu();
            menu.Items.Add("Delete");
            menu.Visibility = Visibility.Visible;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show("Удалить блок?", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) ==
                MessageBoxResult.Yes)
            {
                Visibility = Visibility.Collapsed;
            }
            BorderR.Visibility = Visibility.Visible;
        }

        private void Unit_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (GridR.Children.Contains(_uc) == false)
            {
                BorderR.Visibility = Visibility.Visible;
            }
        }
    }
}
