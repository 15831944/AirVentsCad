using EdmLib;

namespace VentsCadLibrary
{
    public partial class VentsCad
    {
        #region Batches

        #region Variables

        IEdmFile5 aFile;
        IEdmFolder5 aFolder;
        IEdmFolder5 ppoRetParentFolder;
        IEdmPos5 aPos;
        IEdmBatchDelete3 batchDeleter;
        IEdmSelectionList6 fileList = null;
        IEdmBatchUnlock batchUnlocker;
        IEdmBatchAdd2 poAdder;

     //   EdmSelItem[] ppoSelection = new EdmSelItem[0];
        EdmSelectionObject poSel;

        #endregion

        IEdmBatchGet batchGetter;
        //TODO: BatchGet
        public void BatchGet()
        {
            //var vault1 = new EdmVault5();            
            //vault1.LoginAuto("Tets_debag", 0);

            //batchGetter = (IEdmBatchGet)vault2.CreateUtility(EdmUtility.EdmUtil_BatchGet);

            //int i = 0;
            //foreach (var taskVar in TaskListSql())
            //{
            //    //batchGetter.AddSelectionEx((EdmVault5)vault, Convert.ToInt32(taskVar.FileName), Convert.ToInt32(taskVar.FolderPath), taskVar.CurrentVersion);

            //    MessageBox.Show(taskVar.FileName);

            //    //aFile = vault.GetFileFromPath(FileName, out ppoRetParentFolder);

            //    aPos = aFile.GetFirstFolderPosition();
            //    aFolder = aFile.GetNextFolder(aPos);
            //    ppoSelection[i] = new EdmSelItem();
            //    ppoSelection[i].mlDocID = aFile.ID;
            //    ppoSelection[i].mlProjID = aFolder.ID;
            //    i = i + 1;
            //}
            
            //batchUnlocker.AddSelection((EdmVault5)vault, ppoSelection);
            

            //if ((batchGetter != null))
            //{
            //    batchGetter.CreateTree(this.Handle.ToInt32(), (int)EdmGetCmdFlags.Egcf_Nothing);

            //    ////fileCount = batchGetter.FileCount;
            //    //fileList = (IEdmSelectionList6)batchGetter.GetFileList((int)EdmGetFileListFlag.Egflf_GetLocked + (int)EdmGetFileListFlag.Egflf_GetFailed + (int)EdmGetFileListFlag.Egflf_GetRetrieved + (int)EdmGetFileListFlag.Egflf_GetUnprocessed);

            //    //aPos = fileList.GetHeadPosition();

            //    ////str = "Getting " + fileCount + " files: ";
            //    //while (!(aPos.IsNull))
            //    //{
            //    //    fileList.GetNext2(aPos, out poSel);
            //    //    //str = str + Constants.vbLf + poSel.mbsPath;
            //    //}

            //    //MsgBox(str)

            //    //var retVal = batchGetter.ShowDlg(this.Handle.ToInt32());
            //    // No dialog if checking out only one file

            //    //if ((retVal))
            //    //{
            //    batchGetter.GetFiles(this.Handle.ToInt32(), null);
            //    //}

            //}
        }

        //public void BatchUnLock()
        //{
        //    var vault2 = new EdmVault5();
        //    vault2.LoginAuto("Tets_debag", 0);

        //    batchUnlocker = (IEdmBatchUnlock2)vault2.CreateUtility(EdmUtility.EdmUtil_BatchUnlock);
        //    int i = 0;
        //    foreach (string FileName in FileList())
        //    {
        //        //aFile = vault.GetFileFromPath(FileName, out ppoRetParentFolder);
        //        //aPos = aFile.GetFirstFolderPosition();
        //        //aFolder = aFile.GetNextFolder(aPos);

        //        ppoSelection[i] = new EdmSelItem();
        //        ppoSelection[i].mlDocID = aFile.ID;
        //        ppoSelection[i].mlProjID = aFolder.ID;
        //        i++;
        //    }

        //    // Add selections to the batch of files to check in
        //    batchUnlocker.AddSelection((EdmVault5)vault, ppoSelection);

        //    if ((batchUnlocker != null))
        //    {
        //        batchUnlocker.CreateTree(this.Handle.ToInt32(), (int)EdmUnlockBuildTreeFlags.Eubtf_ShowCloseAfterCheckinOption + (int)EdmUnlockBuildTreeFlags.Eubtf_MayUnlock);
        //        fileList = (IEdmSelectionList6)batchUnlocker.GetFileList((int)EdmUnlockFileListFlag.Euflf_GetUnlocked + (int)EdmUnlockFileListFlag.Euflf_GetUndoLocked + (int)EdmUnlockFileListFlag.Euflf_GetUnprocessed);
        //        aPos = fileList.GetHeadPosition();

        //        while (!(aPos.IsNull))
        //        {
        //            fileList.GetNext2(aPos, out poSel);
        //        }

        //        //retVal = batchUnlocker.ShowDlg(this.Handle.ToInt32());
        //        batchUnlocker.UnlockFiles(this.Handle.ToInt32(), null);
        //    }
        //}

        //public List<string> BatchGetVariable(List<TaskParam> listType)
        //{
            //var listStr = new List<string>();

            //foreach (var fileVar in listType)
            //{
            //    aFolder = default(IEdmFolder5);

            //    IEdmFile5 edmFile = vault.GetFileFromPath(fileVar.fullFilePath, out aFolder);


            //    object oVarRevision;

            //    if (edmFile == null)
            //    {
            //        listStr.Add("0");
            //        continue;
            //    }

            //    var pEnumVar = (IEdmEnumeratorVariable8)edmFile.GetEnumeratorVariable();

            //    pEnumVar.GetVar("Revision", "", out oVarRevision);

            //    if (oVarRevision == null)
            //    {
            //        oVarRevision = "0";
            //    }

            //    listStr.Add(oVarRevision.ToString());
            //}
            //return listStr;
        //}

        //public void BatchAddFiles(List<string> list)
        //{
        //    poAdder = (IEdmBatchAdd2)vault2.CreateUtility(EdmUtility.EdmUtil_BatchAdd);
        //    //var newFolder = vault.RootFolderPath + "\\Новая папка (2)";
        //    //poAdder.AddFolderPath(newFolder, 0, (int)EdmBatchAddFolderFlag.Ebaff_Nothing);
        //    foreach (var fileName in list)
        //    {
        //        poAdder.AddFileFromPathToPath(fileName, null, 0);
        //    }
        //    var edmFileInfo = new EdmFileInfo[2];
        //    poAdder.CommitAdd(0, null, 0);
        //}

        public void BatchDelete()
        {
            //batchDeleter = (IEdmBatchDelete3)vault2.CreateUtility(EdmUtility.EdmUtil_BatchDelete);

            //IEdmFolder5 ppoRetParentFolder;
            //foreach (string FileName in FileList())
            //{
            //    aFile = vault.GetFileFromPath(FileName, out ppoRetParentFolder);
            //    aPos = aFile.GetFirstFolderPosition();
            //    aFolder = aFile.GetNextFolder(aPos);
            //    // Add selected file to the batch
            //    batchDeleter.AddFileByPath(FileName);
            //}
            //// Move files and folder to the Recycle Bin
            //batchDeleter.ComputePermissions(true, null);
            //batchDeleter.CommitDelete(this.Handle.ToInt32(), null);
        }

        #endregion

    }
}
