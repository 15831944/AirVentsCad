using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VentsCadLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class VaultSystem
    {
        public static void SetPmdVaultName(string vaultName)
        {
            SwEpdm.VaultName = null;
            SwEpdm.VaultName = vaultName;
            MessageBox.Show(vaultName, "Set VaultName");
        }

        public static string GetSwEpdRootFolder(string vaultName)
        {
            return SwEpdm.GetSwEpdRootFolderPath(vaultName);            
        }

        public static void AddToPdmByPath(string path)
        {
            SwEpdm.AddToPdmByPath(path);
        }

        public class SearchInVault : SwEpdm.EpdmSearch
        {
            public new static void SearchDoc(string fileName, SwDocType swDocType, out List<FindedDocuments> fileList, string vaultName)
            {
                SwEpdm.EpdmSearch.SearchDoc(fileName, swDocType, out fileList, vaultName);
            }
        }

        public class VentsCadFile : SearchInVault
        {
            public enum Type
            {
                Drawing,
                Assembly,
                Part,
                None
            }

            public static List<VentsCadFile> Get(string Name, Type type, string vaultName)
            {
                List<VentsCadFile> cadFiles = null;
                List<FindedDocuments> найденныеФайлы;
                SwDocType doctype;

                switch (type)
                {
                    case Type.Drawing:
                        doctype = SwDocType.SwDocDrawing;
                        break;
                    case Type.Assembly:
                        doctype = SwDocType.SwDocAssembly;
                        break;
                    case Type.Part:
                        doctype = SwDocType.SwDocPart;
                        break;
                    case Type.None:
                        doctype = SwDocType.SwDocNone;
                        break;
                    default:
                        doctype = SwDocType.SwDocLike;
                        break;
                }               

                SearchDoc(Name, doctype, out найденныеФайлы, vaultName);

                if (найденныеФайлы?.Count > 0)
                {
                    cadFiles = new List<VentsCadFile>();
                    try
                    {
                        foreach (var item in найденныеФайлы)
                        {
                            var cadFile = new VentsCadFile
                            {
                                PartIdPdm = item.PartIdPdm,
                                Time = item.Time,
                                ProjectId = item.ProjectId,
                                Path = item.Path,
                                PartName = item.PartName,
                                PartSize = item.PartSize
                            };
                            cadFiles.Add(item);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }
                return cadFiles;
            }

            public int PartIdSql { get; set; }

            public int PartSize { get; set; }

            public int PartIdPdm { get; set; }

            public int ProjectId { get; set; }

            public string PartName { get; set; }

            public string Path { get; set; }            
            
            public DateTime Time { get; set; }

            public string PartWithoutExtension => LocalPartFileInfo?.Substring((int)LocalPartFileInfo?.LastIndexOf('\\'));

            public string LocalPartFileInfo { get; set; }

            public string MessageForCheckOut { get; set; }
        }

        public static void GoToFile(string path)
        {
            SwEpdm.OpenContainingFolder(path);
        }

        public static void BatchUnLock1(List<string> filesPathesList)
        {
            SwEpdm.BatchUnLock1(filesPathesList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vaultName"></param>
        /// <returns></returns>
        public static string GetSwEpdRootFolderPath(string vaultName)
        {
            return SwEpdm.GetSwEpdRootFolderPath();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="vaultName"></param>
        public static void CreateDistDirectory(string path, string vaultName)
        {
            SwEpdm.CreateDistDirectory(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="vaultName"></param>
        public static void GetLastVersionOfFile(string path)
        {
            SwEpdm.GetLastVersionOfFile(path);
        }    

        public static void GetAsmFilesAsBuild(string path, string vaultName)
        {
            SwEpdm.GetAsmFilesAsBuild(path);
        }

        public class BatchParams
        {
            public string FilePath { get; set; }
            public int IdPdm { get; set; }
            public int CurrentVersion { get; set; }

        }

        public static void BatchGet(string vaultName, List<BatchParams> list, out List<PdmFilesAfterGet> pdmFilesAfterGets)
        {
            #region To Delete

            //string message = "";
            //foreach (var item in list)
            //{
            //    message = message + "\n" + $"{item.FilePath}{item.CurrentVersion}" ;
            //}

            //MessageBox.Show(message);

            #endregion

            var taskParams = list.Select(batchGetParam => new AddinConvertTo.Classes.FilesData.TaskParam
            {
                CurrentVersion = batchGetParam.CurrentVersion,
                FullFilePath = batchGetParam.FilePath,
                IdPDM  = batchGetParam.IdPdm
            }).ToList();           

            SwEpdm.BatchGet(vaultName, taskParams);

            pdmFilesAfterGets = null;

            #region To Delete

            //pdmFilesAfterGets = taskParams.Select(batchGetParamse => new PdmFilesAfterGet
            //{
            //    VersionToGet = batchGetParamse.CurrentVersion,
            //    FilePath = batchGetParamse.FullFilePath,
            //    FileName = batchGetParamse.FileName,
            //    VersionAfterGet = SwEpdm.GetLocalVersionOfFile(batchGetParamse.FullFilePath)
            //}).ToList();

            //message = "";
            //foreach (var item in pdmFilesAfterGets)
            //{
            //    message = message + "\n" + $"FilePath - {item.FilePath} VersionAfterGet - {item.VersionAfterGet}";
            //}

            //MessageBox.Show(message);

            #endregion
        }

        public class PdmFilesAfterGet
        {
            public string FilePath { get; set; }

            public string FileName { get; set; }

            public int VersionBeforeGet { get; set; }

            public int VersionToGet { get; set; }

            public int VersionAfterGet { get; set; }

            public bool Equal => VersionToGet == VersionAfterGet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filesList"></param>
        /// <param name="registration"></param>
        /// <param name="vaultName"></param>
        public static void CheckInOutPdm(List<FileInfo> filesList, bool registration, string vaultName)
        {
            SwEpdm.CheckInOutPdm(filesList, registration);
        }

        public static void CheckInOutPdm(List<FileInfo> filesList, bool registration)
        {
            SwEpdm.CheckInOutPdm(filesList, registration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="registration"></param>
        /// <param name="vaultName"></param>
        public static void CheckInOutPdm(string filePath, bool registration, string vaultName)
        {
            SwEpdm.CheckInOutPdm(filePath, registration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="fileIdPdm"></param>
        /// <param name="currentVerison"></param>
        /// <param name="getFileCopy"></param>
        /// <param name="vaultName"></param>
        public static void GetIdPdm(string path, out string fileName, out int fileIdPdm, out int currentVerison, out List<string> configurations, bool getFileCopy, string vaultName)
        {
            SwEpdm.GetIdPdm(path, out fileName, out fileIdPdm, out currentVerison, out configurations, getFileCopy);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<string, int>> GetSwEpdmVaults()
        {
            return SwEpdm.GetSwEpdmVaults();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filesList"></param>
        /// <param name="registration"></param>
        /// <param name="vaultName"></param>
        /// <param name="newFilesList"></param>
        public static void CheckInOutPdmNew(List<VentsCadFile> filesList, bool registration, 
            out List<VentsCadFile> newFilesList)
        {
            SwEpdm.CheckInOutPdmNew(filesList, registration, out newFilesList);
        }
    }
}
