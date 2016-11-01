using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using AirVentsCadWpf.Properties;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;

namespace AirVentsCadWpf.DataControls.ForConstructors
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ForConstructorsUc
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ForConstructorsUc"/> class.
        /// </summary>
        public ForConstructorsUc()
        {
            InitializeComponent();
            Temporary.Visibility = Visibility.Collapsed;
        }

        private void BuildDamper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sw = new ModelSw();
                sw.Dumper(TypeOfDumper.Text, WidthDamper.Text, HeightDamper.Text, false, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Grid_Loaded_2(object sender, RoutedEventArgs e)
        {
           
        }
        
        private void WidthDamper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildDamper_Click(this, new RoutedEventArgs());
            }
        }

        private void HeightDamper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildDamper_Click(this, new RoutedEventArgs());
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ДобавитьТаблицуВидов_Click(object sender, RoutedEventArgs e)
        {
           // MessageBox.Show("Откройте чертеж и выделете основной вид модели!\n Затем нажмите 'OK'");
            
            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                var swModel = (IModelDoc2)swApp.IActiveDoc;

                var swDraw = (DrawingDoc) swModel;
                
                //Select View
                swModel.ClearSelection2(true);
                var getFirstView = swDraw.GetCurrentSheet().GetViews()[0];

                //swDraw.GetFirstView();
                //var getFirstView = swDraw.IActiveDrawingView;

                var referencedDocument = (IModelDoc2)getFirstView.ReferencedDocument;


                #region From swApp

                var docName = referencedDocument.GetTitle();
                var configNames = referencedDocument.GetConfigurationNames();
                var обозначения = new List<string>();
                //foreach (var configName in configNames)
                //{
                //    if (configName == "00")
                //    {
                //        обозначения.Add(docName);
                //    }
                //    else
                //    {
                //        обозначения.Add("-" + configName);
                //    }
                //}

                #endregion

                #region VentsMaterials dll

                var vm = new VentsMaterials.SetMaterials();
                var tosql = new VentsMaterials.ToSQL();
                VentsMaterials.ToSQL.Conn = Settings.Default.ConnectionToSQL;

                swApp.ActivateDoc(referencedDocument.GetTitle());
                var configNamesDll = vm.GetConfigurationNames();
                swApp.CloseDoc(Path.GetFileName(referencedDocument.GetPathName()));


                foreach (var configName in configNamesDll)
                {
                    if (configName == "00")
                    {
                        обозначения.Add(docName);
                    }
                    else
                    {
                        обозначения.Add("-" + configName);
                    }
                }

                #endregion
                
                InsertTableImg(swModel, обозначения);

              //  MessageBox.Show("Готово.");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void GetDataByBaseView(IModelDoc2 swModel)
        {
            var swDraw = (DrawingDoc)swModel;

            swDraw.ResolveOutOfDateLightWeightComponents();
            swDraw.ForceRebuild();

            // Движение по листам
            var vSheetName = (string[])swDraw.GetSheetNames();

            foreach (var name in vSheetName)
            {
                swDraw.ResolveOutOfDateLightWeightComponents();
                var swSheet = swDraw.Sheet[name];
                swDraw.ActivateSheet(swSheet.GetName());

                if ((swSheet.IsLoaded()))
                {
                    try
                    {
                        var sheetviews = (object[])swSheet.GetViews();
                        var firstView = (View)sheetviews[0];
                        firstView.SetLightweightToResolved();

                        var baseView = firstView.IGetBaseView();
                        var dispData = (IModelDoc2)baseView.ReferencedDocument;

                        MessageBox.Show(dispData.GetTitle());
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void GetData(DrawingDoc swDraw)
        {
           // var swDraw = (DrawingDoc) swModel;

          //  var ModelDoc

            var getFirstView = (View)swDraw.GetFirstView();
            MessageBox.Show(getFirstView.ReferencedDocument.GetTitle());


            swDraw.ResolveOutOfDateLightWeightComponents();
            swDraw.ForceRebuild();

            // Движение по листам
            var vSheetName = (string[]) swDraw.GetSheetNames();

            foreach (var name in vSheetName)
            {
                swDraw.ResolveOutOfDateLightWeightComponents();
                var swSheet = swDraw.Sheet[name];
                swDraw.ActivateSheet(swSheet.GetName());

                if ((!swSheet.IsLoaded())) continue;

                var sheetviews1 = (object[])swSheet.GetViews();
                var firstView1 = (View)sheetviews1[0];
                var baseView1 = firstView1.IGetBaseView();
                var dispData1 = (IModelDoc2)baseView1.ReferencedDocument;
                MessageBox.Show(dispData1.GetTitle());

                try
                {
                    var sheetviews = (object[]) swSheet.GetViews();
                    var firstView = (View) sheetviews[0];
                    firstView.SetLightweightToResolved();

                    var baseView = firstView.IGetBaseView();
                    var dispData = (IModelDoc2) baseView.ReferencedDocument;

                    MessageBox.Show(dispData.GetTitle());
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        private static
            void InsertTableImg(IModelDoc2 swModel, IList<string> обозначения)
        {
            var swDrawing = ((DrawingDoc)(swModel));
            var enumerable = обозначения as string[] ?? обозначения.ToArray();
            var myTable = swDrawing.InsertTableAnnotation(0.07, 0.07, 1, enumerable.Count() + 1, 2);
            if ((myTable == null)) return;
            myTable.Title = "Таблица видов";
            myTable.BorderLineWeight = 2;
            myTable.GridLineWeight = 1;
            myTable.Text[0, 0] = "Обозначение";

            myTable.SetColumnWidth(0, 0.05, 0);
            myTable.Text[0, 1] = "Рис.";
            myTable.SetColumnWidth(1, 0.02, 0);

            for (var i = 1; i < обозначения.Count() + 1; i++)
            {
                myTable.Text[i, 0] = обозначения[i - 1];
                if (i > 1)
                {
                    myTable.set_CellTextHorizontalJustification(i, 0, (int)swTextJustification_e.swTextJustificationRight);
                }
                myTable.Text[i, 1] = (i).ToString();
            }
        }

        private static
            void InsertTableSizes(IModelDoc2 swModel, IList<string> обозначения)
        {
            var swDrawing = ((DrawingDoc)(swModel));
            var enumerable = обозначения as string[] ?? обозначения.ToArray();
            var myTable = swDrawing.InsertTableAnnotation(0.07, 0.07, 1, enumerable.Count() + 1, 2);
            if ((myTable == null)) return;
            myTable.Title = "Таблица видов";
            myTable.BorderLineWeight = 2;
            myTable.GridLineWeight = 1;
            myTable.Text[0, 0] = "Обозначение";

            myTable.SetColumnWidth(0, 0.05, 0);
            myTable.Text[0, 1] = "Рис.";
            myTable.SetColumnWidth(1, 0.02, 0);

            for (var i = 1; i < обозначения.Count() + 1; i++)
            {
                myTable.Text[i, 0] = обозначения[i - 1];
                if (i > 1)
                {
                    myTable.set_CellTextHorizontalJustification(i, 0, (int)swTextJustification_e.swTextJustificationRight);
                }
                myTable.Text[i, 1] = (i).ToString();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                var swModel = (IModelDoc2)swApp.IActiveDoc;

                var info = $" swModel.GetPathName - {swModel.GetPathName()} swModel.GetTitle - {swModel.GetTitle()} ";
                MessageBox.Show(info);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                swApp.ExitApp();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                var swModel = (IModelDoc2)swApp.IActiveDoc;

               swApp.CloseDoc(Path.GetFileName(swModel.GetPathName()));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                var processes = Process.GetProcessesByName("SLDWORKS");
                foreach (var process in processes)
                {
                    process.CloseMainWindow();
                    process.Kill();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        

        private void ДобавитьТаблицуРазмеров_Click(object sender, RoutedEventArgs e)
        {
            var classf = new SizesClass();

            ТаблицаВидов.ItemsSource = classf.SheetSizes("DRW1");

            return;


            var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            var swModel = (IModelDoc2)swApp.IActiveDoc;

            var swDraw = (DrawingDoc)swModel;

            //Select View
            swModel.ClearSelection2(true);

            var getFirstView = (View)swDraw.GetCurrentSheet().GetViews()[0];

            var views = swDraw.GetCurrentSheet().GetViews();

            // Получить виды

            var getFirstView2 = (View)swDraw.get_Sheet("DRW1").GetViews()[Convert.ToInt32(getFirstView.get_IPosition())];

            MessageBox.Show(getFirstView2.Name);




            foreach (View view in views)
            {
                MessageBox.Show(view.Name + " - " + view.GetDimensionCount());

                var dimensionsForView = (string[]) view.GetDimensionIds4();
                foreach (var d in dimensionsForView)
                {
                    MessageBox.Show(d);
                }

                //var dimensionsForView = (string[])view.GetDimensionDisplayString4();
                //foreach (var d in dimensionsForView)
                //{
                //    MessageBox.Show(d);
                //}

                //var dimensionsForView = (double[])view.GetDimensionDisplayInfo5();
                //foreach (var d in dimensionsForView)
                //{
                //    MessageBox.Show(d.ToString());
                //}
                
            }
            
            // ТаблицаВидов.ItemsSource = (View[])swDraw.GetCurrentSheet().GetViews();

            // Выделить размер
            swModel.Extension.SelectByID2("RD2@Чертежный вид11", "DIMENSION", 0, 0, 0, false, 0, null, 0);
            
            // Проставить букву
            swModel.EditDimensionProperties2(0, 0, 0, "", "", true, 9, 2, true, 12, 12, "M", "", false, "", "", false);
            
            // Проставить размер
            swModel.EditDimensionProperties2(0, 0, 0, "", "", false, 0, 2, true, 12, 12, "", "", true, "", "", false);
        }


        /// <summary>
        /// 
        /// </summary>
        public class SizesClass
        {
             
            SldWorks _swApp;// = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            IModelDoc2 _swModel;
            DrawingDoc _swDraw;

            /// <summary>
            /// Initializes a new instance of the <see cref="SizesClass"/> class.
            /// </summary>
            public SizesClass()
            {
                _swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                _swModel = (IModelDoc2)_swApp.IActiveDoc;
                _swDraw = (DrawingDoc)_swModel;   
            }

            /// <summary>
            /// Initializes this instance.
            /// </summary>
            public void Init()
            {
                _swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                _swModel = (IModelDoc2)_swApp.IActiveDoc;
                _swDraw = (DrawingDoc)_swModel;
            }

            /// <summary>
            /// Gets or sets the dim identifier.
            /// </summary>
            /// <value>
            /// The dim identifier.
            /// </value>
            public string DimId { get; set; }
            /// <summary>
            /// Gets or sets the dim string.
            /// </summary>
            /// <value>
            /// The dim string.
            /// </value>
            public string DimString { get; set; }
            /// <summary>
            /// Gets or sets the name of the view.
            /// </summary>
            /// <value>
            /// The name of the view.
            /// </value>
            public string ViewName { get; set; }
            //public string SheetName { get; set; }

            /// <summary>
            /// Размеры листа
            /// </summary>
            /// <param name="sheetName">Name of the sheet.</param>
            /// <returns></returns>
            public List<SizesClass> SheetSizes(string sheetName)
            {
                var list = new List<SizesClass>();
                var размеры = new SizesClass();
                Init();
                foreach (View view in _swDraw.get_Sheet(sheetName).GetViews())
                {
                   // MessageBox.Show(view.Name + " - " + view.GetDimensionCount());

                    string[] dimensionIds4 = view.GetDimensionIds4();

                    //swDocExt.SelectByID2("D1@Расстояние1@02-11-40-1.SLDASM", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                    //var myDimension = ((Dimension)(swDoc.Parameter("D1@Расстояние1")));
                    //myDimension.SystemValue = 0; // p1Deep = 19.2;


                    foreach (string dimensoinId in dimensionIds4)
                    {
                       // MessageBox.Show(d);
                        размеры.DimId = dimensoinId;
                        размеры.ViewName = view.Name;
                        list.Add(размеры);
                    }

                    //var dimensionsForView = (string[])view.GetDimensionDisplayString4();
                    //foreach (var d in dimensionsForView)
                    //{
                    //    MessageBox.Show(d);
                    //}

                    //var dimensionsForView = (double[])view.GetDimensionDisplayInfo5();
                    //foreach (var d in dimensionsForView)
                    //{
                    //    MessageBox.Show(d.ToString());
                    //}
                }
                return list;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs args)
        {
            try
            {
                var sw = new ModelSw();sw.Lono();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
                

        class CopyPropsToParentPart
        {
            SldWorks swApp;
            IModelDoc2 parentDoc;
            ModelDoc2 childDoc;

            public CopyPropsToParentPart()
            {
                swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            }

            public void CopyProperties()
            {
                try
                {
                    parentDoc = (IModelDoc2)swApp.IActiveDoc;
                    var parentItem = ExternalFileReferences.Get(swApp);

                    childDoc = swApp.OpenDoc6(parentItem.ModelPathName, (int)swDocumentTypes_e.swDocPART,
                               (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                    swApp.ActivateDoc(parentDoc.GetTitle());  
                    var props = PartProperties.GetAll(swApp, childDoc, ExternalFileReferences.Get(swApp).ConfigName);
                    //foreach (var item in props)
                    //{
                    //    MessageBox.Show(item.Key + " - " + item.Value, "Свойство заготовки");
                    //}                    
                    PartProperties.AddAll(swApp, (ModelDoc2)parentDoc, props);
                }
                catch (Exception)
                {
                    throw;
                }                
            }

            void ActivateChildItem()
            {
                //var swSelMgr = (SelectionMgr)parentDoc.SelectionManager;
                //var swModDocExt = parentDoc.Extension;
                //var nSelType = swSelMgr.GetSelectedObjectType3(1, -1);
                return;


                swApp.ActivateDoc(parentDoc.GetTitle());
                var swFeat = (IFeature)parentDoc.FirstFeature();
                while ((swFeat != null))
                {
                    var FeatName = swFeat.Name;
                    if (swFeat.GetTypeName() == "Stock")
                    {                       
                      var result = parentDoc.Extension.SelectByID2(FeatName, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                        if (result)
                        {
                            break;
                        }
                    }
                    swFeat = (Feature)swFeat.GetNextFeature();
                }
            }

            class PartProperties
            {
                public static PartProperties Get(SldWorks swApp, ModelDoc2 childDoc, string configuration)
                {
                    swApp.ActivateDoc(childDoc.GetTitle());
                    var swCustProp = childDoc.Extension.CustomPropertyManager[configuration];

                    var configProps = new PartProperties
                    {
                        Config = configuration,
                        Наименование = GetProperty(swCustProp, "Наименование"),
                        Обозначение = GetProperty(swCustProp, "Обозначение"),
                        Материал = GetProperty(swCustProp, "Материал"),
                    };

                    return configProps;
                }

                public static List<KeyValuePair<string, string>> GetAll(SldWorks swApp, ModelDoc2 childDoc, string configuration)
                {
                    swApp.ActivateDoc(childDoc.GetTitle());
                    var swCustProp = childDoc.Extension.CustomPropertyManager[configuration];
                    var list = new List<KeyValuePair<string, string>>();
                    foreach (var item in Properties)
                    {
                        list.Add(new KeyValuePair<string, string>(item.Key, GetProperty(swCustProp, item.Key)));                        
                    }
                    swApp.CloseDoc(Path.GetFileName(childDoc.GetPathName()));
                    return list;
                }


                public static void Add(SldWorks swApp, ModelDoc2 parentDoc)
                {
                    swApp.ActivateDoc(parentDoc.GetTitle());
                    var configuration = ((Configuration)parentDoc.GetActiveConfiguration()).Name;
                    var swCustProp = parentDoc.Extension.CustomPropertyManager[configuration];
                    AddProperty(swCustProp, new KeyValuePair<string, string> ("Наименование", "Новое наименование"));
                }

                public static void AddAll(SldWorks swApp, ModelDoc2 parentDoc, List<KeyValuePair<string, string>> properties)
                {
                    swApp.ActivateDoc(parentDoc.GetTitle());
                    var configuration = ((Configuration)parentDoc.GetActiveConfiguration()).Name;
                    MessageBox.Show("configuration - " + configuration);
                    var swCustProp = parentDoc.Extension.CustomPropertyManager[configuration];
                    foreach (var property in properties)
                    {
                        MessageBox.Show(property.Key + " - " + property.Value, "Свойство для добавления");                        
                        AddProperty(swCustProp, property);
                    }                                        
                }

                static string GetProperty(CustomPropertyManager propManger, string propName)
                {
                    string valOut;
                    string value;
                    propManger.Get4(propName, true, out valOut, out value);

                    if (string.IsNullOrEmpty(value))
                    {
                        return null;
                    }
                    else
                    {
                        return value;
                    }
                }

                static void AddProperty(CustomPropertyManager propManger, KeyValuePair<string, string> propName)
                {                    
                        propManger.Add3(propName.Key == "ERP code" ? "Код материала" : "Заготовка_" + propName.Key,
                            (int)swCustomInfoType_e.swCustomInfoText, propName.Value, (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd);                    
                }

                static List<KeyValuePair<string, string>> Properties = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Наименование", ""),
                    new KeyValuePair<string, string>("Обозначение", ""),
                    new KeyValuePair<string, string>("Раздел", ""),
                    new KeyValuePair<string, string>("ERP code", ""),                    
                    //new KeyValuePair<string, string>("Исполнение", ""),
                    //new KeyValuePair<string, string>("Код_ФБ", ""),                    
                    //new KeyValuePair<string, string>("ФорматФБ", ""),
                    //new KeyValuePair<string, string>("Единицы", ""),
                    //new KeyValuePair<string, string>("Литера_ФБ", "")
                };

                #region To Delete

                public string Наименование;
                public string Обозначение;
                public string Материал;
                public string Config;
                
                //public KeyValuePair<string, string> Обозначение = new KeyValuePair<string, string>("Обозначение", "");
                //public KeyValuePair<string, string> Исполнение = new KeyValuePair<string, string>("Исполнение", "");
                //public KeyValuePair<string, string> Код_ФБ = new KeyValuePair<string, string>("Код_ФБ", "");
                //public KeyValuePair<string, string> Раздел = new KeyValuePair<string, string>("Раздел", "");
                //public KeyValuePair<string, string> ФорматФБ = new KeyValuePair<string, string>("ФорматФБ", "");
                //public KeyValuePair<string, string> Единицы = new KeyValuePair<string, string>("Единицы", "");
                //public KeyValuePair<string, string> Литера_ФБ = new KeyValuePair<string, string>("Литера_ФБ", "");

                //public KeyValuePair<string, string> Наименование;


                //public string Config;
                //public string Материал;

                //public double ПлощадьПокрытия;
                //public string КодМатериала;

                //public string ДлинаГраничнойРамки;
                //public string ШиринаГраничнойРамки;
                //public string Сгибы;
                //public string ТолщинаЛистовогоМеталла;                

                //public int? MaterialId;

                //public string FileName;

                //public double? PaintX;
                //public double? PaintY;
                //public double? PaintZ;

                //public int IdPdm;
                //public int Version;

                #endregion
            }

            class ExternalFileReferences
            {
                public string ModelPathName { get; set; }

                public string ComponentPathName { get; set; }

                public string Feature { get; set; }

                public string DataType { get; set; }

                public string Status { get; set; }

                public string ReferenceEntity { get; set; }

                public string FeatureComponent { get; set; }

                public string ConfigOption { get; set; }

                public string ConfigName { get; set; }

                public static ExternalFileReferences Get(SldWorks swApp)
                {
                    try
                    {
                        var obj = new ExternalFileReferences();
                        ModelDoc2 swModel = default(ModelDoc2);
                        ModelDocExtension swModDocExt = default(ModelDocExtension);
                        SelectionMgr swSelMgr = default(SelectionMgr);
                        Feature swFeat = default(Feature);
                        Component2 swComp = default(Component2);
                        object vModelPathName = null;
                        object vComponentPathName = null;
                        object vFeature = null;
                        object vDataType = null;
                        object vStatus = null;
                        object vRefEntity = null;
                        object vFeatComp = null;
                        int nConfigOpt = 0;
                        string sConfigName = null;
                        int nRefCount = 0;
                        int nSelType = 0;
                        int i = 0;

                        swModel = (ModelDoc2)swApp.ActiveDoc;
                        swSelMgr = (SelectionMgr)swModel.SelectionManager;

                        swModDocExt = (ModelDocExtension)swModel.Extension;
                        nSelType = swSelMgr.GetSelectedObjectType3(1, -1);

                        switch (nSelType)
                        {

                            // Selected component in an assembly document
                            case (int)swSelectType_e.swSelCOMPONENTS:
                                swComp = (Component2)swSelMgr.GetSelectedObjectsComponent3(1, -1);
                                nRefCount = swComp.ListExternalFileReferencesCount();
                                swComp.ListExternalFileReferences2(out vModelPathName, out vComponentPathName, out vFeature, out vDataType, out vStatus, out vRefEntity, out vFeatComp, out nConfigOpt, out sConfigName);

                                swModel = (ModelDoc2)swComp.GetModelDoc2();

                                break;
                            // Selected feature in a part or assembly document
                            case (int)swSelectType_e.swSelBODYFEATURES:
                            case (int)swSelectType_e.swSelSKETCHES:
                                swFeat = (Feature)swSelMgr.GetSelectedObject6(1, -1);
                                nRefCount = swFeat.ListExternalFileReferencesCount();
                                swFeat.ListExternalFileReferences2(out vModelPathName, out vComponentPathName, out vFeature, out vDataType, out vStatus, out vRefEntity, out vFeatComp, out nConfigOpt, out sConfigName);

                                break;

                            // Part document only
                            default:
                                nRefCount = swModDocExt.ListExternalFileReferencesCount();
                                swModDocExt.ListExternalFileReferences(out vModelPathName, out vComponentPathName, out vFeature, out vDataType, out vStatus, out vRefEntity, out vFeatComp, out nConfigOpt, out sConfigName);

                                break;
                        }

                        //Debug.Print("Model name = " + swModel.GetPathName());                

                        if (nRefCount >= 1)
                        {
                            object[] ModelPathName = new object[nRefCount - 1];
                            object[] ComponentPathName = new object[nRefCount - 1];
                            object[] Feature = new object[nRefCount - 1];
                            object[] DataType = new object[nRefCount - 1];
                            int[] Status = new int[nRefCount - 1];
                            object[] RefEntity = new object[nRefCount - 1];
                            object[] FeatComp = new object[nRefCount - 1];

                            ModelPathName = (object[])vModelPathName;
                            ComponentPathName = (object[])vComponentPathName;
                            Feature = (object[])vFeature;
                            DataType = (object[])vDataType;
                            Status = (int[])vStatus;
                            RefEntity = (object[])vRefEntity;
                            FeatComp = (object[])vFeatComp;

                            for (i = 0; i <= nRefCount - 1; i++)
                            {
                                obj.ModelPathName = ModelPathName[i].ToString();
                                obj.ComponentPathName = ComponentPathName[i].ToString();
                                obj.Feature = Feature[i].ToString();
                                obj.DataType = DataType[i].ToString();
                                obj.Status = Status[i].ToString();
                                obj.ReferenceEntity = RefEntity[i].ToString();
                                obj.FeatureComponent = FeatComp[i].ToString();
                                obj.ConfigOption = nConfigOpt.ToString();
                                obj.ConfigName = sConfigName?.ToString();
                            }
                        }
                        return obj;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.StackTrace);
                        return null;
                    }
                }
            }

        }

        class PropsToParentPart
        {
            SldWorks swApp;
            IModelDoc2 parentDoc;
            ModelDoc2 childDoc;

            public PropsToParentPart()
            {
                swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            }

            public void CopyProperties()
            {
                try
                {
                    parentDoc = (IModelDoc2)swApp.IActiveDoc;
                    var parentItem = ExternalFileReferences.Get(swApp);
                    childDoc = swApp.OpenDoc6(parentItem.ModelPathName, (int)swDocumentTypes_e.swDocPART,
                               (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                    swApp.ActivateDoc(parentDoc.GetTitle());
                    parentDoc.SelectByName(0, parentItem.ModelPathName);
                    parentItem = ExternalFileReferences.Get(swApp);                    
                    PartProperties.AddAll(swApp, (ModelDoc2)parentDoc, PartProperties.GetAll(swApp, childDoc, ExternalFileReferences.Get(swApp).ConfigName));
                }
                catch (Exception ex )
                {
                    MessageBox.Show(ex.Message);
                }
            }
            class PartProperties
            {
               
                public static List<KeyValuePair<string, string>> GetAll(SldWorks swApp, ModelDoc2 childDoc, string configuration)
                {
                    swApp.ActivateDoc(childDoc.GetTitle());
                    var swCustProp = childDoc.Extension.CustomPropertyManager[configuration];
                    var list = new List<KeyValuePair<string, string>>();
                    foreach (var item in Properties)
                    {
                        list.Add(new KeyValuePair<string, string>(item.Key, GetProperty(swCustProp, item.Key)));
                    }
                    swApp.CloseDoc(Path.GetFileName(childDoc.GetPathName()));
                    return list;
                }               

                public static void AddAll(SldWorks swApp, ModelDoc2 parentDoc, List<KeyValuePair<string, string>> properties)
                {
                    swApp.ActivateDoc(parentDoc.GetTitle());
                    var configuration = ((Configuration)parentDoc.GetActiveConfiguration()).Name;                    
                    var swCustProp = parentDoc.Extension.CustomPropertyManager[configuration];
                    foreach (var property in properties)
                    {                        
                        AddProperty(swCustProp, property);
                    }
                }
                static string GetProperty(CustomPropertyManager propManger, string propName)
                {
                    string valOut;
                    string value;
                    propManger.Get4(propName, true, out valOut, out value);

                    if (string.IsNullOrEmpty(value))
                    {
                        return null;
                    }
                    else
                    {
                        return value;
                    }
                }

                static void AddProperty(CustomPropertyManager propManger, KeyValuePair<string, string> propName)
                {
                    propManger.Add3(propName.Key == "ERP code" ? "Код материала" : "Заготовка_" + propName.Key,
                        (int)swCustomInfoType_e.swCustomInfoText, propName.Value, (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd);
                }

                static List<KeyValuePair<string, string>> Properties = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Наименование", ""),
                    new KeyValuePair<string, string>("Обозначение", ""),
                    new KeyValuePair<string, string>("Раздел", ""),
                    new KeyValuePair<string, string>("ERP code", "") 
                };
            }

            class ExternalFileReferences
            {
                public string ModelPathName { get; set; }

                public string ComponentPathName { get; set; }

                public string Feature { get; set; }

                public string DataType { get; set; }

                public string Status { get; set; }

                public string ReferenceEntity { get; set; }

                public string FeatureComponent { get; set; }

                public string ConfigOption { get; set; }

                public string ConfigName { get; set; }

                public static ExternalFileReferences Get(SldWorks swApp)
                {
                    try
                    {
                        var obj = new ExternalFileReferences();
                        ModelDoc2 swModel = default(ModelDoc2);
                        ModelDocExtension swModDocExt = default(ModelDocExtension);
                        SelectionMgr swSelMgr = default(SelectionMgr);
                        Feature swFeat = default(Feature);                        
                        object vModelPathName = null;
                        object vComponentPathName = null;
                        object vFeature = null;
                        object vDataType = null;
                        object vStatus = null;
                        object vRefEntity = null;
                        object vFeatComp = null;
                        int nConfigOpt = 0;
                        string sConfigName = null;
                        int nRefCount = 0;
                        int i = 0;

                        swModel = (ModelDoc2)swApp.ActiveDoc;
                        swSelMgr = (SelectionMgr)swModel.SelectionManager;

                        swModDocExt = (ModelDocExtension)swModel.Extension;
                        
                        swFeat = (Feature)swSelMgr.GetSelectedObject6(1, -1);
                        nRefCount = swFeat.ListExternalFileReferencesCount();
                        swFeat.ListExternalFileReferences2(out vModelPathName, out vComponentPathName, out vFeature, out vDataType, out vStatus, out vRefEntity, out vFeatComp, out nConfigOpt, out sConfigName);
                             

                        if (nRefCount >= 1)
                        {
                            object[] ModelPathName = new object[nRefCount - 1];
                            object[] ComponentPathName = new object[nRefCount - 1];
                            object[] Feature = new object[nRefCount - 1];
                            object[] DataType = new object[nRefCount - 1];
                            int[] Status = new int[nRefCount - 1];
                            object[] RefEntity = new object[nRefCount - 1];
                            object[] FeatComp = new object[nRefCount - 1];

                            ModelPathName = (object[])vModelPathName;
                            ComponentPathName = (object[])vComponentPathName;
                            Feature = (object[])vFeature;
                            DataType = (object[])vDataType;
                            Status = (int[])vStatus;
                            RefEntity = (object[])vRefEntity;
                            FeatComp = (object[])vFeatComp;

                            for (i = 0; i <= nRefCount - 1; i++)
                            {
                                obj.ModelPathName = ModelPathName[i].ToString();
                                obj.ComponentPathName = ComponentPathName[i].ToString();
                                obj.Feature = Feature[i].ToString();
                                obj.DataType = DataType[i].ToString();
                                obj.Status = Status[i].ToString();
                                obj.ReferenceEntity = RefEntity[i].ToString();
                                obj.FeatureComponent = FeatComp[i].ToString();
                                obj.ConfigOption = nConfigOpt.ToString();
                                obj.ConfigName = sConfigName?.ToString();
                            }
                        }
                        return obj;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return null;
                    }
                }
            }

        }


        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                new PropsToParentPart().CopyProperties();

                return;

                new CopyPropsToParentPart().CopyProperties();

                return;

                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");

                //var item = ExternalFileReferences.Get(swApp);
                //MessageBox.Show(item.ModelPathName + "\n" + item.ConfigName);

                var swModel = (IModelDoc2)swApp.IActiveDoc;
                //var swPart = (IPartDoc)swModel;
                var swFeat = (IFeature)swModel.FirstFeature();
                while ((swFeat != null))
                {
                    var FeatName = swFeat.Name;
                    try
                    {
                        if (swFeat.GetTypeName() == "Stock")
                        {
                            swFeat.Select2(false, -1);
                            MessageBox.Show("Before");
                            var copm = (IComponent2)swFeat;//.GetSpecificFeature2();

                            MessageBox.Show("After");
                            MessageBox.Show((copm == null).ToString());
                            MessageBox.Show("After 2");
                            //MessageBox.Show(copm.Visible.ToString());

                            return;

                            swModel.Extension.SelectByID2(FeatName, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swModel.FeatEdit();
                            var editStatus = swFeat.GetEditStatus();
                            MessageBox.Show($"EditStatus - {editStatus}");

                            var swEnt = (Entity)swFeat;
                            
                            MessageBox.Show($"{swEnt == null}\n{FeatName}\n\n{swEnt.GetType()}");

                            #region to delete

                            //{swEnt.Select(true)}

                            // var sdfvb = new AttributeDef(); sdfvb.AddParameter
                            // MessageBox.Show(swEnt.IFindAttribute(new AttributeDef()));

                            //MessageBox.Show(FeatName);
                            //int longstatus = 0;
                            //var model = swApp.ActivateDoc2(FeatName, false, ref longstatus);
                            //MessageBox.Show(model.GetType());
                            //MessageBox.Show(model.GetTitle() + "\n" + model.GetActiveConfiguration().Name);

                            //swModel.ClearSelection2(true);
                            //swModel.Extension.SelectByID2(FeatName, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            //swModel.ActivateSelectedFeature();

                            ////swModel.EntityProperties();

                            #endregion

                            swModel.Extension.SelectByID2(FeatName, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            var newDoc = swModel.Extension.Document;
                            MessageBox.Show(newDoc.GetTitle());

                            #region to delete

                            //var swCustProp = swModel.Extension.CustomPropertyManager["00"];
                            //string valOut;
                            //string materialId;
                            //swCustProp.Get4("Обозначение", true, out valOut, out materialId);

                            //MessageBox.Show(materialId);

                            //var swEnt = (Entity)swFeat;
                            //MessageBox.Show($"{swEnt == null}\n{FeatName}\n{swEnt.Select(true)}\n{swEnt.GetType()}");
                            //MessageBox.Show(swEnt.IGetComponent().GetType().ToString());

                            #endregion

                        }

                        #region To Delete

                        // swModel.ClearSelection2(true);
                        //   swModel.SelectByName(0, FeatName);

                        //var swSelMgr = (SelectionMgr)swModel.SelectionManager;
                        //var swEnt = (Entity)swSelMgr.GetSelectedObject6(1, -1);
                        //MessageBox.Show(component.GetImportedFileName());


                        //MessageBox.Show(swEnt.ModelName);

                        //var compone = swEnt.IGetComponent2();
                        //MessageBox.Show(compone.Name, compone.GetPathName());

                        #endregion

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.StackTrace + "\n" + ex.Message);
                    }

                    #region To Delete

                    //if (swFeat.GetTypeName() == "Stock")
                    //{
                    //    var swEnt = (Entity)swFeat;
                    //    var compone = swEnt.IGetComponent2();

                    //    MessageBox.Show(compone.Name, compone.GetPathName());                    


                    //    swModel.ClearSelection2(true);
                    //    swModel.SelectByName(0, FeatName);

                    //    ////swModel.Extension.SelectByID2(FeatName, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    //    //var swSelMgr = (SelectionMgr)swModel.SelectionManager;
                    //    ////var component = (Feature)swSelMgr.GetSelectedObject6(1, -1);
                    //    ////MessageBox.Show(component.GetImportedFileName());

                    //    //var component = (Feature)swSelMgr.GetSelectedObject6(1, -1);
                    //    //var swEnt = (Entity)swFeat;
                    //    //var compone = (Component2)swEnt.GetComponent();                        

                    //    //MessageBox.Show(compone.GetPathName());
                    //}

                    #endregion

                    #region To Delete

                    //if (swFeat.GetTypeName() == "Stock")
                    //{

                    //   //Component2 comp = swFeat.Select(false);



                    //    var swSubFeat = swFeat.IGetFirstSubFeature();

                    //    while ((swSubFeat != null))
                    //    {
                    //        MessageBox.Show($"Name - {swSubFeat.Name}\nTypeName - {swSubFeat.GetTypeName()}");

                    //        //if (swSubFeat.GetTypeName() == "OneBend" || swSubFeat.GetTypeName() == "SketchBend")
                    //        //{
                    //        //    swSubFeat.Select(false);
                    //        //}
                    //        swSubFeat = (Feature)swSubFeat.GetNextSubFeature();
                    //    }

                    //}


                    // var swSubFeat = swFeat.IGetFirstSubFeature();

                    // while ((swSubFeat != null))
                    //   {
                    //MessageBox.Show($"Name - {swSubFeat.Name}\nTypeName - {swSubFeat.GetTypeName()}");                        

                    //if (swSubFeat.GetTypeName() == "OneBend" || swSubFeat.GetTypeName() == "SketchBend")
                    //{     
                    //    swSubFeat.Select(false);
                    //}
                    //swSubFeat = (Feature)swSubFeat.GetNextSubFeature();
                    //  }

                    #endregion

                    swFeat = (Feature)swFeat.GetNextFeature();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        string PathToBar(IModelDoc2 swModel)
        {
            string path;
            try
            {
                var swPackAndGo = (PackAndGo)swModel.Extension.GetPackAndGo();
                var namesCount = swPackAndGo.GetDocumentNamesCount();

                MessageBox.Show("Names Count - " + namesCount);

                // Get current paths and filenames of the assembly's documents
                object fileNames;
                object[] pgFileNames = new object[namesCount - 1];
                var status = swPackAndGo.GetDocumentNames(out fileNames);
                pgFileNames = (object[])fileNames;

                if ((pgFileNames != null))
                {
                    for (var i = 0; i <= pgFileNames.GetUpperBound(0); i++)
                    {
                        //  MessageBox.Show(" The path and filename is: " + pgFileNames[i]);
                    }
                }
                //MessageBox.Show(" Путь к заготовке " + pgFileNames[0]);
                path = pgFileNames[1].ToString();
            }
            catch (Exception)
            {
                path = null;
            }
            return path;
        }


    }
}
