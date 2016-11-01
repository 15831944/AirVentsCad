using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.AirVentsClasses.UnitsBuilding;
using AirVentsCadWpf.Properties;
//using ExportPartData;
using HostingWindowsForms.EPDM;
using VentsCadLibrary;

namespace AirVentsCadWpf.DataControls.Specification
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SpecificationUc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationUc"/> class.
        /// </summary>
        public SpecificationUc()
        {
            InitializeComponent();
            ПереченьДеталей_Copy.Visibility = Visibility.Hidden;
            dataGridAfterGet.Visibility = Visibility.Hidden;

            checkBoxOnlyAsms.Visibility = Visibility.Hidden;
            //checkBox.Visibility = Visibility.Hidden;
            COnf.Visibility = Visibility.Hidden;
        }
        
        string ПутьКСборке { get; set; }

        List<string> AssemblyConfigs { get; set; }
        
        string CurrentModel { get; set; }     

        static string FileName(string filePath)
        {
            var fileName = new FileInfo(filePath).Name;
            return fileName;
        }
    
        void AutoCompleteTextBox1Reload()
        {
            List< SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы;
            List<SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы1;

            VaultSystem.SearchInVault.SearchDoc(
                AutoCompleteTextBox1.Text,
                SwEpdm.EpdmSearch.SwDocType.SwDocAssembly,
                out найденныеФайлы, 
                Settings.Default.PdmBaseName);
            
            VaultSystem.SearchInVault.SearchDoc(
                AutoCompleteTextBox1.Text,
                SwEpdm.EpdmSearch.SwDocType.SwDocPart,
                out найденныеФайлы1,
                Settings.Default.PdmBaseName);
            
            if (найденныеФайлы == null & найденныеФайлы1 == null) return;

            if (найденныеФайлы1 != null & найденныеФайлы != null) найденныеФайлы.AddRange(найденныеФайлы1);
            if (найденныеФайлы1 != null & найденныеФайлы == null) найденныеФайлы = найденныеФайлы1;

            var newNames = найденныеФайлы.Select(findedDocuments => findedDocuments.Path).ToList();
                FilePath = newNames.ConvertAll(FileName);
            
            AutoCompleteTextBox1.ItemsSource = FilePath;
            AutoCompleteTextBox1.FilterMode = AutoCompleteFilterMode.Contains;
        }

        List<string> FilePath { get; set; }

        void Button_Click_3(object sender, RoutedEventArgs e)
        {
            AutoCompleteTextBox1Reload();
        }
        
        void EnabledExport()
        {
            Найти.IsEnabled = AutoCompleteTextBox1.Text.Length != 0;
            GetList.Content = AutoCompleteTextBox1.Text.ToLower().EndsWith("prt") ? "Получить данные" : "Получить перечень деталей";
        }

        class PartsListXml2
        {            
            public bool Dxf { get; set; }
            public bool Xml { get; set; }
            public int CurrentVersion { get; set; }
            public int IdPmd { get; set; }
            public string Наименование { get; set; }
            public string Путь { get; set; }
            public string НаименованиеБезРасширения { get; set; }
            public string PartNumber { get; set; }
            public string Конфигурация { get; set; }
            public string ЗаготовкаШирина { get; set; }
            public string ЗаготовкаВысота { get; set; }
            public string Гибы { get; set; }
            public string Толщина { get; set; }     
            public string ImageSrc { get; set; }
            public string ПлощадьПокрытия { get; set; }
            public string Материал { get; set; }
            public string ПлощадьS { get; set; }            
            public string МассаS { get; set; }
        }       
      
        void ПолучитьПереченьДеталей()
        {
            try
            {
                if (AutoCompleteTextBox1.Text.ToLower().EndsWith("dprt"))
                {
                    var emdpService = new Epdm();
                    var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text);
                    var filePath = path[0].FilePath;
                    ПутьКСборке = filePath;
                    var lastVersion = SwEpdm.GetVersionOfFile(filePath, Settings.Default.PdmBaseName);
                    int pdmId;
                    int curVer;
                    string name;
                    List<string> configurations;
                    SwEpdm.GetIdPdm(filePath, out name, out pdmId, out curVer, out configurations, true); 

                    var partsListXml2S = new List<PartsListXml2>();

                    var listOfExc = new List<Exception>();

                    foreach (var configname in configurations)
                    {
                        Exception exc;
                        var existDxf = ExistLastDxf(pdmId, curVer, configname, out exc);
                        if (exc != null)
                        {
                            listOfExc.Add(exc);
                        }

                        string message;

                        partsListXml2S.Add(new PartsListXml2
                        {
                            Наименование = Path.GetFileNameWithoutExtension(filePath),
                            Путь = filePath,
                            Xml = ExportXmlSql.ExistXml(filePath, lastVersion, out message),
                            CurrentVersion = lastVersion,
                            Конфигурация = configname,
                            IdPmd = pdmId,
                            Dxf = existDxf,
                            ImageSrc = existDxf ?
                                        @"\DataControls\Pictures\cancel.jpg" : @"\DataControls\Pictures\empty.jpg",
                            НаименованиеБезРасширения = Path.GetFileNameWithoutExtension(filePath)
                        });
                    }                  

                    PartsListXml2sDataGrid.ItemsSource = null;
                    PartsListXml2sDataGrid.ItemsSource = partsListXml2S;

                    ПереченьДеталей_Copy.Visibility = PartsListXml2sDataGrid.ItemsSource == null
                        ? Visibility.Hidden
                        : Visibility.Visible;

                    PartsListXml2sDataGrid_Copy.Visibility = Visibility.Collapsed;
                    XmlParts1_Copy.Visibility = Visibility.Collapsed;
                    
                }

                else
                {
                    if (CurrentModel != AutoCompleteTextBox1.Text)
                    {
                        var emdpService = new Epdm();
                        var path = emdpService.SearchDoc(AutoCompleteTextBox1.Text);                         
                        ПутьКСборке = path[0].FilePath;
                        var configs = emdpService.GetConfiguration(path[0].FilePath);
                        AssemblyConfigs = configs.ToList();
                        ConfigsCombo.ItemsSource = AssemblyConfigs;
                        COnf.Visibility = Visibility.Visible;
                        ConfigsCombo.Text = AssemblyConfigs[0];
                    }

                    var bomClass = new Epdm
                    {
                        BomId = 8,
                        AssemblyPath = ПутьКСборке
                    };

                    Exception exception;
                    var спецификация = bomClass.BomList(ПутьКСборке, ConfigsCombo.Text, false, out exception);
                    if (exception != null)
                    {
                        MessageBox.Show(exception.StackTrace);
                    }                    
                    
                    var partsListXml2S = new List<PartsListXml2>();
                    foreach (var item in спецификация)
                    {
                        if (!item.ТипФайла.ToLower().Contains("dprt")) continue;
                        if (item.Раздел == "Детали" || item.Раздел == "")
                        {
                            partsListXml2S.Add(new PartsListXml2
                            {
                                CurrentVersion = Convert.ToInt32(item.ПоследняяВерсия),
                                IdPmd = (int)item.IdPdm,
                                Наименование = item.FileName,
                                Путь = item.FilePath + @"\" + item.FileName,
                                Конфигурация = item.Конфигурация,
                                Материал = item.Материал
                            });
                        }
                    }

                    #region to delete
                    //var message = "";
                    //foreach (var item in excList)
                    //{
                    //    message = message + "\n_________\n" + item.Message + "\n" + item.StackTrace;
                    //}
                    //if (!string.IsNullOrEmpty(message))
                    //{
                    //    MessageBox.Show(message);
                    //} 
                    #endregion

                    foreach (var listXml in partsListXml2S)
                    {
                        string message;

                        listXml.Xml =
                            ExportXmlSql.ExistXml(listXml.Наименование, listXml.CurrentVersion, out message);
                        listXml.НаименованиеБезРасширения = listXml.Наименование.ToUpper().Replace(".SLDPRT", "");
                    }

                    var list = InnerPartsList();

                    var newList = from partsListXml2 in partsListXml2S
                        join listXml2 in list
                            on partsListXml2.НаименованиеБезРасширения.ToLower() equals
                            listXml2.PartNumber.ToLower()
                        select new
                        {
                            partsListXml2.НаименованиеБезРасширения,
                            listXml2.PartNumber,

                            listXml2.ЗаготовкаВысота,
                            listXml2.Толщина,
                            listXml2.ЗаготовкаШирина,
                            listXml2.Гибы,
                            listXml2.ПлощадьПокрытия,
                            listXml2.Конфигурация,
                            listXml2.ПлощадьS,
                            listXml2.МассаS,
                            partsListXml2.Материал
                        };


                    var newList2 = newList.Select(variable => new PartsListXml2
                    {
                        НаименованиеБезРасширения = variable.НаименованиеБезРасширения,
                        PartNumber = variable.PartNumber,
                        ЗаготовкаВысота = variable.ЗаготовкаВысота,
                        Толщина = variable.Толщина,
                        ЗаготовкаШирина = variable.ЗаготовкаШирина,
                        Гибы = variable.Гибы,
                        ПлощадьПокрытия = variable.ПлощадьПокрытия,
                        Конфигурация = variable.Конфигурация,
                        Материал = variable.Материал
                    }).ToList();


                    var newListOrder = new List<PartsListXml2>();
                    foreach (var partsListXml2 in partsListXml2S)
                    {
                        try
                        {
                            var selectedItem =
                                newList2.Where(
                                    x =>
                                        string.Equals(x.НаименованиеБезРасширения.ToLower(),
                                            partsListXml2.Наименование.Replace(".SLDPRT", "").ToLower(),
                                            StringComparison.CurrentCultureIgnoreCase) &&
                                        x.Конфигурация == partsListXml2.Конфигурация);

                            foreach (var listXml2 in selectedItem)
                            {
                                if (newListOrder.Any(x => string.Equals(x.НаименованиеБезРасширения,
                                    listXml2.НаименованиеБезРасширения,
                                    StringComparison.CurrentCultureIgnoreCase) &&
                                                          x.Конфигурация == listXml2.Конфигурация)) continue;

                                newListOrder.Add(new PartsListXml2
                                {
                                    НаименованиеБезРасширения = listXml2.НаименованиеБезРасширения,
                                    PartNumber = listXml2.PartNumber,

                                    ЗаготовкаВысота = listXml2.ЗаготовкаВысота,
                                    Толщина = listXml2.Толщина,
                                    ЗаготовкаШирина = listXml2.ЗаготовкаШирина,
                                    Гибы = listXml2.Гибы,
                                    ПлощадьПокрытия = listXml2.ПлощадьПокрытия,
                                    Конфигурация = listXml2.Конфигурация,

                                    Материал = listXml2.Материал,
                                    МассаS = GetMass(listXml2.ПлощадьПокрытия, listXml2.Толщина)
                                });
                            }
                        }
                        catch (Exception exceptio)
                        {
                            MessageBox.Show(exceptio.StackTrace);
                        }
                    }

                    var listXml2S = new List<PartsListXml2>();

                    partsListXml2S = partsListXml2S.OrderBy(x => x.Наименование).ThenBy(x=>x.Конфигурация).ToList();
                    listXml2S.Add(partsListXml2S[0]);

                    for (var i = 0; i < partsListXml2S.Count - 1; i++)
                    {
                        var currentItem = partsListXml2S[i];
                        var newItem = partsListXml2S[i + 1];
                        if (!(currentItem.Наименование == newItem.Наименование && currentItem.Конфигурация == newItem.Конфигурация))
                        {                            
                            listXml2S.Add(newItem);
                        }
                    }

                    foreach (var item in listXml2S)
                    {
                        Exception exc;
                        var existDxf = ExistLastDxf((int)item.IdPmd, (int)item.CurrentVersion, item.Конфигурация, out exc);
                        item.Dxf = existDxf;
                        item.ImageSrc = existDxf ?
                                        @"\DataControls\Pictures\cancel.jpg" : @"\DataControls\Pictures\empty.jpg";
                    }



                    PartsListXml2sDataGrid.ItemsSource = null;
                    PartsListXml2sDataGrid.ItemsSource = listXml2S;

                    ПереченьДеталей_Copy.Visibility = PartsListXml2sDataGrid.ItemsSource == null
                        ? Visibility.Hidden
                        : Visibility.Visible;

                    PartsListXml2sDataGrid_Copy.Visibility = Visibility.Visible;
                    XmlParts1_Copy.Visibility = Visibility.Visible;

                    PartsListXml2sDataGrid_Copy.ItemsSource =
                        newListOrder.OrderBy(x => x.НаименованиеБезРасширения).ThenBy(x => x.Конфигурация);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            finally
            {
                CurrentModel = AutoCompleteTextBox1.Text;
                var sdgv = (List<PartsListXml2>)PartsListXml2sDataGrid.ItemsSource;
                ПереченьДеталей_Copy.Header = CurrentModel.ToLower().EndsWith("prt") ? $"Деталь {CurrentModel}" : 
                    $"Перечень деталей для сборки {CurrentModel.ToUpper().Replace(".SLDASM","")}-{ConfigsCombo.Text} (Всего деталей: {PartsListXml2sDataGrid.Items.Count}шт. Разверток - {sdgv.Count(x=>x.Dxf)} 1С - {sdgv.Count(x => x.Xml)})";
            }
        }

        string Busy { get; set; }

        bool OnlyParts { get; set; }

        private List<PartsListXml2> ListToRun { get; set; }

        private Task Export { get; set; }

        void ExportTaskRun(Action action)
        {
            Export = new Task(action);
            Export.Start();
        }

        static void GetFiles(IEnumerable<PartsListXml2> list, out List<VaultSystem.PdmFilesAfterGet> pdmFilesAfterGet)
        {
            ModelSw.BatchGet(Settings.Default.PdmBaseName, list.Select(x => new VaultSystem.BatchParams
            {
                CurrentVersion = x.CurrentVersion,
                FilePath = x.Путь,
                IdPdm = x.IdPmd
            }).ToList(),
            out pdmFilesAfterGet);
        }

        List<VaultSystem.PdmFilesAfterGet> _pdmFilesAfterGet;

        void AutoCompleteTextBox1_TextChanged(object sender, RoutedEventArgs e)
        {
            EnabledExport();

            var items = AutoCompleteTextBox1.ItemsSource as List<string>;
            if (items == null) return;

            GetList.IsEnabled = items.Contains(AutoCompleteTextBox1.Text);
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            ПолучитьПереченьДеталей();

            if (PartsListXml2sDataGrid_Copy.Items.Count == 0)
            {
                PartsListXml2sDataGrid_Copy.Visibility = Visibility.Collapsed;
                XmlParts1_Copy.Visibility = Visibility.Collapsed;
            }
            else
            {
                PartsListXml2sDataGrid_Copy.Visibility = Visibility.Visible;
                XmlParts1_Copy.Visibility = Visibility.Visible;
            }
        }

        static IEnumerable<PartsListXml2> InnerPartsList()
        {
            var sqlBaseData = new SqlBaseData();
            var table = sqlBaseData.PartTechParams();
            var list = (from DataRow row in table.Rows
                        select new PartsListXml2
                        {
                            PartNumber = row["PartNumber"].ToString(),
                            Конфигурация = row["Конфигурация"].ToString(),
                            ЗаготовкаШирина = row["Заготовка Ширина"].ToString(),
                            ЗаготовкаВысота = row["Заготовка Высота"].ToString(),
                            Гибы = row["Гибы"].ToString(),
                            Толщина = row["Толщина"].ToString(),
                            ПлощадьПокрытия = row["Площадь покрытия"].ToString()
                        }).ToList();
            return list;
        }

        void CollectParams()
        {
            OnlyParts = checkBoxOnlyParts.IsChecked == true;
            OnlyInAsmConfigs = checkBox.IsChecked == true;
            IncludeNonSheetParts = checkBox2.IsChecked == true;
            ListToRun = PartsListXml2sDataGrid.ItemsSource.OfType<PartsListXml2>().ToList();
        }

        bool IsBusy
        {
            get
            {
                if (string.IsNullOrEmpty(Busy)) return false;
                MessageBox.Show(Busy, "Система занята", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
        }

        #region XML

        void XmlParts1_Click(object sender, RoutedEventArgs e)
        {
            CollectParams();
            ExportTaskRun(ExportToXml);
        }      

        void ExportToXml()
        {
            if (IsBusy) return;

            try
            {
                Busy = $"Выгрузка {CurrentModel}";                
                if (!OnlyParts)
                {
                    if (!ПутьКСборке.ToLower().EndsWith("dprt"))                    
                    {
                        List<Exception> excptions;
                        PdmAsmBomToXml.AsmBomToXml.ВыгрузкаСборкиВXml(ПутьКСборке, out excptions);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.StackTrace);
            }            

            GetFiles(ListToRun.Where(newComponent => !newComponent.Xml), out _pdmFilesAfterGet);

            #region To Delete

            // var list = _pdmFilesAfterGet.Where(x => !x.Equal).ToList();

            //var sf = "";

            //foreach (var item in list)
            //{
            //    sf  = sf + "\nName - " + item.FileName + " Path - " + item.FilePath + " VerBG - " + item.VersionBeforeGet + " VerAG - " + item.VersionBeforeGet;
            //}

            //MessageBox.Show(sf, list?.Count.ToString());

            #endregion

            try
            {
                foreach (var newComponent in ListToRun.Where(newComponent => !newComponent.Xml))
                {
                    using (var modelSw = new ModelSw())
                    {
                        try
                        {
                            Exception exception;
                            ExportXmlSql.Export(modelSw.GetSwWithPart(newComponent.Путь), newComponent.CurrentVersion, (int)newComponent.IdPmd, out exception);
                            
                            #region To Delete
                            //if (exception != null)
                            //{
                            //  //  MessageBox.Show(exception.Message);
                            //}
                            ////modelSw.ExitSw();
                            #endregion
                        }
                        catch (Exception ex )
                        {
                          //  MessageBox.Show(ex.Message);
                        }                        
                    }
                }
            }
            catch (Exception exception)
            {
               //MessageBox.Show(exception.StackTrace);
            }
            MessageBox.Show($"Выгрузка {Path.GetFileNameWithoutExtension(ПутьКСборке)} завершена");
            Busy = null;
        }

        #endregion

        #region DXF

        static bool ExistLastDxf(int idPdm, int currentVersion, string configuration, out Exception exc)
        {
            exc = null;
            bool exist;
            try
            {
                exist = Dxf.ExistDxf(idPdm, currentVersion, configuration, out exc);
            }
            catch (Exception)
            {
                exist = false;
            }
            return exist;
        }

        void DeleteDxf(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var item = PartsListXml2sDataGrid.SelectedItem as PartsListXml2;
            if (item == null) return;
            if (!item.Dxf) return;

            try
            {
                if (MessageBox.Show(
                    $"Удалить развертку для\n Детали - {item.НаименованиеБезРасширения}\n Конфигурация - {item.Конфигурация}\n Версия - {item.CurrentVersion}",
                    "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Exception exc;
                    Database.DeleteDxf(item.IdPmd, item.Конфигурация, item.CurrentVersion, out exc);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        void DxfParts1_Click(object sender, RoutedEventArgs e)
        {
            CollectParams();
            ExportTaskRun(ExportToDxf);
        }

        private string FolderToSaveDxf { get; set; }

        private string ParentFolderToSaveDxf { get; set; } = Settings.Default.DXFPath;// @"\\pdmsrv\DXF";  // @"\\srvkb\SolidWorks Admin\DXF\"; // @"C:\DXF\";    

        void ExportToDxf()
        {
            if (IsBusy) return;
            try
            {
                Busy = $"Выгрузка разверток для {CurrentModel}";

                FolderToSaveDxf = CurrentModel.ToLower().EndsWith("dprt") ? "детали" :
                CurrentModel.ToLower().Replace(".sldasm", "");
                var pathToSave = Path.Combine(ParentFolderToSaveDxf, FolderToSaveDxf);
                Directory.CreateDirectory(pathToSave.ToUpper());

                var listToExportLocal = new List<PartsListXml2>();
                var files = "";

                foreach (var item in ListToRun.Where(x => x.Dxf))
                {
                    try
                    {
                        Exception exc;
                        string matId;
                        double? thikness;
                        var blob = Database.DatabaseFileRead(item.IdPmd, item.CurrentVersion, item.Конфигурация, out matId, out thikness, out exc);
                        if (exc != null) { MessageBox.Show(exc.StackTrace); }
                        if (blob != null)
                        {
                            Dxf.SaveByteToFile(blob, Path.Combine(pathToSave, item.НаименованиеБезРасширения.ToUpper().Replace("ВНС-", "")
                                + "-" + item.Конфигурация + (string.IsNullOrEmpty(matId) ? "" : "-" + matId)
                                + ((thikness == null || thikness == 0) ? "" : "-" + string.Format("{0:0.0}", thikness)).Replace(",", ".") + ".dxf"));
                        }
                        else
                        {
                            files = files + "\n" + item.Наименование + " idPdm - " + item.IdPmd + " Ver - " + item.CurrentVersion + " Conf - " + item.Конфигурация;
                            listToExportLocal.Add(item);
                        }
                    }
                    catch (Exception)
                    {
                        listToExportLocal.Add(item);
                    }
                }

                listToExportLocal.AddRange(ListToRun.Where(x => !x.Dxf).ToList());

                GetFiles(listToExportLocal, out _pdmFilesAfterGet);

                var exeptions = new List<string>();

                foreach (var part in listToExportLocal)
                {
                    Exception exception;
                    List<Dxf.ResultList> resultList;
                    List<Dxf.DxfFile> dxfFiles;                  

                    Dxf.Save(Path.GetFullPath(part.Путь), pathToSave, OnlyInAsmConfigs ? part.Конфигурация : null, out exception, part.IdPmd, part.CurrentVersion, out dxfFiles, true, true, IncludeNonSheetParts);
                    if (exception != null)
                    {
                        try
                        {
                            MessageBox.Show(exception.Message + "\n" + exception.StackTrace, "Exception");
                        }
                        catch (Exception) { }
                    }
                    try
                    {
                        Dxf.AddToSql(dxfFiles, false, out resultList);
                        foreach (var item in resultList)
                        {
                            if (item != null)
                            {
                                exeptions.Add($"{item.dxfFile.IdPdm}");
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }                    
                }

                if (exeptions?.Count > 0)
                {
                    using (var sw = new StreamWriter(ParentFolderToSaveDxf + "\\Exceptions List.txt "))
                    //using (var sw = new StreamWriter(pathToSave + "\\Exceptions for " + FolderToSaveDxf.ToUpper() + ".txt"))
                    {
                        foreach (var item in exeptions)
                        {
                            sw.WriteLine(item + ",");
                        }
                    }
                }

                foreach (var process in Process.GetProcessesByName("SLDWORKS"))
                {
                    process.Kill();
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(exception.StackTrace);
            }

            MessageBox.Show($"Выгрузка разверток {Path.GetFileNameWithoutExtension(ПутьКСборке)} завершена");
            Busy = null;
        }

        bool OnlyInAsmConfigs { get; set; }

        bool IncludeNonSheetParts { get; set; }

        #endregion

        #region Инфо для бухгалтерии и технологов

        void XmlParts1_Copy_Click(object sender, RoutedEventArgs e)
        {
            PartsListXml2sDataGrid_Copy.SelectAllCells();
        }

        string GetMass(string ПлощадьПокрытия, string Толщина)
        {
            try
            {
                return ((Math.Round(Convert.ToDouble(ПлощадьПокрытия) * Convert.ToDouble(Толщина) * 780)) / 100).ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}
