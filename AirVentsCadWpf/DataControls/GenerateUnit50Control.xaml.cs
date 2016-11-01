using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AirVentsCadWpf.AirVentsClasses;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;


namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// 
    /// </summary>
    public partial class GenerateUnit50Control
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateUnit50Control"/> class.
        /// </summary>
        public GenerateUnit50Control()
        {
            InitializeComponent();
            InitWindow();
        }

        void InitWindow()
        {
            GridTypeOfUnit50.Visibility = Visibility.Collapsed;
            //AddSizes.Visibility = Visibility.Collapsed;
            TypeOfUnit50.Visibility = Visibility.Collapsed;

            #region UNIT50FULL

            Lenght.MaxLength = 5;
            WidthU.MaxLength = 5;
            WidthU.IsReadOnly = true;
            HeightU.MaxLength = 5;
            HeightU.IsReadOnly = true;

            #endregion

            #region TypeOfUnit50

            ModelOfInnerLabel.Visibility = Visibility.Collapsed;
            ModelOfInner.Visibility = Visibility.Collapsed;
            AddTypeLabel.Visibility = Visibility.Collapsed;
            AddType.Visibility = Visibility.Collapsed;

            #endregion

            #region Panel50

            HeightPanel.MaxLength = 5;
            WidthPanel.MaxLength = 5;

            #endregion

            #region MontageFrame50 Initialize

            FrameOffset.MaxLength = 5;
            FrameOffset.IsReadOnly = true;
            FrameOffsetLabel.Visibility = Visibility.Collapsed;
            FrameOffset.Visibility = Visibility.Collapsed;
            LenghtBaseFrame.MaxLength = 5;
            LenghtBaseFrame.IsReadOnly = true;
            WidthBaseFrame.MaxLength = 5;
            WidthBaseFrame.IsReadOnly = true;
            // MFrame
            WidthBaseFramelabel.Visibility = Visibility.Collapsed;
            WidthBaseFrame.Visibility = Visibility.Collapsed;
            LenghtBaseFrame.Visibility = Visibility.Collapsed;
            LenghtBaseFrameLabel.Visibility = Visibility.Collapsed;

            #endregion;
        }

        void BUILDING_Click(object sender, RoutedEventArgs e)
        {
            _bw.RunWorkerAsync();
            #region to delete

            //Dispatcher.Thread.SetApartmentState(new ApartmentState());
            //Dispatcher.InvokeAsync(() =>
            //{
            //                            try
            //                            {
            //                                var sw = new Unit50Sw();


            //                                if (Unit50Full.IsChecked == true)
            //                                {
            //                                    if (FrameOffset.Text == "")
            //                                    {
            //                                        try
            //                                        {
            //                                            FrameOffset.Text = (Convert.ToDouble(Lenght.Text)/2).ToString();
            //                                        }
            //                                        catch
            //                                        {
            //                                        }
            //                                    }


            //                                    Dispatcher.Invoke(
            //                                        () =>
            //                                            sw.UnitAsmbly(SizeOfUnit.Text, "00", WidthU.Text, HeightU.Text,
            //                                                Lenght.Text, Thikness.Text, TypeOfFrame.Text,
            //                                                FrameOffset.Text));
            //                                    //  Parallel.Invoke(() => sw.UnitAsmbly(SizeOfUnit.Text, "00", WidthU.Text, HeightU.Text, Lenght.Text, Thikness.Text, TypeOfFrame.Text, FrameOffset.Text));
            //                                    //sw.UnitAsmbly(SizeOfUnit.Text, "00", WidthU.Text, HeightU.Text,
            //                                    //    Lenght.Text, Thikness.Text, TypeOfFrame.Text, FrameOffset.Text);
            //                                }
            //                                else if (MontageFrame50.IsChecked == true)
            //                                {
            //                                    if (FrameOffset.Visibility == Visibility.Collapsed &
            //                                        LenghtBaseFrame.Text != "")
            //                                    {
            //                                        try
            //                                        {
            //                                            FrameOffset.Text =
            //                                                (Convert.ToDouble(LenghtBaseFrame.Text)/2).ToString();
            //                                        }
            //                                        catch (Exception)
            //                                        {
            //                                        }
            //                                    }
            //                                    sw.MontageFrame(WidthBaseFrame.Text, LenghtBaseFrame.Text, Thikness.Text,
            //                                        TypeOfFrame.Text, FrameOffset.Text, MaterialMontageFrame.Text);
            //                                }
            //                                else if (TypeOfUnit50.IsChecked == true)
            //                                {

            //                                }
            //                                else if (Panel50.IsChecked == true)
            //                                {
            //                                    sw.Panels50Build(TypeOfPanel50.Text, WidthPanel.Text, HeightPanel.Text,
            //                                        MaterialP1.Text, MaterialP2.Text);
            //                                }
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                MessageBox.Show(ex.Message);
            //                            }
            //});

            #endregion
        }

        #region Unit50Full

        void SizeOfUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        void Width_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (WidthBaseFrame != null)
            {
                WidthBaseFrame.Text = WidthU.Text;
            }
        }

        void UNIT50FULL_Checked(object sender, RoutedEventArgs e)
        {
            LenghtBaseFrame.Text = Lenght.Text;
            LenghtBaseFrame.IsReadOnly = true;
            WidthBaseFrame.Text = WidthU.Text;
            WidthBaseFrame.IsReadOnly = true;
        }

        void UNIT50FULL_Loaded(object sender, RoutedEventArgs e)
        {
            //  Unit50Full.IsChecked = true;
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

        #endregion

        #region MontageFrame50

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

        void MontageFrame50_Checked(object sender, RoutedEventArgs e)
        {
            LenghtBaseFrame.IsReadOnly = false;
            WidthBaseFrame.IsReadOnly = false;

            WidthBaseFramelabel.Visibility = Visibility.Visible;
            WidthBaseFrame.Visibility = Visibility.Visible;
            LenghtBaseFrame.Visibility = Visibility.Visible;
            LenghtBaseFrameLabel.Visibility = Visibility.Visible;
        }

        void MontageFrame50_Unchecked(object sender, RoutedEventArgs e)
        {
            WidthBaseFramelabel.Visibility = Visibility.Collapsed;
            WidthBaseFrame.Visibility = Visibility.Collapsed;
            LenghtBaseFrame.Visibility = Visibility.Collapsed;
            LenghtBaseFrameLabel.Visibility = Visibility.Collapsed;
        }

        void Thikness_SelectionChanged(object sender, SelectionChangedEventArgs e)
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


        void NonstandardMontageFrame_Initialized(object sender, EventArgs e)
        {
            NonstandardMontageFrame.Unchecked += NonstandardMontageFrameOnUnchecked;
            NonstandardMontageFrame.Checked += NonstandardMontageFrameOnChecked;
        }

        void NonstandardMontageFrameOnChecked(object sender, RoutedEventArgs routedEventArgs)
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

        void NonstandardMontageFrameOnUnchecked(object sender, RoutedEventArgs routedEventArgs)
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

        #endregion              

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

        readonly BackgroundWorker _bw = new BackgroundWorker();

        static readonly Action EmptyDelegate = delegate
        {
            try
            {
                var sw = new ModelSw();
                sw.Panels50Build(
                    typeOfPanel: new[] { null, "Съемная" }, 
                    width: "1125", 
                    height: "254", 
                    materialP1: new[] { "Az", null },
                    meterialP2: new[] { "Az", null }, 
                    покрытие: null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        };
        
        void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            _bw.DoWork += ((o, args) =>
            {
                Dispatcher.Invoke(DispatcherPriority.Background, EmptyDelegate);
               // Dispatcher.Thread.SetApartmentState(new ApartmentState());
                Dispatcher.InvokeAsync( ()=>
            {
                                        try
                                        {
                                            var sw = new ModelSw();


                                            if (Unit50Full.IsChecked == true)
                                            {
                                                if (FrameOffset.Text == "")
                                                {
                                                    try
                                                    {
                                                        FrameOffset.Text = Convert.ToString(Convert.ToDouble(Lenght.Text) / 2);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        FrameOffset.Text = Convert.ToString(Convert.ToDouble(Lenght.Text) / 2);
                                                    }
                                                }


                                                //Dispatcher.Invoke(
                                                //    () =>
                                                //        sw.UnitAsmbly(SizeOfUnit.Text, "00", "",WidthU.Text, HeightU.Text,
                                                //            Lenght.Text, Thikness.Text, TypeOfFrame.Text,
                                                //            FrameOffset.Text, new[] { null, TypeOfPanel50.Text }, MaterialP1.Text, MaterialP2.Text, "", ""));
                                                ////  Parallel.Invoke(() => sw.UnitAsmbly(SizeOfUnit.Text, "00", WidthU.Text, HeightU.Text, Lenght.Text, Thikness.Text, TypeOfFrame.Text, FrameOffset.Text));
                                                //sw.UnitAsmbly(SizeOfUnit.Text, "00", WidthU.Text, HeightU.Text,
                                                //    Lenght.Text, Thikness.Text, TypeOfFrame.Text, FrameOffset.Text);
                                            }
                                            else if (MontageFrame50.IsChecked == true)
                                            {
                                                if (FrameOffset.Visibility == Visibility.Collapsed &
                                                    LenghtBaseFrame.Text != "")
                                                {
                                                    try
                                                    {
                                                        FrameOffset.Text = Convert.ToString((Convert.ToDouble(LenghtBaseFrame.Text)/2));
                                                    }
                                                    catch (Exception)
                                                    {
                                                        FrameOffset.Text = Convert.ToString(150);
                                                    }
                                                }
                                                
                                                sw.MontageFrame(WidthBaseFrame.Text, LenghtBaseFrame.Text, Thikness.Text,
                                                    TypeOfFrame.Text, FrameOffset.Text, MaterialMontageFrame.Text, null);
                                            }
                                            else if (TypeOfUnit50.IsChecked == true)
                                            {

                                            }
                                            else if (Panel50.IsChecked == true)
                                            {
                                                sw.Panels50Build(
                                                    new[] { null, TypeOfPanel50.Text }, 
                                                    WidthPanel.Text, 
                                                    HeightPanel.Text,
                                                    new[] { "Az", null }, 
                                                    new[] { "Az", null }, 
                                                    null);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show(ex.Message);
                                        }
            });
            });
            //Dispatcher.Thread.SetApartmentState(new ApartmentState());
            Dispatcher.InvokeAsync(() =>
            {
                //var sw = new ModelSw();
                //sw.CreateDistDirectory(string.Format(@"{0}\{1}", @Properties.Settings.Default.DestinationFolder,
                //    sw.Unit50Folder));
                //sw.CreateDistDirectory(string.Format(@"{0}\{1}", @Properties.Settings.Default.DestinationFolder,
                //    sw.Panel50DestinationFolder));
                //sw.CreateDistDirectory(string.Format(@"{0}\{1}", @Properties.Settings.Default.DestinationFolder,
                //    sw.BaseFrameDestinationFolder));
            });
        }

        void Lenght_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void WidthU_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void HeightU_KeyDown(object sender, KeyEventArgs e)
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

        void HeightPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void FrameOffset_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void LenghtBaseFrame_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void WidthBaseFrame_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}

