using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Windows.Forms;

namespace VentsCadLibrary
{
    partial class VentsCad
    {
        public class MontageFrame : Product
        {
            public override ProductPlace Place { get; set; }

            ProductFactory.Material Material;

            /// <summary>
            /// Монтажная рама
            /// </summary>
            /// <param name="width">The width </param>
            /// <param name="lenght">The lenght </param>            
            /// <param name="typeOfMf">The type of mf.</param>
            /// <param name="frameOffset">The frame offset.</param>
            /// <param name="material">The material.</param>         
            /// <returns></returns>
            public MontageFrame(string type, string width, string lenght, string frameOffset, ProductFactory.Material material)
            {
                if (!ConvertToInt(new[] { width, lenght })) throw new Exception("Недопустимі розміри");

                Type = type;
                Width = width;
                Lenght = lenght;
                FrameOffset = frameOffset;
                Material = material;

                addMatName = "";

                if (material.Value != "1800" & material.Thikness == "2")
                {
                    addMatName = "HK";
                }

                #region Проверка введенных значений и открытие сборки                

                typeOfMfs = "-0" + type;
                if (type == "0")
                {
                    typeOfMfs = "";
                }

                // Тип рымы
                internalCrossbeam = false; // Погашение внутренней поперечной балки
                internalLongitudinalBeam = false; // Погашение внутренней продольной балки
                var frameOffcetStr = "";
                switch (type)
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

                ModelName = $"10-{material.Thikness}{addMatName}-{width}-{lenght}{typeOfMfs}{frameOffcetStr}.SLDASM";
                ModelPath = $@"{destRootFolder}\{DestinationFolder}\{ModelName}";


                Place = GetPlace();
            }

            internal string typeOfMfs;

            internal string addMatName;

            public override void Build()
            {
                if (Exist) return;

                NewComponents = null;

                MessageBox.Show(DateTime.Now.Hour.ToString());

                var modelMontageFramePath = $@"{sourceRootFolder}{TemplateFolder}\{"10-4"}.SLDASM";

                GetLastVersionAsmPdm(modelMontageFramePath, VaultName);
                GetLastVersionAsmPdm($@"{sourceRootFolder}{TemplateFolder}\10-02-01-4.SLDPRT", VaultName);

                var swDocMontageFrame = _swApp.OpenDoc6(modelMontageFramePath, (int)swDocumentTypes_e.swDocASSEMBLY,
                    (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                _swApp.Visible = true;
                var swAsm = (AssemblyDoc)swDocMontageFrame;
                swAsm.ResolveAllLightWeightComponents(false);

                #endregion

                #region Основные размеры, величины и переменные

                // Габариты Ширина меньше ширины установки на 20мм Длина по размеру блока
                var width = Convert.ToDouble(Width); // Поперечные балки
                var lenght = Convert.ToDouble(Lenght); // Продольная балка
                var offsetI = Convert.ToDouble(Convert.ToString(Convert.ToDouble(FrameOffset) * 10)); // Смещение поперечной балки
                if (offsetI > (lenght - 125) * 10)
                {
                    offsetI = (lenght - 250) * 10;
                    MessageBox.Show("Смещение превышает допустимое значение! Программой установлено - " +
                                    (offsetI / 10));
                }

                #region  Металл и х-ки гибки

                // TODO Коэффициенты и радиусы гибов  
                //var sqlBaseData = new SqlBaseData();
                //var bendParams = sqlBaseData.BendTable(Material.Thikness);
                //var bendRadius = Convert.ToDouble(bendParams[0]);
                //var kFactor = Convert.ToDouble(bendParams[1]);

                var bendRadius = Convert.ToDouble("1");
                var kFactor = Convert.ToDouble("1");

                #endregion

                #endregion

                #region Изменение размеров элементов и компонентов сборки

                var thikness = Convert.ToDouble(Material.Thikness) / 1000;
                bendRadius = bendRadius / 1000;
                var w = Convert.ToDouble(width) / 1000;
                var l = Convert.ToDouble(lenght) / 1000;
                var offset = Convert.ToDouble(offsetI) / 10000;
                var offsetMirror = Convert.ToDouble(lenght * 10 - offsetI) / 10000;

                #region 10-02-4 Зеркальная 10-01-4

                if (Type == "3")
                {
                    swDocMontageFrame.Extension.SelectByID2("10-01-01-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm = ((AssemblyDoc)(swDocMontageFrame));
                    swAsm.ReplaceComponents(sourceRootFolder + TemplateFolder + "\\10-02-01-4.SLDPRT", "", false, true);
                    swAsm.ResolveAllLightWeightComponents(false);

                    //Продольная зеркальная балка (Длина установки)
                    swDocMontageFrame.Extension.SelectByID2("D1@Эскиз1@10-02-01-4-1@10-4", "DIMENSION", 0, 0, 00, false, 0, null, 0);
                    ((Dimension)(swDocMontageFrame.Parameter("D1@Эскиз1@10-02-01-4.Part"))).SystemValue = l;
                    //  Длина установки  0.8;

                    swDocMontageFrame.Extension.SelectByID2("D3@Эскиз25@10-02-01-4-1@10-4", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    ((Dimension)(swDocMontageFrame.Parameter("D3@Эскиз25@10-02-01-4.Part"))).SystemValue = offsetMirror;
                    //Смещение поперечной балки от края;
                    swDocMontageFrame.EditRebuild3();
                    swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-02-01-4-1@10-4", "BODYFEATURE", 0, 0, 0,
                        false, 0, null, 0);
                    swDocMontageFrame.ActivateSelectedFeature();
                    swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-02-01-4-1@10-4", "DIMENSION", 0, 0, 0,
                        false, 0, null, 0);
                    ((Dimension)(swDocMontageFrame.Parameter("D1@Листовой металл@10-02-01-4.Part"))).SystemValue =
                        bendRadius; // Радиус гиба  0.005;
                    swDocMontageFrame.EditRebuild3();
                    swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-02-01-4-1@10-4", "BODYFEATURE", 0, 0, 0,
                        false, 0, null, 0);
                    swDocMontageFrame.ActivateSelectedFeature();
                    swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-02-01-4-1@10-4", "DIMENSION", 0, 0, 0,
                        false, 0, null, 0);
                    ((Dimension)(swDocMontageFrame.Parameter("D2@Листовой металл@10-01-01-4.Part"))).SystemValue = kFactor;
                    // K-Factor  0.55;
                    swDocMontageFrame.EditRebuild3();
                    swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-02-01-4-1@10-4", "DIMENSION", 0, 0,
                        0, false, 0, null, 0);
                    ((Dimension)(swDocMontageFrame.Parameter("Толщина@Листовой металл@10-02-01-4.Part"))).SystemValue =
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
                ((Dimension)(swDocMontageFrame.Parameter("D1@Эскиз1@10-01-01-4.Part"))).SystemValue = l;
                //  Длина установки  0.8;
                swDocMontageFrame.Extension.SelectByID2("D3@Эскиз25@10-01-01-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null,
                    0);
                ((Dimension)(swDocMontageFrame.Parameter("D3@Эскиз25@10-01-01-4.Part"))).SystemValue = offset;
                //Смещение поперечной балки от края;
                swDocMontageFrame.EditRebuild3();
                //swApp.SendMsgToUser(Offset.ToString());
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-01-01-4-2@10-4", "BODYFEATURE", 0, 0, 0, false,
                    0, null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-01-01-4-2@10-4", "DIMENSION", 0, 0, 0, false,
                    0, null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("D1@Листовой металл@10-01-01-4.Part"))).SystemValue = bendRadius;
                // Радиус гиба  0.005;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-01-01-4-2@10-4", "BODYFEATURE", 0, 0, 0, false,
                    0, null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-01-01-4-2@10-4", "DIMENSION", 0, 0, 0, false,
                    0, null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("D2@Листовой металл@10-01-01-4.Part"))).SystemValue = kFactor;
                // K-Factor  0.55;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-01-01-4-2@10-4", "DIMENSION", 0, 0, 0,
                    false, 0, null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("Толщина@Листовой металл@10-01-01-4.Part"))).SystemValue =
                    thikness; // Толщина Листового металла 0.006;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.ClearSelection2(true);

                #endregion

                #region 10-04-4-2

                swDocMontageFrame.Extension.SelectByID2("D1@Эскиз1@10-04-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("D1@Эскиз1@10-04-4.Part"))).SystemValue = (l - 0.14);
                // Длина установки - 140  0.66;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-04-4-2@10-4", "BODYFEATURE", 0, 0, 0, false, 0,
                    null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-04-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0,
                    null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("D1@Листовой металл@10-04-4.Part"))).SystemValue = bendRadius;
                // Радиус гиба  0.005;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-04-4-2@10-4", "BODYFEATURE", 0, 0, 0, false, 0,
                    null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-04-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0,
                    null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("D2@Листовой металл@10-04-4.Part"))).SystemValue = kFactor;
                // K-Factor  0.55;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-04-4-2@10-4", "DIMENSION", 0, 0, 0,
                    false, 0, null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("Толщина@Листовой металл@10-04-4.Part"))).SystemValue = thikness;
                // Толщина Листового металла 0.006;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.ClearSelection2(true);

                #endregion

                //Поперечная балка (Ширина установки)

                #region 10-03-4

                swDocMontageFrame.Extension.SelectByID2("D2@Эскиз1@10-03-01-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("D2@Эскиз1@10-03-01-4.Part"))).SystemValue = (w - 0.12);
                //  Ширина установки - 20 - 100  0.88;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-03-01-4-2@10-4", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D1@Листовой металл@10-03-01-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("D1@Листовой металл@10-03-01-4.Part"))).SystemValue = bendRadius;
                // Радиус гиба  0.005;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Листовой металл@10-03-01-4-2@10-4", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
                swDocMontageFrame.ActivateSelectedFeature();
                swDocMontageFrame.Extension.SelectByID2("D2@Листовой металл@10-03-01-4-2@10-4", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("D2@Листовой металл@10-03-01-4.Part"))).SystemValue = kFactor;
                // K-Factor  0.55;
                swDocMontageFrame.EditRebuild3();
                swDocMontageFrame.Extension.SelectByID2("Толщина@Листовой металл@10-03-01-4-2@10-4", "DIMENSION", 0, 0, 0,
                    false, 0, null, 0);
                ((Dimension)(swDocMontageFrame.Parameter("Толщина@Листовой металл@10-03-01-4.Part"))).SystemValue =
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
                    swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-24@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-45@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocMontageFrame.Extension.SelectByID2("Washer 11371_gost-46@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocMontageFrame.Extension.SelectByID2("Washer 6402_gost-26@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocMontageFrame.Extension.SelectByID2("Hex Nut 5915_gost-22@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-25@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-25@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDocMontageFrame.EditDelete();

                    // Удаление ненужных элементов продольной балки
                    const int deleteOption =
                        (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                        (int)swDeleteSelectionOptions_e.swDelete_Children;
                    swDocMontageFrame.Extension.SelectByID2("Вырез-Вытянуть8@10-01-01-4-2@10-4", "BODYFEATURE", 0, 0, 0,
                        false, 0, null, 0);
                    swDocMontageFrame.Extension.DeleteSelection2(deleteOption);
                }

                #endregion

                #region Удаление продольной балки

                // Погашение внутренней продольной балки
                if (internalLongitudinalBeam == false)
                {
                    foreach (var s in new[] { "5", "6", "7", "8", "13" })
                    {
                        swDocMontageFrame.Extension.SelectByID2("Hex Bolt 7805_gost-" + s + "@10-4", "COMPONENT", 0, 0, 0, true, 0, null, 0);
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
                    swDocMontageFrame.Extension.SelectByID2("Регулируемая ножка-10@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDocMontageFrame.EditDelete();
                    swDocMontageFrame.Extension.SelectByID2("Регулируемая ножка-11@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDocMontageFrame.EditDelete();

                    foreach (var s in new[] { "10", "11", "40", "41", "42", "43" })
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
                switch (Type)
                {
                    //case "2":
                    case "0":
                        typeOfMfs = "";
                        break;
                    case "3":
                    case "2":
                    case "1":
                        typeOfMfs = "-0" + Type;
                        break;
                }


                var newPartName = string.Format("10-01-01-{0}{4}-{1}{2}{3}.SLDPRT", Material.Thikness, lenght, FrameOffset,
                    typeOfMfs, addMatName);

                var newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";
                if (File.Exists(newPartPath))
                {
                    swDocMontageFrame = ((ModelDoc2)(_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                    swDocMontageFrame.Extension.SelectByID2("10-01-01-4-2@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("10-01-01-4.SLDPRT");
                }
                else
                {
                    swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

                    AddMaterial(Material, newPartPath);
                    ComponentToAdd(newPartPath);

                    _swApp.CloseDoc(newPartName);
                }

                #endregion

                //

                #region 10-02-01-4 - Деталь Зеркальная 10-01-01-4

                if (Type == "3")
                {
                    _swApp.IActivateDoc2("10-02-01-4", false, 0);
                    swPartDoc = _swApp.IActiveDoc2;
                    switch (Type)
                    {
                        case "2":
                        case "0":
                            typeOfMfs = "";
                            break;
                        case "3":
                        case "1":
                            typeOfMfs = "-0" + Type;
                            break;
                    }

                    newPartName = string.Format("10-02-01-{0}{4}-{1}{2}{3}.SLDPRT", Material.Thikness, lenght, FrameOffset,
                        typeOfMfs, addMatName);

                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";
                    if (File.Exists(newPartPath))
                    {
                        swDocMontageFrame = ((ModelDoc2)(_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                        swDocMontageFrame.Extension.SelectByID2("10-02-01-4-1@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("10-02-01-4.SLDPRT");
                    }
                    else
                    {
                        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        AddMaterial(Material, newPartPath);
                        _swApp.CloseDoc(newPartName);
                        ComponentToAdd(newPartName);
                    }

                    #endregion

                    #region 10-04-4 - Деталь

                    if (internalLongitudinalBeam)
                    {
                        _swApp.IActivateDoc2("10-04-4", false, 0);
                        swPartDoc = ((IModelDoc2)(_swApp.ActiveDoc));

                        newPartName = string.Format("10-04-{0}{2}-{1}.SLDPRT", Material.Thikness, (lenght - 140), addMatName);

                        newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";
                        if (File.Exists(newPartPath))
                        {
                            swDocMontageFrame = ((ModelDoc2)(_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                            swDocMontageFrame.Extension.SelectByID2("10-04-4-2@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("10-04-4.SLDPRT");
                        }
                        else
                        {
                            swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            AddMaterial(Material, newPartPath);
                            _swApp.CloseDoc(newPartName);
                            ComponentToAdd(newPartPath);
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
                    swPartDoc = ((IModelDoc2)(_swApp.ActiveDoc));

                    switch (Type)
                    {
                        case "3":
                        case "2":
                            typeOfMfs = "-0" + Type;
                            break;
                        case "1":
                        case "0":
                            typeOfMfs = "";
                            break;
                    }


                    newPartName = string.Format("10-03-01-{0}{3}-{1}{2}.SLDPRT", Material.Thikness, (width - 120), typeOfMfs, addMatName);

                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";
                    var newPrt0202 = string.Format("10-02-01-{0}{3}-{1}{2}.SLDPRT", Material.Thikness, (width - 120), typeOfMfs,
                        addMatName);
                    newPrt0202 = $@"{destRootFolder}\{DestinationFolder}\{newPrt0202}";

                    if (File.Exists(newPartPath))
                    {
                        swDocMontageFrame = ((ModelDoc2)(_swApp.ActivateDoc2("10-4.SLDASM", true, 0)));
                        swDocMontageFrame.Extension.SelectByID2("10-03-01-4-2@10-4", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("10-03-01-4.SLDPRT");
                    }
                    else
                    {
                        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        AddMaterial(Material, newPartPath);
                        if (Type == "2")
                        {
                            swPartDoc.Extension.SelectByID2("D1@Эскиз28@" + Path.GetFileNameWithoutExtension(newPrt0202) + ".SLDPRT",
                                "DIMENSION", 0, 0, 0, false, 0, null, 0);
                            ((Dimension)(swPartDoc.Parameter("D1@Эскиз28"))).SystemValue = -0.05;

                            swPartDoc.EditRebuild3();
                            swPartDoc.SaveAs2(newPrt0202, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            AddMaterial(Material, newPrt0202);
                            _swApp.CloseDoc(newPrt0202);
                            ComponentToAdd(newPrt0202);
                        }
                        _swApp.CloseDoc(newPartName);
                    }

                    #endregion

                    #endregion

                    _swApp.IActivateDoc2("10-4.SLDASM", false, 0);
                    swDocMontageFrame = ((ModelDoc2)(_swApp.ActiveDoc));

                    GabaritsForPaintingCamera(swDocMontageFrame);

                    swDocMontageFrame.ForceRebuild3(true);

                    swDocMontageFrame.SaveAs2(ModelPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);

                    ComponentToAdd(ModelPath);

                    #endregion

                    _swApp.CloseDoc(ModelPath);
                    _swApp = null;

                    List<VaultSystem.VentsCadFile> newFilesList;
                    VaultSystem.CheckInOutPdmNew(NewComponents, true, out newFilesList);

                    #region Export To XML

                    foreach (var newComponent in NewComponents)
                    {
                        //   ExportXmlSql.Export(newComponent.FullName);
                    }

                    #endregion


                    #region

                    //var drawing = "12-00";
                    //if (modelName == "12-30")
                    //{
                    //    drawing = modelName;
                    //}

                    //Dimension myDimension;
                    //var modelSpigotDrw = $@"{sourceRootFolder}{TemplateFolder}\{drawing}.SLDDRW";

                    //GetLastVersionAsmPdm(modelSpigotDrw, VaultName);

                    //if (!InitializeSw(true)) return;

                    //var swDrwSpigot = _swApp.OpenDoc6(modelSpigotDrw, (int)swDocumentTypes_e.swDocDRAWING,
                    //    (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "", 0, 0);

                    //if (swDrwSpigot == null) return;

                    //ModelDoc2 swDoc = _swApp.ActivateDoc2("12-00", false, 0);
                    //var swAsm = (AssemblyDoc)swDoc;
                    //swAsm.ResolveAllLightWeightComponents(false);

                    //switch (modelName)
                    //{
                    //    case "12-20":
                    //        DelEquations(5, swDoc);
                    //        DelEquations(4, swDoc);
                    //        DelEquations(3, swDoc);
                    //        break;
                    //    case "12-30":
                    //        DelEquations(0, swDoc);
                    //        DelEquations(0, swDoc);
                    //        DelEquations(0, swDoc);
                    //        break;
                    //}
                    //swDoc.ForceRebuild3(true);

                    //string newPartName;
                    //string newPartPath;
                    //IModelDoc2 swPartDoc;

                    //#region Удаление ненужного

                    //string[] itemsToDelete = null;

                    //switch (Type)
                    //{
                    //    case "20":
                    //        itemsToDelete = new[] { "12-30-001-1", "12-30-001-2", "12-30-002-1", "12-30-002-2",
                    //                            "ВНС-96.61.002-1", "ВНС-96.61.002-2", "ВНС-96.61.002-3", "ВНС-96.61.002-4",
                    //                            "ВНС-96.61.002-5", "ВНС-96.61.002-6", "ВНС-96.61.002-7", "ВНС-96.61.002-8",
                    //                            "12-30-001-3", "12-30-001-4", "12-30-002-3", "12-30-002-4",
                    //                            "12-003-2", "Клей-2" };
                    //        break;
                    //    case "30":
                    //        itemsToDelete = new[] { "12-20-001-1", "12-20-001-2", "12-20-002-1", "12-20-002-2",
                    //                            "ВНС-96.61.001-1", "ВНС-96.61.001-2", "ВНС-96.61.001-3", "ВНС-96.61.001-4",
                    //                            "ВНС-96.61.001-5", "ВНС-96.61.001-6", "ВНС-96.61.001-7", "ВНС-96.61.001-8",
                    //                            "12-20-001-3", "12-20-001-4", "12-20-002-3", "12-20-002-4",
                    //                            "12-003-1", "Клей-1"};
                    //        break;
                    //}

                    //foreach (var item in itemsToDelete)
                    //{
                    //    DoWithSwDoc(_swApp, CompType.COMPONENT, item, Act.DeletWithOption);
                    //}

                    //DoWithSwDoc(_swApp, CompType.FTRFOLDER, "30", Act.Delete);
                    //DoWithSwDoc(_swApp, CompType.FTRFOLDER, "20", Act.Delete);
                    //#endregion

                    //#region Сохранение и изменение элементов

                    //string path;
                    //int fileId;
                    //int projectId;

                    //var addDimH = 1;
                    //if (modelName == "12-30")
                    //{
                    //    addDimH = 10;
                    //}

                    //var w = (Convert.ToDouble(Width) - 1) / 1000;
                    //var h = Convert.ToDouble((Convert.ToDouble(Height) + addDimH) / 1000);
                    //const double step = 50;
                    //var weldW = Convert.ToDouble((Math.Truncate(Convert.ToDouble(Width) / step) + 1));
                    //var weldH = Convert.ToDouble((Math.Truncate(Convert.ToDouble(Height) / step) + 1));

                    //if (modelName == "12-20")
                    //{
                    //    //12-20-001
                    //    _swApp.IActivateDoc2("12-20-001", false, 0);
                    //    swPartDoc = _swApp.IActiveDoc2;
                    //    newPartName = $"12-20-{Height}.SLDPRT";
                    //    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";

                    //    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    //    {
                    //        swDoc = ((ModelDoc2)(VentsCad._swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    //        swDoc.Extension.SelectByID2("12-20-001-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    //        swAsm.ReplaceComponents(newPartPath, "", true, true);
                    //        _swApp.CloseDoc("12-20-001.SLDPRT");
                    //    }
                    //    else
                    //    {
                    //        swDoc.Extension.SelectByID2("D1@Вытянуть1@12-20-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-20-001.Part")));
                    //        myDimension.SystemValue = h - 0.031;
                    //        swDoc.Extension.SelectByID2("D1@Кривая1@12-20-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-20-001.Part")));
                    //        myDimension.SystemValue = weldH;
                    //        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    //        ComponentToAdd(newPartPath);
                    //        _swApp.CloseDoc(newPartName);
                    //    }

                    //    //12-20-002
                    //    _swApp.IActivateDoc2("12-20-002", false, 0);
                    //    swPartDoc = _swApp.IActiveDoc2;
                    //    newPartName = $"12-20-{Width}.SLDPRT";
                    //    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";
                    //    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    //    {
                    //        swDoc = ((ModelDoc2)(VentsCad._swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    //        swDoc.Extension.SelectByID2("12-20-002-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    //        swAsm.ReplaceComponents(newPartPath, "", true, true);
                    //        _swApp.CloseDoc("12-20-002.SLDPRT");
                    //    }
                    //    else
                    //    {
                    //        swDoc.Extension.SelectByID2("D1@Вытянуть1@12-20-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-20-002.Part")));
                    //        myDimension.SystemValue = w - 0.031;
                    //        swDoc.Extension.SelectByID2("D1@Кривая1@12-20-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-20-002.Part")));
                    //        myDimension.SystemValue = weldW;
                    //        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    //        ComponentToAdd(newPartPath);
                    //        _swApp.CloseDoc(newPartName);
                    //    }

                    //    //12-003
                    //    _swApp.IActivateDoc2("12-003", false, 0);
                    //    swPartDoc = _swApp.IActiveDoc2;
                    //    newPartName = $"12-03-{Width}-{Height}.SLDPRT";
                    //    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";
                    //    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    //    {
                    //        swDoc = ((ModelDoc2)(VentsCad._swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    //        swDoc.Extension.SelectByID2("12-003-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    //        swAsm.ReplaceComponents(newPartPath, "", true, true);
                    //        _swApp.CloseDoc("12-003.SLDPRT");
                    //    }
                    //    else
                    //    {
                    //        swDoc.Extension.SelectByID2("D3@Эскиз1@12-003-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D3@Эскиз1@12-003.Part")));
                    //        myDimension.SystemValue = w;
                    //        swDoc.Extension.SelectByID2("D2@Эскиз1@12-003-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D2@Эскиз1@12-003.Part")));
                    //        myDimension.SystemValue = h;
                    //        swDoc.EditRebuild3();
                    //        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    //        ComponentToAdd(newPartPath);
                    //        _swApp.CloseDoc(newPartName);
                    //    }
                    //}

                    //if (modelName == "12-30")
                    //{
                    //    //12-30-001
                    //    _swApp.IActivateDoc2("12-30-001", false, 0);
                    //    swPartDoc = _swApp.IActiveDoc2;
                    //    newPartName = $"12-30-01-{Height}.SLDPRT";
                    //    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";
                    //    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    //    {
                    //        swDoc = ((ModelDoc2)(VentsCad._swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    //        swDoc.Extension.SelectByID2("12-30-001-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    //        swAsm.ReplaceComponents(newPartPath, "", true, true);
                    //        _swApp.CloseDoc("12-30-001.SLDPRT");
                    //    }
                    //    else
                    //    {
                    //        swDoc.Extension.SelectByID2("D1@Вытянуть1@12-30-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-30-001.Part")));
                    //        myDimension.SystemValue = h - 0.031;
                    //        swDoc.Extension.SelectByID2("D1@Кривая1@12-30-001-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-30-001.Part")));
                    //        myDimension.SystemValue = weldH;
                    //        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    //        ComponentToAdd(newPartPath);
                    //        _swApp.CloseDoc(newPartName);
                    //    }

                    //    //12-30-002

                    //    _swApp.IActivateDoc2("12-30-002", false, 0);
                    //    swPartDoc = _swApp.IActiveDoc2;
                    //    newPartName = $"12-30-02-{Width}.SLDPRT";
                    //    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";
                    //    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    //    {
                    //        swDoc = ((ModelDoc2)(VentsCad._swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    //        swDoc.Extension.SelectByID2("12-30-002-1@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    //        swAsm.ReplaceComponents(newPartPath, "", true, true);
                    //        _swApp.CloseDoc("12-30-002.SLDPRT");
                    //    }
                    //    else
                    //    {
                    //        swDoc.Extension.SelectByID2("D1@Вытянуть1@12-30-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@12-30-002.Part")));
                    //        myDimension.SystemValue = w - 0.031;
                    //        swDoc.Extension.SelectByID2("D1@Кривая1@12-30-002-1@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D1@Кривая1@12-30-002.Part")));
                    //        myDimension.SystemValue = weldH;
                    //        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    //        ComponentToAdd(newPartPath);
                    //        _swApp.CloseDoc(newPartName);
                    //    }

                    //    //12-003

                    //    _swApp.IActivateDoc2("12-003", false, 0);
                    //    swPartDoc = _swApp.IActiveDoc2;
                    //    newPartName = $"12-03-{Width}-{Height}.SLDPRT";
                    //    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newPartName}";
                    //    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), out path, out fileId, out projectId))
                    //    {
                    //        swDoc = ((ModelDoc2)(VentsCad._swApp.ActivateDoc2("12-00.SLDASM", true, 0)));
                    //        swDoc.Extension.SelectByID2("12-003-2@12-00", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    //        swAsm.ReplaceComponents(newPartPath, "", true, true);
                    //        _swApp.CloseDoc("12-003.SLDPRT");
                    //    }
                    //    else
                    //    {
                    //        swDoc.Extension.SelectByID2("D3@Эскиз1@12-003-2@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D3@Эскиз1@12-003.Part")));
                    //        myDimension.SystemValue = w;
                    //        swDoc.Extension.SelectByID2("D2@Эскиз1@12-003-2@12-00", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                    //        myDimension = ((Dimension)(swDoc.Parameter("D2@Эскиз1@12-003.Part")));
                    //        myDimension.SystemValue = h;
                    //        swDoc.EditRebuild3();
                    //        swPartDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    //        ComponentToAdd(newPartPath);
                    //        _swApp.CloseDoc(newPartName);
                    //    }
                    //}

                    //#endregion

                    //GabaritsForPaintingCamera(swDoc);

                    //swDoc.ForceRebuild3(true);
                    //swDoc.SaveAs2(ModelPath + ".SLDASM", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    //_swApp.CloseDoc(ModelName + ".SLDASM");                
                    //swDrwSpigot.Extension.SelectByID2("DRW1", "SHEET", 0, 0, 0, false, 0, null, 0);
                    //var drw = (DrawingDoc)(_swApp.IActivateDoc3(drawing + ".SLDDRW", true, 0));
                    //drw.ActivateSheet("DRW1");
                    //var m = 5;
                    //if (Convert.ToInt32(Width) > 500 || Convert.ToInt32(Height) > 500) { m = 10; }
                    //if (Convert.ToInt32(Width) > 850 || Convert.ToInt32(Height) > 850) { m = 15; }
                    //if (Convert.ToInt32(Width) > 1250 || Convert.ToInt32(Height) > 1250) { m = 20; }
                    //drw.SetupSheet5("DRW1", 12, 12, 1, m, true, destRootFolder + @"\Vents-PDM\\Библиотека проектирования\\Templates\\Основные надписи\\A3-A-1.slddrt", 0.42, 0.297, "По умолчанию", false);
                    //var errors = 0; var warnings = 0;

                    //swDrwSpigot.SaveAs4(ModelPath + ".SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, (int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings);
                    //ComponentToAdd(new[] { ModelPath + ".SLDDRW", ModelPath + ".SLDASM" });                

                    //_swApp.CloseDoc(ModelPath);
                    //_swApp.ExitApp();
                    //_swApp = null;

                    //List<VaultSystem.VentsCadFiles> newFilesList;
                    //VaultSystem.CheckInOutPdmNew(NewComponents, true, //DestVaultName,
                    //    out newFilesList);

                    //foreach (var item in newFilesList)
                    //{
                    //    if (item.LocalPartFileInfo.ToUpper().Contains(".SLDASM"))
                    //    {
                    //        AddInSqlBaseSpigot(item.PartName.Remove(item.PartName.LastIndexOf('.')), item.PartIdPdm,
                    //       Convert.ToInt32(Type), Convert.ToInt32(Height), Convert.ToInt32(Width));
                    //    }
                    //}

                    //foreach (var newComponent in NewComponents)
                    //{
                    //    PartInfoToXml(newComponent.LocalPartFileInfo);
                    //}

                    #endregion

                    Place = GetPlace();
                }
            }

            internal override string TemplateFolder => @"\Библиотека проектирования\DriveWorks\10 - Base Frame";
            internal override string DestinationFolder => @"\Проекты\Blauberg\10 - Рама монтажная";

            internal string Type;
            internal string Width;
            internal string Lenght;
            internal string FrameOffset;
            

            internal bool internalCrossbeam = false; // Погашение внутренней поперечной балки
            internal bool internalLongitudinalBeam = false; // Погашение внутренней продольной балки


            internal override string ModelName { get; set; }

            internal override string ModelPath { get; set; }


        }
    }


    
}
