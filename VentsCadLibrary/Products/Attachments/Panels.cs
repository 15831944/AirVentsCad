using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace VentsCadLibrary
{
    public partial class VentsCad
    {

        public class Panels : Product
        {
            public Panels()
            {

            }


            public override ProductPlace Place
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            internal override string DestinationFolder { get; } = @"\Проекты\Blauberg\02-01-Panels";

            internal override string TemplateFolder { get; } = @"\Библиотека проектирования\DriveWorks\02 - Panels";


            internal override string ModelName
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            internal override string ModelPath
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            
            public override void Build()
            {
                throw new NotImplementedException();
            }

            enum TypeOfPanel
            {
                Unrem,
                Removable,
                Coil
            }

            
        }

        private const string Panel50Folder = @"\Библиотека проектирования\DriveWorks\02 - Panels";

        public string DublePanel50Folder = @"\Библиотека проектирования\DriveWorks\02 - Duble Panel";

        public string DestinationFolder { get; set; } = @"\Проекты\Blauberg\02 - Панели";


        private const string Panels0201 = @"\Проекты\Blauberg\02-01-Panels";

        private const string Panels0204 = @"\Проекты\Blauberg\02-04-Removable Panels";

        private const string Panels0205 = @"\Проекты\Blauberg\02-05-Coil Panels";

        


        #region Начало построения 50-й панели

        public string Panels50(string[] typeOfPanel, string width, string height, string[] materialP1, string[] materialP2, string[] покрытие, bool onlyPath)
        {
            if (!ConvertToInt(new[] { width, height })) return "";

            //Логгер.Отладка("Начало построения 50-й панели. ", "", "Panels50BuildStr", "Panels50BuildStr");

            string modelPanelsPath;
            string modelName;
            string nameAsm;
            var modelType =
                $"50-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}-MW";

            switch (typeOfPanel[1])
            {
                case "Панель несъемная глухая":
                    modelPanelsPath = Panel50Folder;
                    nameAsm = "02-01";
                    modelName = "02-01";
                    DestinationFolder = Panels0201;
                    break;
                case "Панель съемная с ручками":
                    modelPanelsPath = Panel50Folder;
                    nameAsm = "02-01";
                    modelName = "02-04";
                    DestinationFolder = Panels0204;
                    break;
                case "Панель теплообменника":
                    modelPanelsPath = Panel50Folder;
                    nameAsm = "02-01";
                    modelName = "02-05";
                    DestinationFolder = Panels0205;
                    break;
                case "Панель двойная несъемная":
                    modelPanelsPath = DublePanel50Folder;
                    nameAsm = "02-104-50";
                    modelName = "02-01";
                    DestinationFolder = Panels0201;
                    break;
                case "Панель двойная съемная":
                    modelPanelsPath = DublePanel50Folder;
                    nameAsm = "02-104-50";
                    modelName = "02-01";
                    DestinationFolder = Panels0201;
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
            //        modelName + "-" + width + "-" + height + "-50",
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
            //        Height = Convert.ToInt32(height),
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
            //        Height = Convert.ToInt32(height),
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
            //        Height = Convert.ToInt32(height),
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
            //        Height = Convert.ToInt32(height),
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
            //        Height = Convert.ToInt32(height),
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
            //    Height = Convert.ToInt32(height),

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

            //var newPanel50Name = "02-" + typeOfPanel[0] + "-" + idAsm;

            #endregion

            var newDestPath = !typeOfPanel[1].Contains("Панель двойная съемная") ? DestinationFolder : Panels0204;
            var newModNumber = !typeOfPanel[1].Contains("Панель двойная съемная") ? modelName : "02-04";

            var newPanel50Name = newModNumber + "-" + width + "-" + height + "-" + modelType;

            Логгер.Информация("Построение панели - " + newPanel50Name, null, "", "Panels50BuildStr");

            var sourceFolder = LocalPath(VaultName);
            var destinationFolder = LocalPath(DestVaultName);

            var newPanel50Path = $@"{destinationFolder}{newDestPath}\{newPanel50Name}.SLDASM";

            if (File.Exists(new FileInfo(newPanel50Path).FullName))
            {
                if (onlyPath) return newPanel50Path;

                MessageBox.Show(newPanel50Path, "Данная модель уже находится в базе");
                return "";
            }

            #region modelPanelAsmbly        

            var modelPanelAsmbly = $@"{sourceFolder}{modelPanelsPath}\{nameAsm}.SLDASM";

            GetLastVersionAsmPdm(modelPanelAsmbly, VaultName);

            if (!InitializeSw(true)) return "";

            var swDoc = _swApp.OpenDoc6(modelPanelAsmbly, (int)swDocumentTypes_e.swDocASSEMBLY,
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            _swApp.Visible = true;

            var swAsm = (AssemblyDoc)swDoc;

            swAsm.ResolveAllLightWeightComponents(false);

            // Габариты
            var widthD = Convert.ToDouble(width);
            var heightD = Convert.ToDouble(height);
            var halfWidthD = Convert.ToDouble(widthD / 2);
            // Шаг заклепок
            const double step = 80;
            var rivetW = (Math.Truncate(widthD / step) + 1) * 1000;
            var rivetWd = (Math.Truncate(halfWidthD / step) + 1) * 1000;
            var rivetH = (Math.Truncate(heightD / step) + 1) * 1000;
            if (Math.Abs(rivetW - 1000) < 1) rivetW = 2000;
            // Коэффициенты и радиусы гибов   
            const string thiknessStr = "0,8";
            var bendParams = BendTable(thiknessStr);
            var bendRadius = Convert.ToDouble(bendParams[0]);
            var kFactor = Convert.ToDouble(bendParams[1]);

            #endregion

            // Переменные панели с ручками

            var wR = widthD / 2; // Расстояние межу ручками
            if (widthD < 1000)
            {
                wR = widthD * 0.5;
            }
            if (widthD >= 1000)
            {
                wR = widthD * 0.45;
            }
            if (widthD >= 1300)
            {
                wR = widthD * 0.4;
            }
            if (widthD >= 1700)
            {
                wR = widthD * 0.35;
            }

            #region typeOfPanel != "Панель двойная"

            // Тип панели
            if (modelName == "02-01" & !typeOfPanel[1].Contains("Панель двойная"))
            {
                swDoc.Extension.SelectByID2("Ручка MLA 120-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2("SC GOST 17475_gost-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2("SC GOST 17475_gost-2@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Threaded Rivets-5@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Threaded Rivets-6@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                // Удаление ненужных элементов панели
                const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                         (int)swDeleteSelectionOptions_e.swDelete_Children;
                swDoc.Extension.SelectByID2("Вырез-Вытянуть7@02-01-001-1@02-01", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);

                swDoc.Extension.SelectByID2("Ручка MLA 120-5@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2("SC GOST 17475_gost-9@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2("SC GOST 17475_gost-10@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Threaded Rivets-13@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Threaded Rivets-14@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                // Удаление ненужных элементов панели
                swDoc.Extension.SelectByID2("Вырез-Вытянуть8@02-01-001-1@02-01", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDoc.Extension.DeleteSelection2(deleteOption);
            }

            if (modelName == "02-04" || modelName == "02-05")
            {
                if (Convert.ToInt32(width) > 750)
                {
                    swDoc.Extension.SelectByID2("Ручка MLA 120-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-2@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-5@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-6@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    // Удаление ненужных элементов панели
                    const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                             (int)swDeleteSelectionOptions_e.swDelete_Children;
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть7@02-01-001-1@02-01", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);
                }
                else
                {
                    swDoc.Extension.SelectByID2("Ручка MLA 120-5@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-9@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-10@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-13@02-01", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-14@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    // Удаление ненужных элементов панели
                    const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                             (int)swDeleteSelectionOptions_e.swDelete_Children;
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть8@02-01-001-1@02-01", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);
                }
            }

            if (!typeOfPanel[1].Contains("Панель двойная"))
            {

                var newName =
                    $"{modelName}-01-{width}-{height}-50-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";


                var newPartPath = $@"{destinationFolder}{DestinationFolder}\{newName}.SLDPRT";

                if (File.Exists(newPartPath)) //ExistFileInPdmBase(newName + ".SLDPRT")
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("02-01.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-001-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-001.SLDPRT");
                }
                else if (!File.Exists(newPartPath))
                {
                    SwPartParamsChangeWithNewName("02-01-001",
                        $@"{destinationFolder}{DestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD).ToString()},
                            {"D2@Эскиз1", (widthD).ToString()},
                            {"D1@Кривая2", (rivetH).ToString()},
                            {"D1@Кривая1", (rivetW).ToString()},
                            {"D4@Эскиз30", (wR).ToString()},
                            {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')}
                        },
                        false);
                    try
                    {
                        VentsMatdll(materialP1, new[] { покрытие[6], покрытие[1], покрытие[2] }, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }

                    _swApp.CloseDoc(newName);
                }

                //Панель внутреняя

                var modelnewname = modelName;
                var modelPath = DestinationFolder;
                if (modelName == "02-04")
                {
                    modelnewname = "02-01";
                    modelPath = Panels0201;
                }

                #region

                //newName = String.Format("{0}{1}{2}{3}",
                //modelnewname + "-02-" + width + "-" + height + "-" + "50-" + materialP2[0];
                //   //string.IsNullOrEmpty(покрытие[6]) ? "" : "-" + покрытие[6],
                //   //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
                //   //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]
                //   );

                //newName = панельВнутренняя.NewName;

                //newName = modelnewname + "-02-" + width + "-" + height + "-" + "50-" + materialP2[3] + materialP2[3] == "AZ" ? "" : materialP2[1];

                #endregion

                newName =
                    $"{modelnewname}-02-{width}-{height}-50-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                newPartPath = $@"{destinationFolder}{modelPath}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("02-01.SLDASM", true, 0)));
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
                        $@"{destinationFolder}{modelPath}\{newName}",
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
                        false);
                    try
                    {
                        VentsMatdll(materialP2, new[] { покрытие[7], покрытие[4], покрытие[5] }, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    _swApp.CloseDoc(newName);
                }

                //Панель теплошумоизоляции

                if (modelName == "02-05")
                {
                    modelPath = Panels0201;
                }

                newName = "02-03-" + width + "-" + height; //newName = теплоизоляция.NewName;
                newPartPath = $@"{destinationFolder}{modelPath}\{newName}.SLDPRT";

                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("02-01.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-003-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-003.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-003",
                        $@"{destinationFolder}{DestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD - 10).ToString()},
                            {"D2@Эскиз1", (widthD - 10).ToString()}
                        },
                        false);
                    _swApp.CloseDoc(newName);
                }

                //Уплотнитель

                newName = "02-04-" + width + "-" + height; //newName = уплотнитель.NewName;

                newPartPath = $@"{destinationFolder}{modelPath}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("02-01.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-004-1@02-01", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-004.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-004",
                        $@"{destinationFolder}{DestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D6@Эскиз1", (heightD - 10).ToString()},
                            {"D3@Эскиз1", (widthD - 10).ToString()}
                        },
                        false);
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
                //    modelName + "-01-" + width + "-" + height + "-" + "50-" + materialP1
                //    //string.IsNullOrEmpty(покрытие[6]) ? "" : "-" + покрытие[6],
                //    //string.IsNullOrEmpty(покрытие[1]) ? "" : "-" + покрытие[1],
                //    //string.IsNullOrEmpty(покрытие[2]) ? "" : "-" + покрытие[2]
                //    );

                //var newName = панельВнешняя.NewName;
                //var newName = modelName + "-01-" + width + "-" + height + "-" + "50-" + materialP1[3] + materialP1[3] == "AZ" ? "" : materialP1[1];

                #endregion

                var currDestPath = typeOfPanel[1].Contains("несъемная") ? DestinationFolder : Panels0204;
                var curNumber = typeOfPanel[1].Contains("несъемная") ? "01" : "04";

                //MessageBox.Show("currDestPath - " + currDestPath + "   curNumber -" + curNumber + " -- " + typeOfPanel[1].Contains("несъемная"));

                if (typeOfPanel[1].Contains("несъемная"))
                {
                    //MessageBox.Show("Не съемная - " + typeOfPanel[1]);

                    swDoc.Extension.SelectByID2("Ручка MLA 120-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17475_gost-2@02-104-50", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-1@02-104-50", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-2@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    // Удаление ненужных элементов панели
                    const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                             (int)swDeleteSelectionOptions_e.swDelete_Children;
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@02-01-101-50-1@02-104-50", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                    swDoc.Extension.DeleteSelection2(deleteOption);

                    //MessageBox.Show("Удалилось");

                }

                var newName =
                    $"{modelName}-{curNumber}-{width}-{height}-50-{materialP1[3]}{(materialP1[3] == "AZ" ? "" : materialP1[1])}";

                var newPartPath =
                    $@"{destinationFolder}{currDestPath}\{newName}.SLDPRT";

                MessageBox.Show("newPartPath -" + newPartPath);

                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-101-50-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-101-50.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-101-50",
                        $@"{destinationFolder}{currDestPath}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", (heightD).ToString()},
                            {"D2@Эскиз1", (widthD/2).ToString()},
                            {"D1@Кривая4", (rivetH).ToString()},
                            {"D1@Кривая3", (rivetWd).ToString()},
                            {"D1@Кривая5", (rivetH).ToString()},

                            {"D2@Эскиз47", (wR/2).ToString()},

                            {"Толщина@Листовой металл", materialP1[1].Replace('.', ',')},
                            {"D1@Листовой металл", (bendRadius).ToString()},
                            {"D2@Листовой металл", (kFactor*1000).ToString()}
                        },
                        false);
                    try
                    {
                        VentsMatdll(materialP1, new[] { покрытие[6], покрытие[1], покрытие[2] }, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    _swApp.CloseDoc(newName);
                }

                #region Name before

                //Панель внутреняя
                //newName = String.Format("{0}{1}{2}{3}",
                //   modelName + "-02-" + width + "-" + height + "-" + "50-" + materialP1
                //   //string.IsNullOrEmpty(покрытие[7]) ? "" : "-" + покрытие[7],
                //   //string.IsNullOrEmpty(покрытие[3]) ? "" : "-" + покрытие[3],
                //   //string.IsNullOrEmpty(покрытие[4]) ? "" : "-" + покрытие[4]
                //   );

                //newName = панельВнутренняя.NewName;
                //newName = modelName + "-02-" + width + "-" + height + "-" + "50-" + materialP2[3] + materialP2[3] == "AZ" ? "" : materialP2[1];

                #endregion

                newName =
                    $"{modelName}-02-{width}-{height}-50-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                newPartPath = $@"{destinationFolder}{DestinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-102-50-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-102-50.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-102-50",
                        $@"{destinationFolder}{DestinationFolder}\{newName}",
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
                        false);

                    try
                    {
                        VentsMatdll(materialP1, new[] { покрытие[7], покрытие[4], покрытие[5] }, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }

                    _swApp.CloseDoc(newName);
                }

                #region

                // Профиль 02-01-103-50
                //newName = деталь103.NewName;
                // modelName + "-03-" + width + "-" + height + "-" + "50-" + materialP2[3] + materialP2[3] == "AZ" ? "" : materialP2[1];

                #endregion

                newName =
                    $"{modelName}-03-{width}-{height}-50-{materialP2[3]}{(materialP2[3] == "AZ" ? "" : materialP2[1])}";

                newPartPath = $@"{destinationFolder}{DestinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-103-50-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-103-50.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-103-50",
                        $@"{destinationFolder}{DestinationFolder}\{newName}",
                        newParams: new[,]
                        {
                            {"D1@Эскиз1", (heightD - 15).ToString()},
                            {"D1@Кривая1", (rivetH).ToString()},
                            {"Толщина@Листовой металл", thiknessStr},
                            {"D1@Листовой металл", (bendRadius).ToString()},
                            {"D2@Листовой металл", (kFactor*1000).ToString()}
                        },
                        newFuncOfAdding: false);
                    _swApp.CloseDoc(newName);
                }

                //Панель теплошумоизоляции

                //newName = теплоизоляция.NewName;

                newName = "02-03-" + width + "-" + height;

                newPartPath = $@"{destinationFolder}{DestinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-003-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-003.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-003",
                        $@"{destinationFolder}{DestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D1@Эскиз1", Convert.ToString(heightD - 10)},
                            {"D2@Эскиз1", Convert.ToString(widthD - 10)}
                        },
                        false);
                    _swApp.CloseDoc(newName);
                }

                //Уплотнитель

                //newName = уплотнитель.NewName;

                newName = "02-04-" + width + "-" + height;

                newPartPath = $@"{destinationFolder}{DestinationFolder}\{newName}.SLDPRT";
                if (File.Exists(newPartPath))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("02-104-50.SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("02-01-004-1@02-104-50", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("02-01-004.SLDPRT");
                }
                else if (File.Exists(newPartPath) != true)
                {
                    SwPartParamsChangeWithNewName("02-01-004",
                        $@"{destinationFolder}{DestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D6@Эскиз1", Convert.ToString(heightD - 10)},
                            {"D3@Эскиз1", Convert.ToString(widthD - 10)}
                        },
                        false);
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

            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm, true, 0)));
            var swModelDocExt = swDoc.Extension;
            var swCustPropForDescription = swModelDocExt.CustomPropertyManager[""];
            swCustPropForDescription.Set("Наименование", typeOfPanel[1]);
            swCustPropForDescription.Set("Description", typeOfPanel[1]);

            GabaritsForPaintingCamera(swDoc);

            swDoc.EditRebuild3();
            swDoc.ForceRebuild3(true);
            swDoc.SaveAs2(new FileInfo(newPanel50Path).FullName, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            NewComponents.Add(
                new VaultSystem.VentsCadFile
                {
                    LocalPartFileInfo = newPanel50Path
                });
            _swApp.CloseDoc(new FileInfo(newPanel50Path).Name);
            _swApp.Visible = true;

            List<VaultSystem.VentsCadFile> newFilesList;
            VaultSystem.CheckInOutPdmNew(NewComponents, true, //DestVaultName, 
                out newFilesList);

            foreach (var newComponent in NewComponents)
            {
                //PartInfoToXml(newComponent.LocalPartFileInfo);
            }
            if (onlyPath) return newPanel50Path;
            MessageBox.Show(newPanel50Path, "Модель построена");

            return newPanel50Path;
        }

        #endregion

    }
}
