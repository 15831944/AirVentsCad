using AirVentsCadWpf.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using VentsCadLibrary;
using System.Linq;
using System.Collections.Generic;

namespace AirVentsCadWpf.AirVentsClasses.UnitsBuilding
{
    public partial class ModelSw
    {    
        public static bool OpenIfExist(string FilePath, VaultSystem.VentsCadFile.Type type, DateTime? lastChange)
        {
            var open = false;
            //Получение файлов в хранилище
            if (string.IsNullOrEmpty(FilePath)) return true;
            var fileName = Path.GetFileNameWithoutExtension(FilePath);

            var cadFiles = VaultSystem.VentsCadFile.Get(fileName, type, Settings.Default.PdmBaseName);
            if (cadFiles == null) return false;

            VaultSystem.VentsCadFile findedFile = null;

            try
            {
                findedFile = cadFiles.Single(x => Path.GetFileNameWithoutExtension(x.Path).ToLower() == fileName.ToLower());
            }
            catch (Exception ex)
            {
               //MessageBox.Show(ex.Message, "findedFile");
            }

            if (findedFile == null)
            {
                return false;
            }
            
            #region To delete

            //var filesList = "";
            //foreach (var file in cadFiles)
            //{
            //    filesList = filesList + "\n" + file.Path;
            //}
            //MessageBox.Show(filesList);

            ////Совпавший по наименованию и типу файл
            //MessageBox.Show(cadFiles[0].Path + "\n" + fileName);
            //var findedFile = cadFiles[0].ToString().ToLower().Equals(fileName.ToLower()) ? cadFiles[0] : null;
            //if (findedFile == null) return false;

            #endregion

            //Определение и получение данных в объект -olderFile- если файл записан раньше чем изменен шаблон (findedFile.Time < lastChange)
            var olderFile = VersionsFileInfo.Replaced.GetIfOlder(findedFile.Path, lastChange, findedFile.Time);

            MessageBoxResult result;
            if (olderFile == null)
            {
                result = MessageBox.Show(fileName + " уже есть в базе. Открыть? ", "", MessageBoxButton.YesNo);
            }
            else
            {
                result = MessageBox.Show(fileName + " уже есть в базе, но конструкция устарела.\nЧтобы обновить нажмите \"Да\",\nоткрыть существующую без обновления -  \"Отмена\"? ", "Обновить?", MessageBoxButton.YesNoCancel);
            }                    

            if (result == MessageBoxResult.No)
            {
                open = true;
            }
            else if (result != MessageBoxResult.No)
            {                
                if (result == MessageBoxResult.Yes)
                {
                    if (olderFile == null)
                    {                        
                        var processes = Process.GetProcessesByName("SLDWORKS");
                        if (processes?.Length > 0)
                        {
                            VentsCad.OpenSwDoc(findedFile.Path);
                        }
                        else
                        {
                            Process.Start("conisio://" + Settings.Default.TestPdmBaseName + "/open?projectid=" + findedFile.ProjectId +
                                  "&documentid=" + findedFile.PartIdPdm + "&objecttype=1");
                        }
                        open = true;
                    }
                    else
                    {
                        VaultSystem.CheckInOutPdm(new List<FileInfo> { new FileInfo(findedFile.Path) }, false);
                        open = false;
                    }   
                }
                if (result == MessageBoxResult.Cancel)
                {
                    var processes = Process.GetProcessesByName("SLDWORKS");
                    if (processes?.Length > 0)
                    {
                        VentsCad.OpenSwDoc(findedFile.Path);
                    }
                    else
                    {
                        Process.Start("conisio://" + Settings.Default.TestPdmBaseName + "/open?projectid=" + findedFile.ProjectId +
                              "&documentid=" + findedFile.PartIdPdm + "&objecttype=1");
                    }
                    open = true;

                    //VaultSystem.CheckInOutPdm(new List<FileInfo> { new FileInfo(findedFile.Path) }, false);
                    //open = false;
                                        
                }
            }
            return open;
        }                

    }
}
