using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using EdmLib;
using HostingWindowsForms.Data;
using HostingWindowsForms.Host;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace HostingWindowsForms
{
    public class ClassOfTasks
    {
        //BackgroundWorker bw; 
        public ClassOfTasks()
        {
            SqlDependency.Stop(ConnectionString.ConString);

            // Start the dependency
            SqlDependency.Start(ConnectionString.ConString);
        }
        ~ClassOfTasks()
        {
            // Stop the dependency before exiting
            SqlDependency.Stop(ConnectionString.ConString);
        }

        #region Variables

        // SldWorks
        private SldWorks swApp;
        private ModelDoc2 swModel;
        private ModelDocExtension swExtension;
        private DrawingDoc swDraw;
        public swFileLoadWarning_e swFileLoadWarning;
        // Epdm     
        string RootFolder;

        #endregion

        #region Methods
            #region Run task and open documents

                BackgroundWorker backGroundWork = new BackgroundWorker();

                public void RunTask()
                {
                    try
                    {
                        if (backGroundWork.IsBusy != true)
                        {
                            backGroundWork.DoWork += (obj, ea) => Taskes();
                            backGroundWork.RunWorkerAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectionColor = Color.Red));
                        Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("-------------- Error --------------\r\n")));
                        Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += (String.Format("Source - {0},\r\n" +
                                                    "TargetSite - {1},\r\n" +
                                                    "Message - {2},\r\n" +
                                                    "StackTrace - {3}\r\n", ex.Source, ex.TargetSite, ex.Message, ex.StackTrace))));
                        Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("\r\n")));
                    }
                }
                public void Taskes()
                {
                    //var taskType = TaskListSql().GroupBy(x => x.TaskType, y => y.TaskInstancesID);
                    //var enumerable = taskType as IGrouping<int, string>[] ?? taskType.ToArray();
                    Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("Запускается...\r\n")));
                    
                    var taskSql = TaskListSql();
                    
                    //Group TaskType and TaskInstances
                    var enumerable = from c in TaskListSql()
                                        group c by new
                                        {
                                            c.TaskType,
                                            c.TaskInstancesID
                                        } into gcs
                                        select new TaskParam()
                                        {
                                            TaskType = gcs.Key.TaskType,
                                            TaskInstancesID = gcs.Key.TaskInstancesID
                                        };

                        foreach (var numberOfTaskType in enumerable)
                        {
                            switch (numberOfTaskType.TaskType)
                            {
                            #region case1
                            case 1:
                                var taskErptVar = BatchGetVariable(TaskListEprt());

                                var listConvertToErpt = taskSql.Where((t, i) => t.CurrentVersion != taskErptVar[i].ToString()).Select(t => new TaskParam
                                {
                                    CurrentVersion = t.CurrentVersion,
                                    FileName = t.FileName.ToUpper().Replace("SLDPRT", "EPRT"),
                                    FolderPath = t.FolderPath,
                                    FullFilePath = t.FullFilePath,
                                    TaskType = t.TaskType,
                                    Revision = t.Revision,
                                    IdPdm = t.IdPdm
                                }).ToList();
                                
                                BatchGet(listConvertToErpt);
                                BatchDelete(taskErptVar);
                                
                                var x = 1;
                                foreach (var filePath in listConvertToErpt)
                                {
                                    var idPdm = Convert.ToInt32(filePath.IdPdm);
                                    var rev = filePath.CurrentVersion == "" ? 0 : Convert.ToInt32(filePath.CurrentVersion);

                                    Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectionColor = Color.Green));
                                    Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += (x++ + ". " + filePath.FileName + ", TaskInstancesID: " + numberOfTaskType.TaskInstancesID + "\r\n")));

                                    OpenDoc(idPdm, rev, numberOfTaskType.TaskType, filePath.FullFilePath, filePath.FileName);
                                }

                                BatchAddFiles(listConvertToErpt);

                                BatchSetVariable(listConvertToErpt);

                                BatchUnLock(listConvertToErpt);

                                UpdateInstances(Convert.ToInt32(numberOfTaskType.TaskInstancesID));

                                break;
                            #endregion

                            #region case2
                            case 2:
                                var taskPdfVar = BatchGetVariable(TaskListPdf());

                                var listConvertToPdf = taskSql.Where((t, i) => t.CurrentVersion != taskPdfVar[i].ToString()).Select(t => new TaskParam
                                {
                                    CurrentVersion = t.CurrentVersion,
                                    FileName = t.FileName.ToUpper().Replace("SLDDRW", "PDF"),
                                    FolderPath = t.FolderPath,
                                    FullFilePath = t.FullFilePath,
                                    TaskType = t.TaskType,
                                    Revision = t.Revision
                                }).ToList();

                                //HostForm.Pbar.Maximum = listConvertToPdf.Count;

                                BatchGet(listConvertToPdf);

                                BatchDelete(taskPdfVar);

                                foreach (var filePath in listConvertToPdf)
                                {
                                    OpenDoc(0, 0, enumerable.Single().TaskType, filePath.FullFilePath, "");
                                }

                                BatchAddFiles(listConvertToPdf);

                                BatchSetVariable(listConvertToPdf);

                                BatchUnLock(listConvertToPdf);

                                UpdateInstances(Convert.ToInt32(numberOfTaskType.TaskInstancesID));
                                break;
                                #endregion

                            #region case3
                            case 3:
                                //TODO: SendMail
                                foreach (var param in taskSql)
                                {

                                    MessageBox.Show(@"SendMail - " + param.TaskType.ToString());


                                    SendMail(0);
                                }

                                UpdateInstances(Convert.ToInt32(numberOfTaskType.TaskInstancesID));

                                break;

                            #endregion
                            }
                        }

                    Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectionColor = Color.Black));
                    Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("Задача выполнена!\r\n")));
                    Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("\r\n")));
                }
                public void OpenDoc(int idPdm, int revision, int taskType, string filePath, string fileName)
                {
        
                    swApp = new SldWorks() {Visible = true};

                    Process[] processes = Process.GetProcessesByName("SLDWORKS");

                    var errors = 0;
                    var warnings = 0;

                    #region Case

                    switch (taskType)
                    {
                        case 1:

                            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectionColor = Color.Black));
                            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += (String.Format("Выполняется: {0}\r\n", filePath))));
  
                            swModel = swApp.OpenDoc6(filePath, (int) swDocumentTypes_e.swDocPART, (int) swOpenDocOptions_e.swOpenDocOptions_Silent, "", ref errors, ref warnings);
                            swModel = swApp.ActiveDoc;

                            if (!IsSheetMetalPart((IPartDoc) swModel))
                            {

                                Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectionColor = Color.DarkBlue));
                                Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("Не листовой металл!\r\n")));
                                Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("--------------------------------------------------------------------------------------------------------------\r\n")));
                                Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("\r\n")));

                                swApp.CloseDoc(filePath);
                                swApp.ExitApp();
                                swApp = null;

                                foreach (Process process in processes)
                                {
                                    process.CloseMainWindow();
                                    process.Kill();
                                }
                                return;
                            }

                            swExtension = (ModelDocExtension) swModel.Extension;
                            swModel.EditRebuild3();
                            swModel.ForceRebuild3(false);

                            CreateFlattPatternUpdate();

                            object[] confArray = swModel.GetConfigurationNames();
                            foreach (var confName in confArray)
                            {
                                Configuration swConf = swModel.GetConfigurationByName(confName.ToString());
                                if (swConf.IsDerived()) continue;
                                    
                                Area(confName.ToString());
                                GabaritsForPaintingCamera(confName.ToString());
                            }

                            ExportDataToXmlSql(fileName, idPdm, revision);

                            ConvertToErpt(filePath);
                            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectionColor = Color.Black));
                            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += (String.Format("{0} - Выполнен!\r\n", filePath))));
                            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("-----------------------------------------------------------------\r\n")));
                            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("\r\n")));

                            break;
                        case 2:

                            swModel = swApp.OpenDoc6(filePath, (int) swDocumentTypes_e.swDocDRAWING, (int) swOpenDocOptions_e.swOpenDocOptions_Silent, "", ref errors, ref warnings);

                            //if (warnings == (int)swFileLoadWarning_e.swFileLoadWarning_ReadOnly)
                            //{MessageBox.Show("This file is read-only.");}

                            swDraw = (DrawingDoc) swModel;
                            swExtension = (ModelDocExtension) swModel.Extension;
                            ConvertToPdf(filePath);

                            break;
                        case 3:

                            //swModel = swApp.OpenDoc6(filePath, (int)swDocumentTypes_e.swDocPART, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", ref errors, ref warnings);

                            MessageBox.Show("3");
                            break;
                    }

                    //TODO: swApp exit
                    swApp.CloseDoc(filePath);
                    swApp.ExitApp();
                    swApp = null;
                    
                    foreach (Process process in processes)
                    {
                        process.CloseMainWindow();
                        process.Kill();
                    }
                    #endregion
                }
                public void UpdateInstances(int taskInstancesId)
                {
                    using (var sqlConn = new SqlConnection(ConnectionString.ConString))
                    {
                        sqlConn.Open();

                        var q = @"UPDATE Epdm.TaskInstances
                                    SET TaskStatus = 1
                                    WHERE TaskInstancesID = " + taskInstancesId;

                        using (var cmd = new SqlCommand(q, sqlConn))
                        {
                            cmd.ExecuteReader();
                        }

                        sqlConn.Close();
                    }
                }
                DataTable CheckTaskStatus()
                {
                    var dt = new DataTable();
                    using (var con = new SqlConnection(ConnectionString.ConString))
                    {
                        con.Open();

                        var q = @"Select TaskStatus FROM Tasks";

                        using (var cmd = new SqlCommand(q, con))
                        {
                            dt.Load(cmd.ExecuteReader(CommandBehavior.CloseConnection));
                        }
                        con.Close();
                    }
                    return dt;
                }
            #endregion

            #region CutList

                #region CutList Data
                    public class CutListAndCustomPropFields
                    {
                        public string Title { get; set; }
                        public string ConfigName { get; set; }
                        public string MaterialId { get; set; }
                        public string Description { get; set; }
                        public string Number { get; set; }
                        public string Area { get; set; }
                        public string CodeMat { get; set; }
                        public string Height { get; set; }
                        public string Width { get; set; }
                        public string Thickness { get; set; }
                        public string Folds { get; set; }
                        public string Version { get; set; }
                        public string PaintX { get; set; }
                        public string PaintY { get; set; }
                        public string PaintZ { get; set; }
                    }
                    public List<CutListAndCustomPropFields> ListCutListPropertyName()
                    {
                        var listStr = new List<CutListAndCustomPropFields>();

                        var cutListClass = new CutListAndCustomPropFields
                        {
                            //ConfigName = "",
                            MaterialId = "MaterialID",
                            Description = "Наименование",
                            Number = "Обозначение",
                            Area = "Площадь покрытия",
                            CodeMat = "Код материала",
                            Height = "Длина граничной рамки",
                            Width = "Ширина граничной рамки",
                            Thickness = "Толщина листового металла",
                            Folds = "Сгибы",
                            Version = "Revision",
                            PaintX = "Длина",
                            PaintY = "Высота",
                            PaintZ = "Ширина"

                        };

                        listStr.Add(cutListClass);

                        return listStr;
                    }
                    string CutlistPropertyString(string property)
                    {
                        var propertyValue = "";

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

                        return propertyValue;
                    }
                    string CustomPropertyString(string property, string configName)
                    {
                        var propertyValue = "";

                        var customProp = swModel.Extension.CustomPropertyManager[configName];

                        string valOut;

                        customProp.Get4(property, true, out valOut, out propertyValue);

                        return propertyValue;
                    }
                    public List<CutListAndCustomPropFields> GetPropertyFromCutlistAndCustomProperty()
                    {
                        var listStr = new List<CutListAndCustomPropFields>();

                        string[] configArray = swModel.GetConfigurationNames();

                        var cutProp = ListCutListPropertyName();

                        for (var x = 0; x < ListCutListPropertyName().Count; x++)
                        {
                            foreach (var i in configArray)
                            {
                                var cutListClass = new CutListAndCustomPropFields();

                                Configuration swConf = swModel.GetConfigurationByName(i.ToString());

                                if (swConf.IsDerived()) continue;

                                //swModel.ShowConfiguration(i.ToString());

                                // CustomProperty
                                cutListClass.Title = swModel.GetTitle();
                                cutListClass.ConfigName = i;
                                cutListClass.MaterialId = CustomPropertyString(cutProp[x].MaterialId, i);
                                cutListClass.Description = CustomPropertyString(cutProp[x].Description, i);
                                cutListClass.Number = CustomPropertyString(cutProp[x].Number, i);
                                cutListClass.Area = CustomPropertyString(cutProp[x].Area, i);
                                cutListClass.CodeMat = CustomPropertyString(cutProp[x].CodeMat, i);
                                //CutListClass.Version = CustomPropertyString(cutProp[x].Version);
                                cutListClass.PaintX = CustomPropertyString(cutProp[x].PaintX, i);
                                cutListClass.PaintY = CustomPropertyString(cutProp[x].PaintY, i);
                                cutListClass.PaintZ = CustomPropertyString(cutProp[x].PaintZ, i);

                                //CutList
                                cutListClass.Height = CutlistPropertyString(cutProp[x].Height);
                                cutListClass.Width = CutlistPropertyString(cutProp[x].Width);
                                cutListClass.Thickness = CutlistPropertyString(cutProp[x].Thickness);
                                cutListClass.Folds = CutlistPropertyString(cutProp[x].Folds);

                                listStr.Add(cutListClass);
                            }
                        }
                        return listStr;
                    }
                #endregion

                static bool IsSheetMetalPart(IPartDoc swPart)
                {
                    var isSheet = false;

                    //var mod = (IModelDoc2)swPart;

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

                public void CreateFlattPatternUpdate()
                {
                    //Configuration activeconfiguration;
                    //activeconfiguration = (Configuration)swModel.GetActiveConfiguration();
                    var swModelConfNames = (string[])swModel.GetConfigurationNames();

                    foreach (var name in from name in swModelConfNames
                                         let config = (Configuration)swModel.GetConfigurationByName(name)
                                         where config.IsDerived()
                                         select name)
                    {
                        swModel.DeleteConfiguration(name);
                    }


                    var swModelConfNames2 = (string[])swModel.GetConfigurationNames();

                    foreach (var configName in from name in swModelConfNames2
                                               let config = (Configuration)swModel.GetConfigurationByName(name)
                                               where !config.IsDerived()
                                               select name)
                    {
                        swModel.ShowConfiguration2(configName);
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

                                        //swSubFeature.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature, (int)swInConfigurationOpts_e.swAllConfiguration, swModelConfNames2);
                                        swSubFeature.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressDependent, (int)swInConfigurationOpts_e.swConfigPropertySuppressFeatures, swModelConfNames2);
                                    }
                                    swSubFeature = swSubFeature.GetNextSubFeature();
                                }
                            }
                            swFeature = swFeature.GetNextFeature();
                        
                        }
                        swModel.EditRebuild3();
                        swModel.ForceRebuild3(false);
                    }
                  
                }
                // Площадь покрытия
                public void Area(string configName)
                {
                    var customProp = swModel.Extension.CustomPropertyManager[configName];

                    customProp.Delete("Площадь покрытия");

                    var valOut = "\"SW-SurfaceArea@@" + configName + "@" + swModel.GetTitle() + ".SLDPRT" + "\"";
                    customProp.Add2("Площадь покрытия", 30, valOut);
                }

                public void GabaritsForPaintingCamera(string configname)
                {

                const long valueset = 1000;
                const int swDocPart = 1;
                const int swDocAssembly = 2;

                Configuration swConf = swModel.GetConfigurationByName(configname);
                if (swConf.IsDerived() == false)
                {
                    swModel.EditRebuild3();

                    switch (swModel.GetType())
                    {
                        case swDocPart:
                            {
                                var part = (PartDoc)swModel;
                                var box = part.GetPartBox(true);

                                swModel.AddCustomInfo3(configname, "Длина", 30, "");
                                swModel.AddCustomInfo3(configname, "Ширина", 30, "");
                                swModel.AddCustomInfo3(configname, "Высота", 30, "");

                                swModel.CustomInfo2[configname, "Длина"] = Convert.ToString(Math.Round(Convert.ToDecimal(Math.Abs(box[0] - box[3]) * valueset), 0));
                                swModel.CustomInfo2[configname, "Ширина"] = Convert.ToString(Math.Round(Convert.ToDecimal(Math.Abs(box[1] - box[4]) * valueset), 0));
                                swModel.CustomInfo2[configname, "Высота"] = Convert.ToString(Math.Round(Convert.ToDecimal(Math.Abs(box[2] - box[5]) * valueset), 0));
                            }
                            break;
                        case swDocAssembly:
                            {

                                var swAssy = (AssemblyDoc)swModel;
                                var boxAss = swAssy.GetBox((int)swBoundingBoxOptions_e.swBoundingBoxIncludeRefPlanes);

                                swModel.AddCustomInfo3(configname, "Длина", 30, "");
                                swModel.AddCustomInfo3(configname, "Ширина", 30, "");
                                swModel.AddCustomInfo3(configname, "Высота", 30, "");

                                swModel.CustomInfo2[configname, "Длина"] =
                                    Convert.ToString(Math.Round(Convert.ToDecimal((long)(Math.Abs(boxAss[0] - boxAss[3]) * valueset)), 0));
                                swModel.CustomInfo2[configname, "Ширина"] =
                                    Convert.ToString(Math.Round(Convert.ToDecimal((long)(Math.Abs(boxAss[1] - boxAss[4]) * valueset)), 0));
                                swModel.CustomInfo2[configname, "Высота"] =
                                    Convert.ToString(Math.Round(Convert.ToDecimal((long)(Math.Abs(boxAss[2] - boxAss[5]) * valueset)), 0));
                            }
                            break;
                    }
                }
            }

            #endregion

            #region XML
                void ExportDataToXmlSql(string fileName, int idPdm, int version)
                {
                    //swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                    //swModel = swApp.ActiveDoc;

                    //var myXml = new System.Xml.XmlTextWriter(@"\\srvkb\SolidWorks Admin\XML\" + swModel.GetTitle() + ".xml", System.Text.Encoding.UTF8);
                    //const string xmlPath = @"\\srvkb\SolidWorks Admin\XML\";
                    const string xmlPath = @"C:\XML\";
                    var myXml = new XmlTextWriter(xmlPath + Path.GetFileNameWithoutExtension(swModel.GetPathName()) + ".xml", Encoding.UTF8);

                    myXml.WriteStartDocument();
                    myXml.Formatting = Formatting.Indented;
                    myXml.Indentation = 2;

                    // создаем элементы
                    myXml.WriteStartElement("xml");
                    myXml.WriteStartElement("transactions");
                    myXml.WriteStartElement("transaction");

                    myXml.WriteStartElement("document");

                    //MessageBox.Show("Xml - 2");
                    foreach (var configData in GetPropertyFromCutlistAndCustomProperty())
                    {
                    #region XML
                        // Конфигурация
                        myXml.WriteStartElement("configuration");
                        myXml.WriteAttributeString("name", configData.ConfigName);

                        // Материал
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Материал");
                        myXml.WriteAttributeString("value", configData.MaterialId);
                        myXml.WriteEndElement();

                        // Наименование  -- Из таблицы свойств
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Наименование");
                        myXml.WriteAttributeString("value", configData.Description);
                        myXml.WriteEndElement();

                        // Обозначение
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Обозначение");
                        myXml.WriteAttributeString("value", configData.Number);
                        myXml.WriteEndElement();

                        // Площадь покрытия
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Площадь покрытия");
                        myXml.WriteAttributeString("value", configData.Area);
                        myXml.WriteEndElement();

                        // ERP code
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Код_Материала");
                        myXml.WriteAttributeString("value", configData.CodeMat);
                        myXml.WriteEndElement();

                        // Длина граничной рамки

                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Длина граничной рамки");
                        myXml.WriteAttributeString("value", configData.Height);
                        myXml.WriteEndElement();

                        // Ширина граничной рамки
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Ширина граничной рамки");
                        myXml.WriteAttributeString("value", configData.Width);
                        myXml.WriteEndElement();

                        // Сгибы
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Сгибы");
                        myXml.WriteAttributeString("value", configData.Folds);
                        myXml.WriteEndElement();

                        // Толщина листового металла
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Толщина листового металла");
                        myXml.WriteAttributeString("value", configData.Thickness);
                        myXml.WriteEndElement();

                        // Версия последняя
                        myXml.WriteStartElement("attribute");
                        myXml.WriteAttributeString("name", "Версия");
                        myXml.WriteAttributeString("value", Convert.ToString(version));
                        myXml.WriteEndElement();

                        myXml.WriteEndElement();  //configuration

                    #endregion

                    #region SQL
                        //TODO: Sql
                        // var sqlConnection = new SqlConnection(Settings.Default.SQLBaseCon);
                        //"Data Source=srvkb;Initial Catalog=SWPlusDB;Persist Security Info=True;User ID=sa;Password=PDMadmin;MultipleActiveResultSets=True");
                        var sqlConnection = new SqlConnection(ConnectionString.ConString);
                        sqlConnection.Open();
                        var spcmd = new SqlCommand("UpdateCutList", sqlConnection) { CommandType = CommandType.StoredProcedure };
                        //spcmd.Parameters.Add("@MaterialsID", SqlDbType.Int).Value = КодМатериала;
                        //var partNumber = configData.Number;
                        //var description = configData.Description;
                        double workpieceX; Double.TryParse(configData.Height.Replace('.', ','), out workpieceX);
                        //Convert.ToDouble(configData.ДлинаГраничнойРамки.Replace('.', ','));
                        double workpieceY; Double.TryParse(configData.Width.Replace('.', ','), out workpieceY);
                        //Convert.ToDouble(configData.ШиринаГраничнойРамки.Replace('.', ','));
                        int bend; Int32.TryParse(configData.Width.Replace('.', ','), out bend);
                        //Convert.ToInt32(configData.Сгибы);
                        double thickness; Double.TryParse(configData.Thickness.Replace('.', ','), out thickness);
                        var configuration = configData.ConfigName;

                        //spcmd.Parameters.Add("@PartNumber", SqlDbType.NVarChar).Value = partNumber;
                        //spcmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = description;

   
                        //if (configData.Height == "") { workpieceX = 0; }
                        spcmd.Parameters.Add("@WorkpieceX", SqlDbType.Decimal).Value = workpieceX;
                        //if (configData.Width == "") { workpieceY = 0; }
                        spcmd.Parameters.Add("@WorkpieceY", SqlDbType.Decimal).Value = workpieceY;
                        //if (configData.Folds == "") { bend = 0; }
                        spcmd.Parameters.Add("@Bend", SqlDbType.Int).Value = bend;
                        //if (configData.Thickness == "") { thickness = 0; }
                        spcmd.Parameters.Add("@Thickness", SqlDbType.Decimal).Value = thickness;
                        spcmd.Parameters.Add("@Configuration", SqlDbType.NVarChar).Value = configuration;

                        //!!!!!!!!!!
                        //MessageBox.Show("Xml - 3");
                        var fileNameSldPrt = fileName.Replace("EPRT", "SLDPRT");

                        spcmd.Parameters.Add("@FileName", SqlDbType.NVarChar).Value = fileNameSldPrt;

                        var area = Convert.ToDecimal(configData.Area.Replace('.', ','));
    
                        spcmd.Parameters.Add("@SurfaceArea", SqlDbType.Decimal).Value = area;
                        spcmd.Parameters.Add("@Version", SqlDbType.Int).Value = version;
                        spcmd.Parameters.Add("@IdPDM", SqlDbType.Int).Value = idPdm;

                        //Габариты
                        var paintX = Convert.ToDecimal(configData.PaintX.Replace('.', ','));
                        spcmd.Parameters.Add("@PaintX", SqlDbType.Decimal).Value = paintX;

                        var paintY = Convert.ToDecimal(configData.PaintY.Replace('.', ','));
                        spcmd.Parameters.Add("@PaintY", SqlDbType.Decimal).Value = paintY;

                        var paintZ = Convert.ToDecimal(configData.PaintZ.Replace('.', ','));
                        spcmd.Parameters.Add("@PaintZ", SqlDbType.Decimal).Value = paintZ;

                        if (configData.MaterialId == null)
                        {
                            spcmd.Parameters.Add("@MaterialID", SqlDbType.Int).Value = configData.MaterialId;
                        }
                        else
                        {
                            spcmd.Parameters.Add("@MaterialID", SqlDbType.Int).Value = DBNull.Value;
                        }

                        spcmd.ExecuteNonQuery();
                        sqlConnection.Close();
                    #endregion
                    }

                    //myXml.WriteEndElement();// ' элемент CONFIGURATION
                    myXml.WriteEndElement();// ' элемент DOCUMENT
                    myXml.WriteEndElement();// ' элемент TRANSACTION
                    myXml.WriteEndElement();// ' элемент TRANSACTIONS
                    myXml.WriteEndElement();// ' элемент XML

                    // заносим данные в myMemoryStream
                    myXml.Flush();

                    myXml.Close();
                }
            #endregion

            #region Сохранение детали в eDrawing
                public void ConvertToErpt(string filePath)
                {
                    swApp.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swEdrawingsSaveAsSelectionOption, (int)swEdrawingSaveAsOption_e.swEdrawingSaveAll);
                    swApp.SetUserPreferenceToggle(((int)(swUserPreferenceToggle_e.swEDrawingsOkayToMeasure)), true);

                    var fileParthPdf = filePath.Replace("SLDPRT", "EPRT");
                    swModel.Extension.SaveAs(fileParthPdf, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, (int)swSaveAsOptions_e.swSaveAsOptions_Silent, null, 0, 0);
                }
            #endregion
      
            #region Convert to Pdf
            static public DispatchWrapper[] ObjectArrayToDispatchWrapperArray(object[] Objects)
            {

                var ArraySize = 0;
                ArraySize = Objects.GetUpperBound(0);
                var d = new DispatchWrapper[ArraySize + 1];
                var ArrayIndex = 0;
                for (ArrayIndex = 0; ArrayIndex <= ArraySize; ArrayIndex++)
                {
                    d[ArrayIndex] = new DispatchWrapper(Objects[ArrayIndex]);
                }

                return d;
            }
            public void ConvertToPdf(string filePath)
            {

                string[] obj = null;
                var swExportPDFData = default(ExportPdfData);
                DispatchWrapper[] dispWrapArr = null;
                obj = (string[])swDraw.GetSheetNames();
                var count = 0;
                count = obj.Length;
                var objs = new object[count - 1];

                for (var i = 0; i < count - 1; i++)
                {
                    swDraw.ActivateSheet((obj[i]));
                    var swSheet = (Sheet)swDraw.GetCurrentSheet();
                    objs[i] = swSheet;
                }

                swExportPDFData = (ExportPdfData)swApp.GetExportFileData((int)swExportDataFileType_e.swExportPdfData);
                dispWrapArr = (DispatchWrapper[])ObjectArrayToDispatchWrapperArray((objs));
                swExportPDFData.SetSheets((int)swExportDataSheetsToExport_e.swExportData_ExportSpecifiedSheets, (dispWrapArr));
                var fileParthPdf = filePath.Replace("SLDDRW", "pdf");
                swExtension.SaveAs(fileParthPdf, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, (int)swSaveAsOptions_e.swSaveAsOptions_Silent + (int)swSaveAsOptions_e.swSaveAsOptions_UpdateInactiveViews, swExportPDFData, 0, 0);

            }

        #endregion
        #endregion

        #region SqlDependency
        public delegate void NewMessage();
        public event NewMessage OnNewMessageChataData;
        void OnChange(object sender, SqlNotificationEventArgs e)
        {
            var dependency = sender as SqlDependency;

            // Notices are only a one shot deal
            // so remove the existing one so a new 
            // one can be added
            dependency.OnChange -= OnChange;
            
            // Fire the event
            if (OnNewMessageChataData != null)
            {
                OnNewMessageChataData();
                RunTask();
            }
        }
        #endregion

        #region Data Task
            public class TaskParam
            {
                public string FileName { get; set; }
                public string FolderPath { get; set; }
                public string CurrentVersion { get; set; }
                public int TaskType { get; set; }
                public string FullFilePath { get; set; }
                public string Revision { get; set; }
                public string IdPdm { get; set; }
                public string TaskInstancesID { get; set; }
            }

            public DataTable GetTaskListSql()
            {
                var dt = new DataTable();
                //const string q = @"SELECT FileName, FolderPath, Version, TaskType FROM Epdm.TaskSelection";
                //const string q = @"SELECT * FROM Tasks";

                const string q = @"SELECT ti.TaskInstancesID, t.TaskType, ti.TaskStatus, ts.FileName, ts.FolderPath, ts.Version, ts.IDPDM FROM Epdm.Task t
                      INNER JOIN Epdm.TaskInstances ti ON t.TaskID = ti.TaskID
                      INNER JOIN Epdm.TaskSelection ts ON ti.TaskInstancesID = ts.TaskInstanceID
                      WHERE ti.TaskStatus = 0";

                var sqlConn = new SqlConnection(ConnectionString.ConString);
                var cmd = new SqlCommand(q, sqlConn);

                var dependency = new SqlDependency(cmd);
                
                dependency.OnChange += new OnChangeEventHandler(OnChange);

                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }

                dt.Load(cmd.ExecuteReader(CommandBehavior.CloseConnection));

                //using (var sqlConn = new SqlConnection(conString))
                //using (var cmd = new SqlCommand(q, sqlConn))
                //{
                //}
                //dt.Load(cmd.ExecuteReader());

                sqlConn.Close();
                return dt;
            }

            #region List Sql, Pdf, Eprt
                public List<TaskParam> TaskListSql()
                {
                    RootFolder = HostingForm.Vault1.RootFolderPath;

                    return (from DataRow row in GetTaskListSql().Rows select new TaskParam
                    {

                        CurrentVersion = row["Version"].ToString(),
                        FileName = row["FileName"].ToString(),
                        FolderPath = row["FolderPath"].ToString(),
                        TaskType = Convert.ToInt32(row["TaskType"]),
                        FullFilePath = RootFolder + row["FolderPath"].ToString() + row["FileName"].ToString(),
                        //Revision = "",
                        IdPdm = row["IdPDM"].ToString(),
                        TaskInstancesID = row["TaskInstancesID"].ToString()

                    }).ToList();

                }
                public List<TaskParam> TaskListPdf()
                {
                    RootFolder = HostingForm.Vault1.RootFolderPath;
                    return (from DataRow row in GetTaskListSql().Rows
                            select new TaskParam
                            {
                                CurrentVersion = row["Version"].ToString(),
                                FileName = row["FileName"].ToString().ToUpper().Replace(".SLDDRW", ".pdf"),
                                FolderPath = row["FolderPath"].ToString(),
                                TaskType = Convert.ToInt32(row["TaskType"]),
                                FullFilePath = RootFolder + row["FolderPath"].ToString() + row["FileName"].ToString().ToUpper().Replace(".SLDDRW", ".pdf"),
                                Revision = ""

                            }).ToList();
                }
                public List<TaskParam> TaskListEprt()
                {
                    RootFolder = HostingForm.Vault1.RootFolderPath;
                    return (from DataRow row in GetTaskListSql().Rows
                            select new TaskParam
                            {
                                CurrentVersion = row["Version"].ToString(),
                                FileName = row["FileName"].ToString().ToUpper().Replace(".SLDPRT", ".EPRT"),
                                FolderPath = row["FolderPath"].ToString(),
                                TaskType = Convert.ToInt32(row["TaskType"]),
                                FullFilePath = RootFolder + row["FolderPath"].ToString() + row["FileName"].ToString().ToUpper().Replace(".SLDPRT", ".EPRT"),
                                Revision = ""

                            }).ToList();
                }
            #endregion
        #endregion

        #region Batches

        #region Variables

        IEdmFile5 aFile;
        IEdmFolder5 aFolder;
        IEdmFolder5 ppoRetParentFolder;
        IEdmPos5 aPos;
        //IEdmBatchDelete3 batchDeleter;
        IEdmSelectionList6 fileList = null;
        IEdmBatchUnlock batchUnlocker;
        IEdmBatchAdd2 poAdder;

        EdmSelItem[] ppoSelection;
        EdmSelectionObject poSel;

        #endregion

        IEdmBatchGet batchGetter;
        //TODO: BatchGet
        public void BatchGet(List<TaskParam> listType)
        {
            batchGetter = (IEdmBatchGet)HostingForm.Vault2.CreateUtility(EdmUtility.EdmUtil_BatchGet);

            var i = 0;
            foreach (var taskVar in listType)
            {

                aFile = HostingForm.Vault1.GetFileFromPath(taskVar.FullFilePath, out ppoRetParentFolder);

                aPos = aFile.GetFirstFolderPosition();

                aFolder = aFile.GetNextFolder(aPos);

                batchGetter.AddSelectionEx((EdmVault5)HostingForm.Vault1, aFile.ID, aFolder.ID, taskVar.CurrentVersion);
            }

            //batchUnlocker.AddSelection((EdmVault5)vault, ppoSelection);

            if ((batchGetter != null))
            {
                batchGetter.CreateTree(0, (int)EdmGetCmdFlags.Egcf_Nothing);

                ////fileCount = batchGetter.FileCount;
                //fileList = (IEdmSelectionList6)batchGetter.GetFileList((int)EdmGetFileListFlag.Egflf_GetLocked + (int)EdmGetFileListFlag.Egflf_GetFailed + (int)EdmGetFileListFlag.Egflf_GetRetrieved + (int)EdmGetFileListFlag.Egflf_GetUnprocessed);

                //aPos = fileList.GetHeadPosition();

                ////str = "Getting " + fileCount + " files: ";
                //while (!(aPos.IsNull))
                //{
                //    fileList.GetNext2(aPos, out poSel);
                //    //str = str + Constants.vbLf + poSel.mbsPath;
                //}

                //MsgBox(str)

                //var retVal = batchGetter.ShowDlg(this.Handle.ToInt32());
                // No dialog if checking out only one file

                //if ((retVal))
                //{
                batchGetter.GetFiles(0, null);
                //}
            }
        }  
        public void BatchUnLock(List<TaskParam> listType)
        {

            ppoSelection = new EdmSelItem[listType.Count];
 
            batchUnlocker = (IEdmBatchUnlock)HostingForm.Vault2.CreateUtility(EdmUtility.EdmUtil_BatchUnlock);

            RootFolder = HostingForm.Vault1.RootFolderPath;
            var i = 0;

            foreach (var fileName in listType)
            {
                var filePath = RootFolder + fileName.FolderPath + fileName.FileName;
                if (HostingForm.Vault1.GetFileFromPath(filePath, out ppoRetParentFolder) == null) continue;
                aFile = HostingForm.Vault1.GetFileFromPath(filePath, out ppoRetParentFolder);

                aPos = aFile.GetFirstFolderPosition();
                aFolder = aFile.GetNextFolder(aPos);

                ppoSelection[i] = new EdmSelItem();
                ppoSelection[i].mlDocID = aFile.ID;
                ppoSelection[i].mlProjID = aFolder.ID;

                i = i + 1;
            }

            // Add selections to the batch of files to check in
            batchUnlocker.AddSelection((EdmVault5)HostingForm.Vault1, ppoSelection);

            if ((batchUnlocker != null))
            {
                batchUnlocker.CreateTree(0, (int)EdmUnlockBuildTreeFlags.Eubtf_ShowCloseAfterCheckinOption + (int)EdmUnlockBuildTreeFlags.Eubtf_MayUnlock);
                fileList = (IEdmSelectionList6)batchUnlocker.GetFileList((int)EdmUnlockFileListFlag.Euflf_GetUnlocked + (int)EdmUnlockFileListFlag.Euflf_GetUndoLocked + (int)EdmUnlockFileListFlag.Euflf_GetUnprocessed);
                aPos = fileList.GetHeadPosition();

                while (!(aPos.IsNull))
                {
                    fileList.GetNext2(aPos, out poSel);
                }

                //retVal = batchUnlocker.ShowDlg(this.Handle.ToInt32());
                batchUnlocker.UnlockFiles(0, null);
            }
        }
        public List<TaskParam> BatchGetVariable(List<TaskParam> listType)
        {
            var listStr = new List<TaskParam>();

            foreach (var fileVar in listType)
            {
                aFolder = default(IEdmFolder5);

                IEdmFile5 edmFile = HostingForm.Vault1.GetFileFromPath(fileVar.FullFilePath, out aFolder);

                object oVarRevision;

                var revision = "";
                if (edmFile == null)
                {
                    revision = "";
                }
                else
                {
                    var pEnumVar = (IEdmEnumeratorVariable8)edmFile.GetEnumeratorVariable();

                    pEnumVar.GetVar("Revision", "", out oVarRevision);

                    if (oVarRevision == null)
                    {
                        revision = "0";
                    }
                    else
                    {
                        revision = oVarRevision.ToString();
                    }
                }

                listStr.Add(new TaskParam
                {
                    Revision = revision,
                    CurrentVersion = fileVar.CurrentVersion,
                    FileName = fileVar.FileName,
                    FolderPath = fileVar.FolderPath,
                    FullFilePath = fileVar.FullFilePath,
                    TaskType = fileVar.TaskType
                });
            }
            return listStr;
        }
        public void BatchSetVariable(List<TaskParam> listType)
        {
            RootFolder = HostingForm.Vault1.RootFolderPath;
            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("Файлы добавлены:\r\n")));
            foreach (var fileVar in listType)
            {
                var filePath = RootFolder + fileVar.FolderPath + fileVar.FileName;
                aFolder = default(IEdmFolder5);

                if (HostingForm.Vault1.GetFileFromPath(filePath, out aFolder) == null) continue;

                Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += (filePath + "\r\n")));

                //if (HostingForm.Vault1.GetFileFromPath(filePath, out aFolder) == null)
                //{
                //    MessageBox.Show("false");
                //}
                //else
                //{
                //    MessageBox.Show("true");
                //}

                IEdmFile5 edmFile = HostingForm.Vault1.GetFileFromPath(filePath, out aFolder);

                var pEnumVar = (IEdmEnumeratorVariable8)edmFile.GetEnumeratorVariable(); ;
                pEnumVar.SetVar("Revision", "", fileVar.CurrentVersion);
            }
        }
        public void BatchAddFiles(List<TaskParam> listType)
        {
            poAdder = (IEdmBatchAdd2)HostingForm.Vault2.CreateUtility(EdmUtility.EdmUtil_BatchAdd);

            //var newFolder = vault.RootFolderPath;

            RootFolder = HostingForm.Vault1.RootFolderPath;

            // poAdder.AddFolderPath(newFolder, 0, (int)EdmBatchAddFolderFlag.Ebaff_Nothing);

            foreach (var fileName in listType)
            {
                poAdder.AddFileFromPathToPath(RootFolder + fileName.FolderPath + fileName.FileName, RootFolder + fileName.FolderPath, 0);
            }
            //poAdder.ShowDlg(this.Handle.ToInt32(), (int)EdmAddFileDlgFlag.Eafdf_Nothing, "Files and folders in the batch:", "Batch Items");

            var edmFileInfo = new EdmFileInfo[listType.Count];
            poAdder.CommitAdd(0, null, 0);

            //var idx = edmFileInfo.GetLowerBound(0);
            //string msg = "";

            //while (idx <= edmFileInfo.GetUpperBound(0))
            //{
            //    string row = null;
            //    row = "(" + edmFileInfo[idx].mbsPath + ") arg = " + Convert.ToString(edmFileInfo[idx].mlArg);

            //    if (edmFileInfo[idx].mhResult == 0)
            //    {
            //        row = row + " status = OK ";
            //    }
            //    else
            //    {
            //        string oErrName = "";
            //        string oErrDesc = "";

            //        vault.GetErrorString(edmFileInfo[idx].mhResult, out oErrName, out oErrDesc);
            //        row = row + " status = " + oErrName;
            //    }

            //    idx = idx + 1;
            //    msg = msg + " " + row;

            //}
        }
        public void BatchDelete(List<TaskParam> listType)
        {
            var batchDeleter = (IEdmBatchDelete2)HostingForm.Vault2.CreateUtility(EdmUtility.EdmUtil_BatchDelete);
            IEdmFolder5 ppoRetParentFolder;

            foreach (var fileName in listType)
            {
                if (fileName.Revision == "") continue;

                aFile = HostingForm.Vault1.GetFileFromPath(fileName.FullFilePath, out ppoRetParentFolder);
                aPos = aFile.GetFirstFolderPosition();
                aFolder = aFile.GetNextFolder(aPos);

                // Add selected file to the batch
                batchDeleter.AddFileByPath(fileName.FullFilePath);
            }

            // Move files and folder to the Recycle Bin
            batchDeleter.ComputePermissions(true, null);
            batchDeleter.CommitDelete(0, null);
        }
        
        #endregion

        #region Task Send Mail
        public void SendMail(int parameter)
        {
            var arr = new string[2][];

            arr[0] = new string[] { "n.antoniuk@vents.kiev.ua", "n.chaikin@vents.kiev.ua" };
            arr[1] = new string[] { "d.orel@vents.kiev.ua" };

            for (var i = 0; i < arr.Length; i++)
            {
                for (var j = 0; j < arr[i].Length; j++)
                {
                    if (i == parameter)
                    {

                        var mail = new MailMessage("n.chaikin@vents.kiev.ua", arr[i][j] );
                        var client = new SmtpClient
                        {
                            Port = 26,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Host = "192.168.12.2"
                        };

                        mail.Subject = "this is a test email.";
                        mail.Body = "this is my test email body";
                        client.Send(mail);

                        Debug.Print("{0}", arr[i][j]);
                    }
                }
            }

        }

        #endregion
    }
}