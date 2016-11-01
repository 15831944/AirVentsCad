using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for RoofUC.xaml
    /// </summary>
    public partial class RoofUc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoofUc"/> class.
        /// </summary>
        public RoofUc()
        {
            InitializeComponent();
        }
        
        void BuildRoof_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sw = new ModelSw();
                sw.Roof(TypeOfRoof.Text, WidthRoof.Text, LenghtRoof.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        void WidthRoof_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildRoof_Click(this, new RoutedEventArgs());
            }
        }

        void LenghtRoof_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildRoof_Click(this, new RoutedEventArgs());
            }
        }

        void TypeOfRoof_LayoutUpdated(object sender, EventArgs e)
        {
            const string picturePath = @"\DataControls\Pictures\Крыша\";
            var pictureName = "15-01-800-1100.JPG";
            switch (TypeOfRoof.Text)
            {
                case "2":
                    pictureName = "15-02-800-1100.JPG";
                    break;
                case "3":
                    pictureName = "15-03-800-1100.JPG";
                    break;
                case "4":
                    pictureName = "15-04-800-1100.JPG";
                    break;
            }
            App.ElementVisibility.SetImage(picturePath + pictureName, PictureRoof);
        }
    }
}
