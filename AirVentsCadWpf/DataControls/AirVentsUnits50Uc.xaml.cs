using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AirVentsCadWpf.AirVentsClasses;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for AirVentsUnits50Uc.xaml
    /// </summary>
    public partial class AirVentsUnits50Uc
    {
        public AirVentsUnits50Uc()
        {
            InitializeComponent();
            Switcher.PageSwitcherUnits50Uc = this;
            //Switcher.SwitchUnit(new MontageFrameUc
            //{
            //    Build = { Visibility = Visibility.Collapsed }
            //});
        }

        public void Navigate(UserControl nextPage)
        {
            GridUnit.Children.Clear();
            GridUnit.Children.Add(nextPage);
        }


        private void BUILDING_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SizeOfUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SizeOfUnit == null)
            {
                return;
            }
            var sqlBaseData = new SqlBaseData();
            var standartUnitSizes =
                sqlBaseData.StandartSize(
                    SizeOfUnit.SelectedItem.ToString().Replace("" +
                                                               "System.Windows.Controls.ListBoxItem: ", ""));

            if (WidthU == null || HeightU == null)
            {
                return;
            }
            WidthU.Text = standartUnitSizes[0];
            HeightU.Text = standartUnitSizes[1];
        }

        void UnitDrw(int widthUi, int heightUi)
        {
            if (Unit == null) return;
            var k = 6;
            if (widthUi >= 1300 || heightUi >= 1300) { k = 8; }
            if (widthUi > 1500 || heightUi > 1500) { k = 10; }
            if (widthUi > 2000 || heightUi > 2000) { k = 15; }
            Unit.Width = widthUi / k;
            Unit.Height = heightUi / k;
            Ширина.Content = widthUi;
            Высота.Content = heightUi;
            UnitLDrw(0, Unit1);
        }

        void UnitLDrw(int lenghtUi, FrameworkElement unitRectangle)
        {
            if (unitRectangle == null) return;
            if (lenghtUi <= 0 ) lenghtUi = 250;
            var k = 6;
            if (lenghtUi >= 1300) { k = 8; }
            if (lenghtUi > 1500) { k = 10; }
            if (lenghtUi > 2000) { k = 15; }
            unitRectangle.Width = lenghtUi / k;
            unitRectangle.Height = Unit.Height;
            Длина.Content = lenghtUi;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void HeightU_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
            
        }

        private void WidthU_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        private void TypeOfFrame_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var typeOfFrameCopyValue =
               TypeOfFrame.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
            if (FrameOffsetLabel == null || FrameOffset == null)
            {
                return;
            }
            if (typeOfFrameCopyValue != "3")
            {
                FrameOffsetLabel.Visibility = Visibility.Collapsed;
                FrameOffset.Visibility = Visibility.Collapsed;
            }
            else
            {
                FrameOffset.IsReadOnly = false;
                FrameOffsetLabel.Visibility = Visibility.Visible;
                FrameOffset.Visibility = Visibility.Visible;
            }
        }

        private void Thikness_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Thikness == null || MaterialMontageFrame == null)
            {
                return;
            }
            switch (Thikness.SelectedValue.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", ""))
            {
                case "2":
                    MaterialMontageFrame.SelectedIndex = 0;
                    break;
                case "3":
                case "4":
                    MaterialMontageFrame.SelectedIndex = 1;
                    break;
            }
        }

        private void nonstandard_Checked(object sender, RoutedEventArgs e)
        {
            WidthU.IsReadOnly = false;
            HeightU.IsReadOnly = false;
            WidthLabel.Visibility = Visibility.Visible;
            WidthU.Visibility = Visibility.Visible;
            HeightLabel.Visibility = Visibility.Visible;
            HeightU.Visibility = Visibility.Visible;
        }
        
        private void nonstandard_Unchecked(object sender, RoutedEventArgs e)
        {
            WidthU.IsReadOnly = true;
            HeightU.IsReadOnly = true;
        }

        private void FrameOffset_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        private void NonstandardMontageFrame_Initialized(object sender, EventArgs e)
        {
            NonstandardMontageFrame.Unchecked += NonstandardMontageFrameOnUnchecked;
            NonstandardMontageFrame.Checked += NonstandardMontageFrameOnChecked;
        }

        private void NonstandardMontageFrameOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Thikness == null || MaterialMontageFrame == null)
            {
                return;
            }
            switch (Thikness.SelectedValue.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", ""))
            {
                case "2":
                    MaterialMontageFrame.SelectedIndex = 1;
                    break;
                case "3":
                case "4":
                    MaterialMontageFrame.SelectedIndex = 0;
                    break;
            }
        }

        private void NonstandardMontageFrameOnUnchecked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Thikness == null || MaterialMontageFrame == null)
            {
                return;
            }
            switch (Thikness.SelectedValue.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", ""))
            {
                case "2":
                    MaterialMontageFrame.SelectedIndex = 0;
                    break;
                case "3":
                case "4":
                    MaterialMontageFrame.SelectedIndex = 1;
                    break;
            }
        }

        private void WidthU_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (WidthU == null) return;
            if (HeightU != null) UnitDrw(Convert.ToInt32(WidthU.Text), Convert.ToInt32(HeightU.Text));
        }

        private void HeightU_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (HeightU == null) return;
            if (WidthU != null) UnitDrw(Convert.ToInt32(WidthU.Text), Convert.ToInt32(HeightU.Text));
        }
    }
}
