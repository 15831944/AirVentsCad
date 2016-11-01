using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BomPartList;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;


namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for Bom.xaml
    /// </summary>
    public partial class Bom
    {
        
        #region Поля и начало работы

        const string PdmBaseName = "Vents-PDM";
        const string User = "kb81";
        const string Password = "1";
      //  readonly IEdmVault10 _mVault = new EdmVault5Class();
      //  IEdmFile7 _edmFile7;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bom"/> class.
        /// </summary>
        public Bom()
        {
            InitializeComponent();
            try
            {
                Login();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка");
            }
            
            BomTableAsm.ItemsSource = BomListAsm;
            BomTablePrt.ItemsSource = BomListPrt;
            BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);

         //   HelpSelectedPrtGrid.Visibility = Visibility.Collapsed;
            Expander1.Visibility = Visibility.Collapsed;
        }

        void Login()
        {
           // if (_mVault == null) return;
            try
            {
                //try
                //{
                //    _mVault.LoginAuto(PdmBaseName, 0);
                //}
                //catch (Exception)
                //{
                //    _mVault.Login(User, Password, PdmBaseName);
                //}

                
                //_mVault.CreateSearch();
                //if (_mVault.IsLoggedIn)
                //{
                //    ConnectToVault.Content = "Logged in " + PdmBaseName;
                //}
            }
            catch (COMException ex)
            {
                ConnectToVault.Content = "Failed to connect to " + PdmBaseName + " - " + ex.Message;
            }
        }

        bool GetFile()
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
           //     InitialDirectory = _mVault.RootFolderPath,
                Filter = "SolidWorks Assemblies|*.sldasm"
            };
            if (fileDialog.ShowDialog() != true) return false;
            var filename = fileDialog.FileName;
           // MessageBox.Show(filename);
          //  IEdmFolder5 folder;
        //    _edmFile7 = (IEdmFile7) _mVault.GetFileFromPath(filename, out folder);
            return true;
        }

        void Getbom()
        {
            //if (_edmFile7 == null) return;

            //var bomView = _edmFile7.GetComputedBOM(Convert.ToInt32(8), Convert.ToInt32(-1), "",
            //    (int) EdmBomFlag.EdmBf_ShowSelected);

            //if (bomView == null) return;
            //Array bomRows;
            //Array bomColumns;
            //bomView.GetRows(out bomRows);
            //bomView.GetColumns(out bomColumns);

            //var bomTable = new DataTable();

            //foreach (EdmBomColumn bomColumn in bomColumns)
            //{
            //    bomTable.Columns.Add(new DataColumn {ColumnName = bomColumn.mbsCaption});
            //}

            //bomTable.Columns.Add(new DataColumn{ ColumnName = "Путь" });
            //bomTable.Columns.Add(new DataColumn{ ColumnName = "Уровень" });
            ////bomTable.Columns.Add(new DataColumn { ColumnName = "Errors" });

            //for (var i = 0; i < bomRows.Length; i++)
            //{
            //    var cell = (IEdmBomCell) bomRows.GetValue(i);

            //    bomTable.Rows.Add();
            //    for (var j = 0; j < bomColumns.Length; j++)
            //    {
            //        var column = (EdmBomColumn) bomColumns.GetValue(j);
            //        object value;
            //        object computedValue;
            //        String config;
            //        bool readOnly;
            //        cell.GetVar(column.mlVariableID, column.meType, out value, out computedValue, out config,
            //            out readOnly);
            //        if (value != null)
            //        {
            //            bomTable.Rows[i][j] = value;
            //        }
            //        bomTable.Rows[i][j + 1] = cell.GetPathName();
            //        bomTable.Rows[i][j + 2] = cell.GetTreeLevel();
            //    }
            //}

            //BomList = BomTableToBomList(bomTable);
            //BomTable.ItemsSource = bomTable.DefaultView;
            //BomListAsm = _bomListAsm = BomList.Where(x => x.ТипФайла == "sldasm" & x.Уровень != "0" & x.Раздел == "Сборочные единицы").OrderBy(x => x.Обозначение).ToList();
            //BomListPrt = BomList.Where(x => x.ТипФайла == "sldprt" & x.Раздел == "" || x.Раздел == "Детали").OrderBy(x => x.Обозначение).ToList();

            #region LabelContent
            //var labelContentList = BomList.Where(x => x.Уровень == "0").ToList();
            //var labelContent = "";
            //foreach (var bomCells in labelContentList)
            //{
            //    labelContent = " Сборка - " + Path.GetFileNameWithoutExtension(bomCells.Путь) + " Наименование - " + bomCells.Наименование + " Конфигурация - " + bomCells.Конфигурация;
            //}
            //СборкаLbl.Content = labelContent;


            #endregion

        }

        private void Loggin_Click(object sender, RoutedEventArgs e)
        {
            if (GetFile())
            {
                Getbom();
            }
        }
        
        
        #endregion

        #region BomCells - Поля с х-ками деталей
        
        public class BomCells
        {
            public string Раздел { get; set; }
            public string Обозначение { get; set; }
            public string Наименование { get; set; }
            public string Материал { get; set; }
            public string МатериалЦми { get; set; }
            public string ТолщинаЛиста { get; set; }
            public string Количество { get; set; }
            public string ТипФайла { get; set; }
            public string Конфигурация { get; set; }
            public string ПоследняяВерсия { get; set; }
            public string АссоциированныйОбъект { get; set; }
            public string Путь { get; set; }
            public string Уровень { get; set; }
            public string Errors { get; set; }
        }

        public List<BomCells> BomList;

        private List<BomCells> _bomListAsm;

        List<BomCells> BomListAsm
        {
            get
            { return _bomListAsm; } 
            set
            {
                BomTableAsm.ItemsSource = value;
            }
        }

        public List<BomCells> BomListPrt
        {
            get
            { return null; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                BomTablePrt.ItemsSource = value;
            }
        }
        
        private List<BomCells> _bomListSelectedPrts = new List<BomCells>();
        

        private static List<BomCells> BomTableToBomList(DataTable table)
        {
            var bomList = new List<BomCells>(table.Rows.Count);
            bomList.AddRange(from DataRow row in table.Rows
                select row.ItemArray
                into values
                select new BomCells
                {
                    Раздел = values[0].ToString(),
                    Обозначение = values[1].ToString(),
                    Наименование = values[2].ToString(),
                    Материал = values[3].ToString(),
                    МатериалЦми = values[4].ToString(),
                    ТолщинаЛиста = values[5].ToString(),
                    Количество = values[6].ToString(),
                    ТипФайла = values[7].ToString(),
                    Конфигурация = values[8].ToString(),
                    ПоследняяВерсия = values[9].ToString(),
                    АссоциированныйОбъект = values[10].ToString(),
                    Путь = values[11].ToString(),
                    Уровень = values[12].ToString()
                });
            foreach (var bomCells in bomList)
            {
                bomCells.Errors = ErrorMessageForParts(bomCells);
            }
            return bomList;
        }

        static string ErrorMessageForParts(BomCells bomCells)
        {
            var обозначениеErr = "";
            var материалЦмиErr = "";
            var наименованиеErr = "";
            var разделErr = "";
            var толщинаЛистаErr = "";
            var конфигурацияErr = "";

            var messageErr = String.Format("Необходимо заполнить: {0}{1}{2}{3}{4}{5}",
               обозначениеErr,
               материалЦмиErr,
               наименованиеErr,
               разделErr,
               толщинаЛистаErr,
               конфигурацияErr);

            if (bomCells.Обозначение == "")
            {
                обозначениеErr = "\n Обозначение";
            }

            //if (bomCells.МатериалЦми == "")
            //{
            //    материалЦмиErr = "\n Материал Цми";
            //}

            if (bomCells.Наименование == "")
            {
                наименованиеErr = "\n Наименование";
            }

            if (bomCells.Раздел == "")
            {
                разделErr = "\n Раздел";
            }

            if (bomCells.ТолщинаЛиста == "")
            {
                толщинаЛистаErr = "\n ТолщинаЛиста";
            }

            var regex = new Regex("[^0-9]");

            if (regex.IsMatch(bomCells.Конфигурация))
            {
                конфигурацияErr = "\n Изменить имя конфигурации на численное значение";
            }

            var message = bomCells.Errors = String.Format("Необходимо заполнить: {0}{1}{2}{3}{4}{5}",
                обозначениеErr,
                материалЦмиErr,
                наименованиеErr,
                разделErr,
                толщинаЛистаErr,
                конфигурацияErr);

            return bomCells.Errors == messageErr ? "" : message;

            //var c = new Button { Margin = new Thickness(5, 5, 5, 5), Content = bomCells.Errors };
            //Grid.SetRow(c, 1);
            //Grid.SetColumn(c, 0);
            //PrtMessage.Children.Add(c);
        }

        #endregion

        #region Design

        private readonly SolidColorBrush _orangeColorBrush = new SolidColorBrush(Colors.LightPink);
        private readonly SolidColorBrush _whiteColorBrush = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush _lightCyanColorBrush = new SolidColorBrush(Colors.LightCyan);
        
        private readonly SolidColorBrush _redColorBrush = new SolidColorBrush(Colors.Red);
        

        void BomTable_LoadingRow(object sender, DataGridRowEventArgs e)
        {
             var product = (BomCells)e.Row.DataContext;

             var regex = new Regex("[^0-9]");

             if (!regex.IsMatch(product.Конфигурация) && product.Материал != "" && product.Раздел != ""
                 && product.ТолщинаЛиста != "")
             {
                 e.Row.Background = e.Row.GetIndex() % 2 == 1 ? _lightCyanColorBrush : _whiteColorBrush;
             }
             else
             {
                 e.Row.Background = _orangeColorBrush;
             }
        }

        void BomTableAsmCells(object sender, DataGridRowEventArgs e)
        {
            var product = (BomCells)e.Row.DataContext;

            if (product.Обозначение != "")
            {
                e.Row.Background = e.Row.GetIndex() % 2 == 1 ? _lightCyanColorBrush : _whiteColorBrush;
            }
            else
            {
                e.Row.Background = _orangeColorBrush;
            }
        }

        #endregion

        void BomTablePrt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((BomCells)(BomTablePrt.SelectedValue));
            BomTablePrt.ToolTip = row.Errors;

            //ErrorMessageForParts(row);

            //BomTablePrt.RowDetailsVisibilityMode = Convert.ToInt32(row.Количество)>1 ? DataGridRowDetailsVisibilityMode.Collapsed 
            //    : DataGridRowDetailsVisibilityMode.Visible;
            //   row.Description = row.Путь + "\n" + row.Обозначение + "\n" + row.Раздел;
            
            //   DynamicUi();
                        
            /*     var t = row.Количество;
             PrtMessage.Children.Add(new TextBox{Text = t});
            //  PrtMessage.Children.Add(new Label { Content = t + "- label" });
            MessageBox.Show("Количество - "+ t);*/
        }

        void Сборка_LayoutUpdated(object sender, EventArgs e)
        {
            if ((string)СборкаLbl.Content == "")
            {
                AsmGrid.Visibility = Visibility.Collapsed;
                PrtGrid.Visibility = Visibility.Collapsed;
                SelectedPrtGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                AsmGrid.Visibility = Visibility.Visible;
                PrtGrid.Visibility = Visibility.Visible;
                SelectedPrtGrid.Visibility = Visibility.Visible;
            }
        }
       
        void BomTablePrt_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((BomCells)(BomTablePrt.SelectedValue));
            if (_bomListSelectedPrts.Any(bomListSelectedPrt => row.Обозначение == bomListSelectedPrt.Обозначение)) return;
            _bomListSelectedPrts.Add(row);
            BomTableSelectedPrtUpdate();
        }

        void BomTableSelectedPrtUpdate()
        {
            BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);
        }



        #region Вспомогательные функции SolidWorks

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var sw = new ModelSw();
            sw.ReubildAllOpenedDocs();
        }

        #endregion

        void Button_Click_3(object sender, RoutedEventArgs e)
        {

            foreach (var bomListSelectedPrt in _bomListSelectedPrts)
            {
                try
                {
                    CreateFlattPatternUpdateCutlist(bomListSelectedPrt.Путь, false);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            MakeDxfButton_Click(sender, e);
            Focus();
            MessageBox.Show("Выгрузка данных прошла успешно!");
        }

        #region ToDelete

        private void DynamicUi()
        {
            var myBorder = new Border
            {
                Background = Brushes.LightBlue,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(45),
                Padding = new Thickness(25)
            };

            // Define the Grid.
            var myGrid = new Grid { Background = Brushes.White, ShowGridLines = true };

            // Define the Columns.
            var myColDef1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
            var myColDef2 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var myColDef3 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };

            // Add the ColumnDefinitions to the Grid.
            myGrid.ColumnDefinitions.Add(myColDef1);
            myGrid.ColumnDefinitions.Add(myColDef2);
            myGrid.ColumnDefinitions.Add(myColDef3);

            // Add the first child StackPanel.
            var myStackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(myStackPanel, 0);
            Grid.SetRow(myStackPanel, 0);
            var myTextBlock1 = new TextBlock
            {
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15),
                Text = "StackPanel 1"
            };
            var myButton1 = new Button { Margin = new Thickness(0, 10, 0, 10), Content = "Button 1" };
            var myButton2 = new Button { Margin = new Thickness(0, 10, 0, 10), Content = "Button 2" };
            var myButton3 = new Button { Margin = new Thickness(0, 10, 0, 10) };
            var myTextBlock2 = new TextBlock { Text = @"ColumnDefinition.Width = ""Auto""" };
            var myTextBlock3 = new TextBlock { Text = @"StackPanel.HorizontalAlignment = ""Left""" };
            var myTextBlock4 = new TextBlock { Text = @"StackPanel.VerticalAlignment = ""Top""" };
            var myTextBlock5 = new TextBlock { Text = @"StackPanel.Orientation = ""Vertical""" };
            var myTextBlock6 = new TextBlock { Text = @"Button.Margin = ""1,10,0,10""" };
            myStackPanel.Children.Add(myTextBlock1);
            myStackPanel.Children.Add(myButton1);
            myStackPanel.Children.Add(myButton2);
            myStackPanel.Children.Add(myButton3);
            myStackPanel.Children.Add(myTextBlock2);
            myStackPanel.Children.Add(myTextBlock3);
            myStackPanel.Children.Add(myTextBlock4);
            myStackPanel.Children.Add(myTextBlock5);
            myStackPanel.Children.Add(myTextBlock6);

            // Add the second child StackPanel.
            var myStackPanel2 = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Vertical
            };
            Grid.SetColumn(myStackPanel2, 1);
            Grid.SetRow(myStackPanel2, 0);
            var myTextBlock7 = new TextBlock
            {
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15),
                Text = "StackPanel 2"
            };
            var myButton4 = new Button { Margin = new Thickness(10, 0, 10, 0), Content = "Button 4" };
            var myButton5 = new Button { Margin = new Thickness(10, 0, 10, 0), Content = "Button 5" };
            var myButton6 = new Button { Margin = new Thickness(10, 0, 10, 0), Content = "Button 6" };
            var myTextBlock8 = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = @"ColumnDefinition.Width = ""*"""
            };
            var myTextBlock9 = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = @"StackPanel.HorizontalAlignment = ""Stretch"""
            };
            var myTextBlock10 = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = @"StackPanel.VerticalAlignment = ""Top"""
            };
            var myTextBlock11 = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = @"StackPanel.Orientation = ""Horizontal"""
            };
            var myTextBlock12 = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = @"Button.Margin = ""10,0,10,0"""
            };
            myStackPanel2.Children.Add(myTextBlock7);
            myStackPanel2.Children.Add(myButton4);
            myStackPanel2.Children.Add(myButton5);
            myStackPanel2.Children.Add(myButton6);
            myStackPanel2.Children.Add(myTextBlock8);
            myStackPanel2.Children.Add(myTextBlock9);
            myStackPanel2.Children.Add(myTextBlock10);
            myStackPanel2.Children.Add(myTextBlock11);
            myStackPanel2.Children.Add(myTextBlock12);

            // Add the final child StackPanel.
            var myStackPanel3 = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(myStackPanel3, 2);
            Grid.SetRow(myStackPanel3, 0);
            var myTextBlock13 = new TextBlock
            {
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15),
                Text = "StackPanel 3"
            };
            var myButton7 = new Button { Margin = new Thickness(10), Content = "Button 7" };
            var myButton8 = new Button { Margin = new Thickness(10), Content = "Button 8" };
            var myButton9 = new Button { Margin = new Thickness(10), Content = "Button 9" };
            var myTextBlock14 = new TextBlock { Text = @"ColumnDefinition.Width = ""Auto""" };
            var myTextBlock15 = new TextBlock { Text = @"StackPanel.HorizontalAlignment = ""Left""" };
            var myTextBlock16 = new TextBlock { Text = @"StackPanel.VerticalAlignment = ""Top""" };
            var myTextBlock17 = new TextBlock { Text = @"StackPanel.Orientation = ""Vertical""" };
            var myTextBlock18 = new TextBlock { Text = @"Button.Margin = ""10""" };
            myStackPanel3.Children.Add(myTextBlock13);
            myStackPanel3.Children.Add(myButton7);
            myStackPanel3.Children.Add(myButton8);
            myStackPanel3.Children.Add(myButton9);
            myStackPanel3.Children.Add(myTextBlock14);
            myStackPanel3.Children.Add(myTextBlock15);
            myStackPanel3.Children.Add(myTextBlock16);
            myStackPanel3.Children.Add(myTextBlock17);
            myStackPanel3.Children.Add(myTextBlock18);

            // Add child content to the parent Grid.
            myGrid.Children.Add(myStackPanel);
            myGrid.Children.Add(myStackPanel2);
            myGrid.Children.Add(myStackPanel3);

            // Add the Grid as the lone child of the Border.
            myBorder.Child = myGrid;

            // Add the Border to the Window as Content and show the Window.
            PrtMessage.Children.Add(myBorder);
        }

        #endregion
       
        void MakeDxfButton_Click(object sender, RoutedEventArgs e)
        {
            _bomListSelectedPrts.Clear();
        }

        #region ModelCode

        public static SldWorks SwApp;
        public static ModelDoc2 SwModel;
        public static DrawingDoc SwDraw;

        //public static IEdmObject5 GetObject(IEdmVault12 vault, int objectId, EdmObjectType objectType)
        //{
        //    try
        //    {
        //        var obj = vault.GetObject(objectType, objectId);
        //        return obj;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Ошибка в процедуре  public static IEdmObject5 GetObject {0}:", e);
        //    }
        //    foreach (EdmObjectType enumObjectType in Enum.GetValues(typeof(EdmObjectType)))
        //    {
        //        try
        //        {
        //            var obj = vault.GetObject(enumObjectType, objectId);
        //            return obj;
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine("Ошибка в процедуре  public static IEdmObject5 GetObject {0}:", e);
        //        }
        //    }
        //    //nothing found  
        //    return null;
        //}

        static void CreateFlattPatternUpdateCutlist(string filePath, bool savedxf)
        {
            //var vault1 = new EdmVault5();
            //IEdmFolder5 oFolder;
            //vault1.LoginAuto("Vents-PDM", 0);
            //var edmFile5 = vault1.GetFileFromPath("D:\\Vents-PDM\\Библиотека проектирования\\Templates\\flattpattern.drwdot", out oFolder);
            //edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_Simple);

            try
            {
                SwApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            }
            catch (Exception)
            {
                SwApp = new SldWorks { Visible = true };
            }
            if (SwApp == null) { return; }

            var swModel = SwApp.OpenDoc6(filePath, (int)swDocumentTypes_e.swDocPART,
                            (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            SwApp.SetUserPreferenceStringValue(((int)(swUserPreferenceStringValue_e.swFileLocationsDocumentTemplates)), "D:\\Vents-PDM\\Библиотека проектирования\\Templates\\");
            try
            {
                if (!IsSheetMetalPart((IPartDoc)swModel))
                {
                    SwApp.CloseDoc(swModel.GetTitle());
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }

            var activeconfiguration = (Configuration)swModel.GetActiveConfiguration();

            var swModelConfNames = (string[])swModel.GetConfigurationNames();
            foreach (var name in from name in swModelConfNames
                                 let config = (Configuration)swModel.GetConfigurationByName(name)
                                 where config.IsDerived()
                                 select name)
            {
                swModel.DeleteConfiguration(name);
            }

            var swModelDocExt = swModel.Extension;
            var swModelConfNames2 = (string[])swModel.GetConfigurationNames();

            // Проход по всем родительским конфигурациям (т.е. - конфигурациям деталей)


            var dataList = new List<DataToExport>();

            foreach (var configName in from name in swModelConfNames2
                                       let config = (Configuration)swModel.GetConfigurationByName(name)
                                       where !config.IsDerived()
                                       select name)
            {

                swModel.ShowConfiguration2(configName); swModel.EditRebuild3();

                var confiData = new DataToExport { Config = configName };
                var swDraw =
                    (DrawingDoc)SwApp.NewDrawing2((int)swDwgTemplates_e.swDwgTemplateA0size, "D:\\Vents-PDM\\Библиотека проектирования\\Templates\\flattpattern.drwdot",// "D:\\Vents-PDM\\Библиотека проектирования\\Templates\\flattpattern.drwdot",
                        (int)swDwgPaperSizes_e.swDwgPaperA0size, 0.841, 0.594);
                swDraw.CreateFlatPatternViewFromModelView3(swModel.GetPathName(), configName, 0.841 / 2, 0.594 / 2, 0, true, true);
                swModel.ForceRebuild3(false);

                var swCustProp = swModelDocExt.CustomPropertyManager[configName];
                string valOut;

                string codMaterial;
                swCustProp.Get4("Код материала", true, out valOut, out codMaterial);
                confiData.КодМатериала = codMaterial;

                string материал;
                swCustProp.Get4("Материал", true, out valOut, out материал);
                confiData.Материал = материал;

                string обозначение;
                swCustProp.Get4("Обозначение", true, out valOut, out обозначение);
                confiData.Обозначение = обозначение;

                var swCustPropForDescription = swModelDocExt.CustomPropertyManager[""];
                string наименование;
                swCustPropForDescription.Get4("Наименование", true, out valOut, out наименование);
                confiData.Наименование = наименование;


                if (savedxf)
                {
                    var newDxf = (IModelDoc2)swDraw;
                    SwApp.CloseDoc(newDxf.GetPathName());
                }
                else
                {
                    var newDxf = (IModelDoc2)swDraw;
                    SwApp.CloseDoc(newDxf.GetTitle());
                }

                //UpdateCustomPropertyListFromCutList
                const string длинаГраничнойРамкиName = "Длина граничной рамки";
                const string ширинаГраничнойРамкиName = "Ширина граничной рамки";
                const string толщинаЛистовогоМеталлаNAme = "Толщина листового металла";
                const string сгибыName = "Сгибы";
                const string площадьПокрытияName = "Площадь покрытия";

                Feature swFeat2 = swModel.FirstFeature();
                while (swFeat2 != null)
                {
                    if (swFeat2.GetTypeName2() == "SolidBodyFolder")
                    {
                        BodyFolder swBodyFolder = swFeat2.GetSpecificFeature2();
                        swFeat2.Select2(false, -1);
                        swBodyFolder.SetAutomaticCutList(true);
                        swBodyFolder.UpdateCutList();

                        Feature swSubFeat = swFeat2.GetFirstSubFeature();
                        while (swSubFeat != null)
                        {
                            if (swSubFeat.GetTypeName2() == "CutListFolder")
                            {
                                BodyFolder bodyFolder = swSubFeat.GetSpecificFeature2();
                                swSubFeat.Select2(false, -1);
                                bodyFolder.SetAutomaticCutList(true);
                                bodyFolder.UpdateCutList();
                                var swCustPrpMgr = swSubFeat.CustomPropertyManager;
                                swCustPrpMgr.Add("Площадь поверхности", "Текст", "\"SW-SurfaceArea@@@Элемент списка вырезов1@" + Path.GetFileName(swModel.GetPathName())+"\"");

                                string длинаГраничнойРамки;
                                swCustPrpMgr.Get4(длинаГраничнойРамкиName, true, out valOut, out длинаГраничнойРамки);
                                swCustProp.Set(длинаГраничнойРамкиName, длинаГраничнойРамки);
                                confiData.ДлинаГраничнойРамки = длинаГраничнойРамки;

                                string ширинаГраничнойРамки;
                                swCustPrpMgr.Get4(ширинаГраничнойРамкиName, true, out valOut, out ширинаГраничнойРамки);
                                swCustProp.Set(ширинаГраничнойРамкиName, ширинаГраничнойРамки);
                                confiData.ШиринаГраничнойРамки = ширинаГраничнойРамки;

                                string толщинаЛистовогоМеталла;
                                swCustPrpMgr.Get4(толщинаЛистовогоМеталлаNAme, true, out valOut, out толщинаЛистовогоМеталла);
                                swCustProp.Set(толщинаЛистовогоМеталлаNAme, толщинаЛистовогоМеталла);
                                confiData.ТолщинаЛистовогоМеталла = толщинаЛистовогоМеталла;

                                string сгибы;
                                swCustPrpMgr.Get4(сгибыName, true, out valOut, out сгибы);
                                swCustProp.Set(сгибыName, сгибы);
                                confiData.Сгибы = сгибы;

                                string площадьПоверхности;
                                swCustPrpMgr.Get4("Площадь поверхности", true, out valOut, out площадьПоверхности);
                                swCustProp.Set(площадьПокрытияName, площадьПоверхности);
                                confiData.ПлощадьПокрытия = площадьПоверхности;
                            }
                            swSubFeat = swSubFeat.GetNextFeature();
                        }
                    }
                    swFeat2 = swFeat2.GetNextFeature();
                }
                dataList.Add(confiData);
            }

            swModel.ShowConfiguration2(activeconfiguration.Name);
            //GetXml(swModel);
            ExportDataToXmlSql(swModel, dataList);
            SwApp.CloseDoc(swModel.GetTitle());
        }

        static bool IsSheetMetalPart(IPartDoc swPart)
        {
            var isSheetMetal = false;
            var vBodies = swPart.GetBodies2((int)swBodyType_e.swSolidBody, false);

            foreach (Body2 vBody in vBodies)
            {
                isSheetMetal = vBody.IsSheetMetal();
            }
            return isSheetMetal;
        }

        class DataToExport
        {
            public string Config;
            public string Материал;
            public string Обозначение;
            public string ПлощадьПокрытия;
            public string КодМатериала;
            public string ДлинаГраничнойРамки;
            public string ШиринаГраничнойРамки;
            public string Сгибы;
            public string ТолщинаЛистовогоМеталла;
            public string Наименование;
        }

        static void ExportDataToXmlSql(IModelDoc2 swModel, IEnumerable<DataToExport> dataToExports)
        {
            try
            {
                //var myXml = new System.Xml.XmlTextWriter(@"\\srvkb\SolidWorks Admin\XML\" + swModel.GetTitle() + ".xml", System.Text.Encoding.UTF8);
            //    const string xmlPath = @"\\srvkb\SolidWorks Admin\XML\";
                const string xmlPath = @"C:\Temp\";
                var myXml = new System.Xml.XmlTextWriter(xmlPath + swModel.GetTitle() + ".xml", System.Text.Encoding.UTF8);

                myXml.WriteStartDocument();
                myXml.Formatting = System.Xml.Formatting.Indented;
                myXml.Indentation = 2;

                // создаем элементы
                myXml.WriteStartElement("xml");
                myXml.WriteStartElement("transactions");
                myXml.WriteStartElement("transaction");

                myXml.WriteStartElement("document");

                foreach (var configData in dataToExports)
                {
                    #region XML

                    // Конфигурация
                    myXml.WriteStartElement("configuration");
                    myXml.WriteAttributeString("name", configData.Config);

                    // Материал
                    myXml.WriteStartElement("attribute");
                    myXml.WriteAttributeString("name", "Материал");
                    myXml.WriteAttributeString("value", configData.Материал);
                    myXml.WriteEndElement();

                    // Наименование  -- Из таблицы свойств
                    myXml.WriteStartElement("attribute");
                    myXml.WriteAttributeString("name", "Наименование");
                    myXml.WriteAttributeString("value", configData.Наименование);
                    myXml.WriteEndElement();

                    // Обозначение
                    myXml.WriteStartElement("attribute");
                    myXml.WriteAttributeString("name", "Обозначение");
                    myXml.WriteAttributeString("value", configData.Обозначение);
                    myXml.WriteEndElement();

                    // Площадь покрытия
                    myXml.WriteStartElement("attribute");
                    myXml.WriteAttributeString("name", "Площадь покрытия");
                    myXml.WriteAttributeString("value", configData.ПлощадьПокрытия);
                    myXml.WriteEndElement();

                    // ERP code
                    myXml.WriteStartElement("attribute");
                    myXml.WriteAttributeString("name", "Код_Материала");
                    myXml.WriteAttributeString("value", configData.КодМатериала);
                    myXml.WriteEndElement();

                    // Длина граничной рамки

                    myXml.WriteStartElement("attribute");
                    myXml.WriteAttributeString("name", "Длина граничной рамки");
                    myXml.WriteAttributeString("value", configData.ДлинаГраничнойРамки);
                    myXml.WriteEndElement();

                    // Ширина граничной рамки
                    myXml.WriteStartElement("attribute");
                    myXml.WriteAttributeString("name", "Ширина граничной рамки");
                    myXml.WriteAttributeString("value", configData.ШиринаГраничнойРамки);
                    myXml.WriteEndElement();

                    // Сгибы
                    myXml.WriteStartElement("attribute");
                    myXml.WriteAttributeString("name", "Сгибы");
                    myXml.WriteAttributeString("value", configData.Сгибы);
                    myXml.WriteEndElement();

                    // Толщина листового металла
                    myXml.WriteStartElement("attribute");
                    myXml.WriteAttributeString("name", "Толщина листового металла");
                    myXml.WriteAttributeString("value", configData.ТолщинаЛистовогоМеталла);
                    myXml.WriteEndElement();

                    myXml.WriteEndElement();  //configuration

                    #endregion

                    #region SQL

                    try
                    {
                        // var sqlConnection = new SqlConnection(Settings.Default.SQLBaseCon);
                        //"Data Source=srvkb;Initial Catalog=SWPlusDB;Persist Security Info=True;User ID=sa;Password=PDMadmin;MultipleActiveResultSets=True");
                        var sqlConnection = new SqlConnection("Data Source=srvkb;Initial Catalog=SWPlusDB;Persist Security Info=True;User ID=sa;Password=PDMadmin;MultipleActiveResultSets=True");
                        sqlConnection.Open();
                        var spcmd = new SqlCommand("UpDateCutList", sqlConnection) { CommandType = CommandType.StoredProcedure };
                        //spcmd.Parameters.Add("@MaterialsID", SqlDbType.Int).Value = КодМатериала;
                        var partNumber = configData.Обозначение;
                        var description = configData.Наименование;
                        var workpieceX = Convert.ToDouble(configData.ДлинаГраничнойРамки.Replace('.', ','));
                        var workpieceY = Convert.ToDouble(configData.ШиринаГраничнойРамки.Replace('.', ','));
                        var bend = Convert.ToInt32(configData.Сгибы);
                        var thickness = Convert.ToDouble(configData.ТолщинаЛистовогоМеталла.Replace('.', ','));
                        var configuration = configData.Config;

                        spcmd.Parameters.Add("@PartNumber", SqlDbType.NVarChar).Value = partNumber;
                        spcmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = description;
                        if (configData.ДлинаГраничнойРамки == "") { workpieceX = 0; }
                        spcmd.Parameters.Add("@WorkpieceX", SqlDbType.Float).Value = workpieceX;
                        if (configData.ШиринаГраничнойРамки == "") { workpieceY = 0; }
                        spcmd.Parameters.Add("@WorkpieceY", SqlDbType.Float).Value = workpieceY;
                        if (configData.Сгибы == "") { bend = 0; }
                        spcmd.Parameters.Add("@Bend", SqlDbType.Int).Value = bend;
                        if (configData.ТолщинаЛистовогоМеталла == "") { thickness = 0; }
                        spcmd.Parameters.Add("@Thickness", SqlDbType.Float).Value = thickness;
                        spcmd.Parameters.Add("@Configuration", SqlDbType.NVarChar).Value = configuration;
                        //spcmd.Parameters.Add("@version", SqlDbType.Int);
                        spcmd.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                    catch (Exception)
                    {
                        // Console.WriteLine(e.Message);
                    }

                    #endregion
                }

                //myXml.WriteEndElement();// ' элемент CONFIGURATION
                myXml.WriteEndElement();// ' элемент DOCUMENT
                myXml.WriteEndElement();// ' элемент TRANSACTION
                myXml.WriteEndElement();// ' элемент TRANSACTIONS
                myXml.WriteEndElement();// ' элемент XML
                // заносим данные в myMemoryStream
                myXml.Flush();

            }
            catch (Exception e)
            {
                //  Console.WriteLine(e.Message);
            }
        }

        #endregion

        void FilterTextBox_OnSelectionChanged(object sender, TextChangedEventArgs e)
        {
            var list =  BomTablePrt.ItemsSource as List<BomCells>;// = BomListPrtToList();
            if (list != null) _bomListSelectedPrts = list.Where(x => x.Обозначение.Contains(FilterTextBox.Text)).ToList();
            //if (list != null)
            //    BomTablePrt.ItemsSource = list.Where(x => x.Обозначение.Contains(FilterTextBox.Text)).ToList();

            BomTableSelectedPrtUpdate();
        }

        void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //var mscorlib = typeof(string).Assembly;
            //foreach (var type in mscorlib.GetTypes())
            //{
            //    MessageBox.Show(type.FullName);
            //}

            var bomClass = new BomPartListClass();
            BomTablePrt.ItemsSource = bomClass.BomList().OrderBy(x => x.Обозначение);
        }       
    }
}
