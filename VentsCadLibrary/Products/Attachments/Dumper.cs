using System;
using System.Collections.Generic;
using System.IO;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace VentsCadLibrary
{
    public partial class VentsCad
    {
        public class Dumper : Product
        {
            public override ProductPlace Place { get; set; }

            public Dumper(string typeOfFlange, string width, string height, bool isOutDoor, string[] material = null)
            {
                IsOutDoor = isOutDoor;
                Width = width;
                Height = height;
                TypeOfFlange = typeOfFlange;

                if (!ConvertToInt(new[] { width, height })) return;

                switch (typeOfFlange)
                {
                    case "20":
                        modelName = "11-20";
                        modelDamperPath = TemplateFolder;
                        nameAsm = "11 - Damper";
                        break;
                    case "30":
                        modelName = "11-30";
                        modelDamperPath = TemplateFolder;
                        nameAsm = "11-30";
                        break;
                    default:
                        modelName = "11-20";
                        modelDamperPath = TemplateFolder;
                        nameAsm = "11 - Damper";
                        break;
                }

                modelType = $"{(material[3] == "AZ" ? "" : "-" + material[3])}{(material[3] == "AZ" ? "" : material[1])}";

                drawing = "11-20";
                if (modelName == "11-30")
                { drawing = modelName; }
                ModelName = modelName + "-" + width + "-" + height + modelType + (isOutDoor == true ? "-O" : "");
                ModelPath = $@"{destRootFolder}\{DestinationFolder}\{ModelName}.SLDDRW";

                Place = GetPlace();
            }            

            public override void Build()
            {
                if (Exist) return;                

                Dimension myDimension;
                var modelSpigotDrw = $@"{sourceRootFolder}{TemplateFolder}\{drawing}.SLDDRW";

                NewComponents.Clear();

                GetLastVersionAsmPdm(modelSpigotDrw, VaultName);

                if (!InitializeSw(true)) return;

                var swDrwSpigot = _swApp.OpenDoc6(modelSpigotDrw, (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "", 0, 0);              

                var modelDamperDrw = $@"{destRootFolder}{modelDamperPath}\{drawing}.SLDDRW";
                var modelLamel = $@"{sourceRootFolder}{modelDamperPath}\{"11-100"}.SLDDRW";

                GetLastVersionAsmPdm(new FileInfo(modelDamperDrw).FullName, VaultName);
                GetLastVersionAsmPdm(new FileInfo(modelLamel).FullName, VaultName);

                if (!InitializeSw(true)) return;

                var swDocDrw = _swApp.OpenDoc6(@modelDamperDrw, (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);

                ModelDoc2 swDoc = _swApp.ActivateDoc2(nameAsm, false, 0);

                var swAsm = (AssemblyDoc)swDoc;
                swAsm.ResolveAllLightWeightComponents(false);

                _swApp.Visible = true;

                // Габариты
                var widthD = Convert.ToDouble(Width);
                var heightD = Convert.ToDouble(Height);

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

                #region typeOfFlange = "20"

                if (TypeOfFlange == "20")
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

                    if (IsOutDoor)
                    {
                        swDoc.Extension.SelectByID2("Эскиз1", "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();

                        swDoc.Extension.SelectByID2("Вырез-Вытянуть10@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("Эскиз34@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                        #region To Delete

                        swDoc.Extension.SelectByID2("ВНС-901.41.302-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                        swDoc.Extension.SelectByID2("Rivet Bralo-130@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("Rivet Bralo-131@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("Rivet Bralo-129@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("Rivet Bralo-128@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("Rivet Bralo-127@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("Rivet Bralo-126@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                        #endregion

                        // 11-005 

                        newName = $"11-05-{Height}-{modelType}";
                        newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                        {
                            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                            swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", false, true);
                            _swApp.CloseDoc("11-005.SLDPRT");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-005",
                                $@"{destRootFolder}\{DestinationFolder}\{newName}",
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
                            ComponentToAdd(newPartPath);
                        }

                        // 11-006 
                        newName = $"11-06-{Height}-{modelType}";
                        newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                        {
                            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                            swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", false, true);
                            _swApp.CloseDoc("11-006.SLDPRT");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-006",
                                $@"{destRootFolder}\{DestinationFolder}\{newName}",
                                new[,]
                                {
                                    {"D3@Эскиз1", Convert.ToString(heightD)},
                                    {"D1@Кривая1", Convert.ToString(rivetH)},
                                    {"Толщина@Листовой металл", thiknessStr}
                                },
                                false,
                                null);

                            AddMaterial(material, newName);
                            ComponentToAdd(newPartPath);
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

                        #region To Delete

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

                        #endregion
                    }


                    // 11-001 
                    newName = "11-01-" + Height + modelType + (IsOutDoor ? "-O" : "");
                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";

                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-001-7@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-001.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-001",
                            $@"{destRootFolder}\{DestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D2@Эскиз1", Convert.ToString(heightD + 8.04)},
                                {"D1@Эскиз27", Convert.ToString(countL/10 - 100)},
                                {"D2@Эскиз27", Convert.ToString(100*Math.Truncate(countL/2000))},
                                {"D1@Кривая1", Convert.ToString(countL)},
                                
                                {"D1@Кривая2", Convert.ToString(rivetH)},
                                
                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);

                        AddMaterial(material, newName);
                        ComponentToAdd(newPartPath);
                    }

                    // 11-002 
                    newName = "11-03-" + Width + modelType;
                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-002-4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("11-002.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-002",
                            $@"{destRootFolder}\{DestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D2@Эскиз1", Convert.ToString(widthD - 3.96)},
                                {"D1@Кривая1", Convert.ToString(rivetW)},
                                
                                {"D1@Кривая3", Convert.ToString(rivetH)},
                                
                                {"Толщина@Листовой металл1", thiknessStr}
                            },
                            false,
                            null);

                        AddMaterial(material, newName);
                        ComponentToAdd(newPartPath);
                    }

                    // 11-003 
                    newName = "11-02-" + Height + modelType + (IsOutDoor ? "-O" : "");
                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-003-6@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-003.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-003",
                            $@"{destRootFolder}\{DestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D2@Эскиз1", Convert.ToString(heightD + 8.04)},
                                {"D1@Эскиз27", Convert.ToString(countL/10 - 100)},
                                {"D1@Кривая1", Convert.ToString(countL)},
                                
                                {"D1@Кривая2", Convert.ToString(rivetH)},
                                
                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);

                        AddMaterial(material, newName);
                        ComponentToAdd(newPartPath);
                    }

                    // 11-004
                    newName = "11-04-" + Width + "-" + hC + modelType;
                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-004-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("11-004.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-004",
                            $@"{destRootFolder}\{DestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D2@Эскиз1", Convert.ToString(widthD - 24)},
                                {"D7@Ребро-кромка1", Convert.ToString(hC)},
                                {"D1@Кривая1", Convert.ToString(countL)},
                                
                                {"D1@Кривая5", Convert.ToString(rivetH)},
                                
                                {"Толщина@Листовой металл1", thiknessStr}
                            },
                            false,
                            null);

                        AddMaterial(material, newName);
                        ComponentToAdd(newPartPath);
                        
                    }

                    //11-100 Сборка лопасти
                    var newNameAsm = "11-" + Width;
                    var newPartPathAsm =
                        $@"{destRootFolder}{DestinationFolder}\{newNameAsm}.SLDASM";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
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
                        newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
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
                            myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                            myDimension.SystemValue = (widthD - 23) / 1000;
                            _swApp.ActivateDoc2("11-101", false, 0);
                            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                            swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            _swApp.CloseDoc(newName);

                            ComponentToAdd(newPartPath);                            
                        }

                        #endregion

                        _swApp.ActivateDoc2("11-100", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.ForceRebuild3(false);
                        var docDrw100 = _swApp.OpenDoc6(modelLamel, (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                        swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        _swApp.CloseDoc(newNameAsm);
                        docDrw100.ForceRebuild3(false);
                        var drwNewName = $@"{destRootFolder}\{DestinationFolder}\{newNameAsm}.SLDDRW";
                        docDrw100.SaveAs2(drwNewName, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(drwNewName).FullName) + " - DRW1");

                        ComponentToAdd(new[] { newPartPath, drwNewName });                        
                    }
                }

                #endregion

                #region typeOfFlange = "30"

                if (TypeOfFlange == "30")
                {
                    string newName;
                    string newPartPath;

                    if (IsOutDoor)
                    {
                        swDoc.Extension.SelectByID2("Вырез-Вытянуть7@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("Эскиз27@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                        var itemsToDelete = new[] { "ВНС-902.49.283-1", "Rivet Bralo-314", "Rivet Bralo-315", "Rivet Bralo-316",
                                                    "Rivet Bralo-317", "Rivet Bralo-318", "Rivet Bralo-319", "Rivet Bralo-320", "Rivet Bralo-321" };                                            
                        foreach (var item in itemsToDelete)
                        {
                            DoWithSwDoc(_swApp, CompType.COMPONENT, item, Act.Delete);
                        }

                        #region to delete

                        //swDoc.Extension.SelectByID2("ВНС-902.49.283-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                        //swDoc.Extension.SelectByID2("Rivet Bralo-314@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-323@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-322@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-321@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-320@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                        //swDoc.Extension.SelectByID2("Rivet Bralo-315@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-316@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-317@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-318@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-319@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                        #endregion

                        // 11-005 
                        newName = "11-05-" + Height + modelType;
                        newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                        {
                            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                            swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", false, true);
                            _swApp.CloseDoc("11-005.SLDPRT");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-005",
                                $@"{destRootFolder}\{DestinationFolder}\{newName}",
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
                            ComponentToAdd(newPartPath);
                        }

                        // 11-006 
                        newName = "11-06-" + Height + modelType;
                        newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                        {
                            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                            swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", false, true);
                            _swApp.CloseDoc("11-006.SLDPRT");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-006",
                                $@"{destRootFolder}\{DestinationFolder}\{newName}",
                                new[,]
                                {
                                    {"D3@Эскиз1", Convert.ToString(heightD)},
                                    {"D1@Кривая1", Convert.ToString(rivetH)},
                                    {"Толщина@Листовой металл", thiknessStr}
                                },
                                false,
                                null);
                            AddMaterial(material, newName);
                            ComponentToAdd(newPartPath);                            
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

                        var itemsToDelete = new[] { "11-005-1", "11-006-1",
                                            "Rivet Bralo-346", "Rivet Bralo-347", "Rivet Bralo-348", "Rivet Bralo-349", "Rivet Bralo-350", "Rivet Bralo-351", "Rivet Bralo-356", "Rivet Bralo-357",
                                            "Rivet Bralo-358", "Rivet Bralo-359", "Rivet Bralo-360", "Rivet Bralo-361",
                                            "Rivet Bralo-240", "Rivet Bralo-241", "Rivet Bralo-242", "Rivet Bralo-243", "Rivet Bralo-244", "Rivet Bralo-245", "Rivet Bralo-246", "Rivet Bralo-247",
                                            "Rivet Bralo-248", "Rivet Bralo-249", "Rivet Bralo-250", "Rivet Bralo-251",};
                        foreach (var item in itemsToDelete)
                        {
                            DoWithSwDoc(_swApp, CompType.COMPONENT, item, Act.Delete);
                        }                     

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
                    var lProfName = Width; //
                    var lProfNameLength = (widthD - 23) / 1000;

                    #region IsDouble

                    var isdouble = widthD > 1000;

                    if (!isdouble)
                    {
                        var itemsToDelete = new[] { "11-30-100-4", "11-100-13", "11-30-003-1", "11-30-003-4", "ВНС-47.91.001-1", "ВНС-96.61.001-2", "ВНС-96.61.001-3", "ВНС-96.61.001-4",
                                            "Rivet Bralo-264", "Rivet Bralo-265", "Rivet Bralo-266", "Rivet Bralo-267", "Rivet Bralo-268", "Rivet Bralo-270", "Rivet Bralo-271", "Rivet Bralo-272",
                                            "Rivet Bralo-273", "Rivet Bralo-274", "Rivet Bralo-275", "Rivet Bralo-276",
                                            "Rivet Bralo-240", "Rivet Bralo-241", "Rivet Bralo-242", "Rivet Bralo-243", "Rivet Bralo-244", "Rivet Bralo-245", "Rivet Bralo-246", "Rivet Bralo-247",
                                            "Rivet Bralo-248", "Rivet Bralo-249", "Rivet Bralo-250", "Rivet Bralo-251",};
                        foreach (var item in itemsToDelete)
                        {
                            DoWithSwDoc(_swApp, CompType.COMPONENT, item, Act.Delete);
                        }

                        #region To delete

                        //swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        //  swDoc.Extension.SelectByID2("11-30-100-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        //   swDoc.Extension.SelectByID2("11-30-100-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("11-100-13@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("11-30-003-1@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        // swDoc.Extension.SelectByID2("11-30-003-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        //    swDoc.Extension.SelectByID2("ВНС-47.91.001-1@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                        //   swDoc.Extension.SelectByID2("Rivet Bralo-267@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //   swDoc.Extension.SelectByID2("Rivet Bralo-268@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //    swDoc.Extension.SelectByID2("Rivet Bralo-270@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //    swDoc.Extension.SelectByID2("Rivet Bralo-271@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //    swDoc.Extension.SelectByID2("Rivet Bralo-272@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //    swDoc.Extension.SelectByID2("Rivet Bralo-273@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        //    swDoc.Extension.SelectByID2("Rivet Bralo-274@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //     swDoc.Extension.SelectByID2("Rivet Bralo-275@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //     swDoc.Extension.SelectByID2("Rivet Bralo-276@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        //    swDoc.Extension.SelectByID2("Rivet Bralo-264@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //     swDoc.Extension.SelectByID2("Rivet Bralo-265@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //     swDoc.Extension.SelectByID2("Rivet Bralo-266@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-243@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-244@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-245@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-246@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-247@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-248@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                        //swDoc.Extension.SelectByID2("Rivet Bralo-240@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-241@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-242@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-249@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-250@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        //swDoc.Extension.SelectByID2("Rivet Bralo-251@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();

                        #endregion

                        swDoc.Extension.SelectByID2("ЗеркальныйКомпонент1", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("DerivedCrvPattern3", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    }

                    if (isdouble)
                    {
                        lp = widthD / 2 - 59.5;
                        lp2 = lp - 11.6;
                        lProfName = Convert.ToString(Math.Truncate(Convert.ToDouble(Convert.ToDouble(Width)) / 2 - 9));
                        lProfNameLength = (widthD / 2 - 23) / 1000;
                    }

                    #endregion

                    #region Детали

                    // 11-30-001 
                    newName = "11-30-03-" + Width + modelType;
                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
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
                            $@"{destRootFolder}\{DestinationFolder}\{newName}",
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
                        ComponentToAdd(newPartPath);
                    }

                    // 11-30-002 
                    newName = "11-30-01-" + Height + modelType + (IsOutDoor ? "-O" : "");
                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-30-002-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-30-002.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-30-002",
                            $@"{destRootFolder}\{DestinationFolder}\{newName}",
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
                        ComponentToAdd(newPartPath);                        
                    }

                    // 11-30-004 
                    newName = "11-30-02-" + Height + modelType + (IsOutDoor ? "-O" : "");
                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-30-004-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-30-004.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-30-004",
                            $@"{destRootFolder}\{DestinationFolder}\{newName}",
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
                        ComponentToAdd(newPartPath);                        
                    }


                    // 11-30-003 
                    newName = "11-30-04-" + Math.Truncate(lp) + "-" + hC + modelType;
                    newPartPath = $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-30-003-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("11-30-003.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-30-003",
                            $@"{destRootFolder}\{DestinationFolder}\{newName}",
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
                        ComponentToAdd(newPartPath);                        
                    }

                    #endregion

                    #region Сборки

                    #region 11-100 Сборка лопасти

                    var newNameAsm = "11-2-" + lProfName;
                    string newPartPathAsm =
                        $@"{destRootFolder}{DestinationFolder}\{newNameAsm}.SLDASM";

                    if (isdouble)
                    {
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
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
                                $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                            if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
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
                                myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                                myDimension.SystemValue = lProfNameLength;
                                _swApp.ActivateDoc2("11-101", false, 0);
                                swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                                swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                                _swApp.CloseDoc(newName);
                                ComponentToAdd(newPartPath);                                
                            }

                            #endregion
                        }
                    }

                    if (!isdouble)
                    {
                        newNameAsm = "11-" + lProfName;
                        newPartPathAsm =
                            $@"{destRootFolder}{DestinationFolder}\{newNameAsm}.SLDASM";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
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
                                $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                            if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
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
                                myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                                myDimension.SystemValue = lProfNameLength;
                                _swApp.ActivateDoc2("11-101", false, 0);
                                swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                                swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                                _swApp.CloseDoc(newName);
                                ComponentToAdd(newPartPath);                                
                            }

                            #endregion
                        }
                    }

                    _swApp.ActivateDoc2("11-100", false, 0);
                    swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                    if (isdouble)
                    {
                        swDoc.Extension.SelectByID2("Совпадение76", "MATE", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                        swDoc.Extension.SelectByID2("Совпадение77", "MATE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                        swDoc.Extension.SelectByID2("ВНС-47.91.101-2@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    }

                    var docDrw100 = _swApp.OpenDoc6($@"{sourceRootFolder}{modelDamperPath}\{"11-100"}.SLDDRW", (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                    swDoc.ForceRebuild3(false);
                    swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    _swApp.CloseDoc(newNameAsm);
                    docDrw100.ForceRebuild3(false);
                    var drwNewName = $@"{destRootFolder}\{DestinationFolder}\{newNameAsm}.SLDDRW";
                    docDrw100.SaveAs2(drwNewName, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(drwNewName).FullName) + " - DRW1");

                    ComponentToAdd(new[] { newPartPathAsm, drwNewName });
                                        
                    #endregion

                    #region 11-30-100 Сборка Перемычки

                    newNameAsm = "11-30-100-" + Height + modelType;
                    newPartPathAsm = $@"{destRootFolder}{DestinationFolder}\{newNameAsm}.SLDASM";

                    if (isdouble)
                    {
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
                        {
                            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                            swDoc.Extension.SelectByID2("11-30-100-4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                            _swApp.CloseDoc("11-30-100.SLDASM");
                        }
                        else
                        {
                            #region  11-30-101  Профиль перемычки

                            newName = "11-30-101-" + Height + modelType;
                            newPartPath =
                                $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                            if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 0, VaultName))
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
                                    $@"{destRootFolder}\{DestinationFolder}\{newName}",
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
                                ComponentToAdd(new FileInfo($@"{destRootFolder}\{DestinationFolder}\{newNameAsm}").FullName);
                            }

                            #endregion

                            _swApp.ActivateDoc2("11-30-100", false, 0);
                            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                            swDoc.ForceRebuild3(true);

                            #region  11-30-102  Профиль перемычки

                            newName = "11-30-102-" + Height + modelType;
                            newPartPath =
                                $@"{destRootFolder}\{DestinationFolder}\{newName}.SLDPRT";
                            if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 0, VaultName))
                            {
                                _swApp.IActivateDoc2("10-30-100", false, 0);
                                swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                                swDoc.Extension.SelectByID2("11-30-10@11-30-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                                swAsm.ReplaceComponents(newPartPath, "", true, true);
                                _swApp.CloseDoc("11-30-100.SLDASM");
                            }
                            else
                            {
                                SwPartParamsChangeWithNewName("11-30-102",
                                    $@"{destRootFolder}\{DestinationFolder}\{newName}",
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
                                AddMaterial(material, newName);

                                _swApp.CloseDoc(newName);
                                ComponentToAdd(new FileInfo($@"{destRootFolder}\{DestinationFolder}\{newNameAsm}").FullName);                                
                            }

                            #endregion

                            _swApp.ActivateDoc2("11-30-100", false, 0);
                            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                            swDoc.ForceRebuild3(false); swDoc.ForceRebuild3(true);
                            swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            _swApp.CloseDoc(newNameAsm);                           
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
                var name = $@"{destRootFolder}\{DestinationFolder}\{ModelName}";
                var newAsm = name + ".SLDASM";
                var newDrw = name + ".SLDDRW";
                swDoc.SaveAs2(newAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(newAsm).FullName));
                swDocDrw.Extension.SelectByID2("DRW1", "SHEET", 0, 0, 0, false, 0, null, 0);
                var drw = (DrawingDoc)(_swApp.IActivateDoc3(drawing + ".SLDDRW", true, 0));
                drw.ActivateSheet("DRW1");

                //var m = 5;
                //if (Convert.ToInt32(Width) > 850 || Convert.ToInt32(Height) > 850) { m = 15; }
                //if (Convert.ToInt32(Width) > 1250 || Convert.ToInt32(Height) > 1250) { m = 20; }                
                //drw.SetupSheet5("DRW1", 12, 12, 1, m, true, Settings.Default.DestinationFolder + @"\\srvkb\SolidWorks Admin\Templates\Основные надписи\A2-A-1.slddrt", 0.42, 0.297, "По умолчанию", false);

                swDocDrw.SaveAs2(newDrw, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(ModelPath);
                _swApp.ExitApp();
                _swApp = null;

                ComponentToAdd(new[] { newDrw, newAsm });               

                List<VaultSystem.VentsCadFile> outList;
                VaultSystem.CheckInOutPdmNew(NewComponents, true, out outList);

                //if (unloadXml)
                //{
                //    foreach (var newComponent in NewComponents)
                //    {
                //        PartInfoToXml(newComponent.LocalPartFileInfo);
                //    }
                //}

                Place = GetPlace();

            }            
            
            internal override string DestinationFolder => @"\Проекты\Blauberg\11 - Регулятор расхода воздуха";

            internal override string TemplateFolder => @"\Библиотека проектирования\DriveWorks\11 - Damper";

            internal override string ModelName { get; set; }

            internal override string ModelPath { get; set; }

            internal string modelName;
            internal string modelType;
            internal string modelDamperPath;
            internal string nameAsm;
            internal string drawing;

            internal bool IsOutDoor;

            internal string[] material;

            internal string TypeOfFlange;
            internal string Width;
            internal string Height;
        }
    }
}
