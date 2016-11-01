using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AirVentsCadWpf.AirVentsClasses;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;
using AirVentsCadWpf.ServiceVents;
using System.Threading.Tasks;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for DamperUC.xaml
    /// </summary>
    public partial class DamperUc
    {        
        /// <summary>
        /// Initializes a new instance of the <see cref="DamperUc"/> class.
        /// </summary>
        public DamperUc()
        {
            InitializeComponent();            
            Totals.SetMaterial(MaterialP1);
            ТолщинаВнешней.ItemsSource = Totals.SheetMetalThikness;
            ТолщинаВнешней.SelectedIndex = 2;
        }

        void BuildDamper_Click(object sender, RoutedEventArgs e)
        {

            var mat1Code = "";
            var viewRowMat1 = (DataRowView)MaterialP1.SelectedItem;
            var row1 = viewRowMat1.Row;
            if (row1 != null)
                mat1Code = row1.Field<string>("CodeMaterial");

            var materialP1 = new[] { MaterialP1.SelectedValue.ToString(), ТолщинаВнешней.Text, MaterialP1.Text, mat1Code };
            //goto m2;

            #region ModelSw

            try
            {
                #region CodeMaterial

                //var mat1Code = "";
                //var viewRowMat1 = (DataRowView)MaterialP1.SelectedItem;
                //var row1 = viewRowMat1.Row;
                //if (row1 != null)
                //    mat1Code = row1.Field<string>("CodeMaterial");
                //var materialP1 = new[] { MaterialP1.SelectedValue.ToString(), ТолщинаВнешней.Text, MaterialP1.Text, mat1Code };


                //var vcad = new VentsCadLibrary.VentsCad
                //{
                //    ConnectionToSql = Settings.Default.ConnectionToSQL,
                //    DestVaultName = Settings.Default.TestPdmBaseName,
                //    VaultName = Settings.Default.PdmBaseName
                //};

                //string unit;
                //vcad.DumperS(TypeOfDumper.Text, WidthDamper.Text, HeightDamper.Text, (IsOutDoor.IsChecked == true), materialP1, out unit, false);

                //MessageBox.Show("Finish");

                //return;

                #endregion            
                var sw = new ModelSw();
                sw.Dumper(TypeOfDumper.Text, WidthDamper.Text, HeightDamper.Text, (IsOutDoor.IsChecked == true), materialP1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message);
            }

            return;

            #endregion
            
            m1: 
            //VentsCadLibrary

            try
            {             
                using (var server = new VentsCadLibrary.VentsCad())
                {
                    var newDumper = new VentsCadLibrary.VentsCad.Dumper(TypeOfDumper.Text, WidthDamper.Text, HeightDamper.Text, (IsOutDoor.IsChecked == true), materialP1);
                    if (!newDumper.Exist)
                    {
                        newDumper.Build();
                    }
                    var place = newDumper.GetPlace();
                    MessageBox.Show(place.Path + "\n" + place.IdPdm + "\n" + place.ProjectId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return;

            m2: 
            //VentsCadService

            MessageBox.Show("VentsCadService");

            try
            {  
                var serv = new ServiceV(
                    new VentsCadService.Parameters
                    {
                        Name = "dumper",
                        Type = new VentsCadService.Type
                        {
                            SubType = TypeOfDumper.Text
                        },
                        Sizes = new VentsCadService.Sizes[]
                        {
                            new VentsCadService.Sizes
                            {
                                Width = WidthDamper.Text,
                                Height = HeightDamper.Text
                            }
                        },
                        Materials = new VentsCadService.Material[]
                        {
                            new VentsCadService.Material
                            {                             
                                Code = materialP1[3],
                                Name = materialP1[2],
                                Thikness = materialP1[1],
                                Value = materialP1[0],
                            }
                        }
                    });
                var Build = new Task(serv.build);
                Build.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        void WidthDamper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildDamper_Click(this, new RoutedEventArgs());
            }
        }

        void HeightDamper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildDamper_Click(this, new RoutedEventArgs());
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void MaterialP1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialP1 == null) return;
            if (MaterialP1.SelectedIndex == 0)
            {
                ТолщинаВнешнейLbl.Visibility = Visibility.Hidden;
                ТолщинаВнешней.Visibility = Visibility.Hidden;
            }
            else
            {
                ТолщинаВнешнейLbl.Visibility = Visibility.Visible;
                ТолщинаВнешней.Visibility = Visibility.Visible;
            }
        }

    }
}
