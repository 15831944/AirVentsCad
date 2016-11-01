using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using AirVentsCadWpf.Properties;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using VentsCadLibrary;
using System.Linq;

namespace AirVentsCadWpf.AirVentsClasses.UnitsBuilding
{
    public partial class ModelSw
    {

        #region Fields

        /// <summary>
        ///  Папка с исходной моделью "Вибровставки". 
        /// </summary>
        public string SpigotFolder = @"\Библиотека проектирования\DriveWorks\12 - Spigot";
        /// <summary>
        ///  Папка для сохранения компонентов "Вибровставки". 
        /// </summary>
        public string SpigotDestinationFolder = @"\Проекты\Blauberg\12 - Вибровставка";

        /// <summary>
        /// Папка с исходной моделью "Регулятора расхода воздуха". 
        /// </summary>
        public string DamperFolder = @"\Библиотека проектирования\DriveWorks\11 - Damper";
        /// <summary>
        /// Папка для сохранения компонентов "Регулятора расхода воздуха". 
        /// </summary>
        public string DamperDestinationFolder = @"\Проекты\Blauberg\11 - Регулятор расхода воздуха";

        /// <summary>
        /// Папка с исходной моделью "Крыши".
        /// </summary>
        public string RoofFolder = @"\Библиотека проектирования\DriveWorks\15 - Roof";
        /// <summary>
        /// Папка для сохранения компонентов "Крыши". 
        /// </summary>
        public string RoofDestinationFolder = @"\Проекты\Blauberg\15 - Крыша";

        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        public void Lono()
        {
            InitializeSw(true);
            var swDoc = _swApp.IActiveDoc2;
            var table = (DesignTable)swDoc.GetDesignTable();
            if (table == null) return;

            table.Updatable = true;
            var names = (string[])swDoc.GetConfigurationNames();

            var list = new List<KeyValuePair<string, string>>();
            foreach (var name in names)
            {
                swDoc.GetConfigurationByName(name);
                swDoc.ShowConfiguration2(name);
                swDoc.EditRebuild3();
                var swModelDocExt = swDoc.Extension;
                var swCustPropForDescription = swModelDocExt.CustomPropertyManager[name];
                string valOut;

                string massForConfig;
                swCustPropForDescription.Get4("Масса_Таблица", true, out valOut, out massForConfig);
                if (string.IsNullOrEmpty(massForConfig)) continue;
                list.Add(new KeyValuePair<string, string>(name, massForConfig));
            }

            //"$СВОЙСТВО@Масса_Таблица" MessageBox.Show($"GetTotalColumnCount() - {table.GetTotalColumnCount()}\nGetVisibleColumnCount() - {table.GetVisibleColumnCount()}\nGetEntryText(5,8) - {table.GetEntryText(5,8)}\n");

            table.EditTable2(false);

            //var j = table.GetTotalColumnCount() + 10;
            //table.SetEntryValue(0, j, true, "Конфигурация");
            //table.SetEntryValue(0, j + 1, true, "Масса");
            
            var i = 1;
            foreach (var value in list)
            {
                table.SetEntryValue(i, 1, true, value.Key);
                table.SetEntryValue(i, 2, false, value.Value);
                i++;
            }

            table.EditTable2(false);
            MessageBox.Show($"Масса по конфигурациям добавлены в ячейки таблицы.");
            swDoc.EditRebuild3();
        }

        #region Dumper

        /// <summary>
        /// Метод по генерации "Регулятора расхода воздуха"
        /// </summary>
        /// <param name="type">тип регулятора заслонки</param>
        /// <param name="width">ширина заслонки</param>
        /// <param name="height">высота заслонки</param>
        /// <param name="isOutDoor"></param>
        /// <param name="material"></param>
        public void Dumper(string type, string width, string height, bool isOutDoor, string[] material)
        {
            var path = DumperS(type, width, height, isOutDoor, material);          
        }

        /// <summary>
        /// Dumpers the s.
        /// </summary>
        /// <param name="typeOfFlange">The typeOfFlange.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="isOutDoor"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public string DumperS(string typeOfFlange, string width, string height, bool isOutDoor, string[] material)
        {
            if (!IsConvertToInt(new[] { width, height })){return "";}

            string modelName = null;
            string modelDamperPath = null; 
            string nameAsm = null;

            var replaced = VersionsFileInfo.Replaced.Instance;
            replaced.Clear();

            var repList = replaced.List;

            DateTime? lastChangedDate = null; 

            switch (typeOfFlange)
            {
                case "20":
                    modelName = "11-20";
                    modelDamperPath = DamperFolder;
                    nameAsm = "11 - Damper";
                    lastChangedDate = new DateTime(2016, 5, 20);
                    break;
                case "30":
                    modelName = "11-30";
                    modelDamperPath = DamperFolder;
                    nameAsm = "11-30";                    
                    break;             
            }

            if (string.IsNullOrEmpty(modelName)) return null;             

            var modelType = $"{(material[3] == "AZ" ? "" : "-" + material[3])}{(material[3] == "AZ" ? "" : material[1])}";
            
            var drawing = "11-20";
            if (modelName == "11-30")
            { drawing = modelName; }
            var newDamperName = modelName + "-" + width + "-" + height + modelType + (isOutDoor ? "-O" : "");
            //MessageBox.Show(newDamperName); return null;
            var newDamperPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newDamperName}.SLDDRW";
            var newDamperAsmPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newDamperName}.SLDASM";
            

            if (OpenIfExist(newDamperPath, VaultSystem.VentsCadFile.Type.Drawing, lastChangedDate)) return null;
            VersionsFileInfo.Replaced.ExistLatestVersion(newDamperAsmPath, VaultSystem.VentsCadFile.Type.Assembly, lastChangedDate, Settings.Default.PdmBaseName);

            if (replaced.List.Count > 0)
            {
                VaultSystem.CheckInOutPdm(new List<FileInfo>{ new FileInfo(newDamperPath),}, false);
            }            

            var modelDamperDrw = $@"{Settings.Default.SourceFolder}{modelDamperPath}\{drawing}.SLDDRW";
            var modelLamel = $@"{Settings.Default.SourceFolder}{modelDamperPath}\{"11-100"}.SLDDRW";

            GetLastVersionPdm(new[] { new FileInfo(modelDamperDrw).FullName, new FileInfo(modelLamel).FullName }, Settings.Default.PdmBaseName);

            if (!InitializeSw(true)) return null;
            if (!Warning()) return null;
            var swDocDrw = _swApp.OpenDoc6(@modelDamperDrw, (int)swDocumentTypes_e.swDocDRAWING,
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);

            ModelDoc2 swDoc = _swApp.ActivateDoc2(nameAsm, false, 0);
            var swAsm = (AssemblyDoc)swDoc;
            swAsm.ResolveAllLightWeightComponents(false);

            _swApp.Visible = true;

            // Габариты
            var widthD = Convert.ToDouble(width);
            var heightD = Convert.ToDouble(height);
            // Количество лопастей
            var countL = (Math.Truncate(heightD / 100)) * 1000;
            
            // Шаг заклепок
            const double step = 140;
            var rivetW = (Math.Truncate(widthD / step) + 1) * 1000;
            var rivetH = (Math.Truncate(heightD / step) + 1) * 1000;
            
            // Высота уголков
            var hC = Math.Truncate(7 + 5.02 + (heightD - countL / 10 - 10.04) / 2);

            // Коэффициенты и радиусы гибов   
            var thiknessStr = material?[1].Replace(".", ",") ?? "0,8";
            //MessageBox.Show(thiknessStr);

            #region typeOfFlange = "20"

            if (typeOfFlange == "20")
            {
                if (Convert.ToInt32(countL / 1000) % 2 == 1) //нечетное
                {
                    swDoc.Extension.SelectByID2("Совпадение5918344", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918345", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("Совпадение5918355", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918353", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditUnsuppress2();
                }

                string newName;
                string newPartPath;

                if (isOutDoor)
                {
                    swDoc.Extension.SelectByID2("Эскиз1", "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();

                    swDoc.Extension.SelectByID2("Вырез-Вытянуть10@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз34@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("ВНС-901.41.302-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Rivet Bralo-130@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-131@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-129@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-128@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-127@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-126@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                    // 11-005 
                    newName = "11-05-" + height + modelType;
                    newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (VersionsFileInfo.Replaced.ExistLatestVersion(newPartPath, VaultSystem.VentsCadFile.Type.Part, lastChangedDate, Settings.Default.PdmBaseName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-005.SLDPRT");
                    }
                    else
                    {

                        SwPartParamsChangeWithNewName("11-005",
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D3@Эскиз1", Convert.ToString(heightD)},
                                {"D1@Кривая1", Convert.ToString(rivetH)},
                                {"D3@Эскиз37", (Convert.ToInt32(countL / 1000) % 2 == 1) ? "0" : "50"},
                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);
                        AddMaterial(material, newName);
                        NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                    }

                    // 11-006 
                    newName = "11-06-" + height + modelType;
                    newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (VersionsFileInfo.Replaced.ExistLatestVersion(newPartPath, VaultSystem.VentsCadFile.Type.Part, lastChangedDate, Settings.Default.PdmBaseName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-006.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-006",
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D3@Эскиз1", Convert.ToString(heightD)},
                                {"D1@Кривая1", Convert.ToString(rivetH)},
                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);

                        AddMaterial(material, newName);
                        NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                    }
                }
                else
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть12@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз35@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз36@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть10@11-003-6@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@11-003-6@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз34@11-003-6@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз35@11-003-6@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    
                    
                    swDoc.Extension.SelectByID2("Rivet Bralo-187@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-188@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-189@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-190@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-191@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-192@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-193@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-194@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-195@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-196@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-197@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-198@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }


                // 11-001 
                newName = "11-01-" + height + modelType + (isOutDoor ? "-O" : "");
                                
                newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (VersionsFileInfo.Replaced.ExistLatestVersion(newPartPath, VaultSystem.VentsCadFile.Type.Part, lastChangedDate, Settings.Default.PdmBaseName))                
                {                
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-001-7@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-001.SLDPRT");
                }
                else
                {                 
                    SwPartParamsChangeWithNewName("11-001",
                        $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 7.94)}, {"D1@Эскиз27", Convert.ToString(countL/10 - 100)},
                            {"D2@Эскиз27", Convert.ToString(100*Math.Truncate(countL/2000))}, {"D1@Кривая1", Convert.ToString(countL)},
                            {"D1@Кривая2", Convert.ToString(rivetH)},  {"Толщина@Листовой металл", thiknessStr}
                        }, false, null);                 
                    AddMaterial(material, newName);                    
                    NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                    _swApp.CloseDoc(newName + ".SLDPRT");
                }

                #region OutDoor

                if (isOutDoor)
                {
                    try
                    {
                        swDoc
                           = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-003-6@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-003.SLDPRT");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"{newName}\n{e.Message}\n{e.StackTrace}", "11-003");
                    }
                }

                #endregion

                // 11-002
                newName = "11-03-" + width + modelType;
                newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";

                if (VersionsFileInfo.Replaced.ExistLatestVersion(newPartPath, VaultSystem.VentsCadFile.Type.Part, lastChangedDate, Settings.Default.PdmBaseName))                    
                {                    
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-002-4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-002.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-002",
                        $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(widthD - 0.96)},
                            {"D1@Кривая1", Convert.ToString(rivetW)},

                            {"D1@Кривая3", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл1", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);
                    NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }
                                             
                // todo sdgvr СПИСКИ оформить для регистрации                
                if (!isOutDoor)
                {
                    // 11-003 
                    newName = "11-02-" + height + modelType + (isOutDoor ? "-O" : "");
                    newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";

                    if (VersionsFileInfo.Replaced.ExistLatestVersion(newPartPath, VaultSystem.VentsCadFile.Type.Part, lastChangedDate, Settings.Default.PdmBaseName))                        
                    {                        
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-003-6@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-003.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-003",
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D2@Эскиз1", Convert.ToString(heightD + 7.94)},
                                {"D1@Эскиз27", Convert.ToString(countL/10 - 100)},
                                {"D1@Кривая1", Convert.ToString(countL)},
                                {"D1@Кривая2", Convert.ToString(rivetH)},
                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);

                        AddMaterial(material, newName);
                        NewComponentsFull.Add(new VaultSystem.VentsCadFile
                        {
                            LocalPartFileInfo = new FileInfo(newPartPath).FullName
                        });
                    }
                }

                // 11-004
                newName = "11-04-" + width + "-" + hC + modelType;
                newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";

                if (VersionsFileInfo.Replaced.ExistLatestVersion(newPartPath, VaultSystem.VentsCadFile.Type.Part, lastChangedDate, Settings.Default.PdmBaseName))
                {                    
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-004-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-004.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-004",
                        $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(widthD - 24)},
                            {"D7@Ребро-кромка1", Convert.ToString(hC)},
                            {"D1@Кривая1", Convert.ToString(rivetW)},

                            {"D1@Эскиз8", Convert.ToString(18.5)},
                            {"D1@Эскиз9", Convert.ToString(18.5)},

                            {"D1@Кривая5", Convert.ToString(rivetH)},
                            {"Толщина@Листовой металл1", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);
                    NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                //11-100 Сборка лопасти
                var newNameAsm = "11-" + width;
                var newPartPathAsm =
                    $@"{Settings.Default.DestinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-100-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                    _swApp.CloseDoc("11-100.SLDASM");
                }
                else
                {
                    #region  11-101  Профиль лопасти

                    newName = "11-" + (Math.Truncate(widthD - 23)) + "-01" + modelType;
                    newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                    {
                        _swApp.IActivateDoc2("10-100", false, 0);
                        swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                        swDoc.Extension.SelectByID2("11-101-1@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("11-100.SLDASM");
                    }
                    else
                    {
                        _swApp.IActivateDoc2("10-100", false, 0);
                        swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                        swDoc.Extension.SelectByID2("D1@Вытянуть1@11-101-1@11-100", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        var myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                        myDimension.SystemValue = (widthD - 23) / 1000;
                        _swApp.ActivateDoc2("11-101", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

                        // ToDo Delete
                        _swApp.CloseDoc(newName + ".sldasm");

                        NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.sldasm").FullName });
                    }

                    #endregion
                    
                    _swApp.ActivateDoc2("11-100", false, 0);
                    swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                    swDoc.ForceRebuild3(false);
                                        
                    var docDrw100 = _swApp.OpenDoc6(modelLamel, (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                    swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

                    try
                    {
                        //_swApp.CloseDoc(newNameAsm);                    
                        _swApp.ActivateDoc2(docDrw100?.GetTitle(), false, 0);
                        docDrw100?.ForceRebuild3(true);

                        docDrw100.SaveAs2(
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName) + " - DRW1");

                        NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                        NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo($@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName });
                        
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"{newName}\n{e.Message}\n{e.StackTrace}", "11-101  Профиль лопасти");                        
                    }
                }
            }

            #endregion

            #region typeOfFlange = "30"

            if (typeOfFlange == "30")
            {
                string newName;
                string newPartPath;

                if (isOutDoor)
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть7@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз27@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("ВНС-902.49.283-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Rivet Bralo-314@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-323@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-322@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-321@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-320@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Rivet Bralo-315@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-316@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-317@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-318@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-319@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    // 11-005 
                    newName = "11-05-" + height + modelType;
                    newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-005.SLDPRT");
                    }
                    else 
                    {
                        SwPartParamsChangeWithNewName("11-005",
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D3@Эскиз1", Convert.ToString(heightD)},
                                {"D1@Кривая1", Convert.ToString(rivetH)},
                                
                                {"D1@Кривая1", Convert.ToString(rivetH)},
                                {"D3@Эскиз37", (Convert.ToInt32(countL / 1000) % 2 == 1) ? "0" : "50"},
                                
                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);
                        AddMaterial(material, newName);

                        NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                    }

                    // 11-006 
                    newName = "11-06-" + height + modelType;
                    newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-006.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-006",
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D3@Эскиз1", Convert.ToString(heightD)},
                                {"D1@Кривая1", Convert.ToString(rivetH)},
                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);
                        AddMaterial(material, newName);
                        NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                    }
                }
                else
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть8@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть9@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз28@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз29@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                    swDoc.Extension.SelectByID2("Вырез-Вытянуть7@11-30-004-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть8@11-30-004-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз27@11-30-004-2@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз28@11-30-004-2@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                    swDoc.Extension.SelectByID2("Rivet Bralo-346@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-347@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-348@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-349@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-350@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-351@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-356@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-357@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-358@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-359@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-360@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-361@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }


                #region Кратность лопастей

                if (Convert.ToInt32(countL / 1000) % 2 == 0) //четное
                {
                    swDoc.Extension.SelectByID2("Совпадение5918344", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918345", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("Совпадение5918355", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918353", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditUnsuppress2();
                }

                #endregion

                var lp = widthD - 50; // Размер профиля 640,5 при ширине двойного 1400 = 
                var lp2 = lp - 11.6; // Длина линии под заклепки профиля
                var lProfName = width; //
                var lProfNameLength = (widthD - 23) / 1000;

                #region IsDouble

                var isdouble = widthD > 1000;

                if (!isdouble)
                {
                    swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                    swDoc.Extension.SelectByID2("11-30-100-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.Extension.SelectByID2("11-30-100-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-100-13@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-30-003-1@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-30-003-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ВНС-47.91.001-1@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ЗеркальныйКомпонент1", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("DerivedCrvPattern3", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-267@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-268@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-270@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-271@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-272@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-273@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-274@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-275@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-276@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-264@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-265@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-266@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-243@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-244@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-245@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-246@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-247@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-248@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-240@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-241@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-242@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-249@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-250@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-251@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);swDoc.EditDelete();
                }

                if (isdouble)
                {
                    lp = widthD / 2 - 59.5;
                    lp2 = lp - 11.6;
                    lProfName = Convert.ToString(Math.Truncate(Convert.ToDouble(Convert.ToDouble(width)) / 2 - 9)); 
                    lProfNameLength = (widthD / 2 - 23) / 1000;
                }

                #endregion

                #region Детали

                // 11-30-001 
                newName = "11-30-03-" + width + modelType;
                newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-001-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-30-001.SLDPRT");
                }
                else
                {
                    if (!isdouble)
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("11-30-001.SLDPRT", true, 0)));
                        swDoc.Extension.SelectByID2("Эскиз17", "SKETCH", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditSketch();
                        swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("D22@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D21@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D20@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D19@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D18@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D17@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D4@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D3@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D2@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D1@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditDelete();
                        swDoc.SketchManager.InsertSketch(true);
                    }

                    SwPartParamsChangeWithNewName("11-30-001",
                        $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(widthD/2 - 0.8)},
                            {"D3@Эскиз18", Convert.ToString(lp2)}, 
                            {"D1@Кривая1", Convert.ToString((Math.Truncate(lp2/step) + 1)*1000)},
                            {"Толщина@Листовой металл", thiknessStr}
                        },
                        false,
                        null);
                    AddMaterial(material, newName);

                    NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                // 11-30-002 
                newName = "11-30-01-" + height + modelType + (isOutDoor ? "-O" : "");
                newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-002-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-30-002.SLDPRT");
                }
                else 
                {
                    SwPartParamsChangeWithNewName("11-30-002",
                        $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                            {"D3@Эскиз23", Convert.ToString(countL/10 - 100)},
                            {"D2@Эскиз23", Convert.ToString(100*Math.Truncate(countL/2000))},

                            {"D1@Кривая2", Convert.ToString(countL)},
                            {"D1@Кривая3", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);

                    NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                // 11-30-004 
                newName = "11-30-02-" + height + modelType + (isOutDoor ? "-O" : "");
                newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-004-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-30-004.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-30-004",
                        $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                            {"D3@Эскиз23", Convert.ToString(countL/10 - 100)},
                            {"D2@Эскиз23", Convert.ToString(100*Math.Truncate(countL/2000))},
                            {"D1@Кривая2", Convert.ToString(countL)},

                            {"D1@Кривая5", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);

                    NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }
                
                // 11-30-003 
                newName = "11-30-04-" + Math.Truncate(lp) + "-" + hC + modelType;
                newPartPath = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-003-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-30-003.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-30-003",
                        $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(lp)},
                            {"D7@Ребро-кромка1", Convert.ToString(hC)},
                            {"D1@Кривая1", Convert.ToString((Math.Truncate(lp2/step) + 1)*1000)},
                            {"Толщина@Листовой металл1", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);

                    NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                #endregion

                #region Сборки

                #region 11-100 Сборка лопасти

                var newNameAsm = "11-2-" + lProfName;
                string newPartPathAsm =
                    $@"{Settings.Default.DestinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";

                if (isdouble)
                {
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-100-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                        _swApp.CloseDoc("11-100.SLDASM");
                    }
                    else
                    {
                        #region  11-101  Профиль лопасти

                        newName = "11-" + (Math.Truncate(lProfNameLength * 1000)) + "-01";
                        newPartPath =
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-101-1@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-100.SLDASM");
                        }
                        else
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("D1@Вытянуть1@11-101-1@11-100", "DIMENSION", 0, 0, 0, false, 0,
                                null, 0);
                            var myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                            myDimension.SystemValue = lProfNameLength;
                            _swApp.ActivateDoc2("11-101", false, 0);
                            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                            swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            _swApp.CloseDoc(newName);
                            NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                        }

                        #endregion
                    }
                }

                if (!isdouble)
                {
                    newNameAsm = "11-" + lProfName;
                    newPartPathAsm =
                        $@"{Settings.Default.DestinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-100-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                        _swApp.CloseDoc("11-100.SLDASM");
                    }
                    else 
                    {
                        #region  11-101  Профиль лопасти

                        newName = "11-" + (Math.Truncate(lProfNameLength * 1000)) + "-01";
                        newPartPath =
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1))
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-101-1@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-100.SLDASM");
                        }
                        else
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("D1@Вытянуть1@11-101-1@11-100", "DIMENSION", 0, 0, 0, false, 0,
                                null, 0);
                            var myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                            myDimension.SystemValue = lProfNameLength;
                            _swApp.ActivateDoc2("11-101", false, 0);
                            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                            swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            _swApp.CloseDoc(newName);
                            NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                        }

                        #endregion
                    }
                }

                _swApp.ActivateDoc2("11-100", false, 0);
                swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                if (isdouble)
                {
                    swDoc.Extension.SelectByID2("Совпадение76", "MATE", 0, 0, 0, false, 0, null, 0);swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("Совпадение77", "MATE", 0, 0, 0, false, 0, null, 0);swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("ВНС-47.91.101-2@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                }

                var docDrw100 = _swApp.OpenDoc6($@"{Settings.Default.SourceFolder}{modelDamperPath}\{"11-100"}.SLDDRW", (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                
                _swApp.ActivateDoc2(Path.GetFileNameWithoutExtension(newPartPathAsm), false, 0);
                swDoc.ForceRebuild3(false);
                swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(newNameAsm);
                docDrw100.ForceRebuild3(false);
                docDrw100.SaveAs2(
                    $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(
                    $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName) + " - DRW1");

                NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(newPartPathAsm).FullName });
                NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo($@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName });

                #endregion

                #region 11-30-100 Сборка Перемычки

                newNameAsm = "11-30-100-" + height + modelType;
                newPartPathAsm = $@"{Settings.Default.DestinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";

                if (isdouble)
                {
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-30-100-4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                        _swApp.CloseDoc("11-30-100.SLDASM");
                    }
                    else 
                    {
                        #region  11-30-101  Профиль перемычки

                        newName = "11-30-101-" + height + modelType;
                        newPartPath =
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 0))
                        {
                            _swApp.IActivateDoc2("10-30-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-30-101-2@11-30-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-30-100.SLDASM");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-30-101",
                                $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                                new[,]
                                {
                                    {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                                    {"D3@Эскиз19", Convert.ToString(countL/10 - 100)},
                                    {"D1@Кривая1", Convert.ToString(countL)},
                                    {"D1@Кривая2", Convert.ToString(rivetH)},
                                    {"Толщина@Листовой металл", thiknessStr}
                                },
                                false,
                                null);
                            AddMaterial(material, newName);

                            _swApp.CloseDoc(newName);
                            NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo($@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newNameAsm}").FullName });
                        }

                        #endregion

                        _swApp.ActivateDoc2("11-30-100", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.ForceRebuild3(true);

                        #region  11-30-102  Профиль перемычки

                        newName = "11-30-102-" + height + modelType;
                        newPartPath =
                            $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 0))
                        {
                            _swApp.IActivateDoc2("10-30-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-30-102-2@11-30-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-30-100.SLDASM");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-30-102",
                                $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newName}",
                                new[,]
                                {
                                    {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                                    {"D2@Эскиз19", Convert.ToString(countL/10 - 100)},
                                    {"D1@Кривая2", Convert.ToString(countL)},
                                    {"D1@Кривая1", Convert.ToString(rivetH)},
                                    {"Толщина@Листовой металл", thiknessStr}
                                },
                                false,
                                null);
                            try
                            {
                                VentsMatdll(material, null, newName);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show($"{newName}\n{e.Message}\n{e.StackTrace}", "11-30-102  Профиль перемычки");                                
                            }

                            _swApp.CloseDoc(newName);
                            NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo($@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newNameAsm}").FullName });
                        }

                        #endregion

                        _swApp.ActivateDoc2("11-30-100", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.ForceRebuild3(false);swDoc.ForceRebuild3(true);
                        swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        _swApp.CloseDoc(newNameAsm);
                        NewComponentsFull.Add(new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo($@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newNameAsm}").FullName });

                #endregion

                    }
                }

                #endregion
            }

            #endregion

            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm, true, 0)));

            GabaritsForPaintingCamera(swDoc);
            swDoc.EditRebuild3();
            swDoc.ForceRebuild3(true);
            var name = $@"{Settings.Default.DestinationFolder}\{DamperDestinationFolder}\{newDamperName}";
            swDoc.SaveAs2(name + ".SLDASM", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(name + ".SLDASM").FullName));
            swDocDrw.Extension.SelectByID2("DRW1", "SHEET", 0, 0, 0, false, 0, null, 0);
            var drw = (DrawingDoc)(_swApp.IActivateDoc3(drawing + ".SLDDRW", true, 0));
            drw.ActivateSheet("DRW1");
            var m = 5;
            if (Convert.ToInt32(width) > 500 || Convert.ToInt32(height) > 500) { m = 10; }
            if (Convert.ToInt32(width) > 850 || Convert.ToInt32(height) > 850) { m = 15; }
            if (Convert.ToInt32(width) > 1250 || Convert.ToInt32(height) > 1250) { m = 20; }

            //drw.SetupSheet5("DRW1", 12, 12, 1, m, true, Settings.Default.DestinationFolder + @"\\srvkb\SolidWorks Admin\Templates\Основные надписи\A2-A-1.slddrt", 0.42, 0.297, "По умолчанию", false);

            swDocDrw.SaveAs2(name + ".SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            _swApp.CloseDoc(newDamperPath);
            _swApp.ExitApp();

            NewComponentsFull.AddRange(new List<VaultSystem.VentsCadFile>
                        {
                            new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(name + ".SLDASM").FullName },
                            new VaultSystem.VentsCadFile { LocalPartFileInfo = new FileInfo(name + ".SLDDRW").FullName },
                         });

            #region To Delete

            //var messOfAllNewComps = "\" --NewComponentsFull--" + NewComponentsFull.Count.ToString();

            //foreach (var item in NewComponentsFull)
            //{
            //    messOfAllNewComps = messOfAllNewComps + "\n" + item.PartIdPdm + " - " + item.LocalPartFileInfo;
            //}

            //messOfAllNewComps = messOfAllNewComps + "\n --RepList--" + repList.Count.ToString(); ;

            //foreach (var item in repList)
            //{
            //    messOfAllNewComps = messOfAllNewComps + "\n" + item.PartIdPdm + " - " + item.FilePath;
            //}

            //MessageBox.Show(messOfAllNewComps);

            // Join on the ID properties.
            //var query = from c in customers
            //            join o in orders on c.ID equals o.ID
            //            select new { c.Name, o.Product };

            //var newList = 
            //    from fileToReplace in repList 
            //    join compToAdd in NewComponentsFull
            //    on fileToReplace.FilePath.ToLower() equals
            //    compToAdd.LocalPartFileInfo.ToLower()
            //              //from compToAdd in NewComponentsFull
            //              //join fileToReplace in repList
            //                  //on compToAdd.LocalPartFileInfo.ToLower() equals
            //                  //fileToReplace.FilePath.ToLower()
            //              select
            //              new
            //              {
            //                  compToAdd.LocalPartFileInfo,                              
            //                  fileToReplace.MessageForCheckOut,
            //              };


            //List<VaultSystem.VentsCadFile> listToCheckOut = new List<VaultSystem.VentsCadFile>();
            //foreach (var item in newList)
            //{
            //    listToCheckOut.Add(new VaultSystem.VentsCadFile
            //    {
            //        LocalPartFileInfo = item.LocalPartFileInfo,
            //        MessageForCheckOut = item.MessageForCheckOut,
            //    });
            //}

            //var asdfwe = "";

            //foreach (var item in listToCheckOut)
            //{
            //    asdfwe = asdfwe + $"\nPath - {item.LocalPartFileInfo} Message - {item.MessageForCheckOut}";
            //}

            //MessageBox.Show(asdfwe, $" newList {newList.Count()} listToCheckOut - {listToCheckOut.Count()}");

            #endregion

            string message = null;

            for (int i = 0; i < NewComponentsFull.Count; i++)
            {
                if (repList.Exists(x=>x.FilePath.ToLower() == NewComponentsFull[i].LocalPartFileInfo.ToLower()))
                {
                    
                    NewComponentsFull[i].MessageForCheckOut = repList.Single(x => x.FilePath.ToLower().Contains((NewComponentsFull[i].LocalPartFileInfo.ToLower()))).MessageForCheckOut;
                    if (!string.IsNullOrEmpty(NewComponentsFull[i].MessageForCheckOut))
                    {
                        if (string.IsNullOrEmpty(message))
                        {
                            message = NewComponentsFull[i].MessageForCheckOut;
                        }                        
                    }                    
                }
            }

            #region To Delete

            //var asdfwe = "";
            //foreach (var item in NewComponentsFull)
            //{                
            //    asdfwe = asdfwe + $"\nPath - {item.LocalPartFileInfo} Message - {item.MessageForCheckOut}";
            //}
            //MessageBox.Show(asdfwe, $" newList {NewComponentsFull.Count()} listToCheckOut - {NewComponentsFull.Count()}");

            #endregion

            List<VaultSystem.VentsCadFile> outList;
            VaultSystem.CheckInOutPdmNew(NewComponentsFull, true, out outList);

            if (!string.IsNullOrEmpty(message))
            {
                var list = "";
                foreach (var item in NewComponentsFull.Where(x => !string.IsNullOrEmpty(x.MessageForCheckOut)))
                {

                    list = list + $"\nPath - {Path.GetFileName(item.LocalPartFileInfo)}";
                }
                MessageBox.Show(list, message);// $" Следующие файлы были обновлены {newList.Count()} listToCheckOut - {listToCheckOut.Count()}");
            }           

            _swApp = null;
            return newDamperPath;
        }

        private void AddMaterial(IList<string> material, string newName)
        {
            try
            {
                VentsMatdll(material, null, newName);
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\n{material}\n{newName}{e.StackTrace}" , "Adding Material");
            }
        }

        #endregion

        #region Spigot

        /// <summary>
        /// Метод по генерации модели вибровставки
        /// </summary>
        /// <param name="type">тип вибровставки</param>
        /// <param name="width">ширина вибровставки</param>
        /// <param name="height">высота вибровставки</param>
        public void Spigot(string type, string width, string height)
        {
            var path = SpigotStr(type, width, height);

            #region
            //if (path == "")
            //{
            //    return;
            //}
            //if (MessageBox.Show(string.Format("Модель находится по пути:\n {0}\n Открыть модель?", new FileInfo(path).Directory),
            //      string.Format(" {0} ",
            //          Path.GetFileNameWithoutExtension(new FileInfo(path).FullName)), MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            //{
            //    SpigotStr(typeOfFlange, width, height);
            //}
            #endregion
        }

        /// <summary>
        /// Spigots the string.
        /// </summary>
        /// <param name="type">The typeOfFlange.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public string SpigotStr(string type, string width, string height)
        {
            if (IsConvertToInt(new[] { width, height }) == false)
            {
                return "";
            }
            
            string modelName;

            switch (type)
            {
                case "20":
                    modelName = "12-20";
                    break;
                case "30":
                    modelName = "12-30";
                    break;
                default:
                    modelName = "12-00";
                    break;
            }

            var newSpigotName = modelName + "-" + width + "-" + height;
            var newSpigotPath = $@"{Settings.Default.DestinationFolder}\{SpigotDestinationFolder}\{newSpigotName}";

            if (File.Exists(newSpigotPath + ".SLDDRW"))
            {
                MessageBox.Show(newSpigotPath + ".SLDDRW", "Данная модель уже находится в базе");
                return "";
            }

            var drawing = "12-00";
            if (modelName == "12-30")
            { drawing = modelName; }
            Dimension myDimension;
            var modelSpigotDrw = $@"{Settings.Default.SourceFolder}{SpigotFolder}\{drawing}.SLDDRW";

            GetLastVersionAsmPdm(modelSpigotDrw, Settings.Default.PdmBaseName);

            if (!InitializeSw(true)) return "";
            if (!Warning()) return "";
            var swDrwSpigot = _swApp.OpenDoc6(modelSpigotDrw, (int)swDocumentTypes_e.swDocDRAWING,
                (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
            _swApp.Visible = true;

            ModelDoc2 swDoc = _swApp.ActivateDoc2("12-00", false, 0);
            var swAsm = (AssemblyDoc)swDoc;
            swAsm.ResolveAllLightWeightComponents(false);

            switch (modelName)
            {
                case "12-20":
                    DelEquations(5, swDoc);
                    DelEquations(4, swDoc);
                    DelEquations(3, swDoc);
                    break;
                case "12-30":
                    DelEquations(0, swDoc);
                    DelEquations(0, swDoc);
                    DelEquations(0, swDoc);
                    break;
            }
            swDoc.ForceRebuild3(true);

            string newPartName;
            string newPartPath;
            IModelDoc2 swPartDoc;

            #region Удаление ненужного

            if (type == "20")
            {
                const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                         (int)swDeleteSelectionOptions_e.swDelete_Children;
                swDoc.Extension.SelectByID2("12-30-001-1@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-30-001-2@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-30-002-1@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-30-002-2@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.002-1@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.002-2@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.002-3@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.002-4@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-30-001-3@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-30-001-4@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-30-002-3@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-30-002-4@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.002-5@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.002-6@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.002-7@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.002-8@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-003-2@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Клей-2@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Клей-2@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);
            }
            if (type == "30")
            {
                const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                         (int)swDeleteSelectionOptions_e.swDelete_Children;
                swDoc.Extension.SelectByID2("12-20-001-1@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-20-001-2@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-20-002-1@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-20-002-2@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.001-1@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.001-2@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.001-3@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.001-4@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-20-001-3@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-20-001-4@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-20-002-3@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-20-002-4@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.001-5@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.001-6@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.001-7@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ВНС-96.61.001-8@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("12-003-1@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Клей-1@12-00", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);
            }

            swDoc.Extension.SelectByID2("30", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
            swDoc.EditDelete();
            swDoc.Extension.SelectByID2("20", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
            swDoc.EditDelete();

            #endregion

            #region Сохранение и изменение элементов

            var addDimH = 1;
            if (modelName == "12-30")
            {
                addDimH = 10;
            }

            var w = (Convert.ToDouble(width) -1)/ 1000;
            var h = Convert.ToDouble((Convert.ToDouble(height) + addDimH) / 1000);
            const double step = 50;
            var weldW = Convert.ToDouble((Math.Truncate(Convert.ToDouble(width) / step) + 1));
            var weldH = Convert.ToDouble((Math.Truncate(Convert.ToDouble(height) / step) + 1));

            if (modelName == "12-20")
            {
                //12-20-001
                _swApp.IActivateDoc2("12-20-001", false, 0);
                swPartDoc = _swApp.IActiveDoc2;
                newPartName = $"12-20-{height}.SLDPRT";
                newPartPath = $@"{Settings.Default.DestinationFolder}\{SpigotDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("12-20-001-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("12-20-001.SLDPRT");
                }
                else
                {
                    swDoc.Extension.SelectByID2("D1@Вытянуть1@12-20-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-20-001.Part")));
                    myDimension.SystemValue = h - 0.031;
                    swDoc.Extension.SelectByID2("D1@Кривая1@12-20-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-20-001.Part")));
                    myDimension.SystemValue = weldH;
                    swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    NewComponents.Add(new FileInfo(newPartPath));
                    _swApp.CloseDoc(newPartName);
                }

                //12-20-002
                _swApp.IActivateDoc2("12-20-002", false, 0);
                swPartDoc = _swApp.IActiveDoc2;
                newPartName = $"12-20-{width}.SLDPRT";
                newPartPath = $@"{Settings.Default.DestinationFolder}\{SpigotDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("12-20-002-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("12-20-002.SLDPRT");
                }
                else
                {
                    swDoc.Extension.SelectByID2("D1@Вытянуть1@12-20-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-20-002.Part")));
                    myDimension.SystemValue = w - 0.031;
                    swDoc.Extension.SelectByID2("D1@Кривая1@12-20-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-20-002.Part")));
                    myDimension.SystemValue = weldW;
                    swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    NewComponents.Add(new FileInfo(newPartPath));
                    _swApp.CloseDoc(newPartName);
                }

                //12-003
                _swApp.IActivateDoc2("12-003", false, 0);
                swPartDoc = _swApp.IActiveDoc2;
                newPartName = $"12-03-{width}-{height}.SLDPRT";
                newPartPath = $@"{Settings.Default.DestinationFolder}\{SpigotDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("12-003-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("12-003.SLDPRT");
                }
                else
                {
                    swDoc.Extension.SelectByID2("D3@Эскиз1@12-003-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D3@Эскиз1@12-003.Part")));
                    myDimension.SystemValue = w;
                    swDoc.Extension.SelectByID2("D2@Эскиз1@12-003-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D2@Эскиз1@12-003.Part")));
                    myDimension.SystemValue = h;
                    swDoc.EditRebuild3();
                    swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    NewComponents.Add(new FileInfo(newPartPath));
                    _swApp.CloseDoc(newPartName);
                }
            }

            if (modelName == "12-30")
            {
                //12-30-001
                _swApp.IActivateDoc2("12-30-001", false, 0);
                swPartDoc = _swApp.IActiveDoc2;
                newPartName = $"12-30-{height}.SLDPRT";
                newPartPath = $@"{Settings.Default.DestinationFolder}\{SpigotDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("12-30-001-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("12-30-001.SLDPRT");
                }
                else
                {
                    swDoc.Extension.SelectByID2("D1@Вытянуть1@12-30-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-30-001.Part")));
                    myDimension.SystemValue = h - 0.031;
                    swDoc.Extension.SelectByID2("D1@Кривая1@12-30-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-30-001.Part")));
                    myDimension.SystemValue = weldH;
                    swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    NewComponents.Add(new FileInfo(newPartPath));
                    _swApp.CloseDoc(newPartName);
                }

                //12-30-002
                _swApp.IActivateDoc2("12-30-002", false, 0);
                swPartDoc = _swApp.IActiveDoc2;
                newPartName = $"12-30-{width}.SLDPRT";
                newPartPath = $@"{Settings.Default.DestinationFolder}\{SpigotDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("12-30-002-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("12-30-002.SLDPRT");
                }
                else
                {
                    swDoc.Extension.SelectByID2("D1@Вытянуть1@12-30-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-30-002.Part")));
                    myDimension.SystemValue = w - 0.031;
                    swDoc.Extension.SelectByID2("D1@Кривая1@12-30-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-30-002.Part")));
                    myDimension.SystemValue = weldH;
                    swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    NewComponents.Add(new FileInfo(newPartPath));
                    _swApp.CloseDoc(newPartName);
                }

                //12-003
                _swApp.IActivateDoc2("12-003", false, 0);
                swPartDoc = _swApp.IActiveDoc2;
                newPartName = $"12-03-{width}-{height}.SLDPRT";
                newPartPath = $@"{Settings.Default.DestinationFolder}\{SpigotDestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("12-003-2@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("12-003.SLDPRT");
                }
                else
                {
                    swDoc.Extension.SelectByID2("D3@Эскиз1@12-003-2@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D3@Эскиз1@12-003.Part")));
                    myDimension.SystemValue = w;
                    swDoc.Extension.SelectByID2("D2@Эскиз1@12-003-2@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    myDimension = ((Dimension)(swDoc.Parameter("D2@Эскиз1@12-003.Part")));
                    myDimension.SystemValue = h;
                    swDoc.EditRebuild3();
                    swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    NewComponents.Add(new FileInfo(newPartPath));
                    _swApp.CloseDoc(newPartName);
                }
            }

            #endregion

            GabaritsForPaintingCamera(swDoc);
          

            swDoc.ForceRebuild3(true);
            swDoc.SaveAs2(newSpigotPath + ".SLDASM", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            _swApp.CloseDoc(newSpigotName + ".SLDASM");
            NewComponents.Add(new FileInfo(newSpigotPath + ".SLDASM"));
            swDrwSpigot.Extension.SelectByID2("DRW1", "SHEET", 0, 0, 0, false, 0, null, 0);
            var drw = (DrawingDoc)(_swApp.IActivateDoc3(drawing + ".SLDDRW", true, 0));
            drw.ActivateSheet("DRW1");
            var m = 5;
            if (Convert.ToInt32(width) > 500 || Convert.ToInt32(height) > 500)
            { m = 10; }
            if (Convert.ToInt32(width) > 850 || Convert.ToInt32(height) > 850)
            { m = 15; }
            if (Convert.ToInt32(width) > 1250 || Convert.ToInt32(height) > 1250)
            { m = 20; }
           drw.SetupSheet5("DRW1", 12, 12, 1, m, true, Settings.Default.DestinationFolder + @"\Vents-PDM\\Библиотека проектирования\\Templates\\Основные надписи\\A3-A-1.slddrt", 0.42, 0.297, "По умолчанию", false);
            //swDrwSpigot.SaveAs2(newSpigotPath + ".SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            var errors = 0;
            var warnings = 0;
            
            swDrwSpigot.SaveAs4(newSpigotPath + ".SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, (int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings);

            NewComponents.Add(new FileInfo(newSpigotPath + ".SLDDRW"));
            //MessageBox.Show(new FileInfo(newSpigotPath + ".SLDDRW").Name);
            _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(newSpigotPath + ".SLDDRW").FullName) + " - DRW1");
            _swApp.CloseDoc(newSpigotPath);
            _swApp.ExitApp();
            _swApp = null;
            //RegistrationPdm(NewComponents.OrderByDescending(x => x.Length).ToList(), true, Settings.Default.TestPdmBaseName);
            CheckInOutPdm(NewComponents, true, Settings.Default.TestPdmBaseName);


            foreach (var newComponent in NewComponents)
            {
                //ExportXmlSql.Export(newComponent.FullName);
                //PartInfoToXml(newComponent..FullName);
            }

            MessageBox.Show(newSpigotPath, "Модель построена");

            return newSpigotPath;
        }

        #endregion

        #region Roof

        /// <summary>
        /// Метод по генерации модели "Крыши"
        /// </summary>
        /// <param name="type">тип крыши</param>
        /// <param name="width">ширина крыши</param>
        /// <param name="lenght">высота крыши</param>
        public void Roof(string type, string width, string lenght)
        {
            var path = RoofStr(type, width, lenght, false);
        }

        string RoofStr(string type, string width, string lenght, bool onlyPath)
        {
            string modelName;
            switch (type)
            {
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                    modelName = "15-000";
                    break;
                default:
                    modelName = "15-000";
                    break;
            }

            try
            {
                _swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            }
            catch (Exception)
            {
                _swApp = new SldWorks { Visible = true };
            }
            if (_swApp == null) { return ""; }

            var newRoofName = "15-0" + type + "-" + width + "-" + lenght;
            var newRoofPath = $@"{Settings.Default.DestinationFolder}{RoofDestinationFolder}\{newRoofName}.SLDASM";

            if (File.Exists(newRoofPath))
            {
                if (onlyPath) return newRoofPath;
                MessageBox.Show(newRoofPath, "Данная модель уже находится в базе");
                return "";
            }
            
            GetLatestVersionAsmPdm($@"{Settings.Default.SourceFolder}{SpigotFolder}\{"15-000.SLDASM"}", Settings.Default.PdmBaseName);

            var modelRoofPath = $@"{Settings.Default.SourceFolder}{RoofFolder}\{modelName}.SLDASM";

            //if (!Warning()) return "";
            var swDoc = _swApp.OpenDoc6(modelRoofPath, (int)swDocumentTypes_e.swDocASSEMBLY,
                (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
            _swApp.Visible = true;

            var swAsm = (AssemblyDoc)swDoc;
            swAsm.ResolveAllLightWeightComponents(false);

            #region Удаление ненужного

            if (type == "1" || type == "5")
            {
                const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                        (int)swDeleteSelectionOptions_e.swDelete_Children;
                swDoc.Extension.SelectByID2("Вырез-Вытянуть3@15-001-1@15-000", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);
                swDoc.Extension.SelectByID2("Эскиз26@15-001-1@15-000", "SKETCH", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Hole-M8@15-001-1@15-000", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Вырез-Вытянуть4@15-001-1@15-000", "BODYFEATURE", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);

                swDoc.Extension.SelectByID2("Rivet Bralo-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Rivet Bralo-2@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Fasteners - M8", "FTRFOLDER", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Fasteners - M8", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Washer 11371_gost-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Washer 6402_gost-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Hex Bolt 7805_gost-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Washer 11371_gost-2@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Washer 6402_gost-2@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Hex Bolt 7805_gost-2@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Hex Bolt 7805_gost-2@15-000", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("ЗеркальныйКомпонент2", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }
            if (type == "2" || type == "6")
            {
                swDoc.Extension.SelectByID2("Rivet Bralo-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Rivet Bralo-2@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                swDoc.Extension.SelectByID2("ЗеркальныйКомпонент2", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                swDoc.Extension.SelectByID2("DerivedCrvPattern2", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                swDoc.Extension.SelectByID2("Симметричный1", "MATE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSuppress2(); swDoc.ClearSelection2(true);
                swDoc.Extension.SelectByID2("Расстояние1", "MATE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditUnsuppress2();

                swDoc.Extension.SelectByID2("Fasteners - M8", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Washer 11371_gost-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Washer 6402_gost-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Hex Bolt 7805_gost-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Washer 11371_gost-2@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Washer 6402_gost-2@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Hex Bolt 7805_gost-2@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Зеркальное отражение1@15-001-1@15-000", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Зеркальное отражение1@15-002-1@15-000", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }
            if (type == "3" || type == "4")
            {
                const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                        (int)swDeleteSelectionOptions_e.swDelete_Children;

                swDoc.Extension.SelectByID2("Винт самосверл 6-гр.гол с шайбой-33@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();

                swDoc.Extension.SelectByID2("Вырез-Вытянуть3@15-001-1@15-000", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);
                swDoc.Extension.SelectByID2("Эскиз26@15-001-1@15-000", "SKETCH", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Hole-M8@15-001-1@15-000", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditUnsuppress2();
                swDoc.Extension.SelectByID2("Эскиз23@15-001-1@15-000", "SKETCH", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSuppress2();

                swDoc.Extension.SelectByID2("Винт самосверл 6-гр.гол с шайбой-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Rivet Bralo-1@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Rivet Bralo-2@15-000", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("ЗеркальныйКомпонент2", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Fasteners - M8", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditUnsuppress2();
                swDoc.ClearSelection2(true);
                swDoc.Extension.SelectByID2("MirrorFasteners - M8", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                swDoc.EditUnsuppress2();
            }
            swDoc.ForceRebuild3(false);

            #endregion

            #region Сохранение и изменение элементов

            var addwidth = 100;
            var addwidth2 = 75;
            var type4 = 0;
            var divwidth = 1;
            if (type == "2" || type == "6")
            {
                addwidth = 75;
                divwidth = 2;
            }
            if (type == "4")
            {
                type4 = 170;
                addwidth2 = 170 + 75; 
            }
            
            var widthD = (Convert.ToDouble(width)/divwidth + addwidth);
            var lengthD = (Convert.ToDouble(lenght) - 28.5);
            const double step = 200;
            const double step2 = 150;
            var weldW = Convert.ToDouble((Math.Truncate(Convert.ToDouble(lenght) / step) + 1));
            var weldW2 = Convert.ToDouble((Math.Truncate(Convert.ToDouble(lenght) / step2) + 1));
            var newComponents = new List<FileInfo>();

            //15-001
            try
            {
                _swApp.IActivateDoc2("15-001", true, 0);
                var newPartName = $"15-0{type}-01-{width}-{lenght}";
                var newPartPath = $@"{Settings.Default.DestinationFolder}\{RoofDestinationFolder}\{newPartName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("15-000.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("15-001-1@15-000", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("15-001.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("15-001",
                        $@"{Settings.Default.DestinationFolder}\{RoofDestinationFolder}\{newPartName}",
                        new[,]
                        {
                            {"D1@Эскиз1",  type == "5" || type == "6" ? Convert.ToString(140 + lengthD + type4) : Convert.ToString(lengthD + type4)},
                            {"D2@Эскиз1", Convert.ToString(widthD)},
                            {"D4@Эскиз27", Convert.ToString(addwidth2-4.62)},
                            {"D1@Эскиз27", Convert.ToString(90)},
                            {"D2@Эскиз27", Convert.ToString((75-4.62))},

                            {"D1@Эскиз24", type == "5" || type == "6" ? Convert.ToString(149.53) : Convert.ToString(9.53)},
                            
                            {"D1@Кривая2", Convert.ToString(weldW2*1000)},
                            {"D1@Кривая1", Convert.ToString(weldW*1000)}
                        },
                        false,
                        null);
                    try
                    {
                        VentsMatdll(new[] { "1700" }, new[] { "", "Шаргень", "2" }, newPartName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }

                    _swApp.CloseDoc(newPartName);
                }
            }
            catch (Exception e)
            {
                //Logger.Log(LogLevel.Error, string.Format("Во время изменения детали 15-001.SLDPRT произошла ошибка"), e);
                MessageBox.Show(e.Message);
            }

            //15-002
            if (type == "6")
            {   
                try
                {
                    _swApp.IActivateDoc2("15-002", true, 0);
                    var newPartName = $"15-0{type}-02-{width}-{lenght}";
                    var newPartPath =
                        $@"{Settings.Default.DestinationFolder}\{RoofDestinationFolder}\{newPartName}.SLDPRT";
                    if (File.Exists(newPartPath))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("15-000.SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("15-002-1@15-000", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("15-002.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("15-002",
                            $@"{Settings.Default.DestinationFolder}\{RoofDestinationFolder}\{newPartName}",
                            new[,]
                            {
                                {"D1@Эскиз1",  type == "5" || type == "6" ? Convert.ToString(140 + lengthD + type4) : Convert.ToString(lengthD + type4)},
                                {"D2@Эскиз1", Convert.ToString(widthD)},
                                {"D4@Эскиз27", Convert.ToString(addwidth2-4.62)},
                                {"D1@Эскиз27", Convert.ToString(90)},
                                {"D2@Эскиз27", Convert.ToString((75-4.62))},
                                                                                                                          
                                {"D2@Эскиз23", type == "5" || type == "6" ? Convert.ToString(165) : Convert.ToString(25)},
                                
                                {"D1@Кривая2", Convert.ToString(weldW2*1000)},
                                {"D1@Кривая1", Convert.ToString(weldW*1000)}
                            },
                            false,
                        null);
                        try
                        {
                            VentsMatdll(new[] { "1700" }, new[] { "", "Шаргень", "2" }, newPartName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                        _swApp.CloseDoc(newPartName);
                    }
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("15-000.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("15-001-3@15-000", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.Extension.SelectByID2("ЗеркальныйКомпонент2@15-000", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditSuppress2();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            else if (type != "6")
            {
                swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("15-000.SLDASM", true, 0)));
                swDoc.Extension.SelectByID2("15-002-1@15-000", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            switch (type)
            {
                case "2":
                case "6":
                swDoc.Extension.SelectByID2("Винт самосверл 6-гр.гол с шайбой-33@15-000", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                    break;
                default:
                    swDoc.Extension.SelectByID2("Винт самосверл 6-гр.гол с шайбой-26@15-000", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    break;
            }
            
            #endregion

            GabaritsForPaintingCamera(swDoc);

            swDoc.ForceRebuild3(true);
            swDoc.SaveAs2(newRoofPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            newComponents.Add(new FileInfo(newRoofPath));
            _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(newRoofPath).FullName));
            CheckInOutPdm(newComponents, true, Settings.Default.TestPdmBaseName);

            foreach (var newComponent in NewComponents)
            {
             //   ExportXmlSql.Export(newComponent.FullName);
            }

            if (onlyPath) return newRoofPath;
            MessageBox.Show(newRoofPath, "Модель построена");

            return newRoofPath;
        }

        #endregion    
    }
}
