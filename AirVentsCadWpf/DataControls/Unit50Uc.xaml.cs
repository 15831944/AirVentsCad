using System;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.Properties;
using VentsMaterials;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for Unit50UC.xaml
    /// </summary>
    public partial class Unit50Uc
    {

        readonly SqlBaseData _sqlBaseData = new SqlBaseData();
        readonly SetMaterials _setMaterials = new SetMaterials();
        readonly ToSQL _toSql = new ToSQL();

        /// <summary>
        /// Initializes a new instance of the <see cref="Unit50Uc"/> class.
        /// </summary>
        public Unit50Uc()
        {
            InitializeComponent();
            
            ToSQL.Conn = Settings.Default.ConnectionToSQL;

            InnerPartGrid.Visibility = Visibility.Collapsed;
            
            var sqlBaseData = new SqlBaseData();

            var airVentsStandardSize = sqlBaseData.AirVentsStandardSize();
            SizeOfUnit.ItemsSource = ((IListSource)airVentsStandardSize).GetList();
            SizeOfUnit.DisplayMemberPath = "Type";
            SizeOfUnit.SelectedIndex = 0;

            SectionTextBox.ItemsSource = Totals.SectionLetters;

            TypeOfPanel.ItemsSource = (SqlBaseData.Profils.GetList());
            TypeOfPanel.DisplayMemberPath = "Description";
            TypeOfPanel.SelectedValuePath = "ProfilID";
            TypeOfPanel.SelectedIndex = 0;


            #region UNIT50FULL

            Lenght.MaxLength = 5;
            WidthU.MaxLength = 5;
            WidthU.IsReadOnly = true;
            HeightU.MaxLength = 5;
            HeightU.IsReadOnly = true;
            Lenght.MaxLength = 5;

            #endregion

            Totals.SetPanelType(TypeOfPanel50);
            Totals.SetMaterial(MaterialP1);
            Totals.SetMaterial(MaterialP2);

            ТолщинаВнешней.ItemsSource = Totals.SheetMetalThikness;
            ТолщинаВнешней.SelectedIndex = 2;
            ТолщинаВннутренней.ItemsSource = Totals.SheetMetalThikness;
            ТолщинаВннутренней.SelectedIndex = 2;

            Totals.SetRal(Ral1);
            Totals.SetRal(Ral2);
            Ral1.Visibility = Visibility.Hidden;
            Ral2.Visibility = Visibility.Hidden;
            Totals.SetCoatingType(CoatingType1);
            Totals.SetCoatingType(CoatingType2);
            Totals.SetCoatingClass(CoatingClass1);
            Totals.SetCoatingClass(CoatingClass2);

            PanelGrid.Visibility = Visibility.Collapsed;
            InnerGrid.Visibility = Visibility.Collapsed;
            GridMontageFrame.Visibility = Visibility.Collapsed;
            GridRoof.Visibility = Visibility.Collapsed;

            WidthLabel1.Visibility = Visibility.Collapsed;
            WidthRoof.Visibility = Visibility.Collapsed;
            HeightLabel1.Visibility = Visibility.Collapsed;
            LenghtRoof.Visibility = Visibility.Collapsed;
            
            #region TypeOfUnit50

            ModelOfInnerLabel.Visibility = Visibility.Collapsed;
            ModelOfInner.Visibility = Visibility.Collapsed;
            AddTypeLabel.Visibility = Visibility.Collapsed;
            AddType.Visibility = Visibility.Collapsed;

            #endregion

            #region MontageFrame50 Initialize

            FrameOffset.MaxLength = 5;
            FrameOffset.IsReadOnly = true;
            FrameOffsetLabel.Visibility = Visibility.Collapsed;
            FrameOffset.Visibility = Visibility.Collapsed;
            
            #endregion;

            #region MontageFrame50 Initialize

            LenghtBaseFrame.Visibility = Visibility.Collapsed;
            WidthBaseFrame.Visibility = Visibility.Collapsed;

            Totals.SetMontageFrameMaterial(MaterialMontageFrame);
            FrameOffset.MaxLength = 5;
            FrameOffset.IsReadOnly = true;

            Totals.SetRal(RalFrame1);            
            RalFrame1.Visibility = Visibility.Hidden;
            Totals.SetCoatingType(CoatingTypeFrame1);
            Totals.SetCoatingClass(CoatingClassFrame1);           

            #endregion
        }

        void SizeOfUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (SizeOfUnit?.SelectedItem == null)
                {
                    return;
                }
                var id = Convert.ToInt32(((DataRowView)SizeOfUnit.SelectedItem)["SizeID"].ToString());
                var sqlBaseData = new SqlBaseData();
                var type = Convert.ToInt32(TypeOfPanel.SelectedValue);               
                var standartUnitSizes = sqlBaseData.StandartSize(id, type);

                switch(type)
                {
                    case 1:
                        thicknessOfPanel = "30";
                        break;
                    case 2:
                        thicknessOfPanel = "50";
                        break;
                    case 7:
                        thicknessOfPanel = "70";
                        break;
                    default:
                        thicknessOfPanel = "40";
                        break;
                }

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

        }

        void nonstandard_Checked(object sender, RoutedEventArgs e)
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

        string thicknessOfPanel { get; set; }

        void BUILDING_Click(object sender, RoutedEventArgs args)
        {
            var sw = new ModelSw();         

            switch (thicknessOfPanel)
            {
                case "30":
                    var frame = "";
                    try
                    {
                        if (MontageFrame50.IsChecked == true)
                        {
                            if (FrameOffset.Text == "")
                            {
                                try
                                {
                                    FrameOffset.Text = Convert.ToString((Convert.ToDouble(LenghtBaseFrame.Text) / 2));
                                }
                                catch (Exception)
                                {
                                    FrameOffset.Text = Convert.ToString((Convert.ToDouble(LenghtBaseFrame.Text) / 2));
                                }
                            }
                            frame =
                                sw.MontageFrameS(
                                    WidthBaseFrame.Text,
                                    LenghtBaseFrame.Text,
                                    Thikness.Text,
                                    TypeOfFrame.Text,
                                    FrameOffset.Text,
                                    MaterialMontageFrame.SelectedValue.ToString(),
                                    new[]
                                    {
                                        RalFrame1.Text, CoatingTypeFrame1.Text, CoatingClassFrame1.Text,
                                        RalFrame1.SelectedValue?.ToString() ?? ""
                                    },
                                    true);

                            FrameOffset.Text = "";
                        }

                        var mat1Code = "";
                        var mat2Code = "";

                        var viewRowMat1 = (DataRowView)MaterialP1.SelectedItem;
                        var row1 = viewRowMat1.Row;
                        if (row1 != null)
                            mat1Code = row1.Field<string>("CodeMaterial");
                        var viewRowMat2 = (DataRowView)MaterialP2.SelectedItem;
                        var row2 = viewRowMat2.Row;
                        if (row2 != null)
                            mat2Code = row2.Field<string>("CodeMaterial");

                        #region Панели

                        var materialP1 = new[] { MaterialP1.SelectedValue.ToString(), ТолщинаВнешней.Text, MaterialP1.Text, mat1Code };
                        var materialP2 = new[] { MaterialP2.SelectedValue.ToString(), ТолщинаВннутренней.Text, MaterialP2.Text, mat2Code };

                        string panelWxL = null;
                        string panelHxL = null;
                        string panelHxL04 = null;

                        if (Panel50.IsChecked == true)
                        {
                            try
                            {
                                //Верх - Низ
                                try
                                {
                                     sw.Panels30Build(
                                            typeOfPanel:
                                            new[]
                                            {
                                                SqlBaseData.PanelsTable().Rows[0][2].ToString(),
                                                SqlBaseData.PanelsTable().Rows[0][1].ToString(),
                                                "30"
                                            },
                                            width: Convert.ToString(Convert.ToInt32(Lenght.Text) - 60),
                                            height: Convert.ToString(Convert.ToInt32(WidthU.Text) - 60),
                                            materialP1: materialP1,
                                            materialP2: materialP2,
                                            покрытие: new[]
                                            {
                                                Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                                                Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                                                Ral1.SelectedValue?.ToString() ?? "",
                                                Ral2.SelectedValue?.ToString() ?? ""
                                            },
                                            path: out panelWxL);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                                //Несъемная
                                try
                                {
                                        sw.Panels30Build(
                                            typeOfPanel:
                                            new[]
                                            {
                                                SqlBaseData.PanelsTable().Rows[0][2].ToString(),
                                                SqlBaseData.PanelsTable().Rows[0][1].ToString(),
                                                "30"
                                            },
                                            width: Convert.ToString(Convert.ToInt32(Lenght.Text) - 60),
                                            height: Convert.ToString(Convert.ToInt32(HeightU.Text) - 60),
                                            materialP1: materialP1,
                                            materialP2: materialP2,
                                            покрытие: new[]
                                            {
                                                Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                                                Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                                                Ral1.SelectedValue?.ToString() ?? "",
                                                Ral2.SelectedValue?.ToString() ?? ""
                                            },
                                            path: out panelHxL);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                                //Cъемная
                                try
                                {
                                        sw.Panels30Build(
                                            typeOfPanel: new[]
                                            {
                                                TypeOfPanel50.SelectedValue.ToString(),
                                                TypeOfPanel50.Text,
                                                "30"
                                            },
                                            width: Convert.ToString(Convert.ToInt32(Lenght.Text) - 60),
                                            height: Convert.ToString(Convert.ToInt32(HeightU.Text) - 60),
                                            materialP1: materialP1,
                                            materialP2: materialP2,
                                            покрытие: new[]
                                            {
                                                Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                                                Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                                                Ral1.SelectedValue?.ToString() ?? "",
                                                Ral2.SelectedValue?.ToString() ?? ""
                                            },
                                            path: out panelHxL04);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                        }

                        #endregion

                        var panels = new[] { panelWxL, panelHxL, panelHxL04 };

                        var roofType = TypeOfRoof.Text;
                        if (RoofOfUnit50.IsChecked != true) { roofType = ""; }

                        sw.UnitAsmbly30(((DataRowView)SizeOfUnit.SelectedItem)["Type"].ToString(), OrderTextBox.Text, SideService.Text,
                            WidthU.Text, HeightU.Text, Lenght.Text, frame, panels, roofType, "Section " + SectionTextBox.Text);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                    break;

                default:
                    frame = "";
                    try
                    {
                        if (MontageFrame50.IsChecked == true)
                        {
                            if (FrameOffset.Text == "")
                            {
                                try
                                {
                                    FrameOffset.Text = Convert.ToString((Convert.ToDouble(LenghtBaseFrame.Text) / 2));
                                }
                                catch (Exception)
                                {
                                    FrameOffset.Text = Convert.ToString((Convert.ToDouble(LenghtBaseFrame.Text) / 2));
                                }
                            }
                            frame =
                                sw.MontageFrameS(
                                    WidthBaseFrame.Text,
                                    LenghtBaseFrame.Text,
                                    Thikness.Text,
                                    TypeOfFrame.Text,
                                    FrameOffset.Text,
                                    MaterialMontageFrame.SelectedValue.ToString(),
                                    new[]
                                    {
                                        RalFrame1.Text, CoatingTypeFrame1.Text, CoatingClassFrame1.Text,
                                        RalFrame1.SelectedValue?.ToString() ?? ""
                                    },
                                    true);

                            FrameOffset.Text = "";
                        }

                        var mat1Code = "";
                        var mat2Code = "";

                        var viewRowMat1 = (DataRowView)MaterialP1.SelectedItem;
                        var row1 = viewRowMat1.Row;
                        if (row1 != null)
                            mat1Code = row1.Field<string>("CodeMaterial");
                        var viewRowMat2 = (DataRowView)MaterialP2.SelectedItem;
                        var row2 = viewRowMat2.Row;
                        if (row2 != null)
                            mat2Code = row2.Field<string>("CodeMaterial");

                        #region Панели

                        var materialP1 = new[] { MaterialP1.SelectedValue.ToString(), ТолщинаВнешней.Text, MaterialP1.Text, mat1Code };
                        var materialP2 = new[] { MaterialP2.SelectedValue.ToString(), ТолщинаВннутренней.Text, MaterialP2.Text, mat2Code };

                        string panelWxL = null;
                        string panelHxL = null;
                        string panelHxL04 = null;

                        var panelsDelta = TypeOfPanel.SelectedIndex == 0 ? 100 : 140;

                        if (Panel50.IsChecked == true)
                        {
                            try
                            {
                                //Верх - Низ
                                try
                                {
                                    panelWxL =
                                        sw.Panels50BuildStr(
                                            typeOfPanel:
                                            new[]
                                            {
                                                SqlBaseData.PanelsTable().Rows[0][2].ToString(),
                                                SqlBaseData.PanelsTable().Rows[0][1].ToString(),
                                                TypeOfPanel.SelectedIndex==0 ? "50":"70"
                                            },
                                            width: Convert.ToString(Convert.ToInt32(Lenght.Text) - panelsDelta),
                                            height: Convert.ToString(Convert.ToInt32(WidthU.Text) - panelsDelta),
                                            materialP1: materialP1,
                                            materialP2: materialP2,
                                            покрытие: new[]
                                            {
                                                Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                                                Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                                                Ral1.SelectedValue?.ToString() ?? "",
                                                Ral2.SelectedValue?.ToString() ?? ""
                                            },
                                            onlyPath: true);
                                }
                                catch (Exception)
                                {
                                    // 
                                }

                                //Несъемная
                                try
                                {
                                    panelHxL =
                                        sw.Panels50BuildStr(
                                            typeOfPanel:
                                            new[]
                                            {
                                                SqlBaseData.PanelsTable().Rows[0][2].ToString(),
                                                SqlBaseData.PanelsTable().Rows[0][1].ToString(),
                                                TypeOfPanel.SelectedIndex==0 ? "50":"70"
                                            },
                                            width: Convert.ToString(Convert.ToInt32(Lenght.Text) - panelsDelta),
                                            height: Convert.ToString(Convert.ToInt32(HeightU.Text) - panelsDelta),
                                            materialP1: materialP1,
                                            materialP2: materialP2,
                                            покрытие: new[]
                                            {
                                                Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                                                Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                                                Ral1.SelectedValue?.ToString() ?? "",
                                                Ral2.SelectedValue?.ToString() ?? ""
                                            },
                                            onlyPath: true);
                                }
                                catch (Exception)
                                {
                                    // 
                                }

                                //Cъемная
                                try
                                {
                                    panelHxL04 =
                                        sw.Panels50BuildStr(
                                            typeOfPanel: new[]
                                            {
                                                TypeOfPanel50.SelectedValue.ToString(),
                                                TypeOfPanel50.Text,
                                                TypeOfPanel.SelectedIndex==0 ? "50":"70"
                                            },
                                            width: Convert.ToString(Convert.ToInt32(Lenght.Text) - panelsDelta),
                                            height: Convert.ToString(Convert.ToInt32(HeightU.Text) - panelsDelta),
                                            materialP1: materialP1,
                                            materialP2: materialP2,
                                            покрытие: new[]
                                            {
                                                Ral1.Text, CoatingType1.Text, CoatingClass1.Text,
                                                Ral2.Text, CoatingType2.Text, CoatingClass2.Text,
                                                Ral1.SelectedValue?.ToString() ?? "",
                                                Ral2.SelectedValue?.ToString() ?? ""
                                            },
                                            onlyPath: true);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                        }

                        #endregion
                        
                        var panels = new[]{panelWxL , panelHxL, panelHxL04};
                        
                        var roofType = TypeOfRoof.Text;
                        if (RoofOfUnit50.IsChecked != true){roofType = "";}

                        sw.UnitAsmbly(((DataRowView)SizeOfUnit.SelectedItem)["Type"].ToString(), OrderTextBox.Text, SideService.Text,
                            WidthU.Text, HeightU.Text, Lenght.Text, frame, panels, roofType, "Section " + SectionTextBox.Text,
                            thicknessOfPanel == "50" ? "150" : "170");
                    }

                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                    break;
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        void HeightU_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter){BUILDING_Click(this, new RoutedEventArgs());}
        }

        void WidthU_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter){BUILDING_Click(this, new RoutedEventArgs());}
        }

        void TypeOfFrame_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        
        void FrameOffset_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }
        
        void TypeOfUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(TypeOfUnit != null & ModelOfInnerLabel != null & AddTypeLabel != null)) return;
            switch (TypeOfUnit.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", ""))
            {
                case "Пустой":
                    ModelOfInnerLabel.Visibility = Visibility.Collapsed;
                    ModelOfInner.Visibility = Visibility.Collapsed;
                    AddTypeLabel.Visibility = Visibility.Collapsed;
                    AddType.Visibility = Visibility.Collapsed;
                    break;
                case "Вентилятора":
                    break;
                case "Фильтра":
                    break;
                default:
                    ModelOfInnerLabel.Visibility = Visibility.Collapsed;
                    ModelOfInner.Visibility = Visibility.Collapsed;
                    AddTypeLabel.Visibility = Visibility.Collapsed;
                    AddType.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        void Lenght_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void LenghtRoof_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void WidthRoof_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void RoofOfUnit50_Checked(object sender, RoutedEventArgs e)
        {
            GridRoof.Visibility = Visibility.Visible;
        }

        void RoofOfUnit50_Unchecked(object sender, RoutedEventArgs e)
        {
            GridRoof.Visibility = Visibility.Collapsed;
        }

        void InnerOfUnit50_Checked(object sender, RoutedEventArgs e)
        {
            InnerGrid.Visibility = Visibility.Visible;
        }

        void InnerOfUnit50_Unchecked(object sender, RoutedEventArgs e)
        {
            InnerGrid.Visibility = Visibility.Collapsed;
        }

        void Panel50_Checked(object sender, RoutedEventArgs e)
        {
            PanelGrid.Visibility = Visibility.Visible;
        }

        void Panel50_Unchecked(object sender, RoutedEventArgs e)
        {
            PanelGrid.Visibility = Visibility.Collapsed;
        }

        void MontageFrame50_Checked(object sender, RoutedEventArgs e)
        {
            GridMontageFrame.Visibility = Visibility.Visible;
        }

        void MontageFrame50_Unchecked(object sender, RoutedEventArgs e)
        {
            GridMontageFrame.Visibility = Visibility.Collapsed;
        }

        void Ral1_LayoutUpdated(object sender, EventArgs e)
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

        void Ral2_LayoutUpdated(object sender, EventArgs e)
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

        void MaterialP1_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        void MaterialP2_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        void Ral1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RalFrame1 == null) return;
            if (RalFrame1.SelectedIndex == 0)
            {
                CoatingTypeFrame1.Visibility = Visibility.Hidden;
                CoatingClassFrame1.Visibility = Visibility.Hidden;
            }
            else
            {
                CoatingTypeFrame1.Visibility = Visibility.Visible;
                CoatingClassFrame1.Visibility = Visibility.Visible;
            }
        }

        void WidthU_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WidthBaseFrame != null) WidthBaseFrame.Text = WidthU.Text;
        }

        void Lenght_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LenghtBaseFrame != null) LenghtBaseFrame.Text = Lenght.Text;
        }

        private void TypeOfPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SizeOfUnit_SelectionChanged(sender, e);
        }
    }
}

