using EdmLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;


namespace PdmAsmBomToXml
{
    public class AsmBomToXml
    {
        static IEnumerable<string> Configurations(string asmPath, out List<Exception> exceptions)
        {
            IEnumerable<string> configurations = null;
            exceptions = new List<Exception>();
            try
            {              
                var emdpService = new Epdm();
                configurations = emdpService.GetConfiguration(asmPath);  
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            return configurations;
        }

        static List<Epdm.BomCells> Bom(string asmPath, string config, out List<Exception> exceptions)
        {
            List<Epdm.BomCells> bom = null;            
            exceptions = new List<Exception>();
            try
            {
                if (string.IsNullOrEmpty(config))
                {
                    config = "00";
                }
                var emdpService = new Epdm();
               
                Exception exc;
                var bomClassGetLastVersion = new Epdm
                {
                    BomId = 8,
                    AssemblyPath = asmPath
                };
                bom = bomClassGetLastVersion.BomList(asmPath, config, false, out exc);
                if (exc != null)
                {
                    exceptions.Add(exc);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            return bom;           
        }

        public static void ВыгрузкаСборкиВXml(string ПутьКСборке, out List<Exception> exceptions) 
        {
            exceptions = new List<Exception>();
            if (ПутьКСборке == null)
            {
                exceptions.Add(new Exception("ПутьКСборке пуст"));
                return;
            }       

            var списокКонфигурацийСборки = Configurations(ПутьКСборке, out exceptions);
            var specification = Bom(ПутьКСборке, null, out exceptions);

            #region Выгрузка Главной Сборки

            try
            {
                List<Exception> exc2;    
                            
                ВыгрузитьСборкуПеречень(specification, списокКонфигурацийСборки, out exc2);

              //  return;
                           
                if (exc2 != null) { exceptions.AddRange(exc2); }
                List<Exception> exc3;
                ВыгрузитьСборку(specification, списокКонфигурацийСборки, out exc3);
                if (exc3 != null) { exceptions.AddRange(exc3); }
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }

            #endregion

            #region Выгрузка подсборок

            foreach (var путьКСборке in specification.Where(x =>
            {
                var extension = Path.GetExtension(x.FilePath + "\\" + x.FileName);
                return extension.ToLower() == ".sldasm";
            })
                .Where(x => x.Раздел == "" || x.Раздел == "Сборочные единицы")
                .Select(x => x.FilePath + "\\" + x.FileName)
                .Distinct())
            {
                списокКонфигурацийСборки = Configurations(путьКСборке, out exceptions);
                specification = Bom(путьКСборке, null, out exceptions);
                List<Exception> exc3;
                ВыгрузитьСборку(specification, списокКонфигурацийСборки, out exc3);
                if (exc3 != null) { exceptions.AddRange(exc3); }               
            }

            #endregion

        }

        const string xmlPath = @"\\pdmsrv\XML\";// @"\\srvkb\SolidWorks Admin\XML\"; //@"C:\Temp\";//

        static void ВыгрузитьСборку(List<Epdm.BomCells> спецификация, IEnumerable<string> списокКонфигурацийСборки, out List<Exception> exc)
        {
            exc = new List<Exception>();          

            try
            {                  
                var путьКСборке = спецификация[0].FilePath + "\\" + спецификация[0].FileName;
                var имяСборки = Path.GetFileNameWithoutExtension(путьКСборке);

                var lastVerOfAsm = спецификация[0].ПоследняяВерсия;
                bool exist = false;
                if (lastVerOfAsm != null) exist = ExistLastXml(путьКСборке, (int)lastVerOfAsm, false);
                if (exist) return;

                var myXml = new XmlTextWriter(xmlPath + имяСборки + ".xml", Encoding.UTF8);

                myXml.WriteStartDocument();
                myXml.Formatting = Formatting.Indented;
                myXml.Indentation = 2;

                // создаем элементы
                myXml.WriteStartElement("xml");
                myXml.WriteStartElement("transactions");
                myXml.WriteStartElement("transaction");

                myXml.WriteAttributeString("vaultName", "Vents-PDM");
                myXml.WriteAttributeString("type", "wf_export_document_attributes");
                myXml.WriteAttributeString("date", "1416475021");

                // document
                myXml.WriteStartElement("document");
                myXml.WriteAttributeString("pdmweid", "73592");
                myXml.WriteAttributeString("aliasset", "Export To ERP");

                if (списокКонфигурацийСборки != null)
                    foreach (var config in списокКонфигурацийСборки)
                    {
                        спецификация = Bom(путьКСборке, config, out exc);

                        foreach (var topAsm in спецификация.Where(x => x.Уровень == 0))
                        {
                            #region XML

                            // Конфигурация
                            myXml.WriteStartElement("configuration");
                            myXml.WriteAttributeString("name", topAsm.Конфигурация);

                            // Версия
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Версия");
                            myXml.WriteAttributeString("value", topAsm.ПоследняяВерсия.ToString());
                            myXml.WriteEndElement();

                            // Масса
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Масса");
                            myXml.WriteAttributeString("value", topAsm.Weight);
                            myXml.WriteEndElement();

                            // Наименование
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Наименование");
                            myXml.WriteAttributeString("value", topAsm.Наименование);
                            myXml.WriteEndElement();

                            // Обозначение
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Обозначение");
                            myXml.WriteAttributeString("value", topAsm.Раздел == "Материалы" ? "" : topAsm.Обозначение);
                            myXml.WriteEndElement();

                            // Раздел
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Раздел");
                            myXml.WriteAttributeString("value", topAsm.Раздел);
                            myXml.WriteEndElement();

                            // ERP code
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "ERP code");
                            myXml.WriteAttributeString("value", topAsm.ErpCode);
                            myXml.WriteEndElement();

                            // Код_Материала
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Код_Материала");
                            myXml.WriteAttributeString("value", topAsm.CodeMaterial);
                            myXml.WriteEndElement();

                            // Код документа
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Код документа");
                            myXml.WriteAttributeString("value", "");//topAsm..КодДокумента);
                            myXml.WriteEndElement();

                            // Кол. Материала
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Кол. Материала");
                            myXml.WriteAttributeString("value", topAsm.SummMaterial);
                            myXml.WriteEndElement();

                            // Состояние
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Состояние");
                            myXml.WriteAttributeString("value", topAsm.Состояние);
                            myXml.WriteEndElement();

                            // Подсчет ссылок
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Подсчет ссылок");
                            myXml.WriteAttributeString("value", topAsm.Количество.ToString());
                            myXml.WriteEndElement();

                            // Конфигурация
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Конфигурация");
                            myXml.WriteAttributeString("value", topAsm.Конфигурация);
                            myXml.WriteEndElement();

                            // Идентификатор
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Идентификатор");
                            myXml.WriteAttributeString("value", "");
                            myXml.WriteEndElement();

                            // references
                            myXml.WriteStartElement("references");

                            // document
                            myXml.WriteStartElement("document");
                            myXml.WriteAttributeString("pdmweid", "73592");
                            myXml.WriteAttributeString("aliasset", "Export To ERP");

                            foreach (var topLevel in спецификация.Where(x => x.Уровень == 1))
                            {
                                #region XML

                                // Конфигурация
                                myXml.WriteStartElement("configuration");
                                myXml.WriteAttributeString("name", topLevel.Конфигурация);

                                // Версия
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Версия");
                                myXml.WriteAttributeString("value", topLevel.ПоследняяВерсия.ToString());
                                myXml.WriteEndElement();

                                // Масса
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Масса");
                                myXml.WriteAttributeString("value", topLevel.Weight);
                                myXml.WriteEndElement();

                                // Наименование
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Наименование");
                                myXml.WriteAttributeString("value", topLevel.Наименование);
                                myXml.WriteEndElement();

                                // Обозначение
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Обозначение");
                                myXml.WriteAttributeString("value", topLevel.Раздел == "Материалы" ? "" : topLevel.Обозначение);
                                myXml.WriteEndElement();

                                // Раздел
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Раздел");
                                myXml.WriteAttributeString("value", topLevel.Раздел);
                                myXml.WriteEndElement();

                                // ERP code
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "ERP code");
                                myXml.WriteAttributeString("value", topLevel.ErpCode);
                                myXml.WriteEndElement();

                                // Код_Материала
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Код_Материала");
                                myXml.WriteAttributeString("value", topLevel.CodeMaterial);
                                myXml.WriteEndElement();

                                // Код документа
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Код документа");
                                myXml.WriteAttributeString("value", "");
                                myXml.WriteEndElement();

                                // Кол. Материала
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Кол. Материала");
                                myXml.WriteAttributeString("value", topLevel.SummMaterial);
                                myXml.WriteEndElement();

                                // Состояние
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Состояние");
                                myXml.WriteAttributeString("value", topLevel.Состояние);
                                myXml.WriteEndElement();

                                // Подсчет ссылок
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Подсчет ссылок");
                                myXml.WriteAttributeString("value", topLevel.Количество.ToString());
                                myXml.WriteEndElement();

                                // Конфигурация
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Конфигурация");
                                myXml.WriteAttributeString("value", topLevel.Конфигурация);
                                myXml.WriteEndElement();

                                // Идентификатор
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Идентификатор");
                                myXml.WriteAttributeString("value", "");
                                myXml.WriteEndElement();

                                myXml.WriteEndElement(); //configuration

                                #endregion
                            }

                            myXml.WriteEndElement(); // document
                            myXml.WriteEndElement(); // элемент references
                            myXml.WriteEndElement(); // configuration

                            #endregion
                        }
                    }

                myXml.WriteEndElement(); // ' элемент DOCUMENT
                myXml.WriteEndElement(); // ' элемент TRANSACTION
                myXml.WriteEndElement(); // ' элемент TRANSACTIONS
                myXml.WriteEndElement(); // ' элемент XML

                myXml.Flush();

                myXml.Close();
            }
            catch (Exception e)
            {
                exc.Add(e);
            }
        }

        static int? Version(string xmlPath)
        {
            if (!xmlPath.EndsWith("xml")) { return null; }

            int? version = null;

            try
            {
                var coordinates = XDocument.Load(xmlPath);

                var enumerable = coordinates.Descendants("attribute")
                    .Select(
                        element =>
                            new
                            {
                                Number = element.FirstAttribute.Value,
                                Values = element.Attribute("value")
                            });
                foreach (var obj in enumerable)
                {
                    if (obj.Number != "Версия") continue;

                    version = Convert.ToInt32(obj.Values.Value);

                    goto m1;
                }
            }
            catch (Exception)
            {
                return 0;
            }
            m1:
            return version;
        }

        static bool ExistLastXml(string asmPath, int currentVersion, bool partsXml)
        {
            try
            {
                var xmlPartPath =
                new FileInfo(xmlPath + Path.GetFileNameWithoutExtension(asmPath) + (partsXml ? " Parts" : null) + ".xml");

                if (!xmlPartPath.Exists) return false;

                var xmlPartVersion = Version(xmlPartPath.FullName);

             
                return Equals(xmlPartVersion, currentVersion);
            }
            catch (Exception)
            {
                return false;
            }
        }             

        static void ВыгрузитьСборкуПеречень(List<Epdm.BomCells> спецификация, IEnumerable<string> списокКонфигурацийСборки, out List<Exception> exc)
        {           

            exc = new List<Exception>();            

            var путь = спецификация[0].FilePath + "\\" + спецификация[0].FileName;

            try
            {

                if (списокКонфигурацийСборки != null)
                    foreach (var config in списокКонфигурацийСборки)
                    {
                        спецификация = Bom(путь, config, out exc);

                        //TODO Проверка на наличие XML 
                        //var exist = false;
                        //try
                        //{
                        //    int? lastVerOfAsm = спецификация[0].ПоследняяВерсия;
                        //    if (lastVerOfAsm != null) exist = ExistLastXml(путь, (int)lastVerOfAsm, true);
                        //}
                        //catch (Exception e)
                        //{
                        //    exc.Add(e);
                        //}
                        //if (exist) return;                               

                        var myXml = new XmlTextWriter(xmlPath + Path.GetFileNameWithoutExtension(путь) + "-" + config + " Parts.xml", Encoding.UTF8);

                        myXml.WriteStartDocument();
                        myXml.Formatting = Formatting.Indented;
                        myXml.Indentation = 2;

                        // создаем элементы
                        myXml.WriteStartElement("xml");
                        myXml.WriteStartElement("transactions");
                        myXml.WriteStartElement("transaction");

                        myXml.WriteAttributeString("vaultName", "Vents-PDM");
                        myXml.WriteAttributeString("type", "wf_export_document_attributes");
                        myXml.WriteAttributeString("date", "1416475021");

                        // document
                        myXml.WriteStartElement("document");
                        myXml.WriteAttributeString("pdmweid", "73592");
                        myXml.WriteAttributeString("aliasset", "Export To ERP");

                        foreach (var topAsm in спецификация.Where(x => x.Уровень == 0))
                        {
                            #region XML

                            // Конфигурация
                            myXml.WriteStartElement("configuration");
                            myXml.WriteAttributeString("name", topAsm.Конфигурация);

                            // Версия
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Версия");
                            myXml.WriteAttributeString("value", topAsm.ПоследняяВерсия.ToString());
                            myXml.WriteEndElement();

                            // Масса
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Масса");
                            myXml.WriteAttributeString("value", topAsm.Weight);
                            myXml.WriteEndElement();

                            // Наименование
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Наименование");
                            myXml.WriteAttributeString("value", topAsm.Наименование);
                            myXml.WriteEndElement();

                            // Обозначение
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Обозначение");
                            myXml.WriteAttributeString("value", topAsm.Раздел == "Материалы" ? "" : topAsm.Обозначение);
                            // 1C Для раздела "материалов" вставляем ПУСТО в обозначение из-за конфликта с 1С 
                            myXml.WriteEndElement();

                            // Раздел
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Раздел");
                            myXml.WriteAttributeString("value", topAsm.Раздел);
                            myXml.WriteEndElement();

                            // ERP code
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "ERP code");
                            myXml.WriteAttributeString("value", topAsm.ErpCode);
                            myXml.WriteEndElement();

                            // Код_Материала
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Код_Материала");
                            myXml.WriteAttributeString("value", topAsm.CodeMaterial);
                            myXml.WriteEndElement();

                            // Код документа
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Код документа");
                            myXml.WriteAttributeString("value", ""); //topAsm..КодДокумента);
                            myXml.WriteEndElement();

                            // Кол. Материала
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Кол. Материала");
                            myXml.WriteAttributeString("value", topAsm.SummMaterial);
                            myXml.WriteEndElement();

                            // Состояние
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Состояние");
                            myXml.WriteAttributeString("value", topAsm.Состояние);
                            myXml.WriteEndElement();

                            // Подсчет ссылок
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Подсчет ссылок");
                            myXml.WriteAttributeString("value", topAsm.Количество.ToString());
                            myXml.WriteEndElement();

                            // Конфигурация
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Конфигурация");
                            myXml.WriteAttributeString("value", topAsm.Конфигурация);
                            myXml.WriteEndElement();

                            // Идентификатор
                            myXml.WriteStartElement("attribute");
                            myXml.WriteAttributeString("name", "Идентификатор");
                            myXml.WriteAttributeString("value", "");
                            myXml.WriteEndElement();

                            // references
                            myXml.WriteStartElement("references");

                            // document
                            myXml.WriteStartElement("document");
                            myXml.WriteAttributeString("pdmweid", "73592");
                            myXml.WriteAttributeString("aliasset", "Export To ERP");

                            var allParts =
                                спецификация.Where(x => x.ТипФайла.ToLower() == "sldprt")
                                .Where(x => x.Раздел == "Детали" || x.Раздел == "").OrderBy(x => x.FileName).ToList();


                            foreach (var part in allParts)
                            {
                                #region XML

                                // Конфигурация
                                myXml.WriteStartElement("configuration");
                                myXml.WriteAttributeString("name", part.Конфигурация);

                                // Версия
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Версия");
                                myXml.WriteAttributeString("value", part.ПоследняяВерсия.ToString());
                                myXml.WriteEndElement();

                                // Масса
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Масса");
                                myXml.WriteAttributeString("value", part.Weight);
                                myXml.WriteEndElement();

                                // Наименование
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Наименование");
                                myXml.WriteAttributeString("value", part.Наименование);
                                myXml.WriteEndElement();

                                // Обозначение
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Обозначение");
                                myXml.WriteAttributeString("value", part.Раздел == "Материалы" ? "" : part.Обозначение);
                                myXml.WriteEndElement();

                                // Раздел
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Раздел");
                                myXml.WriteAttributeString("value", part.Раздел);
                                myXml.WriteEndElement();

                                // ERP code
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "ERP code");
                                myXml.WriteAttributeString("value", part.ErpCode);
                                myXml.WriteEndElement();

                                // Код_Материала
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Код_Материала");
                                myXml.WriteAttributeString("value", part.CodeMaterial);
                                myXml.WriteEndElement();

                                // Код документа
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Код документа");
                                myXml.WriteAttributeString("value", ""); // topLevel.КодДокумента);
                                myXml.WriteEndElement();

                                // Кол. Материала
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Кол. Материала");
                                myXml.WriteAttributeString("value", part.SummMaterial);
                                myXml.WriteEndElement();

                                // Состояние
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Состояние");
                                myXml.WriteAttributeString("value", part.Состояние);
                                myXml.WriteEndElement();

                                // Подсчет ссылок
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Подсчет ссылок");
                                myXml.WriteAttributeString("value", part.Количество.ToString());
                                myXml.WriteEndElement();

                                // Конфигурация
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Конфигурация");
                                myXml.WriteAttributeString("value", part.Конфигурация);
                                myXml.WriteEndElement();

                                // Идентификатор
                                myXml.WriteStartElement("attribute");
                                myXml.WriteAttributeString("name", "Идентификатор");
                                myXml.WriteAttributeString("value", "");
                                myXml.WriteEndElement();

                                myXml.WriteEndElement(); //configuration

                                #endregion
                            }

                            myXml.WriteEndElement(); // document
                            myXml.WriteEndElement(); // элемент references
                            myXml.WriteEndElement(); // configuration

                            #endregion
                        }



                        myXml.WriteEndElement(); // ' элемент DOCUMENT
                        myXml.WriteEndElement(); // ' элемент TRANSACTION
                        myXml.WriteEndElement(); // ' элемент TRANSACTIONS
                        myXml.WriteEndElement(); // ' элемент XML
                                                 // заносим данные в myMemoryStream
                        myXml.Flush();

                        myXml.Close();
                    }
            }
            catch (Exception e)
            {
                exc.Add(e);
            }
        }

        public class Epdm
        {
            public IEdmFile7 EdmFile7;

            #region Поля 

            public int BomId { get; set; }

            public string AssemblyInfoLabel;

            public string AssemblyPath { get; set; }

            public string AsmConfiguration { get; set; }

            public class BomCells
            {
                public decimal? Количество { get; set; }
                public string ТипФайла { get; set; }
                public string Конфигурация { get; set; }
                public int? ПоследняяВерсия { get; set; }
                public int? Уровень { get; set; }
                public string Состояние { get; set; }
                public string Раздел { get; set; }
                public string Обозначение { get; set; }
                public string Наименование { get; set; }
                public string Материал { get; set; }
                public string МатериалЦми { get; set; }
                public string ТолщинаЛиста { get; set; }
                public int? IdPdm { get; set; }
                public string FileName { get; set; }
                public string FilePath { get; set; }
                public string ErpCode { get; set; }
                public string SummMaterial { get; set; }
                public string Weight { get; set; }
                public string CodeMaterial { get; set; }
                public string Format { get; set; }
                public string Note { get; set; }
                public int? Position { get; set; }
                public List<decimal> КолПоКонфигурациям { get; set; }
                public string КонфГлавнойСборки { get; set; }
                public string ТипОбъекта { get; set; }
                public string GetPathName { get; set; }

            }

            public List<BomCells> BomList(string filePath, string config, bool asBuild, out Exception exception)
            {
                var bomFlag = asBuild ? EdmBomFlag.EdmBf_AsBuilt : EdmBomFlag.EdmBf_ShowSelected;
                Getbom(BomId, filePath, config, EdmBomFlag.EdmBf_ShowSelected, out exception);
                return _bomList;
            }

            #endregion

            public IEnumerable<string> GetConfiguration(string filePath)
            {
                IEdmFolder5 oFolder;
                CheckPdmVault();
                var edmFile5 = _edmVault5.GetFileFromPath(filePath, out oFolder);
                var configs = edmFile5.GetConfigurations("");

                var headPosition = configs.GetHeadPosition();

                var configsList = new List<string>();

                while (!headPosition.IsNull)
                {
                    var configName = configs.GetNext(headPosition);
                    if (configName != "@")
                    {
                        configsList.Add(configName);
                    }
                }
                return configsList;

            }

            #region Методы получения листа
            public class SearchColumnName
            {
                public string FileName { get; set; }

                public int FileId { get; set; }

                public int FolderId { get; set; }

                public string PartNumber { get; set; }

                public string FilePath { get; set; }
            }

            private EdmVault5 _edmVault5;
            private IEdmVault7 _edmVault7;

            void CheckPdmVault()
            {
                try
                {
                    if (_edmVault5 == null)
                    {
                        _edmVault5 = new EdmVault5();
                    }

                    _edmVault7 = _edmVault5;

                    var ok = _edmVault5.IsLoggedIn;

                    if (!ok)
                    {
                        _edmVault5.LoginAuto("Vents-PDM", 0);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            public List<SearchColumnName> SearchDoc(string name)
            {
                var namedoc = new List<SearchColumnName>();

                CheckPdmVault();

                var search = (IEdmSearch5)_edmVault7.CreateUtility(EdmUtility.EdmUtil_Search);

                search.FindFiles = true;
                search.FindFolders = false;

                search.FileName = "%" + name + "%";

                var result = search.GetFirstResult();

                while ((result != null))
                {
                    var columnClass = new SearchColumnName()
                    {
                        FileName = result.Name,
                        FileId = result.ID,
                        FolderId = result.ParentFolderID,
                        FilePath = result.Path
                    };

                    namedoc.Add(columnClass);

                    result = search.GetNextResult();
                }

                return namedoc;
            }

            void Getbom(int bomId, string filePath, string bomConfiguration, EdmBomFlag bomFlag, out Exception exception)
            {
                exception = null;

                try
                {
                    IEdmFolder5 oFolder;
                    CheckPdmVault();
                    EdmFile7 = (IEdmFile7)_edmVault5.GetFileFromPath(filePath, out oFolder);

                    var bomView = EdmFile7.GetComputedBOM(Convert.ToInt32(bomId), Convert.ToInt32(-1), bomConfiguration, (int)bomFlag);

                    if (bomView == null) return;
                    Array bomRows;
                    Array bomColumns;
                    bomView.GetRows(out bomRows);
                    bomView.GetColumns(out bomColumns);

                    var bomTable = new DataTable();

                    foreach (EdmBomColumn bomColumn in bomColumns)
                    {
                        bomTable.Columns.Add(new DataColumn { ColumnName = bomColumn.mbsCaption });
                    }

                    //bomTable.Columns.Add(new DataColumn { ColumnName = "Путь" });
                    bomTable.Columns.Add(new DataColumn { ColumnName = "Уровень" });
                    bomTable.Columns.Add(new DataColumn { ColumnName = "КонфГлавнойСборки" });
                    bomTable.Columns.Add(new DataColumn { ColumnName = "ТипОбъекта" });
                    bomTable.Columns.Add(new DataColumn { ColumnName = "GetPathName" });

                    for (var i = 0; i < bomRows.Length; i++)
                    {
                        var cell = (IEdmBomCell)bomRows.GetValue(i);

                        bomTable.Rows.Add();

                        for (var j = 0; j < bomColumns.Length; j++)
                        {
                            var column = (EdmBomColumn)bomColumns.GetValue(j);

                            object value;
                            object computedValue;
                            string config;
                            bool readOnly;

                            cell.GetVar(column.mlVariableID, column.meType, out value, out computedValue, out config, out readOnly);

                            if (value != null)
                            {
                                bomTable.Rows[i][j] = value;
                            }
                            else
                            {
                                bomTable.Rows[i][j] = null;
                            }
                            bomTable.Rows[i][j + 1] = cell.GetTreeLevel();

                            bomTable.Rows[i][j + 2] = bomConfiguration;
                            bomTable.Rows[i][j + 3] = config;
                            bomTable.Rows[i][j + 4] = cell.GetPathName();
                        }
                    }

                    _bomList = BomTableToBomList(bomTable);
                    exception = null;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            #endregion

            #region ДанныеДляВыгрузки - Поля с х-ками деталей

            List<BomCells> _bomList = new List<BomCells>();

            private static List<BomCells> BomTableToBomList(DataTable table)
            {

                var bomList = new List<BomCells>(table.Rows.Count);

                bomList.AddRange(from DataRow row in table.Rows
                                 select row.ItemArray into values
                                 select new BomCells
                                 {
                                     Раздел = values[0].ToString(),
                                     Обозначение = values[1].ToString(),
                                     Наименование = values[2].ToString(),
                                     Материал = values[3].ToString(),
                                     МатериалЦми = values[4].ToString(),
                                     ТолщинаЛиста = values[5].ToString(),
                                     Количество = Convert.ToDecimal(values[6]),
                                     ТипФайла = values[7].ToString(),
                                     Конфигурация = values[8].ToString(),
                                     ПоследняяВерсия = Convert.ToInt32(values[9]),
                                     IdPdm = Convert.ToInt32(values[10]),
                                     FileName = values[11].ToString(),
                                     FilePath = values[12].ToString(),
                                     ErpCode = values[13].ToString(),
                                     SummMaterial = values[14].ToString(),
                                     Weight = values[15].ToString(),
                                     CodeMaterial = values[16].ToString(),
                                     Format = values[17].ToString(),
                                     Note = values[18].ToString(),
                                     Уровень = Convert.ToInt32(values[19]),
                                     КонфГлавнойСборки = values[20].ToString(),
                                     ТипОбъекта = values[21].ToString(),
                                     GetPathName = values[22].ToString()
                                 });

                //LoggerInfo("Список из полученой таблицы успешно заполнен элементами в количестве" + bomList.Count);
                return bomList;
            }
            #endregion

           
        }

    }
}
