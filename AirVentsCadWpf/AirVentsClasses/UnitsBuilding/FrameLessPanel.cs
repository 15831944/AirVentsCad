using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using AirVentsCadWpf.Properties;
using AirVentsCadWpf.Логирование;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using VentsCadLibrary;
using static VentsCadLibrary.VaultSystem;
using static VentsCadLibrary.VaultSystem.SearchInVault;


// TODO Добавить при длине от 1200 мм в варианте с ножками третьи ножки посредине

namespace AirVentsCadWpf.AirVentsClasses.UnitsBuilding
{
    public partial class ModelSw
    {

        /// <summary>
        /// 
        /// </summary>
        public List<VentsCadFile> NewComponentsFull = new List<VentsCadFile>();

        #region PanelsFrameless

        /// <summary>
        /// Panelses the frameless.
        /// </summary>
        /// <param name="typeOfPanel">The type of panel.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The lenght.</param>
        /// <param name="materialP1">The material p1.</param>
        /// <param name="materialP2">The meterial p2.</param>
        /// <param name="скотч">if set to <c>true</c> [СКОТЧ].</param>
        /// <param name="усиление">if set to <c>true</c> [УСИЛЕНИЕ].</param>
        /// <param name="config">The configuration.</param>
        /// <param name="расположениеПанелей">The РАСПОЛОЖЕНИЕ ПАНЕЛЕЙ.</param>
        /// <param name="покрытие">The ПОКРЫТИЕ.</param>
        /// <param name="расположениеВставок">The РАСПОЛОЖЕНИЕ ВСТАВОК.</param>
        /// <param name="первыйТип">The СОСТАВНАЯ.</param>
        /// <param name="типТорцевой"></param>
        /// <param name="screws"></param>
        /// <param name="partsToDeleteList"></param>
        /// <param name="existingAsmsAndParts"></param>
        /// <param name="onlySearch"></param>
        public void PanelsFrameless(string[] typeOfPanel, string width, string height, string[] materialP1,
            string[] materialP2, bool скотч, bool усиление, string config, string расположениеПанелей,
            string[] покрытие, string расположениеВставок, string[] первыйТип, string типТорцевой,
            Screws screws, out List<string> partsToDeleteList, out List<ExistingAsmsAndParts> existingAsmsAndParts, bool onlySearch)
        {
            var path = PanelsFramelessStr(typeOfPanel, width, height, materialP1, materialP2, скотч, усиление, config,
                расположениеПанелей, покрытие, расположениеВставок, первыйТип, null, "00",
                screws, out partsToDeleteList, out existingAsmsAndParts, onlySearch);
            if (path == "")  return; 

            if (MessageBox.Show($"Модель находится по пути:\n {new FileInfo(path).Directory}\n Открыть модель?",
                $" {Path.GetFileNameWithoutExtension(new FileInfo(path).FullName)} ", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                PanelsFramelessStr
                    (
                    typeOfPanel, width, height, materialP1, materialP2, скотч, усиление, config,
                    расположениеПанелей, покрытие, расположениеВставок, первыйТип, null, "00",
                    screws, out partsToDeleteList, out existingAsmsAndParts, onlySearch
                    );
            }
        }

        #endregion

        readonly List<KeyValuePair<string, string>> _patternsInAsmToDelete = 
            new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("1", "DerivedCrvPattern1"),
                new KeyValuePair<string, string>("2", "DerivedCrvPattern2"),
                new KeyValuePair<string, string>("4", "DerivedCrvPattern4"),
                new KeyValuePair<string, string>("5", "DerivedCrvPattern5"),
                new KeyValuePair<string, string>("8", "DerivedCrvPattern8"),
                new KeyValuePair<string, string>("9", "DerivedCrvPattern9"),
                new KeyValuePair<string, string>("10", "DerivedCrvPattern10"),
                new KeyValuePair<string, string>("21", "Первая панель право массив"),
                new KeyValuePair<string, string>("22", "Первая панель лево массив"),
                new KeyValuePair<string, string>("23", "Третья панель право массив"),
                new KeyValuePair<string, string>("24", "Третья панель лево массив")
            };


        /// <summary>
        /// 
        /// </summary>
        public List<KeyValuePair<string, string>> PartsToDeleteList = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// 
        /// </summary>
        public void DeleteAllPartsToDelete()
        {
            foreach (var part in PartsToDeleteList)
            {
                try
                {
                    DeletePartFromAssembly(part.Key, part.Value);
                }
                catch (Exception e){MessageBox.Show(e.ToString());}
            }
        }

        void DeletePartFromAssembly(string asmPath, string partPath)
        {
            GetLatestVersionAsmPdm(asmPath, Settings.Default.TestPdmBaseName);

            CheckInOutPdm(new List<FileInfo> {new FileInfo(asmPath) }, false, Settings.Default.TestPdmBaseName);

            if (!InitializeSw(true)) return;

            var swDoc = _swApp.OpenDoc6(asmPath, (int)swDocumentTypes_e.swDocASSEMBLY,
               (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            _swApp.Visible = true;

            if (swDoc == null)
            {
                return;
            }
            
            for (var i = 0; i < 5; i++)
            {
                var select = swDoc.Extension.SelectByID2(Path.GetFileNameWithoutExtension(partPath) + "-"+ i +"@" + Path.GetFileNameWithoutExtension(asmPath), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                if (select) {goto m1;}
            }
            return;
            
            m1:
            swDoc.EditDelete();
            
            swDoc.EditRebuild3();
            swDoc.ForceRebuild3(false);

            var assemblyDoc = (AssemblyDoc)swDoc;
            assemblyDoc.EditRebuild();
            assemblyDoc.ForceRebuild2(false);

            swDoc.Save();
            
            _swApp.CloseDoc(new FileInfo(asmPath).Name);
            _swApp.ExitApp();

            // todo Удаление цельной детали в панели

            BatchUnLock1(new List<string> { new FileInfo(asmPath).FullName });//, Settings.Default.TestPdmBaseName);
        }


        /// <summary>
        /// 
        /// </summary>
        public class ExistingAsmsAndParts
        {
            /// <summary>
            /// 
            /// </summary>
            public string IdAsm { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Comment { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string PartName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string PartQuery { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string PartPath { get; set; }
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="partName"></param>
            /// <param name="vaultName"></param>
            /// <returns></returns>
            public string GetPath(string partName, string vaultName)
            {
                List<VaultSystem.SearchInVault.FindedDocuments> найденныеФайлы;

                VaultSystem.SearchInVault.SearchDoc(partName, SwEpdm.EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы, vaultName);
                
                if (найденныеФайлы != null) return найденныеФайлы[0].Path;
                VaultSystem.SearchInVault.SearchDoc(partName, SwEpdm.EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы, vaultName);
                return найденныеФайлы?[0].Path;
            }

            /// <summary>
            /// 
            /// </summary>
            public string ExistInSistem { get; set; }
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partName"></param>
        /// <param name="path"></param>
        /// <param name="fileId"></param>
        /// <param name="projectId"></param>
        /// <param name="vaultName"></param>
        /// <param name="fileDate"></param>
        /// <returns></returns>
        public static bool GetExistingFile(string partName, out string path, out int fileId, out int projectId, out DateTime fileDate, string vaultName)
        {
            fileId = 0;projectId = 0;path = null; fileDate = new DateTime();

            List<SwEpdm.EpdmSearch.FindedDocuments> найденныеФайлы;

            SearchDoc(partName, SwEpdm.EpdmSearch.SwDocType.SwDocDrawing, out найденныеФайлы, vaultName);
            if (найденныеФайлы?.Count > 0) goto m1;

            SearchDoc(partName, SwEpdm.EpdmSearch.SwDocType.SwDocAssembly, out найденныеФайлы, vaultName);            
            if (найденныеФайлы?.Count > 0) goto m1;

            SearchDoc(partName, SwEpdm.EpdmSearch.SwDocType.SwDocPart, out найденныеФайлы, vaultName);
            if (найденныеФайлы?.Count > 0) goto m1;            

            return false;
            m1:
            try
            {
                path = найденныеФайлы[0].Path;
                GetLastVersionAsmPdm(path, Settings.Default.PdmBaseName);
                fileId = найденныеФайлы[0].PartIdPdm;
                projectId = найденныеФайлы[0].ProjectId;
                fileDate = найденныеФайлы[0].Time;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
        }
        
        bool GetExistingFile(string fileName, int type)
        {
            if (new FileInfo(fileName).Exists)
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }

            List<VaultSystem.SearchInVault.FindedDocuments> найденныеФайлы;
            switch (type)

            {
                case 0:
                    VaultSystem.SearchInVault.SearchDoc(fileName, VaultSystem.SearchInVault.SwDocType.SwDocAssembly, out найденныеФайлы, Settings.Default.PdmBaseName);
                    if (найденныеФайлы?.Count > 0) goto m1;
                    break;
                case 1:
                    VaultSystem.SearchInVault.SearchDoc(fileName, VaultSystem.SearchInVault.SwDocType.SwDocPart, out найденныеФайлы, Settings.Default.PdmBaseName);
                    if (найденныеФайлы?.Count > 0) goto m1;
                    break;
            }

            goto m2;
            m1:
            try
            {
                GetLatestVersionAsmPdm(найденныеФайлы[0].Path, Settings.Default.TestPdmBaseName);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(найденныеФайлы[0].Path);
                return fileNameWithoutExtension != null && string.Equals(fileNameWithoutExtension, fileName, StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());return false;
            }
            m2:
            return false;
        }
        
        static bool Усиливающая(string pType)
        {
            switch (pType)
            {
                case "24":
                case "25":
                case "26":
                case "27":
                case "28":
                case "29":
                    return true;
                default:
                    return false;
            }
        }
        
        static string ТипУсиливающей(string pType)
        {
            switch (pType)
            {
                case "24":
                    return "EE";
                case "25":
                    return "ED";
                case "26":
                    return "EZ";
                case "27":
                    return "TE";
                case "28":
                    return "TZ";
                case "29":
                    return "TD";
                default:
                    return null;
            }
        }


        #region PanelsFramelessStr

        /// <summary>
        /// Panelses the frameless string.
        /// </summary>
        /// <param name="typeOfPanel">The type of panel.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The lenght.</param>
        /// <param name="materialP1">The material p1.</param>
        /// <param name="materialP2">The material p2.</param>
        /// <param name="скотч">The СКОТЧ.</param>
        /// <param name="усиление">if set to <c>true</c> [УСИЛЕНИЕ].</param>
        /// <param name="config">configuration</param>
        /// <param name="расположениеПанелей">Расположение панелей</param>
        /// <param name="покрытие">ПОКРЫТИЕ</param>
        /// <param name="расположениеВставок">Расположение вставок</param>
        /// <param name="первыйТип">СОСТАВНАЯ</param>
        /// <param name="типТорцевой"></param>
        /// <param name="типДвойной"></param>
        /// <param name="screws"></param>
        /// <param name="partsToDeleteList"></param>
        /// <param name="existingAsmsAndParts"></param>
        /// <param name="onlySearch"></param>
        /// <returns></returns>
        public string PanelsFramelessStr(
            string[] typeOfPanel, string width, string height,
            string[] materialP1, string[] materialP2, bool скотч,
            bool усиление, string config, string расположениеПанелей,
            string[] покрытие, string расположениеВставок,
            string[] первыйТип, string типТорцевой, string типДвойной,
            Screws screws, out List<string> partsToDeleteList, 
            out List<ExistingAsmsAndParts> existingAsmsAndParts,
            bool onlySearch)
        {
            partsToDeleteList = new List<string>();
            existingAsmsAndParts = new List<ExistingAsmsAndParts>();

            if (screws == null){screws = new Screws { ByWidth = 0, ByHeight = 0 };}

            InValPanels.StringValue(расположениеПанелей);
            
            //MessageBox.Show($"type - {typeOfPanel[0]}\n расположениеВставок - {расположениеВставок}");

            ValProfils.StringValue(расположениеВставок);
            BackProfils.StringValue(типТорцевой);

            var WidthOfWindow = BackProfils.Height;
                

            var HeightOfWindow = BackProfils.Flange30 ?
                BackProfils.Width : (BackProfils.Width + 2);

          //  MessageBox.Show(HeightOfWindow.ToString());
            //MessageBox.Show($"type - {typeOfPanel[0]}\nWp1 - {ValProfils.Wp1}\nWp2 - {ValProfils.Wp2}");
           // return null;

            #region Начальные проверки и пути

            if (IsConvertToInt(new[] {width, height}) == false) return "-";

            bool needToAddStepInsertionAndStep;

            var pType = typeOfPanel[0];

            var типУсиливающей = ТипУсиливающей(pType);

            var усилисвающя = Усиливающая(pType);

            if (усилисвающя)
            {
                pType = "01";
            }
            
            switch (pType)
            {
                case "01":
                case "04":
                case "05":
                case "35":
                    needToAddStepInsertionAndStep = false;
                    break;
                default:
                    needToAddStepInsertionAndStep = true;
                    break;
            }

            if (усилисвающя) needToAddStepInsertionAndStep = true;

            var panelsUpDownConfigString =
                (pType != "04" & pType != "05" & pType != "01" & pType != "35")
                    ? InValPanels.InValUpDown() : "";

            #region Обозначение ДЕТАЛЕЙ и СБОРКИ из БАЗЫ

            #region Покрытие

            if (покрытие[0] == "Без покрытия")
            {
                покрытие[1] = "0";
                покрытие[2] = "0";
            }
            if (покрытие[3] == "Без покрытия")
            {
                покрытие[4] = "0";
                покрытие[5] = "0";
            }

            #endregion
            
            #region Задание наименований

            var sqlBaseData = new SqlBaseData();
            var newId = sqlBaseData.PanelNumber() + 1;
            var partIds = new List<KeyValuePair<int, int>>();

            #region панельВнешняя, панельВнутренняя            

            var панельВнешняя =
                new AddingPanel
                {
                    PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                    ElementType = 1,
                    Width = Convert.ToInt32(width),
                    Height = Convert.ToInt32(height),
                    PartThick = 40,
                    PartMat = Convert.ToInt32(materialP1[0]),
                    PartMatThick = Convert.ToDouble(materialP1[1].Replace('.', ',')),
                    Reinforcing = усиление,
                    Ral = покрытие[0],
                    CoatingType = покрытие[1],
                    CoatingClass = Convert.ToInt32(покрытие[2]),
                    Mirror = config.Contains("01"),
                    StickyTape = скотч,
                    StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,
                    AirHole = типТорцевой
                };
            var id = панельВнешняя.AddPart();

            панельВнешняя.PartQuery =
                $"PanelTypeId = {Convert.ToInt32(typeOfPanel[2])}, ElementType = {1}\nWidth = {Convert.ToInt32(width)}, Height = {Convert.ToInt32(height)},\nPartThick = 40, PartMat = {Convert.ToInt32(materialP1[0])}, PartMatThick = {Convert.ToDouble(materialP1[1].Replace('.', ','))}\nReinforcing = {усиление}, Ral = {покрытие[0]}, CoatingClass = {Convert.ToInt32(покрытие[2])}\nMirror = {config.Contains("01")}, StickyTape = {скотч}\nStepInsertion = {(needToAddStepInsertionAndStep ? расположениеВставок : null)}, AirHole = {типТорцевой}";

            if (типДвойной == "00")
            {
              //  partIds.Add(new KeyValuePair<int, int>(1, id));
            }

            панельВнешняя.NewName = "02-" + typeOfPanel[0] + "-1-" + id;
            
            var панельВнутренняя =
                new AddingPanel
                {
                    PanelTypeId = усилисвающя ? Convert.ToInt32(первыйТип[2]) : Convert.ToInt32(typeOfPanel[2]),
                    ElementType = 2,
                    Width = Convert.ToInt32(width),
                    Height = Convert.ToInt32(height),
                    PartThick = 40,
                    PartMat = Convert.ToInt32(materialP2[0]),
                    PartMatThick = Convert.ToDouble(materialP2[1].Replace('.', ',')),
                    Reinforcing = усиление,
                    Ral = покрытие[3],
                    CoatingType = покрытие[4],
                    CoatingClass = Convert.ToInt32(покрытие[5]),
                    Mirror = config.Contains("01"),
                    Step = needToAddStepInsertionAndStep ? расположениеПанелей : null,
                    StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,
                    AirHole = типТорцевой
                };
            id = панельВнутренняя.AddPart();

            if (типДвойной == "00")
            {
             //   partIds.Add(new KeyValuePair<int, int>(2, id));
            }

            панельВнутренняя.NewName = "02-" + pType + "-2-" + id;
            панельВнутренняя.PartQuery =
                $" PanelTypeId = {Convert.ToInt32(typeOfPanel[2])},ElementType = 2\nWidth = {Convert.ToInt32(width)}, Height = {Convert.ToInt32(height)}\nPartThick = {40}, PartMat = {Convert.ToInt32(materialP2[0])}, PartMatThick = {Convert.ToDouble(materialP2[1].Replace('.', ','))}\nReinforcing = {усиление}, Ral = {покрытие[3]}, CoatingType = {покрытие[4]}, CoatingClass = {Convert.ToInt32(покрытие[5])}\nMirror = {config.Contains("01")}, Step = {(needToAddStepInsertionAndStep ? расположениеПанелей : null)}, StickyTape = {скотч}\nStepInsertion = {(needToAddStepInsertionAndStep ? расположениеВставок : null)}, AirHole = {типТорцевой}";

            // Сшитые панели Внешняя тип + первая/вторая = 11, 12 или 21, 22 тоже и с нижней

            var имяДвойнойВерхней1 = "";
            var имяДвойнойВерхней2 = "";
            var имяДвойнойНижней1 = "";
            var имяДвойнойНижней2 = "";
            
            AddingPanel панельВнешняяДвойная1 = null;
            AddingPanel панельВнешняяДвойная2 = null;
            AddingPanel панельВнутренняяДвойная1 = null;
            AddingPanel панельВнутренняяДвойная2 = null;

            if (типДвойной != "00")
            {
                панельВнешняяДвойная1 =
                    new AddingPanel
                    {
                        PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                        ElementType = Convert.ToInt32(типДвойной.Remove(1, 1) + "1"),
                        Width = Convert.ToInt32(width),
                        Height = Convert.ToInt32(height),
                        PartThick = 40,
                        PartMat = Convert.ToInt32(materialP1[0]),
                        PartMatThick = Convert.ToDouble(materialP1[1].Replace('.', ',')),
                        Reinforcing = усиление,
                        Ral = покрытие[0],
                        CoatingType = покрытие[1],
                        CoatingClass = Convert.ToInt32(покрытие[2]),
                        Mirror = config.Contains("01"),
                        StickyTape = скотч,//.Contains("Со скотчем"),
                        StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,
                        AirHole = типТорцевой
                    };
                id = панельВнешняяДвойная1.AddPart();

                partIds.Add(new KeyValuePair<int, int>(Convert.ToInt32(типДвойной.Remove(1, 1) + "1"), id));
                панельВнешняяДвойная1.NewName = "02-" + pType + "-1-" + id;
                имяДвойнойВерхней1 = панельВнешняяДвойная1.NewName;

                панельВнешняяДвойная1.PartQuery =
                $"PanelTypeId = {Convert.ToInt32(typeOfPanel[2])}, ElementType = {Convert.ToInt32(типДвойной.Remove(1, 1) + "1")}\nWidth = {Convert.ToInt32(width)}, Height = {Convert.ToInt32(height)},\nPartThick = 40, PartMat = {Convert.ToInt32(materialP1[0])}, PartMatThick = {Convert.ToDouble(materialP1[1].Replace('.', ','))}\nReinforcing = {усиление}, Ral = {покрытие[0]}, CoatingClass = {Convert.ToInt32(покрытие[2])}\nMirror = {config.Contains("01")}, StickyTape = {скотч}\nStepInsertion = {(needToAddStepInsertionAndStep ? расположениеВставок : null)}, AirHole = {типТорцевой}";

                панельВнешняяДвойная2 =
                    new AddingPanel
                    {
                        PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                        ElementType = Convert.ToInt32(типДвойной.Remove(1, 1) + "2"),
                        Width = Convert.ToInt32(width),
                        Height = Convert.ToInt32(height),
                        PartThick = 40,
                        PartMat = Convert.ToInt32(materialP1[0]),
                        PartMatThick = Convert.ToDouble(materialP1[1].Replace('.', ',')),
                        Reinforcing = усиление,
                        Ral = покрытие[0],
                        CoatingType = покрытие[1],
                        CoatingClass = Convert.ToInt32(покрытие[2]),
                        Mirror = config.Contains("01"),
                        StickyTape = скотч,//.Contains("Со скотчем"),
                        StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,
                        AirHole = типТорцевой
                    };
                id = панельВнешняяДвойная2.AddPart();
                partIds.Add(new KeyValuePair<int, int>(Convert.ToInt32(типДвойной.Remove(1, 1) + "2"), id));
                панельВнешняяДвойная2.NewName = "02-" + pType + "-1-" + id;
                имяДвойнойВерхней2 = панельВнешняяДвойная2.NewName;
                панельВнешняяДвойная2.PartQuery =
                    $"PanelTypeId = {Convert.ToInt32(typeOfPanel[2])}, ElementType = {Convert.ToInt32(типДвойной.Remove(1, 1) + "2")}\nWidth = {Convert.ToInt32(width)}, Height = {Convert.ToInt32(height)},\nPartThick = 40, PartMat = {Convert.ToInt32(materialP1[0])}, PartMatThick = {Convert.ToDouble(materialP1[1].Replace('.', ','))}\nReinforcing = {усиление}, Ral = {покрытие[0]}, CoatingClass = {Convert.ToInt32(покрытие[2])}\nMirror = {config.Contains("01")} StickyTape = {скотч}\nStepInsertion = {(needToAddStepInsertionAndStep ? расположениеВставок : null)}, AirHole = {типТорцевой}";


                if (типДвойной.Remove(0, 1) != "0")
                {
                    панельВнутренняяДвойная1 =
                        new AddingPanel
                        {
                            PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                            ElementType = Convert.ToInt32(типДвойной.Remove(0, 1) + "1"),
                            Width = Convert.ToInt32(width),
                            Height = Convert.ToInt32(height),
                            PartThick = 40,
                            PartMat = Convert.ToInt32(materialP2[0]),
                            PartMatThick = Convert.ToDouble(materialP2[1].Replace('.', ',')),
                            Reinforcing = усиление,
                            Ral = покрытие[3],
                            CoatingType = покрытие[4],
                            CoatingClass = Convert.ToInt32(покрытие[5]),
                            Mirror = config.Contains("01"),
                            Step = needToAddStepInsertionAndStep ? расположениеПанелей : null,
                            StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,
                            AirHole = типТорцевой
                        };
                    id = панельВнутренняяДвойная1.AddPart();
                    partIds.Add(new KeyValuePair<int, int>(Convert.ToInt32(типДвойной.Remove(0, 1) + "1"), id));
                    панельВнутренняяДвойная1.NewName = "02-" + pType + "-2-" + id;
                    имяДвойнойНижней1 = панельВнутренняяДвойная1.NewName;

                    панельВнутренняяДвойная2 =
                        new AddingPanel
                        {
                            PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                            ElementType = Convert.ToInt32(типДвойной.Remove(0, 1) + "2"),
                            Width = Convert.ToInt32(width),
                            Height = Convert.ToInt32(height),
                            PartThick = 40,
                            PartMat = Convert.ToInt32(materialP2[0]),
                            PartMatThick = Convert.ToDouble(materialP2[1].Replace('.', ',')),
                            Reinforcing = усиление,
                            Ral = покрытие[3],
                            CoatingType = покрытие[4],
                            CoatingClass = Convert.ToInt32(покрытие[5]),
                            Mirror = config.Contains("01"),
                            Step = needToAddStepInsertionAndStep ? расположениеПанелей : null,
                            StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,
                            AirHole = типТорцевой
                        };
                    id = панельВнутренняяДвойная2.AddPart();
                    partIds.Add(new KeyValuePair<int, int>(Convert.ToInt32(типДвойной.Remove(0, 1) + "2"), id));
                    панельВнутренняяДвойная2.NewName = "02-" + pType + "-2-" + id;
                    имяДвойнойНижней2 = панельВнутренняяДвойная2.NewName;
                }
            }

            #endregion

            #region теплоизоляция, cкотч, усиливающаяРамкаПоШирине, усиливающаяРамкаПоВысоте

            var теплоизоляция =
                new AddingPanel
                {
                    PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                    ElementType = 3,
                    Width = Convert.ToInt32(width),
                    Height = Convert.ToInt32(height),
                    PartThick = 40,
                    PartMat = 4900,

                    PartMatThick = 1,
                    Ral = "Без покрытия",
                    CoatingType = "0",
                    CoatingClass = Convert.ToInt32("0"),
                    AirHole = типТорцевой
                };
            id = теплоизоляция.AddPart();
            partIds.Add(new KeyValuePair<int, int>(3, id));
            теплоизоляция.NewName = "02-" + id;
            теплоизоляция.PartQuery =
                $"PanelTypeId = {Convert.ToInt32(typeOfPanel[2])}, ElementType = 3\nWidth = {Convert.ToInt32(width)}, Height = {Convert.ToInt32(height)}\nPartThick = 40, PartMat = 4900, PartMatThick = 1\nRal = Без покрытия, CoatingType = 0, CoatingClass = {Convert.ToInt32("0")}\nAirHole = {типТорцевой}";

            AddingPanel cкотч = null;

            if (скотч)//.Contains("Со скотчем"))
            {
                cкотч =
                    new AddingPanel
                    {
                        PanelTypeId = 14,
                        ElementType = 4,
                        Width = Convert.ToInt32(width),
                        Height = Convert.ToInt32(height),
                        PartThick = 40,
                        PartMat = 14800,

                        PartMatThick = 1,
                        Ral = "Без покрытия",
                        CoatingType = "0",
                        CoatingClass = Convert.ToInt32("0")
                    };
                id = cкотч.AddPart();
                partIds.Add(new KeyValuePair<int, int>(4, id));
                cкотч.NewName = "02-" + id;
                cкотч.PartQuery =
                    $"PanelTypeId = 14, ElementType = 4\nWidth = {Convert.ToInt32(width)}, Height = {Convert.ToInt32(height)}\nPartThick = 40, PartMat = 14800, PartMatThick = 1, Ral = Без покрытия\nCoatingType = 0, CoatingClass = {Convert.ToInt32("0")}";
            }

            var pes =
                new AddingPanel
                {
                    PanelTypeId = 15,
                    ElementType = 5,
                    Width = Convert.ToInt32(width),
                    Height = Convert.ToInt32(height),
                    PartThick = 40,
                    PartMat = 6700,

                    PartMatThick = 1,
                    Ral = "Без покрытия",
                    CoatingType = "0",
                    CoatingClass = Convert.ToInt32("0")
                };
            id = pes.AddPart();
            partIds.Add(new KeyValuePair<int, int>(5, id));
            pes.NewName = "02-" + id;
            pes.PartQuery =
                $"PanelTypeId = 15, ElementType = 5\nWidth = {Convert.ToInt32(width)}, Height = {Convert.ToInt32(height)}\nPartThick = 40, PartMat = 6700, PartMatThick = 1\nRal = Без покрытия, CoatingType = 0\nCoatingClass = {Convert.ToInt32("0")}";

            AddingPanel усиливающаяРамкаПоШирине = null;
            AddingPanel усиливающаяРамкаПоШирине2 = null;
            AddingPanel усиливающаяРамкаПоВысоте = null;

            if (усиление)
            {
                усиливающаяРамкаПоШирине =
                    new AddingPanel
                    {
                        PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                        ElementType = 6,
                        Height = 40,
                        Width = Convert.ToInt32(width),
                        PartThick = 40,
                        PartMat = 1800,
                        PartMatThick = Convert.ToDouble("1".Replace('.', ',')),
                        Mirror = config.Contains("01"),
                        Step = needToAddStepInsertionAndStep ? расположениеПанелей : null,
                        StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,

                        Ral = "Без покрытия",
                        CoatingType = "0",
                        CoatingClass = Convert.ToInt32("0")
                    };
                id = усиливающаяРамкаПоШирине.AddPart();
                partIds.Add(new KeyValuePair<int, int>(6, id));
                усиливающаяРамкаПоШирине.NewName = "02-" + id;

                if (pType != "01")
                {
                    усиливающаяРамкаПоШирине2 =
                        new AddingPanel
                        {
                            PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                            ElementType = 62,
                            Height = 40,
                            Width = Convert.ToInt32(width),
                            PartThick = 40,
                            PartMat = 1800,
                            PartMatThick = Convert.ToDouble("1".Replace('.', ',')),
                            Mirror = config.Contains("01"),
                            Step = needToAddStepInsertionAndStep ? расположениеПанелей : null,
                            StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,

                            Ral = "Без покрытия",
                            CoatingType = "0",
                            CoatingClass = Convert.ToInt32("0")
                        };
                    id = усиливающаяРамкаПоШирине2.AddPart();
                    partIds.Add(new KeyValuePair<int, int>(62, id));
                    усиливающаяРамкаПоШирине2.NewName = "02-" + id;
                }

                усиливающаяРамкаПоВысоте =
                    new AddingPanel
                    {
                        PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                        ElementType = 7,
                        Height = Convert.ToInt32(height),
                        Width = 40,
                        PartThick = 40,
                        PartMat = 1800,
                        PartMatThick = Convert.ToDouble("1".Replace('.', ',')),
                        Mirror = config.Contains("01"),
                        Step = needToAddStepInsertionAndStep ? расположениеПанелей : null,
                        StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,

                        Ral = "Без покрытия",
                        CoatingType = "0",
                        CoatingClass = Convert.ToInt32("0")
                    };
                id = усиливающаяРамкаПоВысоте.AddPart();
                partIds.Add(new KeyValuePair<int, int>(7, id));
                усиливающаяРамкаПоВысоте.NewName = "02-" + id;
            }

            AddingPanel кронштейнДверной = null;
            
            if (усилисвающя)
            {
                if (типУсиливающей.Remove(0,1).Contains("D"))
                {
                    //MessageBox.Show(Усиливающая(pType) + "\n" + типУсиливающей.Remove(0, 1).Contains("D"));
                    кронштейнДверной = new AddingPanel
                    {
                        PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                        ElementType = 9,
                        Height = Convert.ToInt32(height),
                        Width = 20,
                        PartThick = 40,
                        PartMat = 1800,
                        PartMatThick = Convert.ToDouble("1".Replace('.', ',')),
                        Mirror = config.Contains("01"),

                        Ral = "Без покрытия",
                        CoatingType = "0",
                        CoatingClass = Convert.ToInt32("0")
                    };
                    id = кронштейнДверной.AddPart();
                    partIds.Add(new KeyValuePair<int, int>(9, id));
                    кронштейнДверной.NewName = "02-" + id;
                    кронштейнДверной.PartQuery = $"PanelTypeId = {Convert.ToInt32(typeOfPanel[2])}, ElementType = 9\nHeight = {Convert.ToInt32(height)}, Width = 20\nPartThick = 40, PartMat = 1800\nPartMatThick = {Convert.ToDouble("1".Replace('.', ','))}, Mirror = {config.Contains("01")}\n,Ral = Без покрытия, CoatingType = 0\nCoatingClass = {Convert.ToInt32("0")}";                }
            }

            AddingPanel профильТорцевойРамкиВертикальный = null;
            AddingPanel профильТорцевойРамкиГоризонтальный = null;

            if (типТорцевой != null)
            {
                профильТорцевойРамкиВертикальный = new AddingPanel
                {
                    PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                    ElementType = 11,
                    Height = (int)HeightOfWindow,
                    Width = 40,
                    PartThick = 40,
                    PartMat = 1800,
                    PartMatThick = Convert.ToDouble("1".Replace('.', ',')),
                    Mirror = config.Contains("01"),

                    Ral = "Без покрытия",
                    CoatingType = "0",
                    CoatingClass = Convert.ToInt32("0"),

                    AirHole = типТорцевой
                };
                id = профильТорцевойРамкиВертикальный.AddPart();
                partIds.Add(new KeyValuePair<int, int>(11, id));
                профильТорцевойРамкиВертикальный.NewName = "02-" + id;


                профильТорцевойРамкиГоризонтальный = new AddingPanel
                {
                    PanelTypeId = Convert.ToInt32(typeOfPanel[2]),
                    ElementType = 12,
                    Height = (int)WidthOfWindow,//BackProfils.Height,
                    Width = 40,
                    PartThick = 40,
                    PartMat = 1800,
                    PartMatThick = Convert.ToDouble("1".Replace('.', ',')),
                    Mirror = config.Contains("01"),

                    Ral = "Без покрытия",
                    CoatingType = "0",
                    CoatingClass = Convert.ToInt32("0"),

                    AirHole = типТорцевой
                };
                id = профильТорцевойРамкиГоризонтальный.AddPart();
                partIds.Add(new KeyValuePair<int, int>(12, id));
                профильТорцевойРамкиГоризонтальный.NewName = "02-" + id;
            }

            #endregion

            #region Сборка панели


            var iDs = "";

            var idAsm = 0;
            foreach (var сборка in partIds.Select(partId => new AddingPanel
            {
                PartId = partId.Value,

                PanelTypeId = Convert.ToInt32(typeOfPanel[2]),

                ElementType = partId.Key,

                Width = Convert.ToInt32(width),
                Height = Convert.ToInt32(height),

                PanelMatOut = Convert.ToInt32(materialP1[0]),
                PanelMatIn = Convert.ToInt32(materialP2[0]),
                PanelThick = 40,
                PanelMatThickOut = Convert.ToDouble(materialP1[1].Replace('.', ',')),
                PanelMatThickIn = Convert.ToDouble(materialP2[1].Replace('.', ',')),
                RalOut = покрытие[0],
                RalIn = покрытие[0],
                CoatingTypeOut = покрытие[1],
                CoatingTypeIn = покрытие[1],
                CoatingClassOut = Convert.ToInt32(покрытие[2]),
                CoatingClassIn = Convert.ToInt32(покрытие[2]),

                Mirror = config.Contains("01"),
                Step = needToAddStepInsertionAndStep ? расположениеПанелей : null,
                StepInsertion = needToAddStepInsertionAndStep ? расположениеВставок : null,
                Reinforcing = усиление,
                StickyTape = скотч,//.Contains("Со скотчем"),
                
                AirHole = типТорцевой,

                PanelNumber = newId
            }))
            {
                idAsm = сборка.Add();
                iDs = iDs + "\n" + idAsm;
            }
            
            //MessageBox.Show(iDs);
            //return null;

            #endregion
            

            var обозначениеНовойПанели = "02-" + typeOfPanel[0] + "-" + idAsm;

            existingAsmsAndParts.AddRange(new List<ExistingAsmsAndParts>
                {
                    new ExistingAsmsAndParts
                    {
                        PartName = обозначениеНовойПанели,
                        Comment = "Сборка панели",
                        IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1],
                        PartQuery = $@"PanelTypeId = {Convert.ToInt32(typeOfPanel[2])}
                        Width = {Convert.ToInt32(width)} Height = {Convert.ToInt32(height)}" +

                        #region to delete
                        //PanelMatOut = {Convert.ToInt32(materialP1[0])}
                        //PanelMatIn = {Convert.ToInt32(materialP2[0])}
                        //PanelMatThickOut = {Convert.ToDouble(materialP1[1].Replace('.', ','))}
                        //PanelMatThickIn = {Convert.ToDouble(materialP2[1].Replace('.', ','))}
                        //RalOut = {покрытие[0]}
                        //RalIn = {покрытие[0]}
                        //CoatingTypeOut = {покрытие[1]}
                        //CoatingTypeIn = {покрытие[1]}
                        //CoatingClassOut = {Convert.ToInt32(покрытие[2])}
                        //CoatingClassIn = {Convert.ToInt32(покрытие[2])}
                        #endregion

                        $@" Mirror = {config.Contains("01")}
                        Step = {(needToAddStepInsertionAndStep ? расположениеПанелей : null)}
                        StepInsertion = {(needToAddStepInsertionAndStep ? расположениеВставок : null)}
                        Reinforcing = {усиление}
                        StickyTape = {скотч}
                        AirHole = {типТорцевой}
                        PanelNumber = {newId}"
                    }
                });


            if (типДвойной == "00")
            {
                existingAsmsAndParts.AddRange(new List<ExistingAsmsAndParts>
                {
                    new ExistingAsmsAndParts
                    {
                        PartName = панельВнешняя.NewName,
                        Comment = "Панель Внешняя",
                        IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1],
                        PartQuery = панельВнешняя.PartQuery
                    }
                });
            }
            else
            {
                existingAsmsAndParts.AddRange(new List<ExistingAsmsAndParts>
                {
                    new ExistingAsmsAndParts
                    {
                        PartName = панельВнешняяДвойная1.NewName,
                        Comment = "Панель Внутренняя 1",
                        IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1],
                        PartQuery = панельВнешняяДвойная1.PartQuery
                    },
                    new ExistingAsmsAndParts
                    {
                        PartName = панельВнешняяДвойная2.NewName,
                        Comment = "Панель Внутренняя 2",
                        IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1],
                        PartQuery = панельВнешняяДвойная2.PartQuery
                    }
                });
            }

            if (типДвойной == "00")
            {
                existingAsmsAndParts.AddRange(new List<ExistingAsmsAndParts>
                {
                    new ExistingAsmsAndParts
                    {
                        PartName = панельВнутренняя.NewName,
                        Comment = "Панель Внутренняя",
                        IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1],
                        PartQuery = панельВнутренняя.PartQuery
                    }
                });
            }
            else
            {
                existingAsmsAndParts.AddRange(new List<ExistingAsmsAndParts>
                {
                    new ExistingAsmsAndParts
                    {
                        PartName = панельВнутренняяДвойная1.NewName,
                        Comment = "Панель Внутренняя 1",
                        IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1],
                        PartQuery = панельВнутренняяДвойная1.PartQuery
                    },
                    new ExistingAsmsAndParts
                    {
                        PartName = панельВнутренняяДвойная2.NewName,
                        Comment = "Панель Внутренняя 2",
                        IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1],
                        PartQuery = панельВнутренняяДвойная2.PartQuery
                    }
                });
            }

            if (кронштейнДверной != null)
            {
                existingAsmsAndParts.Add(new ExistingAsmsAndParts
                {
                    PartName = кронштейнДверной.NewName,
                    Comment = "Кронштейн дверной",
                    IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1],
                    PartQuery = кронштейнДверной.PartQuery
                });
            }
            if (cкотч != null)
            {
                existingAsmsAndParts.Add(new ExistingAsmsAndParts
                {
                    PartName = cкотч?.NewName,
                    Comment = "Скотч",
                    IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1],
                    PartQuery = cкотч?.PartQuery
                });
            }
            if (усиливающаяРамкаПоВысоте != null)
            {
                existingAsmsAndParts.Add(new ExistingAsmsAndParts
                {
                    PartName = усиливающаяРамкаПоВысоте?.NewName,
                    Comment = "Усиливающая рамка по высоте",
                    IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1]
                });
            }
            if (усиливающаяРамкаПоШирине != null)
            {
                existingAsmsAndParts.Add(new ExistingAsmsAndParts
                {
                    PartName = усиливающаяРамкаПоШирине?.NewName,
                    Comment = "Усиливающая рамка по ширине",
                    IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1]
                });
            }
            if (профильТорцевойРамкиВертикальный != null)
            {
                existingAsmsAndParts.Add(new ExistingAsmsAndParts
                {
                    PartName = профильТорцевойРамкиВертикальный?.NewName,
                    Comment = "Профиль торцевой рамки вертикальный",
                    IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1]
                });
            }
            if (профильТорцевойРамкиГоризонтальный != null)
            {
                existingAsmsAndParts.Add(new ExistingAsmsAndParts
                {
                    PartName = профильТорцевойРамкиГоризонтальный?.NewName,
                    Comment = "Профиль торцевой рамки горизонтальный",
                    IdAsm = обозначениеНовойПанели + " - " + typeOfPanel[1]
                });
            }
            
            if (onlySearch) return null;

            #endregion

            #endregion

            string[] frameProfils = null;

            if (типТорцевой != null)
            {
                try
                {
                    frameProfils = new[]
                    {
                        FrameProfil(Convert.ToDouble(HeightOfWindow) + 60, скотч,//.Contains("Со скотчем"),
                        "00",
                            BackProfils.Flange30, профильТорцевойРамкиВертикальный.NewName),
                        FrameProfil(Convert.ToDouble(WidthOfWindow) + 0, скотч,//.Contains("Со скотчем"), 
                        "01",
                            BackProfils.Flange30, профильТорцевойРамкиГоризонтальный.NewName)
                    };
                }
                catch (Exception)
                {
                    //
                }
            }

            switch (pType)
            {
                case "04":
                    _destinationFolder = Panels0204;
                    break;
                case "05":
                    _destinationFolder = Panels0205;
                    break;
                default:
                    _destinationFolder = Panels0201;
                    break;
            }
            
            var newFramelessPanelPath =
                $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{обозначениеНовойПанели}.SLDASM";

            if (!InitializeSw(true)) return "-";
            
            if (GetExistingFile(Path.GetFileNameWithoutExtension(newFramelessPanelPath), 0))
            {
                GetLastVersionPdm(new FileInfo(newFramelessPanelPath).FullName, Settings.Default.TestPdmBaseName);
                _swApp.OpenDoc6(newFramelessPanelPath, (int)swDocumentTypes_e.swDocASSEMBLY,
                    (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                return newFramelessPanelPath;
            }
                        
            const string modelPanelsPath = FrameLessFolder;
            var sourceFolder = Settings.Default.SourceFolder;
            var nameAsm = "02-11-40-1";

            var nameUpPanel = "02-11-01-40-";
            var nameDownPanel = "02-11-02-40-";

            #region Двойная

            var типДвойнойВерхней = "0";
            var типДвойнойНижней = "0";
            string типДвойнойРазрез = null;

            if (типДвойной != "00")
            {
                nameAsm = "02-11-40-2";

                nameUpPanel = "02-11-01-40-2-";
                nameDownPanel = "02-11-02-40-2-";

                типДвойнойВерхней = типДвойной.Remove(1, 1);
                типДвойнойНижней = типДвойной.Remove(0, 1);

                if (типДвойной.Contains("1"))
                {
                    типДвойнойРазрез = "W";
                }
                if (типДвойной.Contains("2"))
                {
                    типДвойнойРазрез = "H";
                }
            }
            
            #endregion

            var modelPanelAsmbly = new FileInfo($@"{sourceFolder}{modelPanelsPath}\{nameAsm}.SLDASM").FullName;

            #endregion

            #region Получение последней версии и открытие сборки шаблонной модели

            GetLatestVersionAsmPdm(modelPanelAsmbly, Settings.Default.PdmBaseName);
            
            var swDoc = _swApp.OpenDoc6(modelPanelAsmbly, (int)swDocumentTypes_e.swDocASSEMBLY,
               (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            _swApp.Visible = true;
                
            var swAsm = (AssemblyDoc)swDoc;
            swAsm.ResolveAllLightWeightComponents(false);
            
            #endregion

            #region Расчетные данные, переменные и константы

            #region Габариты

            var ширинаПанели = Convert.ToDouble(width);
            var высотаПанели = Convert.ToDouble(height);

            double количествоВинтов = 2000;

            switch (pType)
            {
                case "04":
                case "05":
                    double расстояниеL;
                    СъемнаяПанель(ширинаПанели, высотаПанели, out ширинаПанели, out высотаПанели, out расстояниеL, out количествоВинтов);
                    break;
            }

            #endregion

            #region Расчет шага для саморезов и заклепок

            double шагСаморезВинтШирина = 200;
            var шагСаморезВинтВысота = шагСаморезВинтШирина;

            if (ширинаПанели < 600)
            {
                шагСаморезВинтШирина = 150;
            }
            if (высотаПанели < 600)
            {
                шагСаморезВинтВысота = 150;
            }

            var колСаморезВинтШирина = (Math.Truncate(ширинаПанели / шагСаморезВинтШирина) + 1) * 1000;
            
            var колСаморезВинтВысота = (Math.Truncate(высотаПанели / шагСаморезВинтВысота) + 1) * 1000;

            if (screws?.ByHeightInnerUp > 1000)
            {
                колСаморезВинтВысота = screws.ByHeightInnerUp;
            }

            var колСаморезВинтШирина2 = колСаморезВинтШирина;

            switch (pType)
            {
                case "04":
                case "05":
                    колСаморезВинтШирина = количествоВинтов;
                    break;
            }

            // Шаг заклепок

            double колЗаклепокВысота;

            switch (pType)
            {
                case "04":
                case "05":
                    const double шагЗаклепокВысота = 125;
                    колЗаклепокВысота = (Math.Truncate(высотаПанели / шагЗаклепокВысота) + 1) * 1000;
                    break;
                default:
                    колЗаклепокВысота = колСаморезВинтВысота + 1000;
                    if (Convert.ToInt32(height) > 1000)
                    {
                        колЗаклепокВысота = колСаморезВинтВысота + 3000;
                    }
                    break;
            }

            var колЗаклепокШирина = колСаморезВинтШирина + 1000;

            #endregion

            #region Оступы для отверстий заклепок, саморезов и винтов

            var отступОтветныхОтверстийШирина = 8;
            var осьСаморезВинт = 9.0;
            var осьОтверстийСаморезВинт = pType == "04" || pType == "05" ? 12.0 : 11.0;
            var осьПоперечныеОтверстия = 10.1; 

            if (скотч)
            {
                осьПоперечныеОтверстия =  10.1;
            }

            switch (pType)
            {
                case "04":
                case "05": break;
                default:
                    отступОтветныхОтверстийШирина = 47;
                    осьСаморезВинт = 9.70;
                    осьОтверстийСаморезВинт = 10.3;
                    break;
            }

            #endregion

            #region Коэффициенты и радиусы гибов

            var sbSqlBaseData = new SqlBaseData();

            #endregion

            #region Диаметры отверстий

            var диамЗаглушкаВинт = 13.1;
            var диамСаморезВинт = 3.3;

            switch (pType)
            {
                case "04":
                case "05":
                    диамЗаглушкаВинт = 11;
                    диамСаморезВинт = 7;
                    break;
            }

            #endregion

            #region Расчет расстояния межу ручками в съемной панели

            var растояниеМеждуРучками = ширинаПанели / 4;

            if (ширинаПанели < 1000)
            {
                растояниеМеждуРучками = ширинаПанели * 0.5 * 0.5;
            }
            if (ширинаПанели >= 1000)
            {
                растояниеМеждуРучками = ширинаПанели * 0.45 * 0.5;
            }
            if (ширинаПанели >= 1300)
            {
                растояниеМеждуРучками = ширинаПанели * 0.4 * 0.5;
            }
            if (ширинаПанели >= 1700)
            {
                растояниеМеждуРучками = ширинаПанели * 0.35 * 0.5;
            }

            #endregion

            #endregion

            #region Типы панелей - Удаление ненужного

            const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                                (int)swDeleteSelectionOptions_e.swDelete_Children;
            var swDocExt = swDoc.Extension;

            var bodyfeat = VentsCad.CompType.BODYFEATURE;
            var sketch = VentsCad.CompType.SKETCH;


            //var ftrfolder = VentsCad.CompType.FTRFOLDER;
            //var dimension = VentsCad.CompType.DIMENSION;           

            //var HeightOfWindow = BackProfils.Flange30 ?
            //        BackProfils.ByHeight / 1000 : (BackProfils.ByHeight + 2) / 1000;

            //MessageBox.Show(HeightOfWindow.ToString());



            var supress = VentsCad.Act.Suppress;
            var unSupress = VentsCad.Act.Unsuppress;
            var doNothing = VentsCad.Act.DoNothing;

            if (string.IsNullOrEmpty(типТорцевой)) 
            {
                swDocExt.SelectByID2("Рамка", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();

                foreach (var component in new List<string>
                {
                    "02-11-11-40--1", "02-11-11-40--2", "02-11-11-40--3","02-11-11-40--4",
                    "Threaded Rivets Increased-1", "Threaded Rivets Increased-2", "Threaded Rivets Increased-3", "Threaded Rivets Increased-4",
                    "Rivet Bralo-71", "Rivet Bralo-72", "Rivet Bralo-73", "Rivet Bralo-74", "Rivet Bralo-75", "Rivet Bralo-76",
                    "Rivet Bralo-83", "Rivet Bralo-84", "Rivet Bralo-91", "Rivet Bralo-92", "Rivet Bralo-93", "Rivet Bralo-94"
                })
                {
                    swDocExt.SelectByID2(component + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                }

                swDoc.Extension.SelectByID2("Вырез-Вытянуть21@"+ nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);

                swDoc.Extension.SelectByID2("Вырез-Вытянуть22@"+ nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);

                swDoc.Extension.SelectByID2("Вырез-Вытянуть1@02-11-03-40--1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);

                swDocExt.SelectByID2("Рамка@" + nameUpPanel + "-1@" + nameAsm, "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Рамка@" + nameDownPanel + "-1@" + nameAsm, "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Рамка@02-11-03-40--1@" + nameAsm, "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                swDoc.EditRebuild3();

                swDoc.Extension.SelectByID2("Вырез-Вытянуть26@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);
                swDocExt.SelectByID2("Hole72", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }
            else
            {
                swDocExt.SelectByID2("Threaded Rivets-38@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                if (config.Contains("01"))
                {
                    swDocExt.SelectByID2("Вырез-Вытянуть25@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                    swDoc.EditRebuild3();
                }
                else if (config.Contains("02"))
                {
                    swDocExt.SelectByID2("Вырез-Вытянуть25@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                    swDocExt.SelectByID2("Кривая3@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                    swDocExt.SelectByID2("Вырез-Вытянуть18@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                }

                swDocExt.SelectByID2("Hole72", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                swDoc.Extension.SelectByID2("Вырез-Вытянуть26@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                swDoc.Extension.SelectByID2("Кривая6@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();


                swDoc.Extension.SelectByID2("D1@Кривая6@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Кривая6@" + nameUpPanel + ".Part"))).SystemValue = колСаморезВинтВысота / 1000; swDoc.EditRebuild3();

                swDoc.Extension.SelectByID2("D2@Эскиз74@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D2@Эскиз74@" + nameUpPanel + ".Part"))).SystemValue = 0.015;
                swDoc.EditRebuild3();

                swDocExt.SelectByID2("Threaded Rivets-37@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                swDoc.Extension.SelectByID2("D2@Эскиз68@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D2@Эскиз68@" + nameUpPanel + ".Part"))).SystemValue = HeightOfWindow / 1000;

                swDoc.Extension.SelectByID2("D1@Эскиз68@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Эскиз68@" + nameUpPanel + ".Part"))).SystemValue = WidthOfWindow / 1000;


                swDoc.Extension.SelectByID2("D3@Эскиз68@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D3@Эскиз68@" + nameUpPanel + ".Part"))).SystemValue = BackProfils.ByWidth / 1000;

                


                //var additionToWindow = BackProfils.Flange30 ?
                ////    BackProfils.ByHeight / 1000 :  (BackProfils.ByHeight + 2) / 1000; 


                swDoc.Extension.SelectByID2("D4@Эскиз68@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D4@Эскиз68@" + nameUpPanel + ".Part"))).SystemValue = BackProfils.ByHeight  / 1000;

                swDoc.Extension.SelectByID2("D1@Эскиз72@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Эскиз72@" + nameUpPanel + ".Part"))).SystemValue = (!BackProfils.Flange30) ? 0.01 : 0.02;
                swDoc.EditRebuild3();

                swDoc.Extension.SelectByID2("D3@Эскиз72@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D3@Эскиз72@" + nameUpPanel + ".Part"))).SystemValue = (!BackProfils.Flange30) ? 0.01 : 0.015;
                swDoc.EditRebuild3();

                var zaklWidth = Convert.ToInt32(Math.Truncate(HeightOfWindow / 100));
                swDoc.Extension.SelectByID2("D1@Кривая4@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Кривая4@" + nameUpPanel + ".Part"))).SystemValue = zaklWidth == 1 ? 2 : zaklWidth;


                var zaklHeight = Convert.ToInt32(Math.Truncate(BackProfils.Height / 100));
                swDoc.Extension.SelectByID2("D1@Кривая5@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Кривая5@" + nameUpPanel + ".Part"))).SystemValue = zaklHeight == 1 ? 2 : zaklHeight;
                swDoc.EditRebuild3();


                swDoc.Extension.SelectByID2("D3@Эскиз95@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D3@Эскиз95@" + nameDownPanel + ".Part"))).SystemValue = HeightOfWindow / 1000;

                swDoc.Extension.SelectByID2("D4@Эскиз95@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D4@Эскиз95@" + nameDownPanel + ".Part"))).SystemValue = WidthOfWindow / 1000;

                swDoc.Extension.SelectByID2("D2@Эскиз95@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D2@Эскиз95@" + nameDownPanel + ".Part"))).SystemValue = BackProfils.ByHeight / 1000;

                swDoc.Extension.SelectByID2("D1@Эскиз95@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Эскиз95@" + nameDownPanel + ".Part"))).SystemValue = BackProfils.ByWidth / 1000;

                swDoc.Extension.SelectByID2("D1@Эскиз99@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Эскиз99@" + nameDownPanel + ".Part"))).SystemValue = (!BackProfils.Flange30) ? 0.01 : 0.02;
                swDoc.EditRebuild3();
                swDoc.Extension.SelectByID2("D3@Эскиз99@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D3@Эскиз99@" + nameDownPanel + ".Part"))).SystemValue = (!BackProfils.Flange30) ? 0.01 : 0.015;
                swDoc.EditRebuild3();


                swDoc.Extension.SelectByID2("D1@Кривая15@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Кривая15@" + nameDownPanel + ".Part"))).SystemValue = zaklWidth == 1 ? 2 : zaklWidth;

                swDoc.Extension.SelectByID2("D1@Кривая16@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Кривая16@" + nameDownPanel + ".Part"))).SystemValue = zaklHeight == 1 ? 2 : zaklHeight;
                swDoc.EditRebuild3();


                swDoc.Extension.SelectByID2("D5@Эскиз3@02-11-03-40--1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D5@Эскиз3@02-11-03-40-.Part"))).SystemValue = HeightOfWindow / 1000;

                swDoc.Extension.SelectByID2("D4@Эскиз3@02-11-03-40--1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D4@Эскиз3@02-11-03-40-.Part"))).SystemValue = WidthOfWindow / 1000;

                swDoc.Extension.SelectByID2("D2@Эскиз3@02-11-03-40--1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D2@Эскиз3@02-11-03-40-.Part"))).SystemValue = BackProfils.ByHeight / 1000;

                swDoc.Extension.SelectByID2("D3@Эскиз3@02-11-03-40--1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D3@Эскиз3@02-11-03-40-.Part"))).SystemValue = BackProfils.ByWidth / 1000;
                swDoc.EditRebuild3();

                if (pType == "01")
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть26@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);
                    swDocExt.SelectByID2("Hole72", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
                
                //***********************************************//

                if (!string.IsNullOrEmpty(frameProfils[0]))
                {
                    try
                    {
                        swDoc.Extension.SelectByID2("02-11-11-40--1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(frameProfils[0], "", false, true);
                    }
                    catch (Exception)
                    {
                        //
                    }
                }

                try
                {
                    swDoc.Extension.SelectByID2("02-11-11-40--2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(frameProfils[0], "", false, true);
                }
                catch (Exception)
                {
                    //
                }

                if (!string.IsNullOrEmpty(frameProfils[1]))
                {
                    try
                    {
                        swDoc.Extension.SelectByID2("02-11-11-40--3@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(frameProfils[1], "", false, true);
                    }
                    catch (Exception)
                    {
                        //
                    }
                    try
                    {
                        swDoc.Extension.SelectByID2("02-11-11-40--4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(frameProfils[1], "", false, true);
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            }

            switch (pType)
            {
                #region 04 05 - Съемные панели

                case "04":
                case "05":

                    if (Convert.ToInt32(width) > 750)
                    {
                        swDocExt.SelectByID2("Handel-1", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditDelete();
                        swDocExt.SelectByID2("Threaded Rivets-25@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDocExt.SelectByID2("Ручка MLA 120-3@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDocExt.SelectByID2("SC GOST 17475_gost-5@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDocExt.SelectByID2("SC GOST 17475_gost-6@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDocExt.SelectByID2("Threaded Rivets-26@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDocExt.SelectByID2("Threaded Rivets-26@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditDelete();

                        swDoc.Extension.SelectByID2("Вырез-Вытянуть14@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE",
                            0, 0, 0, false, 0, null, 0);
                        swDoc.Extension.DeleteSelection2(deleteOption);
                    }
                    else
                    {
                        swDocExt.SelectByID2("Handel-2", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditDelete();
                        swDocExt.SelectByID2("Threaded Rivets-21@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDocExt.SelectByID2("Threaded Rivets-22@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDocExt.SelectByID2("Ручка MLA 120-1@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDocExt.SelectByID2("SC GOST 17475_gost-1@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDocExt.SelectByID2("SC GOST 17475_gost-2@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditDelete();

                        swDocExt.SelectByID2("Вырез-Вытянуть15@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0,false, 0, null, 0);
                        swDocExt.DeleteSelection2(deleteOption);
                    }

                    swDocExt.SelectByID2("Threaded Rivets-60@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("Threaded Rivets-61@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();

                    swDocExt.SelectByID2("1-1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null,0);
                    swDocExt.SelectByID2("1-2@" + nameUpPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("1-3@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null,0);
                    swDocExt.SelectByID2("1-4@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null,0);
                    swDocExt.SelectByID2("1-4@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null,0);
                    swDoc.EditDelete();

                    swDocExt.SelectByID2("3-1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null,0);
                    swDocExt.SelectByID2("3-2@" + nameUpPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("3-3@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null,0);
                    swDocExt.SelectByID2("3-4@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null,0);
                    swDocExt.SelectByID2("3-4@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null,0);
                    swDoc.EditDelete();

                    swDocExt.SelectByID2("1-1-1@" + nameUpPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("3-1-1@" + nameUpPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();

                    swDocExt.SelectByID2("Hole1", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("Hole2", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("D1@2-2@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false,
                        0, null, 0);
                    ((Dimension) (swDoc.Parameter("D1@2-2@" + nameUpPanel + ".Part"))).SystemValue = 0.065;
                    swDoc.EditRebuild3();

                    swDocExt.SelectByID2("D1@1-2@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0,
                        null, 0);
                    ((Dimension) (swDoc.Parameter("D1@1-2@" + nameDownPanel + ".Part"))).SystemValue = pType == "04" ||
                                                                                                       pType == "05"
                        ? 0.044
                        : 0.045;
                    swDoc.EditRebuild3();

                    swDocExt.SelectByID2("Hole2", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("Вырез-Вытянуть1@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("Эскиз27@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("Кривая1@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("Кривая1@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("Эскиз26@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDocExt.SelectByID2("Hole3", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("Вырез-Вытянуть3@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0,
                        true, 0, null, 0);
                    swDocExt.SelectByID2("Эскиз31@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null,
                        0);
                    swDocExt.SelectByID2("Кривая3@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0,
                        null, 0);
                    swDocExt.SelectByID2("Кривая3@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0,
                        null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("Эскиз30@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null,
                        0);
                    swDocExt.SelectByID2("Эскиз30@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null,
                        0);
                    swDoc.EditDelete();

                    // Удаление торцевых отверстий под саморезы
                    swDocExt.SelectByID2("Вырез-Вытянуть6@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0,
                        true, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);
                    swDocExt.SelectByID2("Вырез-Вытянуть7@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0,
                        true, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);

                    // Удаление торцевых отверстий под клепальные гайки
                    swDoc.Extension.SelectByID2("Под клепальные гайки", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз49@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("Панель1", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Панель2", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Панель3", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Панель3", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Эскиз32@" + "02-11-06-40-" + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("Эскиз32@" + "02-11-06_2-40-" + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.EditDelete();


                    // Удаление отверстий под монтажную рамку
                    swDocExt.SelectByID2("Вырез-Вытянуть9@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);
                    swDocExt.SelectByID2("Вырез-Вытянуть10@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);

                    swDocExt.SelectByID2("Вырез-Вытянуть12@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);
                    swDocExt.SelectByID2("Вырез-Вытянуть4@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);
                    swDocExt.SelectByID2("Вырез-Вытянуть5@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);
                    swDocExt.SelectByID2("Вырез-Вытянуть6@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0,true, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);

                    // Одна ручка
                    if (Convert.ToInt32(height) < 825)
                    {
                        swDocExt.SelectByID2("SC GOST 17473_gost-12@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditDelete();
                        swDocExt.SelectByID2("SC GOST 17473_gost-13@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("Вырез-Вытянуть19@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE",
                            0, 0, 0, false, 0, null, 0);
                        swDoc.Extension.DeleteSelection2(deleteOption);
                        swDoc.Extension.SelectByID2("Вырез-Вытянуть11@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE",
                            0, 0, 0, false, 0, null, 0);
                        swDoc.Extension.DeleteSelection2(deleteOption);                  
                    }
                    break;

                    #endregion
                                        

                #region Удаление элементов съемной панели

                case "01":case "21":case "22":case "23":
                case "30":case "31":case "32":case "35":
                    
                    swDocExt.SelectByID2("Handel-1", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDocExt.SelectByID2("Threaded Rivets-25@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("Ручка MLA 120-3@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("SC GOST 17475_gost-5@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("SC GOST 17475_gost-6@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("Threaded Rivets-26@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("Handel-2", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("Threaded Rivets-21@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("Threaded Rivets-22@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("Ручка MLA 120-1@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("SC GOST 17475_gost-1@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocExt.SelectByID2("SC GOST 17475_gost-2@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("Вырез-Вытянуть14@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0,
                        false, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);
                    swDocExt.SelectByID2("Вырез-Вытянуть15@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0,
                        false, 0, null, 0);
                    swDocExt.DeleteSelection2(deleteOption);

                    swDocExt.SelectByID2("SC GOST 17473_gost-12@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDocExt.SelectByID2("SC GOST 17473_gost-13@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть19@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0,
                        0, false, 0, null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0,
                        0, 0, false, 0, null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);

                    #endregion

                    if (pType == "01" || pType == "35")
                    {
                        swDocExt.SelectByID2("D1@1-2@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0,
                            null, 0);
                        ((Dimension) (swDoc.Parameter("D1@1-2@" + nameDownPanel + ".Part"))).SystemValue = 0.047;
                        swDoc.EditRebuild3();

                        swDocExt.SelectByID2("D1@2-2@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0,
                            null, 0);
                        ((Dimension) (swDoc.Parameter("D1@2-2@" + nameUpPanel + ".Part"))).SystemValue = 0.067;
                        swDoc.EditRebuild3();

                        if (pType == "35")
                        {
                            swDocExt.SelectByID2("Вырез-Вытянуть12@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                            swDocExt.DeleteSelection2(deleteOption);
                            swDocExt.SelectByID2("Вырез-Вытянуть4@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                            swDocExt.DeleteSelection2(deleteOption);
                            swDocExt.SelectByID2("Вырез-Вытянуть5@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                            swDocExt.DeleteSelection2(deleteOption);
                            swDocExt.SelectByID2("Вырез-Вытянуть6@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                            swDocExt.DeleteSelection2(deleteOption);
                            swDocExt.SelectByID2("Вырез-Вытянуть7@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                            swDocExt.DeleteSelection2(deleteOption);
                        }

                        if (типУсиливающей != null)
                        {
                            swDocExt.SelectByID2("D1@2-2@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false,
                                0, null, 0);
                            ((Dimension) (swDoc.Parameter("D1@2-2@" + nameUpPanel + ".Part"))).SystemValue = 0.03;
                            swDoc.EditRebuild3();

                            swDocExt.SelectByID2("D1@1-2@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0,
                                false, 0, null, 0);
                            ((Dimension) (swDoc.Parameter("D1@1-2@" + nameDownPanel + ".Part"))).SystemValue = 0.01;
                            swDoc.EditRebuild3();
                        }
                    }
                    
                    break;
            }

            #endregion

            if (!string.IsNullOrEmpty(типТорцевой) && pType != "01")
            {
                swDocExt.SelectByID2("D1@2-2@" + nameUpPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@2-2@" + nameUpPanel + ".Part"))).SystemValue = 0.0975;
                swDoc.EditRebuild3();

                swDocExt.SelectByID2("D1@1-2@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@1-2@" + nameDownPanel + ".Part"))).SystemValue = 0.0775;
                swDoc.EditRebuild3();
            }
            
            if (pType != "01" && pType != "35")
            {                
                swDocExt.SelectByID2("Вырез-Вытянуть18@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDocExt.DeleteSelection2(deleteOption);
                swDocExt.SelectByID2("Вырез-Вытянуть25@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDocExt.DeleteSelection2(deleteOption);             
            }

            if (pType == "01" || pType == "35")
            {                
                //Удаление торцевых отверстий под клепальные гайки
                swDoc.Extension.SelectByID2("Под клепальные гайки", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Эскиз49@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Панель1", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Панель2", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Панель3", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Панель3", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();

                swDoc.Extension.SelectByID2("Эскиз32@" + "02-11-06-40-" + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Эскиз32@" + "02-11-06_2-40-" + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                swDoc.EditDelete();               
            }

            if (pType != "32")
            {
                
                for (var i = 5; i <= 20; i++)
                {
                    swDocExt.SelectByID2("Threaded Rivets-" + i + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                }

                swDocExt.SelectByID2("Ножка", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                swDocExt.SelectByID2("Вырез-Вытянуть11@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDocExt.DeleteSelection2(deleteOption);
                swDocExt.SelectByID2("Зеркальное отражение2@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }


            if (pType != "22" && pType != "31")
            {
                swDocExt.SelectByID2("Threaded Rivets-33@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocExt.SelectByID2("Threaded Rivets-34@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocExt.SelectByID2("Threaded Rivets-35@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocExt.SelectByID2("Threaded Rivets-36@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.EditDelete();
                swDocExt.SelectByID2("Up", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDocExt.SelectByID2("Вырез-Вытянуть16@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDocExt.DeleteSelection2(deleteOption);
                swDoc.ClearSelection2(true);
            }

            if (pType != "04" && pType != "05")
            {               
                swDocExt.SelectByID2("SC GOST 17473_gost-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("SC GOST 17473_gost-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                swDocExt.SelectByID2("Threaded Rivets-59@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Threaded Rivets-60@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Threaded Rivets-61@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Threaded Rivets-62@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDoc.ClearSelection2(true);
            }

            if (config.Contains("00"))
            {
                foreach (var i in new[] { 37, 38, 47, 48, 51, 52 })
                {
                    swDocExt.SelectByID2("Threaded Rivets-" + i + "@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }
            }

            #region to delete

            //if (pType != "21" & pType != "20" & pType != "22" & pType != "23" & pType != "30" & pType != "31" & pType != "32" & pType != "33")
            //{
            //    foreach (var i in new[] { 37, 38, 47, 48, 51, 52 })
            //    {
            //        swDocExt.SelectByID2("Threaded Rivets-" + i + "@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
            //        swDoc.ClearSelection2(true);
            //    }

            //    swDocExt.SelectByID2("4-1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
            //    swDoc.EditSuppress2();
            //    swDocExt.SelectByID2("4-3@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
            //    swDoc.EditSuppress2();

            //    swDoc.ClearSelection2(true);
            //}

            #endregion

            if (config.Contains("01"))
            {
                swDocExt.SelectByID2("Threaded Rivets-38@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Threaded Rivets-48@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Threaded Rivets-51@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                swDocExt.SelectByID2("4-1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress2(); swDoc.EditRebuild3();
                swDocExt.SelectByID2("1-0@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress2(); swDoc.EditRebuild3();
                
                // Погашение отверстий под клепальные гайки
                swDoc.Extension.SelectByID2("Вырез-Вытянуть20@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть18@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть16@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress2();

                swDoc.Extension.SelectByID2("Вырез-Вытянуть7@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSuppress2();

                // В усиливающих профилях
                swDoc.Extension.SelectByID2("Вырез-Вытянуть15@" + "02-11-06_2-40-" + "-4@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть16@" + "02-11-06_2-40-" + "-4@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть17@" + "02-11-06_2-40-" + "-4@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress2();

                swDoc.ClearSelection2(true);
            }
                       

            if (config.Contains("02"))
            {               
                swDocExt.SelectByID2("Threaded Rivets-37@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Threaded Rivets-47@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Threaded Rivets-52@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                swDocExt.SelectByID2("2-1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress2(); swDoc.EditRebuild3();
                swDocExt.SelectByID2("1-1@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress2(); swDoc.EditRebuild3();

                // Погашение отверстий под клепальные гайки
                swDoc.Extension.SelectByID2("Вырез-Вытянуть19@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть17@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть15@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress2();

                swDoc.Extension.SelectByID2("Вырез-Вытянуть6@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSuppress2();

                // В усиливающих профилях
                swDoc.Extension.SelectByID2("Вырез-Вытянуть15@" + "02-11-06-40-" + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть16@" + "02-11-06-40-" + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть17@" + "02-11-06-40-" + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress2();

                swDoc.ClearSelection2(true);

            }
            
            #region Удаление усиления для типоразмера меньше AV09

            if (!усиление)
            {
                foreach (var component in new[] {"02-11-06-40--1", "02-11-06_2-40--4", "02-11-07-40--1", "02-11-07-40--2"})
                {
                    swDocExt.SelectByID2(component + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                }

                foreach (var number in new[]
                {                    
                    "37", "38", "39", "40",
                    "41", "42", "43", "44",
                    "45", "46", "47", "48",
                    "49", "50", "51", "52",
                    "53", "54", "55", "56"
                })
                {
                    swDocExt.SelectByID2("Rivet Bralo-" + number + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                }

                swDoc.ShowConfiguration2("Вытяжная заклепка 3,0х6 (ст ст. с пл. гол.)");
                swDocExt.SelectByID2("Усиление", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                swDocExt.SelectByID2("Hole7@" + nameDownPanel + "-1@" + nameAsm, "FTRFOLDER", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Эскиз45@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Вырез-Вытянуть13@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Кривая7@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Hole8@" + nameDownPanel + "-1@" + nameAsm, "FTRFOLDER", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Эскиз47@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Вырез-Вытянуть14@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Кривая9@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();

                swDoc.ClearSelection2(true);                
            }
            
            #endregion

            #region Отверстия под панели L2 L3

            if (Convert.ToInt32(OutValPanels.L2) == 28)
            {

                swDocExt.SelectByID2("Threaded Rivets-47@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Threaded Rivets-48@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                swDoc.Extension.SelectByID2("Вырез-Вытянуть17@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть18@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Кривая11@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Кривая11@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSuppress();swDoc.ClearSelection2(true);

                swDoc.Extension.SelectByID2("Вырез-Вытянуть16@" + "02-11-06-40-" + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Кривая5@" + "02-11-06-40-" + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress();swDoc.ClearSelection2(true);

                swDoc.Extension.SelectByID2("Вырез-Вытянуть16@" + "02-11-06_2-40-" + "-4@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Кривая5@" + "02-11-06_2-40-" + "-4@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress();swDoc.ClearSelection2(true);                
            }
            

            if (Convert.ToInt32(OutValPanels.L3) == 28)
            {
                swDocExt.SelectByID2("Threaded Rivets-51@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("Threaded Rivets-52@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Панель3", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();

                swDoc.Extension.SelectByID2("Вырез-Вытянуть19@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Вырез-Вытянуть20@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Кривая12@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Кривая12@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSuppress();swDoc.ClearSelection2(true);

                swDoc.Extension.SelectByID2("Вырез-Вытянуть17@" + "02-11-06-40-" + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Кривая6@" + "02-11-06-40-" + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress();swDoc.ClearSelection2(true);

                swDoc.Extension.SelectByID2("Вырез-Вытянуть17@" + "02-11-06_2-40-" + "-4@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Кривая6@" + "02-11-06_2-40-" + "-4@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.EditSuppress();swDoc.ClearSelection2(true);
            }

            
            #endregion

            #region Панели усиливающие

            string типКрепежнойЧастиУсиливающейПанели = null;
            var типТорцевойЧастиУсиливающейПанели = "T";

            if (!string.IsNullOrEmpty(типУсиливающей))
            {
                try
                {
                    типТорцевойЧастиУсиливающейПанели = типУсиливающей.Remove(1).Contains("T") ? "T" : "E";
                    if (типУсиливающей.Remove(0, 1).Contains("E"))
                    {
                        типКрепежнойЧастиУсиливающейПанели = "E";
                    }
                    if (типУсиливающей.Remove(0, 1).Contains("D"))
                    {
                        типКрепежнойЧастиУсиливающейПанели = "D";
                    }
                    if (типУсиливающей.Remove(0, 1).Contains("E"))
                    {
                        типКрепежнойЧастиУсиливающейПанели = "E";
                    }
                    if (типУсиливающей.Remove(0, 1).Contains("Z"))
                    {
                        типКрепежнойЧастиУсиливающейПанели = "Z";
                    }
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                }
            }
            

            if (Convert.ToInt32(height) < 825)
            {
                swDoc.Extension.SelectByID2("UpperAV09@02-11-09-40--1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditSuppress();
            }

            if (типТорцевойЧастиУсиливающейПанели == "E")
            {
                swDocExt.SelectByID2("1-1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("1-3@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("1-1-1@" + nameUpPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);swDoc.EditDelete();

                swDocExt.SelectByID2("Эскиз42@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditSuppress();
                swDocExt.SelectByID2("Hole1@" + nameUpPanel + "-1@" + nameAsm, "FTRFOLDER", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
            }

            if (типКрепежнойЧастиУсиливающейПанели != "D")
            {
                foreach (var component in new[]
                {
                    "02-11-09-40--1", 
                    "Threaded Rivets с насечкой-1", "Threaded Rivets с насечкой-2",
                    "Threaded Rivets с насечкой-3", "Threaded Rivets с насечкой-4"
                })
                {
                    swDocExt.SelectByID2(component + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                }
                swDocExt.SelectByID2("Кронштейн", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("U10@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditSuppress();
            }

            if (типКрепежнойЧастиУсиливающейПанели != "Z")
            {
                swDocExt.SelectByID2("U20@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditSuppress();

                foreach (var component in new[]
                {
                    "Threaded Rivets с насечкой-5", "Threaded Rivets с насечкой-6"
                })
                {
                    swDocExt.SelectByID2(component + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                }
            }

            if (типКрепежнойЧастиУсиливающейПанели == "Z" || типКрепежнойЧастиУсиливающейПанели == "D" || типКрепежнойЧастиУсиливающейПанели == "E")
            {
                swDocExt.SelectByID2("3-2@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("3-1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDocExt.SelectByID2("3-1-1@" + nameUpPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();

                swDocExt.SelectByID2("Эскиз41@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, true, 0, null, 0); swDoc.EditSuppress();
                swDocExt.SelectByID2("Hole3@" + nameUpPanel + "-1@" + nameAsm, "FTRFOLDER", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();

                swDocExt.SelectByID2("Эскиз56@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0); swDoc.EditSuppress();
            }
            

            #endregion

            #region Вставки внутренние

            if (ValProfils.Tp1 == "01" || ValProfils.Tp1 == "00")
            {
                if (config.Contains("01"))
                {
                    swDoc.Extension.SelectByID2("Вырез-ВытянутьTp1R@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditSuppress();
                    swDoc.Extension.SelectByID2("Эскиз80@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditSuppress();
                }
                if (config.Contains("02"))
                {
                    swDoc.Extension.SelectByID2("Вырез-ВытянутьTp1L@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditSuppress();
                    swDoc.Extension.SelectByID2("Эскиз81@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditSuppress();
                }
            }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp1R@" + nameDownPanel + "-1", supress);
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp1L@" + nameDownPanel + "-1", supress);
            }

            if (ValProfils.Tp2 == "01" || ValProfils.Tp2 == "00")
            {
                if (config.Contains("01"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp2R@" + nameDownPanel + "-1", supress);
                    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз61@" + nameDownPanel + "-1", supress);
                }
                if (config.Contains("02"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp2L@" + nameDownPanel + "-1", supress);
                    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз62@" + nameDownPanel + "-1", supress);
                }
            }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp2R@" + nameDownPanel + "-1", supress);
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp2L@" + nameDownPanel + "-1", supress);
            }

            if (ValProfils.Tp3 == "01" || ValProfils.Tp3 == "00")
            {
                if (config.Contains("01"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp3R@" + nameDownPanel + "-1", supress);
                    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз63@" + nameDownPanel + "-1", supress);
                }
                if (config.Contains("02"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp3L@" + nameDownPanel + "-1", supress);
                    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз64@" + nameDownPanel + "-1", supress);
                }
            }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp3R@" + nameDownPanel + "-1", supress);
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp3L@" + nameDownPanel + "-1", supress);               
            }

            if (ValProfils.Tp4 == "01" || ValProfils.Tp4 == "00")
            {
                if (config.Contains("01"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp4R@" + nameDownPanel + "-1", supress);
                    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз82@" + nameDownPanel + "-1", supress);
                }
                if (config.Contains("02"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp4L@" + nameDownPanel + "-1", supress);
                    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз83@" + nameDownPanel + "-1", supress);
                }
            }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp4R@" + nameDownPanel + "-1", supress);
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Вырез-ВытянутьTp4L@" + nameDownPanel + "-1", supress);
            }

            if (ValProfils.Tp1 == "02")
            {
                //VentsCad.DoWithSwDoc(_swApp, bodyfeat, $"Тип-02-{(isLeftSide ? "1R" : "1L")}@{nameDownPanel}-1", supress);

                if (config.Contains("01"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-1R@" + nameDownPanel + "-1", supress);
                }
                if (config.Contains("02"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-1L@" + nameDownPanel + "-1", supress);
                }
            }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-1R@" + nameDownPanel + "-1", supress);
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-1L@" + nameDownPanel + "-1", supress);
            }

            if (ValProfils.Tp2 == "02")
            {
                if (config.Contains("01"))
                {
                    swDoc.Extension.SelectByID2("Тип-02-2R@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditSuppress();
                }
                if (config.Contains("02"))
                {
                    swDoc.Extension.SelectByID2("Тип-02-2L@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditSuppress();
                }
                //VentsCad.DoWithSwDoc(_swApp, bodyfeat,   $"Тип-02-{(isLeftSide ? "2R" : "2L")}@{nameDownPanel}-1", supress);
            }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-2R@" + nameDownPanel + "-1", supress);
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-2L@" + nameDownPanel + "-1", supress);
            }

            if (ValProfils.Tp3 == "02")
            {
                // VentsCad.DoWithSwDoc(_swApp, bodyfeat, $"Тип-02-{(isLeftSide ? "3R" : "3L")}@{nameDownPanel}-1", supress);
                if (config.Contains("01"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-3R@" + nameDownPanel + "-1", supress);
                }
                if (config.Contains("02"))
                {
                    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-3L@" + nameDownPanel + "-1", supress);
                }
            }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-3R@" + nameDownPanel + "-1", supress);
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-3L@" + nameDownPanel + "-1", supress);
            }


            if (ValProfils.Tp4 == "02")
            {
                //VentsCad.DoWithSwDoc(_swApp, bodyfeat, $"Тип-02-{(isLeftSide ? "4R" : "4L")}@{nameDownPanel}-1", supress);
                if (config.Contains("01"))
                {
                    swDoc.Extension.SelectByID2("Тип-02-4R@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditSuppress();
                }
                if (config.Contains("02"))
                {
                    swDoc.Extension.SelectByID2("Тип-02-4L@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditSuppress();
                }

                //if (isLeftSide)
                //{
                //    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-4R@" + nameDownPanel + "-1", supress);
                //}
                //if (!isLeftSide)
                //{
                //    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-4L@" + nameDownPanel + "-1", supress);
                //}
            }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-4R@" + nameDownPanel + "-1", supress);
                VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-02-4L@" + nameDownPanel + "-1", supress);
            }

            // Полупанель внутрення

            //if (ValProfils.Tp1 != "05")             
            //{
            //    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-05-1@" + nameDownPanel + "-1", supress);
            //    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз88@" + nameDownPanel + "-1", supress);
            //}

            //if (ValProfils.Tp2 != "05") 
            //{
            //    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-05-2@" + nameDownPanel + "-1", supress);
            //    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз66@" + nameDownPanel + "-1", supress);
            //}

            //if (ValProfils.Tp3 != "05") 
            //{
            //    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-05-3@" + nameDownPanel + "-1", supress);
            //    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз67@" + nameDownPanel + "-1", supress);
            //}

            //if (ValProfils.Tp4 != "05")
            //{
            //    VentsCad.DoWithSwDoc(_swApp, bodyfeat, "Тип-05-4@" + nameDownPanel + "-1", supress);
            //    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз89@" + nameDownPanel + "-1", supress);
            //}

            #region To Delete

            if (ValProfils.Tp1 == "05") { }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, VentsCad.CompType.BODYFEATURE, "Тип-05-1@" + nameDownPanel + "-1", VentsCad.Act.Suppress);
                VentsCad.DoWithSwDoc(_swApp, VentsCad.CompType.BODYFEATURE, "Эскиз88@" + nameDownPanel + "-1", VentsCad.Act.Suppress);
            }

            if (ValProfils.Tp2 == "05") { }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, VentsCad.CompType.BODYFEATURE, "Тип-05-2@" + nameDownPanel + "-1", VentsCad.Act.Suppress);
                VentsCad.DoWithSwDoc(_swApp, VentsCad.CompType.BODYFEATURE, "Эскиз66@" + nameDownPanel + "-1", VentsCad.Act.Suppress);
            }

            if (ValProfils.Tp3 == "05") { }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, VentsCad.CompType.BODYFEATURE, "Тип-05-3@" + nameDownPanel + "-1", VentsCad.Act.Suppress);
                VentsCad.DoWithSwDoc(_swApp, VentsCad.CompType.BODYFEATURE, "Эскиз67@" + nameDownPanel + "-1", VentsCad.Act.Suppress);
            }

            if (ValProfils.Tp4 == "05") { }
            else
            {
                VentsCad.DoWithSwDoc(_swApp, VentsCad.CompType.BODYFEATURE, "Тип-05-4@" + nameDownPanel + "-1", VentsCad.Act.Suppress);
                VentsCad.DoWithSwDoc(_swApp, VentsCad.CompType.BODYFEATURE, "Эскиз89@" + nameDownPanel + "-1", VentsCad.Act.Suppress);
            }

            #endregion

            #endregion

            #region Отверстия под усиливающие панели

            if (pType == "21" || pType == "22" || pType == "23")
            {

                swDoc.Extension.SelectByID2("Эскиз59@" + nameUpPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                swDoc.EditUnsuppress2();
                swDoc.Extension.SelectByID2("Эскиз73@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                swDoc.EditUnsuppress2();

                if (ValProfils.Tp1 != "-") 
                {
                    if (config.Contains("02"))
                    {
                        foreach (var name in new[] { "U32", "U31" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        }
                        foreach (var name in new[] { "U33", "U34" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        }
                    }
                    if (config.Contains("01"))
                    {
                        foreach (var name in new[] { "U52", "U51" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        }
                        foreach (var name in new[] { "U53", "U54" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        }
                    }
                }

                if (ValProfils.Tp4 != "-") 
                {
                    if (config.Contains("02"))
                    {
                        foreach (var name in new[] { "U42", "U41" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }                    
                        foreach (var name in new[] { "U43", "U44" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                    }
                    if (config.Contains("01"))
                    {
                        foreach (var name in new[] { "U62", "U61" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                        foreach (var name in new[] { "U63", "U64" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                    }
                }
            }

            #region TO delete

            //if (pType == "21" || pType == "22" || pType == "23")
            //{
            //    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз59@" + nameUpPanel + "-1", unSupress);
            //    VentsCad.DoWithSwDoc(_swApp, sketch, "Эскиз73@" + nameUpPanel + "-1", unSupress);

            //    //  Наличие первой усиливающей панели
            //    if (string.IsNullOrEmpty(ValProfils.Tp1))//(!ValProfils.Tp1.Contains("-"))
            //    {
            //        foreach (var name in new[] { "U32", "U31" })
            //        {
            //            VentsCad.DoWithSwDoc(_swApp, bodyfeat, name + "@" + nameUpPanel + "-1", !isLeftSide ? unSupress : doNothing);
            //        }
            //        foreach (var name in new[] { "U33", "U34" })
            //        {
            //            VentsCad.DoWithSwDoc(_swApp, bodyfeat, name + "@" + nameDownPanel + "-1", !isLeftSide ? unSupress : doNothing);
            //        }

            //        foreach (var name in new[] { "U52", "U51" })
            //        {
            //            VentsCad.DoWithSwDoc(_swApp, bodyfeat, name + "@" + nameUpPanel + "-1", isLeftSide ? unSupress : doNothing);
            //        }
            //        foreach (var name in new[] { "U53", "U54" })
            //        {
            //            VentsCad.DoWithSwDoc(_swApp, bodyfeat, name + "@" + nameDownPanel + "-1", isLeftSide ? unSupress : doNothing);
            //        }                    
            //    }
            //    if (string.IsNullOrEmpty(ValProfils.Tp4)) //(!ValProfils.Tp4.Contains("-"))
            //    {
            //        foreach (var name in new[] { "U42", "U41" })
            //        {
            //            VentsCad.DoWithSwDoc(_swApp, bodyfeat, name + "@" + nameUpPanel + "-1", !isLeftSide ? unSupress : doNothing);
            //        }
            //        foreach (var name in new[] { "U43", "U44" })
            //        {
            //            VentsCad.DoWithSwDoc(_swApp, bodyfeat, name + "@" + nameDownPanel + "-1", !isLeftSide ? unSupress : doNothing);
            //        }

            //        foreach (var name in new[] { "U62", "U61" })
            //        {
            //            VentsCad.DoWithSwDoc(_swApp, bodyfeat, name + "@" + nameUpPanel + "-1", isLeftSide ? unSupress : doNothing);
            //        }
            //        foreach (var name in new[] { "U63", "U64" })
            //        {
            //            VentsCad.DoWithSwDoc(_swApp, bodyfeat, name + "@" + nameDownPanel + "-1", isLeftSide ? unSupress : doNothing);
            //        }                    
            //    }
            //}

            #endregion


            if (pType == "30" || pType == "31")
            {

                swDoc.Extension.SelectByID2("Эскиз59@" + nameUpPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                swDoc.EditUnsuppress2();
                swDoc.Extension.SelectByID2("Эскиз73@" + nameDownPanel + "-1@" + nameAsm, "SKETCH", 0, 0, 0, true, 0, null, 0);
                swDoc.EditUnsuppress2();

                if (ValProfils.Tp1 != "-") 
                {
                    if (config.Contains("02"))
                    {
                        foreach (var name in new[] { "U32", "U31" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                        foreach (var name in new[] { "U33", "U34" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                    }
                    if (config.Contains("01"))
                    {
                        foreach (var name in new[] { "U52", "U51" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                        foreach (var name in new[] { "U53", "U54" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                    }
                }
                if (ValProfils.Tp4 != "-")
                {
                    if (config.Contains("02"))
                    {
                        foreach (var name in new[] { "U42", "U41" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                        foreach (var name in new[] { "U43", "U44" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                    }
                    if (config.Contains("01"))
                    {
                        foreach (var name in new[] { "U62", "U61" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                        foreach (var name in new[] { "U63", "U64" })
                        {
                            swDocExt.SelectByID2(name + "@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                            swDoc.EditUnsuppress2();
                        }
                    }
                }
            }

            #endregion

            #region Со скотчем "ребро-кромка"

            if (!скотч)// != "Со скотчем")
            {
                //MessageBox.Show($"Со скотчем {скотч}");
                VentsCad.DoWithSwDoc(_swApp, VentsCad.CompType.COMPONENT, "02-11-04-40--1", VentsCad.Act.Delete);
                //MessageBox.Show("Со скотчем 2");
                swDocExt.SelectByID2("D1@Расстояние1@" + nameAsm + ".SLDASM", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@Расстояние1"))).SystemValue = 0; 
            }

            #endregion

            #region Изменение деталей

            #region Кронштейны двойной панели            
            
            double колЗаклепокКронштейнаДвойнойПанели = 2000;

            if (!string.IsNullOrEmpty(типДвойнойРазрез))
            {
                var idToDelete = "-2";
                var idToChange = "-1";

                var lenghtOfProfil = Convert.ToDouble(height);

                var nameOfProfil =  усиление ? "02-11-13-40-" : "02-11-14-40-";
                var nameOfProfilToDelete = !усиление ? "02-11-13-40-" : "02-11-14-40-";

                // todo учет толщины
                var deltaForLenght = усиление ? 48.0 : 3.5; 
                var newNameP = nameOfProfil + height; 

                var cut = типДвойнойРазрез == "H" ? " по высоте H " : " по ширине W ";

                if (типДвойнойРазрез == "H")
                {
                    idToDelete = "-1";
                    idToChange = "-2";
                    newNameP = nameOfProfil + width; 
                    lenghtOfProfil = Convert.ToDouble(width); 
                }

                swDoc.Extension.SelectByID2(nameOfProfil + idToDelete + "@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2(nameOfProfilToDelete + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2(nameOfProfilToDelete + "-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
               
                #region

                switch (типДвойнойРазрез)
                {
                    case "H":
                        foreach (var number in new[]
                        { "105", "106", "113", "114", "137", "138", "139", "140", "141", "142", "143", "144", "145", "146"})
                        {
                            swDocExt.SelectByID2("Rivet Bralo-" + number + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        }

                        swDoc.Extension.SelectByID2("Cut-ExtrudeWP1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                        swDoc.Extension.SelectByID2("Cut-ExtrudeW@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);

                        swDoc.Extension.SelectByID2("Cut-ExtrudeWP1@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                        swDoc.Extension.SelectByID2("Cut-ExtrudeW@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);

                        swDocExt.SelectByID2("WP1@" + nameAsm, "FTRFOLDER", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();

                        swDoc.Extension.SelectByID2("Cut-ExtrudeWC@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                        
                        if (типДвойнойВерхней == "0")
                        {
                            foreach (var number in new[] {"123", "124", "160", "161", "157", "158", "159"})
                            {
                                swDocExt.SelectByID2("Rivet Bralo-" + number + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                            }                            
                            swDoc.Extension.SelectByID2("Cut-ExtrudeHP1@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                            swDoc.Extension.SelectByID2("Cut-ExtrudeH@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                        }
                        if (типДвойнойНижней == "0")
                        {
                            foreach (var number in new[] { "121", "122", "162", "163", "164", "165", "166"})
                            {
                                swDocExt.SelectByID2("Rivet Bralo-" + number + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);
                                swDoc.EditDelete();
                            }

                            swDoc.Extension.SelectByID2("Cut-ExtrudeHP1@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                            swDoc.Extension.SelectByID2("Cut-ExtrudeH@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                        }
                        break;

                    case "W":
                        foreach (var number in new[]
                        {
                            "121", "122", "123", "124", "162", "163", "164", "165", "166",
                            "157", "158", "159", "160", "161"
                        })
                        {
                            swDocExt.SelectByID2("Rivet Bralo-" + number + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        }

                        swDocExt.SelectByID2("HP1@" + nameAsm, "FTRFOLDER", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();

                        swDoc.Extension.SelectByID2("Cut-ExtrudeHP1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                        swDoc.Extension.SelectByID2("Cut-ExtrudeH@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);

                        swDoc.Extension.SelectByID2("Cut-ExtrudeHP1@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                        swDoc.Extension.SelectByID2("Cut-ExtrudeH@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);

                        swDoc.Extension.SelectByID2("Cut-ExtrudeHC@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);

                        if (типДвойнойВерхней == "0")
                        {
                            foreach (var number in new[] { "113", "114", "137", "138", "139", "140", "141"})
                            {
                                swDocExt.SelectByID2("Rivet Bralo-" + number + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);swDoc.EditDelete();
                            }
                            swDoc.Extension.SelectByID2("Cut-ExtrudeWP1@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                            swDoc.Extension.SelectByID2("Cut-ExtrudeW@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                            
                            swDoc.Extension.SelectByID2("Cut-ExtrudeWC@" + nameUpPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                        }
                        if (типДвойнойНижней == "0")
                        {
                            foreach (var number in new[] { "105", "106", "142", "143", "144", "145", "146"})
                            {
                                swDocExt.SelectByID2("Rivet Bralo-" + number + "@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0);swDoc.EditDelete();
                            }
                           
                            swDoc.Extension.SelectByID2("Cut-ExtrudeWP1@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                            swDoc.Extension.SelectByID2("Cut-ExtrudeW@" + nameDownPanel + "-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.Extension.DeleteSelection2(deleteOption);
                        }
                        break;
                }

                #endregion

                var newPartPathP = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newNameP}.SLDPRT";

                колЗаклепокКронштейнаДвойнойПанели = (Math.Truncate((lenghtOfProfil - 45.0) / 125) + 1) * 1000;
                
                try
                {
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathP), 1))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(nameOfProfil + idToChange + "@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathP, "", false, true);
                        _swApp.CloseDoc(nameOfProfil +".SLDPRT");
                    }
                    else 
                    {
                        SwPartParamsChangeWithNewName(nameOfProfil,
                            $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newNameP}",
                            new[,]
                            {
                                {"D2@Эскиз1", Convert.ToString(lenghtOfProfil - deltaForLenght)},
                                {"D1@CrvPattern1", Convert.ToString(колЗаклепокКронштейнаДвойнойПанели)}
                            },
                            false, null);
                        _swApp.CloseDoc(newNameP);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }              
            }

            #endregion

            #region  Панель внешняя

            var newName = панельВнешняя.NewName;
            var newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}.SLDPRT";

            var outerPanel = newPartPath;

            //todo вынести кол-во в метод модели

            var screwsByHeight =
            string.IsNullOrEmpty(типТорцевой)
                ? колСаморезВинтВысота
                : колСаморезВинтВысота - 1000;

            var zaklByHeight = pType == "01" || pType == "35" || pType == "04" || pType == "05"
                ? колЗаклепокВысота
                : колЗаклепокВысота + 1000;

            if (типДвойнойРазрез == "H")
            {
                if ((screwsByHeight/1000) % 2 != 0)
                {
                    screwsByHeight = screwsByHeight + 1000;
                }

                if ((zaklByHeight/1000) % 2 != 0)
                {
                    zaklByHeight = zaklByHeight + 1000;
                }
            }
            
            var screwsByWidth = pType == "01" || pType == "35" ? (колСаморезВинтШирина - 1000 < 2000 ? 2000 : колСаморезВинтШирина - 1000)
                        : (колСаморезВинтШирина < 2000 ? 2000 : колСаморезВинтШирина);

            var zaklByWidth = колЗаклепокШирина;
            
            if (типДвойнойРазрез == "W")
            {
                if ((screwsByWidth/1000) % 2 != 0)
                {
                    screwsByWidth = screwsByWidth + 1000;
                }

                if ((zaklByWidth/1000) % 2 != 0)
                {
                    zaklByWidth = zaklByWidth + 1000;
                }
            }

            if (screws.ByWidth > 0 & pType.Contains("3"))
            {
                screwsByWidth = screws.ByWidth;
            }
           
            try
            {
                if (screws?.ByHeight > 0)
                {
                    screwsByHeight = screws.ByHeight;
                }
                if (screws?.ByWidth > 0)
                {
                    screwsByWidth = screws.ByWidth;
                }
            }
            catch (Exception){}

            var screwsByWidthInner =
                pType != "01" || pType != "35"
                    ? (колСаморезВинтШирина - 1000 < 2000 ? 2000 : колСаморезВинтШирина - 1000)
                    : (колСаморезВинтШирина2 < 2000 ? 2000 : колСаморезВинтШирина);

            var screwsByHeightInner = pType == "04" || pType == "05"
                ? (колСаморезВинтВысота)
                : (колСаморезВинтВысота - 1000);

            try
            {
                if (screws?.ByHeightInner > 0)
                {
                    screwsByHeightInner = screws.ByHeightInner < 2000 ? 2000 : screws.ByHeightInner;
                }
                if (screws?.ByWidthInner > 0)
                {
                    screwsByWidthInner = screws.ByWidthInner < 2000 ? 2000 : screws.ByWidthInner;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

            if (GetExistingFile(newPartPath, 1))//   (Path.GetFileNameWithoutExtension(newPartPath), 1))
            {
                swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                swDoc.Extension.SelectByID2(nameUpPanel + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", false, true);
                _swApp.CloseDoc(nameUpPanel + ".SLDPRT");
            }
            else 
            {
                var d1Кривая3 = pType == "35"
                    ? (колСаморезВинтШирина - 1000 < 2000 ? 2000 : колСаморезВинтШирина - 1000)
                    : (колСаморезВинтШирина < 2000 ? 2000 : колСаморезВинтШирина);

                var d1Эскиз52 = типКрепежнойЧастиУсиливающейПанели == null ? Convert.ToString(30) : Convert.ToString(20);

                if (!string.IsNullOrEmpty(типТорцевой))
                {
                    d1Кривая3 = колСаморезВинтШирина < 2000 ? 2000 : колСаморезВинтШирина;
                    d1Эскиз52 = Convert.ToString(35);
                }

                if (screws?.ByWidthInnerUp > 0)
                {
                    d1Кривая3 = screws.ByWidthInnerUp;
                }

                //типДвойнойРазрез

                SwPartParamsChangeWithNewName(nameUpPanel,
                    $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}",
                    new[,]
                    {
                        // Габариты
                        {"D1@Эскиз1", Convert.ToString(ширинаПанели)},
                        {"D2@Эскиз1", Convert.ToString(высотаПанели)},
                        
                        {"D1@3-4", Convert.ToString(screwsByHeight)},
                        {"D1@1-4", Convert.ToString(screwsByHeight)},

                        {"D1@2-4",  Convert.ToString(screwsByWidth)},
                        
                        
                        {"D2@2-2", Convert.ToString(осьСаморезВинт)},
                        {"D4@Эскиз47", Convert.ToString(растояниеМеждуРучками)},
                        
                        {"D1@Эскиз50", Convert.ToString(диамСаморезВинт)},
                        {"D1@2-3-1", Convert.ToString(диамСаморезВинт)},
                        
                        {"D1@Эскиз52", d1Эскиз52},
                        {"D2@Эскиз52", Convert.ToString(осьПоперечныеОтверстия)},

                        {"D1@Кривая3", Convert.ToString(d1Кривая3)},
                        
                        {"D3@1-1-1", string.IsNullOrEmpty(типТорцевой) || pType == "01" ?  Convert.ToString(35) : Convert.ToString(158.1)},
                        {"D2@3-1-1", string.IsNullOrEmpty(типТорцевой) || pType == "01" ?  Convert.ToString(35) : Convert.ToString(158.1)},
                        

                        {"D3@2-1-1", Convert.ToString(диамЗаглушкаВинт)},
                        {"D1@Эскиз49", Convert.ToString(диамЗаглушкаВинт)},
                        
                        {"D1@Кривая1", Convert.ToString(zaklByWidth)},

                        
                        {"D1@Кривая2", Convert.ToString(zaklByHeight)},


                        {"D7@Ребро-кромка1", скотч ? Convert.ToString(17.7) : Convert.ToString(19.2)},
                                                
                        
                        {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')},


                        {"D1@CrvPatternW", Convert.ToString(колЗаклепокКронштейнаДвойнойПанели)},
                        {"D1@CrvPatternH", Convert.ToString(колЗаклепокКронштейнаДвойнойПанели)}
                    },
                    true,
                    типДвойнойВерхней != "0" ? new []
                    {
                        $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{имяДвойнойВерхней1}",
                        $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{имяДвойнойВерхней2}"
                    } : null);
                VentsMatdll(materialP1, new[] { покрытие[6], покрытие[1], покрытие[2] }, newName);
                _swApp.CloseDoc(newName);
            }

            #endregion

            #region  Панель внутреняя

            newName = панельВнутренняя.NewName;
            //newName = modelname2 + "-02-" + width + "-" + lenght + "-" + "40-" + materialP2[0] + strenghtP + panelsUpDownConfigString;
            newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}.SLDPRT";
            var innerPanel = newPartPath;

            if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
            {
                swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                swDoc.Extension.SelectByID2(nameDownPanel + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", false, true);
                _swApp.CloseDoc(nameDownPanel + ".SLDPRT");
            }
            else
            {
                SwPartParamsChangeWithNewName(nameDownPanel,
                    $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}",
                    new[,]
                    {
                        {"D1@Эскиз1", pType == "04" || pType == "05"
                                ? Convert.ToString(ширинаПанели - 42)
                                : Convert.ToString(ширинаПанели - 40)},

                        {"D2@Эскиз1", pType == "04" || pType == "05"
                                ? Convert.ToString(высотаПанели - 42)
                                : Convert.ToString(высотаПанели - 40)},

                        {"D1@1-3", Convert.ToString(screwsByWidth)},
                        {"D1@Кривая6", Convert.ToString(screwsByHeight)},

                        {"D1@1-4", Convert.ToString(колСаморезВинтВысота)},

                        {"D1@Кривая5", Convert.ToString(screwsByWidthInner)},

                        {"D1@Кривая4", Convert.ToString(screwsByHeightInner)},

                        {"D2@Эскиз32", pType == "01" || pType == "35"
                                ? Convert.ToString(77.5)
                                : Convert.ToString(158.1)},

                        {"D4@Эскиз47", Convert.ToString(растояниеМеждуРучками)},

                        {"D1@Эскиз38", Convert.ToString(диамСаморезВинт)},
                        {"D3@1-1-1", Convert.ToString(диамСаморезВинт)},

                        {"D1@Эскиз40", string.IsNullOrEmpty(типТорцевой) || pType == "01"
                                ? Convert.ToString(15)
                                : Convert.ToString(138.1)},

                        {"D2@1-2", Convert.ToString(осьОтверстийСаморезВинт)},

                        {"D1@2-3", Convert.ToString(zaklByWidth)},
                        {"D1@Кривая1", Convert.ToString(zaklByWidth)},

                        {"D1@Кривая2", Convert.ToString(zaklByHeight)},

                        {"D3@2-1-1", pType == "04" || pType == "05"
                                ? Convert.ToString(54.0)
                                : Convert.ToString(55.0)},

                        {"D2@Эскиз29", pType == "04" || pType == "05"
                                ? Convert.ToString(84.0)
                                : Convert.ToString(85.0)},

                        {"D2@Эскиз43", pType == "04" || pType == "05"
                                ? Convert.ToString(12.0)
                                : Convert.ToString(11.0)},

                        {"D1@Эскиз29", pType == "04" || pType == "05"
                                ? Convert.ToString(11.3)
                                : Convert.ToString(10.3)},

                        {"D1@2-1-1", pType == "04" || pType == "05"
                                ? Convert.ToString(11.3)
                                : Convert.ToString(10.3)},

                        {"D2@Эскиз39", pType == "04" || pType == "05"
                                ? Convert.ToString(11.3)
                                : Convert.ToString(10.3)},

                        {"D1@Эскиз39", pType == "04" || pType == "05"
                                ? Convert.ToString(5.0)
                                : Convert.ToString(4.0)},

                        //Рамка усиливающая
                        {"D1@Кривая9", pType == "01" || pType == "35"
                                ? Convert.ToString(колСаморезВинтШирина - 1000)
                                : Convert.ToString(колСаморезВинтШирина)},

                        {"D1@Кривая7", Convert.ToString(колЗаклепокВысота)},

                        {"D3@Эскиз56", Convert.ToString(отступОтветныхОтверстийШирина)},

                        //Размеры для отверсти под клепальные гайки под съемные панели
                        {"G0@Эскиз49", Convert.ToString(OutValPanels.G0)},
                        {"G1@Эскиз49", Convert.ToString(OutValPanels.G1)},
                        {"G2@Эскиз49", Convert.ToString(OutValPanels.G2)},
                        {"G3@Эскиз49", Convert.ToString(OutValPanels.G0)},

                        //Convert.ToString(количествоВинтов)
                        {"L1@Эскиз49", Convert.ToString(OutValPanels.L1)},
                        {"D1@Кривая10", Convert.ToString(OutValPanels.D1)},
                        {"L2@Эскиз49", Convert.ToString(OutValPanels.L2)},
                        {"D1@Кривая11", Convert.ToString(OutValPanels.D2)},
                        {"L3@Эскиз49", Convert.ToString(OutValPanels.L3)},
                        {"D1@Кривая12", Convert.ToString(OutValPanels.D3)},

                        //Размеры промежуточных профилей
                        {"Wp1@Эскиз59", Math.Abs(ValProfils.Wp1) < 1 ? "10" : Convert.ToString(ValProfils.Wp1)},
                        {"Wp2@Эскиз59", Math.Abs(ValProfils.Wp2) < 1 ? "10" : Convert.ToString(ValProfils.Wp2)},
                        {"Wp3@Эскиз59", Math.Abs(ValProfils.Wp3) < 1 ? "10" : Convert.ToString(ValProfils.Wp3)},
                        {"Wp4@Эскиз59", Math.Abs(ValProfils.Wp4) < 1 ? "10" : Convert.ToString(ValProfils.Wp4)},

                        //todo Для промежуточной панели отверстия
                        {"D1@Кривая14", Convert.ToString(колЗаклепокВысота*2)},

                        {"Толщина@Листовой металл", materialP2[1].Replace('.', ',')},

                        // Кол-во отверстий под заклепки сшивочных кронштейнов
                        {"D1@CrvPatternW", Convert.ToString(колЗаклепокКронштейнаДвойнойПанели)},
                        {"D1@CrvPatternH", Convert.ToString(колЗаклепокКронштейнаДвойнойПанели)}
                    },
                    true,
                    типДвойнойНижней != "0"
                        ? new[]
                        {
                            $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{имяДвойнойНижней1}",
                            $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{имяДвойнойНижней2}"
                        }
                        : null);

                VentsMatdll(materialP2, new[] { покрытие[7], покрытие[4], покрытие[5] }, newName);
                
                _swApp.CloseDoc(newName);
            }

            #endregion

            #region Усиливающие рамки

            if (усиление)
            {
                const string thiknessF = "1";
                var bendParams = sbSqlBaseData.BendTable(thiknessF);
                var bendRadius = Convert.ToDouble(bendParams[0]);
                var kFactor = Convert.ToDouble(bendParams[1]);

                const double heightF = 38.0;

                #region  Усиливающая рамка по ширине
                
                newName = усиливающаяРамкаПоШирине.NewName;
                //newName = modelName + "-06-" + width + "-" + "40-" + materialP2[0] + скотч;
                newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}.SLDPRT";

                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1)) 
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-11-06-40--1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("02-11-06-40-.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("02-11-06-40-",
                        $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D2@Эскиз1", pType == "04" || pType == "05" ? Convert.ToString(ширинаПанели-49.2) : Convert.ToString(ширинаПанели-47.2)},
                            
                                {"D1@Эскиз1", Convert.ToString(heightF)},
                            
                                {"D1@Кривая3", Convert.ToString(screwsByWidthInner)},  
                                {"D1@Кривая2", Convert.ToString(колСаморезВинтШирина)},
                                
                                
                                //Размеры для отверсти под клепальные гайки под съемные панели
                                {"G0@Эскиз32", Convert.ToString(OutValPanels.G0-3.6)},
                                {"G1@Эскиз32", Convert.ToString(OutValPanels.G1)},
                                {"G2@Эскиз32", Convert.ToString(OutValPanels.G2)},
                                {"G3@Эскиз32", Convert.ToString(OutValPanels.G0)},
                                 
                                //Convert.ToString(количествоВинтов)
                                {"L1@Эскиз32", Convert.ToString(OutValPanels.L1)},
                                {"D1@Кривая4", Convert.ToString(OutValPanels.D1)},
                                {"L2@Эскиз32", Convert.ToString(OutValPanels.L2)},
                                {"D1@Кривая5", Convert.ToString(OutValPanels.D2)},
                                {"L3@Эскиз32", Convert.ToString(OutValPanels.L3)},
                                {"D1@Кривая6", Convert.ToString(OutValPanels.D3)},
                                
                                {"Толщина@Листовой металл", thiknessF},     
                                {"D1@Листовой металл", Convert.ToString(bendRadius)},
                                {"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                            },
                        true,
                        null);
                    _swApp.CloseDoc(newName);
                }

                #endregion

                #region  Усиливающая рамка по ширине 2

                if (pType == "01")
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-11-06_2-40--4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents($@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{усиливающаяРамкаПоШирине.NewName}.SLDPRT", "", true, true);
                    _swApp.CloseDoc("02-11-06_2-40-.SLDPRT");
                }
                else
                {
                    newName = усиливающаяРамкаПоШирине2.NewName;
                    //newName = modelName + "-06-" + width + "-" + "40-" + materialP2[0] + скотч;
                    newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}.SLDPRT";

                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("02-11-06_2-40--4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("02-11-06_2-40-.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("02-11-06_2-40-",
                            $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D2@Эскиз1",
                                    pType == "04" || pType == "05"
                                    ? Convert.ToString(ширинаПанели - 49.2)
                                    : Convert.ToString(ширинаПанели - 47.2)},

                                {"D1@Эскиз1", Convert.ToString(heightF)},

                                {"D1@Кривая3", Convert.ToString(screwsByWidthInner)},
                                {"D1@Кривая2", Convert.ToString(колСаморезВинтШирина)},
                                
                                //Размеры для отверсти под клепальные гайки под съемные панели
                                {"G0@Эскиз32", Convert.ToString(OutValPanels.G0 - 3.6)},
                                {"G1@Эскиз32", Convert.ToString(OutValPanels.G1)},
                                {"G2@Эскиз32", Convert.ToString(OutValPanels.G2)},
                                {"G3@Эскиз32", Convert.ToString(OutValPanels.G0)},

                                //Convert.ToString(количествоВинтов)
                                {"L1@Эскиз32", Convert.ToString(OutValPanels.L1)},
                                {"D1@Кривая4", Convert.ToString(OutValPanels.D1)},
                                {"L2@Эскиз32", Convert.ToString(OutValPanels.L2)},
                                {"D1@Кривая5", Convert.ToString(OutValPanels.D2)},
                                {"L3@Эскиз32", Convert.ToString(OutValPanels.L3)},
                                {"D1@Кривая6", Convert.ToString(OutValPanels.D3)},

                                {"Толщина@Листовой металл", thiknessF},
                                {"D1@Листовой металл", Convert.ToString(bendRadius)},
                                {"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                            },
                            true,
                            null);
                        _swApp.CloseDoc(newName);
                    }
                }

                #endregion

                #region  Усиливающая рамка по высоте

                newName = усиливающаяРамкаПоВысоте.NewName;

                //newName = modelName + "-07-" + lenght + "-" + "40-" + materialP2[0] + скотч;
                newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}.SLDPRT";

                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-11-07-40--1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("02-11-07-40-.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("02-11-07-40-",
                        $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}",
                            new[,]
                            {
                                // Габарит
                                {"D3@Эскиз1", pType == "04" || pType == "05" ? Convert.ToString(высотаПанели-2) : Convert.ToString(высотаПанели)},
                                {"D1@Эскиз1", Convert.ToString(heightF)},
                                // Отверстия
                                {"D1@Эскиз23", pType == "01" ? Convert.ToString(44.4) : Convert.ToString(125)},

                                {"D1@Кривая2", Convert.ToString(screwsByHeightInner)},
                                {"D1@Кривая1", Convert.ToString(колЗаклепокВысота)},
                                // Х-ки листа
                                {"Толщина@Листовой металл", thiknessF},
                                {"D1@Листовой металл", Convert.ToString(bendRadius)},
                                {"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                            },
                            true,
                            null);
                    _swApp.CloseDoc(newName);
                }
                #endregion
            }

            #endregion

            #region Теплоизоляция

            #region наименование теплоизоляции

            //6700  Лента уплотнительная Pes20x3/25 A/AT-B
            //14800  Лента двохсторонняя акриловая HSA 19х2
            //4900  Материал теплоизол. Сlassik TWIN50
                        
            //newName = modelName + "-03-" + width + "-" + lenght + "-" + "40";

            #endregion

            newName = теплоизоляция.NewName;            
            newPartPath = $@"{Settings.Default.DestinationFolder}\{Panels0201}\Materials\{newName}.SLDPRT";
            if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
            {
                swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                swDoc.Extension.SelectByID2("02-11-03-40--1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", false, true);
                _swApp.CloseDoc("02-11-03-40-.SLDPRT");
            }
            else
            {
                SwPartParamsChangeWithNewName("02-11-03-40-",
                    $@"{Settings.Default.DestinationFolder}\{Panels0201}\Materials\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(высотаПанели-1)},
                            {"D1@Эскиз1", Convert.ToString(ширинаПанели-2)}
                        }, 
                        true,
                        null);
                _swApp.CloseDoc(newName);
            }

            #endregion

            #region Скотч

            const double rizn = 3;

            if (скотч)
            {
                //Скотч
                
                newName = cкотч.NewName;
                newPartPath = $@"{Settings.Default.DestinationFolder}\{Panels0201}\Materials\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-11-04-40--1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-11-04-40-.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("02-11-04-40-",
                        $@"{Settings.Default.DestinationFolder}\{Panels0201}\Materials\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(высотаПанели - rizn)},
                            {"D1@Эскиз1", Convert.ToString(ширинаПанели - rizn)}
                        },
                        true,
                        null);
                    _swApp.CloseDoc(newName);
                }
            }

            #endregion

            #region  Pes 20x3/25 A/AT-BT 538x768
            
            newName = pes.NewName;
            newPartPath = $@"{Settings.Default.DestinationFolder}\{Panels0201}\Materials\{newName}.SLDPRT";

            if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1)) 
            {
                swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                swDoc.Extension.SelectByID2("02-11-05-40--1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", false, true);
                _swApp.CloseDoc("02-11-05-40-.SLDPRT");
            }
            else
            {
                SwPartParamsChangeWithNewName("02-11-05-40-",
                    $@"{Settings.Default.DestinationFolder}\{Panels0201}\Materials\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(высотаПанели - rizn)},
                            {"D1@Эскиз1", Convert.ToString(ширинаПанели - rizn)}
                        },
                        true,
                        null);
                _swApp.CloseDoc(newName);
            }

            #endregion

            #region Кронштейн усиливающей панели
            
            if (типКрепежнойЧастиУсиливающейПанели == "D")
            {
                if (кронштейнДверной == null) goto m1;
                newName = кронштейнДверной.NewName;
                //newName = "02-11-09-40-" + lenght;
                newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}.SLDPRT";

                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-11-09-40--1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-11-09-40-.SLDPRT");
                }
                else 
                {
                    SwPartParamsChangeWithNewName("02-11-09-40-",
                        $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{newName}",
                            new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(высотаПанели - 45)},
                            {"D1@Эскиз1", скотч ? Convert.ToString(16.0) : Convert.ToString(17.5)},
                            {"D1@Кривая1", Convert.ToString(колЗаклепокВысота)}  
                        },
                        true,
                        null);
                    _swApp.CloseDoc(newName);
                }
            }

            m1:

            #endregion

            #region Разрезные части

            if (!string.IsNullOrEmpty(типДвойнойРазрез))
            {
                #region to delete

                //var имяДвойнойВерхней1 = панельВнешняя.NewName + "-" + типДвойнойВерхней + "1";
                //var имяДвойнойВерхней2 = панельВнешняя.NewName + "-" + типДвойнойВерхней + "2";
                //var имяДвойнойНижней1 = панельВнутренняя.NewName + "-" + типДвойнойНижней + "1";
                //var имяДвойнойНижней2 = панельВнутренняя.NewName + "-" + типДвойнойНижней + "2";

                //MessageBox.Show("имяДвойнойВерхней1 - " + имяДвойнойВерхней1 + "\nимяДвойнойВерхней2 - " +
                //                имяДвойнойВерхней2 + "\nимяДвойнойНижней1 - " + имяДвойнойНижней1 +
                //                "\nимяДвойнойНижней2 - " + имяДвойнойНижней1);

                #endregion

                if (типДвойнойВерхней != "0")
                {
                    partsToDeleteList.Add(outerPanel);

                    newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{имяДвойнойВерхней1}.SLDPRT";
                    swDoc.Extension.SelectByID2(панельВнешняя.NewName + "-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{имяДвойнойВерхней2}.SLDPRT";
                    swDoc.Extension.SelectByID2(панельВнешняя.NewName + "-3@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);                    
                }

                if (типДвойнойНижней != "0")
                {
                    partsToDeleteList.Add(innerPanel);

                    newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{имяДвойнойНижней1}.SLDPRT";
                    swDoc.Extension.SelectByID2(панельВнутренняя.NewName + "-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    newPartPath = $@"{Settings.Default.DestinationFolder}\{_destinationFolder}\{имяДвойнойНижней2}.SLDPRT";
                    swDoc.Extension.SelectByID2(панельВнутренняя.NewName + "-3@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                }

                switch (типДвойнойВерхней)
                {
                    case "1":
                        swDoc.Extension.SelectByID2("Cut-ExtrudeW1@" + имяДвойнойВерхней1 + "-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        swDoc.Extension.SelectByID2("Cut-ExtrudeW2@" + имяДвойнойВерхней2 + "-3@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        swDocExt.SelectByID2("Rivet Bralo-185@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        break;

                    case "2":
                        swDoc.Extension.SelectByID2("Cut-ExtrudeH1@" + имяДвойнойВерхней1 + "-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        swDoc.Extension.SelectByID2("Cut-ExtrudeH2@" + имяДвойнойВерхней2 + "-3@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        swDocExt.SelectByID2("Rivet Bralo-186@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        break;

                    case "0":                        
                        swDocExt.SelectByID2(панельВнешняя.NewName + "-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDocExt.SelectByID2(панельВнешняя.NewName + "-3@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        
                        swDocExt.SelectByID2("Rivet Bralo-185@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        swDocExt.SelectByID2("Rivet Bralo-186@" + nameAsm, "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        break;
                }

                switch (типДвойнойНижней)
                {
                    case "1":
                        swDoc.Extension.SelectByID2("Cut-ExtrudeW1@" + имяДвойнойНижней1 + "-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        swDoc.Extension.SelectByID2("Cut-ExtrudeW2@" + имяДвойнойНижней2 + "-3@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        break;

                    case "2":
                        swDoc.Extension.SelectByID2("Cut-ExtrudeH1@" + имяДвойнойНижней1 + "-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        swDoc.Extension.SelectByID2("Cut-ExtrudeH2@" + имяДвойнойНижней2 + "-3@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        break;

                    case "0":
                        swDocExt.SelectByID2(панельВнутренняя.NewName + "-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDocExt.SelectByID2(панельВнутренняя.NewName + "-3@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        break;
                }

                swDoc.Extension.SelectByID2("D1@PLANE1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@PLANE1"))).SystemValue = 40.0 / 1000; swDoc.EditRebuild3();

                foreach (var component in new[] {"4", "5", "6", "7", "8", "9", "10", "11", "12", "13"})
                {
                    swDoc.Extension.SelectByID2("DerivedCrvPattern" + component + "@" + nameAsm, "COMPPATTERN", 0, 0, 0, true, 0, null, 0); swAsm.DissolveComponentPattern();
                }
            }

            #endregion

            #endregion

            #region Задание имени сборки (description Наименование)

            switch (pType)
            {
                case "Несъемная":
                case "Съемная":
                    pType = pType + " панель";
                    break;
            }

            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm, true, 0)));
            GabaritsForPaintingCamera(swDoc);

            #endregion

            #region Сохранение и регистрация сборки в базе

            swDoc.EditRebuild3();
            swDoc.ForceRebuild3(true);
            swDoc.SaveAs2(newFramelessPanelPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                                   
            NewComponentsFull.Add(new VentsCadFile
            {
                LocalPartFileInfo = new FileInfo(newFramelessPanelPath).FullName,
                PartIdSql = idAsm
            });

            try
            {
                _swApp.CloseDoc(new FileInfo(newFramelessPanelPath).Name);
            }
            catch (Exception)
            {
                //
            }

            List<VentsCadFile> outList;

            CheckInOutPdmNew(NewComponentsFull, true, out outList);
            
            foreach (var item in outList)
            {
                try
                {
                    var typeFile = 0;
                    if (item.LocalPartFileInfo.ToUpper().Contains(".SLDASM")) { typeFile = 2; }
                    if (item.LocalPartFileInfo.ToUpper().Contains(".SLDPRT")) { typeFile = 1; }

                    //MessageBox.Show("typeFile - " + typeFile + "\n PartIdPdm - " + item.PartIdPdm + "\n PartIdSql - " + item.PartIdSql);

                    if (item.PartIdPdm != 0)
                    {
                        sqlBaseData.AirVents_SetPDMID(typeFile, item.PartIdPdm, item.PartIdSql);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "AirVents_SetPDMID");
                }
            }

            #region Auto Export to XML

            foreach (var newComponent in NewComponents)
            {
                //MessageBox.Show(newComponent.Name);
                // todo open for users
                //PartInfoToXml(newComponent.FullName);
            }

            #endregion

            #endregion

            return newFramelessPanelPath;
        }

        

        /// <summary>
        /// 
        /// </summary>
        public class Screws
        {
            /// <summary>
            /// 
            /// </summary>
            public double ByHeight { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ByWidth { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ByHeightInnerUp { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ByWidthInnerUp { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ByHeightInner { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ByWidthInner { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class PartsToDelete
        {
            /// <summary>
            /// 
            /// </summary>
            public List<string> PartsToDeleteList { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string PartName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string AsmName { get; set; }
        }


        /// <summary>
        /// 
        /// </summary>
        public class Ironwares
        {
            #region Block Dimensions (W, H, L)
            /// <summary>
            /// 
            /// </summary>
            public double Width { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Height { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Lenght { get; set; }

            #endregion

            #region PanelsTypes (W, H, S) - W - разрез вдоль, H - разрез поперек, S - стандартная
           
            /// <summary>
            /// 
            /// </summary>
            public string TypeOfUpPanel { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string TypeOfDownPanel { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string TypeOfUnremPanel { get; set; }

            #endregion

            #region ScrewsNumbers by Dimensions

            /// <summary>
            /// 
            /// </summary>
            public double ScrewsByWidth =>
                TypeDoublePanel(TypeOfUpPanel) == "H" || TypeDoublePanel(TypeOfDownPanel) == "H" ?
                Convert.ToInt32((КолСаморезВинт(Width) / 1000) % 2) != 0 ?
                КолСаморезВинт(Width) + 1000 : КолСаморезВинт(Width) : 
                КолСаморезВинт(Width);

            /// <summary>
            ///  
            /// </summary>
            public double ScrewsByHeight =>
                TypeDoublePanel(TypeOfUnremPanel) == "H" ?
                Convert.ToInt32((КолСаморезВинт(Height) / 1000) % 2) != 0 ?
                КолСаморезВинт(Height) + 1000 : КолСаморезВинт(Height) : 
                КолСаморезВинт(Height);

            /// <summary>
            ///  </summary>
            public double ScrewsByLenght =>
                TypeDoublePanel(TypeOfUpPanel) == "W" || TypeDoublePanel(TypeOfDownPanel) == "W" || TypeDoublePanel(TypeOfUnremPanel) == "W" ?
                Convert.ToInt32((КолСаморезВинт(Lenght) / 1000) % 2) != 0  ?
                КолСаморезВинт(Lenght) + 1000 : КолСаморезВинт(Lenght) :
                КолСаморезВинт(Lenght);

            private static string TypeDoublePanel(string typeString)
                => typeString.Contains("2") ? "H" : typeString.Contains("1") ? "W" : "S";

            private static double ШагСаморезВинт(double gabaritSize) => (gabaritSize < 600) ? 150 : 200;

            private static double КолСаморезВинт(double gabaritSize)
                => (Math.Truncate(gabaritSize / ШагСаморезВинт(gabaritSize)) + 1) * 1000;

            #endregion

            /// <summary>
            /// 
            /// </summary>
            public void ViewParams() =>
                MessageBox.Show(
                    $@"Width - {Width},
                    Height - {Height},
                    Lenght - {Lenght} 
                    TypeOfUpPanel - {TypeOfUpPanel}, 
                    TypeOfDownPanel - {TypeOfDownPanel}, 
                    TypeOfUnremPanel - {TypeOfUnremPanel}, 
                    ScrewsByWidth - {ScrewsByWidth},
                    ScrewsByHeight - {ScrewsByHeight},
                    ScrewsByLenght - {ScrewsByLenght}");}


        /// <summary>
        /// 
        /// </summary>
        public class OutParameters
        {
            /// <summary>
            /// 
            /// </summary>
            public double ScrewsByWidth { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ScrewsByHeight { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public Screws Screws { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public PartsToDelete PartsToDelete { get; set; }
    }

        /// <summary>
        /// Profils the specified lenght.
        /// </summary>
        /// <param name="height">The lenght.</param>
        /// <param name="type">The type.</param>
        /// <param name="колЗаклепокВысота"></param>
        /// <returns></returns>
        public string Profil(double height, string type, double? колЗаклепокВысота)
        {
            const string modelName = "02-11-08";
            double width;
            string config;

            switch (type)
            {
                case "00":
                    config = "00";
                    width = 39;
                    break;
                case "01":
                    config = "01";
                    width = 39;
                    break;
                case "02":
                    config = "02";
                    width = 40;
                    break;
                case "03":
                    config = "03";
                    width = 35;
                    break;
                default:
                    config = "00";
                    width = 39;
                    break;
            }

            if (type == "-" || type == "05") return null;

            var newName = modelName + "-" + Convert.ToString(height - 40) + "-" + type;
            
            var newPartPath = $@"{Settings.Default.DestinationFolder}\{Profil021108Destination}\{newName}.SLDPRT";

            if (!InitializeSw(true)) return null;

            if (File.Exists(newPartPath)){return newPartPath;}

            var pdmFolder = Settings.Default.SourceFolder;

            var components = new[]{$@"{pdmFolder}{Profil021108}\{"02-11-08-40-.SLDPRT"}"};

            GetLastVersionPdm(components, Settings.Default.PdmBaseName);

            var fileName = $@"{Settings.Default.SourceFolder}{Profil021108}\{"02-11-08-40-"}.SLDPRT";

            var swDoc = _swApp.OpenDoc6(fileName, (int)swDocumentTypes_e.swDocPART,
                (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);

            swDoc.ShowConfiguration2(config);
            
            string[] configs = swDoc.GetConfigurationNames();

            foreach (var s in configs)
            {
                try
                {
                    swDoc.DeleteConfiguration2(s);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }

            swDoc.ConfigurationManager.ActiveConfiguration.Name = "00";

            #region Погашения

            if (height < 750)
            {
                swDoc.Extension.SelectByID2("Вырез-Вытянуть5", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSuppress2();
            }
                
            #endregion

            if (колЗаклепокВысота == null)
            {
                колЗаклепокВысота = (Math.Truncate((height + 38) / 125) + 1) * 1000;
            }

            SwPartParamsChangeWithNewName("02-11-08-40-",
                $@"{Settings.Default.DestinationFolder}\{Profil021108Destination}\{newName}",
                new[,]
                {
                    // Габариты
                    {"D1@Эскиз1", Convert.ToString(width)},
                    {"D2@Эскиз1", Convert.ToString(height)},
                    {"D1@Кривая1", Convert.ToString(колЗаклепокВысота)}
                },
                true,
                null);
            _swApp.CloseDoc(newName);
            _swApp.ExitApp();
            //MessageBox.Show("Check in - " + newPartPath);
            CheckInOutPdm(new FileInfo(newPartPath).FullName, true, Settings.Default.TestPdmBaseName);
            //MessageBox.Show("Check in end - " + newPartPath);

            return newPartPath;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lenght"></param>
        /// <param name="соСкотчем"></param>
        /// <param name="type"></param>
        /// <param name="flange30"></param>
        /// <param name="newName1"></param>
        /// <returns></returns>
        public string FrameProfil(double lenght, bool соСкотчем, string type, bool flange30, string newName1)
        {
            //const string modelName = "02-11-11";
            //newName = modelName + "-" + Convert.ToString(lenght) + "-" + type;

            var width = соСкотчем ? 38.5 : 40.0;
            
            string config;

            switch (type)
            {
                case "00":
                    config = "00";
                    break;
                case "01":
                    config = "01";
                    break;
                default:
                    config = "00";
                    break;
            }

            var newPartPath = $@"{Settings.Default.DestinationFolder}\{Profil021108Destination}\{newName1}.SLDPRT";

            //MessageBox.Show(newPartPath, "newPartPath");

            if (!InitializeSw(true)) return null;

            if (File.Exists(newPartPath))
            {
                return newPartPath;
            }

            var pdmFolder = Settings.Default.SourceFolder;

            var components = new[]
            {
                $@"{pdmFolder}{Profil021108Destination}\{"02-11-11-40-.SLDPRT"}"
            };
            GetLastVersionPdm(components, Settings.Default.PdmBaseName);

            var fileName = $@"{Settings.Default.SourceFolder}{Profil021108}\{"02-11-11-40-"}.SLDPRT";

            var swDoc = _swApp.OpenDoc6(fileName, (int)swDocumentTypes_e.swDocPART,
                (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);

            swDoc.ShowConfiguration2(config);

            string[] configs = swDoc.GetConfigurationNames();
            foreach (var s in configs)
            {
                try
                {
                    swDoc.DeleteConfiguration2(s);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "FrameProfil");
                }
            }
            swDoc.ConfigurationManager.ActiveConfiguration.Name = "00";

            var lenght2 = type == "00" ? lenght-60 : lenght;

            var zakl = Convert.ToInt32(Math.Truncate(lenght2 / 100)*1000);
            
            SwPartParamsChangeWithNewName("02-11-11-40-",
                $@"{Settings.Default.DestinationFolder}\{Profil021108Destination}\{newName1}",
                new[,]
                {
                    // Габариты
                    
                    {"D2@Эскиз1", Convert.ToString(lenght)},
                    
                    {"D1@Кривая1", Convert.ToString(zakl == 1000 ? 2000 : zakl)},

                    {"D2@Эскиз23", (!flange30) ? Convert.ToString(10.0) : Convert.ToString(15.0)},
                    {"D3@Эскиз23", (!flange30) ? Convert.ToString(20.0) : Convert.ToString(10.0)}
                },
                true,
                null);
            _swApp.CloseDoc(newName1);
            
            return newPartPath;
        }

        #endregion

        #region Naming of AddingPanel

        /// <summary>
        /// Gets or sets the adding panels.
        /// </summary>
        /// <value>
        /// The adding panels.
        /// </value>
        public List<AddingPanel> AddingPanels = new List<AddingPanel>(); 

        /// <summary>
        /// 
        /// </summary>
        public class AddingPanel : FramelessPanelVault
        {

            /// <summary>
            /// 
            /// </summary>
            public string PartQuery { get; set; }

            /// <summary>
            /// Adds the part.
            /// </summary>
            public int AddPart()
            {
                var id = 0;
                try
                {
                    Логгер.Отладка("Добавление панели: " + Name, "", "AddPart", "AddingPanel");
                    id = AirVents_AddPartOfPanel();
                    Логгер.Отладка("Добавление панели прошло успешно!", "", "AddPart", "AddingPanel");
                }
                catch (Exception e)
                {
                    Логгер.Ошибка("Ошибка во время добавления панели:" + e.Message, e.StackTrace, "Add", "AddingPanel");
                }
                return id;
            }

            /// <summary>
            /// Adds this instance.
            /// </summary>
            public int Add()
            {
                var id = 0;
                try
                {
                    Логгер.Отладка("Добавление панели: " + Name, "", "Add", "AddingPanel");
                    id = AirVents_AddPanel();
                    Логгер.Отладка("Добавление панели прошло успешно!", "", "Add", "AddingPanel");
                }
                catch (Exception e)
                {
                    Логгер.Ошибка("Ошибка во время добавления панели:" + e.Message, e.StackTrace, "Add", "AddingPanel");
                }
                return id;
            }                   
        }
        
        // ReSharper disable once UnusedMember.Local
        class Dimensions : AddingPanel
        {
            protected List<Dimensions> DimensionsCollection => new List<Dimensions>();
        }

        /// <summary>
        /// Хранилище бескаркасных панелей
        /// </summary>
        public class FramelessPanelVault
        {
            /// <summary>
            /// Gets or sets the new name.
            /// </summary>
            /// <value>
            /// The new name.
            /// </value>
            public string NewName { get; set; }
            
            /// <summary>
            /// Gets or sets the part in asm ids.
            /// </summary>
            /// <value>
            /// The part in asm ids.
            /// </value>
            public List<int> PartInAsmIds { get; set; }

            /// <summary>
            /// Gets or sets the part identifier.
            /// </summary>
            /// <value>
            /// The part identifier.
            /// </value>
            public int PartId { get; set; }

            /// <summary>
            /// Имя типа детали
            /// </summary>
            /// <value>
            /// The name of the panel type.
            /// </value>
            public int PanelTypeId { get; set; }

            /// <summary>
            /// Тип детали в сборке
            /// </summary>
            /// <value>
            /// The type of the element.
            /// </value>
            public int ElementType { get; set; }

            /// <summary>
            /// Ширина панели
            /// </summary>
            /// <value>
            /// The width.
            /// </value>
            public int? Width { get; set; }

            /// <summary>
            /// Высота панели 
            /// </summary>
            /// <value>
            /// The lenght.
            /// </value>
            public int? Height { get; set; }

            /// <summary>
            /// Код материала детали
            /// </summary>
            /// <value>
            /// The panel mat.
            /// </value>
            public int? PartMat { get; set; }
            
            /// <summary>
            /// Gets or sets the part thick.
            /// </summary>
            /// <value>
            /// The part thick.
            /// </value>
            public int PartThick { get; set; }

            /// <summary>
            /// Толщина материала детали
            /// </summary>
            /// <value>
            /// The panel mat thick.
            /// </value>
            public double? PartMatThick { get; set; }

            /// <summary>
            /// Наличие усиления
            /// </summary>
            /// <value>
            /// The Reinforcing.
            /// </value>
            public bool Reinforcing { get; set; }
            
            /// <summary>
            /// Gets or sets the sticky tape.
            /// </summary>
            /// <value>
            /// The sticky tape.
            /// </value>
            public bool StickyTape { get; set; }
            
            /// <summary>
            /// Цвет покраски RAL
            /// </summary>
            /// <value>
            /// The ral.
            /// </value>
            public string Ral { get; set; }

            /// <summary>
            ///Тип покрытия
            /// </summary>
            /// <value>
            /// The type of the coating.
            /// </value>
            public string CoatingType { get; set; }

            /// <summary>
            /// Gets or sets the coating class.
            /// </summary>
            /// <value>
            /// The coating class.
            /// </value>
            public int? CoatingClass { get; set; }

            /// <summary>
            /// Зеркальность детали или сборки
            /// </summary>
            /// <value>
            /// The mirror.
            /// </value>
            public bool? Mirror { get; set; }

            /// <summary>
            /// Описание шагов дл ясъемных панелей
            /// </summary>
            /// <value>
            /// The step.
            /// </value>
            public string Step { get; set; }

            /// <summary>
            /// Описание шагов для вставок
            /// </summary>
            /// <value>
            /// The step insertion.
            /// </value>
            public string StepInsertion { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string AirHole { get; set; }            

            /// <summary>
            /// Gets or sets the coating type out.
            /// </summary>
            /// <value>
            /// The coating type out.
            /// </value>
            public string CoatingTypeOut { get; set; }

            /// <summary>
            /// Gets or sets the coating class out.
            /// </summary>
            /// <value>
            /// The coating class out.
            /// </value>
            public int? CoatingClassOut { get; set; }

            /// <summary>
            /// Gets or sets the coating type in.
            /// </summary>
            /// <value>
            /// The coating type in.
            /// </value>
            public string CoatingTypeIn { get; set; }

            /// <summary>
            /// Gets or sets the coating class in.
            /// </summary>
            /// <value>
            /// The coating class in.
            /// </value>
            public int? CoatingClassIn { get; set; }

            /// <summary>
            /// Gets or sets the ral out.
            /// </summary>
            /// <value>
            /// The ral out.
            /// </value>
            public string RalOut { get; set; }

            /// <summary>
            /// Gets or sets the ral in.
            /// </summary>
            /// <value>
            /// The ral in.
            /// </value>
            public string RalIn { get; set; }

            /// <summary>
            /// Gets or sets the part mat thick out.
            /// </summary>
            /// <value>
            /// The part mat thick out.
            /// </value>
            public double? PanelMatThickOut { get; set; }

            /// <summary>
            /// Gets or sets the part mat thick in.
            /// </summary>
            /// <value>
            /// The part mat thick in.
            /// </value>
            public double? PanelMatThickIn { get; set; }
            
            /// <summary>
            /// Gets or sets the panel thick.
            /// </summary>
            /// <value>
            /// The panel thick.
            /// </value>
            public int PanelThick { get; set; }

            /// <summary>
            /// Gets or sets the part mat out.
            /// </summary>
            /// <value>
            /// The part mat out.
            /// </value>
            public int? PanelMatOut { get; set; }

            /// <summary>
            /// Gets or sets the part mat in.
            /// </summary>
            /// <value>
            /// The part mat in.
            /// </value>
            public int? PanelMatIn { get; set; }
            
            /// <summary>
            /// Gets or sets the panel number.
            /// </summary>
            /// <value>
            /// The panel number.
            /// </value>
            public int PanelNumber { get; set; }
            
            /// <summary>
            /// Получение имени детали либо сборки
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name =>
                $"{"02-"}{(string.IsNullOrEmpty(Convert.ToString(PanelTypeId)) ? null : Convert.ToString(PanelTypeId))}{"-0" + ElementType}{(Width == null ? null : "-" + Width)}{(Height == null ? null : "-" + Height)}{(PanelThick == 0 ? null : "-" + PanelThick)}{(PartMat == null ? null : "-" + PartMat)}{(PartMatThick == null ? null : "-" + PartMatThick)}{(Reinforcing ? "-Y" : null)}{(string.IsNullOrEmpty(Ral) ? null : "-" + Ral)}{(string.IsNullOrEmpty(CoatingType) ? null : "-" + CoatingType)}{(CoatingClass == 0 ? null : "-" + CoatingClass)}{(Mirror == true ? "-M" : null)}{Step}{StepInsertion}";

            /// <summary>
            /// Gets the name by identifier.
            /// </summary>
            /// <value>
            /// The name by identifier.
            /// </value>
            public string NameById => $"{"02-00"}{PartId}{"-0" + ElementType}";

            /// <summary>
            /// The parts list
            /// </summary>
            public List<FramelessPanelVault> PartsParamsList;

            /// <summary>
            /// Airs the vents_ add panel full.
            /// </summary>
            public void AirVents_AddPanelFull()
            {
                try
                {
                    var sqlBaseData = new SqlBaseData();
                    sqlBaseData.AirVents_AddPanelFull(ConvertToDataTable(PartsParamsList));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace);
                }
            }

            /// <summary>
            /// Converts to data table.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="data">The data.</param>
            /// <returns></returns>
            public DataTable ConvertToDataTable<T>(IList<T> data)
            {
                var propertyDescriptorCollection =
                   TypeDescriptor.GetProperties(typeof(T));
                var table = new DataTable();
                foreach (PropertyDescriptor prop in propertyDescriptorCollection)
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                foreach (T item in data)
                {
                    var row = table.NewRow();
                    foreach (PropertyDescriptor prop in propertyDescriptorCollection)
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    table.Rows.Add(row);
                }
                return table;
            }

            /// <summary>
            /// Запрос на добавление детали бескаркасной панели
            /// </summary>
            protected int AirVents_AddPanel()
            {
                var id = 0;
                try
                {
                    var sqlBaseData = new SqlBaseData();
                    id = sqlBaseData.AirVents_AddPanel(
                        partId: PartId,
                        panelTypeId : PanelTypeId,
                        width : Width,
                        height : Height,
                        panelMatOut : PanelMatOut,
                        panelMatIn : PanelMatIn,
                        panelThick : PanelThick,

                        panelMatThickOut : PanelMatThickOut,
                        panelMatThickIn : PanelMatThickIn,

                        #region Покраска

                        //ralOut : RalOut,
                        //ralIn : RalIn,
                        //coatingTypeOut : CoatingTypeOut,
                        //coatingTypeIn : CoatingTypeIn,
                        //coatingClassOut : CoatingClassOut,
                        //coatingClassIn : CoatingClassIn

                        #endregion

                        mirror: Mirror.HasValue ? Mirror : false,
                        step : string.IsNullOrEmpty(Step) ? "0" : Step,
                        stepInsertion :  string.IsNullOrEmpty(StepInsertion) ? "0" : StepInsertion,
                        reinforcing01: Reinforcing,

                        stickyTape: StickyTape,

                        airHole: AirHole,
                        
                        panelNumber :PanelNumber);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace);
                }
                return id;
            }

            /// <summary>
            /// Airs the vents_ add part of panel.
            /// </summary>
            protected int AirVents_AddPartOfPanel()
            {
                var id = 0;
                try
                {
                    var sqlBaseData = new SqlBaseData();
                    id = sqlBaseData.AirVents_AddPartOfPanel
                        (
                        panelTypeId: PanelTypeId,
                        elementType: ElementType,
                        width: Width,
                        height: Height,
                        partThick: PartThick,
                        partMat: PartMat,
                        partMatThick: PartMatThick,
                        reinforcing: Reinforcing,
                        stickyTape: StickyTape,
                        #region Покраска
                        //ral: Ral,
                        //coatingType: CoatingType,
                        //coatingClass: CoatingClass
                        #endregion
                        mirror: Mirror.HasValue ? Mirror:false,
                        step: string.IsNullOrEmpty(Step) ? "0" : Step,
                        stepInsertion: string.IsNullOrEmpty(StepInsertion) ? "0" : StepInsertion,
                        airHole: AirHole
                        );
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace);
                }
                return id;
            }

            /// <summary>
            /// Airs the vents_ add panel asm.
            /// </summary>
            protected void AirVents_AddPanelAsm()
            {
                try
                {
                    var sqlBaseData = new SqlBaseData();
                    int partId;
                    sqlBaseData.AirVents_AddPanelAsm(
                        PartInAsmIds, 
                        out partId);
                    PartId = partId;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace);
                }
            }

        }

        #endregion

        #region Съемные панели и перегородки

        static void СъемнаяПанель(double посадочнаяШирина, double посадочнаяВысота,
                                    out double ширина, out double высота,
                                    out double расстояниеL, out double количествоВинтов)
        {
            ширина = посадочнаяШирина - 2;
            высота = посадочнаяВысота - 2;
            расстояниеL = посадочнаяШирина - 132;

            количествоВинтов = 5000;

            if (посадочнаяШирина < 1100)
            {
                количествоВинтов = 4000;
            }
            if (посадочнаяШирина < 700)
            {
                количествоВинтов = 3000;
            }
            if (посадочнаяШирина < 365)
            {
                количествоВинтов = 2000;
            }
        }


        /// <summary>
        /// Входящие параметры для определения отверстий верхней и нижней панелей
        /// </summary>
        struct InValPanels
        {
            //Зазоры
            static public double G0;
            static public double G1;
            static public double G2;

            //Панели
            static public double B1;
            static public double B2;
            static public double B3;

            /// <summary>
            /// Преобразование строки расположение панелей в значения необходимые для построения
            /// </summary>
            /// <param name="values">The values.</param>
            public static void StringValue(string values)
            {
                G0 = 0;
                G1 = 0;
                G2 = 0;
                B1 = 0;
                B2 = 0;
                B3 = 0;

                try
                {
                    var val = values.Split(';');
                    var lenght = val.Length;

                    if (lenght < 1) return;
                    if (val[0] == "") return;
                    G0 = Convert.ToDouble(val[0]);

                    if (lenght < 2) return;
                    if (val[1] == "") return;
                    B1 = Convert.ToDouble(val[1]);

                    if (lenght < 3) return;
                    if (val[2] == "") return;
                    G1 = Convert.ToDouble(val[2]);

                    if (lenght < 4) return;
                    if (val[3] == "") return;
                    B2 = Convert.ToDouble(val[3]);

                    if (lenght < 5) return;
                    if (val[4] == "") return;
                    G2 = Convert.ToDouble(val[4]);

                    if (lenght < 6) return;
                    if (val[5] == "") return;
                    B3 = Convert.ToDouble(val[5]);
                }
                catch (Exception e)
                {
                    MessageBox.Show("StringValue " + e.Message, "InValPanels.StringValue");
                }
                finally
                {
                    //MessageBox.Show(InValUpDown() + " ==  " + OutValPanels.OutValUpDown());
                    ВерхняяНижняяПанельОтверстияПодСъемную();
                    //MessageBox.Show(InValUpDown() + " ==  " + OutValPanels.OutValUpDown());
                }
            }

            public static string InValUpDown()
            {
                return
                    $"_({(Convert.ToInt32(G0) != 0 ? Convert.ToString(G0) : "")}" +
                    $"{(Convert.ToInt32(B1) != 0 ? "-" + Convert.ToString(B1) : "")}" +
                    $"{(Convert.ToInt32(G1) != 0 ? "_" + Convert.ToString(G1) : "")}" +
                    $"{(Convert.ToInt32(B2) != 0 ? "-" + Convert.ToString(B2) : "")}" +
                    $"{(Convert.ToInt32(G2) != 0 ? "_" + Convert.ToString(G2) : "")}" +
                    $"{(Convert.ToInt32(B3) != 0 ? "-" + Convert.ToString(B3) : "")})";
            }
        }

        /// <summary>
        /// Исходящие параметры для определения отверстий верхней и нижней панелей
        /// </summary>
        struct OutValPanels
        {
            //Зазоры
            static public double G0;
            static public double G1;
            static public double G2;
            // Панель1
            public static double L1;
            public static double D1;
            // Панель2
            static public double L2;
            public static double D2;
            // Панель3
            static public double L3;
            public static double D3;

            //public static string OutValUpDown()
            //{
            //    return String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
            //        Convert.ToInt32(G0) != 0 ? Convert.ToString(G0) : "",
            //        Convert.ToInt32(L1) != 0 ? ";" + Convert.ToString(L1) : "",
            //        Convert.ToInt32(D1) != 0 ? ";" + Convert.ToString(D1) : "",
            //        Convert.ToInt32(G1) != 0 ? ";" + Convert.ToString(G1) : "",
            //        Convert.ToInt32(L2) != 0 ? ";" + Convert.ToString(L2) : "",
            //        Convert.ToInt32(D2) != 0 ? ";" + Convert.ToString(D2) : "",
            //        Convert.ToInt32(G2) != 0 ? ";" + Convert.ToString(G2) : "",
            //        Convert.ToInt32(L3) != 0 ? ";" + Convert.ToString(L3) : "",
            //        Convert.ToInt32(D3) != 0 ? ";" + Convert.ToString(D3) : "");
            //}
        }

        static void ВерхняяНижняяПанельОтверстияПодСъемную()
        {
            #region Зазоры

            // Зазоры по умолчанию
            OutValPanels.G0 = 46;
            OutValPanels.G1 = 132;
            OutValPanels.G2 = 132;

            if (Math.Abs(InValPanels.G0) > 0)
            {
                OutValPanels.G0 = InValPanels.G0;
            }
            if (Math.Abs(InValPanels.G1) > 0)
            {
                OutValPanels.G1 = InValPanels.G1;
            }
            if (Math.Abs(InValPanels.G2) > 0)
            {
                OutValPanels.G2 = InValPanels.G2;
            }

            #endregion

            #region Отверстия под панель

            double ширина;
            double высота;
            double расстояниеL;
            double количествоВинтов;

            СъемнаяПанель(InValPanels.B1, 0, out ширина, out высота, out расстояниеL, out количествоВинтов);
            OutValPanels.L1 = расстояниеL;
            OutValPanels.D1 = количествоВинтов;

            OutValPanels.L2 = 28;
            OutValPanels.D2 = 2000;
            OutValPanels.L3 = 28;
            OutValPanels.D3 = 2000;

            if (Math.Abs(InValPanels.B2) > 0)
            {
                СъемнаяПанель(InValPanels.B2, 0, out ширина, out высота, out расстояниеL, out количествоВинтов);
                OutValPanels.L2 = расстояниеL;
                OutValPanels.D2 = количествоВинтов;
            }

            if (!(Math.Abs(InValPanels.B3) > 0)) return;
            СъемнаяПанель(InValPanels.B3, 0, out ширина, out высота, out расстояниеL, out количествоВинтов);
            OutValPanels.L3 = расстояниеL;
            OutValPanels.D3 = количествоВинтов;

            #endregion
        }

        struct ValProfils
        {
            //Посадочные размеры для пормежуточных профилей
            static public double Wp1;
            static public double Wp2;
            static public double Wp3;
            static public double Wp4;           

            //Типы промежуточных профилей
            static public string Tp1;
            static public string Tp2;
            static public string Tp3;
            static public string Tp4;

           //static public string PsTy1;
           // static public string PsTy2;

            public static void StringValue(string values)
            {
                try
                {
                    if (!string.IsNullOrEmpty(values))
                    {
                        var val = values.Split(';');

                        var p1 = val[0]?.Split('_');
                        Tp1 = p1[0];
                        double.TryParse(p1[1], out Wp1);

                        var p2 = val[1].Split('_');
                        Tp2 = p2[0];
                        double.TryParse(p2[1], out Wp2);

                        var p3 = val[2].Split('_');
                        Tp3 = p3[0];
                        double.TryParse(p3[1], out Wp3);

                        var p4 = val[3].Split('_');
                        Tp4 = p4[0];
                        double.TryParse(p4[1], out Wp4);                     
                    }

                    #region To Delete

                    //MessageBox.Show("Tp1 - " + Tp1 + " Wp1 - " + Wp1 +
                    //                "\nTp2 - " + Tp2 + " Wp2 - " + Wp2 +
                    //                "\nTp3 - " + Tp3 + " Wp3 - " + Wp3 +
                    //                "\nTp4 - " + Tp4 + " Wp4 - " + Wp4 );
                    
                    #endregion

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        struct BackProfils
        {
            //Поперечное сечение окна и смещения от центра
            public static double Width;
            public static double Height;
            public static double ByWidth;
            public static double ByHeight;
         
            public static bool Flange30;

            private static double _typeOfPanel;

            public static void StringValue(string values)
            {
                if ( string.IsNullOrEmpty(values)) return;
                try
                {
                    var val = values.Split(';');

                    var p1 = val[0].Split('_');

                    double.TryParse(p1[0], out Width);
                    double.TryParse(p1[1], out Height);
                    double.TryParse(p1[2], out ByWidth);
                    double.TryParse(p1[3], out ByHeight);
                    bool.TryParse(p1[4], out Flange30);
                    double.TryParse(p1[5], out _typeOfPanel);

                    // MessageBox.Show(p1[0] + "\n" + p1[1] + "\n" + p1[2] + "\n" + p1[3] + "\n" + p1[4] + " - " + Flange30 + "\n" + TypeOfPanel);
                }
                catch (Exception)
                {
                    //
                }
            }
        }

        #endregion

    }
}
