using System;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.AirVentsClasses.UnitsBuilding;


namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for Panel50UC.xaml
    /// </summary>
    public partial class Panel50Uc
    {      
        /// <summary>
        /// Initializes a new instance of the <see cref="Panel50Uc"/> class.
        /// </summary>
        public Panel50Uc()
        {
            InitializeComponent();

            _backgroundWorker.DoWork += BackgroundWorkerOnDoWork;           

            ТолщинаВнешней.ItemsSource = Totals.SheetMetalThikness;
            ТолщинаВнешней.SelectedIndex = 2;

            ТолщинаВннутренней.ItemsSource = Totals.SheetMetalThikness;
            ТолщинаВннутренней.SelectedIndex = 2;

            Totals.SetPanelType(TypeOfPanel50);            

            Totals.SetMaterial(MaterialP1);
            Totals.SetMaterial(MaterialP2);

            #region Paint

            Totals.SetRal(Ral1);
            Totals.SetRal(Ral2);
            Ral1.Visibility = Visibility.Hidden;
            Ral2.Visibility = Visibility.Hidden;
            Totals.SetCoatingType(CoatingType1);
            Totals.SetCoatingType(CoatingType2);
            Totals.SetCoatingClass(CoatingClass1);
            Totals.SetCoatingClass(CoatingClass2);

            #endregion

        }

        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            BuildPanel();
        }

        void BUILDING_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorkerOnDoWork(sender, null);
            //BuildPanel();
        }

        readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
        private void BuildPanel()
        {
            var mat1Code = "";
            var mat2Code = "";

            var viewRowMat1 = (DataRowView) MaterialP1.SelectedItem;
            var row1 = viewRowMat1.Row;
            if (row1 != null)
                mat1Code = row1.Field<string>("CodeMaterial");
            var viewRowMat2 = (DataRowView) MaterialP2.SelectedItem;
            var row2 = viewRowMat2.Row;
            if (row2 != null)
                mat2Code = row2.Field<string>("CodeMaterial");


            var materialP1 = new[] {MaterialP1.SelectedValue.ToString(), ТолщинаВнешней.Text, MaterialP1.Text, mat1Code};
            var materialP2 = new[] {MaterialP2.SelectedValue.ToString(), ТолщинаВннутренней.Text, MaterialP2.Text, mat2Code};           


            var thicknessOfPanel = ((ComboBoxItem) TypeOfPanel.SelectedItem).Content.ToString().Remove(2);


            #region VentsCadLibrary

            //MessageBox.Show(thicknessOfPanel);

            //MessageBox.Show(Ral1.SelectedValue + " " + CoatingType1.Text, CoatingClass1.Text);

            //var vcad = new VentsCadLibrary.VentsCad
            //{
            //    ConnectionToSql = Settings.Default.ConnectionToSQL,
            //    DestVaultName = Settings.Default.TestPdmBaseName,
            //    VaultName = Settings.Default.PdmBaseName
            //};

            //MessageBox.Show(TypeOfPanel50.SelectedValue 
            //    + " \n " + TypeOfPanel50.Text
            //    + " \n " + Ral1.Text
            //    + " \n " + CoatingType1.Text
            //    + " \n " + CoatingClass1.Text
            //    );

            //vcad.Panels50(
            //    typeOfPanel: new[] { TypeOfPanel50.SelectedValue.ToString(), TypeOfPanel50.Text },
            //    width: WidthPanel.Text,
            //    lenght: HeightPanel.Text,
            //    materialP1: materialP1,
            //    materialP2: materialP2,
            //    покрытие: new[]
            //    {
            //        Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
            //        Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
            //        Ral1.SelectedValue?.ToString() ?? "",
            //        Ral2.SelectedValue?.ToString() ?? ""
            //    },
            //    onlyPath: false);

            #endregion

            var sw = new ModelSw();

            switch (thicknessOfPanel)
            {
                case "30":
                    string path;
                    sw.Panels30Build(
                        typeOfPanel:
                            new[] {TypeOfPanel50.SelectedValue.ToString(), TypeOfPanel50.Text, thicknessOfPanel},
                        width: WidthPanel.Text,
                        height: HeightPanel.Text,
                        materialP1: materialP1,
                        materialP2: materialP2,
                        покрытие: null,

                    #region To delete

                        //покрытие: new[]
                        //{
                        //    Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                        //    Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                        //    Ral1.SelectedValue?.ToString() ?? "",
                        //    Ral2.SelectedValue?.ToString() ?? ""
                        //},

                    #endregion

                        path: out path);

                    break;
                case "50":
                case "70":
                    sw.Panels50Build(
                        typeOfPanel: new[] {TypeOfPanel50.SelectedValue.ToString(), TypeOfPanel50.Text, thicknessOfPanel},
                        width: WidthPanel.Text,
                        height: HeightPanel.Text,
                        materialP1: materialP1,
                        meterialP2: materialP2,
                        покрытие: null

                    #region To Delete
                        //покрытие: new[]
                        //{
                        //    Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                        //    Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                        //    Ral1.SelectedValue?.ToString() ?? "",
                        //    Ral2.SelectedValue?.ToString() ?? ""
                        //}
                    #endregion

                        );
                    break;
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        void HeightPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void WidthPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            HeightPanel.MaxLength = 5;
            WidthPanel.MaxLength = 5;
        }

        void TypeOfPanel50_LayoutUpdated(object sender, EventArgs e)
        {
            const string picturePath = @"\DataControls\Pictures\Панели\";
            var pictureName = "02-01-500-750-50-Az-Az-MW.JPG";
            switch (TypeOfPanel50.Text)
            {
                case "Съемная":
                    pictureName = "02-04-500-750-50-Az-Az-MW.JPG";
                    break;
                case "Панель теплообменника":
                    pictureName = "02-05-500-750-50-Az-Az-MW.JPG";
                    break;
                case "Панель двойная":
                    pictureName = "02-01-1000-750-50-Az-Az-MW.JPG";
                    break;
            }
            App.ElementVisibility.SetImage(picturePath + pictureName, PicturePanel);
        }               

        private void Ral1_LayoutUpdated(object sender, EventArgs e)
        {
            if (Ral1.Text == "Без покрытия")
            {
                CoatingType1.Visibility = Visibility.Collapsed;
                CoatingClass1.Visibility = Visibility.Collapsed;
            }
            else
            {
                CoatingType1.Visibility = Visibility.Visible;
                CoatingClass1.Visibility = Visibility.Visible;
            }
        }

        private void Ral2_LayoutUpdated(object sender, EventArgs e)
        {
            if (Ral2.Text == "Без покрытия")
            {
                CoatingType2.Visibility = Visibility.Hidden;
                CoatingClass2.Visibility = Visibility.Hidden;
            }
            else
            {
                CoatingType2.Visibility = Visibility.Visible;
                CoatingClass2.Visibility = Visibility.Visible;
            }
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

        private void MaterialP2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialP2 == null) return;
            if (MaterialP2.SelectedIndex == 0)
            {
                ТолщинаВннутреннейLbl.Visibility = Visibility.Hidden;
                ТолщинаВннутренней.Visibility = Visibility.Hidden;
            }
            else
            {
                ТолщинаВннутреннейLbl.Visibility = Visibility.Visible;
                ТолщинаВннутренней.Visibility = Visibility.Visible;
            }
        }
                
    }
}
