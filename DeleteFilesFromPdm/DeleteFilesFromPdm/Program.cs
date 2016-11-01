using System;
using EdmLib;

namespace DeleteFilesFromPdm
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var vaultSource = new EdmVault5();
                vaultSource.LoginAuto("Vents-PDM", 0);
                var oFolder = vaultSource.GetFolderFromPath(@"E:\Vents-PDM\Проекты\Blauberg\02-01-Panels");
                

                Console.WriteLine(oFolder.ID);
                
               
                var edmFile5 = vaultSource.GetFileFromPath(@"E:\Vents-PDM\Проекты\Blauberg\02-01-Panels\02-01-2-6589.sldprt", out oFolder);
                Console.WriteLine(edmFile5.CurrentState);


                edmFile5.UndoLockFile(0, true);
                edmFile5.GetFileCopy(0, 0, oFolder.ID, (int)EdmGetFlag.EdmGet_RefsVerLatest);

                oFolder.DeleteFile(0, 262953, true);
                Console.WriteLine("Vents-PDM");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.ReadLine();
            }
        }
    }
}
