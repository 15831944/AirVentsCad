using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace MakeDxfSw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        void DxfCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateFlattPatternUpdateCutlistAndEdrawing(DataGrid1.ItemsSource as List<Configs>);
        }


        static bool IsSheetMetalPart(IPartDoc swPart)
        {
            try
            {
                var isSheet = false;

                var vBodies = swPart.GetBodies2((int)swBodyType_e.swSolidBody, false);

                foreach (Body2 vBody in vBodies)
                {
                    try
                    {
                        var isSheetMetal = vBody.IsSheetMetal();
                        if (!isSheetMetal) continue;
                        isSheet = true;
                    }
                    catch
                    {
                        isSheet = false;
                    }
                }

                return isSheet;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                return false;
            }
        }

        public void CreateFlattPatternUpdateCutlistAndEdrawing(List<Configs> configsList)
        {

            #region Сбор информации по детали и сохранение разверток

            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");

                IModelDoc2 swModel = swApp.IActiveDoc2;
                swModel.Extension.ViewDisplayRealView = false;

                if (!IsSheetMetalPart((IPartDoc)swModel))
                {
                    MessageBox.Show(@"Деталь не из листового материала");
                }
                
                string[] swModelConfNames2;

                if (configsList != null)
                {
                    swModelConfNames2 = configsList.Where(x => x.IsCheacked).Select(x=>x.Name).ToArray();
                }
                else
                {
                    swModelConfNames2 = (string[])swModel.GetConfigurationNames();
                }
                
                var activeconfiguration = (Configuration)swModel.GetActiveConfiguration();
                
                try
                {
                    foreach (var configName in from name in swModelConfNames2
                                               let config = (Configuration)swModel.GetConfigurationByName(name)
                                               where !config.IsDerived()
                                               select name)
                    {
                        swModel.ShowConfiguration2(configName);
                        swModel.EditRebuild3();

                        {
                            #region Разгибание всех сгибов

                            try
                            {
                                swModel.EditRebuild3();
                                var swPart = (IPartDoc)swModel;

                                Feature swFeature = swPart.FirstFeature();
                                const string strSearch = "FlatPattern";
                                while (swFeature != null)
                                {
                                    var nameTypeFeature = swFeature.GetTypeName2();

                                    if (nameTypeFeature == strSearch)
                                    {
                                        swFeature.Select(true);
                                        swPart.EditUnsuppress();

                                        Feature swSubFeature = swFeature.GetFirstSubFeature();
                                        while (swSubFeature != null)
                                        {
                                            var nameTypeSubFeature = swSubFeature.GetTypeName2();

                                            if (nameTypeSubFeature == "UiBend")
                                            {
                                                swFeature.Select(true);
                                                swPart.EditUnsuppress();
                                                swModel.EditRebuild3();
                                                swSubFeature.SetSuppression2(
                                                    (int)swFeatureSuppressionAction_e.swUnSuppressFeature,
                                                    (int)swInConfigurationOpts_e.swAllConfiguration,
                                                    swModelConfNames2);

                                            }
                                            swSubFeature = swSubFeature.GetNextSubFeature();
                                        }
                                    }
                                    swFeature = swFeature.GetNextFeature();
                                }
                                swModel.EditRebuild3();
                            }

                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.ToString());
                            }

                            #endregion

                            var swModelDocExt = swModel.Extension;
                            string val;
                            string valout;

                            var swCustProp = swModelDocExt.CustomPropertyManager[configName];
                            swCustProp.Get4("Код", false, out val, out valout);

                            if (!string.IsNullOrEmpty(valout))
                            {

                                valout = "-" + valout;
                            }

                            swModel.ForceRebuild3(false);

                            var thikness = GetFromCutlist(swModel, "Толщина листового металла");
                            var boolstatus = (IPartDoc)swModel;
                            Directory.CreateDirectory(@"C:\DXF\");
                            boolstatus.ExportFlatPatternView(
                                @"C:\DXF\" + swModel.GetTitle() + "-" + configName + "-" + thikness + valout + ".dxf",
                                (int)swExportFlatPatternViewOptions_e.swExportFlatPatternOption_RemoveBends);
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }

                try
                {
                    swModel.ShowConfiguration2(activeconfiguration.Name);

                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }

                #endregion
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        static string GetFromCutlist(IModelDoc2 swModel, string property)
        {
            var propertyValue = "";

            try
            {
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
                                string valOut;
                                swCustPrpMgr.Get4(property, true, out valOut, out propertyValue);
                            }
                            swSubFeat = swSubFeat.GetNextFeature();
                        }
                    }
                    swFeat2 = swFeat2.GetNextFeature();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

            return propertyValue;
        }


        public class Configs
        {
            public bool IsCheacked { get; set; }
            public string Name { get; set; }

            public static IEnumerable<Configs> GetConfigs()
            {
                var list = new List<Configs>();
                try
                {
                    var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");

                    IModelDoc2 swModel = swApp.IActiveDoc2;
                    swModel.Extension.ViewDisplayRealView = false;

                    if (!IsSheetMetalPart((IPartDoc)swModel))
                    {
                        MessageBox.Show(@"Деталь не из листового материала");
                    }

                    var swModelConfNames2 = (string[])swModel.GetConfigurationNames();

                    foreach (var configName in from name in swModelConfNames2
                        let config = (Configuration) swModel.GetConfigurationByName(name)
                        where !config.IsDerived()
                        select name)
                    {
                        list.Add(new Configs
                        {
                            Name = configName,
                            IsCheacked = true
                        });
                    }
                }
                catch (Exception exception)
                {
                    //MessageBox.Show(exception.ToString());
                }

                return list;
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var list = Configs.GetConfigs();
                if (!list.Any())
                {
                    MessageBox.Show("Откройте литовую детяль для развертывания в SolidWorks");
                    Close();
                }
                DataGrid1.ItemsSource = list;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
               
            }
            
        }

        private void DxfCreate_Copy_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
