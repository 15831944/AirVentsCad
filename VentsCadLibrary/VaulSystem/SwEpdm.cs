using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using EdmLib;
using System.Linq;

namespace VentsCadLibrary
{
    public class SwEpdm
    {
        public static void SetVault(string vaultName)
        {
            if (!string.IsNullOrEmpty(vaultName))
            {
                VaultName = vaultName;
                _edmVault5 = null;
            }
            if (_edmVault5 == null)
            {
                _edmVault5 = new EdmVault5();
                if (!_edmVault5.IsLoggedIn)
                {
                    _edmVault5.LoginAuto(VaultName, 0);
                }
            }
        }

        internal static string VaultName { get; set; } = "Vents-PDM";

        internal static EdmVault5 edmVault5
        {
            get
            {
                if (_edmVault5 == null)
                {
                    SetVault(null);
                }                                
                return _edmVault5;
            }
            set { value = _edmVault5; }
        }

        internal static EdmVault5 _edmVault5 { get; set; } 

        public static void AddToPdmByPath(string path)
        {
            if (UseDll)
            {
                throw new Exception("!UseDll");
            }

            else
            {
                try
                {
                    var fileDirectory = new FileInfo(path).DirectoryName;

                    var fileFolder = edmVault5.GetFolderFromPath(fileDirectory);
                    fileFolder.AddFile(fileFolder.ID, "", Path.GetFileName(path));
                }
                catch (Exception e)
                {
                    Логгер.Ошибка("Ошибка:" + e.Message, e.StackTrace, "SwEpdm", "AddToPdmByPath");
                }
            }
        }

        public static void OpenContainingFolder(string path)
        {
            try
            {
                IEdmVault8 edmVault8 =  (IEdmVault8)edmVault5;
                edmVault8.LoginAuto(VaultName, 0);
                if (string.IsNullOrEmpty(path)) return;
                edmVault8.OpenContainingFolder(path);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n" + exception.StackTrace);
            }
        }

        public static string GetSwEpdRootFolderPath()
        {
            try
            {              
                return edmVault5.RootFolderPath;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n" + exception.StackTrace);
                return null;
            }
        }

        public static string GetSwEpdRootFolderPath(string vaulName)
        {
            try
            {
                EdmVault5 vault = new EdmVault5();
                vault.LoginAuto(vaulName, 0);
                return vault.RootFolderPath;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n" + exception.StackTrace);
                return null;
            }
        }

        public static void CreateDistDirectory(string path)
        {
            try
            {
                var vault2 = (IEdmVault7)edmVault5;
                var directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Parent == null) return;
                var parentFolder = vault2.GetFolderFromPath(directoryInfo.Parent.FullName);
                parentFolder.AddFolder(0, directoryInfo.Name);
            }
            catch (Exception exception)
            {
               //MessageBox.Show(exception.Message + "\n" + exception.StackTrace);
            }
        }

        public static void CheckInOutPdm(string filePath, bool registration)
        {
            var retryCount = 2;
            var success = false;
            while (!success && retryCount > 0)
            {
                try
                {                    
                    var trys = 1;
                    m1:
                    try
                    {
                        IEdmFolder5 oFolder;
                        var edmFile5 = edmVault5.GetFileFromPath(filePath, out oFolder);
                        if (edmFile5 == null)
                        {
                            if (trys > 5)
                            {
                                return;
                            }
                            Thread.Sleep(1000);
                            trys++;
                            goto m1;
                        }

                        // Разрегистрировать
                        if (registration == false)
                        {
                            edmFile5.LockFile(oFolder.ID, 0);
                        }

                        // Зарегистрировать
                        if (registration) { edmFile5.UnlockFile(oFolder.ID, ""); }
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\n" + filePath, "1");
                        goto m1;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + filePath, "2");
                    retryCount--;
                    Thread.Sleep(200);
                    if (retryCount == 0)
                    {
                        //
                    }
                }
            }
        }

        public static void CheckInOutPdm(List<FileInfo> filesList, bool registration)
        {
            foreach (var file in filesList)
            {
                var retryCount = 2;
                var success = false;
                while (!success && retryCount > 0)
                {
                    try
                    {                        
                        IEdmFolder5 oFolder;                       
                        var edmFile5 = edmVault5.GetFileFromPath(file.FullName, out oFolder);

                        // Разрегистрировать
                        if (registration == false)
                        {
                            edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_Simple);
                            m1:
                            edmFile5.LockFile(oFolder.ID, 0);
                            //MessageBox.Show(edmFile5.Name);
                            Thread.Sleep(50);
                            var j = 0;
                            if (!edmFile5.IsLocked)
                            {
                                j++;
                                if (j > 5)
                                {
                                    goto m3;
                                }
                                goto m1;
                            }
                        }
                        // Зарегистрировать
                        if (registration)
                        {
                            m2:
                            edmFile5.UnlockFile(oFolder.ID, "");
                            Thread.Sleep(50);
                            var i = 0;
                            if (edmFile5.IsLocked)
                            {
                                i++;
                                if (i > 5)
                                {
                                    goto m4;
                                }
                                goto m2;
                            }
                        }
                        m3:
                        m4:
                        //LoggerInfo(string.Format("В базе PDM - {1}, зарегестрирован документ по пути {0}", file.FullName, vaultName), "", "CheckInOutPdm");
                        success = true;
                    }
                    catch (Exception exception)
                    {
                        Логгер.Ошибка($"Message - {exception.Message}\nfile.FullName - {file.FullName}\nStackTrace - {exception.StackTrace}", null, "CheckInOutPdm", "SwEpdm");                        
                        retryCount--;
                        Thread.Sleep(200);
                        if (retryCount == 0)
                        {
                            //
                        }
                    }
                }
                if (!success)
                {
                    //LoggerError($"Во время регистрации документа по пути {file.FullName} возникла ошибка\nБаза - {vaultName}. {ex.Message}", "", "CheckInOutPdm");
                }
            }
        }
        
        public static void GetAsmFilesAsBuild(string path)
        {
            try
            {                
                IEdmFolder5 oFolder;
                GetEdmFile5(path, out oFolder).GetFileCopy(0, 0, oFolder.ID,
                    (int)EdmGetFlag.EdmGet_Refs + (int)EdmGetFlag.EdmGet_RefsOnlyMissing + (int)EdmGetFlag.EdmGet_RefsOverwriteLocked);
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nPath - {path}\nStackTrace - {exception.StackTrace}", null, "GetAsmFilesAsBuild", "SwEpdm");
            }
        }    

        public static void GetLastVersionOfFile(string path)
        {
            try
            {
                IEdmFolder5 oFolder;
                GetEdmFile5(path, out oFolder).GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nPath - {path}\nStackTrace - {exception.StackTrace}", null, "GetLastVersionOfFile", "SwEpdm");
            }
        }

        public static int GetLocalVersionOfFile(string path)
        {
            try
            {
                IEdmFolder5 oFolder;
                var ver = GetEdmFile5(path, out oFolder);
                return ver.CurrentVersion;
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nPath - {path}\nStackTrace - {exception.StackTrace}", null, "GetLastVersionOfFile", "SwEpdm");
                return 0;
            }
        }

        internal static IEdmFile5 GetEdmFile5(string path, out IEdmFolder5 folder)
        {
            folder = null;
            try
            {               
                IEdmFolder5 oFolder;             
                var edmFile5 = edmVault5.GetFileFromPath(path, out oFolder);
                edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);                
                folder = oFolder;
                return edmFile5;
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nPath - {path}\nStackTrace - {exception.StackTrace}", null, "GetEdmFile5", "SwEpdm");
                return null;
            }
        }

        internal int GetPdmIds(string path, out int projectId)
        {
            var Id = 0;
            projectId = 0;
            try
            {
                IEdmFolder5 oFolder;
                Id = GetEdmFile5(path, out oFolder).ID;
                projectId = oFolder.ID;
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nPath - {path}\nStackTrace - {exception.StackTrace}", null, "GetPdmIds", "SwEpdm");
            }
            return Id;            

        }

        public static int GetVersionOfFile(string path, string vaultName)
        {
            IEdmFolder5 oFolder;
            return GetEdmFile5(path, out oFolder).CurrentVersion;
        }

        public static List<KeyValuePair<string, int>> GetSwEpdmVaults() 
        {
            var vaults = new List<KeyValuePair<string, int>>();
            try
            {                
                var vault = edmVault5 as IEdmVault8;
                Array views;
                Debug.Assert(vault != null, "vault != null");
                vault.GetVaultViews(out views, false);

                foreach (EdmViewInfo view in views)
                {
                    vaults.Add(new KeyValuePair<string, int>(view.mbsVaultName, 1));
                }
            }
            catch (Exception)
            {
                vaults.Add(new KeyValuePair<string, int>("Не найдено доступных хранилищ", 9));
            }
            return vaults;
        }

        public static void GetIdPdm(string path, out string fileName, out int fileIdPdm, out int currentVerison, out List<string> configs, bool getFileCopy)
        {
            fileName = null;
            fileIdPdm = 0;
            currentVerison = 0;
            configs = new List<string>();
            try
            {
                IEdmFolder5 oFolder;
                var tries = 1;
                m1:
                Thread.Sleep(500);
                path = new FileInfo(path).FullName;               

                var edmFile5 = edmVault5.GetFileFromPath(path, out oFolder);
                
                if (oFolder == null)
                {
                    tries++;
                    if (tries > 10)
                    {
                        return;
                    }
                    goto m1;
                }

                if (getFileCopy)
                {
                    try
                    {
                        edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
                    }
                    catch (Exception exception)
                    {
                        Логгер.Ошибка($"Message - {exception.Message}\nPath - {path}\nStackTrace - {exception.StackTrace}", null, "GetIdPdm", "SwEpdm");
                    }
                }

                fileName = edmFile5.Name;
                fileIdPdm = edmFile5.ID;
                currentVerison = edmFile5.CurrentVersion;
                EdmStrLst5 list = edmFile5.GetConfigurations();

                IEdmPos5 pos = default(IEdmPos5);
                pos = list.GetHeadPosition();
                string cfgName = null;
                while (!pos.IsNull)
                {
                    cfgName = list.GetNext(pos);
                    if (cfgName == "@") continue;                    
                    configs.Add(cfgName);
                }                
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nPath - {path}\nStackTrace - {exception.StackTrace}", null, "GetIdPdm", "SwEpdm");
            }
        }

        internal static void GetIdPdm(string path, out string fileName, out int fileIdPdm, EdmVault5 edmVault5)
        {
            fileName = null;
            fileIdPdm = 0;
            try
            {
                IEdmFolder5 oFolder;
                var tries = 1;
                m1:
                Thread.Sleep(500);
                path = new FileInfo(path).FullName;

                var edmFile5 = edmVault5.GetFileFromPath(path, out oFolder);

                if (oFolder == null)
                {
                    tries++;
                    if (tries > 10)
                    {
                        return;
                    }
                    goto m1;
                }

                try
                {
                    edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);
                }
                catch (Exception exception)
                {                    
                    Логгер.Ошибка($"Message - {exception.Message}\nPath - {path}\nStackTrace - {exception.StackTrace}", null, "GetIdPdm", "SwEpdm");
                }

                fileName = edmFile5.Name;
                fileIdPdm = edmFile5.ID;
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nPath - {path}\nStackTrace - {exception.StackTrace}", null, "GetIdPdm", "SwEpdm");                
            }
        }
       
        public static void CheckInOutPdmNew(List<VaultSystem.VentsCadFile> filesList, bool registration, out List<VaultSystem.VentsCadFile> newFilesList)
        {                     

            BatchAddFiles(filesList, edmVault5);
            
            var newFiles = filesList.Where(x => string.IsNullOrEmpty(x.MessageForCheckOut)).ToList();
            var filesToUpdate = filesList.Where(x => !string.IsNullOrEmpty(x.MessageForCheckOut)).ToList();                      

            try
            {
                if (newFiles?.Count > 0)
                {
                    BatchUnLock(newFiles, edmVault5);
                }                
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nStackTrace - {exception.StackTrace}", null, "CheckInOutPdmNew", "SwEpdm");
            }

            try
            {
                if (filesToUpdate?.Count > 0)
                {
                    BatchUnLock(filesToUpdate, edmVault5);
                }
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nStackTrace - {exception.StackTrace}", null, "CheckInOutPdmNew", "SwEpdm");
            }

            newFilesList = new List<VaultSystem.VentsCadFile>();

            foreach (var file in filesList)
            {
                string fileName;
                int fileIdPdm;
                GetIdPdm(file.LocalPartFileInfo, out fileName, out fileIdPdm, edmVault5);

                newFilesList.Add(new VaultSystem.VentsCadFile
                {
                    PartName = fileName,
                    PartIdPdm = fileIdPdm,
                    LocalPartFileInfo = file.LocalPartFileInfo,
                    PartIdSql = file.PartIdSql
                });
            }
        }
        
        public static void BatchUnLock1(List<string> filesPathesList)
        {
            IEdmPos5 aPos;

            var batchUnlocker = (IEdmBatchUnlock2)edmVault5.CreateUtility(EdmUtility.EdmUtil_BatchUnlock);
            var i = 0;
            var ppoSelection = new EdmSelItem[filesPathesList.Count];
            foreach (var file in filesPathesList)
            {               

                IEdmFolder5 ppoRetParentFolder;
                var aFile = edmVault5.GetFileFromPath(file, out ppoRetParentFolder);
                aPos = aFile.GetFirstFolderPosition();
                var aFolder = aFile.GetNextFolder(aPos);

                ppoSelection[i] = new EdmSelItem
                {
                    mlDocID = aFile.ID,
                    mlProjID = aFolder.ID
                };
                i++;
            }

            // Add selections to the batch of files to check in
            batchUnlocker.AddSelection(edmVault5, ppoSelection);

            batchUnlocker.CreateTree(0, (int)EdmUnlockBuildTreeFlags.Eubtf_ShowCloseAfterCheckinOption + (int)EdmUnlockBuildTreeFlags.Eubtf_MayUnlock);
            var fileList = (IEdmSelectionList6)batchUnlocker.GetFileList((int)EdmUnlockFileListFlag.Euflf_GetUnlocked + (int)EdmUnlockFileListFlag.Euflf_GetUndoLocked + (int)EdmUnlockFileListFlag.Euflf_GetUnprocessed);
            aPos = fileList.GetHeadPosition();

            while (!(aPos.IsNull))
            {
                EdmSelectionObject poSel;
                fileList.GetNext2(aPos, out poSel);
            }
            batchUnlocker.UnlockFiles(0);
        }

        public static void BatchAdd(List<AddinConvertTo.Classes.FilesData.TaskParam> list)
        {           
            var edmVault7 = (IEdmVault7)edmVault5;
            AddinConvertTo.Classes.Batches.BatchAddFiles(edmVault7, list);
        }

        internal static List<AddinConvertTo.Classes.FilesData.TaskParam> ConvertVentsCadFiles(List<VaultSystem.VentsCadFile> filesList)
        {
            if (filesList?.Count == 0) return null;
            
            List<AddinConvertTo.Classes.FilesData.TaskParam> list = new List<AddinConvertTo.Classes.FilesData.TaskParam>();
            foreach (var file in filesList)
            {
                list.Add(new AddinConvertTo.Classes.FilesData.TaskParam { FullFilePath = new FileInfo(file.LocalPartFileInfo).FullName });
            }
            return list;
        }

        const bool UseDll = false;

        internal static void BatchAddFiles(List<VaultSystem.VentsCadFile> filesList, EdmVault5 edmVault5)
        {
            goto m1;

            #region Dll                                 

            var edmVault7 = (IEdmVault7)edmVault5;
            AddinConvertTo.Classes.Batches.BatchAddFiles(edmVault7, ConvertVentsCadFiles(filesList));
            return;

            #endregion

            m1:

            #region Code

            List<string> stringList = new List<string>();
            
            foreach (var item in filesList.OrderBy(x=>x.LocalPartFileInfo))
            {
                var inf = new FileInfo(item.LocalPartFileInfo);
                if (stringList.Contains(inf.Name)) continue;
                stringList.Add(inf.FullName);
            }
            var files = "";

            foreach (var file in stringList.Distinct())
            {
                files = files + "\n" + file;
            }            

            files = "";

            try
            {
                var  poAdder = (IEdmBatchAdd2)edmVault5.CreateUtility(EdmUtility.EdmUtil_BatchAdd);               

                foreach (var file in stringList.Distinct())
                {
                    var fileInfo = new FileInfo(file);
                    var directoryName = fileInfo.Directory.FullName;
                    files = files + $"\n File - {fileInfo.FullName} directory - {directoryName}";
                    poAdder.AddFileFromPathToPath(fileInfo.FullName, directoryName);
                }
                                
                try
                {                    
                    poAdder.CommitAdd(-1, null);
                }
                catch (Exception exception)
                {
                    Логгер.Ошибка($"Message - {exception.Message}\nStackTrace - {exception.StackTrace}", null, "BatchAddFiles", "SwEpdm");
                }
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nStackTrace - {exception.StackTrace}", null, "BatchAddFiles", "SwEpdm");
            }
            finally
            {
                //MessageBox.Show(files);
            }

            #endregion
        }

        internal static void BatchUnLock(List<VaultSystem.VentsCadFile> filesList, EdmVault5 edmVault5)
        {
            IEdmPos5 aPos;

            var batchUnlocker = (IEdmBatchUnlock2)edmVault5.CreateUtility(EdmUtility.EdmUtil_BatchUnlock);
            var i = 0;
            var ppoSelection = new EdmSelItem[filesList.Count];
            foreach (var file in filesList)
            {
                var fileInfo = new FileInfo(file.LocalPartFileInfo);

                if (string.IsNullOrEmpty(fileInfo.Extension)) continue;
              
                IEdmFolder5 ppoRetParentFolder;
                IEdmFile5 aFile;
                try
                {
                    aFile = edmVault5.GetFileFromPath(fileInfo.FullName, out ppoRetParentFolder);
                }
                catch (Exception exception)
                {
                    Логгер.Ошибка($"Message - {exception.Message}\nStackTrace - {exception.StackTrace}", null, "BatchUnLock", "SwEpdm");
                    continue;
                }

                if (aFile != null)
                {
                    try
                    {
                        aPos = aFile.GetFirstFolderPosition();
                        var aFolder = aFile.GetNextFolder(aPos);

                        ppoSelection[i] = new EdmSelItem
                        {                            
                            mlDocID = aFile.ID,
                            mlProjID = aFolder.ID                            
                        };
                        i++;
                    }
                    catch (Exception exception)
                    {
                        Логгер.Ошибка($"Message - {exception.Message}\nStackTrace - {exception.StackTrace}", null, "BatchUnLock", "SwEpdm");
                    }                    
                }
            }

            // Add selections to the batch of files to check in
            batchUnlocker.AddSelection(edmVault5, ppoSelection);
            batchUnlocker.CreateTree(0, (int)EdmUnlockBuildTreeFlags.Eubtf_ShowCloseAfterCheckinOption + (int)EdmUnlockBuildTreeFlags.Eubtf_MayUnlock);
            batchUnlocker.Comment = filesList[0].MessageForCheckOut;
            var fileList = (IEdmSelectionList6)batchUnlocker.GetFileList((int)EdmUnlockFileListFlag.Euflf_GetUnlocked + (int)EdmUnlockFileListFlag.Euflf_GetUndoLocked + (int)EdmUnlockFileListFlag.Euflf_GetUnprocessed);
            aPos = fileList.GetHeadPosition();

            while (!(aPos.IsNull))
            {
                EdmSelectionObject poSel;
                fileList.GetNext2(aPos, out poSel);
            }
            batchUnlocker.UnlockFiles(0);
        }

        public static void BatchGet(string vaultName, List<AddinConvertTo.Classes.FilesData.TaskParam> list)
        {            
            var edmVault7 = (IEdmVault7)edmVault5;
            BathGet(edmVault7, list);
        }

        internal static void BathGet(IEdmVault7 vault, List<AddinConvertTo.Classes.FilesData.TaskParam> list)
        {            
            BatchGet(vault, list);
        }

        public static void BatchGet(IEdmVault7 vault, List<AddinConvertTo.Classes.FilesData.TaskParam> files)
        {
            try
            {
                var batchGetter = (IEdmBatchGet)vault.CreateUtility(EdmUtility.EdmUtil_BatchGet);
                foreach (var taskVar in files)
                {                    
                    IEdmFolder5 ppoRetParentFolder;
                    IEdmFile5 aFile;
                    try
                    {
                        aFile = vault.GetFileFromPath(taskVar.FullFilePath, out ppoRetParentFolder);
                    }
                    catch (Exception ex)
                    {
                        Логгер.Ошибка($"Message - {ex.Message}\nStackTrace - {ex.StackTrace}", null, "BatchUnLock", "SwEpdm");                        
                        continue;
                    }                    
                    
                    aFile = (IEdmFile5)vault.GetObject(EdmObjectType.EdmObject_File, taskVar.IdPDM);
                    var aPos = aFile.GetFirstFolderPosition();
                    var aFolder = aFile.GetNextFolder(aPos);                   
                    batchGetter.AddSelectionEx((EdmVault5)vault, taskVar.IdPDM, aFolder.ID, taskVar.CurrentVersion);
                }
                if ((batchGetter != null))
                {
                    batchGetter.CreateTree(0, (int)EdmGetCmdFlags.Egcf_SkipUnlockedWritable + (int)EdmGetCmdFlags.Egcf_RefreshFileListing);                    
                    batchGetter.GetFiles(0, null);
                }               
            }
            catch (Exception exception)
            {
                Логгер.Ошибка($"Message - {exception.Message}\nStackTrace - {exception.StackTrace}", null, "BatchUnLock", "SwEpdm");              
            }
        }

        public class EpdmSearch
        {           
            /// <summary>
            /// Тип документа для поиска. 
            /// </summary>
            public enum SwDocType
            {
                /// <summary>
                /// Точное имя файла для поиска
                /// </summary>
                SwDocNone = 0,
                /// <summary>
                /// Имя файла с расширением .sldprt
                /// </summary>
                SwDocPart = 1,
                /// <summary>
                /// Имя файла с расширением .sldasm
                /// </summary>
                SwDocAssembly = 2,
                /// <summary>
                /// Имя файла с расширением .slddrw
                /// </summary>
                SwDocDrawing = 3,
                /// <summary>
                /// По схожести
                /// </summary>
                SwDocLike = 4,
            }

            public class FindedDocuments : VaultSystem.VentsCadFile
            {
               
            }

            public static void SearchDoc(string fileName, SwDocType swDocType, out List<FindedDocuments> fileList, string vaultName)
            {
                var files = new List<FindedDocuments>();
                try
                {                  
                    var edmVault7 = (IEdmVault7)edmVault5;
                    
                    //Search for all text files in the edmVault7
                    var edmSearch5 = (IEdmSearch5)edmVault7.CreateUtility(EdmUtility.EdmUtil_Search);

                    var extenison = "";
                    var like = "";

                    switch ((int)swDocType)
                    {
                        case 0:
                            extenison = "";
                            like = "";
                            break;
                        case 1:
                            like = "%";
                            extenison = ".sldprt";
                            break;
                        case 2:
                            like = "%";
                            extenison = "%.sldasm";
                            break;
                        case 3:
                            like = "%";
                            extenison = "%.slddrw";
                            break;
                        case 4:
                            like = "%";
                            extenison = "%.%";
                            break;
                    }
                    edmSearch5.FileName = like + fileName + extenison;

                    var edmSearchResult5 = edmSearch5.GetFirstResult();

                    while (edmSearchResult5 != null)
                    {
                        files.Add(new FindedDocuments
                            {
                                PartIdPdm = edmSearchResult5.ID,
                                PartName = edmSearchResult5.Name,
                                PartSize = edmSearchResult5.FileSize,
                                ProjectId = edmSearchResult5.ParentFolderID,
                                Path = edmSearchResult5.Path,
                                Time = (DateTime)edmSearchResult5.FileDate
                            });
                        edmSearchResult5 = edmSearch5.GetNextResult();
                    }

                    if (edmSearch5.GetFirstResult() == null)
                    {
                        //LoggerInfo("Файл не найден!");
                        files = null;
                    }
                }

                catch (Exception exception)
                {
                    Логгер.Ошибка($"Message - {exception.Message}\nStackTrace - {exception.StackTrace}", null, "SearchDoc", "SwEpdm");
                    //MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                }
                fileList = files;
            }
        }

    }
}