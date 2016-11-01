using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MakeDxfUpdatePartData;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
//using VentsMaterials;

namespace VentsCadLibrary
{
    public partial class VentsCad : IDisposable
    {
        #region DISPOSE CLASS

       

        private IntPtr _handle;
        // Other managed resource this class uses.
        private readonly System.ComponentModel.Component _component = new System.ComponentModel.Component();
        // Track whether Dispose has been called.
        private bool disposed;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (disposed) return;
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                _component.Dispose();
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // If disposing is false,
            // only the following code is executed.
            CloseHandle(_handle);
            _handle = IntPtr.Zero;

            // Note disposing has been done.
            disposed = true;
        }
        // Use interop to call the method necessary
        // to clean up the unmanaged resource.
        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private extern static bool CloseHandle(IntPtr handle);

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        /// <summary>
        /// 
        /// </summary>
        //public ModelSw()
        //{
        //    // do not re-create dispose clean-up code here.
        //    // calling dispose(false) is optimal in terms of
        //    // readability and maintainability.
        //    Dispose(false);
        //}

        //public ModelSw(IntPtr handle)
        //{
        //    this._handle = handle;
        //}

        #endregion

        public VentsCad(string connectionToSql = null, string vaultName = null, string destVaultName = null)
        {
            Dispose(false);

            if (!string.IsNullOrEmpty(connectionToSql)) { ConnectionToSql = connectionToSql; }
            if (!string.IsNullOrEmpty(vaultName)) { VaultName = vaultName; }
            if (!string.IsNullOrEmpty(destVaultName)) { DestVaultName = destVaultName; }

            sourceRootFolder = LocalPath(VaultName);
            destRootFolder = LocalPath(DestVaultName);
        }

        internal static string sourceRootFolder;
        internal static string destRootFolder;

        public static string ConnectionToSql { get; set; }// = "Data Source=pdmsrv;Initial Catalog=SWPlusDB;Persist Security Info=True;User ID=sa;Password=P@$$w0rd;MultipleActiveResultSets=True";

        internal static string VaultName { get; set; } = "Vents-PDM";

        internal static string DestVaultName { get; set; } = "Vents-PDM";

        internal static string LocalPath(string vault)
        {
            try
            {
                return VaultSystem.GetSwEpdRootFolderPath(vault);
            }
            catch (Exception e)
            {
                Логгер.Ошибка(string.Format("В базе - {1}, не удалось получить корнувую папку ({0})", e.Message, vault), e.StackTrace, "Add", "LocalPath(string Vault)");
                return null;
            }
        }

        internal static SldWorks _swApp;

        public static List<VaultSystem.VentsCadFile> NewComponents = new List<VaultSystem.VentsCadFile>();

        static bool ConvertToInt(IEnumerable<string> newStringParams)
        {
            foreach (var param in newStringParams)
            {
                try
                {                    
                    var y = Convert.ToInt32(param);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        internal static void GetLastVersionAsmPdm(string path, string vaultName)
        {            
            try
            {
                Логгер.Информация($"Получение последней версии по пути {path}\nБаза - {vaultName}", null, "", "GetLastVersionPdm");

                VaultSystem.GetLastVersionOfFile(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
                Логгер.Ошибка($"Во время получения последней версии по пути {path} возникла ошибка!\nБаза - {vaultName}. {e.Message}", e.StackTrace, null, "GetLastVersionPdm");
            }
        }

        internal static bool InitializeSw(bool visible)
        {
            try
            {
                _swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            }
            catch (Exception)
            {
                _swApp = new SldWorks { Visible = visible };
            }
            return _swApp != null;
        }

        public static void OpenSwDoc(string path)
        {           

            if (InitializeSw(true))
            {
                var extension = new FileInfo(path)?.Extension.ToLower().Replace(".","");               
                swDocumentTypes_e docType;
                switch (extension)
                {
                    case "sldprt":
                        docType = swDocumentTypes_e.swDocPART;
                        break;
                    case "sldasm":
                        docType = swDocumentTypes_e.swDocASSEMBLY;
                        break;
                    case "slddrw":
                        docType = swDocumentTypes_e.swDocDRAWING;
                        break;
                    default:
                        docType = swDocumentTypes_e.swDocNONE;
                        break;
                }               

                if (extension != null)
                {                    
                    _swApp.OpenDoc6(path, (int)docType, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                }                
            }
        }

        internal static void DelEquations(int index, IModelDoc2 swModel)
        {
            try
            {
                Логгер.Информация($"Удаление уравнения #{index} в модели {swModel.GetPathName()}", null, "", "DelEquations");
                var myEqu = swModel.GetEquationMgr();
                myEqu.Delete(index);
                swModel.EditRebuild3();                
            }
            catch (Exception e)
            {
                Логгер.Ошибка($"Удаление уравнения #{index} в модели {swModel.GetPathName()}. {e.Message}", null, e.StackTrace, "DelEquations");
            }
        }

        internal static bool GetExistingFile(string fileName, int type, string vaultName)
        {
            List<SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы;
            switch (type)
            {
                case 0:
                    VaultSystem.SearchInVault.SearchDoc(fileName, SwEpdm.EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы, vaultName);
                    if (найденныеФайлы?.Count > 0) goto m1;
                    break;
                case 1:
                    VaultSystem.SearchInVault.SearchDoc(fileName, SwEpdm.EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы, vaultName);
                    if (найденныеФайлы?.Count > 0) goto m1;
                    break;
            }
            goto m2;
            m1:
            try
            {
                GetLastVersionAsmPdm(найденныеФайлы[0].Path, VaultName);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(найденныеФайлы[0].Path);
                return fileNameWithoutExtension != null && string.Equals(fileNameWithoutExtension, fileName, StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString()); return false;
            }
            m2:
            return false;

        }

        internal static void ComponentToAdd(string[] paths)
        {
            foreach (var path in paths)
            {
                ComponentToAdd(path);
            }
        }

        internal static void ComponentToAdd(string path)
        {
            NewComponents.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = path });
        }

        internal static void AddMaterial(ProductFactory.Material material, string newName)
        {
            try
            {
                VentsMatdll(new[] { material.Value, material.Thikness, material.Name, material.Code }, null, newName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        internal static void AddMaterial(string[] material, string newName)
        {
            try
            {
                  VentsMatdll(material, null, newName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        internal static bool GetExistingFile(string partName, out string path, out int fileId, out int projectId)
        {
            fileId = 0;
            projectId = 0;
            path = null;
            
            List<SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы;
            VaultSystem.SearchInVault.SearchDoc(partName, SwEpdm.EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы, VaultName);
            
            if (найденныеФайлы != null) goto m1;

            VaultSystem.SearchInVault.SearchDoc(partName, SwEpdm.EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы, VaultName);
            if (найденныеФайлы != null) goto m1;

            return false;
            m1:
            try
            {
                GetLastVersionAsmPdm(найденныеФайлы[0].Path, VaultName);
                path = найденныеФайлы[0].Path;
                fileId = найденныеФайлы[0].PartIdPdm;
                projectId = найденныеФайлы[0].ProjectId;
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                return false;
            }
        }
        
        static void PartInfoToXml(string filePath)
        {
            try
            {
                if (filePath == "") return;
                var extension = Path.GetExtension(filePath);
                if (extension == null) return;
                if (extension.ToLower() != ".sldprt") return;
                var @class = new MakeDxfExportPartDataClass
                {
                    PdmBaseName = VaultName
                };
                bool isErrors;
                string newEdrwFileName;
                @class.CreateFlattPatternUpdateCutlistAndEdrawing(filePath, out newEdrwFileName, out isErrors, false, false, true, true);
                if (!isErrors)
                {
                    Логгер.Информация("Закончена обработка детали " + Path.GetFileName(filePath), null, "", "PartInfoToXml");
                }
                else
                {
                    List<VaultSystem.VentsCadFile> list;
                    VaultSystem.CheckInOutPdmNew(new List<VaultSystem.VentsCadFile> { new VaultSystem.VentsCadFile { LocalPartFileInfo = newEdrwFileName } }, true,//   VaultName, 
                        out list);
                    Логгер.Информация("Закончена обработка детали " + Path.GetFileName(filePath) + " с ошибками", null, "", "PartInfoToXml");
                }
            }
            catch (Exception e)
            {
                Логгер.Ошибка("Ошибка:" + e.Message, e.StackTrace, "SwEpdm", "AddToPdmByPath");
                
            }
        }

        static void GabaritsForPaintingCamera(IModelDoc2 swmodel)
        {
            try
            {
                const long valueset = 1000;
                const int swDocPart = 1;
                const int swDocAssembly = 2;

                var configNames = swmodel.GetConfigurationNames();

                foreach (var configname in configNames)
                {
                    try
                    {
                        IConfiguration swConf = swmodel.GetConfigurationByName(configname);
                        if (swConf.IsDerived()) continue;
                    }
                    catch (Exception)
                    {
                       //
                    }

                    swmodel.EditRebuild3();


                    switch (swmodel.GetType())
                    {
                        case swDocPart:
                            {
                                var part = (IPartDoc)swmodel;

                                var box = part.GetPartBox(true);

                                swmodel.AddCustomInfo3(configname, "Длина", 30, "");
                                swmodel.AddCustomInfo3(configname, "Ширина", 30, "");
                                swmodel.AddCustomInfo3(configname, "Высота", 30, "");

                                swmodel.CustomInfo2[configname, "Длина"] =
                                    Convert.ToString(
                                        Math.Round(Convert.ToDecimal((long) (Math.Abs(box[0] - box[3])*valueset)), 0));
                                swmodel.CustomInfo2[configname, "Ширина"] =
                                    Convert.ToString(
                                        Math.Round(Convert.ToDecimal((long) (Math.Abs(box[1] - box[4])*valueset)), 0));
                                swmodel.CustomInfo2[configname, "Высота"] =
                                    Convert.ToString(
                                        Math.Round(Convert.ToDecimal((long) (Math.Abs(box[2] - box[5])*valueset)), 0));
                            }
                            break;

                        case swDocAssembly:
                            {
                                var swAssy = (AssemblyDoc) swmodel;
                                var boxAss = swAssy.GetBox((int) swBoundingBoxOptions_e.swBoundingBoxIncludeRefPlanes);

                                swmodel.AddCustomInfo3(configname, "Длина", 30, "");
                                swmodel.AddCustomInfo3(configname, "Ширина", 30, "");
                                swmodel.AddCustomInfo3(configname, "Высота", 30, "");

                                swmodel.CustomInfo2[configname, "Длина"] =
                                    Convert.ToString(
                                        Math.Round(Convert.ToDecimal((long) (Math.Abs(boxAss[0] - boxAss[3])*valueset)), 0));
                                swmodel.CustomInfo2[configname, "Ширина"] =
                                    Convert.ToString(
                                        Math.Round(Convert.ToDecimal((long) (Math.Abs(boxAss[1] - boxAss[4])*valueset)), 0));
                                swmodel.CustomInfo2[configname, "Высота"] =
                                    Convert.ToString(
                                        Math.Round(Convert.ToDecimal((long) (Math.Abs(boxAss[2] - boxAss[5])*valueset)), 0));
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), null, "GabaritsForPaintingCamera");
            }
        }

        internal void GetIdPdm(string path, out string fileName, out int fileIdPdm, out int curVer, out List<string> configurations)
        {
            fileName = null;
            fileIdPdm = 0;
            curVer = 0;
            configurations = null;

            try
            {             
                VaultSystem.GetIdPdm(path, out fileName, out fileIdPdm, out curVer, out configurations, false, VaultName);
            }
            catch (Exception e)
            {
                Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), path, "GetIdPdm");
            }
        }
        
        private static void VentsMatdll(IList<string> materialP1, IList<string> покрытие, string newName)
        {
            try
            {
                //MessageBox.Show("ApplyMaterial 1");

                //_swApp.ActivateDoc2(newName, true, 0);
                //var setMaterials = new SetMaterials();
                //ToSQL.Conn = ConnectionToSql;
                //var toSql = new ToSQL();             

                //MessageBox.Show("ApplyMaterial 2");

                //setMaterials.ApplyMaterial("", "00",
                //    1700,
                //    //Convert.ToInt32(materialP1[0]), 
                //    _swApp);
                //_swApp.IActiveDoc2.Save();

                //foreach (var confname in setMaterials.GetConfigurationNames(_swApp))
                //{
                //    foreach (var matname in setMaterials.GetCustomProperty(confname, _swApp))
                //    {
                //        toSql.AddCustomProperty(confname, matname.Name, _swApp);
                //    }
                //}

                //if (покрытие != null)
                //{
                //    if (покрытие[1] != "0")
                //    {
                //        setMaterials.SetColor("00", покрытие[0], покрытие[1], покрытие[2], _swApp);
                //    }
                //    _swApp.IActiveDoc2.Save();
                //}                

                //try
                //{
                //    string message;
                //    setMaterials.CheckSheetMetalProperty("00", _swApp, out message);
                //    if (message != null)
                //    {
                //        MessageBox.Show(message, newName);
                //    }
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show(e.Message + "\n" + e.StackTrace, "VentsMatdll");
                //    Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), newName, "VentsMatdll");
                //}
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace, "VentsMatdll");
                Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), newName, "VentsMatdll");
            }

            finally
            {
                try
                {
                    GabaritsForPaintingCamera(_swApp.IActiveDoc2);
                    _swApp.IActiveDoc2.Save();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e.StackTrace);
                    Логгер.Ошибка("Ошибка: " + e.StackTrace, e.GetHashCode().ToString("X"), newName, "VentsMatdll");
                }    
            }
        }

        //Todo using
        // Данный класс реализует интерейс IDisposable
        class FinalizeObject : IDisposable
        {
            private int Id { get; set; }

            public FinalizeObject(int id)
            {
                this.Id = id;
            }

            // Реализуем метод Dispose()
            public void Dispose()
            {
                Console.WriteLine("Высвобождение объекта!");
            }
        }

        class Program
        {
            static void Main(string[] args)
            {
                FinalizeObject obj = new FinalizeObject(4);
                obj.Dispose();

                Console.Read();

                using (FinalizeObject objusing = new FinalizeObject(4))
                {
                    // Необходимые действия
                }
            }
        }

        void SwPartParamsChangeWithNewName(string partName, string newName, string[,] newParams, bool newFuncOfAdding)
        {
            try
            {
                Логгер.Информация($"Начало изменения детали {partName}", newName, null, "SwPartParamsChangeWithNewName");
                var swDoc = _swApp.OpenDoc6(partName + ".SLDPRT", (int)swDocumentTypes_e.swDocPART, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                
                var modName = swDoc.GetPathName();
                for (var i = 0; i < newParams.Length / 2; i++)
                {
                    try
                    {
                        var myDimension = ((Dimension)(swDoc.Parameter(newParams[i, 0] + "@" + partName + ".SLDPRT")));
                        var param = Convert.ToDouble(newParams[i, 1]); var swParametr = param;
                        myDimension.SystemValue = swParametr / 1000;
                        swDoc.EditRebuild3();
                    }
                    catch (Exception e)
                    {
                        Логгер.Ошибка(string.Format("Во время изменения детали {4} произошла ошибка при изменении параметров {0}={1}. {2} {3}",
                            newParams[i, 0], newParams[i, 1], e.TargetSite, e.Message, Path.GetFileNameWithoutExtension(modName)), newName, null, "SwPartParamsChangeWithNewName");
                    }
                }

                if (newName == "") { return; }
                
                swDoc.EditRebuild3();
                swDoc.ForceRebuild3(false);


                if (!newFuncOfAdding)
                {
                    NewComponents.Add(new VaultSystem.VentsCadFile
                    {
                        LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName
                    });
                }
                
                if (newFuncOfAdding)
                {
                    NewComponents.Add(new VaultSystem.VentsCadFile
                    {
                        LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName,
                        PartIdSql = Convert.ToInt32(newName.Substring(newName.LastIndexOf('-') + 1))
                    });
                }

                swDoc.SaveAs2(newName + ".SLDPRT", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(newName + ".SLDPRT");
                Логгер.Информация($"Деталь {partName} изменена и сохранена по пути {new FileInfo(newName).FullName}", null, newName, "SwPartParamsChangeWithNewName");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        
        static void SwPartParamsChangeWithNewName(string partName, string newName, string[,] newParams, bool newFuncOfAdding, IList<string> copies)
        {
            try
            {
                Логгер.Информация($"Начало изменения детали {partName}", null, newName, "SwPartParamsChangeWithNewName");
                var swDoc = _swApp.OpenDoc6(partName + ".SLDPRT", (int)swDocumentTypes_e.swDocPART, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                var modName = swDoc.GetPathName();
                for (var i = 0; i < newParams.Length / 2; i++)
                {
                    try
                    {
                        var myDimension = ((Dimension)(swDoc.Parameter(newParams[i, 0] + "@" + partName + ".SLDPRT")));
                        var param = Convert.ToDouble(newParams[i, 1]); var swParametr = param;
                        myDimension.SystemValue = swParametr / 1000;
                        swDoc.EditRebuild3();
                    }
                    catch (Exception e)
                    {
                        Логгер.Ошибка(string.Format("Во время изменения детали {4} произошла ошибка при изменении параметров {0}={1}. {2} {3}",
                            newParams[i, 0], newParams[i, 1], e.TargetSite, e.Message, Path.GetFileNameWithoutExtension(modName)), null,
                            "", "SwPartParamsChangeWithNewName");
                    }
                }
                if (newName == "") { return; }

                GabaritsForPaintingCamera(swDoc);

                swDoc.EditRebuild3();
                swDoc.ForceRebuild3(false);

                if (!newFuncOfAdding)
                {
//                    ComponentToAdd(new FileInfo($@"{destRootFolder}\{DamperDestinationFolder}\{newNameAsm}").FullName);
  //                  NewComponents.Add(new VaultSystem.VentsCadFiles{LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName});
                }

                if (newFuncOfAdding)
                {
                    NewComponents.Add(new VaultSystem.VentsCadFile
                    {
                        LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName,
                        PartIdSql = Convert.ToInt32(newName.Substring(newName.LastIndexOf('-') + 1))
                    });
                }

                swDoc.SaveAs2(new FileInfo(newName + ".SLDPRT").FullName, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

                if (copies != null)
                {
                    swDoc.SaveAs2(new FileInfo(copies[0] + ".SLDPRT").FullName, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, true, true);
                    swDoc.SaveAs2(new FileInfo(copies[1] + ".SLDPRT").FullName, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, true, true);
                }
              
                _swApp.CloseDoc(newName + ".SLDPRT");

                Логгер.Информация($"Деталь {partName} изменена и сохранена по пути {new FileInfo(newName).FullName}", null, "", "SwPartParamsChangeWithNewName");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

    }
}
