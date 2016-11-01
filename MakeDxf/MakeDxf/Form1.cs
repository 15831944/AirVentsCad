using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace MakeDxf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateFlattPatternUpdateCutlistAndEdrawing();
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

        //private const string ШаблонЧертежаРазверткиВнеХранилища = @"\\srvkb\SolidWorks Admin\Templates\flattpattern.drwdot";

        private const string ШаблонЧертежаРазверткиВнеХранилища = "flattpattern.drwdot";//@"C:\flattpattern.drwdot";

        public void CreateFlattPatternUpdateCutlistAndEdrawing()
        {

            #region Сбор информации по детали и сохранение разверток

            //SldWorks swApp = null;
            try
            {
                var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                swApp = new SldWorks { Visible = true };

                IModelDoc2 swModel = swApp.IActiveDoc2;
                swModel.Extension.ViewDisplayRealView = false;

                if (!IsSheetMetalPart((IPartDoc)swModel))
                {
                    MessageBox.Show("Деталь не из листового материала");
                }

                string[] swModelConfNames;


                var activeconfiguration = (Configuration)swModel.GetActiveConfiguration();
                swModelConfNames = (string[])swModel.GetConfigurationNames();


                var swModelConfNames2 = (string[])swModel.GetConfigurationNames();

                try
                {
                    foreach (var configName in from name in swModelConfNames2
                                               let config = (Configuration)swModel.GetConfigurationByName(name)
                                               where !config.IsDerived()
                                               select name)
                    {
                        swModel.ShowConfiguration2(configName);
                        swModel.EditRebuild3();

                        FileInfo template = null;

                        try
                        {
                            template = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\" + ШаблонЧертежаРазверткиВнеХранилища);
                            //MessageBox.Show(template.FullName);

                            Thread.Sleep(1000);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.ToString());
                            template = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\" + ШаблонЧертежаРазверткиВнеХранилища);
                            Thread.Sleep(1000);

                        }
                        finally
                        {
                            if (template == null)
                            {
                                template = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\" + ШаблонЧертежаРазверткиВнеХранилища);
                            }
                        }

                        //MessageBox.Show(template.FullName);

                        var swDraw = (DrawingDoc)swApp.INewDocument2(template.FullName, (int)swDwgPaperSizes_e.swDwgPaperA0size, 0.841, 0.594);

                        swDraw.CreateFlatPatternViewFromModelView3(swModel.GetPathName(), configName, 0.841 / 2, 0.594 / 2, 0, true, false);

                        ((IModelDoc2)swDraw).ForceRebuild3(true);


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

                                            try
                                            {
                                                swSubFeature.SetSuppression2(
                                                    (int)swFeatureSuppressionAction_e.swUnSuppressFeature,
                                                    (int)swInConfigurationOpts_e.swAllConfiguration,
                                                    swModelConfNames2);
                                            }
                                            catch (Exception){}

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
                            MessageBox.Show(exception.Message);
                        }

                        #endregion

                        swModel.ForceRebuild3(false);

                        var thikness = GetFromCutlist(swModel, "Толщина листового металла");

                        var errors = 0;
                        var warnings = 0;
                        var newDxf = (IModelDoc2)swDraw;
                        try
                        {
                            Directory.CreateDirectory("C:\\Dxf\\");
                        }
                        catch (Exception){}

                        newDxf.Extension.SaveAs(
                            "C:\\Dxf\\" + Path.GetFileNameWithoutExtension(swModel.GetPathName()) + "-" + configName + "-" + thikness + ".dxf",
                            (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                            (int)swSaveAsOptions_e.swSaveAsOptions_UpdateInactiveViews, null, ref errors, ref warnings);

                        swApp.CloseDoc(Path.GetFileName(newDxf.GetPathName()));
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
                                //swCustPrpMgr.Add("Площадь поверхности", "Текст", "\"SW-SurfaceArea@@@Элемент списка вырезов1@ВНС-901.81.002.SLDPRT\"");
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
    }
}
