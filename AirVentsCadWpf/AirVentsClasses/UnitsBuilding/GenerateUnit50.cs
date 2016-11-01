using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Diagnostics;
using AirVentsCadWpf.Properties;
using AirVentsCadWpf.Логирование;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using VentsCadLibrary;
using VentsMaterials;


namespace AirVentsCadWpf.AirVentsClasses.UnitsBuilding
{

    /// <summary>
    /// Класс для генерации блоков и элементов установок в SolidWorks
    /// </summary>
    public partial class ModelSw : IDisposable
    {
        #region Static Methods

        public static void Open(int fileId, int projectId, string fileName)
        {
            if (fileId == 0 || projectId == 0) return;
            
            if (MessageBox.Show( string.IsNullOrEmpty(fileName) ? $"Открыть {fileName}?" : 
                "Изделие уже есть в базе. Открыть?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Process.Start("conisio://" + Settings.Default.TestPdmBaseName + "/open?projectid=" + projectId +
                    "&documentid=" + fileId + "&objecttype=1");
            }                        
        }

        #endregion

        private SldWorks _swApp;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partPath"></param>
        /// <returns></returns>
        public SldWorks GetSwWithPart(string partPath)
        {
            InitializeSw(true);
            _swApp?.OpenDoc6(partPath, (int)swDocumentTypes_e.swDocPART,
                    (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "", 0, 0);
            return _swApp;
        }
      
        /// <summary>
        /// Список сгенерированных файлов при построении данной модели.
        /// </summary>
        public List<FileInfo> NewComponents = new List<FileInfo>();
        
        #region Fields

        private const string Unit30FolderS = @"\Библиотека проектирования\DriveWorks\01 - Frame 30mm";
        private const string Unit50FolderS = @"\Библиотека проектирования\DriveWorks\01 - Frame 50mm";
        private const string Unit50FolderD = @"\Проекты\Blauberg\01-Frame";
        private const string Unit50Orders = @"\Заказы AirVents";

        private const string DublePanel50Folder = @"\Библиотека проектирования\DriveWorks\02 - Duble Panel";
        private const string Panel50Folder = @"\Библиотека проектирования\DriveWorks\02 - Panels";
        private const string Panel30Folder = @"\Библиотека проектирования\DriveWorks\02 - Panels 30mm";
        private const string Panel30RemovableFolder = @"\Библиотека проектирования\DriveWorks\02 - Removable Panel 30mm";
        private const string Panel30CoilFolder = @"\Библиотека проектирования\DriveWorks\02 - Coil Panel 30mm";
        private const string Panel30DoubleFolder = @"\Библиотека проектирования\DriveWorks\02 - Duble Panel 30mm";

        private const string Panel30RemovableDoubleFolder =
            @"\Библиотека проектирования\DriveWorks\02 - Removable Panel Duble 30mm";

        private string _destinationFolder = @"\Проекты\Blauberg\02 - Панели";

        private const string Panels0201 = @"\Проекты\Blauberg\02-01-Panels";
        private const string Panels0204 = @"\Проекты\Blauberg\02-04-Removable Panels";
        private const string Panels0205 = @"\Проекты\Blauberg\02-05-Coil Panels";

        private const string Profil021108 = @"\Библиотека проектирования\DriveWorks\01 - Frameless Design 40mm";
        private const string Profil021108Destination = @"\Проекты\Blauberg\02-01-Panels";

        /// <summary>
        /// Папка с исходной моделью "Рамы монтажной". 
        /// </summary>
        public string BaseFrameFolder = @"\Библиотека проектирования\DriveWorks\10 - Base Frame";

        /// <summary>
        /// Папка для сохранения компонентов "Рамы монтажной" 
        /// </summary>
        public string BaseFrameDestinationFolder = @"\Проекты\Blauberg\10 - Рама монтажная";

        #endregion

        #region UnitS50

        /// <summary>
        /// Units the S50.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="type">The type.</param>
        /// <param name="side">The side.</param>
        /// <param name="widthS">The width s.</param>
        /// <param name="heightS">The lenght s.</param>
        /// <param name="lenghtS">The lenght s.</param>
        /// <param name="montageFrame">The montage frame.</param>
        /// <param name="typeOfFrame">The type of frame.</param>
        /// <param name="frameOffset">The frame offset.</param>
        /// <param name="typeOfPanel">The type of panel.</param>
        /// <param name="materialP1">The material p1.</param>
        /// <param name="materialP2">The material p2.</param>
        /// <param name="roofType">Type of the roof.</param>
        /// <param name="dumper">The dumper.</param>
        /// <param name="spigot">The spigot.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="p4">The p4.</param>
        /// <param name="widthVb">The width vb.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="heater">The heater.</param>
        /// <param name="cooler">The cooler.</param>
        /// <param name="ventil">The ventil.</param>
        public void UnitS50(string size, string type, string side, string widthS, string heightS, string lenghtS,
            string montageFrame, string typeOfFrame, string frameOffset,
            string typeOfPanel, string[] materialP1, string[] materialP2, string roofType, string dumper, string spigot,
            string p1, string p2, string p3, string p4, string widthVb,
            string filter, string heater, string cooler, string ventil)
        {

            Логгер.Информация($"Начало генерации установки {$"{size}-{type}"}", "", null, "UnitS50");

            try
            {
                try
                {
                    _swApp = (SldWorks) Marshal.GetActiveObject("SldWorks.Application");
                }
                catch (Exception)
                {
                    _swApp = new SldWorks {Visible = true};
                }
                if (_swApp == null)
                {
                    return;
                }

                _swApp.CloseDoc(dumper.Replace("ASM", "DRW"));
                _swApp.CloseDoc(spigot.Replace("ASM", "DRW"));
                _swApp.CloseDoc(p1);
                _swApp.CloseDoc(p2);
                _swApp.CloseDoc(p3);
                _swApp.CloseDoc(p4);
                UnitS50Str(size, type, side, widthS, heightS, lenghtS, montageFrame, typeOfFrame, frameOffset,
                    typeOfPanel, materialP1, materialP2, roofType, dumper, spigot, p1, p2, p3, p4, widthVb,
                    filter, heater, cooler, ventil);
                foreach (var coponent in NewComponents)
                {
                    MessageBox.Show($"Компоненты, осзданные во время генерации  {$"{coponent.Name}"}");
                    Логгер.Информация($"Компоненты, осзданные во время генерации  {$"{coponent.Name}"}", "", null, "UnitS50");
                }
            }
            catch (Exception e)
            {
                Логгер.Ошибка($"Ошибка во время генерации блока {$"{size}-{type}-{lenghtS}"}, время - {e.Message}", e.StackTrace, null, "UnitS50");
            }
        }

        /// <summary>
        /// Units the S50 string.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="type">The type.</param>
        /// <param name="side">The side.</param>
        /// <param name="widthS">The width s.</param>
        /// <param name="heightS">The lenght s.</param>
        /// <param name="lenghtS">The lenght s.</param>
        /// <param name="thiknessFrame">The thikness frame.</param>
        /// <param name="typeOfFrame">The type of frame.</param>
        /// <param name="frameOffset">The frame offset.</param>
        /// <param name="typeOfPanel">The type of panel.</param>
        /// <param name="materialP1">The material p1.</param>
        /// <param name="materialP2">The material p2.</param>
        /// <param name="roofType">Type of the roof.</param>
        /// <param name="dumper">The dumper.</param>
        /// <param name="spigot">The spigot.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="p4">The p4.</param>
        /// <param name="widthVb">The width vb.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="heater">The heater.</param>
        /// <param name="cooler">The cooler.</param>
        /// <param name="ventil">The ventil.</param>
        /// <returns></returns>
        public string UnitS50Str(string size, string type, string side, string widthS, string heightS, string lenghtS,
            string thiknessFrame,
            string typeOfFrame, string frameOffset, string typeOfPanel, string[] materialP1, string[] materialP2,
            string roofType, string dumper,
            string spigot, string p1, string p2, string p3, string p4, string widthVb, string filter, string heater,
            string cooler, string ventil)
        {

            #region Проверка значений

            if (IsConvertToInt(new[] {widthS, heightS, lenghtS}) == false)
            {
                return "";
            }

            #endregion

            #region Basic Parameters

            // Габариты
            var width = GetInt(widthS);
            var height = GetInt(heightS);
            var lenght = GetInt(lenghtS);

            #endregion

            #region Start

            const string modelName = "01-001-50.SLDASM";
            var unitAsMmodel = $@"{Settings.Default.SourceFolder}{Unit50FolderS}\{modelName}";

            #region UnitName

            // Каркас 50
            var frame50 = $"{size}-{lenght}";
            if (size.EndsWith("N"))
            {
                frame50 = $"{size}-{width}-{height}-{lenght}";
            }

            var typeUnit = "";
            if (type != "Пустой")
            {
                typeUnit = $"-{type}";
            }

            #region Обозначение для панелей

            var panelType = "";
            if (!typeOfPanel.EndsWith("Съемная"))
            {
                panelType = $"{typeOfPanel}-";
            }

            var materialPanel = "";
            if (materialP1[0] != "" & typeOfPanel != "")
            {
                materialPanel = $"{materialP1}-{materialP2}";
            }

            var panels = $"{panelType}{materialPanel}";
            if (panels != "" & typeOfPanel != "")
            {
                panels = $"_({panels})";
            }
            if (typeOfPanel == "")
            {
                panels = "";
            }

            #endregion

            #region Сторона обслуживания

            var sideletter = "";
            if (side == "левая") // & typeOfPanel != "")
            {
                sideletter = "-L";
            }

            #endregion

            #region Обозначение для монтажной рамы

            var montageFrameType = "";
            if (typeOfFrame != "0")
            {
                montageFrameType = $"{typeOfFrame}-";
            }

            var frameOffsetmf = "";
            if (typeOfFrame == "3")
            {
                frameOffsetmf = $"-{frameOffset}";
            }

            var montFrame = $"_({montageFrameType}{thiknessFrame}{frameOffsetmf})";
            if (thiknessFrame == "")
            {
                montFrame = "";
            }

            #endregion

            #region  Обозначения для крыши

            var rooftype = "";
            if (roofType != "")
            {
                rooftype = "_(" + roofType + ")";
            }

            #endregion

            var newUnit50Name = $"MB-{frame50}{typeUnit}{sideletter}{panels}{montFrame}{rooftype}";

            #endregion

            var newUnit50Path = $@"{Settings.Default.DestinationFolder}{Unit50FolderD}\{newUnit50Name}.SLDASM";
            if (File.Exists(newUnit50Path))
            {
                GetLastVersionPdm(new FileInfo(newUnit50Path).FullName, Settings.Default.TestPdmBaseName);
                _swApp.OpenDoc6(newUnit50Path, (int) swDocumentTypes_e.swDocASSEMBLY,
                    (int) swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                return newUnit50Path;
            }

            var pdmFolder = Settings.Default.SourceFolder;
            var components = new[]
            {
                unitAsMmodel,
                $@"{pdmFolder}{@"Vents-PDM\Проекты\AirVents\AV09\ВНС-901.49.100 Блок вентилятора\"}{"ВНС-901.49.111.SLDPRT"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\01-Frame\"}{"01-P150-45-510.SLDPRT"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\01-Frame\"}{"01-P150-45-1640.SLDPRT"}",
                $@"{pdmFolder}{@"Vents-PDM\Библиотека проектирования\DriveWorks\01 - Frame 50mm\"}{"01-003-50.SLDPRT"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\01-Frame\"}{"01-P252-45-550.SLDPRT"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\01-Frame\"}{"01-P252-45-770.SLDPRT"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\02-04-Removable Panels\"}{"02-04-400-550-30-Az-Az-MW.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\02-01-Panels\"}{"02-04-350-550-50-Az-Az-MW.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\10 - Рама монтажная\"}{"10-2-1300-1150.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\15 - Крыша\"}{"15-01-770-1000.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\16 - Узел нагревателя водяного\"}{"16-AV04-3H.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\04 - Фильтры\"}{"04-01-AV04-G4-300.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\03 - Узлы блока вентагрегата\"}{"03-01-AV04-31.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\02-01-Panels\"}{"02-04-700-550-50-Az-Az-MW.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\03 - Узлы блока вентагрегата\"}{"03-02-35.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\11 - Регулятор расхода воздуха\"}{"11-800-500-150.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Проекты\Blauberg\12 - Вибровставка\"}{"12-800-500.SLDASM"}",
                $@"{pdmFolder}{@"Vents-PDM\Библиотека проектирования\Прочие изделия\Крепежные изделия\Замки и ручки\"}{"M8-Panel block-one side.SLDPRT"}",
                $@"{pdmFolder}{@"Vents-PDM\Библиотека проектирования\Стандартные изделия\"}{"Threaded Rivets с насечкой.SLDPRT"}"
            };
            GetLastVersionPdm(components, Settings.Default.PdmBaseName);
            var swDoc = _swApp.OpenDoc6(unitAsMmodel, (int) swDocumentTypes_e.swDocASSEMBLY,
                (int) swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            _swApp.Visible = true;
            var swAsm = (AssemblyDoc) swDoc;
            swAsm.ResolveAllLightWeightComponents(false);

            #endregion

            #region Frame

            const double step = 100;
            double rivetL;

            //Lenght

            var newName = "01-P150-45-" + (lenght - 140);
            var newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";
            if (File.Exists(new FileInfo(newPartPath).FullName))
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("01-P150-45-1640-27@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0,
                    0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("01-P150-45-1640.SLDPRT");
            }
            else if (File.Exists(newPartPath) != true)
            {
                rivetL = (Math.Truncate((lenght - 170)/step) + 1)*1000;
                SwPartParamsChangeWithNewName("01-P150-45-1640",
                    $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                    new[,]
                    {
                        {"D1@Вытянуть1", Convert.ToString(lenght - 140)},
                        {"D1@Кривая1", Convert.ToString(rivetL)}
                    },
                    false,
                    null);
                _swApp.CloseDoc(newName);
            }

            //Width

            newName = "01-P150-45-" + (width - 140);
            newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";
            if (File.Exists(new FileInfo(newPartPath).FullName))
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("01-003-50-22@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("01-003-50.SLDPRT");
            }
            else if (File.Exists(newPartPath) != true)
            {
                rivetL = (Math.Truncate((width - 170)/step) + 1)*1000;
                SwPartParamsChangeWithNewName("01-003-50",
                    $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                    new[,]
                    {
                        {"D1@Вытянуть1", Convert.ToString(width - 140)},
                        {"D1@Кривая1", Convert.ToString(rivetL)}
                    },
                    false,
                    null);
                _swApp.CloseDoc(newName);
            }

            //01-P252-45-770
            newName = "01-P252-45-" + (width - 100);
            newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";
            if (File.Exists(new FileInfo(newPartPath).FullName))
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("01-P252-45-770-6@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("01-P252-45-770.SLDPRT");
            }
            else if (File.Exists(newPartPath) != true)
            {
                SwPartParamsChangeWithNewName("01-P252-45-770",
                    $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                    new[,] {{"D1@Вытянуть1", Convert.ToString(width - 100)}},
                    false,
                    null);
                _swApp.CloseDoc(newName);
            }

            //Height

            newName = "01-P150-45-" + (height - 140);
            newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";
            if (File.Exists(new FileInfo(newPartPath).FullName))
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("01-P150-45-510-23@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0,
                    0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("01-P150-45-510.SLDPRT");
            }
            else if (File.Exists(newPartPath) != true)
            {
                rivetL = (Math.Truncate((height - 170)/step) + 1)*1000;
                SwPartParamsChangeWithNewName("01-P150-45-510",
                    $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                    new[,]
                    {
                        {"D1@Вытянуть1", Convert.ToString(height - 140)},
                        {"D1@Кривая1", Convert.ToString(rivetL)}
                    },
                    false,
                    null);
                _swApp.CloseDoc(newName);
            }

            //  01-P252-45-550
            newName = "01-P252-45-" + (height - 100);
            newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";
            if (File.Exists(new FileInfo(newPartPath).FullName))
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("01-P252-45-550-10@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0,
                    0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("01-P252-45-550.SLDPRT");
            }
            else if (File.Exists(newPartPath) != true)
            {
                SwPartParamsChangeWithNewName("01-P252-45-550",
                    $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                    new[,] {{"D1@Вытянуть1", Convert.ToString(height - 100)}},
                    false,
                    null);
                _swApp.CloseDoc(newName);
            }

            swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
            swDoc.EditRebuild3();
            swDoc.ForceRebuild3(true);
            swAsm = (AssemblyDoc) swDoc;

            if (side == "левая")
            {
                swDoc.Extension.SelectByID2("M8-Panel block-one side-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT",
                    0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("M8-Panel block-one side-6@" + modelName.Replace(".SLDASM", ""), "COMPONENT",
                    0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("M8-Panel block-one side-7@" + modelName.Replace(".SLDASM", ""), "COMPONENT",
                    0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-10@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-4@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-12@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-12@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }
            else if (side != "левая")
            {
                swDoc.Extension.SelectByID2("M8-Panel block-one side-16@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-21@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-22@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("M8-Panel block-one side-17@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-23@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("M8-Panel block-one side-18@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("M8-Panel block-one side-18@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Roof

            if (roofType != "")
            {
                swAsm = (AssemblyDoc) swDoc;
                swAsm.ResolveAllLightWeightComponents(true);
                var roofUnit50 = RoofStr(roofType, Convert.ToString(width), Convert.ToString(lenght), true);
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("15-01-770-1000-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swAsm.ReplaceComponents(roofUnit50, "", false, true);
                _swApp.CloseDoc(new FileInfo(roofUnit50).Name);
            }
            else
            {
                swDoc.Extension.SelectByID2("15-01-770-1000-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Крыша", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Panels

            if (p1 != "")
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));

                var panelVbUp = Panels50BuildStr(new[] {"01", "Несъемная"}, widthVb, Convert.ToString(width - 100),
                    materialP1, materialP2, null, true);
                _swApp.CloseDoc(panelVbUp);
                swDoc.Extension.SelectByID2("02-01-700-770-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(panelVbUp, "", true, true);

                var panelVbSide = Panels50BuildStr(new[] {"01", "Несъемная"}, widthVb, Convert.ToString(height - 100),
                    materialP1, materialP2, null, true);
                _swApp.CloseDoc(panelVbUp);
                swDoc.Extension.SelectByID2("02-01-700-550-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(panelVbSide, "", true, true);

                var panelUp = Panels50BuildStr(new[] {"01", "Несъемная"},
                    Convert.ToString(lenght - 150 - Convert.ToDouble(widthVb)), Convert.ToString(width - 100),
                    materialP1, materialP2, null, true);
                _swApp.CloseDoc(panelVbUp);
                swDoc.Extension.SelectByID2("02-01-930-770-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(panelUp, "", true, true);
                var panelSide = Panels50BuildStr(new[] {"01", "Несъемная"},
                    Convert.ToString(lenght - 150 - Convert.ToDouble(widthVb)), Convert.ToString(height - 100),
                    materialP1, materialP2, null, true);
                _swApp.CloseDoc(panelVbUp);
                swDoc.Extension.SelectByID2("02-01-930-550-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(panelSide, "", true, true);

             
                swDoc.Extension.SelectByID2("02-04-400-550-30-Az-Az-MW-7@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(p1, "", false, true);
                swDoc.Extension.SelectByID2("02-04-350-550-50-Az-Az-MW-3@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(p2, "", false, true);
                swDoc.Extension.SelectByID2("02-01-80-550-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(p3, "", false, true);
                swDoc.Extension.SelectByID2("02-04-700-550-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(p4, "", false, true);

            }
            else
            {
                swDoc.Extension.SelectByID2("02-04-400-550-30-Az-Az-MW-7@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-04-350-550-50-Az-Az-MW-3@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-04-700-550-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-01-700-550-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-01-930-550-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-01-700-770-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-01-700-770-50-Az-Az-MW-2@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-01-930-770-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-01-930-770-50-Az-Az-MW-2@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-01-80-550-50-Az-Az-MW-1@" + modelName.Replace(".SLDASM", ""),
                    "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Панели", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }
            
            #endregion

            #region Монтажная рама

            if (montFrame != "")
            {
                swAsm = (AssemblyDoc) swDoc;
                swAsm.ResolveAllLightWeightComponents(true);
                var montageFrame = MontageFrameS(Convert.ToString(width), Convert.ToString(lenght), thiknessFrame,
                    typeOfFrame,
                    frameOffset, "", null, true);
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("10-2-1300-1150-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swAsm.ReplaceComponents(montageFrame, "", false, true);
                _swApp.CloseDoc(new FileInfo(montageFrame).Name);
            }
            else
            {
                swDoc.Extension.SelectByID2("10-2-1300-1150-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Монтажная рама", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Фильтр

            if (filter != "")
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("04-01-AV04-G4-300-2@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0,
                    0, false, 0, null, 0);
                swAsm.ReplaceComponents(filter, "", false, true);
            }
            else
            {
                swDoc.Extension.SelectByID2("04-01-AV04-G4-300-2@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0,
                    0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Теплообменник 1 (Нагрев)

            if (heater != "")
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("16-AV04-3H-3@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swAsm.ReplaceComponents(heater, "", false, true);
            }
            else
            {
                swDoc.Extension.SelectByID2("16-AV04-3H-3@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion
            
            #region Венитиляторный блок 

            if (ventil != "")
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("03-02-35-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swAsm.ReplaceComponents(ventil, "", false, true);
            }
            else
            {
                swDoc.Extension.SelectByID2("03-02-35-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Заслонка

            if (dumper != "")
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("11-800-500-150-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swAsm.ReplaceComponents(dumper, "", false, true);
            }
            else
            {
                swDoc.Extension.SelectByID2("11-800-500-150-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Вибровставка

            if (spigot != "")
            {
                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName, true, 0)));
                swDoc.Extension.SelectByID2("12-800-500-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                // MessageBox.Show("Замена" + spigot.Replace(".SLDASM", "") + ".SLDASM");
                swAsm.ReplaceComponents(spigot.Replace(".SLDASM", "") + ".SLDASM", "", true, true);
            }
            else
            {
                swDoc.Extension.SelectByID2("12-800-500-1@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("12-800-500-2@" + modelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Омеги

            ////Погашение вертикальной омеги
            //swDoc.Extension.SelectByID2("Омега вертикальная зеркалка", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
            //swDoc.EditSuppress2();
            //boolstatus = swDoc.Extension.SelectByID2("Омега вертикальная", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
            //swDoc.EditSuppress2();
            ////Погашение горизонтальной омеги
            //swDoc.Extension.SelectByID2("Омега горизонтальная зеркальное", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
            //swDoc.EditSuppress2();
            //boolstatus = swDoc.Extension.SelectByID2("Омега горизонтальная", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
            //swDoc.EditSuppress2();


            //swDoc.Extension.SelectByID2("Coil-3@01-000-500", "COMPONENT", 0, 0, 0, false, 0, null, 0);
            ////var myModelView = ((ModelView)(swDoc.ActiveView));
            ////myModelView.RotateAboutCenter(0.047743940985476567, 0.42716619534422462);
            //swAsm.ReplaceComponents("C:\\Tets_debag\\Frameless Besing\\16-AV04-2H.SLDASM", "", false, true);

            #endregion

            #region  Сохранение

            _swApp.IActivateDoc2("01-000-500.SLDASM", false, 0);
            swDoc = ((ModelDoc2) (_swApp.ActiveDoc));
            swDoc.ForceRebuild3(true);
            swDoc.SaveAs2($@"{Settings.Default.DestinationFolder}{Unit50FolderD}\{newUnit50Name}.SLDASM",
                (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            NewComponents.Add(new FileInfo(newUnit50Path));

            _swApp.CloseDoc(new FileInfo(newUnit50Path).Name);
            _swApp = null;
            CheckInOutPdm(NewComponents, true, Settings.Default.TestPdmBaseName);

            return newUnit50Path;

            #endregion
        }

        #endregion

        #region Unit

        /// <summary>
        /// Метод по генерации модели блока усановки на 50-м профиле.
        /// </summary>
        /// <param name="size">типоразмер</param>
        /// <param name="order">The order.</param>
        /// <param name="side">сторона обслуживания</param>
        /// <param name="widthS">ширина блока</param>
        /// <param name="heightS">высота блока</param>
        /// <param name="lenghtS">длина блока</param>
        /// <param name="frame">The frame.</param>
        /// <param name="panels">The panels.</param>
        /// <param name="roofType">тип крыши</param>
        /// <param name="section">The section.</param>
        /// <param name="profilOfCascet"></param>
        public void UnitAsmbly(string size, string order, string side, string widthS, string heightS,
            string lenghtS, string frame,
            string[] panels, string roofType, string section,
            string profilOfCascet)
        {

            Логгер.Информация($"Начало генерации блока {$"{size} {order} {section}"}", "", null, "UnitAsmbly");

            try
            {
                if (!InitializeSw(true)) return;

                var path = UnitAsmblyStr(size, order, side, widthS, heightS, lenghtS, frame,
                    panels, roofType, section, profilOfCascet);

                if (path == "")
                {
                    return;
                }
                if (MessageBox.Show(
                    $"Модель находится по пути:\n {new FileInfo(path).Directory}\n Открыть модель?",
                    $" {Path.GetFileNameWithoutExtension(new FileInfo(path).FullName)} ", MessageBoxButton.YesNoCancel) ==
                    MessageBoxResult.Yes)
                {
                    UnitAsmblyStr(size, order, side, widthS, heightS, lenghtS, frame,
                        panels, roofType, section, profilOfCascet);
                }
            }
            catch (Exception e)
            {
                Логгер.Ошибка($"Ошибка во время генерации блока {$"{size}-{order}-{section}"}. {e.Message}", e.StackTrace, null, "UnitAsmbly");
            }
        }

        /// <summary>
        /// Units the asmbly string.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="order">The order.</param>
        /// <param name="side">The side.</param>
        /// <param name="widthS">The width s.</param>
        /// <param name="heightS">The lenght s.</param>
        /// <param name="lenghtS">The lenght s.</param>
        /// <param name="frame"></param>
        /// <param name="panels">The panels.</param>
        /// <param name="roofType">Type of the roof.</param>
        /// <param name="section">The section.</param>
        /// <param name="profilOfCascet"></param>
        /// <returns></returns>
        public string UnitAsmblyStr(string size, string order, string side, string widthS, string heightS,
            string lenghtS,
            string frame, string[] panels, string roofType, string section, string profilOfCascet)
        {

            #region Проверка значений

            if (IsConvertToInt(new[] {widthS, heightS, lenghtS}) == false)
            {
                return "";
            }

            #endregion

            #region Basic Parameters

            // Габариты
            var width = GetInt(widthS);
            var height = GetInt(heightS);
            var lenght = GetInt(lenghtS);

            #endregion

            #region Start

            var modelName = profilOfCascet == "150" ? "01-000-500" : "01-000-700";

            var unitAsMmodel = $@"{Settings.Default.SourceFolder}{Unit50FolderS}\{modelName + ".SLDASM"}";

            var newUnit50Name = $"{size} {order} {section}";


            var orderFolder = $@"{Settings.Default.DestinationFolder}\{Unit50Orders}\{size}\{size} {order}";

            CreateDistDirectory(orderFolder, Settings.Default.PdmBaseName);

            var newUnit50Path = $@"{orderFolder}\{newUnit50Name}.SLDASM";

            if (File.Exists(newUnit50Path))
            {
                GetLastVersionPdm(new FileInfo(newUnit50Path).FullName, Settings.Default.TestPdmBaseName);
                _swApp.OpenDoc6(newUnit50Path, (int) swDocumentTypes_e.swDocASSEMBLY,
                    (int) swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                return newUnit50Path;
            }

            GetLatestVersionAsmPdm(unitAsMmodel, Settings.Default.PdmBaseName);

            if (!Warning()) return "";
            var swDoc = _swApp.OpenDoc6(unitAsMmodel, (int) swDocumentTypes_e.swDocASSEMBLY,
                (int) swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            _swApp.Visible = true;
            var swAsm = (AssemblyDoc) swDoc;
            swAsm.ResolveAllLightWeightComponents(false);

            #endregion

            #region Frame

            try
            {
                var profilName = profilOfCascet == "150" ? "01-P150-45-" : "01-P170-50-";
                var deltaForProfil = profilOfCascet == "150" ? 140 : 180;

                //Lenght
                const double step = 100;
                double rivetL;


                var modelLenghtProfil = profilOfCascet == "150" ? "01-002-50" : "01-002-170-45";
                var lenghtProfil = profilOfCascet == "150" ? "01-002-50-27" : "01-002-170-45-3";


                var newName = profilName + (lenght - deltaForProfil);
                var newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";
                if (File.Exists(new FileInfo(newPartPath).FullName))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2(lenghtProfil + "@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc(modelLenghtProfil + ".SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    rivetL = (Math.Truncate((lenght - deltaForProfil - 30)/step) + 1)*1000;
                    SwPartParamsChangeWithNewName(modelLenghtProfil,
                        $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                        new[,]
                        {
                            {"D1@Вытянуть1", Convert.ToString(lenght - deltaForProfil)},
                            {"D1@Кривая1", Convert.ToString(rivetL)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                // SwPartSizeChangeWithNewName("01-002-50", "01-P150-45-" + (lenght - 140), "D1@Вытянуть1", lenght - 140);//(Width - 140).ToString()
                // SwPartSizeChangeWithNewName("01-005-50", "01-P252-45-" + (Lenght - 140 + 40).ToString(), "D1@Вытянуть1", Lenght - 140 + 40);//(Width - 140 + 40).ToString()

                //Width
                newName = profilName + (width - deltaForProfil);
                newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";

                var modelWidthProfil = profilOfCascet == "150" ? "01-003-50" : "01-003-170-45";
                var widthProfil = profilOfCascet == "150" ? "01-003-50-22" : "01-003-170-45-3";

                if (File.Exists(new FileInfo(newPartPath).FullName))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2(widthProfil + "@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc(modelWidthProfil + ".SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    rivetL = (Math.Truncate((width - deltaForProfil - 30)/step) + 1)*1000;
                    SwPartParamsChangeWithNewName(modelWidthProfil,
                        $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                        new[,]
                        {
                            {"D1@Вытянуть1", Convert.ToString(width - deltaForProfil)},
                            {"D1@Кривая1", Convert.ToString(rivetL)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                //SwPartSizeChangeWithNewName("01-003-50", "01-P150-45-" + (width - 140), "D1@Вытянуть1", width - 140);//"01-P150-45-" + (Lenght - 140).ToString(),

                //Height
                newName = profilName + (height - deltaForProfil);
                newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";

                var modelHeightProfil = profilOfCascet == "150" ? "01-001-50" : "01-001-170-45";
                var heightProfil = profilOfCascet == "150" ? "01-001-50-23" : "01-001-170-45-3";

                if (File.Exists(new FileInfo(newPartPath).FullName))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2(heightProfil + "@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc(modelHeightProfil + ".SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    rivetL = (Math.Truncate((height - deltaForProfil - 30)/step) + 1)*1000;
                    SwPartParamsChangeWithNewName(modelHeightProfil,
                        $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                        new[,]
                        {
                            {"D1@Вытянуть1", Convert.ToString(height - deltaForProfil)},
                            {"D1@Кривая1", Convert.ToString(rivetL)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                //SwPartSizeChangeWithNewName("01-001-50", "01-P150-45-" + (lenght - 140), "D1@Вытянуть1", lenght - 140);//"01-P150-45-" + (Height - 140).ToString()
                //SwPartSizeChangeWithNewName("01-004-50", "01-P252-45-" + (Height - 140 + 40).ToString(), "D1@Вытянуть1", Height - 140 + 40);//"01-P252-45-" + (Height - 140 + 40).ToString()

                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                swDoc.EditRebuild3();
                swDoc.ForceRebuild3(true);
                swAsm = (AssemblyDoc) swDoc;

                if (side == "левая")
                {
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-6@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-7@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-10@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-4@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-12@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-12@" + modelName, "COMPONENT", 0, 0, 0,
                        false, 0, null, 0);
                    swDoc.EditDelete();
                }
                else if (side != "левая")
                {
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-16@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-21@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-22@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-17@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-23@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-18@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-18@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swDoc.EditDelete();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Frame");
            }

            #endregion

            #region Roof

            try
            {
                if (!string.IsNullOrEmpty(roofType))
                {
                    swAsm = (AssemblyDoc) swDoc;
                    swAsm.ResolveAllLightWeightComponents(false);
                    Thread.Sleep(5000);

                    //MessageBox.Show("Roof");
                    var roofUnit50 = RoofStr(roofType, Convert.ToString(width), Convert.ToString(lenght), true);
                    // MessageBox.Show("Roof End");
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));

                    swDoc.Extension.SelectByID2("15-01-770-1000-1@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(roofUnit50, "", false, true);
                    _swApp.CloseDoc(new FileInfo(roofUnit50).Name);
                }
                else
                {
                    swDoc.Extension.SelectByID2("15-01-770-1000-1@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Roof");
            }

            #endregion

            #region Panels

            try
            {
                if (panels[0] != null)
                {

                    #region to delete

                    //{{
                    //    //WxL
                    //    //string panelWxL = Panels50BuildStr("01 - Несъемная (50-Az-Az-MW)", (lenght - 100).ToString(), (width - 100).ToString(), "Az", "Az");
                    //    //_swApp.CloseDoc(panelWxL);
                    //    var panelWxL = Panels50BuildStr(
                    //        typeOfPanel: new[] { "01", "Несъемная" },
                    //        width: Convert.ToString(lenght - 100),
                    //        lenght: Convert.ToString(width - 100),
                    //        materialP1: new[] { materialP1, null, null, null },
                    //        materialP2: new[] { materialP2, null, null, null },
                    //        покрытие: new[] { (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null },
                    //        onlyPath: true);
                    //    var closedFile = new FileInfo(panelWxL); _swApp.CloseDoc(closedFile.Name);
                    //    //HxL
                    //    //string panelHxL = Panels50BuildStr("01 - Несъемная (50-Az-Az-MW)", (lenght - 100).ToString(), (lenght - 100).ToString(), "Az", "Az");
                    //    //_swApp.CloseDoc(panelHxL);
                    //    var panelHxL = Panels50BuildStr(
                    //        typeOfPanel: new[] { "01", "Несъемная" },
                    //        width: Convert.ToString(lenght - 100),
                    //        lenght: Convert.ToString(lenght - 100),
                    //        materialP1: new[] { materialP1, null, null, null },
                    //        materialP2: new[] { materialP2, null, null, null },
                    //        покрытие: new[] { (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null },
                    //        onlyPath: true);
                    //    closedFile = new FileInfo(panelHxL); _swApp.CloseDoc(closedFile.Name);
                    //    var panelHxL04 = Panels50BuildStr(
                    //        typeOfPanel: typeOfPanel,
                    //        width: Convert.ToString(lenght - 100),
                    //        lenght: Convert.ToString(lenght - 100),
                    //        materialP1: new[] { materialP1, null, null, null },
                    //        materialP2: new[] { materialP2, null, null, null },
                    //        покрытие: new[] { (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null },
                    //        onlyPath: true);
                    //    closedFile = new FileInfo(panelHxL04); _swApp.CloseDoc(closedFile.Name);

                    #endregion

                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));

                    var panelWxL = panels[0];
                    var panelHxL = panels[1];
                    var panelHxL04 = panels[2];

                    var sidePanelL = panelHxL;
                    var sidePanelR = panelHxL04;

                    if (side == "левая")
                    {
                        sidePanelL = panelHxL04;
                        sidePanelR = panelHxL;
                    }

                    swDoc.Extension.SelectByID2("02-01-650-670-50-Az-Az-MW-4@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swAsm.ReplaceComponents(panelWxL, "", false, true);
                    swDoc.Extension.SelectByID2("02-01-650-670-50-Az-Az-MW-5@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swAsm.ReplaceComponents(panelWxL, "", false, true);


                    swDoc.Extension.SelectByID2("02-01-650-625-50-Az-Az-MW-6@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swAsm.ReplaceComponents(sidePanelL, "", false, true);
                    swDoc.Extension.SelectByID2("02-04-650-625-50-Az-Az-MW-5@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swAsm.ReplaceComponents(sidePanelR, "", false, true);
                }
                else
                {
                    //  MessageBox.Show("delete", "panelWxL\n" + panels[0]);

                    swDoc.Extension.SelectByID2("02-01-650-670-50-Az-Az-MW-4@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("02-01-650-670-50-Az-Az-MW-5@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("02-01-650-625-50-Az-Az-MW-6@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("02-04-650-625-50-Az-Az-MW-5@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swDoc.EditDelete();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Panels");
            }

            #endregion

            #region Монтажная рама

            try
            {
                if (frame != "")
                {
                    swAsm = (AssemblyDoc) swDoc;
                    swAsm.ResolveAllLightWeightComponents(true);
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("10-2-1300-1150-1@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(frame, "", false, true);
                    _swApp.CloseDoc(new FileInfo(frame).Name);
                }
                else
                {
                    swDoc.Extension.SelectByID2("10-2-1300-1150-1@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Монтажная рама");
            }

            #endregion

            #region Омеги

            ////Погашение вертикальной омеги
            //swDoc.Extension.SelectByID2("Омега вертикальная зеркалка", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
            //swDoc.EditSuppress2();
            //boolstatus = swDoc.Extension.SelectByID2("Омега вертикальная", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
            //swDoc.EditSuppress2();
            ////Погашение горизонтальной омеги
            //swDoc.Extension.SelectByID2("Омега горизонтальная зеркальное", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
            //swDoc.EditSuppress2();
            //boolstatus = swDoc.Extension.SelectByID2("Омега горизонтальная", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
            //swDoc.EditSuppress2();

            #endregion

            #region COMPPATTERN

            swDoc.Extension.SelectByID2("DerivedCrvPattern1", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("DerivedCrvPattern2", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("DerivedCrvPattern3", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега вертикальная зеркалка", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега горизонтальная зеркальное", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега горизонтальная зеркальное", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
            swDoc.EditDelete();
            swDoc.Extension.SelectByID2("Уголок 901.31.209", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Косынка ВНС-901.31.126", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега вертикальная", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега горизонтальная", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега горизонтальная", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
            swDoc.EditDelete();
            swDoc.Extension.SelectByID2("ВНС-901.31.209-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-2@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-3@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-4@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-5@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-6@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-7@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-8@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-2@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-3@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-4@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-5@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-6@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-7@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-8@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("01-004-50-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-2@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-3@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-4@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("01-005-50-3@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-9@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-10@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-13@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-14@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-14@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
            swDoc.EditDelete();

            #endregion

            #region to delete

            //swDoc.Extension.SelectByID2("Coil-3@01-000-500", "COMPONENT", 0, 0, 0, false, 0, null, 0);
            //var myModelView = ((ModelView)(swDoc.ActiveView));
            //myModelView.RotateAboutCenter(0.047743940985476567, 0.42716619534422462);
            //swAsm.ReplaceComponents("C:\\Tets_debag\\Frameless Besing\\16-AV04-2H.SLDASM", "", false, true);

            #endregion

            #region  Сохранение

            _swApp.IActivateDoc2(modelName + ".SLDASM", false, 0);
            swDoc = ((ModelDoc2) (_swApp.ActiveDoc));
            swDoc.ForceRebuild3(true);
            swDoc.SaveAs2(newUnit50Path, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

            //swDoc.SaveAs2(String.Format(@"{0}\{1}\{2}.SLDASM", Settings.Default.DestinationFolder, Unit50Folder, newUnit50Name), (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            NewComponents.Add(new FileInfo(newUnit50Path));

            _swApp.CloseDoc(new FileInfo(newUnit50Path).Name);
            //_swApp.ExitApp();
            _swApp = null;
            CheckInOutPdm(NewComponents, true, Settings.Default.TestPdmBaseName);
                //.OrderBy(x => x.Length).ToList(), true, Settings.Default.TestPdmBaseName);

            #region to delete

            //            var createdFileInfosM = "";
            //            var count = 0;

            //            foreach (var fileInfo in NewComponents.OrderByDescending(x => x.Length).ToList())
            //            {
            //                count += 1;
            //                var fileSize = fileInfo.Length.ToString();
            //                if (fileInfo.Length >= (1 << 30))
            //                    fileSize = String.Format("{0}Gb", fileInfo.Length >> 30);
            //                else if (fileInfo.Length >= (1 << 20))
            //                    fileSize = String.Format("{0}Mb", fileInfo.Length >> 20);
            //                else if (fileInfo.Length >= (1 << 10))
            //                    fileSize = String.Format("{0}Kb", fileInfo.Length >> 10);
            //                var extension = " Деталь - ";
            //                if (Path.GetFileNameWithoutExtension(fileInfo.FullName).ToUpper() == ".SLDASM")
            //                {
            //                    extension = " Сборка - ";
            //                }
            //                createdFileInfosM = createdFileInfosM + @"
            //" + count + ". " + extension + fileInfo.Name + " - " + fileSize;
            //            }
            //            MessageBox.Show(createdFileInfosM, "Созданы следующие файлы");

            #endregion

            return newUnit50Path;

            #endregion
        }

        #endregion

        #region Unit 30

        /// <summary>
        /// Метод по генерации модели блока усановки на 50-м профиле.
        /// </summary>
        /// <param name="size">типоразмер</param>
        /// <param name="order">The order.</param>
        /// <param name="side">сторона обслуживания</param>
        /// <param name="widthS">ширина блока</param>
        /// <param name="heightS">высота блока</param>
        /// <param name="lenghtS">длина блока</param>
        /// <param name="frame">The frame.</param>
        /// <param name="panels">The panels.</param>
        /// <param name="roofType">тип крыши</param>
        /// <param name="section">The section.</param>
        public void UnitAsmbly30(string size, string order, string side, string widthS, string heightS,
            string lenghtS, string frame,
            string[] panels, string roofType, string section)
        {
            Логгер.Информация($"Начало генерации блока {$"{size} {order} {section}"}", "", null, "UnitAsmbly");
            try
            {
                if (!InitializeSw(true)) return;

                var path = UnitAsmbly30Str(size, order, side, widthS, heightS, lenghtS, frame,
                    panels, roofType, section);

                if (path == "")
                {
                    return;
                }
                if (MessageBox.Show(
                    $"Модель находится по пути:\n {new FileInfo(path).Directory}\n Открыть модель?",
                    $" {Path.GetFileNameWithoutExtension(new FileInfo(path).FullName)} ", MessageBoxButton.YesNoCancel) ==
                    MessageBoxResult.Yes)
                {
                    UnitAsmbly30Str(size, order, side, widthS, heightS, lenghtS, frame,
                        panels, roofType, section);
                }
            }
            catch (Exception e)
            {
                Логгер.Ошибка(
                    $"Ошибка во время генерации блока {$"{size}-{order}-{section}"}. {e.Message}", e.StackTrace, null,
                    "UnitAsmbly");
            }
        }

        /// <summary>
        /// Units the asmbly string.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="order">The order.</param>
        /// <param name="side">The side.</param>
        /// <param name="widthS">The width s.</param>
        /// <param name="heightS">The lenght s.</param>
        /// <param name="lenghtS">The lenght s.</param>
        /// <param name="frame"></param>
        /// <param name="panels">The panels.</param>
        /// <param name="roofType">Type of the roof.</param>
        /// <param name="section">The section.</param>
        /// <returns></returns>
        public string UnitAsmbly30Str(string size, string order, string side, string widthS, string heightS,
            string lenghtS,
            string frame, string[] panels, string roofType, string section)
        {
            string panel1 = null;
            //foreach (var panel in panels)
            //{
            //    panel1 = panel1 +"\n"+ panel;
            //}
            //MessageBox.Show(panel1);

            if (!IsConvertToInt(new[] {widthS, heightS, lenghtS}))
            {
                return null;
            }

            #region Basic Parameters

            // Габариты
            var width = GetInt(widthS);
            var height = GetInt(heightS);
            var lenght = GetInt(lenghtS);

            #endregion

            #region Start

            const string modelName = "01 - Frame 30mm";

            var unitAsMmodel = $@"{Settings.Default.SourceFolder}{Unit30FolderS}\{modelName + ".SLDASM"}";


            var newUnit30Name = $"{size} {order} {section}";
            var orderFolder = $@"{Settings.Default.DestinationFolder}\{Unit50Orders}\{size}\{size} {order}";
            CreateDistDirectory(orderFolder, Settings.Default.PdmBaseName);

            var newUnit30Path = $@"{orderFolder}\{newUnit30Name}.SLDASM";

            if (File.Exists(newUnit30Path))
            {
                GetLastVersionPdm(new FileInfo(newUnit30Path).FullName, Settings.Default.TestPdmBaseName);
                _swApp.OpenDoc6(newUnit30Path, (int) swDocumentTypes_e.swDocASSEMBLY,
                    (int) swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                return newUnit30Path;
            }

            GetLatestVersionAsmPdm(unitAsMmodel, Settings.Default.PdmBaseName);

            if (!Warning()) return "";
            var swDoc = _swApp.OpenDoc6(unitAsMmodel, (int) swDocumentTypes_e.swDocASSEMBLY,
                (int) swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            _swApp.Visible = true;
            var swAsm = (AssemblyDoc) swDoc;
            swAsm.ResolveAllLightWeightComponents(false);

            #endregion

            #region Frame

            try
            {
                const string profilName = "01-S 1300-25-";

                //Lenght

                const string modelLenghtProfil = "01-S 1300-25-003";
                const string lenghtProfil = "01-S 1300-25-003-20";

                var newName = profilName + (lenght - 100);
                var newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";
                if (File.Exists(new FileInfo(newPartPath).FullName))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2(lenghtProfil + "@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc(modelLenghtProfil + ".SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName(modelLenghtProfil,
                        $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                        new[,]
                        {
                            {"D1@Бобышка-Вытянуть1", Convert.ToString(lenght - 100)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                //Width
                newName = profilName + (width - 100);
                newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";

                const string modelWidthProfil = "01-S 1300-25-002";
                const string widthProfil = "01-S 1300-25-002-16";

                if (File.Exists(new FileInfo(newPartPath).FullName))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2(widthProfil + "@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc(modelWidthProfil + ".SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName(modelWidthProfil,
                        $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                        new[,]
                        {
                            {"D1@Бобышка-Вытянуть1", Convert.ToString(width - 100)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                //Height
                newName = profilName + (height - 100);
                newPartPath = $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}.SLDPRT";

                const string modelHeightProfil = "01-S 1300-25-001";
                const string heightProfil = "01-S 1300-25-001-16";

                if (File.Exists(new FileInfo(newPartPath).FullName))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2(heightProfil + "@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc(modelHeightProfil + ".SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName(modelHeightProfil,
                        $@"{Settings.Default.DestinationFolder}\{Unit50FolderD}\{newName}",
                        new[,]
                        {
                            {"D1@Бобышка-Вытянуть1", Convert.ToString(height - 100)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                swDoc.EditRebuild3();
                swDoc.ForceRebuild3(true);
                swAsm = (AssemblyDoc) swDoc;

                if (side == "левая")
                {
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-6@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-7@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-10@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-4@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-12@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-12@" + modelName, "COMPONENT", 0, 0, 0,
                        false, 0, null, 0);
                    swDoc.EditDelete();
                }
                else if (side != "левая")
                {
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-16@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-21@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-22@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-17@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets с насечкой-23@" + modelName, "COMPONENT", 0, 0, 0, true,
                        0, null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-18@" + modelName, "COMPONENT", 0, 0, 0, true, 0,
                        null, 0);
                    swDoc.Extension.SelectByID2("M8-Panel block-one side-18@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swDoc.EditDelete();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Frame");
            }

            #endregion

            #region Roof

            try
            {
                if (!string.IsNullOrEmpty(roofType))
                {
                    swAsm = (AssemblyDoc) swDoc;
                    swAsm.ResolveAllLightWeightComponents(false);
                    Thread.Sleep(5000);

                    var roofUnit50 = RoofStr(roofType, Convert.ToString(width), Convert.ToString(lenght), true);
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));

                    swDoc.Extension.SelectByID2("15-01-770-1000-1@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(roofUnit50, "", false, true);
                    _swApp.CloseDoc(new FileInfo(roofUnit50).Name);
                }
                else
                {
                    swDoc.Extension.SelectByID2("15-01-770-1000-1@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Roof");
            }

            #endregion

            #region Panels

            try
            {
                if (panels[0] != null)
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));

                    var panelWxL = panels[0];
                    var panelHxL = panels[1];
                    var panelHxL04 = panels[2];

                    var sidePanelL = panelHxL;
                    var sidePanelR = panelHxL04;

                    if (side == "левая")
                    {
                        sidePanelL = panelHxL04;
                        sidePanelR = panelHxL;
                    }

                    swDoc.Extension.SelectByID2("02-01-400-880-30-Az-Az-MW-1@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swAsm.ReplaceComponents(panelWxL, "", false, true);

                    swDoc.Extension.SelectByID2("02-01-400-625-30-Az-Az-MW-1@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swAsm.ReplaceComponents(sidePanelL, "", false, true);
                    swDoc.Extension.SelectByID2("02-04-400-625-30-Az-Az-MW-1@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swAsm.ReplaceComponents(sidePanelR, "", false, true);
                }
                else
                {
                    swDoc.Extension.SelectByID2("02-01-400-880-30-Az-Az-MW-1@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("02-01-400-625-30-Az-Az-MW-1@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("02-04-400-625-30-Az-Az-MW-1@" + modelName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swDoc.EditDelete();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Panels");
            }

            #endregion

            #region Монтажная рама

            try
            {
                if (frame != "")
                {
                    swAsm = (AssemblyDoc) swDoc;
                    swAsm.ResolveAllLightWeightComponents(true);
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(modelName + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("10-2-1300-1150-1@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(frame, "", false, true);
                    _swApp.CloseDoc(new FileInfo(frame).Name);
                }
                else
                {
                    swDoc.Extension.SelectByID2("10-2-1300-1150-1@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Монтажная рама");
            }

            #endregion

            #region COMPPATTERN

            swDoc.Extension.SelectByID2("DerivedCrvPattern1", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("DerivedCrvPattern2", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("DerivedCrvPattern3", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега вертикальная зеркалка", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега горизонтальная зеркальное", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега горизонтальная зеркальное", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
            swDoc.EditDelete();
            swDoc.Extension.SelectByID2("Уголок 901.31.209", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Косынка ВНС-901.31.126", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега вертикальная", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега горизонтальная", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("Омега горизонтальная", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
            swDoc.EditDelete();
            swDoc.Extension.SelectByID2("ВНС-901.31.209-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-2@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-3@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-4@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-5@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-6@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-7@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.209-8@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-2@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-3@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-4@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-5@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-6@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-7@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.126-8@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("01-004-50-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-1@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-2@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-3@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-4@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("01-005-50-3@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-9@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-10@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-13@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-14@" + modelName, "COMPONENT", 0, 0, 0, true, 0, null, 0);
            swDoc.Extension.SelectByID2("ВНС-901.31.125-14@" + modelName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
            swDoc.EditDelete();

            #endregion


            #region  Сохранение

            _swApp.IActivateDoc2(modelName + ".SLDASM", false, 0);
            swDoc = ((ModelDoc2) (_swApp.ActiveDoc));
            swDoc.ForceRebuild3(true);
            swDoc.SaveAs2(newUnit30Path, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            NewComponents.Add(new FileInfo(newUnit30Path));
            _swApp.CloseDoc(new FileInfo(newUnit30Path).Name);
            _swApp = null;
            CheckInOutPdm(NewComponents, true, Settings.Default.TestPdmBaseName);

            return newUnit30Path;

            #endregion
        }

        #endregion

        #region Panels

        /// <summary>
        /// Метод по генерации модели панелей и дверей для блоков на 50-м профиле.
        /// </summary>
        /// <param name="typeOfPanel">тип панели</param>
        /// <param name="width">ширина панели</param>
        /// <param name="height">высота панели</param>
        /// <param name="materialP1">материал внешней панели (наименование либо код из базы)</param>
        /// <param name="meterialP2">материал внутренней панели (наименование либо код из базы)</param>
        /// <param name="покрытие">The ПОКРЫТИЕ.</param>
        public void Panels50Build(string[] typeOfPanel, string width, string height, string[] materialP1,
            string[] meterialP2, string[] покрытие)
        {
            var path = Panels50BuildStr(typeOfPanel, width, height, materialP1, meterialP2, покрытие, false);
        }

        /// <summary>
        /// Panels50s the build string.
        /// </summary>
        /// <param name="typeOfPanel">The type of panel.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The lenght.</param>
        /// <param name="materialP1">The material p1.</param>
        /// <param name="materialP2">The material p2.</param>
        /// <param name="покрытие">The ПОКРЫТИЕ.</param>
        /// <param name="onlyPath">if set to <c>true</c> [only path].</param>
        /// <returns></returns>
        public string Panels50BuildStr(string[] typeOfPanel, string width, string height, string[] materialP1,
            string[] materialP2, string[] покрытие, bool onlyPath)
        {
            if (IsConvertToInt(new[] {width, height}) == false)
            {
                return "";
            }

            var thicknessOfPanel = typeOfPanel[2];

            Логгер.Отладка("Начало построения 50-й панели. ", "", "Panels50BuildStr", "Panels50BuildStr");

            string modelPanelsPath;
            string modelName;
            string nameAsm;
            var modelType =
                $"{thicknessOfPanel}-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}-MW";

            switch (typeOfPanel[1])
            {
                case "Панель несъемная глухая":
                    modelPanelsPath = Panel50Folder;
                    nameAsm = "02-01";
                    modelName = "02-01";
                    _destinationFolder = Panels0201;
                    break;
                case "Панель съемная с ручками":
                    modelPanelsPath = Panel50Folder;
                    nameAsm = "02-01";
                    modelName = "02-04";
                    _destinationFolder = Panels0204;
                    break;
                case "Панель теплообменника":
                    modelPanelsPath = Panel50Folder;
                    nameAsm = "02-01";
                    modelName = "02-05";
                    _destinationFolder = Panels0205;
                    break;
                case "Панель двойная несъемная":
                    modelPanelsPath = DublePanel50Folder;
                    nameAsm = "02-104-50";
                    modelName = "02-01";
                    _destinationFolder = Panels0201;
                    break;

                case "Панель двойная съемная":
                    modelPanelsPath = DublePanel50Folder;
                    nameAsm = "02-104-50";
                    modelName = "02-01";
                    _destinationFolder = Panels0201;
                    break;
                default:
                    modelPanelsPath = Panel50Folder;
                    nameAsm = "02-01";
                    modelName = "02-01";
                    break;
            }

            #region Обозначения и Хранилище

            #region before

            //var newPanel50Name = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
            //        modelName + "-" + width + "-" + lenght + "-50",
            //        string.IsNullOrEmpty(materialP1) ? "" : "-" + materialP1,
            //        //string.IsNullOrEmpty(покрытие[0]) ? "" : "-" + покрытие[0],
            //        //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
            //        //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2],
            //        string.IsNullOrEmpty(materialP2) ? "" : "-" + materialP2,
            //        //string.IsNullOrEmpty(покрытие[3]) ? "" : "-" + покрытие[3],
            //        //string.IsNullOrEmpty(покрытие[4]) ? "" : "-" + покрытие[4],
            //        //string.IsNullOrEmpty(покрытие[5]) ? "" : "-" + покрытие[5],
            //        "-MW");

            #endregion

            #region Без покрытия

            //if (покрытие[0] == "Без покрытия")
            //{
            //    покрытие[1] = "0";
            //    покрытие[2] = "0";
            //}
            //if (покрытие[3] == "Без покрытия")
            //{
            //    покрытие[4] = "0";
            //    покрытие[5] = "0";
            //}

            //var partIds = new List<int>();
            //var панельВнешняя =
            //    new PanelsVault
            //    {
            //        PanelTypeName = typeOfPanel[1],
            //        ElementType = 1,
            //        Width = Convert.ToInt32(width),
            //        Height = Convert.ToInt32(lenght),
            //        PartThick = 50,
            //        PartMat = Convert.ToInt32(materialP1[0]),
            //        PartMatThick = Convert.ToDouble(materialP1[1].Replace('.', ',')),
            //        Ral = покрытие[0],
            //        CoatingType = покрытие[1],
            //        CoatingClass = Convert.ToInt32(покрытие[2]),
            //        Mirror = false,
            //        Reinforcing = false
            //    };
            //var id = панельВнешняя.AirVents_AddPartOfPanel();
            //partIds.Add(id);
            //панельВнешняя.NewName = "02-" + typeOfPanel[0] + "-1-" + id;

            //var панельВнутренняя =
            //    new AddingPanel
            //    {
            //        PanelTypeName = typeOfPanel[1],
            //        ElementType = 2,
            //        Width = Convert.ToInt32(width),
            //        Height = Convert.ToInt32(lenght),
            //        PartThick = 50,
            //        PartMat = Convert.ToInt32(materialP2[0]),
            //        PartMatThick = Convert.ToDouble(materialP2[1].Replace('.', ',')),
            //        Ral = покрытие[3],
            //        CoatingType = покрытие[4],
            //        CoatingClass = Convert.ToInt32(покрытие[5]),
            //        Reinforcing = false
            //    };
            //id = панельВнутренняя.AddPart();
            //partIds.Add(id);
            //панельВнутренняя.NewName = "02-" + typeOfPanel[0] + "-2-" + id;

            //var теплоизоляция =
            //    new AddingPanel
            //    {
            //        PanelTypeName = typeOfPanel[1],
            //        ElementType = 3,
            //        Width = Convert.ToInt32(width),
            //        Height = Convert.ToInt32(lenght),
            //        PartThick = 50,
            //        PartMat = 4900,
            //        Reinforcing = false,

            //        PartMatThick = 1,
            //        Ral = "Без покрытия",
            //        CoatingType = "0",
            //        CoatingClass = Convert.ToInt32("0")
            //    };
            //id = теплоизоляция.AddPart();
            //partIds.Add(id);
            //теплоизоляция.NewName = "02-" + id;

            //var уплотнитель =
            //    new AddingPanel
            //    {
            //        PanelTypeName = typeOfPanel[1],
            //        ElementType = 4,
            //        Width = Convert.ToInt32(width),
            //        Height = Convert.ToInt32(lenght),
            //        PartThick = 50,
            //        PartMat = 14800,
            //        Reinforcing = false,


            //        PartMatThick = 1,
            //        Ral = "Без покрытия",
            //        CoatingType = "0",
            //        CoatingClass = Convert.ToInt32("0")
            //    };
            //id = уплотнитель.AddPart();
            //partIds.Add(id);
            //уплотнитель.NewName = "02-" + id;

            //var деталь103 =
            //    new AddingPanel
            //    {
            //        PanelTypeName = typeOfPanel[1],
            //        ElementType = 103,
            //        Width = Convert.ToInt32(width),
            //        Height = Convert.ToInt32(lenght),
            //        PartThick = 50,
            //        PartMat = 14800,
            //        Reinforcing = false,

            //        PartMatThick = 1,
            //        Ral = "Без покрытия",
            //        CoatingType = "0",
            //        CoatingClass = Convert.ToInt32("0")
            //    };
            //id = деталь103.AddPart();
            //partIds.Add(id);
            //деталь103.NewName = "02-" + id;

            //#region
            ////MessageBox.Show(
            ////   String.Format("{0}\n{1}\n{2}\n{3}\n{4}\n",
            ////   панельВнешняя.NewName,
            ////   панельВнутренняя.NewName,
            ////   теплоизоляция.NewName,
            ////   уплотнитель.NewName,
            ////   деталь103.NewName)); return "";
            //#endregion

            //var sqlBaseData = new SqlBaseData();
            //var newId = sqlBaseData.PanelNumber() + 1;

            //var idAsm = 0;
            //foreach (var сборка in partIds.Select(partId => new AddingPanel
            //{
            //    PartId = partId,

            //    PanelTypeName = typeOfPanel[1],
            //    Width = Convert.ToInt32(width),
            //    Height = Convert.ToInt32(lenght),

            //    PanelMatOut = Convert.ToInt32(materialP1[0]),
            //    PanelMatIn = Convert.ToInt32(materialP2[0]),
            //    PanelThick = 50,
            //    PanelMatThickOut = Convert.ToDouble(materialP1[1].Replace('.', ',')),
            //    PanelMatThickIn = Convert.ToDouble(materialP2[1].Replace('.', ',')),
            //    RalOut = покрытие[0],
            //    RalIn = покрытие[3],
            //    CoatingTypeOut = покрытие[1],
            //    CoatingTypeIn = покрытие[4],
            //    CoatingClassOut = Convert.ToInt32(покрытие[2]),
            //    CoatingClassIn = Convert.ToInt32(покрытие[5]),

            //    Reinforcing = false,

            //    PanelNumber = newId
            //}))
            //{
            //    idAsm = сборка.Add();
            //}

            #endregion

            #endregion

            //var newPanel50Name = "02-" + typeOfPanel[0] + "-" + idAsm;

            var newDestPath = !typeOfPanel[1].Contains("Панель двойная съемная") ? _destinationFolder : Panels0204;
            var newModNumber = !typeOfPanel[1].Contains("Панель двойная съемная") ? modelName : "02-04";

            var newPanel50Name = newModNumber + "-" + width + "-" + height + "-" + modelType;

            Логгер.Информация("Построение панели - " + newPanel50Name, "", null, "Panels50BuildStr");

            var newPanel50Path = $@"{Settings.Default.DestinationFolder}{newDestPath}\{newPanel50Name}.SLDASM";

            if (File.Exists(new FileInfo(newPanel50Path).FullName))
            {
                if (onlyPath) return newPanel50Path;

                MessageBox.Show(newPanel50Path, "Данная модель уже находится в базе");
                return "";

                #region To delete

                //GetLastVersionPdm(new FileInfo(newPanel50Path).FullName, Settings.Default.TestPdmBaseName);
                //System.Diagnostics.Process.Start(@newPanel50Path);

                //if (!InitializeSw(true)) return "";

                //_swApp.OpenDoc6(new FileInfo(newPanel50Path).FullName, (int)swDocumentTypes_e.swDocASSEMBLY,
                //    (int) swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                //return newPanel50Path;

                #endregion

            }

            #region modelPanelAsmbly        

            var modelPanelAsmbly = $@"{Settings.Default.SourceFolder}{modelPanelsPath}\{nameAsm}.SLDASM";

            GetLatestVersionAsmPdm(modelPanelAsmbly, Settings.Default.PdmBaseName);

            if (!InitializeSw(true)) return "";

            var swDoc = _swApp.OpenDoc6(modelPanelAsmbly, (int) swDocumentTypes_e.swDocASSEMBLY,
                (int) swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            _swApp.Visible = true;

            var swAsm = (AssemblyDoc) swDoc;

            swAsm.ResolveAllLightWeightComponents(false);

            // Габариты
            var widthD = Convert.ToDouble(width);
            var heightD = Convert.ToDouble(height);
            var halfWidthD = Convert.ToDouble(widthD/2);
            // Шаг заклепок
            const double step = 80;
            var rivetW = (Math.Truncate(widthD/step) + 1)*1000;
            var rivetWd = (Math.Truncate(halfWidthD/step) + 1)*1000;
            var rivetH = (Math.Truncate(heightD/step) + 1)*1000;
            if (Math.Abs(rivetW - 1000) < 1) rivetW = 2000;
            // Коэффициенты и радиусы гибов   
            const string thiknessStr = "0,8";
            var sbSqlBaseData = new SqlBaseData();
            var bendParams = sbSqlBaseData.BendTable(thiknessStr);
            var bendRadius = Convert.ToDouble(bendParams[0]);
            var kFactor = Convert.ToDouble(bendParams[1]);

            #endregion

            // Переменные панели с ручками

            var wR = widthD/2; // Расстояние межу ручками
            if (widthD < 1000)
            {
                wR = widthD*0.5;
            }
            if (widthD >= 1000)
            {
                wR = widthD*0.45;
            }
            if (widthD >= 1300)
            {
                wR = widthD*0.4;
            }
            if (widthD >= 1700)
            {
                wR = widthD*0.35;
            }

            #region typeOfPanel != "Панель двойная"

            // Тип панели
            if (modelName == "02-01" & !typeOfPanel[1].Contains("Панель двойная"))
            {
                swDoc.Extension.SelectByID2("Ручка MLA 120-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("SC GOST 17475_gost-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("SC GOST 17475_gost-2@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Threaded Rivets-5@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Threaded Rivets-6@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();

                // Удаление ненужных элементов панели
                const int deleteOption = (int) swDeleteSelectionOptions_e.swDelete_Absorbed +
                                         (int) swDeleteSelectionOptions_e.swDelete_Children;
                swDoc.Extension.SelectByID2("Вырез-Вытянуть7@02-01-001-1@02-01", "BODYFEATURE", 0, 0, 0, false, 0, null,
                    0);
                swDoc.Extension.DeleteSelection2(deleteOption);

                swDoc.Extension.SelectByID2("Ручка MLA 120-5@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("SC GOST 17475_gost-9@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("SC GOST 17475_gost-10@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Threaded Rivets-13@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Threaded Rivets-14@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                // Удаление ненужных элементов панели
                swDoc.Extension.SelectByID2("Вырез-Вытянуть8@02-01-001-1@02-01", "BODYFEATURE", 0, 0, 0, false, 0, null,
                    0);
                swDoc.Extension.DeleteSelection2(deleteOption);
            }

            if (modelName == "02-04" || modelName == "02-05")
            {
                if (Convert.ToInt32(width) > 750)
                {
                    swDoc.Extension.SelectByID2("Ручка MLA 120-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-2@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-5@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-6@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    // Удаление ненужных элементов панели
                    const int deleteOption = (int) swDeleteSelectionOptions_e.swDelete_Absorbed +
                                             (int) swDeleteSelectionOptions_e.swDelete_Children;
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть7@02-01-001-1@02-01", "BODYFEATURE", 0, 0, 0, false, 0,
                        null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);
                }
                else
                {
                    swDoc.Extension.SelectByID2("Ручка MLA 120-5@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-9@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-10@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-13@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-14@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    // Удаление ненужных элементов панели
                    const int deleteOption = (int) swDeleteSelectionOptions_e.swDelete_Absorbed +
                                             (int) swDeleteSelectionOptions_e.swDelete_Children;
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть8@02-01-001-1@02-01", "BODYFEATURE", 0, 0, 0, false, 0,
                        null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);
                }
            }

            if (!typeOfPanel[1].Contains("Панель двойная"))
            {
                // Панель внешняя 

                #region

                //var newName = панельВнешняя.NewName;
                //var newName =modelName + "-01-" + width + "-" + lenght + "-" + "50-" + materialP1[3] + materialP1[3] == "AZ" ? "" : materialP1[1];

                #endregion

                var newName =
                    $"{modelName}-01-{width}-{height}-{thicknessOfPanel}-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";

                #region

                //var newName = String.Format("{0}{1}{2}{3}",
                //    modelName + "-01-" + width + "-" + lenght + "-" + "50-" + materialP1
                //    //string.IsNullOrEmpty(покрытие[6]) ? "" : "-" + покрытие[6],
                //    //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
                //    //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]
                //    );

                #endregion

                var newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-01.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-001-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-001.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {

                    #region before

                    //swApp.SendMsgToUser("Изменяем компонент " + @NewPartPath);
                    //SetMeterial(materialP1[0], _swApp.ActivateDoc2("02-01-001.SLDPRT", true, 0), "");// _swApp.ActivateDoc2("02-01-001.SLDPRT"
                    //try
                    //{
                    //    var setMaterials = new SetMaterials();
                    //    _swApp.ActivateDoc2("02-11-01-40-.SLDPRT", true, 0);
                    //    setMaterials.SetColor("00", покрытие[6], покрытие[1], покрытие[2], _swApp);// setMaterials.SetColor("00", "F6F6F6", "Шаргень", "2", _swApp);
                    //}
                    //catch (Exception e)
                    //{
                    //    MessageBox.Show(e.StackTrace);
                    //}

                    #endregion

                    SwPartParamsChangeWithNewName("02-01-001",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD).ToString()},
                            {"D2@Эскиз1", (widthD).ToString()},
                            {"D1@Кривая2", (rivetH).ToString()},
                            {"D1@Кривая1", (rivetW).ToString()},
                            {"D4@Эскиз30", (wR).ToString()},

                            {"D7@Ребро-кромка1", thicknessOfPanel == "50" ? "48" : "50"},

                            {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')}
                        },
                        false,
                        null);
                    try
                    {
                        VentsMatdll(materialP1, new[] {покрытие[6], покрытие[1], покрытие[2]}, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }

                    _swApp.CloseDoc(newName);
                }

                //Панель внутреняя

                var modelnewname = modelName;
                var modelPath = _destinationFolder;
                if (modelName == "02-04")
                {
                    modelnewname = "02-01";
                    modelPath = Panels0201;
                }

                #region

                //newName = String.Format("{0}{1}{2}{3}",
                //modelnewname + "-02-" + width + "-" + lenght + "-" + "50-" + materialP2[0];
                //   //string.IsNullOrEmpty(покрытие[6]) ? "" : "-" + покрытие[6],
                //   //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
                //   //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]
                //   );

                //newName = панельВнутренняя.NewName;

                #endregion

                newName =
                    $"{modelnewname}-02-{width}-{height}-50-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                //newName = modelnewname + "-02-" + width + "-" + lenght + "-" + "50-" + materialP2[3] + materialP2[3] == "AZ" ? "" : materialP2[1];

                newPartPath = $@"{Settings.Default.DestinationFolder}{modelPath}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-01.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-002-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-002.SLDPRT");
                }
                else if (!File.Exists(newPartPath))
                {

                    #region before

                    //SetMeterial(materialP2[0], _swApp.ActivateDoc2("02-01-002.SLDPRT", true, 0), "");
                    //try
                    //{
                    //    var setMaterials = new SetMaterials();
                    //    _swApp.ActivateDoc2("02-11-01-40-.SLDPRT", true, 0);
                    //    setMaterials.SetColor("00", покрытие[7], покрытие[4], покрытие[5], _swApp);// setMaterials.SetColor("00", "F6F6F6", "Шаргень", "2", _swApp);
                    //}
                    //catch (Exception e)
                    //{
                    //    MessageBox.Show(e.StackTrace);
                    //}

                    #endregion

                    SwPartParamsChangeWithNewName("02-01-002",
                        $@"{Settings.Default.DestinationFolder}{modelPath}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD - 10).ToString()},
                            {"D2@Эскиз1", (widthD - 10).ToString()},
                            {"D1@Кривая2", (rivetH).ToString()},
                            {"D1@Кривая1", (rivetW).ToString()},
                            {"Толщина@Листовой металл", materialP2[1].Replace('.', ',')},
                            {"D1@Листовой металл", (bendRadius).ToString()},
                            {"D2@Листовой металл", (kFactor*1000).ToString()}
                        },
                        false,
                        null);
                    try
                    {
                        VentsMatdll(materialP2, new[] {покрытие[7], покрытие[4], покрытие[5]}, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                    _swApp.CloseDoc(newName);
                }

                //Панель теплошумоизоляции

                if (modelName == "02-05")
                {
                    modelPath = Panels0201;
                }

                newName = "02-03-" + width + "-" + height; //newName = теплоизоляция.NewName;
                newPartPath = $@"{Settings.Default.DestinationFolder}{modelPath}\{newName}.SLDPRT";

                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-01.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-003-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-003.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-003",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD - 10).ToString()},
                            {"D2@Эскиз1", (widthD - 10).ToString()}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                //Уплотнитель

                newName = "02-04-" + width + "-" + height; //newName = уплотнитель.NewName;

                newPartPath = $@"{Settings.Default.DestinationFolder}{modelPath}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-01.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-004-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-004.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-004",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D6@Эскиз1", (heightD - 10).ToString()},
                            {"D3@Эскиз1", (widthD - 10).ToString()}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }
            }

            #endregion

            #region typeOfPanel == "Панель двойная несъемная"

            if (typeOfPanel[1].Contains("Панель двойная"))
            {

                #region before

                // Панель внешняя 
                //var newName = String.Format("{0}{1}{2}{3}",
                //    modelName + "-01-" + width + "-" + lenght + "-" + "50-" + materialP1
                //    //string.IsNullOrEmpty(покрытие[6]) ? "" : "-" + покрытие[6],
                //    //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
                //    //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]
                //    );

                //var newName = панельВнешняя.NewName;
                //var newName = modelName + "-01-" + width + "-" + lenght + "-" + "50-" + materialP1[3] + materialP1[3] == "AZ" ? "" : materialP1[1];

                #endregion

                var currDestPath = typeOfPanel[1].Contains("несъемная") ? _destinationFolder : Panels0204;
                var curNumber = typeOfPanel[1].Contains("несъемная") ? "01" : "04";

                //MessageBox.Show("currDestPath - " + currDestPath + "   curNumber -" + curNumber + " -- " + typeOfPanel[1].Contains("несъемная"));

                if (typeOfPanel[1].Contains("несъемная"))
                {
                    //MessageBox.Show("Не съемная - " + typeOfPanel[1]);

                    swDoc.Extension.SelectByID2("Ручка MLA 120-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null,
                        0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-2@02-104-50", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-1@02-104-50", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-2@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    // Удаление ненужных элементов панели
                    const int deleteOption = (int) swDeleteSelectionOptions_e.swDelete_Absorbed +
                                             (int) swDeleteSelectionOptions_e.swDelete_Children;
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@02-01-101-50-1@02-104-50", "BODYFEATURE", 0, 0, 0,
                        false, 0, null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);

                    //MessageBox.Show("Удалилось");
                }


                var newName =
                    $"{modelName}-{curNumber}-{width}-{height}-{thicknessOfPanel}-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";

                var newPartPath =
                    $@"{Settings.Default.DestinationFolder}{currDestPath}\{newName}.SLDPRT";

                //MessageBox.Show("newPartPath -" + newPartPath);

                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-101-50-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-101-50.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-101-50",
                        $@"{Settings.Default.DestinationFolder}{currDestPath}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD).ToString()},
                            {"D2@Эскиз1", (widthD/2).ToString()},
                            {"D1@Кривая4", (rivetH).ToString()},
                            {"D1@Кривая3", (rivetWd).ToString()},
                            {"D1@Кривая5", (rivetH).ToString()},


                            {"D7@Ребро-кромка2", thicknessOfPanel == "50" ? "48" : "50"},


                            {"D2@Эскиз47", (wR/2).ToString()},

                            {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')},
                            {"D1@Листовой металл", (bendRadius).ToString()},
                            {"D2@Листовой металл", (kFactor*1000).ToString()}
                        },
                        false,
                        null);
                    try
                    {
                        VentsMatdll(materialP1, new[] {покрытие[6], покрытие[1], покрытие[2]}, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                    _swApp.CloseDoc(newName);
                }

                #region Name before

                //Панель внутреняя
                //newName = String.Format("{0}{1}{2}{3}",
                //   modelName + "-02-" + width + "-" + lenght + "-" + "50-" + materialP1
                //   //string.IsNullOrEmpty(покрытие[7]) ? "" : "-" + покрытие[7],
                //   //string.IsNullOrEmpty(покрытие[3]) ? "" : "-" + покрытие[3],
                //   //string.IsNullOrEmpty(покрытие[4]) ? "" : "-" + покрытие[4]
                //   );

                //newName = панельВнутренняя.NewName;

                //newName = modelName + "-02-" + width + "-" + lenght + "-" + "50-" + materialP2[3] + materialP2[3] == "AZ" ? "" : materialP2[1];

                #endregion

                newName =
                    $"{modelName}-02-{width}-{height}-50-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-102-50-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-102-50.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-102-50",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD - 10).ToString()},
                            {"D2@Эскиз1", ((widthD - 10)/2).ToString()},
                            {"D1@Кривая3", (rivetH).ToString()},
                            {"D1@Кривая2", (rivetH).ToString()},
                            {"D1@Кривая1", (rivetWd).ToString()},
                            {"Толщина@Листовой металл", materialP2[1].Replace('.', ',')},
                            {"D1@Листовой металл", (bendRadius).ToString()},
                            {"D2@Листовой металл", (kFactor*1000).ToString()}
                        },
                        false,
                        null);

                    try
                    {
                        VentsMatdll(materialP1, new[] {покрытие[7], покрытие[4], покрытие[5]}, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }

                    _swApp.CloseDoc(newName);
                }

                #region

                // Профиль 02-01-103-50
                //newName = деталь103.NewName;
                // modelName + "-03-" + width + "-" + lenght + "-" + "50-" + materialP2[3] + materialP2[3] == "AZ" ? "" : materialP2[1];

                #endregion

                newName =
                    $"{modelName}-03-{width}-{height}-{thicknessOfPanel}-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-103-50-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-103-50.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-103-50",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD - 15).ToString()},
                            {"D1@Кривая1", (rivetH).ToString()},

                            {"D2@Эскиз1", thicknessOfPanel == "50" ? "46" : "48"},

                            {"Толщина@Листовой металл", thiknessStr},
                            {"D1@Листовой металл", (bendRadius).ToString()},
                            {"D2@Листовой металл", (kFactor*1000).ToString()}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                //Панель теплошумоизоляции

                //newName = теплоизоляция.NewName;

                newName = "02-03-" + width + "-" + height;

                newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-003-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-003.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-003",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", Convert.ToString(heightD - 10)},
                            {"D2@Эскиз1", Convert.ToString(widthD - 10)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                //Уплотнитель

                //newName = уплотнитель.NewName;

                newName = "02-04-" + width + "-" + height;

                newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-004-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-004.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-004",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D6@Эскиз1", Convert.ToString(heightD - 10)},
                            {"D3@Эскиз1", Convert.ToString(widthD - 10)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }
            }

            #endregion

            #region Несъемная

            //switch (typeOfPanel[1])
            //{
            //    case "Несъемная":
            //    case "Съемная":
            //        typeOfPanel[1] = typeOfPanel[1] + " панель";
            //        break;
            //    case "Панель теплообменника":
            //    case "Панель двойная":
            //        break;
            //}

            #endregion

            swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm, true, 0)));
            var swModelDocExt = swDoc.Extension;
            var swCustPropForDescription = swModelDocExt.CustomPropertyManager[""];
            swCustPropForDescription.Set("Наименование", typeOfPanel[1]);
            swCustPropForDescription.Set("Description", typeOfPanel[1]);

            GabaritsForPaintingCamera(swDoc);

            swDoc.EditRebuild3();
            swDoc.ForceRebuild3(true);
            swDoc.SaveAs2(new FileInfo(newPanel50Path).FullName, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false,
                true);
            NewComponents.Add(new FileInfo(newPanel50Path));
            _swApp.CloseDoc(new FileInfo(newPanel50Path).Name);
            _swApp.Visible = true;
            CheckInOutPdm(NewComponents, true, Settings.Default.TestPdmBaseName);

            foreach (var newComponent in NewComponents)
            {
               // ExportXmlSql.Export(newComponent.FullName);
            }

            if (onlyPath) return newPanel50Path;
            MessageBox.Show(newPanel50Path, "Модель построена");

            return newPanel50Path;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeOfPanel"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="materialP1"></param>
        /// <param name="materialP2"></param>
        /// <param name="покрытие"></param>
        /// <param name="path"></param>
        public void Panels30Build(string[] typeOfPanel, string width, string height, string[] materialP1,
            string[] materialP2, string[] покрытие, out string path)
        {
            path = null;

            if (!IsConvertToInt(new[] {width, height})) return;

            Логгер.Отладка("Начало построения 30-й панели. ", "", "Panels30Build", "Panels30Build");

            string modelPanelsPath;
            string modelName;
            string nameAsm;

            var modelType =
                $"30-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}-MW";

            switch (typeOfPanel[1])
            {
                case "Панель несъемная глухая":
                    modelPanelsPath = Panel30Folder;
                    nameAsm = "02-01-30";
                    modelName = "02-01";
                    _destinationFolder = Panels0201;
                    break;
                case "Панель съемная с ручками":
                    modelPanelsPath = Panel30RemovableFolder;
                    nameAsm = "02-04-30";
                    modelName = "02-04";
                    _destinationFolder = Panels0204;
                    break;
                case "Панель теплообменника":
                    modelPanelsPath = Panel30CoilFolder;
                    nameAsm = "02-05-30";
                    modelName = "02-05";
                    _destinationFolder = Panels0205;
                    break;
                case "Панель двойная несъемная":
                    modelPanelsPath = Panel30DoubleFolder;
                    nameAsm = "02-1-30";
                    modelName = "02-01";
                    _destinationFolder = Panels0201;
                    break;

                case "Панель двойная съемная":
                    modelPanelsPath = Panel30RemovableDoubleFolder;
                    nameAsm = "02-4-30";
                    modelName = "02-04";
                    _destinationFolder = Panels0201;
                    break;
                default:
                    modelPanelsPath = Panel30Folder;
                    nameAsm = "02-01";
                    modelName = "02-01";
                    break;
            }

            var newPanel30Name = modelName + "-" + width + "-" + height + "-" + modelType;

            var newDestPath = !typeOfPanel[1].Contains("Панель двойная съемная") ? _destinationFolder : Panels0204;

            Логгер.Информация("Построение панели - " + newPanel30Name, "", null, "Panels30BuildStr");

            var newPanel30Path = $@"{Settings.Default.DestinationFolder}{newDestPath}\{newPanel30Name}.SLDASM";

            if (File.Exists(new FileInfo(newPanel30Path).FullName))
            {
                path = new FileInfo(newPanel30Path).FullName;
                return;
                //MessageBox.Show(newPanel30Path, "Данная модель уже находится в базе");return;
            }

            #region modelPanelAsmbly        

            var modelPanelAsmbly = $@"{Settings.Default.SourceFolder}{modelPanelsPath}\{nameAsm}.SLDASM";

            GetLatestVersionAsmPdm(modelPanelAsmbly, Settings.Default.PdmBaseName);

            if (!InitializeSw(true)) return;

            var swDoc = _swApp.OpenDoc6(modelPanelAsmbly, (int) swDocumentTypes_e.swDocASSEMBLY,
                (int) swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            _swApp.Visible = true;

            var swAsm = (AssemblyDoc) swDoc;

            swAsm.ResolveAllLightWeightComponents(false);

            // Габариты
            var widthD = Convert.ToDouble(width);
            var heightD = Convert.ToDouble(height);
            var halfWidthD = Convert.ToDouble(widthD/2);
            // Шаг заклепок
            const double step = 100;
            var rivetW = (Math.Truncate(widthD/step) + 1)*1000;
            var rivetWd = (Math.Truncate(halfWidthD/step) + 1)*1000;
            var rivetH = (Math.Truncate(heightD/step) + 1)*1000;
            if (Math.Abs(rivetW - 1000) < 1) rivetW = 2000;
            // Коэффициенты и радиусы гибов   
            const string thiknessStr = "0,8";
            var sbSqlBaseData = new SqlBaseData();
            var bendParams = sbSqlBaseData.BendTable(thiknessStr);
            var bendRadius = Convert.ToDouble(bendParams[0]);
            var kFactor = Convert.ToDouble(bendParams[1]);

            #endregion

            // Переменные панели с ручками

            var wR = widthD/2; // Расстояние межу ручками
            if (widthD < 1000)
            {
                wR = widthD*0.5;
            }
            if (widthD >= 1000)
            {
                wR = widthD*0.45;
            }
            if (widthD >= 1300)
            {
                wR = widthD*0.4;
            }
            if (widthD >= 1700)
            {
                wR = widthD*0.35;
            }


            switch (typeOfPanel[1])
            {
                case "Панель несъемная глухая":

                    #region "Панель несъемная глухая"

                    // Панель внешняя 

                    var partModName = "02-01-30-001";
                    var newName =
                        $"{modelName}-01-{width}-{height}-30-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";
                    var newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD).ToString()},
                                {"D2@Эскиз1", (widthD).ToString()},
                                {"D1@Кривая2", (rivetH).ToString()},
                                {"D1@Кривая1", (rivetW).ToString()},
                                {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP1, new[] {покрытие[6], покрытие[1], покрытие[2]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                        _swApp.CloseDoc(newName);
                    }

                    //Панель внутреняя

                    partModName = "02-01-30-002";
                    newName =
                        $"{modelName}-02-{width}-{height}-30-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";
                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 7).ToString()},
                                {"D2@Эскиз1", (widthD - 7).ToString()},
                                {"D1@Кривая2", (rivetH).ToString()},
                                {"D1@Кривая1", (rivetW).ToString()},

                                {"Толщина@Листовой металл", materialP2[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP2, new[] {покрытие[7], покрытие[4], покрытие[5]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                        _swApp.CloseDoc(newName);
                    }

                    //Панель теплошумоизоляции

                    partModName = "02-01-30-003";
                    newName = "02-03-30-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 10).ToString()},
                                {"D2@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }

                    //Уплотнитель

                    partModName = "02-01-30-004";
                    newName = "02-04-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D6@Эскиз1", (heightD - 10).ToString()},
                                {"D3@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }
                    goto m1;

                    #endregion

                case "Панель съемная с ручками":

                    #region "Панель съемная с ручками"

                    // Панель внешняя 

                    partModName = "02-04-30-001";
                    newName =
                        $"{modelName}-01-{width}-{height}-30-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";
                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD).ToString()},
                                {"D2@Эскиз1", (widthD).ToString()},
                                {"D1@Кривая2", (rivetH).ToString()},
                                {"D1@Кривая1", (rivetW).ToString()},

                                {"D1@Эскиз25", (wR/2).ToString()},

                                {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP1, new[] {покрытие[6], покрытие[1], покрытие[2]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                        _swApp.CloseDoc(newName);
                    }

                    //Панель внутреняя

                    partModName = "02-04-30-002";
                    newName =
                        $"{modelName}-02-{width}-{height}-30-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";
                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 7).ToString()},
                                {"D2@Эскиз1", (widthD - 7).ToString()},
                                {"D1@Кривая2", (rivetH).ToString()},
                                {"D1@Кривая1", (rivetW).ToString()},

                                {"Толщина@Листовой металл", materialP2[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP2, new[] {покрытие[7], покрытие[4], покрытие[5]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                        _swApp.CloseDoc(newName);
                    }

                    //Панель теплошумоизоляции

                    partModName = "02-04-30-003";
                    newName = "02-03-30-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 10).ToString()},
                                {"D2@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }

                    //Уплотнитель

                    partModName = "02-04-30-004";
                    newName = "02-04-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D6@Эскиз1", (heightD - 10).ToString()},
                                {"D3@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }

                    goto m1;

                    #endregion

                case "Панель теплообменника":

                    #region "Панель теплообменника"

                    // Панель внешняя 

                    partModName = "02-05-30-001";
                    newName =
                        $"{modelName}-01-{width}-{height}-30-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";
                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD).ToString()},
                                {"D2@Эскиз1", (widthD).ToString()},
                                {"D1@Кривая2", (rivetH).ToString()},
                                {"D1@Кривая1", (rivetW).ToString()},

                                {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP1, new[] {покрытие[6], покрытие[1], покрытие[2]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                        _swApp.CloseDoc(newName);
                    }

                    //Панель внутреняя

                    partModName = "02-05-30-002";
                    newName =
                        $"{modelName}-02-{width}-{height}-30-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";
                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 7).ToString()},
                                {"D2@Эскиз1", (widthD - 7).ToString()},
                                {"D1@Кривая2", (rivetH).ToString()},
                                {"D1@Кривая1", (rivetW).ToString()},

                                {"Толщина@Листовой металл", materialP2[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP2, new[] {покрытие[7], покрытие[4], покрытие[5]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                        _swApp.CloseDoc(newName);
                    }

                    //Панель теплошумоизоляции

                    partModName = "02-05-30-003";
                    newName = "02-03-30-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 10).ToString()},
                                {"D2@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }

                    //Уплотнитель

                    partModName = "02-05-30-004";
                    newName = "02-04-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D6@Эскиз1", (heightD - 10).ToString()},
                                {"D3@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }

                    goto m1;

                    #endregion

                case "Панель двойная несъемная":

                    #region "Панель двойная несъемная"

                    // Панель внешняя 

                    partModName = "02-1-30-101";
                    newName =
                        $"{modelName}-01-{width}-{height}-30-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";
                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD).ToString()},
                                {"D2@Эскиз1", (widthD/2).ToString()},
                                {"D1@Кривая4", (rivetH).ToString()},
                                {"D1@Кривая3", (Math.Abs(rivetW/2) + 1).ToString()},
                                {"D1@Кривая5", (rivetH).ToString()},


                                {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP1, new[] {покрытие[6], покрытие[1], покрытие[2]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                        _swApp.CloseDoc(newName);
                    }

                    //Панель внутреняя

                    partModName = "02-1-30-102";
                    newName =
                        $"{modelName}-02-{width}-{height}-30-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";
                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 7).ToString()},
                                {"D2@Эскиз1", ((widthD - 7)/2).ToString()},
                                {"D1@Кривая2", (rivetH).ToString()},
                                {"D1@Кривая1", (Math.Abs(rivetW/2) + 1).ToString()},

                                {"D1@Кривая3", (rivetH).ToString()},

                                {"Толщина@Листовой металл", materialP2[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP2, new[] {покрытие[7], покрытие[4], покрытие[5]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                        _swApp.CloseDoc(newName);
                    }


                    //Профиль

                    partModName = "02-4-30-103";

                    newName = $"{modelName}-05-{height}";
                        // -30-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 15).ToString()},
                                {"D1@Кривая1", (rivetH).ToString()}
                            },
                            false,
                            null);
                        try
                        {
                            //VentsMatdll(materialP2, new[] { покрытие[7], покрытие[4], покрытие[5] }, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                        _swApp.CloseDoc(newName);
                    }

                    //Панель теплошумоизоляции

                    partModName = "02-04-30-003";
                    newName = "02-03-30-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 10).ToString()},
                                {"D2@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }

                    //Уплотнитель

                    partModName = "02-04-30-004";
                    newName = "02-04-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D6@Эскиз1", (heightD - 10).ToString()},
                                {"D3@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }

                    goto m1;

                    #endregion

                case "Панель двойная съемная":

                    #region "Панель двойная съемная"

                    // Панель внешняя 

                    partModName = "02-4-30-101";
                    newName =
                        $"{modelName}-01-{width}-{height}-30-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";
                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD).ToString()},
                                {"D2@Эскиз1", (widthD/2).ToString()},
                                {"D1@Кривая4", (rivetH).ToString()},
                                {"D1@Кривая3", (Math.Abs(rivetW/2) + 1).ToString()},
                                {"D1@Кривая5", (rivetH).ToString()},

                                {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP1, new[] {покрытие[6], покрытие[1], покрытие[2]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                        _swApp.CloseDoc(newName);
                    }

                    //Панель внутреняя

                    partModName = "02-4-30-102";
                    newName =
                        $"{modelName}-02-{width}-{height}-30-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";
                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 7).ToString()},
                                {"D2@Эскиз1", ((widthD - 7)/2).ToString()},
                                {"D1@Кривая2", (rivetH).ToString()},
                                {"D1@Кривая1", (Math.Abs(rivetW/2) + 1).ToString()},

                                {"D1@Кривая3", (rivetH).ToString()},

                                {"Толщина@Листовой металл", materialP2[1].Replace('.', ',')}
                            },
                            false,
                            null);
                        try
                        {
                            VentsMatdll(materialP2, new[] {покрытие[7], покрытие[4], покрытие[5]}, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                        _swApp.CloseDoc(newName);
                    }


                    //Профиль

                    partModName = "02-4-30-103";

                    newName = $"{modelName}-05-{height}";
                        // -30-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                    newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 15).ToString()},
                                {"D1@Кривая1", (rivetH).ToString()}
                            },
                            false,
                            null);
                        try
                        {
                            //VentsMatdll(materialP2, new[] { покрытие[7], покрытие[4], покрытие[5] }, newName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                        _swApp.CloseDoc(newName);
                    }

                    //Панель теплошумоизоляции

                    partModName = "02-04-30-003";
                    newName = "02-03-30-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D1@Эскиз1", (heightD - 10).ToString()},
                                {"D2@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }

                    //Уплотнитель

                    partModName = "02-04-30-004";
                    newName = "02-04-" + width + "-" + height;
                    newPartPath = $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}.SLDPRT";

                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2(partModName + "-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null,
                            0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc(partModName + ".SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName(partModName,
                            $@"{Settings.Default.DestinationFolder}{Panels0201}\Materials\{newName}",
                            new[,]
                            {
                                {"D6@Эскиз1", (heightD - 10).ToString()},
                                {"D3@Эскиз1", (widthD - 10).ToString()}
                            },
                            false,
                            null);
                        _swApp.CloseDoc(newName);
                    }

                    goto m1;

                    #endregion
            }            

            #region typeOfPanel == "Панель двойная несъемная"

            if (typeOfPanel[1].Contains("Панель двойная"))
            {

                var currDestPath = typeOfPanel[1].Contains("несъемная") ? _destinationFolder : Panels0204;
                var curNumber = typeOfPanel[1].Contains("несъемная") ? "01" : "04";
                //MessageBox.Show("currDestPath - " + currDestPath + "   curNumber -" + curNumber + " -- " + typeOfPanel[1].Contains("несъемная"));

                if (typeOfPanel[1].Contains("несъемная"))
                {
                    //MessageBox.Show("Не съемная - " + typeOfPanel[1]);
                    swDoc.Extension.SelectByID2("Ручка MLA 120-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null,
                        0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-2@02-104-50", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-1@02-104-50", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-2@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    // Удаление ненужных элементов панели
                    const int deleteOption = (int) swDeleteSelectionOptions_e.swDelete_Absorbed +
                                             (int) swDeleteSelectionOptions_e.swDelete_Children;
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@02-01-101-50-1@02-104-50", "BODYFEATURE", 0, 0, 0,
                        false, 0, null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);
                }


                var newName =
                    $"{modelName}-{curNumber}-{width}-{height}-30-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";

                var newPartPath =
                    $@"{Settings.Default.DestinationFolder}{currDestPath}\{newName}.SLDPRT";

                //MessageBox.Show("newPartPath -" + newPartPath);

                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-101-50-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-101-50.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-101-50",
                        $@"{Settings.Default.DestinationFolder}{currDestPath}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD).ToString()},
                            {"D2@Эскиз1", (widthD/2).ToString()},
                            {"D1@Кривая4", (rivetH).ToString()},
                            {"D1@Кривая3", (rivetWd).ToString()},
                            {"D1@Кривая5", (rivetH).ToString()},
                            
                            //{"D7@Ребро-кромка2", thicknessOfPanel == "50" ? "48" : "50"},

                            {"D2@Эскиз47", (wR/2).ToString()},

                            {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')},
                            {"D1@Листовой металл", (bendRadius).ToString()},
                            {"D2@Листовой металл", (kFactor*1000).ToString()}
                        },
                        false,
                        null);
                    try
                    {
                        VentsMatdll(materialP1, new[] {покрытие[6], покрытие[1], покрытие[2]}, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                    _swApp.CloseDoc(newName);
                }

                newName =
                    $"{modelName}-02-{width}-{height}-50-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-102-50-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-102-50.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-102-50",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD - 10).ToString()},
                            {"D2@Эскиз1", ((widthD - 10)/2).ToString()},
                            {"D1@Кривая3", (rivetH).ToString()},
                            {"D1@Кривая2", (rivetH).ToString()},
                            {"D1@Кривая1", (rivetWd).ToString()},
                            {"Толщина@Листовой металл", materialP2[1].Replace('.', ',')},
                            {"D1@Листовой металл", (bendRadius).ToString()},
                            {"D2@Листовой металл", (kFactor*1000).ToString()}
                        },
                        false,
                        null);

                    try
                    {
                        VentsMatdll(materialP1, new[] {покрытие[7], покрытие[4], покрытие[5]}, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }

                    _swApp.CloseDoc(newName);
                }

                newName =
                    $"{modelName}-03-{width}-{height}-30-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-103-50-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-103-50.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-103-50",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD - 15).ToString()},
                            {"D1@Кривая1", (rivetH).ToString()},

                            //{"D2@Эскиз1", thicknessOfPanel == "50" ? "46" : "48"},

                            {"Толщина@Листовой металл", thiknessStr},
                            {"D1@Листовой металл", (bendRadius).ToString()},
                            {"D2@Листовой металл", (kFactor*1000).ToString()}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                //Панель теплошумоизоляции

                //newName = теплоизоляция.NewName;

                newName = "02-03-" + width + "-" + height;

                newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-003-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-003.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-003",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", Convert.ToString(heightD - 10)},
                            {"D2@Эскиз1", Convert.ToString(widthD - 10)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }

                //Уплотнитель

                //newName = уплотнитель.NewName;

                newName = "02-04-" + width + "-" + height;

                newPartPath = $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2) (_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-004-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-004.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-004",
                        $@"{Settings.Default.DestinationFolder}{_destinationFolder}\{newName}",
                        new[,]
                        {
                            {"D6@Эскиз1", Convert.ToString(heightD - 10)},
                            {"D3@Эскиз1", Convert.ToString(widthD - 10)}
                        },
                        false,
                        null);
                    _swApp.CloseDoc(newName);
                }
            }

            #endregion

            m1:

            swDoc = ((ModelDoc2) (_swApp.ActivateDoc2(nameAsm, true, 0)));
            var swModelDocExt = swDoc.Extension;
            var swCustPropForDescription = swModelDocExt.CustomPropertyManager[""];
            swCustPropForDescription.Set("Наименование", typeOfPanel[1]);
            swCustPropForDescription.Set("Description", typeOfPanel[1]);

            GabaritsForPaintingCamera(swDoc);

            swDoc.EditRebuild3();
            swDoc.ForceRebuild3(true);
            swDoc.SaveAs2(new FileInfo(newPanel30Path).FullName, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false,
                true);
            path = new FileInfo(newPanel30Path).FullName;
            NewComponents.Add(new FileInfo(newPanel30Path));
            _swApp.CloseDoc(new FileInfo(newPanel30Path).Name);
            _swApp.Visible = true;
            CheckInOutPdm(NewComponents, true, Settings.Default.TestPdmBaseName);
            
            #region Выгрузка XML

            foreach (var newComponent in NewComponents)
            {
                //  PartInfoToXml(newComponent.FullName);
            }

            #endregion
        }

        private void VentsMatdll(IList<string> materialP1, IList<string> покрытие, string newName)
        {
            ModelDoc2 model = _swApp.ActivateDoc2(newName, true, 0);
            if (model == null) return;            

            try
            {
                //MessageBox.Show(newName);
                
                var setMaterials = new SetMaterials();
                ToSQL.Conn = Settings.Default.ConnectionToSQL;
                var toSql = new ToSQL();
                
                //MessageBox.Show($"Conn - {ToSQL.Conn} SetMaterials {setMaterials == null} toSql - {toSql == null} _swApp {_swApp == null} levelId - {Convert.ToInt32(materialP1[0])}");

                setMaterials?.ApplyMaterial("", "00", Convert.ToInt32(materialP1[0]), _swApp);
                model?.Save();

                foreach (var confname in setMaterials.GetConfigurationNames(_swApp))
                {
                    foreach (var matname in setMaterials.GetCustomProperty(confname, _swApp))
                    {
                        toSql.AddCustomProperty(confname, matname.Name, _swApp);
                    }
                }

                if (покрытие != null)
                {
                    if (покрытие[1] != "0")
                    {
                        setMaterials.SetColor("00", покрытие[0], покрытие[1], покрытие[2], _swApp);
                    }
                    _swApp.IActiveDoc2.Save();
                }

                try
                {
                    string message;
                    setMaterials.CheckSheetMetalProperty("00", _swApp, out message);
                    if (message != null)
                    {
                       // MessageBox.Show(message, newName + " 858 ");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"{newName}\n{e.Message}\n{e.StackTrace}", "VentsMatdll");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"{newName}\n{e.Message}\n{e.StackTrace}\n{newName}", "VentsMatdll 2");
            }

            GabaritsForPaintingCamera(model);

            model?.Save();
        }
    
        #endregion

        #region Montage Frame

        /// <summary>
        /// Метод по генерации монтажной рамы для блоков и установок
        /// </summary>
        /// <param name="widthS">ширина монтажной рамы</param>
        /// <param name="lenghtS">длина рамы</param>
        /// <param name="thiknessS">толщина материала рамы</param>
        /// <param name="typeOfMf">тип рамы</param>
        /// <param name="frameOffset">отступ поперечной балки</param>
        /// <param name="material">материал рамы</param>
        /// <param name="покрытие">The ПОКРЫТИЕ.</param>
        public void MontageFrame(string widthS, string lenghtS, string thiknessS, string typeOfMf, string frameOffset,
            string material, string[] покрытие)
        {
            var path = MontageFrameS(widthS, lenghtS, thiknessS, typeOfMf, frameOffset, material, покрытие, false);         
        }

        /// <summary>
        /// Montages the frame s.
        /// </summary>
        /// <param name="widthS">The width s.</param>
        /// <param name="lenghtS">The lenght s.</param>
        /// <param name="thiknessS">The thikness s.</param>
        /// <param name="typeOfMf">The type of mf.</param>
        /// <param name="frameOffset">The frame offset.</param>
        /// <param name="material">The material.</param>
        /// <param name="покрытие">The ПОКРЫТИЕ.</param>
        /// <param name="onlyPath">The ПОКРЫТИЕ.</param>
        /// <returns></returns>
        public string MontageFrameS(string widthS, string lenghtS, string thiknessS, string typeOfMf, string frameOffset,
            string material, IList<string> покрытие, bool onlyPath)
        {
            var addMatName = "";

            if (material != "1800" & thiknessS == "2")
            {
                addMatName = "HK";
            }

            #region Проверка введенных значений и открытие сборки

            if (IsConvertToDouble(new[] {widthS, lenghtS, thiknessS, frameOffset}) == false)
            {
                return "";
            }

            if (!InitializeSw(true)) return "";

            var typeOfMfs = "-0" + typeOfMf;
            if (typeOfMf == "0")
            {
                typeOfMfs = "";
            }

            // Тип рымы
            var internalCrossbeam = false; // Погашение внутренней поперечной балки
            var internalLongitudinalBeam = false; // Погашение внутренней продольной балки
            var frameOffcetStr = "";
            switch (typeOfMf)
            {
                case "1":
                    internalCrossbeam = true;
                    break;
                case "2":
                    internalLongitudinalBeam = true;
                    break;
                case "3":
                    internalCrossbeam = true;
                    frameOffcetStr = "-" + frameOffset;
                    break;
            }

            var newMontageFrameName = string.Format("10-{0}{5}-{1}-{2}{3}{4}.SLDASM", thiknessS, widthS, lenghtS,
                typeOfMfs, frameOffcetStr, addMatName);

            #region To delete

            //var newMontageFrameName = String.Format("10-{0}-{1}-{2}{3}-{4}{5}{6}{7}.SLDASM",
            //    thiknessS,
            //    widthS,
            //    lenghtS,
            //    typeOfMfs,
            //    material,
            //    string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
            //    string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
            //    string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]
            //    );

            #endregion

            var newMontageFramePath =
                $@"{Settings.Default.DestinationFolder}{BaseFrameDestinationFolder}\{newMontageFrameName}";
            if (File.Exists(newMontageFramePath))
            {
                if (onlyPath) return newMontageFramePath;

                MessageBox.Show(newMontageFramePath, "Данная модель уже находится в базе");
                return "";

                #region To delete

                //GetLastVersionPdm(new FileInfo(newMontageFramePath).FullName, Settings.Default.TestPdmBaseName);
                //_swApp.OpenDoc6(newMontageFramePath, (int)swDocumentTypes_e.swDocASSEMBLY, (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                //return newMontageFramePath;

                #endregion

            }
            var modelMontageFramePath = $@"{Settings.Default.SourceFolder}{BaseFrameFolder}\{"10-4"}.SLDASM";

            #region to delete

            //var components = new[]
            //{
            //    modelMontageFramePath,
            //    //String.Format(@"{0}{1}\{2}", _pdmFolder, BaseFrameFolder,"10-01-4.SLDASM"),
            //    String.Format(@"{0}{1}\{2}", _pdmFolder, BaseFrameFolder,"10-04-4.SLDPRT"),
            //    //String.Format(@"{0}{1}\{2}", _pdmFolder, BaseFrameFolder,"10-03-4.SLDASM"),
            //    String.Format(@"{0}{1}\{2}", _pdmFolder, BaseFrameFolder,"10-01-01-4.SLDPRT"),
            //    String.Format(@"{0}{1}\{2}", _pdmFolder, BaseFrameFolder,"10-03-01-4.SLDPRT"),
            //    String.Format(@"{0}{1}{2}", _pdmFolder, @"Vents-PDM\Библиотека проектирования\Стандартные изделия\", "Washer 11371_gost.SLDPRT"),
            //    String.Format(@"{0}{1}{2}", _pdmFolder, @"Vents-PDM\Библиотека проектирования\Прочие изделия\Крепежные изделия\", "Регулируемая ножка.SLDASM"),
            //    String.Format(@"{0}{1}{2}", _pdmFolder, @"Vents-PDM\Библиотека проектирования\Стандартные изделия\", "Washer 6402_gost.SLDPRT"),
            //    String.Format(@"{0}{1}{2}", _pdmFolder, @"Vents-PDM\Библиотека проектирования\Стандартные изделия\", "Hex Bolt 7805_gost.SLDPRT"),
            //    String.Format(@"{0}{1}{2}", _pdmFolder, @"Vents-PDM\Библиотека проектирования\Стандартные изделия\", "Hex Nut 5915_gost.SLDPRT")
            //};
            //GetLastVersionPdm(components, Settings.Default.PdmBaseName);

            #endregion

            GetLatestVersionAsmPdm(modelMontageFramePath, Settings.Default.PdmBaseName);
            GetLatestVersionAsmPdm(Settings.Default.DestinationFolder + BaseFrameFolder + "\\10-02-01-4.SLDPRT",
                Settings.Default.PdmBaseName);

            if (!Warning()) return "";
            var swDocMontageFrame = _swApp.OpenDoc6(modelMontageFramePath, (int) swDocumentTypes_e.swDocASSEMBLY,
                (int) swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
            _swApp.Visible = true;
            var swAsm = (AssemblyDoc) swDocMontageFrame;
            swAsm.ResolveAllLightWeightComponents(false);

            #endregion

            #region Основные размеры, величины и переменные

            // Габариты Ширина меньше ширины установки на 20мм Длина по размеру блока
            var width = GetInt(widthS); // Поперечные балки
            var lenght = GetInt(lenghtS); // Продольная балка
            var offsetI = GetInt(Convert.ToString(Convert.ToDouble(frameOffset)*10)); // Смещение поперечной балки
            if (offsetI > (lenght - 125)*10)
            {
                offsetI = (lenght - 250)*10;
                MessageBox.Show("Смещение превышает допустимое значение! Программой установлено - " +
                                (offsetI/10));
            }

            #region  Металл и х-ки гибки

            // Коэффициенты и радиусы гибов  
            var sqlBaseData = new SqlBaseData();
            var bendParams = sqlBaseData.BendTable(thiknessS);
            var bendRadius = Convert.ToDouble(bendParams[0]);
            var kFactor = Convert.ToDouble(bendParams[1]);

            #endregion

            #endregion

            #region Изменение размеров элементов и компонентов сборки

            var thikness = Convert.ToDouble(thiknessS)/1000;
            bendRadius = bendRadius/1000;
            var w = Convert.ToDouble(width)/1000;
            var l = Convert.ToDouble(lenght)/1000;
            var offset = Convert.ToDouble(offsetI)/10000;
            var offsetMirror = Convert.ToDouble(lenght*10 - offsetI)/10000;

            #region 10-02-4 Зеркальная 10-01-4

            if (typeOfMf == "3")
            {
                swDocMontageFrame.Extension.SelectByID2("10-01-01-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm = ((AssemblyDoc) (swDocMontageFrame));
                swAsm.ReplaceComponents(Settings.Default.DestinationFolder + BaseFrameFolder + "\\10-02-01-4.SLDPRT", "",
                    false, true);
                swAsm.ResolveAllLightWeightComponents(false);

                // var newMirrorAsm = (ModelDocSw)_swApp.ActivateDoc("10-02-4.SLDASM"); var swMirrorAsm = ((AssemblyDoc)(newMirrorAsm)); swMirrorAsm.ResolveAllLightWeightComponents(false);

                //Продольная зеркальная балка (Длина установки)
                swDocMontageFrame.Extension.SelectByID2("D1@Эскиз1@10-02-01-4-1@10-4", "DIMENSION", 0, 0, 00, false, 0,
                    null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("D1@Эскиз1@10-02-01-4.Part"))).SystemValue = l;
                    //  Длина установки  0.8;

                swDocMontageFrame.Extension.SelectByID2("D3@Эскиз25@10-02-01-4-1@10-4", "DIMENSION", 0, 0, 0, false, 0,
                    null, 0);
                // boolstatus = swDocMontageFrame.Extension.SelectByID2("D3@Эскиз25@10-01-4-1@10-4/10-01-01-4-1@10-01-4", "DIMENSION", 0, 0, 00, false, 0, null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("D3@Эскиз25@10-02-01-4.Part"))).SystemValue = offsetMirror;
                    //Смещение поперечной балки от края;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-02-01-4-1@10-4", "BODYFEATURE", 0, 0, 0,
                    false, 0, null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-02-01-4-1@10-4", "DIMENSION", 0, 0, 0,
                    false, 0, null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("D1@Листовой металл@10-02-01-4.Part"))).SystemValue =
                    bendRadius; // Радиус гиба  0.005;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-02-01-4-1@10-4", "BODYFEATURE", 0, 0, 0,
                    false, 0, null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-02-01-4-1@10-4", "DIMENSION", 0, 0, 0,
                    false, 0, null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("D2@Листовой металл@10-01-01-4.Part"))).SystemValue = kFactor;
                    // K-Factor  0.55;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-02-01-4-1@10-4", "DIMENSION", 0, 0,
                    0, false, 0, null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("Толщина@Листовой металл@10-02-01-4.Part"))).SystemValue =
                    thikness; // Толщина Листового металла 0.006;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.ClearSelection2(true);
            }


            #endregion

            //swApp.SendMsgToUser(string.Format("Thikness= {0}, BendRadius= {1}, Ширина= {2}, Длина= {3}, ", Thikness * 1000, BendRadius * 1000, Ширина * 1000, Длина * 1000));

            //Продольные балки (Длина установки)

            #region 10-01-4

            swDocMontageFrame.Extension.SelectByID2("D1@Эскиз1@10-01-01-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null,
                0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Эскиз1@10-01-01-4.Part"))).SystemValue = l;
                //  Длина установки  0.8;
            swDocMontageFrame.Extension.SelectByID2("D3@Эскиз25@10-01-01-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null,
                0);
            ((Dimension) (swDocMontageFrame.Parameter("D3@Эскиз25@10-01-01-4.Part"))).SystemValue = offset;
                //Смещение поперечной балки от края;
            swDocMontageFrame.EditRebuild3();
            //swApp.SendMsgToUser(Offset.ToString());
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-01-01-4-2@10-4", "BODYFEATURE", 0, 0, 0, false,
                0, null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-01-01-4-2@10-4", "DIMENSION", 0, 0, 0, false,
                0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Листовой металл@10-01-01-4.Part"))).SystemValue = bendRadius;
                // Радиус гиба  0.005;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-01-01-4-2@10-4", "BODYFEATURE", 0, 0, 0, false,
                0, null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-01-01-4-2@10-4", "DIMENSION", 0, 0, 0, false,
                0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D2@Листовой металл@10-01-01-4.Part"))).SystemValue = kFactor;
                // K-Factor  0.55;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-01-01-4-2@10-4", "DIMENSION", 0, 0, 0,
                false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("Толщина@Листовой металл@10-01-01-4.Part"))).SystemValue =
                thikness; // Толщина Листового металла 0.006;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.ClearSelection2(true);

            #endregion

            #region 10-04-4-2

            swDocMontageFrame.Extension.SelectByID2("D1@Эскиз1@10-04-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Эскиз1@10-04-4.Part"))).SystemValue = (l - 0.14);
                // Длина установки - 140  0.66;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-04-4-2@10-4", "BODYFEATURE", 0, 0, 0, false, 0,
                null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-04-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0,
                null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Листовой металл@10-04-4.Part"))).SystemValue = bendRadius;
                // Радиус гиба  0.005;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-04-4-2@10-4", "BODYFEATURE", 0, 0, 0, false, 0,
                null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-04-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0,
                null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D2@Листовой металл@10-04-4.Part"))).SystemValue = kFactor;
                // K-Factor  0.55;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-04-4-2@10-4", "DIMENSION", 0, 0, 0,
                false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("Толщина@Листовой металл@10-04-4.Part"))).SystemValue = thikness;
                // Толщина Листового металла 0.006;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.ClearSelection2(true);

            #endregion

            //Поперечная балка (Ширина установки)

            #region 10-03-4

            swDocMontageFrame.Extension.SelectByID2("D2@Эскиз1@10-03-01-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null,
                0);
            ((Dimension) (swDocMontageFrame.Parameter("D2@Эскиз1@10-03-01-4.Part"))).SystemValue = (w - 0.12);
                //  Ширина установки - 20 - 100  0.88;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-03-01-4-2@10-4", "BODYFEATURE", 0, 0, 0, false,
                0, null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-03-01-4-2@10-4", "DIMENSION", 0, 0, 0, false,
                0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Листовой металл@10-03-01-4.Part"))).SystemValue = bendRadius;
                // Радиус гиба  0.005;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-03-01-4-2@10-4", "BODYFEATURE", 0, 0, 0, false,
                0, null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-03-01-4-2@10-4", "DIMENSION", 0, 0, 0, false,
                0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D2@Листовой металл@10-03-01-4.Part"))).SystemValue = kFactor;
                // K-Factor  0.55;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-03-01-4-2@10-4", "DIMENSION", 0, 0, 0,
                false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("Толщина@Листовой металл@10-03-01-4.Part"))).SystemValue =
                thikness; // Толщина Листового металла 0.006;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.ClearSelection2(true);

            #endregion

            #endregion

            #region Удаление поперечной балки

            if (internalCrossbeam == false)
            {
                swDocMontageFrame.Extension.SelectByID2("10-03-01-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-39@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-40@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-23@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-19@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-22@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-41@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-42@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-24@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-20@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-23@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-43@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-44@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-25@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-21@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-24@10-4", "COMPONENT", 0, 0, 0, true, 0,null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-45@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-46@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-26@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-22@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-25@10-4", "COMPONENT", 0, 0, 0, true, 0,
                    null, 0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-25@10-4", "COMPONENT", 0, 0, 0, false, 0,
                    null, 0);
                swDocMontageFrame.EditDelete();

                // Удаление ненужных элементов продольной балки
                const int deleteOption =
                    (int) swDeleteSelectionOptions_e.swDelete_Absorbed +
                    (int) swDeleteSelectionOptions_e.swDelete_Children;
                swDocMontageFrame.Extension.SelectByID2("Вырез-Вытянуть8@10-01-01-4-2@10-4", "BODYFEATURE", 0, 0, 0,
                    false, 0, null, 0);
                swDocMontageFrame.Extension.DeleteSelection2(deleteOption);
            }

            #endregion

            #region Удаление продольной балки

            // Погашение внутренней продольной балки
            if (internalLongitudinalBeam == false)
            {
                foreach (var s in new [] {"5", "6", "7", "8", "13"})
                {
                    swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-" + s + "@10-4", "COMPONENT", 0, 0, 0, true, 0, null,0);
                    swDocMontageFrame.EditDelete();
                }
                foreach (var s in new[] { "6", "7", "8", "9", "37", "38", "39", "40" })
                {
                    swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-" + s + "@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocMontageFrame.EditDelete();
                }
                foreach (var s in new[] { "17", "18", "19", "20", "21", "22", "23", "24", "57", "58", "59", "60" })
                {
                    swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-" + s + "@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocMontageFrame.EditDelete();
                }

                swDocMontageFrame.Extension.SelectByID2("10-04-4-2@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.EditDelete();

               
                // Удаление ненужных элементов поперечной балки
                swDocMontageFrame.Extension.SelectByID2("Регулируемая ножка-10@10-4", "COMPONENT", 0, 0, 0, false, 0,null, 0);
                swDocMontageFrame.EditDelete();
                swDocMontageFrame.Extension.SelectByID2("Регулируемая ножка-11@10-4", "COMPONENT", 0, 0, 0, false, 0,null, 0);
                swDocMontageFrame.EditDelete();

                foreach (var s in new[] { "10", "11", "40", "41", "42", "43"})
                {
                    swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-" + s + "@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocMontageFrame.EditDelete();
                }
                
                const int deleteOption =
                   (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                   (int)swDeleteSelectionOptions_e.swDelete_Children;
                swDocMontageFrame.Extension.SelectByID2("Вырез-Вытянуть5@10-03-01-4-2@10-4", "BODYFEATURE", 0, 0, 0,
                    false, 0, null, 0);
                swDocMontageFrame.Extension.DeleteSelection2(deleteOption);
                swDocMontageFrame.Extension.SelectByID2("Вырез-Вытянуть4@10-03-01-4-2@10-4", "BODYFEATURE", 0, 0, 0,
                    false, 0, null, 0);
                swDocMontageFrame.Extension.DeleteSelection2(deleteOption);
            }

            #endregion

            #region Сохранение элементов и сборки, а также применение материалов

            #region Детали

            //Продольные балки (Длина установки)

            #region 10-01-01-4 - Деталь

            _swApp.IActivateDoc2("10-01-01-4", false, 0);
            IModelDoc2 swPartDoc = _swApp.IActiveDoc2;
            switch (typeOfMf)
            {
                //case "2":
                case "0":
                    typeOfMfs = "";
                    break;
                case "3":
                case "2":
                case "1":
                    typeOfMfs = "-0" + typeOfMf;
                    break;
            }

            //var newPartName = String.Format("10-01-01-{0}-{1}{2}{3}-{4}{5}{6}{7}.SLDPRT",
            //    thiknessS,
            //    lenght,
            //    frameOffcetStr,
            //    typeOfMfs,
            //    material,
            //    string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
            //    string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
            //    string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]);

            var newPartName = string.Format("10-01-01-{0}{4}-{1}{2}{3}.SLDPRT", thiknessS, lenght, frameOffcetStr,
                typeOfMfs, addMatName);

            var newPartPath = $@"{Settings.Default.DestinationFolder}{BaseFrameDestinationFolder}\{newPartName}";
            if (File.Exists(newPartPath))
            {
                swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                swDocMontageFrame.Extension.SelectByID2("10-01-01-4-2@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("10-01-01-4.SLDPRT");
            }
            else
            {
                swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

                VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);

                _swApp.CloseDoc(newPartName);
                NewComponents.Add(new FileInfo(newPartPath));
            }

            #endregion

            //

            #region 10-02-01-4 - Деталь Зеркальная 10-01-01-4

            if (typeOfMf == "3")
            {
                _swApp.IActivateDoc2("10-02-01-4", false, 0);
                swPartDoc = _swApp.IActiveDoc2;
                switch (typeOfMf)
                {
                    case "2":
                    case "0":
                        typeOfMfs = "";
                        break;
                    case "3":
                    case "1":
                        typeOfMfs = "-0" + typeOfMf;
                        break;
                }

                //newPartName = String.Format("10-02-01-{0}-{1}{2}{3}-{4}{5}{6}{7}.SLDPRT",
                //thiknessS,
                //lenght,
                //frameOffcetStr,
                //typeOfMfs,
                //material,
                //string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
                //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
                //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]);

                newPartName = string.Format("10-02-01-{0}{4}-{1}{2}{3}.SLDPRT", thiknessS, lenght, frameOffcetStr,
                    typeOfMfs, addMatName);

                newPartPath = $@"{Settings.Default.DestinationFolder}{BaseFrameDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                    swDocMontageFrame.Extension.SelectByID2("10-02-01-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("10-02-01-4.SLDPRT");
                }
                else
                {
                    swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);

                    _swApp.CloseDoc(newPartName);
                    NewComponents.Add(new FileInfo(newPartPath));
                }
            }

            #endregion

            #region 10-04-4 - Деталь

            if (internalLongitudinalBeam)
            {
                _swApp.IActivateDoc2("10-04-4", false, 0);
                swPartDoc = ((IModelDoc2) (_swApp.ActiveDoc));

                #region Деталь

                //newPartName = String.Format("10-04-{0}-{1}{2}{3}-{4}{5}.SLDPRT",
                //thiknessS,
                //lenght - 140,
                //material,
                //string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
                //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
                //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]);

                #endregion

                newPartName = string.Format("10-04-{0}{2}-{1}.SLDPRT", thiknessS, (lenght - 140), addMatName);

                newPartPath = $@"{Settings.Default.DestinationFolder}{BaseFrameDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                    swDocMontageFrame.Extension.SelectByID2("10-04-4-2@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("10-04-4.SLDPRT");
                }
                else
                {
                    swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);

                    _swApp.CloseDoc(newPartName);
                    NewComponents.Add(new FileInfo(newPartName));
                }
            }
            else
            {
                _swApp.CloseDoc("10-04-4.SLDPRT");
            }

            #endregion

            //Поперечная балка (Ширина установки)

            #region 10-03-01-4 - Деталь

            _swApp.IActivateDoc2("10-03-01-4", false, 0);
            swPartDoc = ((IModelDoc2) (_swApp.ActiveDoc));
            switch (typeOfMf)
            {
                case "3":
                case "2":
                    typeOfMfs = "-0" + typeOfMf;
                    break;
                case "1":
                case "0":
                    typeOfMfs = "";
                    break;
            }


            newPartName = string.Format("10-03-01-{0}{3}-{1}{2}.SLDPRT", thiknessS, (width - 120), typeOfMfs, addMatName);

            newPartPath = $@"{Settings.Default.DestinationFolder}{BaseFrameDestinationFolder}\{newPartName}";
            var newPrt0202 = string.Format("10-02-01-{0}{3}-{1}{2}.SLDPRT", thiknessS, (width - 120), typeOfMfs,
                addMatName);
            newPrt0202 = $@"{Settings.Default.DestinationFolder}{BaseFrameDestinationFolder}\{newPrt0202}";

            if (File.Exists(newPartPath))
            {
                swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                swDocMontageFrame.Extension.SelectByID2("10-03-01-4-2@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("10-03-01-4.SLDPRT");
            }
            else
            {

                swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);

                if (typeOfMf == "2")
                {
                    swPartDoc.Extension.SelectByID2(
                        "D1@Эскиз28@" + Path.GetFileNameWithoutExtension(newPrt0202) + ".SLDPRT", "DIMENSION", 0, 0, 0,
                        false, 0, null, 0);

                    ((Dimension) (swPartDoc.Parameter("D1@Эскиз28"))).SystemValue = -0.05;

                    swPartDoc.EditRebuild3();
                    swPartDoc.SaveAs2(newPrt0202, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);

                    _swApp.CloseDoc(newPrt0202);
                    NewComponents.Add(new FileInfo(newPrt0202));
                }
                _swApp.CloseDoc(newPartName);
                NewComponents.Add(new FileInfo(newPartPath));
            }

            #endregion

            #endregion

            _swApp.IActivateDoc2("10-4.SLDASM", false, 0);
            swDocMontageFrame = ((ModelDoc2) (_swApp.ActiveDoc));

            GabaritsForPaintingCamera(swDocMontageFrame);

            swDocMontageFrame.ForceRebuild3(true);

            swDocMontageFrame.SaveAs2(newMontageFramePath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            NewComponents.Add(new FileInfo(newMontageFramePath));

            #endregion

            _swApp.CloseDoc(new FileInfo(newMontageFramePath).Name);
            _swApp = null;
            CheckInOutPdm(NewComponents, true, Settings.Default.TestPdmBaseName);

            foreach (var newComponent in NewComponents)
            {
             //   ExportXmlSql.Export(newComponent.FullName);
            }

            if (onlyPath) return newMontageFramePath;

            MessageBox.Show(newMontageFramePath, "Модель построена");

            return newMontageFramePath;
        }

        private string MontageFrameSSubAsm(string widthS, string lenghtS, string thiknessS, string typeOfMf,
            string frameOffset, string material, IList<string> покрытие)
        {

            #region Проверка введенных значений и открытие сборки

            if (IsConvertToDouble(new[] {widthS, lenghtS, thiknessS, frameOffset}) == false)
            {
                return "";
            }

            if (!InitializeSw(true)) return "";

            var typeOfMfs = "-0" + typeOfMf;
            if (typeOfMf == "0")
            {
                typeOfMfs = "";
            }

            // Тип рымы
            var internalCrossbeam = false; // Погашение внутренней поперечной балки
            var internalLongitudinalBeam = false; // Погашение внутренней продольной балки
            var frameOffcetStr = "";
            switch (typeOfMf)
            {
                case "1":
                    internalCrossbeam = true;
                    break;
                case "2":
                    internalLongitudinalBeam = true;
                    break;
                case "3":
                    internalCrossbeam = true;
                    frameOffcetStr = "-" + frameOffset;
                    break;
            }


            var newMontageFrameName = $"10-{thiknessS}-{widthS}-{lenghtS}{typeOfMfs}{frameOffcetStr}.SLDASM";

            var newMontageFramePath =
                $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newMontageFrameName}";
            if (File.Exists(newMontageFramePath))
            {
                GetLastVersionPdm(new FileInfo(newMontageFramePath).FullName, Settings.Default.TestPdmBaseName);
                _swApp.OpenDoc6(newMontageFramePath, (int) swDocumentTypes_e.swDocASSEMBLY,
                    (int) swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                return newMontageFramePath;
            }
            var modelMontageFramePath = $@"{Settings.Default.SourceFolder}{BaseFrameFolder}\{"10-4"}.SLDASM";
            
            GetLatestVersionAsmPdm(modelMontageFramePath, Settings.Default.PdmBaseName);           


            if (!Warning()) return "";
            var swDocMontageFrame = _swApp.OpenDoc6(modelMontageFramePath, (int) swDocumentTypes_e.swDocASSEMBLY,
                (int) swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
            _swApp.Visible = true;
            var swAsm = (AssemblyDoc) swDocMontageFrame;
            swAsm.ResolveAllLightWeightComponents(false);

            #endregion

            #region Основные размеры, величины и переменные

            // Габариты Ширина меньше ширины установки на 20мм Длина по размеру блока
            var width = GetInt(widthS); // Поперечные балки
            var lenght = GetInt(lenghtS); // Продольная балка
            var offsetI = GetInt(Convert.ToString(Convert.ToDouble(frameOffset)*10)); // Смещение поперечной балки
            if (offsetI > (lenght - 125)*10)
            {
                offsetI = (lenght - 250)*10;
                MessageBox.Show("Смещение превышает допустимое значение! Программой установлено - " +
                                (offsetI/10));
            }

            #region  Металл и х-ки гибки

            // Коэффициенты и радиусы гибов  
            var sqlBaseData = new SqlBaseData();
            var bendParams = sqlBaseData.BendTable(thiknessS);
            var bendRadius = Convert.ToDouble(bendParams[0]);
            var kFactor = Convert.ToDouble(bendParams[1]);

            

            #endregion

            #endregion

            #region Изменение размеров элементов и компонентов сборки

            var thikness = Convert.ToDouble(thiknessS)/1000;
            bendRadius = bendRadius/1000;
            var w = Convert.ToDouble(width)/1000;
            var l = Convert.ToDouble(lenght)/1000;
            var offset = Convert.ToDouble(offsetI)/10000;
            var offsetMirror = Convert.ToDouble(lenght*10 - offsetI)/10000;

            #region 10-02-4 Зеркальная 10-01-4

            if (typeOfMf == "3")
            {
                swDocMontageFrame.Extension.SelectByID2("10-01-4-3@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm = ((AssemblyDoc) (swDocMontageFrame));
                swAsm.ReplaceComponents(
                    Settings.Default.DestinationFolder + "\\" + BaseFrameFolder + "\\10-02-4.SLDASM", "", false, true);
                swAsm.ResolveAllLightWeightComponents(false);
                // var newMirrorAsm = (ModelDocSw)_swApp.ActivateDoc("10-02-4.SLDASM"); var swMirrorAsm = ((AssemblyDoc)(newMirrorAsm)); swMirrorAsm.ResolveAllLightWeightComponents(false);

                //Продольная зеркальная балка (Длина установки)
                swDocMontageFrame.Extension.SelectByID2("D1@Эскиз1@10-02-4-1@10-4/10-02-01-4-1@10-02-4", "DIMENSION", 0,
                    0, 00, false, 0, null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("D1@Эскиз1@10-02-01-4.Part"))).SystemValue = l;
                    //  Длина установки  0.8;

                swDocMontageFrame.Extension.SelectByID2("D3@Эскиз25@10-02-4-1@10-4/10-02-01-4-1@10-02-4", "DIMENSION", 0,
                    0, 0, false, 0, null, 0);
                // boolstatus = swDocMontageFrame.Extension.SelectByID2("D3@Эскиз25@10-01-4-1@10-4/10-01-01-4-1@10-01-4", "DIMENSION", 0, 0, 00, false, 0, null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("D3@Эскиз25@10-02-01-4.Part"))).SystemValue = offsetMirror;
                    //Смещение поперечной балки от края;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-02-4-1@10-4/10-02-01-4-1@10-02-4",
                    "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-02-4-1@10-4/10-02-01-4-1@10-02-4",
                    "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("D1@Листовой металл@10-02-01-4.Part"))).SystemValue =
                    bendRadius; // Радиус гиба  0.005;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-02-4-1@10-4/10-02-01-4-1@10-02-4",
                    "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-02-4-1@10-4/10-02-01-4-1@10-02-4",
                    "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("D2@Листовой металл@10-01-01-4.Part"))).SystemValue = kFactor;
                    // K-Factor  0.55;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-02-4-1@10-4/10-02-01-4-1@10-02-4",
                    "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension) (swDocMontageFrame.Parameter("Толщина@Листовой металл@10-02-01-4.Part"))).SystemValue =
                    thikness; // Толщина Листового металла 0.006;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.ClearSelection2(true);
            }


            #endregion

            //swApp.SendMsgToUser(string.Format("Thikness= {0}, BendRadius= {1}, Ширина= {2}, Длина= {3}, ", Thikness * 1000, BendRadius * 1000, Ширина * 1000, Длина * 1000));

            //Продольные балки (Длина установки)

            #region 10-01-4

            swDocMontageFrame.Extension.SelectByID2("D1@Эскиз1@10-01-4-1@10-4/10-01-01-4-1@10-01-4", "DIMENSION", 0, 0,
                0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Эскиз1@10-01-01-4.Part"))).SystemValue = l;
                //  Длина установки  0.8;
            swDocMontageFrame.Extension.SelectByID2("D3@Эскиз25@10-01-4-1@10-4/10-01-01-4-1@10-01-4", "DIMENSION", 0, 0,
                0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D3@Эскиз25@10-01-01-4.Part"))).SystemValue = offset;
                //Смещение поперечной балки от края;
            swDocMontageFrame.EditRebuild3();
            //swApp.SendMsgToUser(Offset.ToString());
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-01-4-1@10-4/10-01-01-4-1@10-01-4", "BODYFEATURE",
                0, 0, 0, false, 0, null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-01-4-1@10-4/10-01-01-4-1@10-01-4",
                "DIMENSION", 0, 0, 0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Листовой металл@10-01-01-4.Part"))).SystemValue = bendRadius;
                // Радиус гиба  0.005;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-01-4-1@10-4/10-01-01-4-1@10-01-4", "BODYFEATURE",
                0, 0, 0, false, 0, null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-01-4-1@10-4/10-01-01-4-1@10-01-4",
                "DIMENSION", 0, 0, 0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D2@Листовой металл@10-01-01-4.Part"))).SystemValue = kFactor;
                // K-Factor  0.55;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-01-4-1@10-4/10-01-01-4-1@10-01-4",
                "DIMENSION", 0, 0, 0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("Толщина@Листовой металл@10-01-01-4.Part"))).SystemValue =
                thikness; // Толщина Листового металла 0.006;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.ClearSelection2(true);

            #endregion

            #region 10-04-4-2

            swDocMontageFrame.Extension.SelectByID2("D1@Эскиз1@10-04-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Эскиз1@10-04-4.Part"))).SystemValue = (l - 0.14);
                // Длина установки - 140  0.66;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-04-4-2@10-4", "BODYFEATURE", 0, 0, 0, false, 0,
                null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-04-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0,
                null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Листовой металл@10-04-4.Part"))).SystemValue = bendRadius;
                // Радиус гиба  0.005;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-04-4-2@10-4", "BODYFEATURE", 0, 0, 0, false, 0,
                null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-04-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0,
                null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D2@Листовой металл@10-04-4.Part"))).SystemValue = kFactor;
                // K-Factor  0.55;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-04-4-2@10-4", "DIMENSION", 0, 0, 0,
                false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("Толщина@Листовой металл@10-04-4.Part"))).SystemValue = thikness;
                // Толщина Листового металла 0.006;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.ClearSelection2(true);

            #endregion

            //Поперечная балка (Ширина установки)

            #region 10-03-4

            swDocMontageFrame.Extension.SelectByID2("D2@Эскиз1@10-03-4-1@10-4/10-03-01-4-1@10-03-4", "DIMENSION", 0, 0,
                0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D2@Эскиз1@10-03-01-4.Part"))).SystemValue = (w - 0.12);
                //  Ширина установки - 20 - 100  0.88;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-03-4-1@10-4/10-03-01-4-1@10-03-4", "BODYFEATURE",
                0, 0, 0, false, 0, null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-03-4-1@10-4/10-03-01-4-1@10-03-4",
                "DIMENSION", 0, 0, 0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D1@Листовой металл@10-03-01-4.Part"))).SystemValue = bendRadius;
                // Радиус гиба  0.005;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-03-4-1@10-4/10-03-01-4-1@10-03-4", "BODYFEATURE",
                0, 0, 0, false, 0, null, 0);
            swDocMontageFrame.ActivateSelectedFeature();
            swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-03-4-1@10-4/10-03-01-4-1@10-03-4",
                "DIMENSION", 0, 0, 0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("D2@Листовой металл@10-03-01-4.Part"))).SystemValue = kFactor;
                // K-Factor  0.55;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-03-4-1@10-4/10-03-01-4-1@10-03-4",
                "DIMENSION", 0, 0, 0, false, 0, null, 0);
            ((Dimension) (swDocMontageFrame.Parameter("Толщина@Листовой металл@10-03-01-4.Part"))).SystemValue =
                thikness; // Толщина Листового металла 0.006;
            swDocMontageFrame.EditRebuild3();
            swDocMontageFrame.ClearSelection2(true);

            #endregion

            #endregion

            #region Удаление поперечной балки

            if (internalCrossbeam == false)
            {
                swDocMontageFrame.Extension.SelectByID2("10-03-01-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-39@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-40@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-23@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-19@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-22@10-4", "COMPONENT", 0, 0, 0, true, 0,
                    null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-41@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-42@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-24@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-20@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-23@10-4", "COMPONENT", 0, 0, 0, true, 0,
                    null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-43@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-44@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-25@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-21@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-24@10-4", "COMPONENT", 0, 0, 0, true, 0,
                    null, 0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-45@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-46@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-26@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-22@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-25@10-4", "COMPONENT", 0, 0, 0, true, 0,
                    null, 0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-25@10-4", "COMPONENT", 0, 0, 0, false, 0,
                    null, 0);
                swDocMontageFrame.EditDelete();

                // Удаление ненужных элементов продольной балки
                const int deleteOption =
                    (int) swDeleteSelectionOptions_e.swDelete_Absorbed +
                    (int) swDeleteSelectionOptions_e.swDelete_Children;
                swDocMontageFrame.Extension.SelectByID2("Вырез-Вытянуть8@10-01-4-3@10-4/10-01-01-4-1@10-01-4",
                    "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDocMontageFrame.Extension.DeleteSelection2(deleteOption);
            }

            #endregion

            #region Удаление продольной балки

            // Погашение внутренней продольной балки
            if (internalLongitudinalBeam == false)
            {
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-5@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-6@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-7@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-8@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-10@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-11@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-12@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-13@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-6@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-7@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-8@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-9@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-17@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-18@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-19@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-20@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-21@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-22@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-23@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-24@10-4", "COMPONENT", 0, 0, 0, true, 0, null,
                    0);
                swDocMontageFrame.Extension.SelectByID2("10-04-4-2@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDocMontageFrame.EditDelete();

                const int deleteOption =
                    (int) swDeleteSelectionOptions_e.swDelete_Absorbed +
                    (int) swDeleteSelectionOptions_e.swDelete_Children;
                // Удаление ненужных элементов поперечной балки
                swDocMontageFrame.Extension.SelectByID2("10-03-4-2@10-4/Регулируемая ножка-1@10-03-4", "COMPONENT", 0, 0,
                    0, false, 0, null, 0);
                swDocMontageFrame.EditDelete();
                swDocMontageFrame.Extension.SelectByID2("10-03-4-2@10-4/Hex Nut 5915_gost-3@10-03-4", "COMPONENT", 0, 0,
                    0, false, 0, null, 0);
                swDocMontageFrame.EditDelete();
                swDocMontageFrame.Extension.SelectByID2("10-03-4-2@10-4/Hex Nut 5915_gost-4@10-03-4", "COMPONENT", 0, 0,
                    0, false, 0, null, 0);
                swDocMontageFrame.EditDelete();
                swDocMontageFrame.Extension.SelectByID2("Вырез-Вытянуть5@10-03-4-2@10-4/10-03-01-4-1@10-03-4",
                    "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDocMontageFrame.Extension.DeleteSelection2(deleteOption);
                swDocMontageFrame.Extension.SelectByID2("Вырез-Вытянуть4@10-03-4-2@10-4/10-03-01-4-1@10-03-4",
                    "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDocMontageFrame.Extension.DeleteSelection2(deleteOption);
            }

            #endregion

            #region Сохранение элементов и сборки, а также применение материалов

            if (material == "")
            {
                material = "HK";

                if (thiknessS == "2")
                {
                    material = "OZ";
                }
            }

            #region Детали


            //Продольные балки (Длина установки)

            #region 10-01-01-4 - Деталь

            _swApp.IActivateDoc2("10-01-01-4", false, 0);
            IModelDoc2 swPartDoc = _swApp.IActiveDoc2;
            switch (typeOfMf)
            {
                case "2":
                case "0":
                    typeOfMfs = "";
                    break;
                case "3":
                case "1":
                    typeOfMfs = "-0" + typeOfMf;
                    break;
            }

            //var newPartName = String.Format("10-01-01-{0}-{1}{2}{3}-{4}{5}{6}{7}.SLDPRT",
            //    thiknessS,
            //    lenght,
            //    frameOffcetStr,
            //    typeOfMfs,
            //    material,
            //    string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
            //    string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
            //    string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]);

            var newPartName = $"10-01-01-{thiknessS}-{lenght}{frameOffcetStr}{typeOfMfs}.SLDPRT";

            var newPartPath = $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newPartName}";
            if (File.Exists(newPartPath))
            {
                //swApp.SendMsgToUser("Заменяем компонент");
                swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                swDocMontageFrame.Extension.SelectByID2("10-01-4-1@10-4/10-01-01-4-1@10-01-4", "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", false, true);
                _swApp.CloseDoc("10-01-01-4.SLDPRT");
            }
            else
            {
                swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

                try
                {
                    VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                _swApp.CloseDoc(newPartName);
                NewComponents.Add(new FileInfo(newPartPath));
            }

            #endregion

            //

            #region 10-02-01-4 - Деталь Зеркальная 10-01-01-4

            if (typeOfMf == "3")
            {
                _swApp.IActivateDoc2("10-02-01-4", false, 0);
                swPartDoc = _swApp.IActiveDoc2;
                switch (typeOfMf)
                {
                    case "2":
                    case "0":
                        typeOfMfs = "";
                        break;
                    case "3":
                    case "1":
                        typeOfMfs = "-0" + typeOfMf;
                        break;
                }


                newPartName = $"10-02-01-{thiknessS}-{lenght}{frameOffcetStr}{typeOfMfs}.SLDPRT";

                newPartPath = $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                    swDocMontageFrame.Extension.SelectByID2("10-02-4-3@10-4/10-02-01-4-1@10-02-4", "COMPONENT", 0, 0, 0,
                        false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("10-02-01-4.SLDPRT");
                }
                else
                {

                    swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    try
                    {
                        VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);
                    }
                    catch (Exception)
                    {
                        //
                    }
                    _swApp.CloseDoc(newPartName);
                    NewComponents.Add(new FileInfo(newPartPath));
                }
            }

            #endregion

            #region 10-04-4 - Деталь

            if (internalLongitudinalBeam)
            {
                _swApp.IActivateDoc2("10-04-4", false, 0);
                swPartDoc = ((IModelDoc2) (_swApp.ActiveDoc));

                //newPartName = String.Format("10-04-{0}-{1}{2}{3}-{4}{5}.SLDPRT",
                //thiknessS,
                //lenght - 140,
                //material,
                //string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
                //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
                //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]);

                newPartName = $"10-04-{thiknessS}-{(lenght - 140)}.SLDPRT";

                newPartPath = $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                    swDocMontageFrame.Extension.SelectByID2("10-04-4-2@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("10-04-4.SLDPRT");
                }
                else
                {
                    swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    try
                    {
                        VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);
                    }
                    catch (Exception)
                    {
                        //
                    }
                    _swApp.CloseDoc(newPartName);
                    NewComponents.Add(new FileInfo(newPartName));
                }
            }
            else
            {
                _swApp.CloseDoc("10-04-4.SLDPRT");
            }

            #endregion

            //Поперечная балка (Ширина установки)

            #region 10-03-01-4 - Деталь

            _swApp.IActivateDoc2("10-03-01-4", false, 0);
            swPartDoc = ((IModelDoc2) (_swApp.ActiveDoc));
            switch (typeOfMf)
            {
                case "3":
                case "2":
                    typeOfMfs = "-0" + typeOfMf;
                    break;
                case "1":
                case "0":
                    typeOfMfs = "";
                    break;
            }
            //newPartName = String.Format("10-04-{0}-{1}{2}{3}-{4}{5}.SLDPRT",
            //    thiknessS,
            //    width - 120,
            //    typeOfMfs,
            //    string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
            //    string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
            //    string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]);

            newPartName = $"10-03-01-{thiknessS}-{(width - 120)}{typeOfMfs}.SLDPRT";

            newPartPath = $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newPartName}";
            var newPrt0202 = $"10-02-01-{thiknessS}-{(width - 120)}{typeOfMfs}.SLDPRT";
            newPrt0202 = $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newPrt0202}";
            var newPartPath0301 = newPartPath;
            var newPrt0302 = $"10-03-01-{thiknessS}-{(width - 120)}{typeOfMfs}";

            if (File.Exists(newPartPath))
            {
                swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                swDocMontageFrame.Extension.SelectByID2("10-03-01-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("10-03-01-4.SLDPRT");
            }
            else
            {
                swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                try
                {
                    VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);
                }
                catch (Exception)
                {
                    //
                }
                if (typeOfMf == "2")
                {
                    swPartDoc.Extension.SelectByID2(
                        "D1@Эскиз28@" + Path.GetFileNameWithoutExtension(newPrt0202) + ".SLDPRT", "DIMENSION", 0, 0, 0,
                        false, 0, null, 0);

                    ((Dimension) (swPartDoc.Parameter("D1@Эскиз28"))).SystemValue = -0.05;

                    swPartDoc.EditRebuild3();
                    swPartDoc.SaveAs2(newPrt0202, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, true, true);
                    try
                    {
                        VentsMatdll(new[] {material}, new[] {покрытие[3], покрытие[1], покрытие[2]}, newPartPath);
                    }
                    catch (Exception)
                    {
                        // 
                    }
                    _swApp.CloseDoc(newPrt0202);
                    NewComponents.Add(new FileInfo(newPrt0202));
                }
                _swApp.CloseDoc(newPartName);
                NewComponents.Add(new FileInfo(newPartPath));
            }

            #endregion

            #endregion

            //Сборки

            //Продольные балки (Длина установки)

            #region 10-01-4 - Сборка

            _swApp.IActivateDoc2("10-01-4", false, 0);
            swPartDoc = _swApp.ActiveDoc;
            typeOfMfs = "-0" + typeOfMf;
            if (typeOfMf == "0" || typeOfMf == "2" || typeOfMf == "3")
            {
                typeOfMfs = "";
            }

            //newPartName = String.Format("10-01-4-{0}-{1}{2}{3}{4}{5}{6}.SLDASM",
            //    thiknessS,
            //    lenght,
            //    frameOffcetStr,
            //    typeOfMfs,
            //    string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
            //    string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
            //    string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]);

            newPartName = $"10-01-4-{thiknessS}-{lenght}{frameOffcetStr}{typeOfMfs}.SLDASM";

            newPartPath = $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newPartName}";
            if (File.Exists(newPartPath))
            {
                swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                swDocMontageFrame.Extension.SelectByID2("10-01-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("10-01-4.SLDASM");
            }
            else
            {
                swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

                _swApp.CloseDoc(newPartName);
                NewComponents.Add(new FileInfo(newPartPath));
            }

            #endregion


            #region 10-02-4 - Сборка Зеркальная 10-01-4

            if (typeOfMf == "3")
            {
                _swApp.IActivateDoc2("10-02-4", false, 0);
                swPartDoc = _swApp.ActiveDoc;
                typeOfMfs = "-0" + typeOfMf;
                if (typeOfMf == "0" || typeOfMf == "2" || typeOfMf == "3")
                {
                    typeOfMfs = "";
                }

                //newPartName = String.Format("10-02-4-{0}-{1}{2}{3}{4}{5}{6}.SLDASM",
                //thiknessS,
                //lenght,
                //frameOffcetStr,
                //typeOfMfs,
                //string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
                //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
                //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]);

                newPartName = $"10-02-4-{thiknessS}-{lenght}{frameOffcetStr}{typeOfMfs}.SLDASM";

                newPartPath = $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                    swDocMontageFrame.Extension.SelectByID2("10-02-4-3@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("10-02-4.SLDASM");
                }
                else
                {
                    swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    _swApp.CloseDoc(newPartName);
                    NewComponents.Add(new FileInfo(newPartPath));
                }
            }

            #endregion


            ////Поперечная балка (Ширина установки)

            #region 10-03-4 - Сборка

            _swApp.IActivateDoc2("10-03-4", false, 0);
            swPartDoc = _swApp.ActiveDoc;
            switch (typeOfMf)
            {
                case "3":
                case "2":
                    typeOfMfs = "-0" + typeOfMf;
                    break;
                case "1":
                case "0":
                    typeOfMfs = "";
                    break;
            }


            //newPartName = String.Format("10-03-{0}-{1}{2}{3}{4}{5}.SLDASM",
            //    thiknessS,
            //    (width - 120),
            //    typeOfMfs,
            //    string.IsNullOrEmpty(покрытие[3]) ? null : "-" + покрытие[0],
            //    string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
            //    string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]);


            newPartName = $"10-03-{thiknessS}-{(width - 120)}{typeOfMfs}.SLDASM";

            var newAsm0202 = $"10-02-{thiknessS}-{(width - 120)}{typeOfMfs}.SLDASM";
            newAsm0202 = $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newAsm0202}";
            newPartPath = $@"{Settings.Default.DestinationFolder}\{BaseFrameDestinationFolder}\{newPartName}";
            var newAsm0302 = $"10-03-{thiknessS}-{(width - 120)}{typeOfMfs}";
            if (File.Exists(newPartPath))
            {
                swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                swDocMontageFrame.Extension.SelectByID2("10-03-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swAsm.ReplaceComponents(newPartPath, "", true, true);
                _swApp.CloseDoc("10-03-4.SLDASM");
            }
            else
            {
                swPartDoc.SaveAs2(newPartPath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                if (typeOfMf == "2")
                {
                    swPartDoc.SaveAs2(newAsm0202, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, true, true);
                    NewComponents.Add(new FileInfo(newAsm0202));
                }
                _swApp.CloseDoc(newPartName);
                NewComponents.Add(new FileInfo(newPartPath));
            }

            #endregion

            try
            {
                var balka = (ModelDoc2) _swApp.IActivateDoc2("10-4.SLDASM", false, 0);
                balka.Extension.SelectByID2("10-03-01-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                var swAssembly = ((AssemblyDoc) (balka));
                swAssembly.ReplaceComponents(newPartPath0301, "", true, true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            if (typeOfMf == "2")
            {
                try
                {
                    swDocMontageFrame = ((ModelDoc2) (_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                    swDocMontageFrame.Extension.SelectByID2(newAsm0302 + "-2@10-4", "COMPONENT", 0, 0, 0, false, 0, null,
                        0);
                    swAsm.ReplaceComponents(newAsm0202, "", false, true);
                    _swApp.CloseDoc(newAsm0302 + ".SLDASM");

                    var newAsm0202Model =
                        ((ModelDoc2)
                            (_swApp.ActivateDoc2(Path.GetFileNameWithoutExtension(newAsm0202) + ".SLDASM", true, 0)));
                        // _swApp.OpenDoc6(newAsm0202, (int)swDocumentTypes_e.swDocASSEMBLY, (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                    newAsm0202Model.Extension.SelectByID2(
                        Path.GetFileNameWithoutExtension(newPrt0302) + "-1@" +
                        Path.GetFileNameWithoutExtension(newAsm0202), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    var newAsm0202ModelAssy = (AssemblyDoc) newAsm0202Model;
                    newAsm0202ModelAssy.ReplaceComponents(newPrt0202, "", false, true);
                    newAsm0202Model.Save();
                    _swApp.CloseDoc(Path.GetFileNameWithoutExtension(newAsm0202) + ".SLDASM");

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            _swApp.IActivateDoc2("10-4.SLDASM", false, 0);
            swDocMontageFrame = ((ModelDoc2) (_swApp.ActiveDoc));
            swDocMontageFrame.ForceRebuild3(true);

            swDocMontageFrame.SaveAs2(newMontageFramePath, (int) swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            NewComponents.Add(new FileInfo(newMontageFramePath));

            #endregion

            _swApp.CloseDoc(new FileInfo(newMontageFramePath).Name);
            //_swApp.ExitApp();
            _swApp = null;
            CheckInOutPdm(NewComponents, true, Settings.Default.TestPdmBaseName);

            return newMontageFramePath;
        }

        #endregion

        #region Registration

        /// <summary>
        /// Получение последней версии файла в базе PDM.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="vaultName">Имя базы PDM.</param>
        /// 
        internal static void GetLastVersionAsmPdm(string path, string vaultName)
        {
            try
            {
                Логгер.Информация($"Получение последней версии по пути {path}\nБаза - {vaultName}", "", null, "GetLastVersionPdm");

                VaultSystem.GetLastVersionOfFile(path);
            }
            catch (Exception e)
            {
                Логгер.Ошибка(
                    $"Во время получения последней версии по пути {path} возникла ошибка!\nБаза - {vaultName}. {e.Message}", null,
                    e.StackTrace, "GetLastVersionPdm");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="vaultName"></param>
        public static void GetLatestVersion(string path, string vaultName)
        {
            new ModelSw().GetLatestVersionAsmPdm( path, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="vaultName"></param>
        public static void GetAsBuild(string path, string vaultName)
        {
            new ModelSw().GetFilesAsBuild(path, vaultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vaultName"></param>
        /// <param name="list"></param>
        /// <param name="pdmFilesAfterGet"></param>
        /// <param name="getList"></param>
        public static void BatchGet(string vaultName, List<VaultSystem.BatchParams> list, out List<VaultSystem.PdmFilesAfterGet> pdmFilesAfterGet)
        {
            pdmFilesAfterGet = null;
            //Логгер.Информация($"Получение последней версии по пути {}\nБаза - {vaultName}", "", null, "GetLastVersionPdm");
            VaultSystem.BatchGet(vaultName, list, out pdmFilesAfterGet);           
        }

        internal void GetFilesAsBuild(string path, string vaultName)
        {
            try
            {
                Логгер.Информация($"Получение последней версии по пути {path}\nБаза - {vaultName}", "", null, "GetLastVersionPdm");
                VaultSystem.GetAsmFilesAsBuild(path, vaultName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                Логгер.Ошибка(
                    $"Во время получения последней версии по пути {path} возникла ошибка!\nБаза - {vaultName}. {e.Message}", null,
                    e.StackTrace, "GetLastVersionPdm");
            }
        }

        internal void GetLatestVersionAsmPdm(string path, string vaultName)
        {
            try
            {
                Логгер.Информация($"Получение последней версии по пути {path}\nБаза - {vaultName}", "", null, "GetLastVersionPdm");
                VaultSystem.GetLastVersionOfFile(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                Логгер.Ошибка(
                    $"Во время получения последней версии по пути {path} возникла ошибка!\nБаза - {vaultName}. {e.Message}", null,
                    e.StackTrace, "GetLastVersionPdm");
            }
        }

        // TODO Asm REf
        internal static void GetLastVersionPdm(string[] path, string vaultName)
        {
            //if (Settings.Default.Developer)
            //{
            //    return;
            //}
            for (var i = 0; i < path.Length; i++)
            {
                try
                {
                    Логгер.Информация(
                    $"Получение последней версии по пути {path[i]}\nБаза - {vaultName}", "", null,
                        "GetLastVersionPdm");
                    VaultSystem.GetLastVersionOfFile(path[i]);
                }
                catch (Exception e)
                {
                    Логгер.Ошибка(
                        $"Во время получения последней версии по пути {path[i]} возникла ошибка!\nБаза - {vaultName}. {e.Message}", null,
                        e.StackTrace, "GetLastVersionPdm");
                }
            }
        }

        /// <summary>
        /// Gets the last version PDM.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="vaultName">The PDM base.</param>
        public void GetLastVersionPdm(string path, string vaultName)
        {
            if (Settings.Default.Developer)
            {
                return;
            }
            try
            {
                Логгер.Информация(
                $"Получение последней версии по пути {path}\nБаза - {vaultName}", "", null, "GetLastVersionPdm");
                VaultSystem.GetLastVersionOfFile(path);
            }
            catch (Exception e)
            {
                Логгер.Ошибка(
                    $"Во время получения последней версии по пути {path} возникла ошибка\nБаза - {vaultName}. {e.Message}", null,
                    e.StackTrace, "GetLastVersionPdm");
            }
        }

        /// <summary>
        /// Регистрация (разрегистрация) в хранилище списка файлов
        /// </summary>
        /// <param name="filesList">The files list.</param>
        /// <param name="registration">if set to <c>true</c> [registration].</param>
        /// <param name="vaultName">The PDM base.</param>
        internal void CheckInOutPdm(List<FileInfo> filesList, bool registration, string vaultName)
        {
            try
            {
                VaultSystem.CheckInOutPdm(filesList, registration, vaultName);
            }
            catch (Exception ex)
            {
                Логгер.Ошибка(
                    $"Во время регистрации документа по пути {ex.StackTrace} возникла ошибка\nБаза - {vaultName}. {ex.Message}", null,
                    "", "CheckInOutPdm");
            }
        }

        /// <summary>
        /// Registrations the PDM.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="registration">if set to <c>true</c> [registration].</param>
        /// <param name="vaultName">The PDM base.</param>
        public void CheckInOutPdm(string filePath, bool registration, string vaultName)
        {
            try
            {
                VaultSystem.CheckInOutPdm(filePath, registration, vaultName);
            }
            catch (Exception ex)
            {
                Логгер.Ошибка( $"Во время регистрации документа по пути {ex.StackTrace} возникла ошибка\nБаза - {vaultName}. {ex.Message}", null, "", "CheckInOutPdm");
            }
        }

        #endregion

        #region AdditionalMethods

        private static void DelEquations(int index, IModelDoc2 swModel)
        {
            try
            {
                Логгер.Информация($"Удаление уравнения #{index} в модели {swModel.GetPathName()}", "", null, "DelEquations");
                var myEqu = swModel.GetEquationMgr();
                myEqu.Delete(index);
                swModel.EditRebuild3();
                //myEqu.Add2(index, "\"" + System.Convert.ToChar(index) + "\"=" + index, false);
            }
            catch (Exception e)
            {
                Логгер.Ошибка($"Удаление уравнения #{index} в модели {swModel.GetPathName()}. {e.Message}", e.StackTrace, null, "DelEquations");
            }
        }

        /// <summary>
        /// Создает директорию для сохранения компонентов новой сборки.
        /// </summary>
        /// <param name="path">Путь к папке</param>
        /// <param name="vaultName"></param>
        public void CreateDistDirectory(string path, string vaultName)
        {
            try
            {
                Логгер.Информация($"Создание папки по пути {path} для сохранения", "", null, "CreateDistDirectory");
                VaultSystem.CreateDistDirectory(path, vaultName);
            }
            catch (Exception e)
            {
                Логгер.Ошибка($"Не удалось создать папку по пути {path}. Ошибка {e.Message}", e.StackTrace, null,
                    "CreateDistDirectory");
            }
        }

        private static int GetInt(string param)
        {
            return Convert.ToInt32(param);
        }

        private static bool IsConvertToInt(IEnumerable<string> newStringParams)
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

        private static bool IsConvertToDouble(IEnumerable<string> newStringParams)
        {
            foreach (var value in newStringParams)
            {
                try
                {                    
                    Convert.ToDouble(value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + " Повторите ввод данных!");
                    return false;
                }
            }
            return true;
        }

        private void SwPartParamsChangeWithNewName(string partName, string newName, string[,] newParams,
            bool newFuncOfAdding, IReadOnlyList<string> copies)
        {
            try
            {
                Логгер.Отладка($"Начало изменения детали {partName}", "", "", "SwPartParamsChangeWithNewName");
                var swDoc = _swApp.OpenDoc6(partName + ".SLDPRT", (int) swDocumentTypes_e.swDocPART,
                    (int) swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                var modName = swDoc.GetPathName();
                for (var i = 0; i < newParams.Length/2; i++)
                {
                    try
                    {
                        var myDimension = ((Dimension) (swDoc.Parameter(newParams[i, 0] + "@" + partName + ".SLDPRT")));
                        var param = Convert.ToDouble(newParams[i, 1]);
                        var swParametr = param;
                        myDimension.SystemValue = swParametr/1000;
                        swDoc.EditRebuild3();
                    }
                    catch (Exception e)
                    {
                        Логгер.Отладка(
                            string.Format(
                                "Во время изменения детали {4} произошла ошибка при изменении параметров {0}={1}. {2} {3}",
                                newParams[i, 0], newParams[i, 1], e.TargetSite, e.Message,
                                Path.GetFileNameWithoutExtension(modName)),
                            "", "", "SwPartParamsChangeWithNewName");
                    }
                }
                if (newName == "")
                {
                    return;
                }

                GabaritsForPaintingCamera(swDoc);

                swDoc.EditRebuild3();
                swDoc.ForceRebuild3(false);

                if (!newFuncOfAdding)
                {
                    NewComponents.Add(new FileInfo(newName + ".SLDPRT"));
                }

                if (newFuncOfAdding)
                {
                    // todo проверить
                    //if (!string.IsNullOrEmpty(copies)) return;
                    NewComponentsFull.Add(new VaultSystem.VentsCadFile
                    {
                        LocalPartFileInfo = new FileInfo(newName + ".SLDPRT").FullName,
                        PartIdSql = Convert.ToInt32(newName.Substring(newName.LastIndexOf('-') + 1))
                    });
                }


                swDoc.SaveAs2(new FileInfo(newName + ".SLDPRT").FullName, (int) swSaveAsVersion_e.swSaveAsCurrentVersion,
                    false, true);

                if (copies != null)
                {
                    //MessageBox.Show("copies - " + copies + "  addingInName - " + addingInName);
                    swDoc.SaveAs2(new FileInfo(copies[0] + ".SLDPRT").FullName,
                        (int) swSaveAsVersion_e.swSaveAsCurrentVersion, true, true);
                    swDoc.SaveAs2(new FileInfo(copies[1] + ".SLDPRT").FullName,
                        (int) swSaveAsVersion_e.swSaveAsCurrentVersion, true, true);
                }


                _swApp.CloseDoc(newName + ".SLDPRT");
                Логгер.Отладка($"Деталь {partName} изменена и сохранена по пути {new FileInfo(newName).FullName}", "",
                    "", "SwPartParamsChangeWithNewName");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Determines whether [is sheet metal part] [the specified part path].
        /// </summary>
        /// <param name="partPath">The part path.</param>
        /// <param name="pdmBase">The PDM base.</param>
        /// <returns></returns>
        public bool IsSheetMetalPart(string partPath, string pdmBase)
        {
            InitializeSw(true);
            IModelDoc2 swDoc = null;

            try
            {
                GetLastVersionPdm(partPath, pdmBase);
                swDoc = _swApp.OpenDoc6(partPath, (int) swDocumentTypes_e.swDocPART,
                    (int) swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                return IsSheetMetalPart((IPartDoc) swDoc);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (swDoc != null) _swApp.CloseDoc(swDoc.GetTitle());
                _swApp.ExitApp();
                _swApp = null;
            }
        }

        private static bool IsSheetMetalPart(IPartDoc swPart)
        {
            var isSheetMetal = false;
            try
            {
                var vBodies = swPart.GetBodies2((int) swBodyType_e.swSolidBody, false);

                foreach (Body2 vBody in vBodies)
                {
                    isSheetMetal = vBody.IsSheetMetal();
                }
                return isSheetMetal;
            }
            catch (Exception)
            {
                //  
            }
            return isSheetMetal;
        }

        internal bool Warning()
        {
            return MessageBox.Show("Сохраните все открытые документы!",
                //" Перед началом работы рекомендуется закрыть все открытые документы в SolidWorks.", 
                "", //"Сохраните все открытые документы!",
                MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.None) !=
                   MessageBoxResult.Cancel;
        }

        internal bool InitializeSw(bool visible)
        {
            try
            {
                _swApp = (SldWorks) Marshal.GetActiveObject("SldWorks.Application");
            }
            catch (Exception)
            {
                _swApp = new SldWorks {Visible = visible};
            }
            return _swApp != null;
        }

        #region ReubildAllDocs

        /// <summary>
        /// Reubilds all docs.
        /// </summary>
        public void ReubildAllOpenedDocs()
        {
            try
            {
                _swApp = (SldWorks) Marshal.GetActiveObject("SldWorks.Application");
                if (_swApp == null) return;
                var docs = _swApp.GetDocuments();
                foreach (ModelDoc2 doc in docs)
                {
                    doc.EditRebuild3();
                    doc.ForceRebuild3(false);
                    doc.Save();
                    _swApp.CloseDoc(doc.GetTitle());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        #endregion

        #endregion

        #region  MATERIAL EDIT

        #region AddMaterialtoXML

        // работает по имени материала или по коду материала
        private static string AddMaterialtoXml(string material)
        {
            switch (material)
            {
                case "Az":
                    material = "AZ";
                    break;
                case "Oz":
                    material = "OZ";
                    break;
                case "Hk":
                    material = "HK";
                    break;
            }

            //var ERP = "";
            var densValue = ""; //Density
            //var SWProperty = "";
            //var description = "";
            //var CodeMaterial = "";
            var xhatchName = "5"; //xhatch
            var xhatchAngle = ""; //angle
            var xhatchScale = ""; //scale
            //var pwshader2 = "";
            var path = "";
            var rgb = "";
            var materialName = "";

            var sqlBaseData = new SqlBaseData();
            var materialDataTable = sqlBaseData.MaterialsTable(material);
            foreach (DataRow dataRow in materialDataTable.Rows)
            {
                if (dataRow["MaterialsName"].ToString() == material)
                {
                    //ERP = dataRow["ERP"].ToString();
                    densValue = dataRow["Density"].ToString();
                    //SWProperty = dataRow["SWProperty"].ToString();
                    //description = dataRow["description"].ToString();
                    materialName = dataRow["MaterialsName"].ToString();
                    //CodeMaterial = dataRow["CodeMaterial"].ToString();
                    xhatchName = dataRow["xhatch"].ToString();
                    xhatchAngle = dataRow["angle"].ToString();
                    xhatchScale = dataRow["scale"].ToString();
                    //pwshader2 = dataRow["pwshader2"].ToString();
                    path = dataRow["path"].ToString();
                    rgb = dataRow["RGB"].ToString();
                    //MessageBox.Show(CodeMaterial);
                }
                else if (dataRow["CodeMaterial"].ToString() == material)
                {
                    //ERP = dataRow["ERP"].ToString();
                    densValue = dataRow["Density"].ToString();
                    //SWProperty = dataRow["SWProperty"].ToString();
                    //description = dataRow["description"].ToString();
                    materialName = dataRow["MaterialsName"].ToString();
                    //CodeMaterial = dataRow["CodeMaterial"].ToString();
                    xhatchName = dataRow["xhatch"].ToString();
                    xhatchAngle = dataRow["angle"].ToString();
                    xhatchScale = dataRow["scale"].ToString();
                    //pwshader2 = dataRow["pwshader2"].ToString();
                    path = dataRow["path"].ToString();
                    rgb = dataRow["RGB"].ToString();
                    //MessageBox.Show(materialName);
                }
                else if (dataRow["LevelID"].ToString() == material)
                {
                    //ERP = dataRow["ERP"].ToString();
                    densValue = dataRow["Density"].ToString();
                    //SWProperty = dataRow["SWProperty"].ToString();
                    //description = dataRow["description"].ToString();
                    materialName = dataRow["MaterialsName"].ToString();
                    //CodeMaterial = dataRow["CodeMaterial"].ToString();
                    xhatchName = dataRow["xhatch"].ToString();
                    xhatchAngle = dataRow["angle"].ToString();
                    xhatchScale = dataRow["scale"].ToString();
                    //pwshader2 = dataRow["pwshader2"].ToString();
                    path = dataRow["path"].ToString();
                    rgb = dataRow["RGB"].ToString();
                    //MessageBox.Show(materialName);
                }
            }

            //System.IO.MemoryStream myMemoryStream;
            var myXml = new XmlTextWriter(@"C:\Program Files\SW-Complex\materialsXML.sldmat", Encoding.UTF8);

            //создаем XML
            myXml.WriteStartDocument();

            // устанавливаем параметры форматирования xml-документа
            myXml.Formatting = Formatting.Indented;

            // длина отступа
            myXml.Indentation = 2;

            // создаем элементы
            myXml.WriteStartElement("mstns:materials");
            myXml.WriteAttributeString("xmlns:mstns", "http://www.solidworks.com/sldmaterials");
            myXml.WriteAttributeString("xmlns:msdata", "urn:schemas-microsoft-com:xml-msdata");
            myXml.WriteAttributeString("xmlns:sldcolorswatch", "http://www.solidworks.com/sldcolorswatch");
            myXml.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            myXml.WriteAttributeString("version", "2008.03");

            //
            myXml.WriteStartElement("curves");
            myXml.WriteAttributeString("id", "curve0");
            //
            myXml.WriteStartElement("point");
            myXml.WriteAttributeString("x", "1.000000");
            myXml.WriteAttributeString("y", "1.000000");
            myXml.WriteEndElement();
            //
            myXml.WriteStartElement("point");
            myXml.WriteAttributeString("x", "2.000000");
            myXml.WriteAttributeString("y", "1.000000");
            myXml.WriteEndElement();
            //
            myXml.WriteStartElement("point");
            myXml.WriteAttributeString("x", "3.000000");
            myXml.WriteAttributeString("y", "1.000000");
            myXml.WriteEndElement();
            myXml.WriteEndElement();

            //
            myXml.WriteStartElement("classification");
            myXml.WriteAttributeString("name", "Металл");

            // Material name
            myXml.WriteStartElement("material");
            myXml.WriteAttributeString("name", materialName); //MaterialName.Text
            myXml.WriteAttributeString("matid", "480");
            myXml.WriteAttributeString("description", "");
            myXml.WriteAttributeString("propertysource", "Алюмоцинк AZ150 0,7");
            myXml.WriteAttributeString("appdata", "");

            // xhatch name - штриховка
            myXml.WriteStartElement("xhatch");
            //myXml.WriteAttributeString("name", "ANSI32 (Сталь)")
            myXml.WriteAttributeString("name", xhatchName); //CboHatch.Text
            //'myXml.WriteAttributeString("angle", "0.0");
            myXml.WriteAttributeString("angle", xhatchAngle); //TextBox1.Text
            //'myXml.WriteAttributeString("scale", "1.0");
            myXml.WriteAttributeString("scale", xhatchScale); //TxtDens.Text
            myXml.WriteEndElement(); // '\ xhatch name

            // shaders
            myXml.WriteStartElement("shaders");
            // pwshader2
            myXml.WriteStartElement("pwshader2");
            myXml.WriteAttributeString("name", "кремовый сильно-глянцевый пластик");
            //myXml.WriteAttributeString("path", "\plastic\high gloss\" & TreeView1.SelectedNode.Text & ".p2m")
            myXml.WriteAttributeString("path", path); //@"\plastic\high gloss\cream high gloss plastic.p2m"
            myXml.WriteEndElement(); // pwshader2
            myXml.WriteEndElement(); // shaders

            // swatchcolor
            myXml.WriteStartElement("swatchcolor");
            myXml.WriteAttributeString("RGB", rgb); // "D7D0C0"
            myXml.WriteStartElement("sldcolorswatch:Optical");
            myXml.WriteAttributeString("Ambient", "1.000000");
            myXml.WriteAttributeString("Transparency", "0.000000");
            myXml.WriteAttributeString("Diffuse", "1.000000");
            myXml.WriteAttributeString("Specularity", "1.000000");
            myXml.WriteAttributeString("Shininess", "0.310000");
            myXml.WriteAttributeString("Emission", "0.000000");
            myXml.WriteEndElement(); // sldcolorswatch:Optical
            myXml.WriteEndElement(); // swatchcolor

            // physicalproperties
            myXml.WriteStartElement("physicalproperties");
            //
            myXml.WriteStartElement("EX");
            myXml.WriteAttributeString("displayname", "Модуль упругости");
            myXml.WriteAttributeString("value", "2E+011");
            myXml.WriteAttributeString("usepropertycurve", "0");
            myXml.WriteEndElement();
            //
            myXml.WriteStartElement("NUXY");
            myXml.WriteAttributeString("displayname", "Коэффициент Пуассона");
            myXml.WriteAttributeString("value", "0.29");
            myXml.WriteAttributeString("usepropertycurve", "0");
            myXml.WriteEndElement();
            //
            myXml.WriteStartElement("DENS");
            myXml.WriteAttributeString("displayname", "Массовая плотность");
            myXml.WriteAttributeString("value", densValue); //Plotnost.Text
            myXml.WriteAttributeString("usepropertycurve", "0");
            myXml.WriteEndElement();
            //
            myXml.WriteStartElement("SIGXT");
            myXml.WriteAttributeString("displayname", "Предел прочности при растяжении");
            myXml.WriteAttributeString("value", "356901000");
            myXml.WriteAttributeString("usepropertycurve", "0");
            myXml.WriteEndElement();
            //
            myXml.WriteStartElement("SIGYLD");
            myXml.WriteAttributeString("displayname", "Предел текучести");
            myXml.WriteAttributeString("value", "203943000");
            myXml.WriteAttributeString("usepropertycurve", "0");
            myXml.WriteEndElement();
            myXml.WriteEndElement(); // physicalproperties

            // sustainability
            myXml.WriteStartElement("sustainability");
            myXml.WriteAttributeString("linkId", "");
            myXml.WriteAttributeString("dbName", "");
            myXml.WriteEndElement();
            // custom
            myXml.WriteStartElement("custom");
            myXml.WriteEndElement();

            myXml.WriteEndElement(); // Material name

            myXml.WriteEndElement(); // classification name
            myXml.WriteEndElement(); // mstns:materials

            // заносим данные в myMemoryStream 
            myXml.Flush();
            myXml.Close();

            return materialName;
        }

        #endregion

        #region MATERIAL ACCEPT

        /// <summary>
        /// Задает материал для детали из сборки (если задан componentId) либо по имени детали
        /// </summary>
        /// <param name="materialName">Материал (код или наименование по базе)</param>
        /// <param name="swDoc">Деталь</param>
        /// <param name="componentId">The component identifier.</param>
        private static void SetMeterial(string materialName, ModelDoc2 swDoc, string componentId)
        {
            if (componentId != "")
            {
                try
                {
                    //"ВНС-901.43.230-1@ВНС-901.43.200/ВНС-901.43.231-1@ВНС-901.43.230"
                    swDoc.Extension.SelectByID2(componentId, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    var comp = swDoc.ISelectionManager.GetSelectedObject3(1);
                    var matChangingModel = comp.GetModelDoc();
                    const string configName = "";
                    const string databaseName = "materialsXML.sldmat";
                    matChangingModel.SetMaterialPropertyName2(configName, databaseName, AddMaterialtoXml(materialName));
                    Логгер.Информация(string.Format("Для компонента {1} применен материал {0} ", AddMaterialtoXml(materialName), componentId), null, "", "SetMeterial");
                }
                catch (Exception ex)
                {
                    Логгер.Ошибка($"Не удалось применить материал {AddMaterialtoXml(materialName)} для компонента {componentId}. {ex.Message}",
                        ex.StackTrace, null, "SetMeterial");
                }
            }

            if (componentId == "")
            {
                try
                {
                    var swPart = ((PartDoc) (swDoc));
                    const string configName = "";
                    const string databaseName = "materialsXML.sldmat";
                    swPart.SetMaterialPropertyName2(configName, databaseName, AddMaterialtoXml(materialName));

                    Логгер.Информация(
                        string.Format("Для детали {1} применен материал {0} ", AddMaterialtoXml(materialName),
                            swDoc.GetPathName()), null, "", "SetMeterial");
                }
                catch (Exception ex)
                {
                    Логгер.Ошибка($"Не удалось применить материал {AddMaterialtoXml(materialName)} для детали {componentId}. {ex.Message}", ex.StackTrace, null, "SetMeterial");
                }
            }

            File.Delete(@"C:\Program Files\SW-Complex\materialsXML.sldmat");
        }

        #endregion

        private static void GabaritsForPaintingCamera(IModelDoc2 swmodel)
        {
            try
            {
                const long valueset = 1000;
                const int swDocPart = 1;
                const int swDocAssembly = 2;

                for (var i = 0; i < swmodel.GetConfigurationCount(); i++)
                {
                    i = i + 1;
                    var configname = swmodel.IGetConfigurationNames(ref i);

                    // MessageBox.Show(configname, swmodel.GetConfigurationCount().ToString());

                    Configuration swConf = swmodel.GetConfigurationByName(configname);
                    if (swConf.IsDerived()) continue;
                    //swmodel.ShowConfiguration2(configname);
                    swmodel.EditRebuild3();

                    switch (swmodel.GetType())
                    {
                        case swDocPart:
                        {
                            //    MessageBox.Show("swDocPart");
                            var part = (PartDoc) swmodel;
                            var box = part.GetPartBox(true);

                            swmodel.AddCustomInfo3(configname, "Длина", 30, "");
                            swmodel.AddCustomInfo3(configname, "Ширина", 30, "");
                            swmodel.AddCustomInfo3(configname, "Высота", 30, "");

                            // swmodel.AddCustomInfo3(configname, "Длина", , "");

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
                            //   MessageBox.Show("AssemblyDoc");

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
            catch (Exception ex)
            {
                MessageBox.Show($"{swmodel.GetTitle()}\n{ex.Message}\n{ex.StackTrace}", "GabaritsForPaintingCamera");
            }
        }

        #endregion         

        #region DISPOSE CLASS

        public ModelSw(IntPtr handle)
        {
            this._handle = handle;
        }

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
        public ModelSw()
        {
            // do not re-create dispose clean-up code here.
            // calling dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

    }

}

