using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AirVentsCadWpf.Properties;
using BomPartList;
using MakeDxfUpdatePartData;
using VentsMaterials;
using VentsPDM_dll;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls
{

    /// <summary>
    /// SpecificationUc и перечень деталей. Выгрузка и работа со сборкой и деталями.
    /// </summary>
    public partial class PartsListUc
    {

        #region Fields and Constructor
        
        List<BomPartListClass.BomCells> _bomPartsList;
        List<BomPartListClass.BomCells> _bomPartsListFiltered;
        List<BomPartListClass.BomCells> _bomPartsListOriginal;
        
        readonly List<BomPartListClass.BomCells> _bomListSelectedPrts = new List<BomPartListClass.BomCells>();

        BomPartListClass _bomClass;

        readonly SetMaterials _swMaterials = new SetMaterials();
        readonly ToSQL _connectToSql = new ToSQL();

        readonly ModelSw _modelSwClass = new ModelSw();

        string _assemblyPath;
        string _asmConfiguration;
        string[] _asmConfigurations;
        List<BomPartListClass.BomCells> _bomListAllAsms;


        /// <summary>
        /// Initializes a new instance of the <see cref="DataControls.Спецификация.PartsListUc"/> class.
        /// </summary>
        public PartsListUc()
        {
            InitializeComponent();

            ChangedDataGrid(false);

            ToSQL.Conn = Settings.Default.ConnectionToSQL;
            MaterialsList.ItemsSource = _connectToSql.GetSheetMetalMaterialsName();
            MaterialsList.DisplayMemberPath = "MatName";
            MaterialsList.SelectedValuePath = "MatName";
            MaterialsList.SelectedIndex = 0;
            
            OpenAsm.Visibility = Visibility.Collapsed;
            Конфигурации.Visibility = Visibility.Collapsed;
            //ВыгрузитьСпецификацию.Visibility = Visibility.Collapsed;
            //ВыгрузитьСписокДеталей.Visibility = Visibility.Collapsed; 

            PartPropsConfig.Visibility = Visibility.Collapsed;
            PartPropsCopy.Visibility = Visibility.Collapsed;

            // Buttons
            BeginUpdatePartPropConfig.Visibility = Visibility.Collapsed;
            BeginUpdatePartProp.Visibility = Visibility.Collapsed;
            BeginUpdatePartPropThikness.Visibility = Visibility.Collapsed;

            AsmComponents.Visibility = Visibility.Collapsed;

        }

        #endregion

        #region Main Methods

        bool LoginAndFileDialog()
        {
            //ToSQL.Conn = Settings.Default.ConnectionToSQL;

            //IEdmVault10 mVault = new EdmVault5Class();
            //try
            //{
            //    if (mVault.IsLoggedIn) goto m1;
            //    try
            //    {
            //        mVault.LoginAuto(Settings.Default.PdmBaseName, 0);
            //    }
            //    catch (Exception)
            //    {
            //        mVault.Login(Settings.Default.UserName, Settings.Default.Password, Settings.Default.PdmBaseName);
            //        return false;
            //    }
            //    mVault.CreateSearch();
            //}
            //catch (COMException ex)
            //{
            //    MessageBox.Show(ex.Message);
            //    return false;
            //}
            //m1:
            //var fileDialog = new Microsoft.Win32.OpenFileDialog
            //{
            //    InitialDirectory = mVault.RootFolderPath,
            //    Filter = "SolidWorks Assemblies|*.sldasm"
            //};
            //if (fileDialog.ShowDialog() != true) return false;

            //_assemblyPath = fileDialog.FileName;
            //_asmConfiguration = "";

            return true;
        }

        bool GetAsmByName()
        {
           // ToSQL.Conn = Settings.Default.ConnectionToSQL;
            
            _assemblyPath = "";

            //IEdmVault10 mVault = new EdmVault5Class();
            //try
            //{
            //    if (mVault.IsLoggedIn) goto m1;
            //    try
            //    {
            //        mVault.LoginAuto(Settings.Default.PdmBaseName, 0);
            //    }
            //    catch (Exception)
            //    {
            //        mVault.Login(Settings.Default.UserName, Settings.Default.Password, Settings.Default.PdmBaseName);
            //        return false;
            //    }
            //    mVault.CreateSearch();
            //}
            //catch (COMException ex)
            //{
            //   MessageBox.Show(ex.Message);
            //    return false;
            //}
            m1:
            try
            {
                var pdmDll = new PDM {vaultname = Settings.Default.PdmBaseName};
                _assemblyPath = pdmDll.SearchDoc(ИмяСборки.Text);
                MessageBox.Show(_assemblyPath);

                return new FileInfo(_assemblyPath).Exists;
            }
            catch (Exception e)
            {
               // LoggerError(string.Format("При попытке выгрузки в XML установки c именем {0} возникла ошибка {1}", ИмяСборки.Text, e.StackTrace));
                MessageBox.Show("В базе отсутствует сборка с указаным именем");
                return false;
            }
        }

        private void GetAssemblyConfigurations()
        {
            try
            {
                //var vault1 = new EdmVault5();
                //vault1.LoginAuto(Settings.Default.PdmBaseName, 0);
                //IEdmFolder5 oFolder;
                //var edmFile5 = vault1.GetFileFromPath(new FileInfo(_assemblyPath).FullName, out oFolder);
                //var configs = edmFile5.GetConfigurations("");

                //var headPosition = configs.GetHeadPosition();

                //var configsList = new List<string>();

                //while (!headPosition.IsNull)
                //{
                //    var configName = configs.GetNext(headPosition);
                //    if (configName != "@")
                //    {
                //        configsList.Add(configName);
                //    }
                //}
                //Конфигурации.ItemsSource = configsList;
                //_asmConfigurations = configsList.ToArray();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        
        //readonly ServiceReference1.BomTableServiceClient _serviceClient = new ServiceReference1.BomTableServiceClient();
        //IEnumerable<BomPartListClass.BomCells> BomList
        //{
        //    get
        //    {
        //        return _serviceClient.BomParts(assemblyPath, Settings.Default.PdmBaseName, Settings.Default.userName, Settings.Default.Password, Settings.Default.ConnectionToSQL);
        //    }
        //}

        void GetBomList()
        {

           var bomId = 7;
           if (Settings.Default.PdmBaseName == "Vents-PDM")
           {
               bomId = 8;
           }

           _bomClass = new BomPartListClass
           {
               ConnectionToSql = Settings.Default.ConnectionToSQL,
               BomId = bomId,
               PdmBaseName = Settings.Default.PdmBaseName,
               UserName = Settings.Default.UserName,
               UserPassword = Settings.Default.Password,
               AssemblyPath = _assemblyPath,
               AsmConfiguration = _asmConfiguration
           };

           _bomPartsListOriginal = _bomClass.BomListPrt(false);
           _bomPartsList = _bomClass.BomListPrt(false);
           BomTablePrt.ItemsSource = _bomPartsList;

        }

        void ChangedDataGrid(bool isChanged)
        {
            if (isChanged)
            {
                СосотяниеСборки.Content = "В детали сборки внесены изменения. Необходимо применить, для вступления в силу!";
                СосотяниеСборки.Foreground = Brushes.Tomato;
                BeginUpdate1.IsEnabled = true;
            }
            else
            {
                СосотяниеСборки.Content = " Детали сборки";
                СосотяниеСборки.Foreground = Brushes.Black;
                BeginUpdate1.IsEnabled = false;
            }
        }

        private static IEnumerable<BomPartListClass.BomCells> ChangeListMaterial(string partName, string partConfig, string partMaterial, IReadOnlyList<BomPartListClass.BomCells> listToChange)
        {
            foreach (var bomCells in listToChange.Where(bomCells => bomCells.ОбозначениеDocMgr == partName).Where(bomCells => bomCells.Конфигурация == partConfig))
            {
                bomCells.МатериалЦмиDocMgr = partMaterial;
                bomCells.Материал = partMaterial;
            }
            return listToChange;
        }
        
        #endregion

        #region User Interface

        private void MaterialsList_LayoutUpdated(object sender, EventArgs e)
        {
            MaterialsList.Background = MaterialsList.Text == "" ? Brushes.Pink : Brushes.Azure;
            Accept.Content = MaterialsList.Text != "" ? "Задать материал" : "Выбрать материал";
        }

        void FilterTextBox_OnSelectionChanged(object sender, TextChangedEventArgs e)
        {
            FilterBomTable();
        }

        void FilterBomTable()
        {
            _bomPartsListFiltered = _bomPartsList;
            if (_bomPartsListFiltered != null) _bomPartsListFiltered = _bomPartsListFiltered.Where(x => x.Обозначение.Contains(FilterTextBox.Text)).ToList();
            BomTablePrt.ItemsSource = _bomPartsListFiltered;
        }

        
        readonly SolidColorBrush _orangeColorBrush = new SolidColorBrush(Colors.LavenderBlush);
        readonly SolidColorBrush _whiteColorBrush = new SolidColorBrush(Colors.White);
        readonly SolidColorBrush _lightCyanColorBrush = new SolidColorBrush(Colors.LightCyan);

        void BomTable_LoadingRow(object sender, DataGridRowEventArgs e)
        {
           var bomCells = (BomPartListClass.BomCells)e.Row.DataContext;

             if (bomCells.Errors == "")
             {
                 e.Row.Background = e.Row.GetIndex() % 2 == 1 ? _lightCyanColorBrush : _whiteColorBrush;
             }
             else
             {
                 e.Row.Background = _orangeColorBrush;
             }
        }
        
        void AssemblyInfo_LayoutUpdated(object sender, EventArgs e)
        {
          //  AsmComponents.Visibility = (string)AssemblyInfo.Content == "" ? Visibility.Hidden : Visibility.Visible;
           // PartProps.Visibility = (string)AssemblyInfo.Content == "" ? Visibility.Hidden : Visibility.Visible;
        }
        
        void BomTableSelectedPrt_LayoutUpdated(object sender, EventArgs e)
        {
            BomTableSelectedPrt.Visibility = BomTableSelectedPrt.HasItems ? Visibility.Visible : Visibility.Collapsed;

            ClearList.IsEnabled = BomTableSelectedPrt.HasItems;

            УдалитьВыбранную.IsEnabled = BomTableSelectedPrt.SelectedIndex != -1;

            BeginUpdateButton();

        }

        void BeginUpdateButton()
        {
            if (!BomTableSelectedPrt.HasItems)
            {
                BeginUpdate.IsEnabled = false;
                return;
            }
            if (UpdateCutList.IsChecked == true || MakeDxf.IsChecked == true)
            {
                BeginUpdate.IsEnabled = true;
            }
            else
            {
                BeginUpdate.IsEnabled = false;
            } 
        }

        void BomTableSelectedPrt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            УдалитьВыбранную.IsEnabled = BomTableSelectedPrt.SelectedIndex != -1;
        }

        void BomTablePrt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((BomPartListClass.BomCells)(BomTablePrt.SelectedValue));
            MaterialsList.SelectedValue = row.Материал;

            SelectedPart.ItemsSource = _bomPartsList.Where(x => x.Обозначение == row.ОбозначениеDocMgr);
        }

        void BomTablePrt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((BomPartListClass.BomCells)(BomTablePrt.SelectedValue));
            if (_bomListSelectedPrts.Any(bomListSelectedPrt => row.Обозначение == bomListSelectedPrt.Обозначение)) return;
            _bomListSelectedPrts.Add(row);
            BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);
        }

        // Buttons

        void OpenAsm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!LoginAndFileDialog()) return;
                GetBomList();
                AssemblyInfo.Content = _bomClass.AssemblyInfoLabel;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        void ClearList_Click(object sender, RoutedEventArgs e)
        {
            _bomListSelectedPrts.Clear();
            BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);
        }

        void УдалитьВыбранную_Click(object sender, RoutedEventArgs e)
        {
            _bomListSelectedPrts.Remove(BomTableSelectedPrt.SelectedItem as BomPartListClass.BomCells);
            BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);
        }

        void BeginUpdate_Click(object sender, RoutedEventArgs e)
        {
            var partDataClass = new MakeDxfExportPartDataClass { PdmBaseName = Settings.Default.PdmBaseName };

            //partDataClass.CreateFlattPatternUpdateCutlist(@"C:\Tets_debag\Vents-PDM\ВНС-901.Проба.sldprt", true, true);

        //    partDataClass.CreateFlattPatternUpdateCutlistAndEdrawing(@"E:\Vents-PDM\Проекты\AirVents\AV03\ВНС-901.78.001.sldprt");

            foreach (var selectedPrt in _bomListSelectedPrts)
            {
                try
                {
                    //if (UpdateCutList.IsChecked == true)
                    //{
                    //    partDataClass.CreateFlattPatternUpdateCutlist(selectedPrt.Путь, false, false);
                    //}
                    ////if (MakeDxf.IsChecked == true)
                    //{
                    //    partDataClass.CreateFlattPatternUpdateCutlist(selectedPrt.Путь, true, true);
                    //}
                    if (MakeDxf.IsChecked == true)
                    {
                     //   partDataClass.CreateFlattPatternUpdateCutlistAndEdrawing(@"C:\Tets_debag\Vents-PDM\ВНС-901.Проба.sldprt");
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }
            }

            _bomListSelectedPrts.Clear();
            BomTableSelectedPrt.ItemsSource = _bomListSelectedPrts.OrderBy(x => x.Обозначение);

            MakeDxf.IsChecked = false;
            UpdateCutList.IsChecked = false;

            Application.Current.MainWindow.Activate();
            MessageBox.Show("Детали обработаны!");
        }

        void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsList.SelectedValue == null) return;
            var material = ((ToSQL.ColumnMatProps)(MaterialsList.SelectedItem));
            //var material = ((ToSQL.ColumnMatProps)(MaterialsList.SelectedValue));
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((BomPartListClass.BomCells)(BomTablePrt.SelectedValue));

            BomTablePrt.ItemsSource = _bomPartsList = ChangeListMaterial(row.ОбозначениеDocMgr, row.Конфигурация, material.MatName, _bomPartsList).ToList();
            ChangedDataGrid(true);
            SelectedPart.ItemsSource = ChangeListMaterial(row.ОбозначениеDocMgr, row.Конфигурация, material.MatName, _bomPartsList.Where(x => x.Обозначение == row.ОбозначениеDocMgr).ToList()).ToList();

            FilterBomTable();
        }

        #endregion

        #region ToDelete

        void BeginUpdatePartProp_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsList.SelectedValue == null) return;
            var material = ((ToSQL.ColumnMatProps)(MaterialsList.SelectedValue));
            if (BomTablePrt.SelectedValue == null) return;
            var row = ((BomPartListClass.BomCells)(BomTablePrt.SelectedValue));
            SetMaterialsProperty(row.Путь, row.Конфигурация, Convert.ToInt32(material.LevelID));
            GetBomList();
            ChangedDataGrid(false);
        }
        

        void SetMaterialsProperty(string partPath, string partConfig, int materialId)
        {
            var pdmBase = Settings.Default.PdmBaseName;
            MessageBox.Show(pdmBase);
            // Test
            //partPath = @Шляхи.Text;
            //pdmBase = "Tets_debag";

            try
            {
                if (!_modelSwClass.IsSheetMetalPart(partPath, pdmBase))
                {
                    //MessageBox.Show("Не листовая деталь");
                    return;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка во время определения листовой детали" + e.Message, partPath);
            }

            try
            {
                _modelSwClass.CheckInOutPdm(partPath, false, pdmBase);
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка во время разрегистрации " + e.Message, partPath);
            }

            try
            {
                _swMaterials.ApplyMaterial(partPath, partConfig, materialId, null);
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка во время применения материала " + e.Message, partPath);
            }

            finally
            {
                try
                {
                    _modelSwClass.CheckInOutPdm(partPath, true, pdmBase);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка во время регистрации " + e.Message, partPath);
                }
            }
        }
        #endregion

        void BeginUpdate1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _bomClass.AcceptAllChanges(_bomPartsListOriginal, _bomPartsList);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            
            GetBomList();
            ChangedDataGrid(false);
        }

        void UpdateCutList_Click(object sender, RoutedEventArgs e)
        {
            BeginUpdateButton();
        }

        void MakeDxf_Click(object sender, RoutedEventArgs e)
        {
            BeginUpdateButton();
        }
        
        void ВыгрузитьСписокДеталей_Click(object sender, RoutedEventArgs e)
        {
            if (!GetAsmByName())
            {
                //LoggerError(string.Format("В базе отсутствует установка c именем {0}", ИмяСборки.Text));
                return;
            }
                GetAssemblyConfigurations();
        }

        void ВыгрузитьСпецификацию_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _asmConfiguration = Конфигурации.Text;
                GetBomList();
                AssemblyInfo.Content = _bomClass.AssemblyInfoLabel;
                AsmComponents.Visibility = Visibility.Visible;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        void ИмяСборки_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ВыгрузитьВXml_Click(this, new RoutedEventArgs());
            }
        }

        void Конфигурации_LayoutUpdated(object sender, EventArgs e)
        {
 //           ВыгрузитьСпецификацию.IsEnabled = !Конфигурации.Items.IsEmpty;
           // ВыгрузитьВXml.IsEnabled = !Конфигурации.Items.IsEmpty;
           // AsmComponents.Visibility = Конфигурации.Items.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        void ВыгрузитьВXml_Click(object sender, RoutedEventArgs e)
        {
            var isAllParts = ".";

            if (IncludeAsms.IsChecked == true)
            {
                isAllParts = "(включая все подсборки).";
            }
            
            ВыгрузитьСписокДеталей_Click(sender, e);

           // LoggerInfo(string.Format("Начало выгрузки в XML установки {0}{1}", ИмяСборки.Text, isAllParts));
            
            if (new FileInfo(_assemblyPath).Exists == false) return;

            try
            {
                var bomId = 7;
                if (Settings.Default.PdmBaseName == "Vents-PDM")
                {
                    bomId = 8;
                }
                _bomClass = new BomPartListClass
                {
                    ConnectionToSql = Settings.Default.ConnectionToSQL,
                    BomId = bomId,
                    PdmBaseName = Settings.Default.PdmBaseName,
                    UserName = Settings.Default.UserName,
                    UserPassword = Settings.Default.Password,
                    AssemblyPath = _assemblyPath
                };

                //_bomListAllAsms = _bomClass.BomListDocMgr();
                _bomListAllAsms = _bomClass.BomListDocMgrOnlyAllAsms();

                if (IncludeAsms.IsChecked == true)
                {
                    foreach (var asm in _bomListAllAsms.Where(x => x.ТипФайла == "sldasm").OrderBy(x => x.ОбозначениеDocMgr))
                    {
                            bomId = 7;
                            if (Settings.Default.PdmBaseName == "Vents-PDM")
                            {
                                bomId = 8;
                            }
                            _bomClass = new BomPartListClass
                            {
                                ConnectionToSql = Settings.Default.ConnectionToSQL,
                                BomId = bomId,
                                PdmBaseName = Settings.Default.PdmBaseName,
                                UserName = Settings.Default.UserName,
                                UserPassword = Settings.Default.Password,
                                AssemblyPath = asm.Путь
                            };

                            ExportDataToXml1C(Path.GetFileNameWithoutExtension(asm.Путь), GetAssemblyConfigurationsPdm(asm.Путь));
                        }
                }
                else
                {
                    ExportDataToXml1C(Path.GetFileNameWithoutExtension(_assemblyPath), _asmConfigurations);  
                }
                
                //LoggerInfo(string.Format("Выгрузка в XML установки {0}{1} завершена", ИмяСборки.Text, isAllParts));
                MessageBox.Show("SpecificationUc для сборки "+ Path.GetFileNameWithoutExtension(_assemblyPath) + " успешно выгружена");
            }

            catch (Exception exception)
            {
                //LoggerError(string.Format("Ошибка что выгрузка в XML установки {0}  {1}", ИмяСборки.Text, e.StackTrace));
                MessageBox.Show(exception.Message);
            }
        }

        //myList1 = myList1.Concat(myList2).ToList(); -- суммирование списков

        void ExportDataToXml1C(string asmName, IEnumerable<string> configsList)
        {
            //LoggerInfo(string.Format("Формирование XML для установки {0}", asmName));

            try
            {
                //const string xmlPath = @"\\srvkb\SolidWorks Admin\XML\";
                
                const string xmlPath = @"C:\";
                //try
                //{
                //    Directory.CreateDirectory(xmlPath);
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show(e.Message);
                //}

                var myXml = new System.Xml.XmlTextWriter(xmlPath + asmName + ".xml", System.Text.Encoding.UTF8);

                myXml.WriteStartDocument();
                myXml.Formatting = System.Xml.Formatting.Indented;
                myXml.Indentation = 2;

                // создаем элементы
                myXml.WriteStartElement("xml");
                myXml.WriteStartElement("transactions");
                myXml.WriteStartElement("transaction");

                myXml.WriteAttributeString("vaultName", "Vents-PDM");
                myXml.WriteAttributeString("type", "wf_export_document_attributes");
                myXml.WriteAttributeString("date", "1416475021");

                // document
                myXml.WriteStartElement("document");
                myXml.WriteAttributeString("pdmweid", "73592");
                myXml.WriteAttributeString("aliasset", "Export To ERP");

                foreach (var config in configsList)

                {
                    var bomTable = _bomClass.BomListDocMgrTopLevel(config);

                    foreach (var topAsm in bomTable.Where(x => x.Уровень == "0"))
                    {

                        #region XML

                        // Конфигурация
                        myXml.WriteStartElement("configuration");
                        myXml.WriteAttributeString("name", topAsm.Конфигурация);

                        // Версия
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Версия");
                        myXml.WriteAttributeString("value", topAsm.ПоследняяВерсия);
                        myXml.WriteEndElement();

                        // Масса
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Масса");
                        myXml.WriteAttributeString("value", topAsm.МассаDocMgr);
                        myXml.WriteEndElement();

                        // Наименование
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Наименование");
                        myXml.WriteAttributeString("value", topAsm.НаименованиеDocMgr);
                        myXml.WriteEndElement();

                        // Обозначение
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Обозначение");
                        myXml.WriteAttributeString("value", topAsm.ОбозначениеDocMgr);
                        myXml.WriteEndElement();

                        // Раздел
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Раздел");
                        myXml.WriteAttributeString("value", topAsm.РазделDocMgr);
                        myXml.WriteEndElement();

                        // ERP code
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "ERP code");
                        myXml.WriteAttributeString("value", topAsm.КодERP);
                        myXml.WriteEndElement();

                        // Код_Материала
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Код_Материала");
                        myXml.WriteAttributeString("value", topAsm.КодМатериалаDocMgr);
                        myXml.WriteEndElement();

                        // Код документа
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Код документа");
                        myXml.WriteAttributeString("value", topAsm.КодДокументаDocMgr);
                        myXml.WriteEndElement();

                        // Кол. Материала
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Кол. Материала");
                        myXml.WriteAttributeString("value", topAsm.КоличествоМатериалаDocMgr);
                        myXml.WriteEndElement();

                        // Состояние
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Состояние");
                        myXml.WriteAttributeString("value", topAsm.Состояние);
                        myXml.WriteEndElement();

                        // Подсчет ссылок
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Подсчет ссылок");
                        myXml.WriteAttributeString("value", topAsm.Количество);
                        myXml.WriteEndElement();

                        // Конфигурация
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Конфигурация");
                        myXml.WriteAttributeString("value", topAsm.Конфигурация);
                        myXml.WriteEndElement();

                        // Идентификатор
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Идентификатор");
                        myXml.WriteAttributeString("value", "");
                        myXml.WriteEndElement();

                        // references
                        myXml.WriteStartElement("references");

                        // document
                        myXml.WriteStartElement("document");
                        myXml.WriteAttributeString("pdmweid", "73592");
                        myXml.WriteAttributeString("aliasset", "Export To ERP");

                        foreach (var topLevel in bomTable.Where(x => x.Уровень == "1"))
                        {
                            #region XML

                            // Конфигурация
                            myXml.WriteStartElement("configuration");
                            myXml.WriteAttributeString("name", topLevel.Конфигурация);

                            // Версия
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Версия");
                            myXml.WriteAttributeString("value", topLevel.ПоследняяВерсия);
                            myXml.WriteEndElement();

                            // Масса
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Масса");
                            myXml.WriteAttributeString("value", topLevel.МассаDocMgr);
                            myXml.WriteEndElement();

                            // Наименование
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Наименование");
                            myXml.WriteAttributeString("value", topLevel.НаименованиеDocMgr);
                            myXml.WriteEndElement();

                            // Обозначение
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Обозначение");
                            myXml.WriteAttributeString("value", topLevel.ОбозначениеDocMgr);
                            myXml.WriteEndElement();

                            // Раздел
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Раздел");
                            myXml.WriteAttributeString("value", topLevel.РазделDocMgr);
                            myXml.WriteEndElement();

                            // ERP code
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "ERP code");
                            myXml.WriteAttributeString("value", topLevel.КодERP);
                            myXml.WriteEndElement();

                            // Код_Материала
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Код_Материала");
                            myXml.WriteAttributeString("value", topLevel.КодМатериалаDocMgr);
                            myXml.WriteEndElement();

                            // Код документа
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Код документа");
                            myXml.WriteAttributeString("value", topLevel.КодДокументаDocMgr);
                            myXml.WriteEndElement();

                            // Кол. Материала
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Кол. Материала");
                            myXml.WriteAttributeString("value", topLevel.КоличествоМатериалаDocMgr);
                            myXml.WriteEndElement();

                            // Состояние
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Состояние");
                            myXml.WriteAttributeString("value", topLevel.Состояние);
                            myXml.WriteEndElement();

                            // Подсчет ссылок
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Подсчет ссылок");
                            myXml.WriteAttributeString("value", topLevel.Количество);
                            myXml.WriteEndElement();

                            // Конфигурация
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Конфигурация");
                            myXml.WriteAttributeString("value", topLevel.Конфигурация);
                            myXml.WriteEndElement();

                            // Идентификатор
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Идентификатор");
                            myXml.WriteAttributeString("value", "");
                            myXml.WriteEndElement();

                            myXml.WriteEndElement(); //configuration

                            #endregion
                        }

                        myXml.WriteEndElement(); // document

                        myXml.WriteEndElement(); // элемент references

                        myXml.WriteEndElement(); // configuration

                        #endregion

                    }
                }
                
                myXml.WriteEndElement();// ' элемент DOCUMENT
                myXml.WriteEndElement();// ' элемент TRANSACTION
                myXml.WriteEndElement();// ' элемент TRANSACTIONS
                myXml.WriteEndElement();// ' элемент XML
                // заносим данные в myMemoryStream
                myXml.Flush();

                myXml.Close();

            }
            catch (Exception e)
            {
             //  LoggerError(string.Format("При формировании XML для установки возникла ошибка {0}", e.StackTrace));
               MessageBox.Show(e.Message);
            }
            
           // LoggerInfo(string.Format("Завершено формирование XML для установки {0}", asmName));
        }

        void ExportToXml(string asmName)
        {
            ExportDataToXml1C(asmName, GetAssemblyConfigurationsPdm(GetAsmByName(asmName)));
        }
        void ExportDataToXml1C2(string asmName, IEnumerable<string> configsList)
        {

            var bomId = 7;
            if (Settings.Default.PdmBaseName == "Vents-PDM")
            {
                bomId = 8;
            }

            _bomClass = new BomPartListClass
            {
                ConnectionToSql = Settings.Default.ConnectionToSQL,
                BomId = bomId,
                PdmBaseName = Settings.Default.PdmBaseName,
                UserName = Settings.Default.UserName,
                UserPassword = Settings.Default.Password,
                AssemblyPath = _assemblyPath
            };
            
            //  var bomTable = _bomClass.BomListDocMgrTopLevel(Config);

            try
            {
                const string xmlPath = @"\\srvkb\SolidWorks Admin\XML\";

                var myXml = new System.Xml.XmlTextWriter(xmlPath + asmName + ".xml", System.Text.Encoding.UTF8);

                myXml.WriteStartDocument();
                myXml.Formatting = System.Xml.Formatting.Indented;
                myXml.Indentation = 2;

                // создаем элементы
                myXml.WriteStartElement("xml");
                myXml.WriteStartElement("transactions");
                myXml.WriteStartElement("transaction");

                myXml.WriteAttributeString("vaultName", "Vents-PDM");
                myXml.WriteAttributeString("type", "wf_export_document_attributes");
                myXml.WriteAttributeString("date", "1416475021");

                // document
                myXml.WriteStartElement("document");
                myXml.WriteAttributeString("pdmweid", "73592");
                myXml.WriteAttributeString("aliasset", "Export To ERP");

                foreach (var config in configsList)
                {
                    var bomTable = _bomClass.BomListDocMgrTopLevel(config);

                    foreach (var topAsm in bomTable.Where(x => x.Уровень == "0"))
                    {
                        #region XML

                        // Конфигурация
                        myXml.WriteStartElement("configuration");
                        myXml.WriteAttributeString("name", topAsm.Конфигурация);

                        // Версия
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Версия");
                        myXml.WriteAttributeString("value", topAsm.ПоследняяВерсия);
                        myXml.WriteEndElement();

                        // Масса
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Масса");
                        myXml.WriteAttributeString("value", topAsm.МассаDocMgr);
                        myXml.WriteEndElement();

                        // Наименование
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Наименование");
                        myXml.WriteAttributeString("value", topAsm.НаименованиеDocMgr);
                        myXml.WriteEndElement();

                        // Обозначение
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Обозначение");
                        myXml.WriteAttributeString("value", topAsm.ОбозначениеDocMgr);
                        myXml.WriteEndElement();

                        // Раздел
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Раздел");
                        myXml.WriteAttributeString("value", topAsm.РазделDocMgr);
                        myXml.WriteEndElement();

                        // ERP code
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "ERP code");
                        myXml.WriteAttributeString("value", topAsm.КодERPDocMgr);
                        myXml.WriteEndElement();

                        // Код_Материала
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Код_Материала");
                        myXml.WriteAttributeString("value", topAsm.КодМатериалаDocMgr);
                        myXml.WriteEndElement();

                        // Код документа
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Код документа");
                        myXml.WriteAttributeString("value", topAsm.КодДокументаDocMgr);
                        myXml.WriteEndElement();

                        // Кол. Материала
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Кол. Материала");
                        myXml.WriteAttributeString("value", topAsm.КоличествоМатериалаDocMgr);
                        myXml.WriteEndElement();

                        // Состояние
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Состояние");
                        myXml.WriteAttributeString("value", topAsm.Состояние);
                        myXml.WriteEndElement();

                        // Подсчет ссылок
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Подсчет ссылок");
                        myXml.WriteAttributeString("value", topAsm.Количество);
                        myXml.WriteEndElement();

                        // Конфигурация
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Конфигурация");
                        myXml.WriteAttributeString("value", topAsm.Конфигурация);
                        myXml.WriteEndElement();

                        // Идентификатор
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Идентификатор");
                        myXml.WriteAttributeString("value", "");
                        myXml.WriteEndElement();

                        // references
                        myXml.WriteStartElement("references");

                        // document
                        myXml.WriteStartElement("document");
                        myXml.WriteAttributeString("pdmweid", "73592");
                        myXml.WriteAttributeString("aliasset", "Export To ERP");

                        foreach (var topLevel in bomTable.Where(x => x.Уровень == "1"))
                        {
                            #region XML

                            // Конфигурация
                            myXml.WriteStartElement("configuration");
                            myXml.WriteAttributeString("name", topLevel.Конфигурация);

                            // Версия
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Версия");
                            myXml.WriteAttributeString("value", topLevel.ПоследняяВерсия);
                            myXml.WriteEndElement();

                            // Масса
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Масса");
                            myXml.WriteAttributeString("value", topLevel.МассаDocMgr);
                            myXml.WriteEndElement();

                            // Наименование
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Наименование");
                            myXml.WriteAttributeString("value", topLevel.НаименованиеDocMgr);
                            myXml.WriteEndElement();

                            // Обозначение
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Обозначение");
                            myXml.WriteAttributeString("value", topLevel.ОбозначениеDocMgr);
                            myXml.WriteEndElement();

                            // Раздел
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Раздел");
                            myXml.WriteAttributeString("value", topLevel.РазделDocMgr);
                            myXml.WriteEndElement();

                            // ERP code
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "ERP code");
                            myXml.WriteAttributeString("value", topLevel.КодERPDocMgr);
                            myXml.WriteEndElement();

                            // Код_Материала
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Код_Материала");
                            myXml.WriteAttributeString("value", topLevel.КодМатериалаDocMgr);
                            myXml.WriteEndElement();

                            // Код документа
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Код документа");
                            myXml.WriteAttributeString("value", topLevel.КодДокументаDocMgr);
                            myXml.WriteEndElement();

                            // Кол. Материала
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Кол. Материала");
                            myXml.WriteAttributeString("value", topLevel.КоличествоМатериалаDocMgr);
                            myXml.WriteEndElement();

                            // Состояние
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Состояние");
                            myXml.WriteAttributeString("value", topLevel.Состояние);
                            myXml.WriteEndElement();

                            // Подсчет ссылок
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Подсчет ссылок");
                            myXml.WriteAttributeString("value", topLevel.Количество);
                            myXml.WriteEndElement();

                            // Конфигурация
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Конфигурация");
                            myXml.WriteAttributeString("value", topLevel.Конфигурация);
                            myXml.WriteEndElement();

                            // Идентификатор
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Идентификатор");
                            myXml.WriteAttributeString("value", "");
                            myXml.WriteEndElement();

                            myXml.WriteEndElement(); //configuration

                            #endregion
                        }

                        myXml.WriteEndElement(); // document

                        myXml.WriteEndElement(); // элемент references

                        myXml.WriteEndElement(); // configuration

                        #endregion
                    }
                }

                myXml.WriteEndElement();// ' элемент DOCUMENT
                myXml.WriteEndElement();// ' элемент TRANSACTION
                myXml.WriteEndElement();// ' элемент TRANSACTIONS
                myXml.WriteEndElement();// ' элемент XML
                // заносим данные в myMemoryStream
                myXml.Flush();

                myXml.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        
        string GetAsmByName(string asmName)
        {
            //IEdmVault10 mVault = new EdmVault5Class();
            //try
            //{
            //    if (mVault.IsLoggedIn) goto m1;
            //    try
            //    {
            //        mVault.LoginAuto(Settings.Default.PdmBaseName, 0);
            //    }
            //    catch (Exception)
            //    {
            //        mVault.Login(Settings.Default.UserName, Settings.Default.Password, Settings.Default.PdmBaseName);
            //        return "";
            //    }
            //    mVault.CreateSearch();
            //}
            //catch (COMException ex)
            //{
            //    MessageBox.Show(ex.Message);
            //    return "";
            //}
            //m1:
            try
            {
                var pdmDll = new PDM { vaultname = Settings.Default.PdmBaseName };
                _assemblyPath = pdmDll.SearchDoc(ИмяСборки.Text);
                return new FileInfo(_assemblyPath).Exists ? _assemblyPath : "";
                
            }
            catch (Exception)
            {
                MessageBox.Show("В базе отсутствует сборка с указаным именем");
                return "";
            }
        }
        
        IEnumerable<string> GetAssemblyConfigurationsPdm(string assemblyPath)
        {
            try
            {
                //var vault1 = new EdmVault5();
                //vault1.LoginAuto(Settings.Default.PdmBaseName, 0);
                //IEdmFolder5 oFolder;
                //var edmFile5 = vault1.GetFileFromPath(new FileInfo(assemblyPath).FullName, out oFolder);
                //var configs = edmFile5.GetConfigurations("");

                //var headPosition = configs.GetHeadPosition();

                //var configsList = new List<string>();

                //while (!headPosition.IsNull)
                //{
                //    var configName = configs.GetNext(headPosition);
                //    if (configName != "@")
                //    {
                //        configsList.Add(configName);
                //    }
                //}
                //return configsList;
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BeginUpdate_Click(sender, e);
        }

    }
}
