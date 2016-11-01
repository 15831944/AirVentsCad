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
    /// Interaction logic for Unit50UC.xaml
    /// </summary>
    public partial class MonoBlock01Uc50
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonoBlock01Uc50"/> class.
        /// </summary>
        public MonoBlock01Uc50()
        {
            InitializeComponent();

            var sqlBaseData = new SqlBaseData();
            var airVentsStandardSize = sqlBaseData.AirVentsStandardSize();
            SizeOfUnit.ItemsSource = ((IListSource)airVentsStandardSize).GetList();
            SizeOfUnit.DisplayMemberPath = "Type";
            SizeOfUnit.SelectedIndex = 0;

            SectionTextBox.ItemsSource = new[]
                {
                    "A", "B", "C", "D", "E", "F", "G", "H",
                    "I","J", "K", "L", "M", "N", "O", "P", 
                    "R", "S","T", "U", "V","W", "X", "Y", "Z"
                };


          //  GridTypeOfUnit50.Children.Add(new UnitElement());

            SelectHeaters("");
            DataTable1.Visibility = Visibility.Collapsed;

            #region UNIT50FULL

            Lenght.MaxLength = 5;
            WidthU.MaxLength = 5;
            WidthU.IsReadOnly = true;
            HeightU.MaxLength = 5;
            HeightU.IsReadOnly = true;
            Lenght.MaxLength = 5;

            #endregion

            PanelGrid.Visibility = Visibility.Collapsed;
            GridMontageFrame.Visibility = Visibility.Collapsed;
            GridRoof.Visibility = Visibility.Collapsed;

            WidthLabel1.Visibility = Visibility.Collapsed;
            WidthRoof.Visibility = Visibility.Collapsed;
            HeightLabel1.Visibility = Visibility.Collapsed;
            LenghtRoof.Visibility = Visibility.Collapsed;
            

            #region MontageFrame50 Initialize

            FrameOffset.MaxLength = 5;
            FrameOffset.IsReadOnly = true;
            FrameOffsetLabel.Visibility = Visibility.Collapsed;
            FrameOffset.Visibility = Visibility.Collapsed;
            TypeOfFrame.SelectedIndex = 3;
            
            #endregion;

            #region Dumper

            WidthLabel2.Visibility = Visibility.Collapsed;
            WidthDamper.Visibility = Visibility.Collapsed;
            HeightLabel2.Visibility = Visibility.Collapsed;
            HeightDamper.Visibility = Visibility.Collapsed;

            #endregion

            #region Spigot

            WidthLabel3.Visibility = Visibility.Collapsed;
            WidthSpigot.Visibility = Visibility.Collapsed;
            HeightLabel3.Visibility = Visibility.Collapsed;
            HeightSpigot.Visibility = Visibility.Collapsed;

            #endregion

            #region Panels

          
            //Panel1.SelectionChanged += (sender, args) => 
            LabelType1.Visibility = Visibility.Collapsed;
            TypeOfPanel1.Visibility = Visibility.Collapsed;
            LabelType2.Visibility = Visibility.Collapsed;
            TypeOfPanel2.Visibility = Visibility.Collapsed;
            LabelType3.Visibility = Visibility.Collapsed;
            TypeOfPanel3.Visibility = Visibility.Collapsed;
            LabelType4.Visibility = Visibility.Collapsed;
            TypeOfPanel4.Visibility = Visibility.Collapsed;

            #endregion
        }

        private static void SelectHeaters(string sizeOfUnit)
        {
            var sqlBaseData = new SqlBaseData();
            //if (sizeOfUnit == "")
            //{
            //    sizeOfUnit = "AV04";
            //}
            //var heatersTable = 
                sqlBaseData.HeatersTable(sizeOfUnit);
            //if (DataTable1 == null) {return;}
            //DataTable1.ItemsSource = heatersTable.DefaultView;

            //ModelName3.ItemsSource = ((IListSource)heatersTable).GetList();
            //ModelName3.DisplayMemberPath = "CoilName";
            //ModelName3.SelectedIndex = 1;
        }

        private void SizeOfUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (SizeOfUnit?.SelectedItem == null)
                {
                    return;
                }
                var id = Convert.ToInt32(((DataRowView)SizeOfUnit.SelectedItem)["SizeID"].ToString());
                var sqlBaseData = new SqlBaseData();
                var standartUnitSizes = sqlBaseData.StandartSize(id, 2);

                if (WidthU == null || HeightU == null)
                {
                    return;
                }
                WidthU.Text = standartUnitSizes[0];
                HeightU.Text = standartUnitSizes[1];
            }
            catch (Exception)
            {
                if (WidthU != null) WidthU.Text = "";
                if (HeightU != null) HeightU.Text = "";
            }
            
            //if (SizeOfUnit == null)
            //{
            //    return;
            //}
            //var sqlBaseData = new SqlBaseData();
            //var standartUnitSizes =
            //    sqlBaseData.StandartSize(
            //        SizeOfUnit.SelectedItem.ToString().

            
            //System.Windows.Controls.ListBoxItem: ", ""));

            //if (WidthU == null || HeightU == null)
            //{
            //    return;
            //}
            //WidthU.Text = standartUnitSizes[0];
            //HeightU.Text = standartUnitSizes[1];

            //SelectHeaters(SizeOfUnit.SelectedItem.ToString().Replace("System.Windows.Controls.ListBoxItem: ", ""));
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
       
        void nonstandard_Unchecked(object sender, RoutedEventArgs e)
        {
            WidthU.IsReadOnly = true;
            HeightU.IsReadOnly = true;
        }
         
         void BUILDING_Click(object sender, RoutedEventArgs args)
        {
            var sw = new ModelSw();

            var width = Convert.ToDouble(WidthU.Text);
            var height = Convert.ToDouble(HeightU.Text);
            var lenght = Convert.ToDouble(Lenght.Text);

            try
            {
               
                #region Панели

                var paneltype1 = TypeOfPanel1.Text;
                var m1 = new[] {MaterialP1.Text, null};
                var m2 = new[] { MaterialP2.Text, null };

                var p1 = "";
                var p2 = "";
                var p3 = "";
                var p4 = "";
                var widthVb = Panel4.Text;
                if (Panel50.IsChecked != true)
                {
                     paneltype1 = "";
                     m1 = null;
                     m2 = null;
                }
                else
                {
                    p1 = sw.Panels50BuildStr(new[] { null, TypeOfPanel1.Text }, Panel1.Text, Convert.ToString(height - 100), m1, m2, null, true);
                    p2 = sw.Panels50BuildStr(new[] { null, TypeOfPanel1.Text }, Panel2.Text, Convert.ToString(height - 100), m1, m2, null, true);
                    p3 = sw.Panels50BuildStr(new[] { null, TypeOfPanel1.Text }, Panel3.Text, Convert.ToString(height - 100), m1, m2, null, true);
                    p4 = sw.Panels50BuildStr(new[] { null, TypeOfPanel1.Text }, Panel4.Text, Convert.ToString(height - 100), m1, m2, null, true);
                }

                #endregion

                #region Монтажная рама
                
                if (FrameOffset.Text == "")
                {
                    FrameOffset.Text = Convert.ToString(Convert.ToDouble(Lenght.Text) / 2);
                }
                var mfthikness = Thikness.Text;
                var typeofframe = TypeOfFrame.Text;
                var frameoffcet = FrameOffset.Text;
                if (MontageFrame50.IsChecked != true)
                {
                    mfthikness = "";
                }

                #endregion

                #region Крыша

                var roofType = TypeOfRoof.Text;
                if (RoofOfUnit50.IsChecked != true)
                {
                    roofType = "";
                }
               
                var size = SizeOfUnit.Text;
                if (Nonstandard.IsChecked == true)
                {
                    size = size + "N";
                }

                #endregion

                #region Заслонка

                var dumper = "";
                if (DumperInMonoblock.IsChecked == true)
                {
                    var w = width - 80;
                    var h = height - 90;
                    if (TypeOfDumper.Text == "20")
                    {
                        w = width - 70;
                        h = height - 90 + 17;
                    }
                    dumper = sw.DumperS(TypeOfDumper.Text, Convert.ToString(w), Convert.ToString(h), false, null).Replace("DRW", "ASM");
                }

                #endregion

                #region Вибровставки

                var spigot = "";
                if (Spigot.IsChecked == true)
                {
                    var w = width - 114 + 31;
                    var h = height - 114 + 31;
                    if (TypeOfSpigot.Text == "20")
                    {
                        w = width - 100 + 31;
                        h = height - 100 + 31;
                    }
                    spigot = sw.SpigotStr(TypeOfSpigot.Text, Convert.ToString(w), Convert.ToString(h)).Replace("DRW", "ASM");
                }

                #endregion

                #region Фильтр

                var filter = "";
                if (InnerOfUnit50.IsChecked == true)
                {
                    filter = @"D:\Vents-PDM\Проекты\Blauberg\04 - Фильтры\" + ModelName1.Text + ".SLDASM";
                }

                #endregion

                #region Теплообменник 1

                var heater = "";
                if (InnerOfUnit50.IsChecked == true)
                {
                    heater = @"D:\Vents-PDM\Проекты\Blauberg\16 - Узел нагревателя водяного\" + ModelName2.Text + ".SLDASM";
                }

                #endregion

                #region Теплообменник 2

                var cooler = "";
                if (InnerOfUnit50.IsChecked == true)
                {
                    cooler = @"D:\Vents-PDM\Проекты\Blauberg\16 - Узел нагревателя водяного\" + ModelName2.Text + ".SLDASM";
                }

                #endregion

                #region Вентагрегат

                var ventil = "";
                if (InnerOfUnit50.IsChecked == true)
                {
                    ventil = @"D:\Vents-PDM\Проекты\Blauberg\16 - Узел нагревателя водяного\" + ModelName2.Text + ".SLDASM";
                }

                #endregion

                sw.UnitS50(
                    size: size, 
                    type: "MonoBlock", 
                    side: SideService.Text, 
                    widthS: Convert.ToString(width), 
                    heightS: Convert.ToString(height), 
                    lenghtS: Convert.ToString(lenght), 
                    montageFrame: mfthikness, 
                    typeOfFrame: typeofframe, 
                    frameOffset: frameoffcet, 
                    typeOfPanel: paneltype1, 
                    materialP1: m1, 
                    materialP2: m2, 
                    roofType: roofType, 
                    dumper: dumper, 
                    spigot: spigot, 
                    p1: p1, 
                    p2: p2, 
                    p3: p3, 
                    p4: p4, 
                    widthVb: widthVb, 
                    filter: filter, 
                    heater: heater, 
                    cooler: cooler, 
                    ventil: ventil);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                //  sw.Logger.Log(LogLevel.Error, string.Format("Ошибка во время генерации блока {0}, время - {1}", DateTime.Now), args);
            }
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

        private void Lenght_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        private void LenghtRoof_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        private void WidthRoof_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        private void RoofOfUnit50_Checked(object sender, RoutedEventArgs e)
        {
            GridRoof.Visibility = Visibility.Visible;
        }

        private void RoofOfUnit50_Unchecked(object sender, RoutedEventArgs e)
        {
            GridRoof.Visibility = Visibility.Collapsed;
        }

        private void InnerOfUnit50_Checked(object sender, RoutedEventArgs e)
        {
            InnerGrid.Visibility = Visibility.Visible;
        }

        private void InnerOfUnit50_Unchecked(object sender, RoutedEventArgs e)
        {
            InnerGrid.Visibility = Visibility.Collapsed;
        }

        private void Panel50_Checked(object sender, RoutedEventArgs e)
        {
            PanelGrid.Visibility = Visibility.Visible;
            PanelGrid2.Visibility = Visibility.Visible;
        }

        private void Panel50_Unchecked(object sender, RoutedEventArgs e)
        {
            PanelGrid.Visibility = Visibility.Collapsed;
            PanelGrid2.Visibility = Visibility.Collapsed;
        }

        private void MontageFrame50_Checked(object sender, RoutedEventArgs e)
        {
            GridMontageFrame.Visibility = Visibility.Visible;
        }

        private void MontageFrame50_Unchecked(object sender, RoutedEventArgs e)
        {
            GridMontageFrame.Visibility = Visibility.Collapsed;
        }

        private void DumperInMonoblock_Checked(object sender, RoutedEventArgs e)
        {
            GridDimDumper.Visibility = Visibility.Visible;
        }

        private void DumperInMonoblock_Unchecked(object sender, RoutedEventArgs e)
        {
            GridDimDumper.Visibility = Visibility.Collapsed;
        }

        private void Spigot_Unchecked(object sender, RoutedEventArgs e)
        {
            SpigotDimGrid.Visibility = Visibility.Collapsed;
        }

        private void Spigot_Checked(object sender, RoutedEventArgs e)
        {
            SpigotDimGrid.Visibility = Visibility.Visible;
        }

        private void WidthU_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (TypeOfSpigot == null) {return;}
            if (TypeOfDumper == null) { return; }
            if (WidthU.Text == "") { return; }


            // Типы вибровставок и заслонки
            if (Convert.ToInt32(WidthU.Text) >= 1000)
            {
                TypeOfDumper.SelectedIndex = 1;
                TypeOfSpigot.SelectedIndex = 1;
            }
            else
            {
                TypeOfDumper.SelectedIndex = 0;
                TypeOfSpigot.SelectedIndex = 0;
            }

            // Тип монтажной рамы
            
        }

        static void SumPanel(TextBox lenght, TextBox p1, TextBox p2, TextBox p3, TextBox p4, TextBox frameOffset)
        {
            if (lenght == null || p1 == null || p2 == null || p3 == null || p4 == null || frameOffset == null)
            {return;}

            lenght.Text = Convert.ToString(250 + Convert.ToDouble(p1.Text) + Convert.ToDouble(p2.Text) + Convert.ToDouble(p3.Text) +
                    Convert.ToDouble(p4.Text));
            frameOffset.Text = Convert.ToString(175 + Convert.ToDouble(p1.Text) + Convert.ToDouble(p2.Text) + Convert.ToDouble(p3.Text));
        }

        private void Panel1_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SumPanel(Lenght, Panel1, Panel2, Panel3, Panel4, FrameOffset);
        }

        private void Panel2_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SumPanel(Lenght, Panel1, Panel2, Panel3, Panel4, FrameOffset);
        }

        private void Panel3_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SumPanel(Lenght, Panel1, Panel2, Panel3, Panel4, FrameOffset);
        }

        private void Panel4_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SumPanel(Lenght, Panel1, Panel2, Panel3, Panel4, FrameOffset);
        }
        
        private void ModelName1_LayoutUpdated(object sender, EventArgs e)
        {
            if (!(WidhFilter != null & ModelName1 != null)) return;

            var itemTo = ModelName1.SelectedItem as ComboBoxItem;
            if (itemTo == null)
            {
                return;
            }
            WidhFilter.Text = Convert.ToString(Convert.ToDouble(itemTo.ToolTip.ToString()) + 50);
        }

        private void WidhFilter_LayoutUpdated(object sender, EventArgs e)
        {
            Panel1.Text = Convert.ToString(Convert.ToDouble(WidhFilter.Text) + 50);
        }

        private void Panel7_LayoutUpdated(object sender, EventArgs e)
        {
            Panel2.Text = Convert.ToString(Convert.ToDouble(WidhTo1.Text) + 50);
        }

        private void ModelName2_LayoutUpdated(object sender, EventArgs e)
        {
            if (!(WidhTo1 != null & ModelName2 != null)) return;

            var itemTo = ModelName2.SelectedItem as ComboBoxItem;
            if (itemTo == null)
            {
                return;
            }
            WidhTo1.Text = Convert.ToString(Convert.ToDouble(itemTo.ToolTip.ToString()) + 50);
        }

    }
}
