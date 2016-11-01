using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml.Linq;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace AirVentsCadWpf.DataControls.Specification
{
    class BomFormat
    {
        public List<BomData> ListXmlBomData(string xmlPath)
        {
            return GetDataFromXml(xmlPath) ? BomDataListFormatted(BomDataList) : null;
        }
        public List<List<BomData>> BomByPage()
        {
            return BomDataListsDivided;
        }
        
        internal List<BomData> BomDataList;
        internal static List<BomData> BomListFormatted;
        internal static List<List<BomData>> BomDataListsDivided;

        bool GetDataFromXml(string xmlPath)
            {
                if (!xmlPath.EndsWith("xml"))
                {
                    return false;
                }

                var coordinates = XDocument.Load(xmlPath);//MessageBox.Show(coordinates.ToString());

                BomDataList = new List<BomData>();

                try
                {
                    foreach (var coordinate in coordinates.Descendants("part"))
                    {
                        
                        var config = coordinate.Element("config");
                        var pathNameComponent = coordinate.Element("PathNameComponent");
                        var row = coordinate.Element("Row");
                        var itemNum = coordinate.Element("ItemNum");
                        var раздел = coordinate.Element("Раздел");
                        var обозначение = coordinate.Element("Обозначение");
                        var наименование = coordinate.Element("Наименование");
                        var формат = coordinate.Element("Формат");
                        var erpCode = coordinate.Element("ErpCode");
                        var кодМатериала = coordinate.Element("КодМатериала");
                        var примечание = coordinate.Element("Примечание");
                        var количество = coordinate.Element("Количество");
                        

                        BomDataList.Add(new BomData
                        {
                            Config = config?.Value ?? "",
                            PathNameComponent = pathNameComponent?.Value ?? "",
                            Row = row?.Value ?? "",
                            ItemNum = itemNum?.Value ?? "",
                            Раздел = раздел?.Value ?? "part",
                            Обозначение = обозначение?.Value ?? "",
                            Наименование = наименование?.Value ?? "",
                            Формат = формат?.Value ?? "",
                            ErpCode = erpCode?.Value ?? "",
                            КодМатериала = кодМатериала?.Value ?? "",
                            Примечание = примечание?.Value ?? "SK",
                            Количество = количество?.Value ?? ""
                        });
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }

                foreach (var coordinate in coordinates.Descendants("Item"))
                {

                    var config = coordinate.Element("config");
                    var pathNameComponent = coordinate.Element("PathNameComponent");
                    var row = coordinate.Element("Row");
                    var itemNum = coordinate.Element("ItemNum");
                    var раздел = coordinate.Element("Раздел");
                    var обозначение = coordinate.Element("Обозначение");
                    var наименование = coordinate.Element("Наименование");
                    var формат = coordinate.Element("Формат");
                    var erpCode = coordinate.Element("ErpCode");
                    var кодМатериала = coordinate.Element("КодМатериала");
                    var примечание = coordinate.Element("Примечание");
                    var количество = coordinate.Element("Количество");

                    BomDataList.Add(new BomData
                    {
                        Config = config?.Value ?? "",
                        PathNameComponent = pathNameComponent?.Value ?? "",
                        Row = row?.Value ?? "",
                        ItemNum = itemNum?.Value ?? "",
                        Раздел = раздел?.Value ?? "",
                        Обозначение = обозначение?.Value ?? "",
                        Наименование = наименование?.Value ?? "",
                        Формат = формат?.Value ?? "",
                        ErpCode = erpCode?.Value ?? "",
                        КодМатериала = кодМатериала?.Value ?? "",
                        Примечание = примечание?.Value ?? "",
                        Количество = количество?.Value ?? ""
                    });
                }

                return true;
            }
        
        public void InsertBomTable()
        {
            var swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            if (swApp?.IActiveDoc != null) AddSpecification1((IModelDoc2)swApp.IActiveDoc, BomListFormatted);
        }

        public static class BomSettings
         {
            public static int КоличествоСтрокНаПервомЛистеА4 { get; set; } = 26;
            public static int КоличествоСтрокНаВторомЛистеА4 { get; set; } = 30;

            public static string Sp1 { get; set; } = "SP-1.slddrt";
            public static string Sp2 { get; set; } = "SP-2.slddrt";
         }

        public static List<List<BomData>> BomDataLists(IList<BomData> bomDataList)
        {
            var toTake = BomSettings.КоличествоСтрокНаПервомЛистеА4 -
                bomDataList.Take(BomSettings.КоличествоСтрокНаПервомЛистеА4).Count(x => x.Наименование != null && x.Наименование.Count() > 34);

            var list = new List<List<BomData>> { bomDataList.Take(toTake).ToList() };

            m1:

            bomDataList = bomDataList.Skip(toTake).ToList();

            toTake = BomSettings.КоличествоСтрокНаВторомЛистеА4 -
                bomDataList.Take(BomSettings.КоличествоСтрокНаВторомЛистеА4).Count(x => x.Наименование != null && x.Наименование.Count() > 34);

            if (bomDataList.Count > 0)
            {
                list.Add(bomDataList.Take(toTake).ToList());
                goto m1;
            }
            BomDataListsDivided = list;
            return list;
        }

        public static List<BomData> BomDataListFormatted(IList<BomData> bomDataList)
        {
            var bomDataListFormatted = new List<BomData>
            {
                new BomData(),
                new BomData {Заголовок = true, Наименование = "Документація"},
                new BomData(),
                new BomData {Обозначение = bomDataList.First(x => x.Раздел == "").Обозначение, Наименование = "Складальне креслення"}
            };

            var sectionName1 = new[] { "Складальні одиниці", "Сборочные единицы" };
            var sectionList1 = bomDataList.Where(x => x.Раздел == sectionName1[1]).OrderBy(x => x.Обозначение).ToList();
            if (sectionList1.Count > 0)
            {
                bomDataListFormatted.AddRange(new List<BomData>
                {
                    new BomData(),
                    new BomData{Заголовок = true, Наименование = sectionName1[0]},
                    new BomData()
                });
                bomDataListFormatted.AddRange(sectionList1);
            }

            var sectionName2 = new[] { "Деталі", "Детали" };
            var sectionList2 = bomDataList.Where(x => x.Раздел == sectionName2[1]).OrderBy(x => x.Обозначение).ToList();
            if (sectionList2.Count > 0)
            {
                bomDataListFormatted.AddRange(new List<BomData>
                {
                    new BomData(),
                    new BomData{Заголовок = true, Наименование = "Деталі"},
                    new BomData()
                });
                bomDataListFormatted.AddRange(sectionList2);
            }

            var sectionName3 = new[] { "Стандартні вироби", "Стандартные изделия" };
            var sectionList3 = bomDataList.Where(x => x.Раздел == sectionName3[1]).OrderBy(x => x.Обозначение).ToList();
            if (sectionList3.Count > 0)
            {
                bomDataListFormatted.AddRange(new List<BomData>
                {
                    new BomData(),
                    new BomData{Заголовок = true, Наименование = "Стандартні вироби"},
                    new BomData()
                });
                bomDataListFormatted.AddRange(sectionList3);
            }

            var sectionName4 = new[] { "Інші вироби", "Причие изделия" };
            var sectionList4 = bomDataList.Where(x => x.Раздел == sectionName4[1]).OrderBy(x => x.Обозначение).ToList();
            if (sectionList4.Count > 0)
            {
                bomDataListFormatted.AddRange(new List<BomData>
                {
                    new BomData(),
                    new BomData{Заголовок = true, Наименование = "Інші вироби"},
                    new BomData()
                });
                bomDataListFormatted.AddRange(sectionList4);
            }

            var sectionName5 = new[] { "Матеріали", "Причие изделия" };
            var sectionList5 = bomDataList.Where(x => x.Раздел == sectionName5[1]).OrderBy(x => x.Обозначение).ToList();
            if (sectionList5.Count > 0)
            {
                bomDataListFormatted.AddRange(new List<BomData>
                {
                    new BomData(),
                    new BomData{Заголовок = true, Наименование = "Матеріали"},
                    new BomData()
                });
                bomDataListFormatted.AddRange(sectionList5);
            }

            BomListFormatted = bomDataListFormatted;
            return bomDataListFormatted;
        }
        
        private static
           void AddSpecification1(IModelDoc2 swModel, IList<BomData> bomDataList)
        {
            var swDrawing = ((DrawingDoc)(swModel));
            if (swDrawing == null) return;

            string[] spSheetNames = { "SP1", "SP2", "SP3", "SP4", "SP5", "SP6", "SP7", "SP8", "SP9", "SP10", "GSP1", "GSP2" };

            foreach (var t in spSheetNames.Where(swDrawing.ActivateSheet))
            {
                swModel.Extension.SelectByID2(t, "SHEET", 0, 0, 0, false, 0, null, 0);
                swModel.Extension.DeleteSelection2(1);
            }

            BomDataLists(bomDataList);

            InsertBomTable(BomDataListsDivided[0], swDrawing, spSheetNames[0], BomSettings.Sp1);//"GSP-1.slddrt"

            for (var i = 1; i < BomDataListsDivided.Count ; i++)
            {
                InsertBomTable(BomDataListsDivided[i], swDrawing, spSheetNames[1], BomSettings.Sp2);        
            }
        }

        private static void InsertBomTable(IList<BomData> bomDataList, IDrawingDoc swDrawing, string sheetName, string templateSheet)
        {
            var existSheet = swDrawing.NewSheet3(sheetName, 9, 12, 1, 10, true, templateSheet, 0.21, 0.297, "По умолчанию");
            var nawSheetName = sheetName;
            if (!existSheet)
            {
            m1:
                var newNumber = Convert.ToInt32(nawSheetName.Replace(nawSheetName.Remove(2), "")) + 1;
                nawSheetName = sheetName.Remove(2) + newNumber;
                var exist = swDrawing.NewSheet3(nawSheetName, 9, 12, 1, 10, true, templateSheet, 0.21, 0.297, "По умолчанию");
                if (!exist)
                {
                    goto m1;
                }
            }

            swDrawing.ActivateSheet(nawSheetName);

            var myTable = swDrawing.InsertTableAnnotation(0.02, 0.292, 1, bomDataList.Count+1, 7);
            if ((myTable == null)) return;
            for (var i = 0; i < BomSettings.КоличествоСтрокНаПервомЛистеА4; i++)
            {
                myTable.SetRowHeight(i, 0.008, 0);
            }
            myTable.Title = "Таблица видов";
            myTable.BorderLineWeight = 1;
            myTable.GridLineWeight = 1;

            #region Ширина колонок

            myTable.SetRowHeight(0, 0.015, 0);
            myTable.SetColumnWidth(0, 0.006, 0);
            myTable.SetColumnWidth(1, 0.006, 0);
            myTable.SetColumnWidth(2, 0.008, 0);
            myTable.SetColumnWidth(3, 0.07, 0);
            myTable.SetColumnWidth(4, 0.063, 0);
            myTable.SetColumnWidth(5, 0.010, 0);
            myTable.SetColumnWidth(6, 0.022, 0);

            #endregion

            const int position = 1;
           
            for (var i = 0; i < bomDataList.Count(); i++)
            {
                myTable.Text[position + i, 0] = bomDataList[i].Формат;
                myTable.Text[position + i, 2] = bomDataList[i].Row;
                myTable.Text[position + i, 3] = bomDataList[i].Обозначение;
                myTable.Text[position + i, 4] = bomDataList[i].Наименование;
                myTable.Text[position + i, 5] = bomDataList[i].Количество;
                myTable.Text[position + i, 6] = bomDataList[i].Примечание;
                var наименование = bomDataList[i].Наименование;
                if (наименование == null)
                {
                    myTable.SetRowHeight(position + i, 0.008, 0);
                }
                else
                {
                    myTable.SetRowHeight(position + i, наименование.Count() < 35 ? 0.008 : 0.016, 0);
                }

                if (i <= position) continue;
                myTable.set_CellTextHorizontalJustification(position + i, 0,
                    (int) swTextJustification_e.swTextJustificationRight);
            }
        }

        public class BomData
        {
            public string Config { get; set; }
            public string PathNameComponent { get; set; }
            public string Row { get; set; }
            public string ItemNum { get; set; }
            public string Раздел { get; set; }
            public string Обозначение { get; set; }
            public string Наименование { get; set; }
            public string Формат { get; set; }
            public string ErpCode { get; set; }
            public string КодМатериала { get; set; }
            public string Примечание { get; set; }
            public string Количество { get; set; }
            public bool Заголовок { get; set; }
            public bool ЗанимаетСтрок { get; set; }
        }
    }
}
