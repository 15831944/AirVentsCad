using System;
using System.Collections.Generic;
using System.IO;

namespace VentsCadLibrary
{
    public class VersionsFileInfo
    {
        public static Replaced replaced = Replaced.Instance;
        /// <summary>
        /// 
        /// </summary>
        public class Replaced
        {
            public static Replaced Instance => instance;

            private static readonly Replaced instance = new Replaced();
            static Replaced() { }
            private Replaced()
            {
                List = new List<OldFile>();
            }

            public List<OldFile> List { get; set; }

            public void Add(OldFile file)
            {
                List.Add(file);
            }

            public void Clear()
            {
                List.Clear();
            }

            public static bool ExistLatestVersion(string FilePath, VaultSystem.VentsCadFile.Type type, DateTime? lastChange, string vaultName)
            {
                if (string.IsNullOrEmpty(FilePath)) return true;
                var fileName = Path.GetFileNameWithoutExtension(FilePath);
                var cadFiles = VaultSystem.VentsCadFile.Get(fileName, type, vaultName);
                if (cadFiles == null) return false;                
                var findedFile = cadFiles[0];
                VaultSystem.CheckInOutPdm(new List<FileInfo> { new FileInfo(findedFile.Path) }, false);

                //Определение и получение данных в объект -olderFile- если файл записан раньше чем изменен шаблон (findedFile.Time < lastChange)
                var olderFile = GetIfOlder(findedFile.Path, lastChange, findedFile.Time);                

                if (olderFile == null)
                {
                    return true;
                }
                else return false;
            }

            public class OldFile : VaultSystem.VentsCadFile
            {
                public string FilePath { get; set; }
                public DateTime? LastTemplateChange { get; set; }
                public DateTime? LastWriteTime { get; set; }
                public bool IsOlder { get; set; }
            }

            public static OldFile GetIfOlder(string filePath, DateTime? lastChanged, DateTime fileTime, bool CheckOutIfOlder = false)
            {
                OldFile oldFile = null;
                if (lastChanged != null)
                {
                    var info = new FileInfo(filePath);
                    var compare = DateTime.Compare((DateTime)lastChanged, fileTime);

                    if (compare > 0)
                    {
                        oldFile = new OldFile
                        {
                            LastTemplateChange = lastChanged,
                            LastWriteTime = fileTime,
                            FilePath = new FileInfo(filePath)?.FullName,
                            MessageForCheckOut = $"Замена версии в связи с обновлением конструкции от {lastChanged}"                            
                        };
                        replaced.Add(oldFile);
                    }
                    //MessageBox.Show($"Name - {filePath}\nChanged - {lastChanged}\nWriteTime - {fileTime}\nCompare - /{compare}/");
                }
                return oldFile;
            }

            public static OldFile CheckOutAndGetIfOlderToReplace(string filePath, DateTime? lastChange, DateTime lastSaved)
            {
                VaultSystem.CheckInOutPdm(new List<FileInfo> { new FileInfo(filePath) }, false);
                return GetIfOlder(filePath, lastChange, lastSaved);
            }
        }
    }
}
